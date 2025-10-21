// ================================================
// USER THEME SWITCHER
// เฉพาะ User เท่านั้น
// ================================================

(function () {
    'use strict';

    // ตรวจสอบว่าไม่ใช่ Admin
    if (document.body.classList.contains('admin-body')) {
        return; // ไม่ทำงานถ้าเป็น Admin
    }

    const STORAGE_KEY = 'theme'; // ใช้ key เดิม
    const THEME_ATTR = 'data-theme';
    const THEMES = {
        LIGHT: 'light',
        DARK: 'dark'
    };

    class UserThemeSwitcher {
        constructor() {
            this.currentTheme = THEMES.LIGHT;
            this.init();
        }

        init() {
            const savedTheme = this.getSavedTheme();
            const themeToApply = savedTheme || THEMES.LIGHT;
            this.applyTheme(themeToApply, false);
            this.setupEventListeners();
            console.log('✓ User theme switcher ready');
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
                console.warn('Failed to save user theme');
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
            console.log('🎨 User theme:', newTheme);
        }

        updateUI() {
            const themeIcon = document.getElementById('themeIcon');
            if (themeIcon) {
                if (this.currentTheme === THEMES.DARK) {
                    // Dark mode - แสดงไอคอนดวงจันทร์
                    themeIcon.innerHTML = '<path fill-rule="evenodd" d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" clip-rule="evenodd" />';
                } else {
                    // Light mode - แสดงไอคอนดวงอาทิตย์
                    themeIcon.innerHTML = '<path fill-rule="evenodd" d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z" clip-rule="evenodd" />';
                }
            }

            const toggleButtons = document.querySelectorAll('.modern-theme-toggle, .theme-toggle');
            toggleButtons.forEach(button => {
                const isDark = this.currentTheme === THEMES.DARK;
                button.setAttribute('aria-checked', isDark);
            });
        }

        setupEventListeners() {
            // ไม่ต้องใช้ onclick เพราะจะใช้ผ่าน global function
        }
    }

    // Initialize
    const themeSwitcher = new UserThemeSwitcher();

    // Export global function สำหรับ onclick="toggleTheme()"
    window.toggleTheme = function () {
        themeSwitcher.toggleTheme();
    };
})();