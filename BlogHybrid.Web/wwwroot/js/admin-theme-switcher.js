// ================================================
// ADMIN THEME SWITCHER
// เฉพาะ Admin เท่านั้น
// ================================================

(function () {
    'use strict';

    // ตรวจสอบว่าเป็น Admin หรือไม่
    if (!document.body.classList.contains('admin-body')) {
        return; // ไม่ทำงานถ้าไม่ใช่ Admin
    }

    const STORAGE_KEY = 'admin-theme'; // ✅ ใช้ key แยกจาก User
    const THEME_ATTR = 'data-theme';
    const THEMES = {
        LIGHT: 'light',
        DARK: 'dark'
    };

    class AdminThemeSwitcher {
        constructor() {
            this.currentTheme = THEMES.LIGHT;
            this.init();
        }

        init() {
            const savedTheme = this.getSavedTheme();
            const themeToApply = savedTheme || THEMES.LIGHT;
            this.applyTheme(themeToApply, false);
            this.setupEventListeners();
            console.log('✓ Admin theme switcher ready');
        }

        getSavedTheme() {
            try {
                return localStorage.getItem(STORAGE_KEY);
            } catch (e) {
                return null;
            }
        }

        saveTheme(theme) {
            try {
                localStorage.setItem(STORAGE_KEY, theme);
            } catch (e) {
                console.warn('Failed to save admin theme');
            }
        }

        applyTheme(theme, animate = true) {
            if (animate) {
                document.documentElement.classList.add('theme-transitioning');
            }

            document.documentElement.setAttribute(THEME_ATTR, theme);
            this.currentTheme = theme;
            this.saveTheme(theme);
            this.updateUI();

            if (animate) {
                setTimeout(() => {
                    document.documentElement.classList.remove('theme-transitioning');
                }, 300);
            }
        }

        toggleTheme() {
            const newTheme = this.currentTheme === THEMES.DARK ? THEMES.LIGHT : THEMES.DARK;
            this.applyTheme(newTheme);
            console.log('🎨 Admin theme:', newTheme);
        }

        updateUI() {
            const toggleButtons = document.querySelectorAll('.theme-toggle');
            toggleButtons.forEach(button => {
                const isDark = this.currentTheme === THEMES.DARK;
                button.setAttribute('aria-checked', isDark);
            });
        }

        setupEventListeners() {
            // ใช้ onclick แทน addEventListener เพื่อไม่ทับกับ User
            const toggleButtons = document.querySelectorAll('.theme-toggle');
            toggleButtons.forEach(button => {
                button.onclick = (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    this.toggleTheme();
                };
            });
        }
    }

    // Initialize
    new AdminThemeSwitcher();
})();