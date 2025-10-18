// ================================================
// MOBILE SIDEBAR DRAWER FUNCTIONALITY
// เหมือน Reddit - สำหรับ User เท่านั้น
// ================================================

(function () {
    'use strict';

    // ตรวจสอบว่าอยู่ใน Admin หรือไม่
    if (document.body.classList.contains('admin-body')) {
        return; // ไม่ทำงานใน Admin
    }

    // รอให้ DOM โหลดเสร็จ
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initMobileDrawer);
    } else {
        initMobileDrawer();
    }

    function initMobileDrawer() {
        // สร้าง Elements ที่จำเป็น
        createDrawerElements();

        // ดึง Elements
        const sidebarToggle = document.getElementById('mobileSidebarToggle');
        const userSidebar = document.querySelector('.user-sidebar, .modern-sidebar');
        const drawerOverlay = document.getElementById('sidebarDrawerOverlay');
        const drawerClose = document.getElementById('drawerClose');

        // ตรวจสอบว่ามี elements ครบหรือไม่
        if (!sidebarToggle || !userSidebar || !drawerOverlay) {
            console.log('Mobile drawer elements not found');
            return;
        }

        // ฟังก์ชันเปิด Drawer
        function openDrawer() {
            userSidebar.classList.add('active');
            drawerOverlay.classList.add('active');
            sidebarToggle.classList.add('active');
            document.body.classList.add('drawer-open');

            // Accessibility
            sidebarToggle.setAttribute('aria-expanded', 'true');
            userSidebar.setAttribute('aria-hidden', 'false');
        }

        // ฟังก์ชันปิด Drawer
        function closeDrawer() {
            userSidebar.classList.remove('active');
            drawerOverlay.classList.remove('active');
            sidebarToggle.classList.remove('active');
            document.body.classList.remove('drawer-open');

            // Accessibility
            sidebarToggle.setAttribute('aria-expanded', 'false');
            userSidebar.setAttribute('aria-hidden', 'true');
        }

        // ฟังก์ชัน Toggle
        function toggleDrawer() {
            if (userSidebar.classList.contains('active')) {
                closeDrawer();
            } else {
                openDrawer();
            }
        }

        // Event Listeners
        sidebarToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleDrawer();
        });

        drawerOverlay.addEventListener('click', closeDrawer);

        if (drawerClose) {
            drawerClose.addEventListener('click', closeDrawer);
        }

        // ปิด Drawer เมื่อกด Escape
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && userSidebar.classList.contains('active')) {
                closeDrawer();
            }
        });

        // ปิด Drawer เมื่อคลิก Link ใน Sidebar (Optional)
        const sidebarLinks = userSidebar.querySelectorAll('a');
        sidebarLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                // ปิด Drawer หลังจากคลิกลิงก์
                setTimeout(closeDrawer, 150);
            });
        });

        // จัดการเมื่อ Resize หน้าจอ
        let resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                // ถ้าหน้าจอกว้างกว่า 991px และ Drawer เปิดอยู่ ให้ปิด
                if (window.innerWidth > 991 && userSidebar.classList.contains('active')) {
                    closeDrawer();
                }
            }, 250);
        });

        // Prevent scrolling on touch devices when drawer is open
        let touchStartY = 0;
        userSidebar.addEventListener('touchstart', function (e) {
            touchStartY = e.touches[0].clientY;
        }, { passive: true });

        userSidebar.addEventListener('touchmove', function (e) {
            const touchY = e.touches[0].clientY;
            const scrollTop = userSidebar.scrollTop;
            const scrollHeight = userSidebar.scrollHeight;
            const clientHeight = userSidebar.clientHeight;

            // Prevent overscroll
            if ((scrollTop === 0 && touchY > touchStartY) ||
                (scrollTop + clientHeight >= scrollHeight && touchY < touchStartY)) {
                e.preventDefault();
            }
        }, { passive: false });
    }

    // สร้าง Elements ที่จำเป็น
    function createDrawerElements() {
        // สร้าง Hamburger Button
        const navbar = document.querySelector('.main-navbar, .modern-navbar-container');
        if (navbar && !document.getElementById('mobileSidebarToggle')) {
            const navbarLeft = navbar.querySelector('.main-navbar-left, .modern-navbar-left') || navbar;

            const toggleBtn = document.createElement('button');
            toggleBtn.id = 'mobileSidebarToggle';
            toggleBtn.className = 'mobile-sidebar-toggle';
            toggleBtn.setAttribute('aria-label', 'Toggle Sidebar Menu');
            toggleBtn.setAttribute('aria-expanded', 'false');
            toggleBtn.innerHTML = '<div class="hamburger"></div>';

            navbarLeft.insertBefore(toggleBtn, navbarLeft.firstChild);
        }

        // สร้าง Overlay
        if (!document.getElementById('sidebarDrawerOverlay')) {
            const overlay = document.createElement('div');
            overlay.id = 'sidebarDrawerOverlay';
            overlay.className = 'sidebar-drawer-overlay';
            document.body.appendChild(overlay);
        }

        // สร้าง Drawer Header ใน Sidebar
        const sidebar = document.querySelector('.user-sidebar, .modern-sidebar');
        if (sidebar && !sidebar.querySelector('.drawer-header')) {
            const drawerHeader = document.createElement('div');
            drawerHeader.className = 'drawer-header';
            drawerHeader.innerHTML = `
                <h3 class="drawer-title">เมนู</h3>
                <button class="drawer-close" id="drawerClose" aria-label="Close Sidebar">
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M15 5L5 15M5 5l10 10"/>
                    </svg>
                </button>
            `;
            sidebar.insertBefore(drawerHeader, sidebar.firstChild);
        }

        // เพิ่ม aria attributes
        if (sidebar) {
            sidebar.setAttribute('role', 'complementary');
            sidebar.setAttribute('aria-hidden', 'true');
        }
    }

    // Export function สำหรับใช้ภายนอก (ถ้าต้องการ)
    window.mobileDrawer = {
        open: function () {
            const sidebar = document.querySelector('.user-sidebar, .modern-sidebar');
            if (sidebar) {
                const event = new Event('click');
                document.getElementById('mobileSidebarToggle')?.dispatchEvent(event);
            }
        },
        close: function () {
            const sidebar = document.querySelector('.user-sidebar, .modern-sidebar');
            if (sidebar && sidebar.classList.contains('active')) {
                const overlay = document.getElementById('sidebarDrawerOverlay');
                if (overlay) {
                    const event = new Event('click');
                    overlay.dispatchEvent(event);
                }
            }
        }
    };

    console.log('Mobile drawer initialized');
})();