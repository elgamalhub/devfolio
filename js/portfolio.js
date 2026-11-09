(function () {
    'use strict';

    const config = window.portfolioConfig || {};
    const messages = config.messages || {};
    const lang = config.lang === 'ar' ? 'ar' : 'en';
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    const getToken = () => {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    };

    /* Preloader */
    const preloader = document.getElementById('preloader');
    if (preloader) {
        const duration = parseInt(preloader.getAttribute('data-duration') || '600', 10);
        const hidePreloader = () => preloader.classList.add('hidden');
        window.addEventListener('load', () => window.setTimeout(hidePreloader, Number.isFinite(duration) ? duration : 600));
        // Safety net: never trap the page behind the preloader.
        window.setTimeout(hidePreloader, 4000);
    }

    /* Theme toggle */
    const themeToggle = document.getElementById('themeToggle');
    const applyTheme = (theme) => {
        const isLight = theme === 'light';
        document.body.classList.toggle('light-mode', isLight);
        document.body.setAttribute('data-theme', isLight ? 'light' : 'dark');

        if (themeToggle) {
            const icon = themeToggle.querySelector('i');
            if (icon) {
                icon.className = isLight ? 'fas fa-sun' : 'fas fa-moon';
            }
            themeToggle.setAttribute('aria-pressed', isLight ? 'true' : 'false');
        }
    };

    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const nextTheme = document.body.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
            applyTheme(nextTheme);

            // Persist the choice server-side; failures keep the in-page theme.
            fetch('/Home/SetTheme', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getToken()
                },
                body: JSON.stringify({ theme: nextTheme })
            }).catch(() => { /* theme still applies for this page */ });
        });
    }

    /* Scroll progress, back to top and sticky navbar */
    const scrollProgress = document.getElementById('scrollProgress');
    const backToTop = document.getElementById('backToTop');
    const navbar = document.querySelector('.navbar');
    let scrollTicking = false;

    const onScroll = () => {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;

        if (scrollProgress) {
            const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
            scrollProgress.style.width = (height > 0 ? (scrollTop / height) * 100 : 0) + '%';
        }

        if (backToTop) {
            backToTop.classList.toggle('show', scrollTop > 300);
        }

        if (navbar) {
            navbar.classList.toggle('scrolled', scrollTop > 50);
        }

        scrollTicking = false;
    };

    window.addEventListener('scroll', () => {
        if (!scrollTicking) {
            scrollTicking = true;
            window.requestAnimationFrame(onScroll);
        }
    }, { passive: true });
    onScroll();

    if (backToTop) {
        backToTop.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: prefersReducedMotion ? 'auto' : 'smooth' });
        });
    }

    /* Smooth scrolling for same-page anchors */
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener('click', (event) => {
            const href = anchor.getAttribute('href');
            if (!href || href === '#') {
                return;
            }

            const target = document.querySelector(href);
            if (!target) {
                return;
            }

            event.preventDefault();
            target.scrollIntoView({ behavior: prefersReducedMotion ? 'auto' : 'smooth', block: 'start' });
            history.replaceState(null, '', href);

            const navbarCollapse = document.getElementById('mainNavbar');
            if (navbarCollapse && navbarCollapse.classList.contains('show') && window.bootstrap) {
                window.bootstrap.Collapse.getOrCreateInstance(navbarCollapse).hide();
            }
        });
    });

    /* Typing animation */
    const typingElement = document.getElementById('typingText');
    const typingTitles = Array.isArray(config.typingTitles)
        ? config.typingTitles.filter((title) => typeof title === 'string' && title.trim().length > 0)
        : [];

    if (typingElement) {
        if (typingTitles.length === 0 || prefersReducedMotion) {
            typingElement.textContent = typingTitles[0] || '';
        } else {
            let textIndex = 0;
            let charIndex = 0;
            let deleting = false;

            const tick = () => {
                const current = typingTitles[textIndex] || '';
                charIndex = deleting ? charIndex - 1 : charIndex + 1;
                typingElement.textContent = current.substring(0, Math.max(charIndex, 0));

                let delay = deleting ? 45 : 95;

                if (!deleting && charIndex >= current.length) {
                    deleting = true;
                    delay = 1800;
                } else if (deleting && charIndex <= 0) {
                    deleting = false;
                    textIndex = (textIndex + 1) % typingTitles.length;
                    delay = 400;
                }

                window.setTimeout(tick, delay);
            };

            window.setTimeout(tick, 400);
        }
    }

    /* Animate skill bars when they enter the viewport */
    const skillsSection = document.querySelector('#skills');
    if (skillsSection && 'IntersectionObserver' in window) {
        const skillsObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) {
                    return;
                }

                entry.target.querySelectorAll('.progress-bar[data-width]').forEach((bar) => {
                    bar.style.width = bar.getAttribute('data-width') || '0%';
                });
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.25 });

        skillsObserver.observe(skillsSection);
    } else if (skillsSection) {
        skillsSection.querySelectorAll('.progress-bar[data-width]').forEach((bar) => {
            bar.style.width = bar.getAttribute('data-width') || '0%';
        });
    }

    /* Project filtering and "show more" */
    const filterButtons = Array.from(document.querySelectorAll('.filter-btn'));
    const projectItems = Array.from(document.querySelectorAll('.project-item'));
    const showMoreButton = document.getElementById('showMoreBtn');
    const showMoreText = document.getElementById('showMoreText');
    const showMoreIcon = document.getElementById('showMoreIcon');
    let currentFilter = 'all';
    let showingAll = false;

    const matchesFilter = (item) => {
        const category = item.getAttribute('data-category');
        return currentFilter === 'all' || category === currentFilter;
    };

    const updateProjects = () => {
        projectItems.forEach((item) => {
            const hidden = item.classList.contains('hidden-project');
            const visible = matchesFilter(item) && (!hidden || showingAll);
            item.style.display = visible ? 'block' : 'none';
        });

        if (!showMoreButton) {
            return;
        }

        const hiddenMatches = projectItems.filter(
            (item) => matchesFilter(item) && item.classList.contains('hidden-project')
        ).length;

        showMoreButton.style.display = hiddenMatches > 0 ? 'inline-block' : 'none';

        if (showMoreText) {
            showMoreText.textContent = showingAll
                ? (messages.showLess || 'Show fewer projects')
                : (messages.showMore || 'Show more projects');
        }

        if (showMoreIcon) {
            showMoreIcon.className = showingAll ? 'fas fa-chevron-up ms-2' : 'fas fa-chevron-down ms-2';
        }
    };

    filterButtons.forEach((button) => {
        button.addEventListener('click', () => {
            filterButtons.forEach((item) => item.classList.remove('active'));
            button.classList.add('active');
            currentFilter = button.getAttribute('data-filter') || 'all';
            showingAll = false;
            updateProjects();
        });
    });

    if (showMoreButton) {
        showMoreButton.addEventListener('click', () => {
            showingAll = !showingAll;
            updateProjects();
        });
    }

    if (projectItems.length > 0) {
        updateProjects();
    }

    /* Contact form */
    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        const statusElement = document.getElementById('contactFormStatus');
        const submitButton = document.getElementById('contactSubmit');
        const originalButtonHtml = submitButton ? submitButton.innerHTML : '';

        const setStatus = (message, type) => {
            if (!statusElement) {
                return;
            }

            statusElement.textContent = message;
            statusElement.classList.remove('is-success', 'is-error');
            statusElement.classList.add(type === 'success' ? 'is-success' : 'is-error');
        };

        contactForm.addEventListener('submit', async (event) => {
            event.preventDefault();

            if (!contactForm.reportValidity()) {
                return;
            }

            const formData = new FormData(contactForm);
            const payload = {
                Name: formData.get('Name'),
                Email: formData.get('Email'),
                Message: formData.get('Message')
            };

            if (submitButton) {
                submitButton.disabled = true;
            }
            setStatus(messages.sending || 'Sending...', 'success');

            try {
                const response = await fetch('/Home/SendMessage', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': getToken()
                    },
                    body: JSON.stringify(payload)
                });

                const result = await response.json().catch(() => ({}));

                if (response.ok && result.success) {
                    setStatus(messages.sent || 'Message sent successfully!', 'success');
                    contactForm.reset();
                } else if (response.status === 429) {
                    setStatus(messages.rateLimited || 'Too many messages. Please try again later.', 'error');
                } else {
                    setStatus(result.message || messages.failed || 'Failed to send message.', 'error');
                }
            } catch (error) {
                console.error('Contact form error:', error);
                setStatus(messages.error || 'An error occurred. Please try again.', 'error');
            } finally {
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.innerHTML = originalButtonHtml;
                }
            }
        });
    }

    /* Copy-to-clipboard buttons (article share) */
    document.querySelectorAll('[data-copy-url]').forEach((button) => {
        button.addEventListener('click', async () => {
            const url = button.getAttribute('data-copy-url');
            if (!url) {
                return;
            }

            try {
                if (navigator.clipboard && window.isSecureContext) {
                    await navigator.clipboard.writeText(url);
                } else {
                    const helper = document.createElement('textarea');
                    helper.value = url;
                    helper.setAttribute('readonly', '');
                    helper.style.position = 'absolute';
                    helper.style.left = '-9999px';
                    document.body.appendChild(helper);
                    helper.select();
                    document.execCommand('copy');
                    document.body.removeChild(helper);
                }

                const icon = button.querySelector('i');
                const originalClass = icon ? icon.className : '';
                if (icon) {
                    icon.className = 'fas fa-check';
                }
                button.classList.add('copied');
                button.setAttribute('title', messages.copied || 'Link copied');

                window.setTimeout(() => {
                    if (icon) {
                        icon.className = originalClass;
                    }
                    button.classList.remove('copied');
                }, 2000);
            } catch (error) {
                console.error('Copy link failed:', error);
            }
        });
    });

    /* Project link analytics (does not block navigation) */
    document.querySelectorAll('[data-track-project]').forEach((link) => {
        link.addEventListener('click', () => {
            const projectId = parseInt(link.getAttribute('data-track-project') || '', 10);
            if (!Number.isFinite(projectId)) {
                return;
            }

            fetch('/Home/TrackProjectClick', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getToken()
                },
                body: JSON.stringify({ projectId: projectId }),
                keepalive: true
            }).catch(() => { /* analytics is best-effort */ });
        });
    });

    /* Close the mobile navbar after following an off-page link */
    const navbarCollapse = document.getElementById('mainNavbar');
    if (navbarCollapse) {
        navbarCollapse.querySelectorAll('a.nav-link').forEach((link) => {
            link.addEventListener('click', () => {
                if (navbarCollapse.classList.contains('show') && window.bootstrap) {
                    window.bootstrap.Collapse.getOrCreateInstance(navbarCollapse).hide();
                }
            });
        });
    }
})();
