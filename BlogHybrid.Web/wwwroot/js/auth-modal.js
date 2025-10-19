/* ========================================
   AUTH MODALS - JavaScript Controller
   ======================================== */

// เปิด Login Modal
function openLoginModal() {
    closeAllAuthModals();
    const modal = document.getElementById('loginModal');
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';

        // Focus ที่ input แรก
        setTimeout(() => {
            const firstInput = modal.querySelector('input');
            if (firstInput) firstInput.focus();
        }, 300);
    }
}

// เปิด Register Modal
function openRegisterModal() {
    closeAllAuthModals();
    const modal = document.getElementById('registerModal');
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';

        // Focus ที่ input แรก
        setTimeout(() => {
            const firstInput = modal.querySelector('input');
            if (firstInput) firstInput.focus();
        }, 300);
    }
}

// ปิด Modal ที่ระบุ
function closeAuthModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// ปิด Modal ทั้งหมด
function closeAllAuthModals() {
    document.querySelectorAll('.auth-modal-overlay').forEach(modal => {
        modal.classList.remove('active');
    });
    document.body.style.overflow = '';
}

// สลับระหว่าง Login <-> Register
function switchAuthModal(closeId, openId) {
    closeAuthModal(closeId);
    setTimeout(() => {
        if (openId === 'loginModal') {
            openLoginModal();
        } else if (openId === 'registerModal') {
            openRegisterModal();
        }
    }, 200);
}

// Toggle แสดง/ซ่อนรหัสผ่าน
function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);
    const icon = button.querySelector('i');

    if (input && icon) {
        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.remove('bi-eye');
            icon.classList.add('bi-eye-slash');
        } else {
            input.type = 'password';
            icon.classList.remove('bi-eye-slash');
            icon.classList.add('bi-eye');
        }
    }
}

// Event Listeners
document.addEventListener('DOMContentLoaded', function () {

    // ปิด Modal เมื่อคลิกที่ Overlay
    document.querySelectorAll('.auth-modal-overlay').forEach(overlay => {
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                closeAuthModal(overlay.id);
            }
        });
    });

    // ปิด Modal เมื่อกด ESC
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllAuthModals();
        }
    });

    // Enter Key Navigation - Login Form
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        const loginInputs = loginForm.querySelectorAll('input[type="text"], input[type="email"], input[type="password"]');
        loginInputs.forEach((input, index) => {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();

                    // ถ้าเป็น input สุดท้าย → submit
                    if (index === loginInputs.length - 1) {
                        loginForm.submit();
                    } else {
                        // ไปยัง input ถัดไป
                        loginInputs[index + 1].focus();
                    }
                }
            });
        });
    }

    // Enter Key Navigation - Register Form
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        const registerInputs = registerForm.querySelectorAll('input[type="text"], input[type="email"], input[type="password"]');
        registerInputs.forEach((input, index) => {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();

                    // ข้าม checkbox AcceptTerms
                    const nextIndex = index + 1;
                    if (nextIndex < registerInputs.length) {
                        registerInputs[nextIndex].focus();
                    } else {
                        // ถ้าถึง input สุดท้ายแล้ว ให้ตรวจสอบ checkbox
                        const acceptTerms = registerForm.querySelector('#registerAcceptTerms');
                        if (acceptTerms && acceptTerms.checked) {
                            registerForm.submit();
                        } else {
                            // Focus ที่ checkbox ถ้ายังไม่ tick
                            if (acceptTerms) {
                                acceptTerms.focus();
                            }
                        }
                    }
                }
            });
        });

        // Validate Confirm Password
        const password = registerForm.querySelector('input[name="Password"]');
        const confirmPassword = registerForm.querySelector('input[name="ConfirmPassword"]');

        if (password && confirmPassword) {
            confirmPassword.addEventListener('input', function () {
                if (this.value !== password.value) {
                    this.setCustomValidity('รหัสผ่านไม่ตรงกัน');
                } else {
                    this.setCustomValidity('');
                }
            });
        }
    }

    // ✨ ตรวจสอบ TempData Flag เพื่อเปิด Modal อัตโนมัติ
    // ใช้ร่วมกับ data attributes ใน Layout
    if (document.body.dataset.openLoginModal === 'true') {
        openLoginModal();
    }

    if (document.body.dataset.openRegisterModal === 'true') {
        openRegisterModal();
    }
});

// Export functions for global access
window.openLoginModal = openLoginModal;
window.openRegisterModal = openRegisterModal;
window.closeAuthModal = closeAuthModal;
window.switchAuthModal = switchAuthModal;
window.togglePassword = togglePassword;