/* ========================================
   AUTH MODALS - JavaScript with AJAX
   ======================================== */

// เปิด Login Modal
function openLoginModal() {
    closeAllAuthModals();
    const modal = document.getElementById('loginModal');
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
        setTimeout(() => {
            const firstInput = modal.querySelector('input');
            if (firstInput) firstInput.focus();
        }, 100);
    }
}

// เปิด Register Modal
function openRegisterModal() {
    closeAllAuthModals();
    const modal = document.getElementById('registerModal');
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
        setTimeout(() => {
            const firstInput = modal.querySelector('input');
            if (firstInput) firstInput.focus();
        }, 100);
    }
}

// ปิด Modal
function closeAuthModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// ปิดทั้งหมด
function closeAllAuthModals() {
    document.querySelectorAll('.auth-modal-overlay').forEach(modal => {
        modal.classList.remove('active');
    });
    document.body.style.overflow = '';
}

// สลับแบบ Smooth
function switchAuthModal(fromId, toId) {
    const fromModal = document.getElementById(fromId);
    const toModal = document.getElementById(toId);

    if (!fromModal || !toModal) return;

    const fromContent = fromModal.querySelector('.auth-modal');
    const toContent = toModal.querySelector('.auth-modal');

    fromContent.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
    fromContent.style.transform = 'translateX(-50px)';
    fromContent.style.opacity = '0';

    setTimeout(() => {
        fromModal.classList.remove('active');
        fromContent.style.transform = '';
        fromContent.style.opacity = '';

        toContent.style.transform = 'translateX(50px)';
        toContent.style.opacity = '0';
        toModal.classList.add('active');

        setTimeout(() => {
            toContent.style.transition = 'transform 0.3s ease, opacity 0.3s ease';
            toContent.style.transform = 'translateX(0)';
            toContent.style.opacity = '1';

            setTimeout(() => {
                toContent.style.transition = '';
                const firstInput = toModal.querySelector('input');
                if (firstInput) firstInput.focus();
            }, 300);
        }, 50);
    }, 300);
}

// Toggle Password
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

// ✨ แสดง Error Toast
function showErrorToast(message) {
    // ลบ toast เก่า
    const oldToast = document.querySelector('.toast-notification');
    if (oldToast) oldToast.remove();

    const toast = document.createElement('div');
    toast.className = 'toast-notification toast-error';
    toast.innerHTML = `<i class="bi bi-exclamation-triangle-fill"></i><span>${message}</span>`;
    document.body.appendChild(toast);

    setTimeout(() => toast.classList.add('toast-show'), 100);

    setTimeout(() => {
        toast.classList.remove('toast-show');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

// ✨ แสดง Success Toast
function showSuccessToast(message) {
    const oldToast = document.querySelector('.toast-notification');
    if (oldToast) oldToast.remove();

    const toast = document.createElement('div');
    toast.className = 'toast-notification toast-success';
    toast.innerHTML = `<i class="bi bi-check-circle-fill"></i><span>${message}</span>`;
    document.body.appendChild(toast);

    setTimeout(() => toast.classList.add('toast-show'), 100);

    setTimeout(() => {
        toast.classList.remove('toast-show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Event Listeners
document.addEventListener('DOMContentLoaded', function () {

    // ปิด Modal เมื่อคลิก Overlay
    document.querySelectorAll('.auth-modal-overlay').forEach(overlay => {
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay) {
                closeAuthModal(overlay.id);
            }
        });
    });

    // ปิดเมื่อกด ESC
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAllAuthModals();
        }
    });

    // ✨ AJAX Login Form
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            const formData = new FormData(this);

            // ✨ แก้ Checkbox Remember Me
            const rememberMe = this.querySelector('#loginRememberMe');
            if (rememberMe && rememberMe.checked) {
                formData.set('RememberMe', 'true');
            } else {
                formData.set('RememberMe', 'false');
            }

            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;

            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> กำลังเข้าสู่ระบบ...';

            try {
                const response = await fetch(this.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();

                if (result.success) {
                    showSuccessToast(result.message || 'เข้าสู่ระบบสำเร็จ!');
                    closeAuthModal('loginModal');

                    setTimeout(() => {
                        window.location.href = result.redirectUrl || '/';
                    }, 500);
                } else {
                    showErrorToast(result.message || 'อีเมลหรือรหัสผ่านไม่ถูกต้อง');
                }
            } catch (error) {
                console.error('Login error:', error);
                showErrorToast('เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง');
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        });

        // Enter Key Navigation
        const loginInputs = loginForm.querySelectorAll('input[type="email"], input[type="password"]');
        loginInputs.forEach((input, index) => {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    if (index === loginInputs.length - 1) {
                        loginForm.dispatchEvent(new Event('submit'));
                    } else {
                        loginInputs[index + 1].focus();
                    }
                }
            });
        });
    }

    // ✨ AJAX Register Form
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', async function (e) {
            e.preventDefault();

            // ✨ ตรวจสอบ Checkbox
            const acceptTerms = this.querySelector('#registerAcceptTerms');
            if (!acceptTerms || !acceptTerms.checked) {
                showErrorToast('กรุณายอมรับข้อกำหนดและเงื่อนไข');
                if (acceptTerms) acceptTerms.focus();
                return;
            }

            const formData = new FormData(this);

            // ✨ บังคับให้ Checkbox เป็น true
            if (acceptTerms.checked) {
                formData.set('AcceptTerms', 'true');
            }

            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;

            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> กำลังสร้างบัญชี...';

            try {
                const response = await fetch(this.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const result = await response.json();

                if (result.success) {
                    showSuccessToast(result.message || 'สมัครสมาชิกสำเร็จ!');

                    if (result.openLogin) {
                        setTimeout(() => {
                            switchAuthModal('registerModal', 'loginModal');
                        }, 1000);
                    } else {
                        closeAuthModal('registerModal');
                        setTimeout(() => {
                            window.location.href = result.redirectUrl || '/';
                        }, 500);
                    }
                } else {
                    showErrorToast(result.message || 'ไม่สามารถสมัครสมาชิกได้');
                }
            } catch (error) {
                console.error('Register error:', error);
                showErrorToast('เกิดข้อผิดพลาด กรุณาลองใหม่อีกครั้ง');
            } finally {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        });

        // Enter Key Navigation
        const registerInputs = registerForm.querySelectorAll('input[type="email"], input[type="text"], input[type="password"]');
        registerInputs.forEach((input, index) => {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    if (index === registerInputs.length - 1) {
                        const acceptTerms = registerForm.querySelector('#registerAcceptTerms');
                        if (acceptTerms && acceptTerms.checked) {
                            registerForm.dispatchEvent(new Event('submit'));
                        } else {
                            showErrorToast('กรุณายอมรับข้อกำหนดและเงื่อนไข');
                            if (acceptTerms) acceptTerms.focus();
                        }
                    } else {
                        registerInputs[index + 1].focus();
                    }
                }
            });
        });

        // ✨ Checkbox - ลบ error เมื่อ tick
        const acceptTerms = registerForm.querySelector('#registerAcceptTerms');
        if (acceptTerms) {
            acceptTerms.addEventListener('change', function () {
                console.log('Checkbox changed:', this.checked);
            });
        }

        // Password Match Validation
        const password = registerForm.querySelector('input[name="Password"]');
        const confirmPassword = registerForm.querySelector('input[name="ConfirmPassword"]');

        if (password && confirmPassword) {
            confirmPassword.addEventListener('input', function () {
                if (this.value && this.value !== password.value) {
                    this.setCustomValidity('รหัสผ่านไม่ตรงกัน');
                } else {
                    this.setCustomValidity('');
                }
            });
        }
    }

    // เปิด Modal อัตโนมัติ
    if (document.body.dataset.openLoginModal === 'true') {
        setTimeout(() => openLoginModal(), 100);
    }

    if (document.body.dataset.openRegisterModal === 'true') {
        setTimeout(() => openRegisterModal(), 100);
    }
});

// Export
window.openLoginModal = openLoginModal;
window.openRegisterModal = openRegisterModal;
window.closeAuthModal = closeAuthModal;
window.switchAuthModal = switchAuthModal;
window.togglePassword = togglePassword;