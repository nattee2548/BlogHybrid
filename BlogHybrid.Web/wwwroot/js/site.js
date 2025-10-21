// ========== Site-wide JavaScript ==========

// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('404alk - Site loaded');

    // Initialize components
    initMobileMenu();
    initUserDropdown();
    initToastFromTempData(); // Auto show toast from TempData
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

// ========== 🎯 UNIVERSAL TOAST SYSTEM (แก้ไขแล้ว) ==========
function showToast(message, type = 'info') {
    // ลบ toast เก่าทั้งหมด
    const oldToasts = document.querySelectorAll('.toast-notification');
    oldToasts.forEach(toast => toast.remove());

    // เลือกสีตาม type
    const colors = {
        success: { bg: '#10b981', icon: 'bi-check-circle-fill' },
        error: { bg: '#ef4444', icon: 'bi-exclamation-triangle-fill' },
        warning: { bg: '#f59e0b', icon: 'bi-exclamation-circle-fill' },
        info: { bg: '#3b82f6', icon: 'bi-info-circle-fill' }
    };

    const config = colors[type] || colors.info;

    // สร้าง toast
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        min-width: 300px;
        max-width: 450px;
        padding: 1rem 1.25rem;
        background: ${config.bg};
        color: white;
        border-radius: 12px;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
        z-index: 99999;
        display: flex;
        align-items: center;
        gap: 0.75rem;
        opacity: 0;
        transform: translateX(100px);
        transition: all 0.3s ease;
        font-size: 0.9rem;
        line-height: 1.4;
    `;

    toast.innerHTML = `
        <i class="bi ${config.icon}" style="font-size: 1.5rem; flex-shrink: 0;"></i>
        <span style="flex: 1;">${message}</span>
        <button onclick="this.parentElement.remove()" style="
            background: transparent;
            border: none;
            color: white;
            font-size: 1.25rem;
            cursor: pointer;
            opacity: 0.8;
            padding: 0;
            width: 24px;
            height: 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 4px;
            transition: opacity 0.2s, background 0.2s;
        " onmouseover="this.style.background='rgba(255,255,255,0.2)'" 
           onmouseout="this.style.background='transparent'">✕</button>
    `;

    document.body.appendChild(toast);

    // Slide in animation
    setTimeout(() => {
        toast.style.opacity = '1';
        toast.style.transform = 'translateX(0)';
    }, 50);

    // Auto remove
    const duration = type === 'error' ? 6000 : 4000;
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100px)';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// ========== 🎯 Init Toast จาก TempData (Auto show on page load) ==========
function initToastFromTempData() {
    const successToast = document.getElementById('successToast');
    const errorToast = document.getElementById('errorToast');
    const infoToast = document.getElementById('infoToast');

    if (successToast) {
        const message = successToast.querySelector('span')?.textContent;
        if (message) showToast(message, 'success');
        successToast.remove();
    }

    if (errorToast) {
        const message = errorToast.querySelector('span')?.textContent;
        if (message) showToast(message, 'error');
        errorToast.remove();
    }

    if (infoToast) {
        const message = infoToast.querySelector('span')?.textContent;
        if (message) showToast(message, 'info');
        infoToast.remove();
    }
}

// ========== Mobile Responsive Styles ==========
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { opacity: 0; transform: translateX(100px); }
        to { opacity: 1; transform: translateX(0); }
    }
    @keyframes slideOut {
        from { opacity: 1; transform: translateX(0); }
        to { opacity: 0; transform: translateX(100px); }
    }
    @media (max-width: 640px) {
        .toast-notification {
            right: 10px !important;
            left: 10px !important;
            top: 10px !important;
            min-width: auto !important;
            max-width: none !important;
        }
    }
`;
document.head.appendChild(style);