// BlogHybrid.Web/wwwroot/js/admin.js
// Admin Dashboard JavaScript - Final Clean Version

(function () {
    'use strict';

    // Admin Theme Management
    class AdminThemeManager {
        constructor() {
            this.themeKey = '404talk-admin-theme';
            this.toggleBtn = document.getElementById('adminThemeToggle');
            this.themeIcon = document.getElementById('adminThemeIcon');
            this.init();
        }

        init() {
            const savedTheme = this.getSavedTheme();
            this.setTheme(savedTheme);

            if (this.toggleBtn) {
                this.toggleBtn.addEventListener('click', () => {
                    this.toggleTheme();
                });
            }
        }

        getSavedTheme() {
            return localStorage.getItem(this.themeKey) || 'dark';
        }

        setTheme(theme) {
            document.documentElement.setAttribute('data-theme', theme);

            if (this.themeIcon) {
                if (theme === 'dark') {
                    this.themeIcon.className = 'fas fa-sun';
                    this.toggleBtn.title = 'เปลี่ยนเป็นธีมสว่าง';
                } else {
                    this.themeIcon.className = 'fas fa-moon';
                    this.toggleBtn.title = 'เปลี่ยนเป็นธีมมืด';
                }
            }

            localStorage.setItem(this.themeKey, theme);
        }

        toggleTheme() {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            this.setTheme(newTheme);
        }
    }

    // Enhanced Sidebar Management with Mobile Support
    class AdminSidebar {
        constructor() {
            this.sidebar = document.getElementById('adminSidebar');
            this.toggleBtn = document.getElementById('sidebarToggle');
            this.overlay = document.getElementById('sidebarOverlay');
            this.collapsedKey = '404talk-admin-sidebar-collapsed';
            this.isMobile = false;
            this.init();
        }

        init() {
            // Check if mobile on load
            this.checkMobileState();

            // Create overlay if it doesn't exist
            this.ensureOverlay();

            // Load saved state only for desktop
            if (!this.isMobile) {
                const isCollapsed = localStorage.getItem(this.collapsedKey) === 'true';
                if (isCollapsed) {
                    this.collapse();
                }
            }

            // Bind events
            this.bindEvents();

            // Handle resize
            this.handleResize();
            window.addEventListener('resize', () => this.handleResize());
        }

        ensureOverlay() {
            if (!this.overlay) {
                this.overlay = document.createElement('div');
                this.overlay.id = 'sidebarOverlay';
                this.overlay.className = 'sidebar-overlay';
                document.body.appendChild(this.overlay);
            }
        }

        bindEvents() {
            if (this.toggleBtn) {
                this.toggleBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.toggle();
                });
            }

            // Close sidebar when clicking overlay on mobile
            if (this.overlay) {
                this.overlay.addEventListener('click', () => {
                    if (this.isMobile) {
                        this.closeMobile();
                    }
                });
            }

            // Handle escape key to close mobile sidebar
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && this.isMobile && document.body.classList.contains('sidebar-open')) {
                    this.closeMobile();
                }
            });

            // Close mobile sidebar when clicking nav links
            if (this.sidebar) {
                const navLinks = this.sidebar.querySelectorAll('.nav-item');
                navLinks.forEach(link => {
                    link.addEventListener('click', () => {
                        if (this.isMobile) {
                            setTimeout(() => this.closeMobile(), 150);
                        }
                    });
                });
            }
        }

        checkMobileState() {
            this.isMobile = window.innerWidth < 992;
        }

        toggle() {
            if (this.isMobile) {
                this.toggleMobile();
            } else {
                this.toggleDesktop();
            }
        }

        toggleMobile() {
            const isOpen = document.body.classList.contains('sidebar-open');
            if (isOpen) {
                this.closeMobile();
            } else {
                this.openMobile();
            }
        }

        openMobile() {
            document.body.classList.add('sidebar-open');
            document.body.style.overflow = 'hidden'; // Prevent background scroll
        }

        closeMobile() {
            document.body.classList.remove('sidebar-open');
            document.body.style.overflow = ''; // Restore scroll
        }

        toggleDesktop() {
            const isCollapsed = document.body.classList.contains('sidebar-collapsed');
            if (isCollapsed) {
                this.expand();
            } else {
                this.collapse();
            }
        }

        collapse() {
            document.body.classList.add('sidebar-collapsed');
            localStorage.setItem(this.collapsedKey, 'true');
        }

        expand() {
            document.body.classList.remove('sidebar-collapsed');
            localStorage.setItem(this.collapsedKey, 'false');
        }

        handleResize() {
            const wasMobile = this.isMobile;
            this.checkMobileState();

            // If switching between mobile and desktop
            if (wasMobile !== this.isMobile) {
                // Clean up mobile state when switching to desktop
                if (!this.isMobile) {
                    this.closeMobile();
                    document.body.style.overflow = '';

                    // Restore desktop collapsed state
                    const isCollapsed = localStorage.getItem(this.collapsedKey) === 'true';
                    if (isCollapsed) {
                        this.collapse();
                    } else {
                        this.expand();
                    }
                } else {
                    // Clean up desktop state when switching to mobile
                    document.body.classList.remove('sidebar-collapsed');
                }
            }
        }
    }

    // Loading Overlay Manager
    class AdminLoadingManager {
        constructor() {
            this.loadingOverlay = document.getElementById('adminLoading');
            this.ensureLoadingElement();
        }

        ensureLoadingElement() {
            if (!this.loadingOverlay) {
                this.loadingOverlay = document.createElement('div');
                this.loadingOverlay.id = 'adminLoading';
                this.loadingOverlay.className = 'admin-loading';
                this.loadingOverlay.innerHTML = `
                    <div class="loading-content">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <div class="loading-text">กำลังโหลด...</div>
                    </div>
                `;
                document.body.appendChild(this.loadingOverlay);
            }
        }

        show(text = 'กำลังโหลด...') {
            if (this.loadingOverlay) {
                const loadingText = this.loadingOverlay.querySelector('.loading-text');
                if (loadingText) {
                    loadingText.textContent = text;
                }
                this.loadingOverlay.classList.add('show');
            }
        }

        hide() {
            if (this.loadingOverlay) {
                this.loadingOverlay.classList.remove('show');
            }
        }
    }

    // Navigation State Manager
    class AdminNavManager {
        constructor() {
            this.init();
        }

        init() {
            this.setActiveNavItem();
            this.updateBreadcrumb();
        }

        setActiveNavItem() {
            const currentPath = window.location.pathname;
            const navItems = document.querySelectorAll('.nav-item');

            navItems.forEach(item => {
                const href = item.getAttribute('href');
                if (href && currentPath.includes(href)) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
            });
        }

        updateBreadcrumb() {
            // Update breadcrumb based on current page
            const breadcrumb = document.querySelector('.admin-breadcrumb');
            if (breadcrumb) {
                // This would be implemented based on your routing structure
                // For now, we'll keep the existing breadcrumb
            }
        }
    }

    // Stats Counter Animation
    class AdminStatsManager {
        constructor() {
            this.init();
        }

        init() {
            this.animateCounters();
        }

        animateCounters() {
            const counters = document.querySelectorAll('[data-counter]');
            counters.forEach(counter => {
                const target = parseInt(counter.getAttribute('data-counter'));
                const duration = 2000; // 2 seconds
                const step = target / (duration / 16); // 60fps
                let current = 0;

                const timer = setInterval(() => {
                    current += step;
                    if (current >= target) {
                        current = target;
                        clearInterval(timer);
                    }
                    counter.textContent = Math.floor(current).toLocaleString();
                }, 16);
            });
        }
    }

    // Data Tables Enhancement
    class AdminDataTable {
        constructor(tableSelector) {
            this.table = document.querySelector(tableSelector);
            if (this.table) {
                this.init();
            }
        }

        init() {
            this.addSorting();
            this.addRowHover();
            this.addBulkActions();
            this.makeResponsive();
        }

        addSorting() {
            const headers = this.table.querySelectorAll('th[data-sortable]');
            headers.forEach(header => {
                header.style.cursor = 'pointer';
                header.addEventListener('click', () => {
                    this.sortTable(header);
                });
            });
        }

        sortTable(header) {
            const table = header.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const columnIndex = Array.from(header.parentNode.children).indexOf(header);
            const isAscending = header.classList.contains('sort-asc');

            rows.sort((a, b) => {
                const aText = a.children[columnIndex].textContent.trim();
                const bText = b.children[columnIndex].textContent.trim();

                if (isAscending) {
                    return bText.localeCompare(aText);
                } else {
                    return aText.localeCompare(bText);
                }
            });

            // Update DOM
            rows.forEach(row => tbody.appendChild(row));

            // Update sort indicators
            header.parentNode.querySelectorAll('th').forEach(th => {
                th.classList.remove('sort-asc', 'sort-desc');
            });
            header.classList.add(isAscending ? 'sort-desc' : 'sort-asc');
        }

        addRowHover() {
            const rows = this.table.querySelectorAll('tbody tr');
            rows.forEach(row => {
                row.addEventListener('mouseenter', () => {
                    row.style.backgroundColor = 'var(--admin-bg-tertiary)';
                });
                row.addEventListener('mouseleave', () => {
                    row.style.backgroundColor = '';
                });
            });
        }

        addBulkActions() {
            const selectAll = this.table.querySelector('input[data-select-all]');
            const rowCheckboxes = this.table.querySelectorAll('input[data-select-row]');

            if (selectAll) {
                selectAll.addEventListener('change', () => {
                    rowCheckboxes.forEach(checkbox => {
                        checkbox.checked = selectAll.checked;
                    });
                    this.updateBulkActionBar();
                });
            }

            rowCheckboxes.forEach(checkbox => {
                checkbox.addEventListener('change', () => {
                    this.updateBulkActionBar();
                });
            });
        }

        updateBulkActionBar() {
            const selected = this.table.querySelectorAll('input[data-select-row]:checked');
            const bulkBar = document.querySelector('.bulk-action-bar');

            if (bulkBar) {
                if (selected.length > 0) {
                    bulkBar.style.display = 'flex';
                    bulkBar.querySelector('.selected-count').textContent = selected.length;
                } else {
                    bulkBar.style.display = 'none';
                }
            }
        }

        makeResponsive() {
            if (window.innerWidth < 768) {
                this.table.classList.add('table-responsive');
            }
        }
    }

    // Form Enhancement
    class AdminForm {
        static init() {
            // Auto-save drafts
            const forms = document.querySelectorAll('form[data-auto-save]');
            forms.forEach(form => {
                const inputs = form.querySelectorAll('input, textarea, select');
                inputs.forEach(input => {
                    input.addEventListener('change', () => {
                        AdminForm.saveDraft(form);
                    });
                });
            });

            // Confirm before leaving unsaved forms
            AdminForm.setupUnsavedWarning();
        }

        static saveDraft(form) {
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            const key = `admin-draft-${form.id || 'form'}`;
            localStorage.setItem(key, JSON.stringify(data));
        }

        static loadDraft(formId) {
            const key = `admin-draft-${formId}`;
            const data = localStorage.getItem(key);
            if (data) {
                return JSON.parse(data);
            }
            return null;
        }

        static clearDraft(formId) {
            const key = `admin-draft-${formId}`;
            localStorage.removeItem(key);
        }

        static setupUnsavedWarning() {
            let hasUnsavedChanges = false;

            const forms = document.querySelectorAll('form[data-warn-unsaved]');
            forms.forEach(form => {
                const inputs = form.querySelectorAll('input, textarea, select');
                inputs.forEach(input => {
                    input.addEventListener('change', () => {
                        hasUnsavedChanges = true;
                    });
                });

                form.addEventListener('submit', () => {
                    hasUnsavedChanges = false;
                });
            });

            window.addEventListener('beforeunload', (e) => {
                if (hasUnsavedChanges) {
                    e.preventDefault();
                    e.returnValue = 'คุณมีการเปลี่ยนแปลงที่ยังไม่ได้บันทึก ต้องการออกจากหน้านี้หรือไม่?';
                }
            });
        }
    }

    // Touch gesture support for mobile sidebar
    class TouchGestureManager {
        constructor() {
            this.startX = 0;
            this.startY = 0;
            this.threshold = 100; // Minimum swipe distance
            this.restraint = 100; // Maximum perpendicular distance
            this.allowedTime = 300; // Maximum time allowed for swipe
            this.startTime = 0;
            this.init();
        }

        init() {
            if ('ontouchstart' in window) {
                document.addEventListener('touchstart', (e) => this.handleTouchStart(e), { passive: true });
                document.addEventListener('touchend', (e) => this.handleTouchEnd(e), { passive: true });
            }
        }

        handleTouchStart(e) {
            const touchObj = e.changedTouches[0];
            this.startX = touchObj.pageX;
            this.startY = touchObj.pageY;
            this.startTime = new Date().getTime();
        }

        handleTouchEnd(e) {
            const touchObj = e.changedTouches[0];
            const distX = touchObj.pageX - this.startX;
            const distY = touchObj.pageY - this.startY;
            const elapsedTime = new Date().getTime() - this.startTime;

            // Check if it's a valid swipe
            if (elapsedTime <= this.allowedTime) {
                // Swipe right from left edge to open sidebar
                if (distX >= this.threshold && Math.abs(distY) <= this.restraint && this.startX <= 50) {
                    if (window.adminSidebar && window.adminSidebar.isMobile) {
                        window.adminSidebar.openMobile();
                    }
                }
                // Swipe left to close sidebar
                else if (distX <= -this.threshold && Math.abs(distY) <= this.restraint) {
                    if (window.adminSidebar && window.adminSidebar.isMobile &&
                        document.body.classList.contains('sidebar-open')) {
                        window.adminSidebar.closeMobile();
                    }
                }
            }
        }
    }

    // Global Admin Utilities
    window.AdminUtils = {
        // Show loading overlay
        showLoading: function (text) {
            if (window.adminLoadingManager) {
                window.adminLoadingManager.show(text);
            }
        },

        // Hide loading overlay
        hideLoading: function () {
            if (window.adminLoadingManager) {
                window.adminLoadingManager.hide();
            }
        },

        // Show notification
        notify: function (type, message) {
            if (window.adminNotyf) {
                window.adminNotyf.open({
                    type: type,
                    message: message
                });
            }
        },

        // Confirm dialog
        confirm: function (message, callback) {
            if (confirm(message)) {
                callback();
            }
        },

        // Format numbers
        formatNumber: function (num) {
            return new Intl.NumberFormat('th-TH').format(num);
        },

        // Format date
        formatDate: function (date) {
            return new Intl.DateTimeFormat('th-TH', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            }).format(new Date(date));
        }
    };

    // Dashboard specific functions
    window.refreshDashboard = function () {
        window.AdminUtils.showLoading('กำลังรีเฟรชข้อมูล...');

        // Simulate API call
        setTimeout(() => {
            window.AdminUtils.hideLoading();
            window.AdminUtils.notify('success', 'รีเฟรชข้อมูลเรียบร้อยแล้ว');

            // Re-animate counters
            if (window.adminStatsManager) {
                window.adminStatsManager.animateCounters();
            }
        }, 1500);
    };

    window.changePeriod = function (period) {
        const periodText = {
            'today': 'วันนี้',
            'week': '7 วันที่แล้ว',
            'month': '30 วันที่แล้ว',
            'year': 'ปีนี้'
        };

        window.AdminUtils.showLoading(`กำลังโหลดข้อมูล${periodText[period]}...`);

        // Update dropdown text
        const dropdownBtn = document.querySelector('.dropdown-toggle');
        if (dropdownBtn) {
            dropdownBtn.innerHTML = `<i class="fas fa-calendar me-1"></i>${periodText[period]}`;
        }

        // Simulate API call
        setTimeout(() => {
            window.AdminUtils.hideLoading();
            window.AdminUtils.notify('info', `แสดงข้อมูล${periodText[period]}`);
        }, 1000);
    };

    // HTMX Integration
    if (typeof htmx !== 'undefined') {
        // Show loading on HTMX requests
        document.body.addEventListener('htmx:beforeRequest', function (evt) {
            window.AdminUtils.showLoading();
        });

        // Hide loading when HTMX request completes
        document.body.addEventListener('htmx:afterRequest', function (evt) {
            window.AdminUtils.hideLoading();
        });

        // Handle HTMX errors
        document.body.addEventListener('htmx:responseError', function (evt) {
            window.AdminUtils.hideLoading();
            window.AdminUtils.notify('error', 'เกิดข้อผิดพลาดในการเชื่อมต่อ');
        });

        // Handle successful responses
        document.body.addEventListener('htmx:afterSwap', function (evt) {
            // Re-initialize components after HTMX swap
            if (window.adminStatsManager) {
                window.adminStatsManager.animateCounters();
            }
        });
    }

    // Initialize everything when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        // Initialize all managers
        window.adminThemeManager = new AdminThemeManager();
        window.adminSidebar = new AdminSidebar();
        window.adminLoadingManager = new AdminLoadingManager();
        window.adminNavManager = new AdminNavManager();
        window.adminStatsManager = new AdminStatsManager();
        window.touchGestureManager = new TouchGestureManager();

        // Initialize data tables
        const tables = document.querySelectorAll('.admin-table');
        tables.forEach((table, index) => {
            new AdminDataTable(`#${table.id || 'table-' + index}`);
        });

        // Initialize forms
        AdminForm.init();

        // Add smooth scrolling to navigation links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });

        // Auto-hide alerts after 5 seconds
        setTimeout(() => {
            const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
            alerts.forEach(alert => {
                if (alert.querySelector('.btn-close')) {
                    alert.querySelector('.btn-close').click();
                }
            });
        }, 5000);

        // Initialize tooltips if Bootstrap is available
        if (typeof bootstrap !== 'undefined') {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }

        console.log('🎉 Admin Dashboard initialized successfully');
    });

    // Handle page visibility change
    document.addEventListener('visibilitychange', function () {
        if (document.hidden) {
            // Page is hidden - pause any animations or polls
            console.log('Admin dashboard hidden');
        } else {
            // Page is visible - resume activities
            console.log('Admin dashboard visible');
        }
    });

    // Export for global access
    window.AdminDashboard = {
        themeManager: () => window.adminThemeManager,
        sidebar: () => window.adminSidebar,
        loading: () => window.adminLoadingManager,
        nav: () => window.adminNavManager,
        stats: () => window.adminStatsManager,
        utils: window.AdminUtils
    };

})();