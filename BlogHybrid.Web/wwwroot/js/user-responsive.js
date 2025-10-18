// ================================================
// USER RESPONSIVE - MOBILE DRAWER
// ไฟล์เดียวจบ
// ================================================

(function () {
    'use strict';

    // Skip if Admin
    if (document.body.classList.contains('admin-body')) return;

    // Init
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        createElements();
        setupEvents();
        console.log('✓ Mobile drawer ready');
    }

    function createElements() {
        // 1. Hamburger button
        const navbar = document.querySelector('.modern-navbar-container');
        if (navbar && !document.getElementById('mobileSidebarToggle')) {
            const logo = navbar.querySelector('.modern-logo');
            if (logo) {
                const btn = document.createElement('button');
                btn.id = 'mobileSidebarToggle';
                btn.className = 'mobile-sidebar-toggle';
                btn.innerHTML = '<div class="hamburger"></div>';
                logo.parentNode.insertBefore(btn, logo);
            }
        }

        // 2. Overlay
        if (!document.getElementById('sidebarDrawerOverlay')) {
            const overlay = document.createElement('div');
            overlay.id = 'sidebarDrawerOverlay';
            overlay.className = 'sidebar-drawer-overlay';
            document.body.appendChild(overlay);
        }

        // 3. Drawer header
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        if (sidebar && !sidebar.querySelector('.drawer-header')) {
            const header = document.createElement('div');
            header.className = 'drawer-header';
            header.innerHTML = `
                <h3 class="drawer-title">เมนู</h3>
                <button class="drawer-close" id="drawerCloseBtn">
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M15 5L5 15M5 5l10 10"/>
                    </svg>
                </button>
            `;
            sidebar.insertBefore(header, sidebar.firstChild);
        }
    }

    function setupEvents() {
        const toggle = document.getElementById('mobileSidebarToggle');
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        const overlay = document.getElementById('sidebarDrawerOverlay');
        const close = document.getElementById('drawerCloseBtn');

        if (!toggle || !sidebar || !overlay) return;

        // Toggle - ใช้ stopPropagation เพื่อไม่กระทบ event อื่น
        toggle.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const isOpen = sidebar.classList.contains('active');
            isOpen ? closeDrawer() : openDrawer();
        });

        // Overlay - ใช้ stopPropagation
        overlay.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            closeDrawer();
        });

        // Close button - ใช้ stopPropagation
        if (close) {
            close.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                closeDrawer();
            });
        }

        // ESC key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && sidebar.classList.contains('active')) {
                closeDrawer();
            }
        });

        // Resize
        let timer;
        window.addEventListener('resize', () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                if (window.innerWidth > 991 && sidebar.classList.contains('active')) {
                    closeDrawer();
                }
            }, 250);
        });

        // Auto-close on link click
        sidebar.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', () => setTimeout(closeDrawer, 150));
        });
    }

    function openDrawer() {
        const toggle = document.getElementById('mobileSidebarToggle');
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        const overlay = document.getElementById('sidebarDrawerOverlay');

        sidebar.classList.add('active');
        overlay.classList.add('active');
        toggle.classList.add('active');
        document.body.classList.add('drawer-open');
    }

    function closeDrawer() {
        const toggle = document.getElementById('mobileSidebarToggle');
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        const overlay = document.getElementById('sidebarDrawerOverlay');

        sidebar.classList.remove('active');
        overlay.classList.remove('active');
        toggle.classList.remove('active');
        document.body.classList.remove('drawer-open');
    }

    // API
    window.MobileDrawer = { open: openDrawer, close: closeDrawer };
})();