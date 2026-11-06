// Preloader - use dynamic duration from settings
window.addEventListener('load', function () {
    const preloader = document.getElementById('preloader');
    if (preloader) {
        const duration = parseInt(preloader.getAttribute('data-duration') || '1000');
        setTimeout(() => {
            preloader.classList.add('hidden');
        }, duration);
    }
});
// Theme initialization on page load
document.addEventListener('DOMContentLoaded', function () {
    // Get theme from body attribute (set by server)
    const currentTheme = document.body.getAttribute('data-theme');

    // Apply theme class
    if (currentTheme === 'light') {
        document.body.classList.add('light-mode');
    }

    // Update theme toggle icon
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        const icon = themeToggle.querySelector('i');
        if (icon) {
            icon.className = currentTheme === 'light' ? 'fas fa-sun' : 'fas fa-moon';
        }
    }
});
// Scroll Progress Bar
window.addEventListener('scroll', function () {
    const scrollProgress = document.getElementById('scrollProgress');
    if (scrollProgress) {
        const winScroll = document.documentElement.scrollTop;
        const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
        const scrolled = height > 0 ? (winScroll / height) * 100 : 0;
        scrollProgress.style.width = scrolled + '%';
    }

    // Back to Top Button
    const backToTop = document.getElementById('backToTop');
    if (backToTop) {
        backToTop.classList.toggle('show', window.scrollY > 300);
    }

    // Navbar scroll effect
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        navbar.classList.toggle('scrolled', window.scrollY > 50);
    }
});

// Back to Top
const backToTopButton = document.getElementById('backToTop');
if (backToTopButton) {
    backToTopButton.addEventListener('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
}

// Typing Animation - Multiple titles
let textIndex = 0;
let charIndex = 0;
let isDeleting = false;

function typeText() {
    const typingElement = document.getElementById('typingText');
    if (!typingElement || typeof typingTitles === 'undefined' || !typingTitles.length) {
        return;
    }

    const currentText = typingTitles[textIndex];

    if (!isDeleting && charIndex <= currentText.length) {
        typingElement.textContent = currentText.substring(0, charIndex);
        charIndex++;
        setTimeout(typeText, 100);
    } else if (isDeleting && charIndex >= 0) {
        typingElement.textContent = currentText.substring(0, charIndex);
        charIndex--;
        setTimeout(typeText, 50);
    } else if (!isDeleting && charIndex > currentText.length) {
        setTimeout(() => {
            isDeleting = true;
            typeText();
        }, 2000);
    } else if (isDeleting && charIndex < 0) {
        isDeleting = false;
        textIndex = (textIndex + 1) % typingTitles.length;
        charIndex = 0;
        setTimeout(typeText, 500);
    }
}

setTimeout(typeText, 500);

// Particles Animation
const canvas = document.getElementById('particles-canvas');
const ctx = canvas ? canvas.getContext('2d') : null;

if (canvas && ctx) {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
}

const particles = [];
const particleCount = 80;
const mouse = { x: null, y: null, radius: 150 };

class Particle {
    constructor() {
        this.x = Math.random() * canvas.width;
        this.y = Math.random() * canvas.height;
        this.size = Math.random() * 3 + 1;
        this.speedX = Math.random() * 2 - 1;
        this.speedY = Math.random() * 2 - 1;
    }

    update() {
        this.x += this.speedX;
        this.y += this.speedY;

        if (this.x > canvas.width || this.x < 0) this.speedX *= -1;
        if (this.y > canvas.height || this.y < 0) this.speedY *= -1;

        const dx = mouse.x - this.x;
        const dy = mouse.y - this.y;
        const distance = Math.sqrt(dx * dx + dy * dy);

        if (distance < mouse.radius && mouse.x !== null) {
            this.x -= dx / distance * 2;
            this.y -= dy / distance * 2;
        }
    }

    draw() {
        ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
        ctx.fill();
    }
}

function initParticles() {
    for (let i = 0; i < particleCount; i++) {
        particles.push(new Particle());
    }
}

function connectParticles() {
    for (let i = 0; i < particles.length; i++) {
        for (let j = i + 1; j < particles.length; j++) {
            const dx = particles[i].x - particles[j].x;
            const dy = particles[i].y - particles[j].y;
            const distance = Math.sqrt(dx * dx + dy * dy);

            if (distance < 120) {
                ctx.strokeStyle = `rgba(255, 255, 255, ${1 - distance / 120})`;
                ctx.lineWidth = 0.5;
                ctx.beginPath();
                ctx.moveTo(particles[i].x, particles[i].y);
                ctx.lineTo(particles[j].x, particles[j].y);
                ctx.stroke();
            }
        }
    }
}

function animateParticles() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    particles.forEach(particle => {
        particle.update();
        particle.draw();
    });
    connectParticles();
    requestAnimationFrame(animateParticles);
}

if (canvas) {
    canvas.addEventListener('mousemove', (e) => {
        mouse.x = e.x;
        mouse.y = e.y;
    });

    canvas.addEventListener('mouseleave', () => {
        mouse.x = null;
        mouse.y = null;
    });

    window.addEventListener('resize', () => {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    });
}

if (canvas && ctx && typeof enableParticles !== 'undefined' && enableParticles) {
    initParticles();
    animateParticles();
}

// GSAP Scroll Animations (optional enhancement)
if (typeof gsap !== 'undefined' && typeof ScrollTrigger !== 'undefined') {
    gsap.registerPlugin(ScrollTrigger);

    gsap.utils.toArray('.timeline-item, .about-card, .skill-item').forEach(elem => {
        gsap.fromTo(elem,
            {
                y: 50,
                opacity: 0
            },
            {
                scrollTrigger: {
                    trigger: elem,
                    start: 'top 85%',
                    toggleActions: 'play none none none'
                },
                y: 0,
                opacity: 1,
                duration: 0.8,
                ease: 'power2.out'
            }
        );
    });
}

// Smooth scrolling
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});

// Skills progress bar animation
const skillsObserver = new IntersectionObserver(function (entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const progressBars = entry.target.querySelectorAll('.progress-bar');
            progressBars.forEach(bar => {
                setTimeout(() => {
                    bar.style.width = bar.getAttribute('data-width');
                }, 200);
            });
            skillsObserver.unobserve(entry.target);
        }
    });
}, { threshold: 0.5 });

const skillsSection = document.querySelector('#skills');
if (skillsSection) {
    skillsObserver.observe(skillsSection);
}

function getAntiForgeryToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
}

// Contact Form Submission
const contactForm = document.getElementById('contactForm');
if (contactForm) {
    const statusElement = document.getElementById('contactFormStatus');
    const submitButton = document.getElementById('contactSubmit');

    const setStatus = (message, type) => {
        if (!statusElement) return;
        statusElement.textContent = message;
        statusElement.classList.remove('is-success', 'is-error');
        statusElement.classList.add(type === 'success' ? 'is-success' : 'is-error');
    };

    contactForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const data = {
            Name: formData.get('Name'),
            Email: formData.get('Email'),
            Message: formData.get('Message')
        };

        if (submitButton) {
            submitButton.disabled = true;
        }
        setStatus(currentLang === 'en' ? 'Sending...' : 'جارٍ الإرسال...', 'success');

        try {
            const response = await fetch('/Home/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(data)
            });

            const result = await response.json().catch(() => ({}));

            if (response.ok && result.success) {
                setStatus(
                    currentLang === 'en' ? 'Message sent successfully!' : 'تم إرسال الرسالة بنجاح!',
                    'success');
                this.reset();
            } else if (response.status === 429) {
                setStatus(
                    currentLang === 'en'
                        ? 'Too many messages. Please try again later.'
                        : 'عدد الرسائل كبير. يرجى المحاولة لاحقًا.',
                    'error');
            } else {
                setStatus(
                    result.message ||
                        (currentLang === 'en' ? 'Failed to send message.' : 'فشل إرسال الرسالة.'),
                    'error');
            }
        } catch (error) {
            console.error('Contact form error:', error);
            setStatus(
                currentLang === 'en'
                    ? 'An error occurred. Please try again.'
                    : 'حدث خطأ. يرجى المحاولة مرة أخرى.',
                'error');
        } finally {
            if (submitButton) {
                submitButton.disabled = false;
            }
        }
    });
}
//// Theme Toggle
//let isDark = true;
//document.getElementById('themeToggle').addEventListener('click', function () {
//    isDark = !isDark;
//    document.body.classList.toggle('light-mode');
//    const icon = this.querySelector('i');
//    icon.className = isDark ? 'fas fa-moon' : 'fas fa-sun';
//});

// Project Filter
const filterBtns = document.querySelectorAll('.filter-btn');
const projectItems = document.querySelectorAll('.project-item');
let currentFilter = 'all';
let showingAll = false;

filterBtns.forEach(btn => {
    btn.addEventListener('click', function () {
        filterBtns.forEach(b => b.classList.remove('active'));
        this.classList.add('active');

        currentFilter = this.getAttribute('data-filter');
        showingAll = false;
        updateProjectVisibility();
        updateShowMoreButton();
    });
});

function updateProjectVisibility() {
    projectItems.forEach(item => {
        const category = item.getAttribute('data-category');
        const isHidden = item.classList.contains('hidden-project');
        const matchesFilter = currentFilter === 'all' || category === currentFilter;

        if (matchesFilter) {
            if (!isHidden) {
                item.style.display = 'block';
            } else {
                item.style.display = showingAll ? 'block' : 'none';
            }
        } else {
            item.style.display = 'none';
        }
    });
}

function updateShowMoreButton() {
    const showMoreBtn = document.getElementById('showMoreBtn');
    if (!showMoreBtn) return;

    const btnText = document.getElementById('showMoreText');
    const icon = document.getElementById('showMoreIcon');

    let hiddenMatchingCount = 0;
    projectItems.forEach(item => {
        const category = item.getAttribute('data-category');
        const isHidden = item.classList.contains('hidden-project');
        const matchesFilter = currentFilter === 'all' || category === currentFilter;

        if (isHidden && matchesFilter) {
            hiddenMatchingCount++;
        }
    });

    if (hiddenMatchingCount > 0) {
        showMoreBtn.style.display = 'inline-block';

        const showMoreText = currentLang === 'en' ? 'Show More Projects' : 'عرض المزيد من المشاريع';
        const showLessText = currentLang === 'en' ? 'Show Less Projects' : 'عرض أقل من المشاريع';

        btnText.textContent = showingAll ? showLessText : showMoreText;
        icon.className = showingAll ? 'fas fa-chevron-up ms-2' : 'fas fa-chevron-down ms-2';
    } else {
        showMoreBtn.style.display = 'none';
    }
}

function toggleProjects() {
    showingAll = !showingAll;
    updateProjectVisibility();
    updateShowMoreButton();
}

// Track Project Click
async function trackProjectClick(projectId, url) {
    if (!url) {
        return;
    }

    try {
        await fetch('/Home/TrackProjectClick', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ projectId })
        });
    } catch (error) {
        console.error('Error tracking click:', error);
    }

    window.open(url, '_blank', 'noopener');
}

// Initialize on page load
updateShowMoreButton();