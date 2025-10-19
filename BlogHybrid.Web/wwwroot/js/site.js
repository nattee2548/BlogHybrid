// ========== Site-wide JavaScript ==========

// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('404alk - Site loaded');

    // Initialize components
    initMobileMenu();
    initUserDropdown();
});

// ========== Mobile Menu Toggle ==========
function initMobileMenu() {
    const menuToggle = document.querySelector('.menu-toggle');
    const navbarMenu = document.querySelector('.navbar-menu');

    if (menuToggle && navbarMenu) {
        menuToggle.addEventListener('click', function () {
            navbarMenu.classList.toggle('active');
        });
    }
}

// ========== User Dropdown ==========
function initUserDropdown() {
    const userMenu = document.querySelector('.user-menu');

    if (userMenu) {
        // Close dropdown when clicking outside
        document.addEventListener('click', function (event) {
            if (!userMenu.contains(event.target)) {
                const dropdown = userMenu.querySelector('.user-dropdown');
                if (dropdown) {
                    dropdown.style.display = 'none';
                }
            }
        });
    }
}

// ========== Helper: Get CSRF Token ==========
function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

// ========== Helper: Show Toast Message (with close button) ==========
function showToast(message, type = 'info') {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    // เลือกสี
    const bgColor = type === 'success' ? '#10b981' :
        type === 'error' ? '#ef4444' :
            '#3b82f6';

    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 1rem 1.5rem;
        padding-right: 2.5rem;
        background: ${bgColor};
        color: white;
        border-radius: 8px;
        box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.1);
        z-index: 9999;
        animation: slideIn 0.3s ease;
        max-width: 400px;
    `;

    // สร้าง HTML
    toast.innerHTML = `
        ${message}
        <button onclick="this.parentElement.remove()" style="
            position: absolute;
            top: 8px;
            right: 8px;
            background: transparent;
            border: none;
            color: white;
            font-size: 18px;
            line-height: 1;
            cursor: pointer;
            opacity: 0.7;
            padding: 4px 8px;
        ">✕</button>
    `;

    document.body.appendChild(toast);

    // Auto remove after 5 seconds
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

// ========== Helper: Debounce Function ==========
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

// ========== Helper: Format Date ==========
function formatDate(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'เมื่อสักครู่';
    if (diffMins < 60) return `${diffMins} นาทีที่แล้ว`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} ชั่วโมงที่แล้ว`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays} วันที่แล้ว`;

    return date.toLocaleDateString('th-TH', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// ========== Add CSS Animations ==========
if (!document.querySelector('#toast-animations')) {
    const style = document.createElement('style');
    style.id = 'toast-animations';
    style.textContent = `
        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateX(100%);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes slideOut {
            from {
                opacity: 1;
                transform: translateX(0);
            }
            to {
                opacity: 0;
                transform: translateX(100%);
            }
        }
    `;
    document.head.appendChild(style);
}

// ========== Export for module usage ==========
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        getAntiForgeryToken,
        showToast,
        debounce,
        formatDate
    };
}