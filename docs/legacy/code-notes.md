## 🔧 **Fix 2: Add Preloader Background Methods to SiteSettingsController**

Add these methods to your `SiteSettingsController.cs`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UploadPreloaderBackgroundImage(IFormFile backgroundImage)
{
    if (backgroundImage != null && backgroundImage.Length > 0)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "preloader");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(backgroundImage.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await backgroundImage.CopyToAsync(fileStream);
        }

        var imageUrl = "/images/preloader/" + uniqueFileName;

        var setting = (await _settingsRepo.GetAllAsync())
            .FirstOrDefault(s => s.Key == "preloader_background_image");

        if (setting != null)
        {
            if (!string.IsNullOrEmpty(setting.ValueEn))
            {
                var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }
            setting.ValueEn = imageUrl;
            setting.ValueAr = imageUrl;
            await _settingsRepo.UpdateAsync(setting);
        }
        else
        {
            await _settingsRepo.AddAsync(new SiteSettings
            {
                Key = "preloader_background_image",
                ValueEn = imageUrl,
                ValueAr = imageUrl,
                Description = "Custom background image for preloader",
                DataType = "file"
            });
        }

        TempData["Success"] = "Preloader background uploaded successfully!";
    }

    return RedirectToAction(nameof(Index));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RemovePreloaderBackgroundImage()
{
    var setting = (await _settingsRepo.GetAllAsync())
        .FirstOrDefault(s => s.Key == "preloader_background_image");

    if (setting != null && !string.IsNullOrEmpty(setting.ValueEn))
    {
        var oldPath = Path.Combine(_env.WebRootPath, setting.ValueEn.TrimStart('/'));
        if (System.IO.File.Exists(oldPath))
            System.IO.File.Delete(oldPath);

        setting.ValueEn = "";
        setting.ValueAr = "";
        await _settingsRepo.UpdateAsync(setting);

        TempData["Success"] = "Preloader background removed successfully!";
    }

    return RedirectToAction(nameof(Index));
}
```

## 🔧 **Fix 3: Update DbInitializer with Missing Settings**

Add these to your `DbInitializer.cs`:

```csharp
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
},
```

## 🔧 **Fix 4: Update Home/Index.cshtml Preloader Section**

Replace the preloader div in your `Home/Index.cshtml`:

```cshtml
@{
    var enablePreloader = settings.GetValueOrDefault("enable_preloader", "true") == "true";
    var preloaderStyle = settings.GetValueOrDefault("preloader_style", "spinner");
    var preloaderColor = settings.GetValueOrDefault("preloader_color", "#667eea");
    var preloaderDuration = settings.GetValueOrDefault("preloader_duration", "1000");
    var preloaderLogo = settings.GetValueOrDefault("preloader_logo", "");
    var preloaderBg = settings.GetValueOrDefault("preloader_background", "gradient");
    var preloaderBgImage = settings.GetValueOrDefault("preloader_background_image", "");
    
    string preloaderBackground = "";
    if (preloaderBg == "image" && !string.IsNullOrEmpty(preloaderBgImage))
    {
        preloaderBackground = $"background-image: url('{preloaderBgImage}'); background-size: cover; background-position: center;";
    }
    else if (preloaderBg == "solid")
    {
        preloaderBackground = $"background: {primaryColor};";
    }
    else
    {
        preloaderBackground = $"background: linear-gradient(135deg, {primaryColor} 0%, {secondaryColor} 100%);";
    }
}

<!-- Preloader -->
@if (enablePreloader)
{
    <div id="preloader" data-duration="@preloaderDuration" style="@preloaderBackground">
        @if (preloaderStyle == "spinner")
        {
            <div class="spinner" style="border-color: rgba(255,255,255,0.3); border-top-color: @preloaderColor;"></div>
        }
        else if (preloaderStyle == "dots")
        {
            <div class="dots-loader">
                <div style="background: @preloaderColor;"></div>
                <div style="background: @preloaderColor;"></div>
                <div style="background: @preloaderColor;"></div>
            </div>
        }
        else if (preloaderStyle == "pulse")
        {
            <div class="pulse-loader" style="background: @preloaderColor;"></div>
        }
        else if (preloaderStyle == "bars")
        {
            <div class="bars-loader">
                <div style="background: @preloaderColor;"></div>
                <div style="background: @preloaderColor;"></div>
                <div style="background: @preloaderColor;"></div>
                <div style="background: @preloaderColor;"></div>
                <div style="background: @preloaderColor;"></div>
            </div>
        }
        else if (preloaderStyle == "circle")
        {
            <div class="circle-loader" style="border-top-color: @preloaderColor; border-right-color: @preloaderColor;"></div>
        }
        else if (preloaderStyle == "logo" && !string.IsNullOrEmpty(preloaderLogo))
        {
            <div class="logo-loader">
                <img src="@preloaderLogo" alt="Loading..." style="max-width: 150px; animation: pulse 1.5s ease-in-out infinite;" />
            </div>
        }
    </div>
}
```

## 🔧 **Fix 5: Fix Image Paths in Export (ExportController.cs)**

Update the `GenerateStaticHTML` method to fix image paths:

```csharp
private async Task<string> GenerateStaticHTML(string lang)
{
    // ... existing code ...
    
    // Update the image generation part
    if (!string.IsNullOrEmpty(project.ImagePath))
    {
        // Convert absolute path to relative
        var relativePath = project.ImagePath.TrimStart('/');
        html.AppendLine($"                            <img src=\"{relativePath}\" alt=\"{project.TitleEn}\" />");
    }
    
    // Same for profile images, testimonial images, etc.
    if (!string.IsNullOrEmpty(about?.ProfileImage))
    {
        var relativePath = about.ProfileImage.TrimStart('/');
        html.AppendLine($"                            <img src=\"{relativePath}\" alt=\"{about.NameEn}\" class=\"rounded-circle\" style=\"width: 200px; height: 200px; object-fit: cover; border: 5px solid var(--primary-color);\">");
    }
    
    // For CV file
    if (!string.IsNullOrEmpty(about?.CVFilePath))
    {
        var relativePath = about.CVFilePath.TrimStart('/');
        html.AppendLine($"                    <a href=\"{relativePath}\" download class=\"btn btn-custom-outline\">");
        html.AppendLine($"                        <i class=\"fas fa-download me-2\"></i> {(lang == "en" ? "Download CV" : "تحميل السيرة الذاتية")}");
        html.AppendLine("                    </a>");
    }
}
```

## 🔧 **Fix 6: Update Contact Section with Social Links**

**Portfolio.Web/Views/Home/_ContactSection.cshtml** (Complete Rewrite)

```cshtml
@model Portfolio.Core.DTOs.HomeViewModel
@{
    var lang = Model.CurrentLanguage;
}

<div class="container">
    <h2 class="section-title">@(lang == "en" ? "Get In Touch" : "تواصل معي")</h2>
    <div class="row justify-content-center">
        <div class="col-lg-8">
            <div class="row">
                <!-- Contact Info Cards -->
                @if (Model.About != null)
                {
                    <div class="col-md-4 mb-4">
                        <div class="contact-info-card">
                            <div class="contact-icon">
                                <i class="fas fa-envelope"></i>
                            </div>
                            <h5>@(lang == "en" ? "Email" : "البريد الإلكتروني")</h5>
                            <a href="mailto:@Model.About.Email">@Model.About.Email</a>
                        </div>
                    </div>

                    @if (!string.IsNullOrEmpty(Model.About.Phone))
                    {
                        <div class="col-md-4 mb-4">
                            <div class="contact-info-card">
                                <div class="contact-icon">
                                    <i class="fas fa-phone"></i>
                                </div>
                                <h5>@(lang == "en" ? "Phone" : "الهاتف")</h5>
                                <a href="tel:@Model.About.Phone">@Model.About.Phone</a>
                            </div>
                        </div>
                    }

                    @if (!string.IsNullOrEmpty(Model.About.Location))
                    {
                        <div class="col-md-4 mb-4">
                            <div class="contact-info-card">
                                <div class="contact-icon">
                                    <i class="fas fa-map-marker-alt"></i>
                                </div>
                                <h5>@(lang == "en" ? "Location" : "الموقع")</h5>
                                <p>@Model.About.Location</p>
                            </div>
                        </div>
                    }
                }
            </div>

            <!-- Social Links -->
            @if (Model.SocialLinks.Any())
            {
                <div class="social-links-section text-center my-5">
                    <h4 class="mb-4">@(lang == "en" ? "Connect With Me" : "تواصل معي")</h4>
                    <div class="social-links-grid">
                        @foreach (var link in Model.SocialLinks)
                        {
                            <a href="@link.Url" target="_blank" class="social-link-card" title="@link.Platform">
                                <i class="@link.IconClass"></i>
                                <span>@link.Platform</span>
                            </a>
                        }
                    </div>
                </div>
            }

            <!-- Contact Form -->
            <div class="contact-form mt-5">
                <h4 class="text-center mb-4">@(lang == "en" ? "Send Me a Message" : "أرسل لي رسالة")</h4>
                <form id="contactForm">
                    <div class="row">
                        <div class="col-md-6">
                            <input type="text" class="form-control mb-3" name="Name" 
                                   placeholder="@(lang == "en" ? "Your Name" : "اسمك")" required>
                        </div>
                        <div class="col-md-6">
                            <input type="email" class="form-control mb-3" name="Email" 
                                   placeholder="@(lang == "en" ? "Your Email" : "بريدك الإلكتروني")" required>
                        </div>
                    </div>
                    <textarea class="form-control mb-3" name="Message" rows="5" 
                              placeholder="@(lang == "en" ? "Your Message" : "رسالتك")" required></textarea>
                    <button type="submit" class="btn btn-custom w-100">
                        <i class="fas fa-paper-plane me-2"></i>
                        @(lang == "en" ? "Send Message" : "إرسال الرسالة")
                    </button>
                </form>
            </div>
        </div>
    </div>
</div>
```

## 🔧 **Fix 7: Add Contact Section Styles to portfolio.css**

```css
/* Contact Section Styles */
.contact-info-card {
    background: rgba(255, 255, 255, 0.05);
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 15px;
    padding: 30px 20px;
    text-align: center;
    transition: all 0.3s ease;
    height: 100%;
}

.contact-info-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
    border-color: var(--primary-color);
}

.contact-icon {
    width: 70px;
    height: 70px;
    margin: 0 auto 20px;
    background: linear-gradient(135deg, var(--primary-color), var(--secondary-color));
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 30px;
    color: white;
}

.contact-info-card h5 {
    font-size: 18px;
    font-weight: 600;
    margin-bottom: 10px;
    color: white;
}

.contact-info-card a,
.contact-info-card p {
    color: rgba(255, 255, 255, 0.8);
    text-decoration: none;
    margin: 0;
}

.contact-info-card a:hover {
    color: var(--primary-color);
}

/* Social Links Grid */
.social-links-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 20px;
    max-width: 800px;
    margin: 0 auto;
}

.social-link-card {
    background: rgba(255, 255, 255, 0.05);
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 15px;
    padding: 25px 15px;
    text-align: center;
    text-decoration: none;
    color: white;
    transition: all 0.3s ease;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
}

.social-link-card:hover {
    transform: translateY(-5px);
    background: var(--primary-color);
    border-color: var(--primary-color);
    box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
}

.social-link-card i {
    font-size: 32px;
}

.social-link-card span {
    font-size: 14px;
    font-weight: 500;
}

/* Light Mode Contact */
body.light-mode .contact-info-card,
body.light-mode .social-link-card {
    background: white;
    border-color: #e0e0e0;
    color: #333;
}

body.light-mode .contact-info-card h5,
body.light-mode .social-link-card {
    color: #333;
}

body.light-mode .contact-info-card a,
body.light-mode .contact-info-card p {
    color: #666;
}

body.light-mode .social-link-card:hover {
    color: white;
}
```

## 🔧 **Fix 8: Update Export to Include Contact with Social Links**

Update the `GenerateContactSection` method in `ExportController.cs`:

```csharp
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
```

## 🔧 **Fix 9: Fix Project Click to Open GitHub**

Update `_ProjectsSection.cshtml`:

```cshtml
@foreach (var project in Model.Projects)
{
    <div class="col-md-4 project-item @(project.IsHidden ? "hidden-project" : "")" 
         data-category="@project.Category" 
         style="display: @(project.IsHidden ? "none" : "block")">
        <div class="project-card">
            <div class="project-image">
                @if (!string.IsNullOrEmpty(project.ImagePath))
                {
                    <img src="@project.ImagePath" alt="@project.TitleEn" />
                }
                else
                {
                    <i class="@project.IconClass"></i>
                }
            </div>
            <div class="project-content">
                <h3 class="project-title">@(lang == "en" ? project.TitleEn : project.TitleAr)</h3>
                <p class="project-desc">@(lang == "en" ? project.ShortDescriptionEn : project.ShortDescriptionAr)</p>
                <div class="mb-3">
                    @foreach (var tech in project.Technologies)
                    {
                        <span class="tech-tag">@tech.TechnologyName</span>
                    }
                </div>
                <!-- Project Links -->
                <div class="project-links">
                    @if (!string.IsNullOrEmpty(project.GithubUrl))
                    {
                        <a href="@project.GithubUrl" target="_blank" class="btn btn-sm btn-custom me-2" 
                           onclick="event.stopPropagation(); trackProjectClick(@project.Id, '@project.GithubUrl')">
                            <i class="fab fa-github"></i> GitHub
                        </a>
                    }
                    @if (!string.IsNullOrEmpty(project.DemoUrl))
                    {
                        <a href="@project.DemoUrl" target="_blank" class="btn btn-sm btn-custom-outline"
                           onclick="event.stopPropagation(); trackProjectClick(@project.Id, '@project.DemoUrl')">
                            <i class="fas fa-external-link-alt"></i> Demo
                        </a>
                    }
                </div>
            </div>
        </div>
    </div>
}
```

Update `portfolio.js` `trackProjectClick` function:

```javascript
async function trackProjectClick(projectId, url) {
    try {
        await fetch('/Home/TrackProjectClick', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ projectId })
        });
    } catch (error) {
        console.error('Error tracking click:', error);
    }
    
    // Return true to allow link to proceed
    return true;
}
```

Add this CSS for project links:

```css
.project-links {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
}

.project-links .btn {
    flex: 1;
    min-width: 100px;
}
```

## 🧪 **Testing Checklist**

1. **Site Settings:**
   - [ ] Page loads without errors
   - [ ] Can change preloader background (gradient/solid/image)
   - [ ] Can upload preloader logo
   - [ ] Can upload preloader background image
   - [ ] Preview updates correctly

2. **Export:**
   - [ ] Images load in exported HTML
   - [ ] All paths are relative
   - [ ] Contact section shows with social links
   - [ ] No error messages in contact form

3. **Contact Section:**
   - [ ] Shows contact info cards
   - [ ] Social links display in grid
   - [ ] Form works (tracks but shows friendly message in static export)
   - [ ] Responsive on mobile

4. **Projects:**
   - [ ] GitHub button opens GitHub in new tab
   - [ ] Demo button opens demo in new tab
   - [ ] Click tracking works
   - [ ] No modal appears

Run migration:
```bash
dotnet ef migrations add AddPreloaderBackgroundSettings --project ../Portfolio.Infrastructure
dotnet ef database update
```

Everything should now work perfectly! 🚀