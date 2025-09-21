// BlogHybrid.Web/wwwroot/js/admin.js
// Admin Dashboard JavaScript

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

    // Sidebar Management
    class AdminSidebar {
        constructor() {
            this.sidebar = document.getElementById('adminSidebar');
            this.toggleBtn = document.getElementById('sidebarToggle');
            this.collapsedKey = '404talk-admin-sidebar-collapsed';
            this.init();
        }

        init() {
            const isCollapsed = localStorage.getItem(this.collapsedKey) === 'true';
            if (isCollapsed) {
                this.collapse();
            }

            if (this.toggleBtn) {
                this.toggleBtn.addEventListener('click', () => {
                    this.toggle();
                });
            }

            // Auto-collapse on mobile
            this.handleResize();
            window.addEventListener('resize', () => this.handleResize());
        }

        toggle() {
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
            if (window.innerWidth < 992) {
                this.collapse();
            }
        }
    }

    // Loading Management
    class AdminLoading {
        constructor() {
            this.loadingEl = document.getElementById('adminLoading');
        }

        show(message = 'กำลังโหลด...') {
            if (this.loadingEl) {
                const textEl = this.loadingEl.querySelector('.loading-text');
                if (textEl) textEl.textContent = message;
                this.loadingEl.style.display = 'flex';
            }
        }

        hide() {
            if (this.loadingEl) {
                this.loadingEl.style.display = 'none';
            }
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
            // Basic sorting implementation
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

    // Statistics Counter Animation
    class AdminStats {
        static animateCounters() {
            const counters = document.querySelectorAll('[data-counter]');
            counters.forEach(counter => {
                const target = parseInt(counter.getAttribute('data-counter'));
                const duration = 2000; // 2 seconds
                const increment = target / (duration / 16); // 60fps
                let current = 0;

                const timer = setInterval(() => {
                    current += increment;
                    if (current >= target) {
                        current = target;
                        clearInterval(timer);
                    }
                    counter.textContent = Math.floor(current).toLocaleString();
                }, 16);
            });
        }
    }

    // Initialize everything when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        // Initialize core components
        window.adminTheme = new AdminThemeManager();
        window.adminSidebar = new AdminSidebar();
        window.adminLoading = new AdminLoading();

        // Initialize data tables
        const tables = document.querySelectorAll('.admin-table');
        tables.forEach(table => {
            new AdminDataTable(table);
        });

        // Initialize forms
        AdminForm.init();

        // Animate statistics
        AdminStats.animateCounters();

        // HTMX integration
        if (typeof htmx !== 'undefined') {
            // Show loading on HTMX requests
            document.addEventListener('htmx:beforeRequest', function (evt) {
                window.adminLoading.show();
            });

            document.addEventListener('htmx:afterRequest', function (evt) {
                window.adminLoading.hide();
            });

            // Re-initialize components after HTMX swaps
            document.addEventListener('htmx:afterSwap', function (evt) {
                AdminForm.init();
                AdminStats.animateCounters();
            });
        }

        // Hide loading overlay
        window.adminLoading.hide();
    });

    // Export for global access
    window.AdminThemeManager = AdminThemeManager;
    window.AdminSidebar = AdminSidebar;
    window.AdminLoading = AdminLoading;
    window.AdminDataTable = AdminDataTable;
    window.AdminForm = AdminForm;
    window.AdminStats = AdminStats;

})();