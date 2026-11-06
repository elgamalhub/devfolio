using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;

namespace Portfolio.Infrastructure.Data
{
    /// <summary>
    /// Seeds the portfolio with professional, CV-backed starter content.
    /// Every step is idempotent and never overwrites content that an administrator
    /// has already customized, apart from obvious legacy placeholders.
    /// </summary>
    public static class ContentSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await SeedSettingsAsync(context);
            await SeedAboutAsync(context);
            await SeedExperiencesAsync(context);
            await SeedProjectsAsync(context);
            await SeedCertificatesAsync(context);
            await SeedSectionsAsync(context);
            await SeedBlogAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedSettingsAsync(ApplicationDbContext context)
        {
            var settings = await context.SiteSettings.ToDictionaryAsync(s => s.Key, StringComparer.OrdinalIgnoreCase);

            void Set(
                string key,
                string valueEn,
                string valueAr,
                string description,
                string dataType,
                params string[] legacyEnglishValues)
            {
                if (!settings.TryGetValue(key, out var setting))
                {
                    setting = new SiteSettings
                    {
                        Key = key,
                        ValueEn = valueEn,
                        ValueAr = valueAr,
                        Description = description,
                        DataType = dataType
                    };
                    context.SiteSettings.Add(setting);
                    settings[key] = setting;
                    return;
                }

                // Upgrade only known legacy placeholders; keep administrator edits intact.
                if (legacyEnglishValues.Length > 0 &&
                    legacyEnglishValues.Any(value => string.Equals(value, setting.ValueEn, StringComparison.Ordinal)))
                {
                    setting.ValueEn = valueEn;
                    setting.ValueAr = valueAr;
                    setting.Description = description;
                    setting.DataType = dataType;
                }
            }

            Set("site_title_en", "Ahmed Elgamal", "Ahmed Elgamal",
                "Site title in English", "text", "My Portfolio");
            Set("site_title_ar", "أحمد الجمال", "أحمد الجمال",
                "Site title in Arabic", "text", "My Portfolio", "محفظتي");

            Set("meta_description_en",
                "Full Stack .NET developer in Cairo building secure, scalable web applications with ASP.NET Core, SQL Server, and Angular.",
                "Full Stack .NET developer in Cairo building secure, scalable web applications with ASP.NET Core, SQL Server, and Angular.",
                "SEO meta description (English)", "textarea", "Professional Portfolio Website");
            Set("meta_description_ar",
                "مطور .NET متكامل في القاهرة يبني تطبيقات ويب آمنة وقابلة للتوسع باستخدام ASP.NET Core وSQL Server وAngular.",
                "مطور .NET متكامل في القاهرة يبني تطبيقات ويب آمنة وقابلة للتوسع باستخدام ASP.NET Core وSQL Server وAngular.",
                "SEO meta description (Arabic)", "textarea", "موقع محفظة احترافي");

            Set("enable_preloader", "false", "false",
                "Enable the page preloader animation", "boolean", "true");
            Set("preloader_style", "spinner", "spinner",
                "Preloader animation style (spinner, dots, pulse, bars, circle)", "select");
            Set("preloader_color", "#667eea", "#667eea",
                "Preloader accent color", "color");
            Set("preloader_duration", "600", "600",
                "Preloader display duration in milliseconds", "number", "1000");
            Set("preloader_logo", "", "",
                "Custom logo for the preloader (optional)", "file");
            Set("preloader_background", "gradient", "gradient",
                "Preloader background style (gradient, solid, image)", "select");
            Set("preloader_background_image", "", "",
                "Custom background image for the preloader", "file");
            Set("og_image", "", "",
                "Default social sharing image (absolute URL or site path)", "file");
            Set("show_back_to_top", "true", "true",
                "Show the back-to-top button", "boolean");
        }

        private static async Task SeedAboutAsync(ApplicationDbContext context)
        {
            var about = await context.Abouts.FirstOrDefaultAsync();
            if (about == null)
            {
                return;
            }

            if (string.Equals(about.NameEn, "Ahmed Elgamal", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(about.NameEn))
            {
                about.NameEn = "Ahmed Abdelrahman Elgamal";
            }

            if (string.Equals(about.NameAr, "أحمد الجمال", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(about.NameAr))
            {
                about.NameAr = "أحمد عبد الرحمن الجمال";
            }

            var legacyDescription = about.DescriptionEn?.Contains(
                "Passionate .NET Developer with comprehensive hands-on experience",
                StringComparison.Ordinal) == true;

            if (legacyDescription || string.IsNullOrWhiteSpace(about.DescriptionEn))
            {
                about.DescriptionEn =
                    ".NET Developer with hands-on experience in C#, ASP.NET Core (MVC and API), and Entity Framework Core. " +
                    "I build structured, scalable applications backed by SQL Server and modern Angular front ends, with a focus on clean architecture, maintainability, and measurable performance. " +
                    "I completed intensive training through ITI's ITP program, the Digital Egypt Pioneers Initiative (DEPI), FWD, and ITI's Summer Code Camp. " +
                    "My work spans streaming platforms, job portals, school attendance systems, and e-commerce applications — from database design to deployment.";
            }

            if (legacyDescription || string.IsNullOrWhiteSpace(about.DescriptionAr))
            {
                about.DescriptionAr =
                    "مطور .NET لديه خبرة عملية في C# وASP.NET Core (MVC وAPI) وEntity Framework Core. " +
                    "أبني تطبيقات منظمة وقابلة للتوسع باستخدام SQL Server وواجهات Angular حديثة، مع التركيز على الهندسة النظيفة وقابلية الصيانة والأداء. " +
                    "أكملت برامج تدريبية مكثفة من خلال برنامج ITP في معهد تكنولوجيا المعلومات، ومبادرة رواد مصر الرقمية (DEPI)، وبرنامج FWD، والمعسكر الصيفي لـ ITI. " +
                    "تشمل أعمالي منصات بث، وبوابات وظائف، وأنظمة حضور مدرسي، وتطبيقات تجارة إلكترونية — من تصميم قاعدة البيانات حتى النشر.";
            }

            about.Email = string.IsNullOrWhiteSpace(about.Email)
                ? "ahmed.elgamal.dev22@gmail.com"
                : about.Email;
            about.Location = string.IsNullOrWhiteSpace(about.Location) ? "Cairo, Egypt" : about.Location;
            about.GitHubUrl = string.IsNullOrWhiteSpace(about.GitHubUrl)
                ? "https://github.com/elgamalhub"
                : about.GitHubUrl;
            about.LinkedInUrl = string.IsNullOrWhiteSpace(about.LinkedInUrl)
                ? "https://www.linkedin.com/in/ahmed-elgamal1"
                : about.LinkedInUrl;
            about.UpdatedAt = DateTime.UtcNow;
        }

        private static async Task SeedExperiencesAsync(ApplicationDbContext context)
        {
            var existing = await context.Experiences
                .Select(e => new { e.TitleEn, e.CompanyEn })
                .ToListAsync();

            var seeds = new[]
            {
                new Experience
                {
                    TitleEn = "Full Stack Developer (.NET Track)",
                    TitleAr = "مطور متكامل (مسار .NET)",
                    CompanyEn = "Information Technology Institute (ITI) — ITP",
                    CompanyAr = "معهد تكنولوجيا المعلومات (ITI) — برنامج ITP",
                    DescriptionEn = "Intensive five-month training program covering ASP.NET Core, SQL Server, Angular, clean architecture, and team-based delivery of a full-stack graduation project.",
                    DescriptionAr = "برنامج تدريبي مكثف لمدة خمسة أشهر يغطي ASP.NET Core وSQL Server وAngular والهندسة النظيفة، مع تنفيذ مشروع تخرج متكامل ضمن فريق.",
                    StartDate = new DateTime(2025, 7, 1),
                    EndDate = new DateTime(2025, 11, 30),
                    IsCurrent = false,
                    Order = 1,
                    IsActive = true
                },
                new Experience
                {
                    TitleEn = "Full Stack .NET Developer Track",
                    TitleAr = "مسار مطور .NET متكامل",
                    CompanyEn = "Digital Egypt Pioneers Initiative (DEPI)",
                    CompanyAr = "مبادرة رواد مصر الرقمية (DEPI)",
                    DescriptionEn = "Eight-month scholarship track focused on full-stack .NET development, databases, and practical project work at ITC College, Tanta.",
                    DescriptionAr = "مسار تدريبي لمدة ثمانية أشهر يركز على تطوير .NET المتكامل وقواعد البيانات والعمل على مشاريع عملية في كلية ITC بطنطا.",
                    StartDate = new DateTime(2024, 10, 1),
                    EndDate = new DateTime(2025, 5, 31),
                    IsCurrent = false,
                    Order = 2,
                    IsActive = true
                },
                new Experience
                {
                    TitleEn = "Front End Web Developer",
                    TitleAr = "مطور واجهات أمامية",
                    CompanyEn = "Ministry of Communications and Information Technology (MCIT) — FWD",
                    CompanyAr = "وزارة الاتصالات وتكنولوجيا المعلومات — برنامج FWD",
                    DescriptionEn = "Remote front-end training program covering HTML, CSS, JavaScript, responsive design, and modern web development practices.",
                    DescriptionAr = "برنامج تدريبي عن بُعد في تطوير الواجهات الأمامية يغطي HTML وCSS وJavaScript والتصميم المتجاوب وممارسات الويب الحديثة.",
                    StartDate = new DateTime(2022, 8, 1),
                    EndDate = new DateTime(2022, 11, 30),
                    IsCurrent = false,
                    Order = 3,
                    IsActive = true
                }
            };

            foreach (var seed in seeds)
            {
                var alreadyExists = existing.Any(e =>
                    string.Equals(e.CompanyEn, seed.CompanyEn, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.TitleEn, seed.TitleEn, StringComparison.OrdinalIgnoreCase));

                if (!alreadyExists)
                {
                    context.Experiences.Add(seed);
                }
            }
        }

        private static async Task SeedProjectsAsync(ApplicationDbContext context)
        {
            var projects = await context.Projects
                .Include(p => p.Technologies)
                .ToListAsync();

            void EnsureTechnology(Project project, string technologyName)
            {
                if (!project.Technologies.Any(t => string.Equals(t.TechnologyName, technologyName, StringComparison.OrdinalIgnoreCase)))
                {
                    project.Technologies.Add(new ProjectTechnology
                    {
                        ProjectId = project.Id,
                        TechnologyName = technologyName
                    });
                }
            }

            var ecommerce = projects.FirstOrDefault(p =>
                p.TitleEn.Contains("E-Commerce", StringComparison.OrdinalIgnoreCase));

            if (ecommerce != null)
            {
                if (string.IsNullOrWhiteSpace(ecommerce.ShortDescriptionEn) ||
                    ecommerce.ShortDescriptionEn.Length < 20)
                {
                    ecommerce.ShortDescriptionEn =
                        "A responsive storefront for mobile phone sales with product browsing, cart, checkout, and admin and seller dashboards.";
                }

                if (string.IsNullOrWhiteSpace(ecommerce.ShortDescriptionAr) ||
                    ecommerce.ShortDescriptionAr.Length < 20)
                {
                    ecommerce.ShortDescriptionAr =
                        "متجر إلكتروني متجاوب لبيع الهواتف المحمولة يتضمن تصفح المنتجات والسلة وإتمام الشراء ولوحات تحكم للمدير والبائع.";
                }

                if (string.IsNullOrWhiteSpace(ecommerce.FullDescriptionEn) ||
                    ecommerce.FullDescriptionEn.Length < 40)
                {
                    ecommerce.FullDescriptionEn =
                        "A front-end e-commerce application built with HTML, CSS, JavaScript, and Bootstrap. It includes admin and seller dashboards, sales charts, product/user/order management, a shopping cart, checkout, and a Visa payment simulation. Data and images are persisted locally using LocalStorage.";
                }

                if (string.IsNullOrWhiteSpace(ecommerce.FullDescriptionAr) ||
                    ecommerce.FullDescriptionAr.Length < 40)
                {
                    ecommerce.FullDescriptionAr =
                        "تطبيق تجارة إلكترونية للواجهة الأمامية مبني باستخدام HTML وCSS وJavaScript وBootstrap. يشمل لوحات تحكم للمدير والبائع ورسومًا بيانية للمبيعات وإدارة المنتجات والمستخدمين والطلبات، وسلة تسوق وإتمام شراء ومحاكاة دفع بفيزا. تُحفظ البيانات والصور محليًا باستخدام LocalStorage.";
                }

                ecommerce.IconClass = "fas fa-shopping-cart";
                ecommerce.Category = "web";
                ecommerce.IsActive = true;

                foreach (var technology in new[] { "HTML", "CSS", "JavaScript", "Bootstrap" })
                {
                    EnsureTechnology(ecommerce, technology);
                }
            }

            var seeds = new[]
            {
                new Project
                {
                    TitleEn = "Netflix Clone Streaming Platform",
                    TitleAr = "منصة بث شبيهة بنتفليكس",
                    ShortDescriptionEn = "A full-stack streaming platform with subscriptions, secure authentication, real-time notifications, and a complete admin dashboard.",
                    ShortDescriptionAr = "منصة بث متكاملة تتضمن الاشتراكات والمصادقة الآمنة والإشعارات الفورية ولوحة تحكم كاملة.",
                    FullDescriptionEn = "A full-stack Netflix-style platform built with ASP.NET Core and Angular. The back end applies Domain-Driven Design and clean architecture with CQRS and MediatR; the Angular front end is organised as an NX monorepo. Features include JWT authentication, subscription plans, Stripe payments, SignalR notifications, and an admin dashboard with reports, pagination, and filtering. The API and database layer use Entity Framework Core with SQL Server, and the application is containerized with Docker and delivered through a GitHub Actions pipeline.",
                    FullDescriptionAr = "منصة بث متكاملة على غرار نتفليكس مبنية باستخدام ASP.NET Core وAngular. يطبّق الجزء الخلفي مبادئ التصميم الموجه بالمجال والهندسة النظيفة مع CQRS وMediatR، بينما تُنظَّم واجهة Angular كـ monorepo باستخدام NX. تشمل المزايا المصادقة بـ JWT وخطط الاشتراك ومدفوعات Stripe وإشعارات SignalR ولوحة تحكم مع تقارير وترقيم صفحات وتصفية. تعتمد طبقة البيانات على Entity Framework Core مع SQL Server، ويتم تشغيل التطبيق عبر Docker مع خط نشر آلي باستخدام GitHub Actions.",
                    IconClass = "fas fa-film",
                    Category = "web",
                    IsHidden = false,
                    Order = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Technologies =
                    {
                        new ProjectTechnology { TechnologyName = "ASP.NET Core" },
                        new ProjectTechnology { TechnologyName = "C#" },
                        new ProjectTechnology { TechnologyName = "Angular" },
                        new ProjectTechnology { TechnologyName = "Entity Framework Core" },
                        new ProjectTechnology { TechnologyName = "SQL Server" },
                        new ProjectTechnology { TechnologyName = "SignalR" },
                        new ProjectTechnology { TechnologyName = "Docker" },
                        new ProjectTechnology { TechnologyName = "Stripe" }
                    }
                },
                new Project
                {
                    TitleEn = "Job Portal System",
                    TitleAr = "نظام بوابة وظائف",
                    ShortDescriptionEn = "A full-stack job portal connecting employers and job seekers with dashboards, CV uploads, and application tracking.",
                    ShortDescriptionAr = "بوابة وظائف متكاملة تربط أصحاب العمل والباحثين عن عمل، مع لوحات تحكم ورفع السير الذاتية ومتابعة الطلبات.",
                    FullDescriptionEn = "A professional job portal built with ASP.NET Core MVC, Razor views, Entity Framework Core, and SQL Server. The system implements role-based access for administrators, employers, and job seekers. It includes dedicated dashboards, company profiles, CV uploads, job posting management, and end-to-end application handling with validation and authorization at every layer.",
                    FullDescriptionAr = "بوابة وظائف احترافية مبنية باستخدام ASP.NET Core MVC وRazor وEntity Framework Core وSQL Server. يطبّق النظام صلاحيات قائمة على الأدوار للمدير وأصحاب العمل والباحثين عن عمل، ويتضمن لوحات تحكم وملفات شركات ورفع السير الذاتية وإدارة الوظائف ومعالجة كاملة للطلبات مع التحقق والتفويض في كل طبقة.",
                    IconClass = "fas fa-briefcase",
                    Category = "web",
                    IsHidden = false,
                    Order = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Technologies =
                    {
                        new ProjectTechnology { TechnologyName = "ASP.NET Core MVC" },
                        new ProjectTechnology { TechnologyName = "Razor Views" },
                        new ProjectTechnology { TechnologyName = "Entity Framework Core" },
                        new ProjectTechnology { TechnologyName = "SQL Server" }
                    }
                },
                new Project
                {
                    TitleEn = "School Attendance System",
                    TitleAr = "نظام حضور مدرسي",
                    ShortDescriptionEn = "A desktop application that automates attendance tracking and reporting for schools and training centres.",
                    ShortDescriptionAr = "تطبيق سطح مكتب يؤتمت تسجيل الحضور وإعداد التقارير للمدارس ومراكز التدريب.",
                    FullDescriptionEn = "A Windows Forms desktop application built with C#, Entity Framework Core, and SQL Server. It records daily attendance, manages student and class data, and generates attendance reports for schools and organisations. The data layer uses EF Core with parameterized queries to keep records consistent and secure.",
                    FullDescriptionAr = "تطبيق سطح مكتب باستخدام Windows Forms وC# وEntity Framework Core وSQL Server. يسجّل الحضور اليومي ويدير بيانات الطلاب والفصول ويولّد تقارير الحضور للمدارس والمؤسسات. تستخدم طبقة البيانات EF Core مع استعلامات معلمات للحفاظ على اتساق السجلات وأمانها.",
                    IconClass = "fas fa-school",
                    Category = "desktop",
                    IsHidden = false,
                    Order = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Technologies =
                    {
                        new ProjectTechnology { TechnologyName = "C#" },
                        new ProjectTechnology { TechnologyName = "Windows Forms" },
                        new ProjectTechnology { TechnologyName = "Entity Framework Core" },
                        new ProjectTechnology { TechnologyName = "SQL Server" }
                    }
                }
            };

            foreach (var seed in seeds)
            {
                var alreadyExists = projects.Any(p =>
                    string.Equals(p.TitleEn, seed.TitleEn, StringComparison.OrdinalIgnoreCase));

                if (!alreadyExists)
                {
                    context.Projects.Add(seed);
                }
            }
        }

        private static async Task SeedCertificatesAsync(ApplicationDbContext context)
        {
            if (await context.Certificates.AnyAsync())
            {
                return;
            }

            var certificates = new[]
            {
                new Certificate
                {
                    NameEn = "Full Stack Developer Using .NET — ITP",
                    NameAr = "مطور متكامل باستخدام .NET — برنامج ITP",
                    Issuer = "Information Technology Institute (ITI)",
                    IssueDate = new DateTime(2025, 11, 1),
                    Order = 1,
                    IsActive = true
                },
                new Certificate
                {
                    NameEn = "Full Stack .NET Developer — DEPI",
                    NameAr = "مطور .NET متكامل — DEPI",
                    Issuer = "Digital Egypt Pioneers Initiative (MCIT)",
                    IssueDate = new DateTime(2025, 5, 1),
                    Order = 2,
                    IsActive = true
                },
                new Certificate
                {
                    NameEn = "Front End Web Development — FWD",
                    NameAr = "تطوير الواجهات الأمامية — FWD",
                    Issuer = "Ministry of Communications and Information Technology (MCIT)",
                    IssueDate = new DateTime(2022, 11, 1),
                    Order = 3,
                    IsActive = true
                },
                new Certificate
                {
                    NameEn = "Summer Code Camp — Front End Web Development",
                    NameAr = "المعسكر الصيفي — تطوير الواجهات الأمامية",
                    Issuer = "Information Technology Institute (ITI)",
                    IssueDate = new DateTime(2021, 8, 1),
                    Order = 4,
                    IsActive = true
                }
            };

            context.Certificates.AddRange(certificates);
        }

        private static async Task SeedSectionsAsync(ApplicationDbContext context)
        {
            var sections = await context.Sections.ToListAsync();

            Section? Find(string identifier) => sections.FirstOrDefault(s =>
                string.Equals(s.Identifier, identifier, StringComparison.OrdinalIgnoreCase));

            var blog = Find("blog");
            if (blog == null)
            {
                context.Sections.Add(new Section
                {
                    NameEn = "Blog",
                    NameAr = "المدونة",
                    Identifier = "blog",
                    IconClass = "fas fa-blog",
                    Order = 6,
                    IsVisible = true,
                    IsBuiltIn = false,
                    ShowInNavbar = true
                });
            }
            else
            {
                blog.IsVisible = true;
                blog.ShowInNavbar = true;
            }

            var certificates = Find("certificates");
            if (certificates == null)
            {
                context.Sections.Add(new Section
                {
                    NameEn = "Certificates",
                    NameAr = "الشهادات",
                    Identifier = "certificates",
                    IconClass = "fas fa-certificate",
                    Order = 7,
                    IsVisible = true,
                    IsBuiltIn = false,
                    ShowInNavbar = true
                });
            }
            else
            {
                certificates.IsVisible = true;
                certificates.ShowInNavbar = true;
            }

            var contact = Find("contact");
            if (contact != null && contact.Order < 9)
            {
                contact.Order = 9;
            }
        }

        private static async Task SeedBlogAsync(ApplicationDbContext context)
        {
            var posts = await context.BlogPosts.ToListAsync();

            // Hide the original placeholder post if it is still around.
            foreach (var placeholder in posts.Where(p =>
                         string.Equals(p.Slug, "hekedlf", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(p.TitleEn, "hekedlf", StringComparison.OrdinalIgnoreCase)))
            {
                placeholder.IsPublished = false;
            }

            if (posts.Any(p => p.IsPublished))
            {
                return;
            }

            var seeds = new[]
            {
                new BlogPost
                {
                    TitleEn = "Clean Architecture in ASP.NET Core: A Practical Starting Point",
                    TitleAr = "الهندسة النظيفة في ASP.NET Core: نقطة بداية عملية",
                    Slug = "clean-architecture-aspnet-core",
                    PublishedAt = new DateTime(2026, 9, 15),
                    IsPublished = true,
                    ContentEn =
                        """
                        <p>Clean architecture is less about adding folders and more about controlling dependencies. In an ASP.NET Core solution, the goal is to keep business rules independent from frameworks, databases, and UI concerns.</p>
                        <h2>The four layers</h2>
                        <ul>
                            <li><strong>Domain</strong> — entities and business invariants with no external dependencies.</li>
                            <li><strong>Application</strong> — use cases, commands, queries, and interfaces.</li>
                            <li><strong>Infrastructure</strong> — EF Core, file storage, email, and other implementations.</li>
                            <li><strong>Web</strong> — controllers, Razor views, middleware, and composition root.</li>
                        </ul>
                        <h2>The dependency rule</h2>
                        <p>Dependencies point inward. The Domain project references nothing. The Application project references Domain. Infrastructure and Web reference Application, but Application never references them. This keeps the core testable and lets you replace infrastructure without rewriting business logic.</p>
                        <h2>A minimal layout</h2>
                        <pre><code>src/
                          Portfolio.Domain/
                          Portfolio.Application/
                          Portfolio.Infrastructure/
                          Portfolio.Web/</code></pre>
                        <h2>Where to start</h2>
                        <p>Start with one real use case, not with a full abstraction map. Move code inward only when it needs to be reused or tested independently. A small solution with a clear dependency direction is more valuable than a large one with perfect folder names.</p>
                        """,
                    ContentAr =
                        """
                        <p>الهندسة النظيفة لا تعني إضافة مجلدات أكثر، بل التحكم في اتجاه الاعتماديات. في حل ASP.NET Core يكون الهدف هو فصل قواعد العمل عن الأطر وقواعد البيانات وواجهة المستخدم.</p>
                        <h2>الطبقات الأربع</h2>
                        <ul>
                            <li><strong>Domain</strong> — الكيانات وقواعد العمل دون أي اعتماديات خارجية.</li>
                            <li><strong>Application</strong> — حالات الاستخدام والأوامر والاستعلامات والواجهات.</li>
                            <li><strong>Infrastructure</strong> — تنفيذ EF Core والتخزين والبريد وغيرها.</li>
                            <li><strong>Web</strong> — المتحكمات وواجهات Razor والوسائط ونقطة تجميع الخدمات.</li>
                        </ul>
                        <h2>قاعدة الاعتماديات</h2>
                        <p>تتجه الاعتماديات إلى الداخل. مشروع Domain لا يعتمد على شيء، وApplication يعتمد على Domain، بينما تعتمد Infrastructure وWeb على Application، ولا يحدث العكس. بهذا يبقى جوهر النظام قابلًا للاختبار ويمكن استبدال البنية التحتية دون إعادة كتابة منطق العمل.</p>
                        <h2>هيكل بسيط</h2>
                        <pre><code>src/
                          Portfolio.Domain/
                          Portfolio.Application/
                          Portfolio.Infrastructure/
                          Portfolio.Web/</code></pre>
                        <h2>من أين تبدأ</h2>
                        <p>ابدأ بحالة استخدام حقيقية واحدة، لا بخريطة كاملة من التجريدات. انقل الكود إلى الداخل فقط عندما يحتاج إلى إعادة استخدام أو اختبار مستقل. الحل الصغير ذو الاتجاه الواضح للاعتماديات أكثر قيمة من حل ضخم بأسماء مجلدات مثالية.</p>
                        """
                },
                new BlogPost
                {
                    TitleEn = "Entity Framework Core Performance: Five Habits That Pay Off",
                    TitleAr = "أداء Entity Framework Core: خمس عادات فعالة",
                    Slug = "ef-core-performance-habits",
                    PublishedAt = new DateTime(2026, 10, 6),
                    IsPublished = true,
                    ContentEn =
                        """
                        <p>Most EF Core performance problems are not caused by the ORM itself. They come from how we query and how much data we move. These five habits cover the highest-impact cases.</p>
                        <h2>1. Use AsNoTracking for read-only queries</h2>
                        <p>Change tracking is useful when you intend to update entities. For lists and reports, <code>AsNoTracking()</code> avoids the cost of snapshotting every row.</p>
                        <pre><code>var posts = await context.BlogPosts
                            .AsNoTracking()
                            .Where(p => p.IsPublished)
                            .OrderByDescending(p => p.PublishedAt)
                            .Take(20)
                            .ToListAsync();</code></pre>
                        <h2>2. Project instead of loading whole entities</h2>
                        <p>Select only the columns the screen needs. A projection reduces I/O, memory, and serialization cost.</p>
                        <h2>3. Paginate at the database</h2>
                        <p>Always apply <code>Skip</code> and <code>Take</code> before materialising a list. Loading a full table and paging in memory is a common source of slow pages.</p>
                        <h2>4. Prevent N+1 queries</h2>
                        <p>Use <code>Include</code> and <code>ThenInclude</code> deliberately, or project related data in one query. Enable query logging in development so repeated statements are visible.</p>
                        <h2>5. Index the columns you filter and sort by</h2>
                        <p>Add indexes for foreign keys, slugs, and frequently filtered columns. Verify the execution plan before adding more indexes — each one has a write cost.</p>
                        """,
                    ContentAr =
                        """
                        <p>معظم مشاكل الأداء في Entity Framework Core لا يسببها الـ ORM نفسه، بل طريقة الاستعلام وكمية البيانات المنقولة. تغطي هذه العادات الخمس الحالات الأكثر تأثيرًا.</p>
                        <h2>1. استخدم AsNoTracking للاستعلامات للقراءة فقط</h2>
                        <p>تتبع التغييرات مفيد عند نية تحديث الكيانات. أما القوائم والتقارير فيوفر لها <code>AsNoTracking()</code> تكلفة تتبع كل صف.</p>
                        <pre><code>var posts = await context.BlogPosts
                            .AsNoTracking()
                            .Where(p => p.IsPublished)
                            .OrderByDescending(p => p.PublishedAt)
                            .Take(20)
                            .ToListAsync();</code></pre>
                        <h2>2. استخدم الإسقاط بدل تحميل الكيانات كاملة</h2>
                        <p>اختر الأعمدة التي تحتاجها الشاشة فقط. الإسقاط يقلل الإدخال والإخراج والذاكرة وتكلفة التسلسل.</p>
                        <h2>3. طبّق الترقيم داخل قاعدة البيانات</h2>
                        <p>استخدم <code>Skip</code> و<code>Take</code> قبل تحميل القائمة. تحميل الجدول بالكامل ثم الترقيم في الذاكرة سبب شائع لبطء الصفحات.</p>
                        <h2>4. تجنب استعلامات N+1</h2>
                        <p>استخدم <code>Include</code> و<code>ThenInclude</code> بوعي، أو اسقط البيانات المرتبطة في استعلام واحد. فعّل تسجيل الاستعلامات أثناء التطوير لرؤية التكرار.</p>
                        <h2>5. افهرس الأعمدة التي تفلتر أو ترتب بها</h2>
                        <p>أضف فهارس للمفاتيح الأجنبية والـ slug والأعمدة كثيرة التصفية. راجع خطة التنفيذ قبل إضافة فهارس جديدة، فلكل فهرس تكلفة على الكتابة.</p>
                        """
                },
                new BlogPost
                {
                    TitleEn = "From ITI to Full Stack: What Intensive Training Taught Me",
                    TitleAr = "من ITI إلى مطور متكامل: ماذا علّمني التدريب المكثف",
                    Slug = "from-iti-to-full-stack",
                    PublishedAt = new DateTime(2026, 10, 28),
                    IsPublished = true,
                    ContentEn =
                        """
                        <p>My path into .NET development was built through structured training rather than a single course. Each program added a layer: front-end fundamentals, then backend development, then a complete full-stack project.</p>
                        <h2>Start with the web platform, not the framework</h2>
                        <p>FWD and the ITI Summer Code Camp gave me a solid grounding in HTML, CSS, and JavaScript. Understanding the browser made it much easier to debug rendered pages and write accessible markup later.</p>
                        <h2>Learn the database alongside the language</h2>
                        <p>DEPI introduced SQL Server, Entity Framework Core, and role-based application design. Designing tables and relationships before writing controllers changed how I approach features.</p>
                        <h2>Ship something end to end</h2>
                        <p>The ITI ITP program ended with a team project that covered authentication, dashboards, uploads, and deployment. Working through integration problems with a team taught me more than isolated exercises.</p>
                        <h2>Keep the feedback loop short</h2>
                        <p>Build a small slice, run it, inspect the response, and improve it. That habit — not any specific framework — is what makes the next project faster and more reliable.</p>
                        """,
                    ContentAr =
                        """
                        <p>بُني طريقي إلى تطوير .NET عبر تدريب منظم بدل دورة واحدة. أضاف كل برنامج طبقة جديدة: أساسيات الواجهات الأمامية، ثم التطوير الخلفي، ثم مشروع متكامل.</p>
                        <h2>ابدأ بمنصة الويب لا بالإطار</h2>
                        <p>منحني برنامج FWD والمعسكر الصيفي لـ ITI أساسًا قويًا في HTML وCSS وJavaScript. فهم المتصفح جعل تصحيح الصفحات وكتابة ترميز سهل الوصول أسهل بكثير لاحقًا.</p>
                        <h2>تعلّم قاعدة البيانات مع اللغة</h2>
                        <p>عرّفني DEPI على SQL Server وEntity Framework Core وتصميم التطبيقات القائمة على الأدوار. تصميم الجداول والعلاقات قبل كتابة المتحكمات غيّر طريقة تعاملي مع المزايا.</p>
                        <h2>سلّم شيئًا متكاملًا</h2>
                        <p>انتهى برنامج ITP بمشروع جماعي شمل المصادقة ولوحات التحكم والرفع والنشر. حل مشكلات التكامل مع فريق علّمني أكثر من التمارين المنفصلة.</p>
                        <h2>اجعل حلقة التغذية الراجعة قصيرة</h2>
                        <p>ابنِ جزءًا صغيرًا، وشغّله، وافحص النتيجة، ثم حسّنه. هذه العادة — لا أي إطار بعينه — هي ما يجعل المشروع التالي أسرع وأكثر موثوقية.</p>
                        """
                }
            };

            foreach (var post in seeds)
            {
                post.Tags.Add(new BlogTag { TagName = "ASP.NET Core" });
                post.Tags.Add(new BlogTag
                {
                    TagName = post.Slug == "from-iti-to-full-stack" ? "Career" : "Best Practices"
                });
            }

            context.BlogPosts.AddRange(seeds);
        }
    }
}
