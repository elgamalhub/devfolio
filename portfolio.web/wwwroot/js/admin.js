(function () {
    'use strict';

    const shell = document.getElementById('wrapper');
    const sidebar = document.getElementById('accordionSidebar');
    const toggle = document.getElementById('sidebarToggle');
    const backdrop = document.getElementById('sidebarBackdrop');
    const mobileQuery = window.matchMedia('(max-width: 991.98px)');
    const storageKey = 'portfolio-admin-sidebar-collapsed';

    /* Sidebar ------------------------------------------------------------- */

    const isMobile = () => mobileQuery.matches;

    const syncToggleState = () => {
        if (!toggle || !shell) {
            return;
        }

        if (isMobile()) {
            const open = shell.classList.contains('sidebar-open');
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        } else {
            const collapsed = shell.classList.contains('sidebar-collapsed');
            toggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
        }
    };

    const closeMobileSidebar = () => {
        if (!shell) {
            return;
        }
        shell.classList.remove('sidebar-open');
        if (backdrop) {
            backdrop.hidden = true;
        }
        document.body.style.overflow = '';
        syncToggleState();
    };

    const openMobileSidebar = () => {
        if (!shell) {
            return;
        }
        shell.classList.add('sidebar-open');
        if (backdrop) {
            backdrop.hidden = false;
        }
        document.body.style.overflow = 'hidden';
        syncToggleState();
    };

    if (shell && !isMobile()) {
        const stored = window.localStorage.getItem(storageKey);
        if (stored === 'true') {
            shell.classList.add('sidebar-collapsed');
        }
        syncToggleState();
    }

    if (toggle && shell) {
        toggle.addEventListener('click', () => {
            if (isMobile()) {
                if (shell.classList.contains('sidebar-open')) {
                    closeMobileSidebar();
                } else {
                    openMobileSidebar();
                }
                return;
            }

            const collapsed = shell.classList.toggle('sidebar-collapsed');
            window.localStorage.setItem(storageKey, collapsed ? 'true' : 'false');
            syncToggleState();
        });
    }

    if (backdrop) {
        backdrop.addEventListener('click', closeMobileSidebar);
    }

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && shell && shell.classList.contains('sidebar-open')) {
            closeMobileSidebar();
        }
    });

    mobileQuery.addEventListener('change', () => {
        closeMobileSidebar();
        syncToggleState();
    });

    if (sidebar && isMobile()) {
        sidebar.querySelectorAll('a.nav-link').forEach((link) => {
            link.addEventListener('click', closeMobileSidebar);
        });
    }

    /* Auto-dismiss alerts ------------------------------------------------- */

    document.querySelectorAll('.alert.alert-dismissible').forEach((alert) => {
        window.setTimeout(() => {
            if (!alert.isConnected || !window.bootstrap) {
                return;
            }
            const instance = window.bootstrap.Alert.getOrCreateInstance(alert);
            instance.close();
        }, 6000);
    });

    /* Confirmation prompts ------------------------------------------------ */

    document.querySelectorAll('[data-confirm]').forEach((form) => {
        form.addEventListener('submit', (event) => {
            const message = form.getAttribute('data-confirm') || 'Are you sure?';
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });

    /* Image previews ------------------------------------------------------ */

    document.querySelectorAll('input[type="file"][data-preview]').forEach((input) => {
        const target = document.querySelector(input.getAttribute('data-preview'));
        if (!target) {
            return;
        }

        input.addEventListener('change', () => {
            const file = input.files && input.files[0];
            if (!file || !file.type.startsWith('image/')) {
                target.classList.remove('is-visible');
                target.removeAttribute('src');
                return;
            }

            const reader = new FileReader();
            reader.onload = (loadEvent) => {
                target.src = loadEvent.target.result;
                target.classList.add('is-visible');
            };
            reader.readAsDataURL(file);
        });
    });

    /* Unsaved-changes guard for long editor forms ------------------------- */

    document.querySelectorAll('form[data-guard-unsaved]').forEach((form) => {
        let dirty = false;
        const markDirty = () => { dirty = true; };

        form.querySelectorAll('input, textarea, select').forEach((field) => {
            field.addEventListener('input', markDirty);
            field.addEventListener('change', markDirty);
        });

        form.addEventListener('submit', () => {
            dirty = false;
        });

        window.addEventListener('beforeunload', (event) => {
            if (!dirty) {
                return;
            }
            event.preventDefault();
            event.returnValue = '';
        });
    });
})();
