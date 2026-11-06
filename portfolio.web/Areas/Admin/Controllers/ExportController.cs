using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.Entities;
using Portfolio.Core.Interfaces;
using System.IO.Compression;
using System.Text;

namespace portfolio.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ExportController : Controller
    {
        private readonly IRepository<About> _aboutRepo;
        private readonly IRepository<Skill> _skillRepo;
        private readonly IRepository<Experience> _expRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IRepository<SocialLink> _socialRepo;
        private readonly IRepository<TypingTitle> _titleRepo;
        private readonly IRepository<Testimonial> _testimonialRepo;
        private readonly IRepository<SiteSettings> _settingsRepo;
        private readonly IRepository<Section> _sectionRepo;
        private readonly IWebHostEnvironment _env;

        public ExportController(
            IRepository<About> aboutRepo,
            IRepository<Skill> skillRepo,
            IRepository<Experience> expRepo,
            IProjectRepository projectRepo,
            IRepository<SocialLink> socialRepo,
            IRepository<TypingTitle> titleRepo,
            IRepository<Testimonial> testimonialRepo,
            IRepository<SiteSettings> settingsRepo,
            IRepository<Section> sectionRepo,
            IWebHostEnvironment env)
        {
            _aboutRepo = aboutRepo;
            _skillRepo = skillRepo;
            _expRepo = expRepo;
            _projectRepo = projectRepo;
            _socialRepo = socialRepo;
            _titleRepo = titleRepo;
            _testimonialRepo = testimonialRepo;
            _settingsRepo = settingsRepo;
            _sectionRepo = sectionRepo;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportAsZip(string language = "en")
        {
            try
            {
                // Create temp directory
                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                // Generate HTML
                var html = await GenerateStaticHTML(language);
                await System.IO.File.WriteAllTextAsync(Path.Combine(tempPath, "index.html"), html);

                // Copy CSS
                var cssPath = Path.Combine(_env.WebRootPath, "css", "portfolio.css");
                if (System.IO.File.Exists(cssPath))
                {
                    var cssDir = Path.Combine(tempPath, "css");
                    Directory.CreateDirectory(cssDir);
                    System.IO.File.Copy(cssPath, Path.Combine(cssDir, "portfolio.css"));
                }

                // Copy JS (standalone version)
                var jsContent = GenerateStandaloneJS(language);
                var jsDir = Path.Combine(tempPath, "js");
                Directory.CreateDirectory(jsDir);
                await System.IO.File.WriteAllTextAsync(Path.Combine(jsDir, "portfolio.js"), jsContent);

                // Copy images
                await CopyImages(tempPath);

                // Create README
                var readme = GenerateReadme();
                await System.IO.File.WriteAllTextAsync(Path.Combine(tempPath, "README.md"), readme);

                // Create ZIP
                var zipPath = Path.Combine(Path.GetTempPath(), $"portfolio-export-{DateTime.Now:yyyyMMddHHmmss}.zip");
                if (System.IO.File.Exists(zipPath))
                    System.IO.File.Delete(zipPath);

                ZipFile.CreateFromDirectory(tempPath, zipPath);

                // Clean up temp directory
                Directory.Delete(tempPath, true);

                // Return file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
                System.IO.File.Delete(zipPath);

                return File(fileBytes, "application/zip", $"portfolio-{language}-{DateTime.Now:yyyyMMddHHmmss}.zip");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Export failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<string> GenerateStaticHTML(string lang)
        {
            var about = (await _aboutRepo.GetAllAsync()).FirstOrDefault();
            var skills = (await _skillRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.Order).ToList();
            var experiences = (await _expRepo.GetAllAsync()).Where(e => e.IsActive).OrderByDescending(e => e.StartDate).ToList();
            var projects = await _projectRepo.GetActiveProjectsOrderedAsync();
            var socialLinks = (await _socialRepo.GetAllAsync()).Where(s => s.IsActive).OrderBy(s => s.Order).ToList();
            var typingTitles = (await _titleRepo.GetAllAsync()).Where(t => t.IsActive).OrderBy(t => t.Order).ToList();
            var testimonials = (await _testimonialRepo.GetAllAsync()).Where(t => t.IsActive).OrderBy(t => t.Order).ToList();
            var settings = (await _settingsRepo.GetAllAsync()).ToDictionary(s => s.Key, s => s.ValueEn);
            var sections = (await _sectionRepo.GetAllAsync()).Where(s => s.IsVisible).OrderBy(s => s.Order).ToList();

            var primaryColor = settings.GetValueOrDefault("primary_color", "#667eea");
            var secondaryColor = settings.GetValueOrDefault("secondary_color", "#764ba2");
            var siteTitle = lang == "en" ? settings.GetValueOrDefault("site_title_en", "Portfolio") : settings.GetValueOrDefault("site_title_ar", "محفظتي");
            var metaDescription = lang == "en" ? settings.GetValueOrDefault("meta_description_en", "") : settings.GetValueOrDefault("meta_description_ar", "");

            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine($"<html lang=\"{lang}\" dir=\"{(lang == "ar" ? "rtl" : "ltr")}\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine($"    <title>{siteTitle}</title>");
            html.AppendLine($"    <meta name=\"description\" content=\"{metaDescription}\">");
            html.AppendLine("    <link href=\"https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css\" rel=\"stylesheet\">");
            html.AppendLine("    <link href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css\" rel=\"stylesheet\">");
            html.AppendLine("    <link href=\"css/portfolio.css\" rel=\"stylesheet\">");
            html.AppendLine("    <style>");
            html.AppendLine("        :root {");
            html.AppendLine($"            --primary-color: {primaryColor};");
            html.AppendLine($"            --secondary-color: {secondaryColor};");
            html.AppendLine("        }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Preloader
            html.AppendLine("    <div id=\"preloader\">");
            html.AppendLine("        <div class=\"spinner\"></div>");
            html.AppendLine("    </div>");

            // Scroll Progress
            html.AppendLine("    <div id=\"scrollProgress\"></div>");

            // Back to Top
            if (settings.GetValueOrDefault("show_back_to_top", "true") == "true")
            {
                html.AppendLine("    <button id=\"backToTop\"><i class=\"fas fa-arrow-up\"></i></button>");
            }

            // Navbar
            html.AppendLine("    <nav class=\"navbar navbar-expand-lg navbar-dark fixed-top\">");
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <a class=\"navbar-brand\" href=\"#\"><i class=\"fas fa-code\"></i> {siteTitle}</a>");
            html.AppendLine("            <button class=\"navbar-toggler\" type=\"button\" data-bs-toggle=\"collapse\" data-bs-target=\"#navbarNav\">");
            html.AppendLine("                <span class=\"navbar-toggler-icon\"></span>");
            html.AppendLine("            </button>");
            html.AppendLine("            <div class=\"collapse navbar-collapse\" id=\"navbarNav\">");
            html.AppendLine("                <ul class=\"navbar-nav ms-auto\">");

            foreach (var section in sections.Where(s => s.ShowInNavbar))
            {
                var sectionName = lang == "en" ? section.NameEn : section.NameAr;
                html.AppendLine($"                    <li class=\"nav-item\"><a class=\"nav-link\" href=\"#{section.Identifier}\">{sectionName}</a></li>");
            }

            html.AppendLine("                </ul>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </nav>");

            // Sections
            foreach (var section in sections)
            {
                html.AppendLine($"    <section id=\"{section.Identifier}\">");

                if (section.Identifier == "home")
                {
                    GenerateHeroSection(html, about, lang);
                }
                else if (section.Identifier == "about")
                {
                    GenerateAboutSection(html, about, socialLinks, lang);
                }
                else if (section.Identifier == "experience")
                {
                    GenerateExperienceSection(html, experiences, lang);
                }
                else if (section.Identifier == "skills")
                {
                    GenerateSkillsSection(html, skills, lang);
                }
                else if (section.Identifier == "projects")
                {
                    GenerateProjectsSection(html, projects, lang);
                }
                else if (section.Identifier == "testimonials" && testimonials.Any())
                {
                    GenerateTestimonialsSection(html, testimonials, lang);
                }
                else if (section.Identifier == "contact")
                {
                    GenerateContactSection(html, about, socialLinks, lang);
                }

                html.AppendLine("    </section>");
            }

            // Footer
            html.AppendLine("    <footer>");
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <p>&copy; {DateTime.Now.Year} {(lang == "en" ? about?.NameEn : about?.NameAr)}. All rights reserved.</p>");
            html.AppendLine("        </div>");
            html.AppendLine("    </footer>");

            // Scripts
            html.AppendLine("    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/js/bootstrap.bundle.min.js\"></script>");
            html.AppendLine("    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/gsap/3.12.2/gsap.min.js\"></script>");
            html.AppendLine("    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/gsap/3.12.2/ScrollTrigger.min.js\"></script>");
            html.AppendLine("    <script>");
            html.AppendLine($"        const typingTitles = {System.Text.Json.JsonSerializer.Serialize(typingTitles.Select(t => lang == "en" ? t.TitleEn : t.TitleAr))};");
            html.AppendLine($"        const currentLang = '{lang}';");
            html.AppendLine($"        const enableParticles = {settings.GetValueOrDefault("enable_particles", "true").ToLower()};");
            html.AppendLine("    </script>");
            html.AppendLine("    <script src=\"js/portfolio.js\"></script>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private void GenerateHeroSection(StringBuilder html, About? about, string lang)
        {
            html.AppendLine("        <div class=\"hero\">");
            if (about != null)
            {
                html.AppendLine("            <canvas id=\"particles-canvas\"></canvas>");
                html.AppendLine("            <div class=\"hero-content\">");
                html.AppendLine($"                <h1>{(lang == "en" ? $"Hi, I'm {about.NameEn}" : $"مرحباً، أنا {about.NameAr}")}</h1>");
                html.AppendLine("                <p class=\"tagline\">");
                html.AppendLine("                    <span class=\"typing-text\" id=\"typingText\"></span>");
                html.AppendLine("                </p>");
                html.AppendLine("                <div class=\"hero-buttons\">");
                html.AppendLine($"                    <button class=\"btn btn-custom me-3\" onclick=\"document.getElementById('contact').scrollIntoView({{behavior: 'smooth'}})\">");
                html.AppendLine($"                        {(lang == "en" ? "Get In Touch" : "تواصل معي")} <i class=\"fas fa-arrow-right ms-2\"></i>");
                html.AppendLine("                    </button>");
                if (!string.IsNullOrEmpty(about.CVFilePath))
                {
                    html.AppendLine($"                    <a href=\"{about.CVFilePath}\" download class=\"btn btn-custom-outline\">");
                    html.AppendLine($"                        <i class=\"fas fa-download me-2\"></i> {(lang == "en" ? "Download CV" : "تحميل السيرة الذاتية")}");
                    html.AppendLine("                    </a>");
                }
                html.AppendLine("                </div>");
                html.AppendLine("            </div>");
            }
            html.AppendLine("        </div>");
        }

        private void GenerateAboutSection(StringBuilder html, About? about, List<SocialLink> socialLinks, string lang)
        {
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <h2 class=\"section-title\">{(lang == "en" ? "About Me" : "عني")}</h2>");
            html.AppendLine("            <div class=\"row justify-content-center\">");
            html.AppendLine("                <div class=\"col-lg-8\">");
            html.AppendLine("                    <div class=\"about-card\">");

            if (!string.IsNullOrEmpty(about?.ProfileImage))
            {
                html.AppendLine("                        <div class=\"text-center mb-4\">");
                html.AppendLine($"                            <img src=\".{about.ProfileImage}\" alt=\"{about.NameEn}\" class=\"rounded-circle\" style=\"width: 200px; height: 200px; object-fit: cover; border: 5px solid var(--primary-color);\">");
                html.AppendLine("                        </div>");
            }

            html.AppendLine($"                        <p class=\"lead\">{(lang == "en" ? about?.DescriptionEn : about?.DescriptionAr)}</p>");

            if (socialLinks.Any())
            {
                html.AppendLine("                        <div class=\"social-links mt-4 text-center\">");
                foreach (var link in socialLinks)
                {
                    html.AppendLine($"                            <a href=\"{link.Url}\" target=\"_blank\" class=\"social-icon\" title=\"{link.Platform}\">");
                    html.AppendLine($"                                <i class=\"{link.IconClass}\"></i>");
                    html.AppendLine("                            </a>");
                }
                html.AppendLine("                        </div>");
            }

            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }

        private void GenerateExperienceSection(StringBuilder html, List<Experience> experiences, string lang)
        {
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <h2 class=\"section-title\">{(lang == "en" ? "Experience Journey" : "رحلة الخبرات")}</h2>");
            html.AppendLine("            <div class=\"timeline\">");

            for (int i = 0; i < experiences.Count; i++)
            {
                var exp = experiences[i];
                var alignment = i % 2 == 0 ? "left" : "right";

                html.AppendLine($"                <div class=\"timeline-item {alignment}\">");
                html.AppendLine("                    <div class=\"timeline-content\">");
                html.AppendLine($"                        <div class=\"timeline-date\">");
                html.AppendLine($"                            {exp.StartDate:MMM yyyy} - {(exp.IsCurrent ? (lang == "en" ? "Present" : "الآن") : exp.EndDate?.ToString("MMM yyyy"))}");
                html.AppendLine("                        </div>");
                html.AppendLine($"                        <h3>{(lang == "en" ? exp.TitleEn : exp.TitleAr)}</h3>");
                html.AppendLine($"                        <h4>{(lang == "en" ? exp.CompanyEn : exp.CompanyAr)}</h4>");
                html.AppendLine($"                        <p>{(lang == "en" ? exp.DescriptionEn : exp.DescriptionAr)}</p>");
                html.AppendLine("                    </div>");
                html.AppendLine("                </div>");
            }

            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }

        private void GenerateSkillsSection(StringBuilder html, List<Skill> skills, string lang)
        {
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <h2 class=\"section-title\">{(lang == "en" ? "Skills" : "المهارات")}</h2>");
            html.AppendLine("            <div class=\"row\">");

            var half = (int)Math.Ceiling(skills.Count / 2.0);

            html.AppendLine("                <div class=\"col-md-6\">");
            for (int i = 0; i < half; i++)
            {
                var skill = skills[i];
                html.AppendLine("                    <div class=\"skill-item\">");
                html.AppendLine($"                        <div class=\"skill-name\">{(lang == "en" ? skill.NameEn : skill.NameAr)}</div>");
                html.AppendLine("                        <div class=\"progress\">");
                html.AppendLine($"                            <div class=\"progress-bar\" role=\"progressbar\" style=\"width: 0%\" data-width=\"{skill.Percentage}%\"></div>");
                html.AppendLine("                        </div>");
                html.AppendLine("                    </div>");
            }
            html.AppendLine("                </div>");

            html.AppendLine("                <div class=\"col-md-6\">");
            for (int i = half; i < skills.Count; i++)
            {
                var skill = skills[i];
                html.AppendLine("                    <div class=\"skill-item\">");
                html.AppendLine($"                        <div class=\"skill-name\">{(lang == "en" ? skill.NameEn : skill.NameAr)}</div>");
                html.AppendLine("                        <div class=\"progress\">");
                html.AppendLine($"                            <div class=\"progress-bar\" role=\"progressbar\" style=\"width: 0%\" data-width=\"{skill.Percentage}%\"></div>");
                html.AppendLine("                        </div>");
                html.AppendLine("                    </div>");
            }
            html.AppendLine("                </div>");

            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }

        private void GenerateProjectsSection(StringBuilder html, IEnumerable<Project> projects, string lang)
        {
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <h2 class=\"section-title\">{(lang == "en" ? "Projects" : "المشاريع")}</h2>");
            html.AppendLine("            <div class=\"row g-4\">");

            foreach (var project in projects.Where(p => !p.IsHidden))
            {
                html.AppendLine("                <div class=\"col-md-4\">");
                html.AppendLine("                    <div class=\"project-card\">");
                html.AppendLine("                        <div class=\"project-image\">");

                if (!string.IsNullOrEmpty(project.ImagePath))
                {
                    html.AppendLine($"                            <img src=\".{project.ImagePath}\" alt=\"{project.TitleEn}\" />");
                }
                else
                {
                    html.AppendLine($"                            <i class=\"{project.IconClass}\"></i>");
                }

                html.AppendLine("                        </div>");
                html.AppendLine("                        <div class=\"project-content\">");
                html.AppendLine($"                            <h3 class=\"project-title\">{(lang == "en" ? project.TitleEn : project.TitleAr)}</h3>");
                html.AppendLine($"                            <p class=\"project-desc\">{(lang == "en" ? project.ShortDescriptionEn : project.ShortDescriptionAr)}</p>");
                html.AppendLine("                            <div>");

                foreach (var tech in project.Technologies)
                {
                    html.AppendLine($"                                <span class=\"tech-tag\">{tech.TechnologyName}</span>");
                }

                html.AppendLine("                            </div>");

                if (!string.IsNullOrEmpty(project.GithubUrl) || !string.IsNullOrEmpty(project.DemoUrl))
                {
                    html.AppendLine("                            <div class=\"mt-3\">");
                    if (!string.IsNullOrEmpty(project.GithubUrl))
                    {
                        html.AppendLine($"                                <a href=\"{project.GithubUrl}\" target=\"_blank\" class=\"btn btn-sm btn-custom me-2\">");
                        html.AppendLine("                                    <i class=\"fab fa-github\"></i> GitHub");
                        html.AppendLine("                                </a>");
                    }
                    if (!string.IsNullOrEmpty(project.DemoUrl))
                    {
                        html.AppendLine($"                                <a href=\"{project.DemoUrl}\" target=\"_blank\" class=\"btn btn-sm btn-custom-outline\">");
                        html.AppendLine("                                    <i class=\"fas fa-external-link-alt\"></i> Demo");
                        html.AppendLine("                                </a>");
                    }
                    html.AppendLine("                            </div>");
                }

                html.AppendLine("                        </div>");
                html.AppendLine("                    </div>");
                html.AppendLine("                </div>");
            }

            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }

        private void GenerateTestimonialsSection(StringBuilder html, List<Testimonial> testimonials, string lang)
        {
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <h2 class=\"section-title\">{(lang == "en" ? "Testimonials" : "آراء العملاء")}</h2>");
            html.AppendLine("            <div class=\"row\">");

            foreach (var testimonial in testimonials)
            {
                html.AppendLine("                <div class=\"col-md-4 mb-4\">");
                html.AppendLine("                    <div class=\"testimonial-card\">");

                if (!string.IsNullOrEmpty(testimonial.ClientImage))
                {
                    html.AppendLine($"                        <img src=\".{testimonial.ClientImage}\" alt=\"{testimonial.ClientName}\" class=\"client-image\" />");
                }

                html.AppendLine("                        <div class=\"rating\">");
                for (int i = 0; i < testimonial.Rating; i++)
                {
                    html.AppendLine("                            <i class=\"fas fa-star\"></i>");
                }
                html.AppendLine("                        </div>");

                html.AppendLine($"                        <p class=\"testimonial-text\">{(lang == "en" ? testimonial.TestimonialTextEn : testimonial.TestimonialTextAr)}</p>");
                html.AppendLine($"                        <h5 class=\"client-name\">{testimonial.ClientName}</h5>");
                html.AppendLine($"                        <p class=\"client-position\">{testimonial.Position}{(!string.IsNullOrEmpty(testimonial.Company) ? " at " + testimonial.Company : "")}</p>");

                html.AppendLine("                    </div>");
                html.AppendLine("                </div>");
            }

            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }

        private void GenerateContactSection(StringBuilder html, About? about, List<SocialLink> socialLinks, string lang)
        {
            html.AppendLine("        <div class=\"container\">");
            html.AppendLine($"            <h2 class=\"section-title\">{(lang == "en" ? "Get In Touch" : "تواصل معي")}</h2>");
            html.AppendLine("            <div class=\"row justify-content-center\">");
            html.AppendLine("                <div class=\"col-lg-8\">");

            // Contact Info Cards
            html.AppendLine("                    <div class=\"row\">");

            if (about != null)
            {
                // Email Card
                html.AppendLine("                        <div class=\"col-md-4 mb-4\">");
                html.AppendLine("                            <div class=\"contact-info-card\">");
                html.AppendLine("                                <div class=\"contact-icon\"><i class=\"fas fa-envelope\"></i></div>");
                html.AppendLine($"                                <h5>{(lang == "en" ? "Email" : "البريد الإلكتروني")}</h5>");
                html.AppendLine($"                                <a href=\"mailto:{about.Email}\">{about.Email}</a>");
                html.AppendLine("                            </div>");
                html.AppendLine("                        </div>");

                // Phone Card
                if (!string.IsNullOrEmpty(about.Phone))
                {
                    html.AppendLine("                        <div class=\"col-md-4 mb-4\">");
                    html.AppendLine("                            <div class=\"contact-info-card\">");
                    html.AppendLine("                                <div class=\"contact-icon\"><i class=\"fas fa-phone\"></i></div>");
                    html.AppendLine($"                                <h5>{(lang == "en" ? "Phone" : "الهاتف")}</h5>");
                    html.AppendLine($"                                <a href=\"tel:{about.Phone}\">{about.Phone}</a>");
                    html.AppendLine("                            </div>");
                    html.AppendLine("                        </div>");
                }

                // Location Card
                if (!string.IsNullOrEmpty(about.Location))
                {
                    html.AppendLine("                        <div class=\"col-md-4 mb-4\">");
                    html.AppendLine("                            <div class=\"contact-info-card\">");
                    html.AppendLine("                                <div class=\"contact-icon\"><i class=\"fas fa-map-marker-alt\"></i></div>");
                    html.AppendLine($"                                <h5>{(lang == "en" ? "Location" : "الموقع")}</h5>");
                    html.AppendLine($"                                <p>{about.Location}</p>");
                    html.AppendLine("                            </div>");
                    html.AppendLine("                        </div>");
                }
            }

            html.AppendLine("                    </div>");

            // Social Links
            if (socialLinks.Any())
            {
                html.AppendLine("                    <div class=\"social-links-section text-center my-5\">");
                html.AppendLine($"                        <h4 class=\"mb-4\">{(lang == "en" ? "Connect With Me" : "تواصل معي")}</h4>");
                html.AppendLine("                        <div class=\"social-links-grid\">");

                foreach (var link in socialLinks)
                {
                    html.AppendLine($"                            <a href=\"{link.Url}\" target=\"_blank\" class=\"social-link-card\" title=\"{link.Platform}\">");
                    html.AppendLine($"                                <i class=\"{link.IconClass}\"></i>");
                    html.AppendLine($"                                <span>{link.Platform}</span>");
                    html.AppendLine("                            </a>");
                }

                html.AppendLine("                        </div>");
                html.AppendLine("                    </div>");
            }

            html.AppendLine("                </div>");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
        }
        private string GenerateStandaloneJS(string lang)
        {
            // Return the portfolio.js content but remove contact form submission since it won't work
            var jsContent = System.IO.File.ReadAllText(Path.Combine(_env.WebRootPath, "js", "portfolio.js"));

            // Remove contact form section
            jsContent = jsContent.Replace("function initializeContactForm()", "function initializeContactForm() { return; // Disabled in static export");

            return jsContent;
        }

        private async Task CopyImages(string destPath)
        {
            var imagesDir = Path.Combine(destPath, "images");
            Directory.CreateDirectory(imagesDir);

            // Copy all images from wwwroot/images
            var sourceImagesDir = Path.Combine(_env.WebRootPath, "images");
            if (Directory.Exists(sourceImagesDir))
            {
                CopyDirectory(sourceImagesDir, imagesDir);
            }

            // Copy files (CV)
            var filesDir = Path.Combine(destPath, "files");
            var sourceFilesDir = Path.Combine(_env.WebRootPath, "files");
            if (Directory.Exists(sourceFilesDir))
            {
                CopyDirectory(sourceFilesDir, filesDir);
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                var destFile = Path.Combine(destDir, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                System.IO.File.Copy(file, destFile, true);
            }
        }

        private string GenerateReadme()
        {
            return @"# Portfolio Static Export

## About
This is a static HTML export of your portfolio website.

## Files Included
- `index.html` - Main portfolio page
- `css/portfolio.css` - Stylesheet
- `js/portfolio.js` - JavaScript functionality
- `images/` - All portfolio images
- `files/` - CV and other files

## How to Use

### Local Viewing
1. Simply open `index.html` in your web browser
2. All files are self-contained and will work offline

### Deployment Options

#### Option 1: GitHub Pages
1. Create a new repository on GitHub
2. Upload all files to the repository
3. Go to Settings > Pages
4. Select main branch as source
5. Your site will be live at `https://yourusername.github.io/repo-name`

#### Option 2: Netlify
1. Go to https://app.netlify.com
2. Drag and drop this folder to deploy
3. Your site will be live instantly with a custom URL

#### Option 3: Vercel
1. Go to https://vercel.com
2. Import this folder
3. Deploy with one click

#### Option 4: Traditional Web Hosting
1. Upload all files to your hosting provider via FTP
2. Ensure `index.html` is in the root directory
3. Access your site via your domain

## Notes
- Contact form is disabled in this static export
- Contact information is displayed instead
- All links to external resources (GitHub, LinkedIn) work normally
- Images are included in the export

## Support
For any issues or questions, please contact the original site administrator.

Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}