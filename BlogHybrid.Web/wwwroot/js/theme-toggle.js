// wwwroot/js/theme-toggle.js

(function () {
    'use strict';

    // Theme management class
    class ThemeManager {
        constructor() {
            this.themeKey = '404talk-theme';
            this.toggleBtn = document.getElementById('theme-toggle');
            this.themeIcon = document.getElementById('theme-icon');

            this.init();
        }

        init() {
            // Load saved theme or default to light
            const savedTheme = this.getSavedTheme();
            this.setTheme(savedTheme);

            // Add event listener to toggle button
            if (this.toggleBtn) {
                this.toggleBtn.addEventListener('click', () => {
                    this.toggleTheme();
                });
            }

            // Listen for system theme changes
            if (window.matchMedia) {
                window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                    if (!this.hasUserPreference()) {
                        this.setTheme(e.matches ? 'dark' : 'light');
                    }
                });
            }
        }

        getSavedTheme() {
            // Check localStorage first
            const saved = localStorage.getItem(this.themeKey);
            if (saved) {
                return saved;
            }

            // Fall back to system preference
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                return 'dark';
            }

            return 'light';
        }

        hasUserPreference() {
            return localStorage.getItem(this.themeKey) !== null;
        }

        setTheme(theme) {
            document.documentElement.setAttribute('data-theme', theme);

            // Update icon
            if (this.themeIcon) {
                if (theme === 'dark') {
                    this.themeIcon.className = 'fas fa-sun';
                    this.toggleBtn.title = 'เปลี่ยนเป็นธีมสว่าง';
                } else {
                    this.themeIcon.className = 'fas fa-moon';
                    this.toggleBtn.title = 'เปลี่ยนเป็นธีมมด';
                }
            }

            // Save to localStorage
            localStorage.setItem(this.themeKey, theme);

            // Dispatch custom event for other components
            window.dispatchEvent(new CustomEvent('themeChanged', {
                detail: { theme: theme }
            }));
        }

        toggleTheme() {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            // Add transition class for smooth animation
            document.body.classList.add('theme-transitioning');

            this.setTheme(newTheme);

            // Remove transition class after animation
            setTimeout(() => {
                document.body.classList.remove('theme-transitioning');
            }, 300);
        }

        getCurrentTheme() {
            return document.documentElement.getAttribute('data-theme') || 'light';
        }
    }

    // Additional theme utilities
    const ThemeUtils = {
        // Check if current theme is dark
        isDarkTheme() {
            return document.documentElement.getAttribute('data-theme') === 'dark';
        },

        // Get appropriate contrast color
        getContrastColor(backgroundColor) {
            // Simple contrast calculation
            const rgb = backgroundColor.match(/\d+/g);
            if (rgb) {
                const brightness = (parseInt(rgb[0]) * 299 + parseInt(rgb[1]) * 587 + parseInt(rgb[2]) * 114) / 1000;
                return brightness > 125 ? '#000000' : '#ffffff';
            }
            return this.isDarkTheme() ? '#ffffff' : '#000000';
        },

        // Apply theme to dynamically created elements
        applyThemeToElement(element, options = {}) {
            const theme = this.isDarkTheme() ? 'dark' : 'light';

            if (options.card) {
                element.style.backgroundColor = theme === 'dark' ? '#2d3748' : '#ffffff';
                element.style.borderColor = theme === 'dark' ? '#495057' : '#dee2e6';
                element.style.color = theme === 'dark' ? '#e9ecef' : '#333';
            }

            if (options.input) {
                element.style.backgroundColor = theme === 'dark' ? '#2d3748' : '#ffffff';
                element.style.borderColor = theme === 'dark' ? '#495057' : '#dee2e6';
                element.style.color = theme === 'dark' ? '#e9ecef' : '#333';
            }
        }
    };

    // Initialize when DOM is loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            window.themeManager = new ThemeManager();
            window.themeUtils = ThemeUtils;
        });
    } else {
        window.themeManager = new ThemeManager();
        window.themeUtils = ThemeUtils;
    }

    // Export for global access
    window.ThemeManager = ThemeManager;
    window.ThemeUtils = ThemeUtils;

})();

// Additional CSS for smooth transitions
const themeTransitionCSS = `
.theme-transitioning,
.theme-transitioning *,
.theme-transitioning *:before,
.theme-transitioning *:after {
    transition: background-color 0.3s ease, 
                color 0.3s ease, 
                border-color 0.3s ease,
                box-shadow 0.3s ease !important;
}
`;

// Inject transition CSS
if (document.head) {
    const style = document.createElement('style');
    style.textContent = themeTransitionCSS;
    document.head.appendChild(style);
}