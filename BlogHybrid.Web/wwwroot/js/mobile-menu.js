// ================================================
// MOBILE MENU FUNCTIONALITY
// ================================================

document.addEventListener('DOMContentLoaded', function() {
    initMobileMenu();
    initResponsiveUtilities();
});

// ========== Mobile Menu ==========
function initMobileMenu() {
    const toggleBtn = document.getElementById('mobileMenuToggle');
    const menu = document.getElementById('mobileMenu');
    const overlay = document.getElementById('mobileMenuOverlay');
    
    if (!toggleBtn || !menu || !overlay) {
        console.log('Mobile menu elements not found');
        return;
    }

    // Open/Close menu
    toggleBtn.addEventListener('click', function(e) {
        e.stopPropagation();
        toggleMenu();
    });

    // Close when clicking overlay
    overlay.addEventListener('click', function() {
        closeMenu();
    });

    // Close when clicking menu link
    const menuLinks = menu.querySelectorAll('a');
    menuLinks.forEach(link => {
        link.addEventListener('click', function() {
            closeMenu();
        });
    });

    // Close on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && menu.classList.contains('open')) {
            closeMenu();
        }
    });

    // Prevent body scroll when menu is open
    function toggleMenu() {
        const isOpen = menu.classList.contains('open');
        if (isOpen) {
            closeMenu();
        } else {
            openMenu();
        }
    }

    function openMenu() {
        menu.classList.add('open');
        overlay.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closeMenu() {
        menu.classList.remove('open');
        overlay.classList.remove('open');
        document.body.style.overflow = '';
    }
}

// ========== Responsive Utilities ==========
function initResponsiveUtilities() {
    // Add touch class to body on touch devices
    if ('ontouchstart' in window || navigator.maxTouchPoints > 0) {
        document.body.classList.add('touch-device');
    }

    // Detect viewport size
    updateViewportSize();
    window.addEventListener('resize', debounce(updateViewportSize, 250));

    // Lazy load images
    initLazyLoading();

    // Smooth scroll for anchor links
    initSmoothScroll();
}

function updateViewportSize() {
    const width = window.innerWidth;
    const body = document.body;

    // Remove all viewport classes
    body.classList.remove('viewport-mobile', 'viewport-tablet', 'viewport-desktop');

    // Add appropriate class
    if (width < 768) {
        body.classList.add('viewport-mobile');
    } else if (width < 1024) {
        body.classList.add('viewport-tablet');
    } else {
        body.classList.add('viewport-desktop');
    }
}

// ========== Lazy Loading Images ==========
function initLazyLoading() {
    // Check if browser supports IntersectionObserver
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src || img.src;
                    img.classList.add('loaded');
                    observer.unobserve(img);
                }
            });
        });

        // Observe all images with loading="lazy"
        const lazyImages = document.querySelectorAll('img[loading="lazy"]');
        lazyImages.forEach(img => imageObserver.observe(img));
    }
}

// ========== Smooth Scroll ==========
function initSmoothScroll() {
    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            
            // Skip if it's just "#"
            if (href === '#') return;

            const targetId = href.substring(1);
            const target = document.getElementById(targetId);

            if (target) {
                e.preventDefault();
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });

                // Update URL without jumping
                history.pushState(null, null, href);
            }
        });
    });
}

// ========== Utility Functions ==========

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Throttle function
function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Check if element is in viewport
function isInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}

// Get viewport width
function getViewportWidth() {
    return Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
}

// Get viewport height
function getViewportHeight() {
    return Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
}

// Check if mobile device
function isMobile() {
    return getViewportWidth() < 768;
}

// Check if tablet device
function isTablet() {
    const width = getViewportWidth();
    return width >= 768 && width < 1024;
}

// Check if desktop device
function isDesktop() {
    return getViewportWidth() >= 1024;
}

// ========== Export utility functions ==========
window.ResponsiveUtils = {
    debounce,
    throttle,
    isInViewport,
    getViewportWidth,
    getViewportHeight,
    isMobile,
    isTablet,
    isDesktop
};

// ========== Console log for debugging ==========
console.log('Responsive Framework Loaded');
console.log('Viewport:', getViewportWidth() + 'x' + getViewportHeight());
console.log('Device Type:', isMobile() ? 'Mobile' : isTablet() ? 'Tablet' : 'Desktop');
