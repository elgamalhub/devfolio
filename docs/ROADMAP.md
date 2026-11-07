# Roadmap

This roadmap reflects the portfolio as it stands after the public-site and admin rebuilds.
Completed work is kept for context; planned items are ordered by expected impact.

## Completed

### Public site

- [x] Data-driven, bilingual (English / Arabic) sections with RTL support
- [x] Dark / light theme with a persisted visitor preference
- [x] Dynamic branding colours, site titles, and SEO metadata from site settings
- [x] Hero typing animation, particles, custom background, and CV download
- [x] About, skills, experience timeline, projects, certificates, testimonials, and contact sections
- [x] Project category filters, technology tags, demo/GitHub links, and click analytics
- [x] Blog with search, tag filtering, reading time, related posts, social sharing, and view counts
- [x] Per-page titles and descriptions, Open Graph, Twitter cards, canonical URLs, and JSON-LD
- [x] `sitemap.xml`, `robots.txt`, and an RSS feed
- [x] Accessible navigation, skip link, focus states, and reduced-motion support
- [x] Local Bootstrap, Font Awesome, and site CSS/JS with no CDN dependency

### Admin and content management

- [x] Responsive admin shell with collapsible sidebar and mobile navigation
- [x] Dashboard with project, blog, message, and page-view statistics
- [x] CRUD for about, typing titles, skills, experience, project categories, projects, testimonials, certificates, and blog posts
- [x] Bilingual site settings (English / Arabic) with safe image upload and removal
- [x] Section manager with ordering, visibility, navbar flags, and custom HTML sections
- [x] Contact inbox with read state, notes, and deletion
- [x] Static portfolio export as a self-contained ZIP
- [x] CV-backed starter content seeded on first run

### Security and operations

- [x] Anti-forgery validation on every state-changing endpoint
- [x] Config-driven admin seeding with no hard-coded credentials
- [x] Upload validation by extension, MIME type, size, and magic bytes
- [x] Security headers, HTTPS redirection, HSTS, and secure cookie defaults
- [x] Login lockout and rate limiting on public write endpoints
- [x] Page-view de-duplication with crawler and administrator exclusion
- [x] GitHub Actions build and publish workflow

## Planned

### Near term

- [ ] Automated tests for controllers, upload validation, and seeding
- [ ] Deployment guide and environment-specific configuration samples
- [ ] Image optimisation and responsive image sizes
- [ ] Response caching for public pages

### Medium term

- [ ] Blog comments with moderation and spam protection
- [ ] Newsletter subscription and email notifications
- [ ] Media library with reusable uploads
- [ ] Project case-study pages
- [ ] Richer admin analytics and exportable reports

### Long term

- [ ] Multi-admin roles and permissions
- [ ] Public REST API for content
- [ ] PWA support with offline mode
- [ ] Optional integrations (email, analytics, social auto-posting)

## Notes

- The application targets SQL Server only; there is no SQLite fallback.
- The admin area is intentionally single-role (`Admin`) for now.
- This document is updated as features are completed, not as a promise of delivery dates.
