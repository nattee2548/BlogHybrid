/* ================================================
   Theme Switcher Logic
   - Auto-detect system preference
   - Save user preference to localStorage
   - Smooth theme transitions
   ================================================ */

(function() {
    'use strict';

    const STORAGE_KEY = 'theme'; // ✅ เปลี่ยนเป็น 'theme' เพื่อรักษา backward compatibility
    const THEME_ATTR = 'data-theme';
    const THEMES = {
        LIGHT: 'light',
        DARK: 'dark',
        AUTO: 'auto'
    };

    class ThemeSwitcher {
        constructor() {
            this.currentTheme = THEMES.LIGHT;
            this.systemPreference = this.getSystemPreference();
            this.init();
        }

        /**
         * Initialize theme switcher
         */
        init() {
            // Load saved preference or use auto
            const savedPreference = this.getSavedPreference();
            const themeToApply = savedPreference || THEMES.AUTO;
            
            // Apply theme immediately (before page render to prevent flash)
            this.applyTheme(themeToApply, false);
            
            // Setup event listeners
            this.setupEventListeners();
            
            // Listen for system preference changes
            this.watchSystemPreference();
        }

        /**
         * Get system color scheme preference
         */
        getSystemPreference() {
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                return THEMES.DARK;
            }
            return THEMES.LIGHT;
        }

        /**
         * Get saved theme preference from localStorage
         */
        getSavedPreference() {
            try {
                return localStorage.getItem(STORAGE_KEY);
            } catch (e) {
                console.warn('localStorage not available:', e);
                return null;
            }
        }

        /**
         * Save theme preference to localStorage
         */
        savePreference(theme) {
            try {
                localStorage.setItem(STORAGE_KEY, theme);
            } catch (e) {
                console.warn('Failed to save theme preference:', e);
            }
        }

        /**
         * Apply theme to document
         * @param {string} theme - Theme to apply (light, dark, or auto)
         * @param {boolean} animate - Whether to animate the transition
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
            this.updateUI(theme);

            // Remove transition class after animation
            if (animate) {
                setTimeout(() => {
                    document.documentElement.classList.remove('theme-transitioning');
                }, 300);
            }

            // Dispatch custom event
            this.dispatchThemeChangeEvent(actualTheme, theme);
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
        updateUI(preferenceTheme) {
            // Update toggle buttons
            const toggleButtons = document.querySelectorAll('.theme-toggle');
            toggleButtons.forEach(button => {
                const isDark = this.currentTheme === THEMES.DARK;
                button.setAttribute('aria-checked', isDark);
            });

            // Update theme buttons (if using button group)
            document.querySelectorAll('.theme-button').forEach(button => {
                const buttonTheme = button.getAttribute('data-theme');
                if (buttonTheme === preferenceTheme) {
                    button.classList.add('active');
                } else {
                    button.classList.remove('active');
                }
            });

            // Update theme options (if using dropdown)
            document.querySelectorAll('.theme-option').forEach(option => {
                const optionTheme = option.getAttribute('data-theme');
                if (optionTheme === preferenceTheme) {
                    option.classList.add('active');
                } else {
                    option.classList.remove('active');
                }
            });
        }

        /**
         * Setup event listeners for theme controls
         */
        setupEventListeners() {
            // Toggle switches
            document.addEventListener('click', (e) => {
                if (e.target.closest('.theme-toggle')) {
                    this.toggleTheme();
                }
            });

            // Theme buttons
            document.addEventListener('click', (e) => {
                const themeButton = e.target.closest('.theme-button');
                if (themeButton) {
                    const theme = themeButton.getAttribute('data-theme');
                    this.setTheme(theme);
                }
            });

            // Theme dropdown
            document.addEventListener('click', (e) => {
                const themeOption = e.target.closest('.theme-option');
                if (themeOption) {
                    const theme = themeOption.getAttribute('data-theme');
                    this.setTheme(theme);
                    // Close dropdown
                    const dropdown = themeOption.closest('.theme-dropdown-menu');
                    if (dropdown) {
                        dropdown.classList.remove('active');
                    }
                }

                // Toggle dropdown
                const dropdownButton = e.target.closest('.theme-dropdown-button');
                if (dropdownButton) {
                    const dropdown = dropdownButton.nextElementSibling;
                    if (dropdown) {
                        dropdown.classList.toggle('active');
                    }
                }
            });

            // Close dropdown when clicking outside
            document.addEventListener('click', (e) => {
                if (!e.target.closest('.theme-dropdown')) {
                    document.querySelectorAll('.theme-dropdown-menu.active')
                        .forEach(dropdown => dropdown.classList.remove('active'));
                }
            });
        }

        /**
         * Watch for system preference changes
         */
        watchSystemPreference() {
            if (window.matchMedia) {
                const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
                
                // Modern browsers
                if (darkModeQuery.addEventListener) {
                    darkModeQuery.addEventListener('change', (e) => {
                        this.systemPreference = e.matches ? THEMES.DARK : THEMES.LIGHT;
                        
                        // Only apply if user is using auto theme
                        const savedPreference = this.getSavedPreference();
                        if (savedPreference === THEMES.AUTO || !savedPreference) {
                            this.applyTheme(THEMES.AUTO);
                        }
                    });
                }
                // Legacy browsers
                else if (darkModeQuery.addListener) {
                    darkModeQuery.addListener((e) => {
                        this.systemPreference = e.matches ? THEMES.DARK : THEMES.LIGHT;
                        const savedPreference = this.getSavedPreference();
                        if (savedPreference === THEMES.AUTO || !savedPreference) {
                            this.applyTheme(THEMES.AUTO);
                        }
                    });
                }
            }
        }

        /**
         * Dispatch custom event when theme changes
         */
        dispatchThemeChangeEvent(actualTheme, preferenceTheme) {
            const event = new CustomEvent('themechange', {
                detail: {
                    theme: actualTheme,
                    preference: preferenceTheme,
                    timestamp: Date.now()
                }
            });
            document.dispatchEvent(event);
        }

        /**
         * Get current theme
         */
        getCurrentTheme() {
            return this.currentTheme;
        }

        /**
         * Get current preference (including 'auto')
         */
        getCurrentPreference() {
            return this.getSavedPreference() || THEMES.AUTO;
        }
    }

    // Initialize theme switcher
    const themeSwitcher = new ThemeSwitcher();

    // Expose to global scope for manual control
    window.ThemeSwitcher = themeSwitcher;

    // Add helper functions to window
    window.setTheme = (theme) => themeSwitcher.setTheme(theme);
    window.toggleTheme = () => themeSwitcher.toggleTheme();
    window.getCurrentTheme = () => themeSwitcher.getCurrentTheme();

    // Log initialization (can be removed in production)
    console.log('🎨 Theme Switcher initialized');
    console.log('Current theme:', themeSwitcher.getCurrentTheme());
    console.log('Preference:', themeSwitcher.getCurrentPreference());

})();
