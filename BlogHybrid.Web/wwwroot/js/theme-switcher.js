// ================================================
// Theme Switcher - Simplified Icon Button Version
// ================================================

(function() {
    'use strict';

    // Theme constants
    const THEMES = {
        LIGHT: 'light',
        DARK: 'dark',
        AUTO: 'auto'
    };

    const STORAGE_KEY = 'main-theme-preference';
    const THEME_ATTR = 'data-theme';

    class ThemeSwitcher {
        constructor() {
            this.currentTheme = this.getStoredTheme() || THEMES.LIGHT;
            this.systemPreference = this.getSystemPreference();
            
            // Initialize theme on page load (without animation)
            this.applyTheme(this.currentTheme, false);
            
            // Listen for system preference changes
            this.listenToSystemPreference();
            
            // Setup event listeners
            this.setupEventListeners();
        }

        /**
         * Get system color scheme preference
         */
        getSystemPreference() {
            return window.matchMedia('(prefers-color-scheme: dark)').matches 
                ? THEMES.DARK 
                : THEMES.LIGHT;
        }

        /**
         * Get stored theme preference
         */
        getStoredTheme() {
            try {
                return localStorage.getItem(STORAGE_KEY);
            } catch (e) {
                console.warn('localStorage not available:', e);
                return null;
            }
        }

        /**
         * Save theme preference
         */
        savePreference(theme) {
            try {
                localStorage.setItem(STORAGE_KEY, theme);
            } catch (e) {
                console.warn('Could not save theme preference:', e);
            }
        }

        /**
         * Listen to system preference changes
         */
        listenToSystemPreference() {
            const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
            
            const handleChange = (e) => {
                this.systemPreference = e.matches ? THEMES.DARK : THEMES.LIGHT;
                
                // If user has auto preference, update theme
                if (this.getStoredTheme() === THEMES.AUTO) {
                    this.applyTheme(THEMES.AUTO);
                }
            };

            // Modern browsers
            if (mediaQuery.addEventListener) {
                mediaQuery.addEventListener('change', handleChange);
            } else if (mediaQuery.addListener) {
                // Fallback for older browsers
                mediaQuery.addListener(handleChange);
            }
        }

        /**
         * Setup event listeners for theme toggle buttons
         */
        setupEventListeners() {
            // Icon button toggle
            const toggleButton = document.getElementById('themeToggle');
            if (toggleButton) {
                toggleButton.addEventListener('click', () => this.toggleTheme());
            }

            // Any other toggle buttons
            document.querySelectorAll('[data-theme-toggle]').forEach(button => {
                button.addEventListener('click', () => this.toggleTheme());
            });

            // Theme selection buttons (if any)
            document.querySelectorAll('[data-theme]').forEach(button => {
                button.addEventListener('click', (e) => {
                    const theme = e.currentTarget.getAttribute('data-theme');
                    this.setTheme(theme);
                });
            });
        }

        /**
         * Apply theme to document
         */
        applyTheme(theme, animate = true) {
            // Add transition class for smooth animation
            if (animate) {
                document.documentElement.classList.add('theme-transitioning');
            }

            // Determine actual theme to apply
            let actualTheme = theme;
            if (theme === THEMES.AUTO) {
                actualTheme = this.systemPreference;
            }

            // Set theme attribute
            document.documentElement.setAttribute(THEME_ATTR, actualTheme);
            this.currentTheme = actualTheme;

            // Save preference
            this.savePreference(theme);

            // Update UI
            this.updateUI(actualTheme);

            // Remove transition class after animation
            if (animate) {
                setTimeout(() => {
                    document.documentElement.classList.remove('theme-transitioning');
                }, 300);
            }

            // Update button icon
            this.updateIcon(actualTheme);
        }

        /**
         * Toggle between light and dark themes
         */
        toggleTheme() {
            const currentTheme = document.documentElement.getAttribute(THEME_ATTR);
            const newTheme = currentTheme === THEMES.DARK ? THEMES.LIGHT : THEMES.DARK;
            this.applyTheme(newTheme);
        }

        /**
         * Set specific theme
         */
        setTheme(theme) {
            if (Object.values(THEMES).includes(theme)) {
                this.applyTheme(theme);
            }
        }

        /**
         * Update UI elements to reflect current theme
         */
        updateUI(actualTheme) {
            // Update toggle buttons
            const toggleButtons = document.querySelectorAll('[data-theme-toggle], #themeToggle');
            toggleButtons.forEach(button => {
                const isDark = actualTheme === THEMES.DARK;
                button.setAttribute('aria-checked', isDark);
                
                // Update aria-label
                button.setAttribute('aria-label', 
                    isDark ? 'สลับเป็นโหมดสว่าง' : 'สลับเป็นโหมดมืด'
                );
            });
        }

        /**
         * Update icon in theme toggle button
         */
        updateIcon(actualTheme) {
            const toggleButton = document.getElementById('themeToggle');
            if (!toggleButton) return;

            const isDark = actualTheme === THEMES.DARK;
            
            // Sun icon (Light mode)
            const sunIcon = `<svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
                <path d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z"/>
            </svg>`;
            
            // Moon icon (Dark mode)
            const moonIcon = `<svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
                <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z"/>
            </svg>`;
            
            toggleButton.innerHTML = isDark ? sunIcon : moonIcon;
        }
    }

    // Initialize theme switcher when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            window.themeSwitcher = new ThemeSwitcher();
        });
    } else {
        window.themeSwitcher = new ThemeSwitcher();
    }

})();
