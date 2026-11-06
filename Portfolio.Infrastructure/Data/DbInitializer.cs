using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Create Admin role
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Admin user
            if (await userManager.FindByEmailAsync("admin@portfolio.com") == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin@portfolio.com",
                    Email = "admin@portfolio.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed initial data if database is empty
            if (!await context.Abouts.AnyAsync())
            {
                var about = new About
                {
                    NameEn = "Ahmed Elgamal",
                    NameAr = "أحمد الجمال",
                    DescriptionEn = "Passionate .NET Developer with comprehensive hands-on experience.",
                    DescriptionAr = "مطور .NET شغوف ذو خبرة عملية شاملة.",
                    Email = "ahmed.elgamal.dev22@gmail.com",
                    Phone = "+201022172951",
                    Location = "Cairo, Egypt",
                    GitHubUrl = "https://github.com/elgamalhub",
                    LinkedInUrl = "https://www.linkedin.com/in/ahmed-elgamal1",
                    UpdatedAt = DateTime.UtcNow
                };
                context.Abouts.Add(about);
            }

            if (!await context.TypingTitles.AnyAsync())
            {
                var titles = new[]
                {
                    new TypingTitle { TitleEn = "Full Stack Developer", TitleAr = "مطور متكامل", Order = 1, IsActive = true },
                    new TypingTitle { TitleEn = "Backend .NET Developer", TitleAr = "مطور .NET خلفي", Order = 2, IsActive = true },
                    new TypingTitle { TitleEn = "Angular Developer", TitleAr = "مطور Angular", Order = 3, IsActive = true },
                    new TypingTitle { TitleEn = "ASP.NET Core Specialist", TitleAr = "متخصص ASP.NET Core", Order = 4, IsActive = true }
                };
                context.TypingTitles.AddRange(titles);
            }

            if (!await context.Skills.AnyAsync())
            {
                var skills = new[]
                {
                    new Skill { NameEn = "C# / .NET Core", NameAr = "C# / .NET Core", Percentage = 90, Order = 1 },
                    new Skill { NameEn = "ASP.NET Core (API & MVC)", NameAr = "ASP.NET Core (API & MVC)", Percentage = 85, Order = 2 },
                    new Skill { NameEn = "Entity Framework Core", NameAr = "Entity Framework Core", Percentage = 85, Order = 3 },
                    new Skill { NameEn = "SQL Server / T-SQL", NameAr = "SQL Server / T-SQL", Percentage = 80, Order = 4 },
                    new Skill { NameEn = "Angular / TypeScript", NameAr = "Angular / TypeScript", Percentage = 80, Order = 5 },
                    new Skill { NameEn = "HTML / CSS / Bootstrap", NameAr = "HTML / CSS / Bootstrap", Percentage = 90, Order = 6 }
                };
                context.Skills.AddRange(skills);
            }

            if (!await context.SocialLinks.AnyAsync())
            {
                var socialLinks = new[]
                {
                    new SocialLink { Platform = "GitHub", Url = "https://github.com/elgamalhub", IconClass = "fab fa-github", Order = 1 },
                    new SocialLink { Platform = "LinkedIn", Url = "https://www.linkedin.com/in/ahmed-elgamal1", IconClass = "fab fa-linkedin", Order = 2 },
                    new SocialLink { Platform = "Email", Url = "mailto:ahmed.elgamal.dev22@gmail.com", IconClass = "fas fa-envelope", Order = 3 },
                    new SocialLink { Platform = "Phone", Url = "tel:+201022172951", IconClass = "fas fa-phone", Order = 4 }
                };
                context.SocialLinks.AddRange(socialLinks);
            }
            // Add this in the Initialize method after existing seeding

            // Seed Site Settings
            if (!await context.SiteSettings.AnyAsync())
            {
                var settings = new[]
                {
        new SiteSettings
        {
            Key = "default_language",
            ValueEn = "en",
            ValueAr = "en",
            Description = "Default site language (en or ar)",
            DataType = "select"
        },
        new SiteSettings
        {
            Key = "default_theme",
            ValueEn = "dark",
            ValueAr = "dark",
            Description = "Default theme mode (dark or light)",
            DataType = "select"
        },
        new SiteSettings
        {
            Key = "site_title_en",
            ValueEn = "My Portfolio",
            ValueAr = "My Portfolio",
            Description = "Site title in English",
            DataType = "text"
        },
        new SiteSettings
        {
            Key = "site_title_ar",
            ValueEn = "محفظتي",
            ValueAr = "محفظتي",
            Description = "Site title in Arabic",
            DataType = "text"
        },
        new SiteSettings
        {
            Key = "hero_background_image",
            ValueEn = "",
            ValueAr = "",
            Description = "Hero section background image URL (leave empty for gradient)",
            DataType = "file"
        },
        new SiteSettings
        {
            Key = "primary_color",
            ValueEn = "#667eea",
            ValueAr = "#667eea",
            Description = "Primary brand color",
            DataType = "color"
        },
        new SiteSettings
        {
            Key = "secondary_color",
            ValueEn = "#764ba2",
            ValueAr = "#764ba2",
            Description = "Secondary brand color",
            DataType = "color"
        },
        new SiteSettings
        {
            Key = "enable_particles",
            ValueEn = "true",
            ValueAr = "true",
            Description = "Enable particle animation",
            DataType = "boolean"
        },
        new SiteSettings
        {
            Key = "show_back_to_top",
            ValueEn = "true",
            ValueAr = "true",
            Description = "Show back to top button",
            DataType = "boolean"
        },
        new SiteSettings
        {
            Key = "meta_description_en",
            ValueEn = "Professional Portfolio Website",
            ValueAr = "Professional Portfolio Website",
            Description = "SEO meta description (English)",
            DataType = "textarea"
        },
        new SiteSettings
        {
            Key = "meta_description_ar",
            ValueEn = "موقع محفظة احترافي",
            ValueAr = "موقع محفظة احترافي",
            Description = "SEO meta description (Arabic)",
            DataType = "textarea"
        },
        // Add these to your existing settings array in DbInitializer.cs
        new SiteSettings
        {
            Key = "enable_preloader",
            ValueEn = "true",
            ValueAr = "true",
            Description = "Enable/disable preloader animation",
            DataType = "boolean"
        },
        new SiteSettings
        {
            Key = "preloader_style",
            ValueEn = "spinner",
            ValueAr = "spinner",
            Description = "Preloader animation style (spinner, dots, pulse, bars, circle)",
            DataType = "select"
        },
        new SiteSettings
        {
            Key = "preloader_color",
            ValueEn = "#667eea",
            ValueAr = "#667eea",
            Description = "Preloader color",
            DataType = "color"
        },
        new SiteSettings
        {
            Key = "preloader_duration",
            ValueEn = "1000",
            ValueAr = "1000",
            Description = "Preloader display duration in milliseconds",
            DataType = "number"
        },
        new SiteSettings
        {
            Key = "preloader_logo",
            ValueEn = "",
            ValueAr = "",
            Description = "Custom logo for preloader (optional)",
            DataType = "file"
        },
        new SiteSettings
        {
            Key = "preloader_background",
            ValueEn = "gradient",
            ValueAr = "gradient",
            Description = "Preloader background style (gradient, solid, image)",
            DataType = "select"
        },
        new SiteSettings
        {
            Key = "preloader_background_image",
            ValueEn = "",
            ValueAr = "",
            Description = "Custom background image for preloader",
            DataType = "file"
        },
        new SiteSettings
        {
            Key = "hero_background_image",
            ValueEn = "",
            ValueAr = "",
            Description = "Hero section background image",
            DataType = "file"
        }
    };
                context.SiteSettings.AddRange(settings);
            }

            // Seed Default Sections
            if (!await context.Sections.AnyAsync())
            {
                var sections = new[]
                {
        new Section
        {
            NameEn = "Home",
            NameAr = "الرئيسية",
            Identifier = "home",
            IconClass = "fas fa-home",
            Order = 1,
            IsVisible = true,
            IsBuiltIn = true,
            ShowInNavbar = true
        },
        new Section
        {
            NameEn = "About",
            NameAr = "عني",
            Identifier = "about",
            IconClass = "fas fa-user",
            Order = 2,
            IsVisible = true,
            IsBuiltIn = true,
            ShowInNavbar = true
        },
        new Section
        {
            NameEn = "Experience",
            NameAr = "الخبرات",
            Identifier = "experience",
            IconClass = "fas fa-briefcase",
            Order = 3,
            IsVisible = true,
            IsBuiltIn = true,
            ShowInNavbar = true
        },
        new Section
        {
            NameEn = "Skills",
            NameAr = "المهارات",
            Identifier = "skills",
            IconClass = "fas fa-code",
            Order = 4,
            IsVisible = true,
            IsBuiltIn = true,
            ShowInNavbar = true
        },
        new Section
        {
            NameEn = "Projects",
            NameAr = "المشاريع",
            Identifier = "projects",
            IconClass = "fas fa-project-diagram",
            Order = 5,
            IsVisible = true,
            IsBuiltIn = true,
            ShowInNavbar = true
        },
        new Section
        {
            NameEn = "Blog",
            NameAr = "المدونة",
            Identifier = "blog",
            IconClass = "fas fa-blog",
            Order = 6,
            IsVisible = true,
            IsBuiltIn = false,
            ShowInNavbar = true
        },
        new Section
        {
            NameEn = "Testimonials",
            NameAr = "آراء العملاء",
            Identifier = "testimonials",
            IconClass = "fas fa-comments",
            Order = 7,
            IsVisible = true,
            IsBuiltIn = false,
            ShowInNavbar = false
        },
        new Section
        {
            NameEn = "Contact",
            NameAr = "تواصل",
            Identifier = "contact",
            IconClass = "fas fa-envelope",
            Order = 8,
            IsVisible = true,
            IsBuiltIn = true,
            ShowInNavbar = true
        }
    };
                context.Sections.AddRange(sections);
            }
            // Seed Project Categories
            if (!await context.ProjectCategories.AnyAsync())
            {
                var categories = new[]
                {
        new ProjectCategory
        {
            NameEn = "Web Applications",
            NameAr = "تطبيقات ويب",
            Slug = "web",
            IconClass = "fas fa-globe",
            Order = 1,
            IsActive = true
        },
        new ProjectCategory
        {
            NameEn = "Desktop Applications",
            NameAr = "تطبيقات سطح المكتب",
            Slug = "desktop",
            IconClass = "fas fa-desktop",
            Order = 2,
            IsActive = true
        },
        new ProjectCategory
        {
            NameEn = "Other Projects",
            NameAr = "مشاريع أخرى",
            Slug = "other",
            IconClass = "fas fa-folder",
            Order = 4,
            IsActive = true
        }
    };
                context.ProjectCategories.AddRange(categories);
            }

            await context.SaveChangesAsync();
        }
    }
}
