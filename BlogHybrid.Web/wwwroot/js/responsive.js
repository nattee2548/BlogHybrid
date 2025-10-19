// ================================================
// USER RESPONSIVE - MOBILE DRAWER
// แก้ไข: ไม่กระทบ Theme Switcher
// ================================================

(function () {
    'use strict';

    // ตรวจสอบว่าไม่ใช่ Admin
    if (document.body.classList.contains('admin-body')) {
        console.log('⚠️ Admin page - Mobile drawer disabled');
        return;
    }

    console.log('🚀 Initializing mobile drawer...');

    // เช็คว่า theme-switcher.js โหลดแล้วหรือยัง
    if (typeof window.ThemeSwitcher !== 'undefined') {
        console.log('✓ Theme switcher detected');
    }

    // รอให้ DOM โหลด
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
        // 1. สร้าง Hamburger Button
        const navbar = document.querySelector('.modern-navbar-container');
        if (navbar && !document.getElementById('mobileSidebarToggle')) {
            const logo = navbar.querySelector('.modern-logo');
            if (logo) {
                const btn = document.createElement('button');
                btn.id = 'mobileSidebarToggle';
                btn.className = 'mobile-sidebar-toggle';
                btn.setAttribute('aria-label', 'เปิด/ปิดเมนู');
                btn.setAttribute('data-mobile-toggle', 'true'); // ✅ เพิ่ม attribute เพื่อแยกจาก theme toggle
                btn.innerHTML = '<div class="hamburger"></div>';
                logo.parentNode.insertBefore(btn, logo);
                console.log('✓ Hamburger button created');
            }
        }

        // 2. สร้าง Overlay
        if (!document.getElementById('sidebarDrawerOverlay')) {
            const overlay = document.createElement('div');
            overlay.id = 'sidebarDrawerOverlay';
            overlay.className = 'sidebar-drawer-overlay';
            overlay.setAttribute('data-mobile-overlay', 'true'); // ✅ เพิ่ม attribute
            document.body.appendChild(overlay);
            console.log('✓ Overlay created');
        }

        // 3. สร้าง Drawer Header
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        if (sidebar && !sidebar.querySelector('.drawer-header')) {
            const header = document.createElement('div');
            header.className = 'drawer-header';
            header.innerHTML = `
                <h3 class="drawer-title">เมนู</h3>
                <button class="drawer-close" id="drawerCloseBtn" data-mobile-close="true" aria-label="ปิด">
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M15 5L5 15M5 5l10 10"/>
                    </svg>
                </button>
            `;
            sidebar.insertBefore(header, sidebar.firstChild);
            console.log('✓ Drawer header created');
        }
    }

    function setupEvents() {
        const toggle = document.getElementById('mobileSidebarToggle');
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        const overlay = document.getElementById('sidebarDrawerOverlay');
        const closeBtn = document.getElementById('drawerCloseBtn');

        if (!toggle || !sidebar || !overlay) {
            console.error('❌ Required elements not found');
            return;
        }

        // ✅ ใช้ Event Listener แบบเฉพาะเจาะจง - ไม่ใช้ document.addEventListener

        // 1. Toggle Button - ใช้ direct event
        toggle.onclick = function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation(); // ✅ หยุด event ทันที

            const isOpen = sidebar.classList.contains('active');
            if (isOpen) {
                closeDrawer();
            } else {
                openDrawer();
            }
            console.log('🍔 Hamburger clicked');
        };

        // 2. Overlay - ใช้ direct event
        overlay.onclick = function (e) {
            e.preventDefault();
            e.stopPropagation();
            closeDrawer();
            console.log('📱 Overlay clicked');
        };

        // 3. Close Button - ใช้ direct event
        if (closeBtn) {
            closeBtn.onclick = function (e) {
                e.preventDefault();
                e.stopPropagation();
                closeDrawer();
                console.log('❌ Close button clicked');
            };
        }

        // 4. ESC Key - แยกต่างหาก
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('active')) {
                closeDrawer();
                console.log('⎋ ESC pressed');
            }
        });

        // 5. Window Resize
        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (window.innerWidth > 991 && sidebar.classList.contains('active')) {
                    closeDrawer();
                }
            }, 250);
        });

        // 6. Auto-close เมื่อคลิกลิงก์
        const links = sidebar.querySelectorAll('a');
        links.forEach(function (link) {
            link.addEventListener('click', function () {
                setTimeout(closeDrawer, 150);
            });
        });

        console.log('✓ Events setup complete');
    }

    function openDrawer() {
        const toggle = document.getElementById('mobileSidebarToggle');
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        const overlay = document.getElementById('sidebarDrawerOverlay');

        if (!toggle || !sidebar || !overlay) return;

        sidebar.classList.add('active');
        overlay.classList.add('active');
        toggle.classList.add('active');
        document.body.classList.add('drawer-open');

        toggle.setAttribute('aria-expanded', 'true');
        sidebar.setAttribute('aria-hidden', 'false');

        console.log('✅ Drawer opened');
    }

    function closeDrawer() {
        const toggle = document.getElementById('mobileSidebarToggle');
        const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
        const overlay = document.getElementById('sidebarDrawerOverlay');

        if (!toggle || !sidebar || !overlay) return;

        sidebar.classList.remove('active');
        overlay.classList.remove('active');
        toggle.classList.remove('active');
        document.body.classList.remove('drawer-open');

        toggle.setAttribute('aria-expanded', 'false');
        sidebar.setAttribute('aria-hidden', 'true');

        console.log('✅ Drawer closed');
    }

    // Export API
    window.MobileDrawer = {
        open: openDrawer,
        close: closeDrawer,
        isOpen: function () {
            const sidebar = document.querySelector('.modern-sidebar, .user-sidebar');
            return sidebar ? sidebar.classList.contains('active') : false;
        }
    };

})();