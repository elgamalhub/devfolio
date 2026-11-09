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
            await SeedModernProjectsAsync(context);
            await SeedSkillsAsync(context);
            await SeedSocialLinksAsync(context);
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

            about.DescriptionEn =
                "Full-stack .NET developer who ships production-shaped systems end to end — from database design and secure APIs to polished Angular interfaces and containerised deployment. " +
                "I work daily with C#, ASP.NET Core, Entity Framework Core, SQL Server, Angular, and TypeScript, and I care about clean architecture, concurrency safety, observability, and automated tests. " +
                "Recent work includes EvTicket (a .NET 10 + Angular 21 ticketing platform with atomic seat locking and real-time availability), NileLearn (a bilingual LMS API with payments and secure video), and this data-driven portfolio. " +
                "I completed intensive training through ITI's ITP program, the Digital Egypt Pioneers Initiative (DEPI), FWD, and ITI's Summer Code Camp.";

            about.DescriptionAr =
                "مطور .NET متكامل يبني أنظمة جاهزة للإنتاج من البداية للنهاية — من تصميم قاعدة البيانات وواجهات API الآمنة إلى واجهات Angular المصقولة والنشر باستخدام الحاويات. " +
                "أعمل يوميًا بـ C# وASP.NET Core وEntity Framework Core وSQL Server وAngular وTypeScript، وأهتم بالهندسة النظيفة وسلامة التزامن وقابلية المراقبة والاختبارات الآلية. " +
                "من أحدث أعمالي EvTicket (منصة تذاكر بـ .NET 10 وAngular 21 مع حجز مقاعد ذري وتوافر لحظي)، وNileLearn (واجهة API لنظام تعليمي ثنائي اللغة مع المدفوعات والفيديو الآمن)، وهذا الموقع الشخصي المدفوع بالبيانات. " +
                "أكملت برامج تدريبية مكثفة من خلال برنامج ITP في معهد تكنولوجيا المعلومات، ومبادرة رواد مصر الرقمية (DEPI)، وبرنامج FWD، والمعسكر الصيفي لـ ITI.";

            about.CVFilePath = "/files/cv/Ahmed_Abdelrahman_Elgamal_CV.pdf";

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

        private static async Task SeedModernProjectsAsync(ApplicationDbContext context)
        {
            var projects = await context.Projects
                .Include(p => p.Technologies)
                .ToListAsync();

            Project Upsert(string titleEn, Action<Project> apply)
            {
                var project = projects.FirstOrDefault(p =>
                    string.Equals(p.TitleEn, titleEn, StringComparison.OrdinalIgnoreCase));

                if (project is null)
                {
                    project = new Project
                    {
                        TitleEn = titleEn,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Projects.Add(project);
                    projects.Add(project);
                }

                apply(project);
                return project;
            }

            void Tech(Project project, params string[] names)
            {
                foreach (var name in names)
                {
                    if (project.Technologies.Any(t =>
                            string.Equals(t.TechnologyName, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    project.Technologies.Add(new ProjectTechnology { TechnologyName = name });
                }
            }

            var evTicket = Upsert("EvTicket — Event Ticketing Platform", project =>
            {
                project.TitleAr = "EvTicket — منصة حجز تذاكر الفعاليات";
                project.ShortDescriptionEn =
                    "A production-shaped ticketing platform that prevents double-booking with geography-aware seat maps, atomic seat holds, live availability, and QR tickets.";
                project.ShortDescriptionAr =
                    "منصة تذاكر جاهزة للإنتاج تمنع الحجز المزدوج عبر خرائط مقاعد جغرافية وحجز ذري وتوافر لحظي وتذاكر QR.";
                project.FullDescriptionEn =
                    "EvTicket is a .NET 10 + Angular 21 monorepo built around one hard guarantee: two people can never buy the same seat. " +
                    "The API uses EF Core 10 with atomic ExecuteUpdateAsync holds, explicit transactions, a background expiry worker, and SignalR to broadcast live seat availability. " +
                    "It ships with JWT access tokens, rotating hashed refresh tokens, QR-coded tickets, organizer analytics, an admin console, Scalar/OpenAPI docs, SQLite for local demos plus SQL Server for production, a single-container Docker image, and xUnit integration tests that reproduce the checkout race (one 200, one 409).";
                project.FullDescriptionAr =
                    "EvTicket مشروع monorepo مبني بـ .NET 10 وAngular 21 ويقوم على ضمان واحد صارم: لا يمكن لشخصين شراء نفس المقعد. " +
                    "تستخدم الواجهة الخلفية EF Core 10 مع حجز ذري عبر ExecuteUpdateAsync ومعاملات صريحة وعامل خلفي لانتهاء الحجوزات وSignalR لبث توافر المقاعد لحظيًا. " +
                    "يتضمن مصادقة JWT ورموز تحديث دوارة ومشفرة وتذاكر QR وتحليلات للمنظمين ولوحة إدارة وتوثيق Scalar/OpenAPI وSQLite للعرض المحلي وSQL Server للإنتاج وصورة Docker واحدة واختبارات تكامل xUnit تعيد إنتاج سباق الدفع (200 و409).";
                project.IconClass = "fas fa-ticket-alt";
                project.Category = "web";
                project.GithubUrl = "https://github.com/elgamalhub/EvTicketApi";
                project.DemoUrl = string.Empty;
                project.ImagePath = "/images/projects/evticket.jpg";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 1;
            });
            Tech(evTicket, ".NET 10", "ASP.NET Core", "C#", "Angular 21", "Entity Framework Core",
                "SignalR", "SQL Server", "SQLite", "JWT", "Docker", "xUnit");

            var nileLearn = Upsert("NileLearn — Learning Management System", project =>
            {
                project.TitleAr = "NileLearn — نظام إدارة التعلم";
                project.ShortDescriptionEn =
                    "A bilingual LMS platform with course authoring, subscriptions, Egyptian payment gateways, secure video, progress tracking, certificates, and admin analytics.";
                project.ShortDescriptionAr =
                    "منصة تعلم ثنائية اللغة تتضمن تأليف الدورات والاشتراكات وبوابات الدفع المصرية والفيديو الآمن وتتبع التقدم والشهادات وتحليلات الإدارة.";
                project.FullDescriptionEn =
                    "NileLearn is an enterprise-style learning management system built with ASP.NET Core 8 and a clean layered architecture (Domain, Application, Infrastructure, API). " +
                    "It covers course authoring and publishing, sections and lessons, free previews, subscriptions and coupons, Egyptian payment gateways (Paymob, Fawry, Kashier) plus Stripe and a sandbox gateway, chunked video upload with signed playback and heartbeat tracking, progress and certificates, quizzes, audit logs, and role-based admin analytics. " +
                    "Security uses ASP.NET Core Identity with JWT access and refresh tokens; the data layer uses EF Core with SQL Server or SQLite, and the suite includes unit and WebApplicationFactory integration tests.";
                project.FullDescriptionAr =
                    "NileLearn نظام إدارة تعلم بمستوى المؤسسات مبني بـ ASP.NET Core 8 وهندسة طبقية نظيفة (Domain وApplication وInfrastructure وAPI). " +
                    "يغطي تأليف الدورات ونشرها والأقسام والدروس والمعاينات المجانية والاشتراكات والكوبونات وبوابات الدفع المصرية (Paymob وFawry وKashier) مع Stripe وبوابة تجريبية، ورفع الفيديو على أجزاء والتشغيل الموقّع وتتبع المشاهدة، والتقدم والشهادات والاختبارات وسجلات التدقيق وتحليلات الإدارة حسب الأدوار. " +
                    "يعتمد الأمان على ASP.NET Core Identity مع JWT ورموز تحديث، وطبقة البيانات على EF Core مع SQL Server أو SQLite، ويشمل المشروع اختبارات وحدة وتكامل عبر WebApplicationFactory.";
                project.IconClass = "fas fa-graduation-cap";
                project.Category = "web";
                project.GithubUrl = "https://github.com/elgamalhub/NileLearn";
                project.DemoUrl = string.Empty;
                project.ImagePath = "/images/projects/nilelearn.jpg";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 2;
            });
            Tech(nileLearn, ".NET 8", "ASP.NET Core", "C#", "Entity Framework Core", "SQL Server",
                "JWT", "Paymob", "Fawry", "Kashier", "Stripe", "xUnit");

            var devfolio = Upsert("Developer Portfolio — Devfolio", project =>
            {
                project.TitleAr = "الموقع الشخصي — Devfolio";
                project.ShortDescriptionEn =
                    "A bilingual, data-driven portfolio and CMS with an admin studio, blog, SEO, static export, and a live GitHub Pages deployment.";
                project.ShortDescriptionAr =
                    "موقع شخصي ثنائي اللغة ومدفوع بالبيانات مع لوحة إدارة ومدونة وتحسين محركات بحث وتصدير ثابت ونشر مباشر على GitHub Pages.";
                project.FullDescriptionEn =
                    "Devfolio is the portfolio you are reading. It is a data-driven ASP.NET Core 9 MVC application with a clean Core/Infrastructure split, EF Core and SQL Server, ASP.NET Core Identity, Serilog, and a full admin studio. " +
                    "Every public section — hero, about, experience, skills, projects, certificates, testimonials, blog, contact, colours, and SEO metadata — is editable from the admin area. " +
                    "It also generates a self-contained static export that is published to GitHub Pages, so the same content powers both the dynamic site and the always-on public version.";
                project.FullDescriptionAr =
                    "Devfolio هو الموقع الشخصي الذي تقرأه الآن؛ تطبيق ASP.NET Core 9 MVC مدفوع بالبيانات بفصل نظيف بين Core وInfrastructure، مع EF Core وSQL Server وASP.NET Core Identity وSerilog ولوحة إدارة كاملة. " +
                    "كل قسم عام — الواجهة الرئيسية والعن والملف والخبرات والمهارات والمشاريع والشهادات وآراء العملاء والمدونة والتواصل والألوان وبيانات SEO — قابل للتحرير من لوحة الإدارة. " +
                    "كما يولّد نسخة ثابتة مستقلة تُنشر على GitHub Pages، لتستخدم نفس المحتوى في الموقع الديناميكي والنسخة العامة الدائمة.";
                project.IconClass = "fas fa-laptop-code";
                project.Category = "web";
                project.GithubUrl = "https://github.com/elgamalhub/devfolio";
                project.DemoUrl = "https://elgamalhub.github.io/devfolio/";
                project.ImagePath = "/images/projects/devfolio.jpg";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 3;
            });
            Tech(devfolio, "ASP.NET Core 9", "C#", "Entity Framework Core", "SQL Server",
                "Identity", "Bootstrap", "Serilog", "GitHub Pages");

            var netflix = Upsert("Netflix Clone Streaming Platform", project =>
            {
                project.GithubUrl = "https://github.com/elgamalhub/NetflixCloneAPI";
                project.ImagePath = "/images/projects/netflix.jpg";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 4;
            });
            Tech(netflix, "ASP.NET Core", "Angular", "Entity Framework Core", "SQL Server",
                "SignalR", "Docker", "Stripe");

            var jobPortal = Upsert("Job Portal System", project =>
            {
                project.GithubUrl = "https://github.com/elgamalhub/JobPortal";
                project.ImagePath = "/images/projects/job-portal.jpg";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 5;
            });
            Tech(jobPortal, "ASP.NET Core MVC", "Razor Views", "Entity Framework Core", "SQL Server");

            var attendance = Upsert("School Attendance System", project =>
            {
                project.GithubUrl = "https://github.com/elgamalhub/AttendanceApp";
                project.ImagePath = "/images/projects/attendance.jpg";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 6;
            });
            Tech(attendance, "C#", "Windows Forms", "Entity Framework Core", "SQL Server");

            var ecommerce = Upsert("E-Commerce Website", project =>
            {
                project.GithubUrl = "https://github.com/Tanta-itp-team1/ecommerce-project";
                project.DemoUrl = "https://tanta-itp-team1.github.io/ecommerce-project/Home/homepage.html";
                project.IconClass = "fas fa-shopping-cart";
                project.IsHidden = false;
                project.IsActive = true;
                project.Order = 7;
            });
            Tech(ecommerce, "HTML", "CSS", "JavaScript", "Bootstrap");
        }

        private static async Task SeedSkillsAsync(ApplicationDbContext context)
        {
            var skills = await context.Skills.ToListAsync();

            void Upsert(string nameEn, string nameAr, int percentage, int order)
            {
                var skill = skills.FirstOrDefault(s =>
                    string.Equals(s.NameEn, nameEn, StringComparison.OrdinalIgnoreCase));

                if (skill is null)
                {
                    skill = new Skill { NameEn = nameEn };
                    context.Skills.Add(skill);
                    skills.Add(skill);
                }

                skill.NameAr = nameAr;
                skill.Percentage = percentage;
                skill.Order = order;
                skill.IsActive = true;
            }

            Upsert("C# / .NET", "C# / .NET", 90, 1);
            Upsert("ASP.NET Core (API & MVC)", "ASP.NET Core (API & MVC)", 88, 2);
            Upsert("Entity Framework Core", "Entity Framework Core", 85, 3);
            Upsert("SQL Server / T-SQL", "SQL Server / T-SQL", 82, 4);
            Upsert("Angular 21 / TypeScript", "Angular 21 / TypeScript", 82, 5);
            Upsert("HTML / CSS / Bootstrap", "HTML / CSS / Bootstrap", 90, 6);
            Upsert("SignalR / Real-time systems", "SignalR / الأنظمة اللحظية", 78, 7);
            Upsert("Docker / CI-CD", "Docker / CI-CD", 75, 8);
            Upsert("JWT / Identity / Security", "JWT / Identity / الأمان", 80, 9);
            Upsert("xUnit / Integration testing", "xUnit / اختبارات التكامل", 75, 10);
            Upsert("Clean Architecture / DDD", "الهندسة النظيفة / DDD", 80, 11);
            Upsert("Git / GitHub / Postman", "Git / GitHub / Postman", 85, 12);
        }

        private static async Task SeedSocialLinksAsync(ApplicationDbContext context)
        {
            var links = await context.SocialLinks.ToListAsync();

            void Upsert(string platform, string url, string iconClass, int order)
            {
                var link = links.FirstOrDefault(l =>
                    string.Equals(l.Platform, platform, StringComparison.OrdinalIgnoreCase));

                if (link is null)
                {
                    link = new SocialLink { Platform = platform };
                    context.SocialLinks.Add(link);
                    links.Add(link);
                }

                link.Url = url;
                link.IconClass = iconClass;
                link.Order = order;
                link.IsActive = true;
            }

            Upsert("GitHub", "https://github.com/elgamalhub", "fab fa-github", 1);
            Upsert("LinkedIn", "https://www.linkedin.com/in/ahmed-elgamal1", "fab fa-linkedin", 2);
            Upsert("Email", "mailto:ahmed.elgamal.dev22@gmail.com", "fas fa-envelope", 3);
            Upsert("Phone", "tel:+201022172951", "fas fa-phone", 4);
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
