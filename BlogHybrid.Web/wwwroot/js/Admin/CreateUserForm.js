// BlogHybrid.Web/wwwroot/js/Admin/CreateUserForm.js
class CreateUserForm {
    constructor() {
        this.isNavigating = false;
        this.init();
    }

    init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.setupAll();
            });
        } else {
            this.setupAll();
        }
    }

    setupAll() {
        console.log('CreateUserForm: Initializing...');

        this.preventFormSubmitOnEnter();
        this.setupFieldNavigation();
        this.setupPasswordToggleKeyboard();
        this.setupCheckboxKeyboard();
        this.setupFormValidation();
        this.addEnterHints();

        console.log('CreateUserForm: Initialized successfully');
    }

    preventFormSubmitOnEnter() {
        const form = document.querySelector('.admin-form');
        if (!form) {
            console.warn('CreateUserForm: Form not found');
            return;
        }

        form.addEventListener('submit', (e) => {
            if (this.isNavigating) {
                console.log('CreateUserForm: Preventing submit during navigation');
                e.preventDefault();
                e.stopPropagation();
                this.isNavigating = false;
                return false;
            }
        }, true);

        form.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' || e.keyCode === 13) {
                const target = e.target;

                if (target.tagName === 'TEXTAREA') {
                    return true;
                }

                if (target.type === 'submit' ||
                    (target.tagName === 'BUTTON' && target.getAttribute('type') === 'submit')) {
                    return true;
                }

                console.log('CreateUserForm: Preventing Enter submit from', target.name || target.id);
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        }, true);
    }

    setupFieldNavigation() {
        // ใช้ name attribute เพราะ ASP.NET Core generate id ที่แตกต่างกัน
        const fieldSelectors = [
            '[name="Email"]',
            '[name="FirstName"]',
            '[name="LastName"]',
            '[name="PhoneNumber"]',
            '[name="Password"]',
            '[name="ConfirmPassword"]',
            '[name="IsActive"]',
            '[name="EmailConfirmed"]'
        ];

        console.log('CreateUserForm: Setting up field navigation');

        const fields = fieldSelectors.map(selector => document.querySelector(selector)).filter(f => f);

        console.log('CreateUserForm: Found', fields.length, 'fields:', fields.map(f => f.name));

        fields.forEach((field, index) => {
            field.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    console.log('CreateUserForm: Enter on', field.name, '- Moving to next');

                    e.preventDefault();
                    e.stopPropagation();
                    e.stopImmediatePropagation();

                    this.isNavigating = true;

                    setTimeout(() => {
                        this.focusNextField(fields, index);
                        this.isNavigating = false;
                    }, 10);

                    return false;
                }
            }, true);
        });

        this.setupRoleNavigation();
    }

    focusNextField(fields, currentIndex) {
        const nextIndex = currentIndex + 1;

        if (nextIndex < fields.length) {
            const nextField = fields[nextIndex];
            nextField.focus();

            if (nextField.select &&
                (nextField.type === 'text' || nextField.type === 'email' ||
                    nextField.type === 'tel' || nextField.type === 'password')) {
                setTimeout(() => nextField.select(), 10);
            }

            console.log('CreateUserForm: Focused on', nextField.name);
        } else {
            this.focusRoleSection();
        }
    }

    focusRoleSection() {
        const firstRoleCheckbox = document.querySelector('input[name="SelectedRoles"]');
        if (firstRoleCheckbox) {
            console.log('CreateUserForm: Moving to role section');
            firstRoleCheckbox.focus();
        } else {
            console.log('CreateUserForm: No roles, moving to submit');
            this.focusSubmitButton();
        }
    }

    setupRoleNavigation() {
        const roleCheckboxes = document.querySelectorAll('input[name="SelectedRoles"]');

        console.log('CreateUserForm: Setup', roleCheckboxes.length, 'role checkboxes');

        roleCheckboxes.forEach((checkbox, index) => {
            checkbox.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' || e.keyCode === 13) {
                    e.preventDefault();
                    e.stopPropagation();

                    this.isNavigating = true;
                    checkbox.checked = !checkbox.checked;

                    setTimeout(() => {
                        const nextIndex = index + 1;
                        if (nextIndex < roleCheckboxes.length) {
                            roleCheckboxes[nextIndex].focus();
                        } else {
                            this.focusSubmitButton();
                        }
                        this.isNavigating = false;
                    }, 10);

                    return false;
                }

                if (e.key === 'ArrowDown' || e.key === 'ArrowRight') {
                    e.preventDefault();
                    roleCheckboxes[(index + 1) % roleCheckboxes.length].focus();
                }

                if (e.key === 'ArrowUp' || e.key === 'ArrowLeft') {
                    e.preventDefault();
                    const prevIndex = index === 0 ? roleCheckboxes.length - 1 : index - 1;
                    roleCheckboxes[prevIndex].focus();
                }

                if (e.key === ' ' || e.keyCode === 32) {
                    e.preventDefault();
                    checkbox.checked = !checkbox.checked;
                }
            }, true);
        });
    }

    focusSubmitButton() {
        const submitBtn = document.querySelector('button[type="submit"]');
        if (submitBtn) {
            console.log('CreateUserForm: Focusing submit button');
            submitBtn.focus();
        }
    }

    setupPasswordToggleKeyboard() {
        ['Password', 'ConfirmPassword'].forEach(name => {
            const input = document.querySelector(`[name="${name}"]`);
            if (!input) return;

            input.addEventListener('keydown', (e) => {
                if (e.altKey && (e.key === 'v' || e.key === 'V')) {
                    e.preventDefault();
                    const inputId = input.id || input.name;
                    if (typeof togglePasswordVisibility === 'function') {
                        togglePasswordVisibility(inputId);
                    }
                }
            });
        });
    }

    setupCheckboxKeyboard() {
        ['IsActive', 'EmailConfirmed'].forEach(name => {
            const checkbox = document.querySelector(`[name="${name}"]`);
            if (!checkbox) return;

            checkbox.addEventListener('keydown', (e) => {
                if (e.key === ' ' || e.keyCode === 32) {
                    e.preventDefault();
                    checkbox.checked = !checkbox.checked;
                }
            });
        });
    }

    setupFormValidation() {
        const form = document.querySelector('.admin-form');
        if (!form) return;

        form.addEventListener('submit', (e) => {
            if (this.isNavigating) {
                e.preventDefault();
                return false;
            }

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>กำลังสร้างผู้ใช้...';

                setTimeout(() => {
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = '<i class="fas fa-save me-2"></i>สร้างผู้ใช้';
                    }
                }, 5000);
            }
        });

        this.setupFieldValidation();
    }

    setupFieldValidation() {
        const emailField = document.querySelector('[name="Email"]');
        if (emailField) {
            emailField.addEventListener('blur', () => {
                const email = emailField.value.trim();
                if (email && !this.isValidEmail(email)) {
                    this.showFieldHint(emailField, 'รูปแบบอีเมลไม่ถูกต้อง', 'error');
                } else if (email) {
                    this.showFieldHint(emailField, 'รูปแบบอีเมลถูกต้อง', 'success');
                } else {
                    this.clearFieldHint(emailField);
                }
            });
        }

        const passwordField = document.querySelector('[name="Password"]');
        const confirmPasswordField = document.querySelector('[name="ConfirmPassword"]');

        if (passwordField) {
            passwordField.addEventListener('input', () => {
                const password = passwordField.value;
                if (password.length > 0 && password.length < 6) {
                    this.showFieldHint(passwordField, 'รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร', 'warning');
                } else if (password.length >= 6) {
                    this.showFieldHint(passwordField, 'ความแข็งแกร่ง: ' + this.getPasswordStrength(password), 'info');
                } else {
                    this.clearFieldHint(passwordField);
                }

                if (confirmPasswordField && confirmPasswordField.value) {
                    this.validatePasswordConfirm();
                }
            });
        }

        if (confirmPasswordField) {
            confirmPasswordField.addEventListener('input', () => {
                this.validatePasswordConfirm();
            });
        }

        const phoneField = document.querySelector('[name="PhoneNumber"]');
        if (phoneField) {
            phoneField.addEventListener('input', (e) => {
                let value = e.target.value.replace(/\D/g, '');
                if (value.length > 0) {
                    if (value.length <= 3) {
                        value = value;
                    } else if (value.length <= 6) {
                        value = value.substring(0, 3) + '-' + value.substring(3);
                    } else {
                        value = value.substring(0, 3) + '-' + value.substring(3, 6) + '-' + value.substring(6, 10);
                    }
                }
                e.target.value = value;
            });
        }
    }

    validatePasswordConfirm() {
        const passwordField = document.querySelector('[name="Password"]');
        const confirmPasswordField = document.querySelector('[name="ConfirmPassword"]');

        if (!passwordField || !confirmPasswordField) return;

        const password = passwordField.value;
        const confirmPassword = confirmPasswordField.value;

        if (confirmPassword.length > 0) {
            if (password === confirmPassword) {
                this.showFieldHint(confirmPasswordField, 'รหัสผ่านตรงกัน', 'success');
            } else {
                this.showFieldHint(confirmPasswordField, 'รหัสผ่านไม่ตรงกัน', 'error');
            }
        } else {
            this.clearFieldHint(confirmPasswordField);
        }
    }

    isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    getPasswordStrength(password) {
        let strength = 0;
        if (password.length >= 8) strength++;
        if (/[a-z]/.test(password)) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[^A-Za-z0-9]/.test(password)) strength++;

        const levels = ['อ่อนมาก', 'อ่อน', 'ปานกลาง', 'แข็งแกร่ง', 'แข็งแกร่งมาก'];
        return levels[strength] || 'อ่อนมาก';
    }

    showFieldHint(field, message, type = 'info') {
        this.clearFieldHint(field);

        const hintElement = document.createElement('div');
        hintElement.className = `field-hint hint-${type} mt-1`;
        hintElement.innerHTML = `<small><i class="fas fa-${this.getHintIcon(type)} me-1"></i>${message}</small>`;

        const parent = field.closest('.password-field-group') || field.parentNode;
        parent.appendChild(hintElement);
    }

    clearFieldHint(field) {
        const parent = field.closest('.password-field-group') || field.parentNode;
        const existingHint = parent.querySelector('.field-hint');
        if (existingHint) {
            existingHint.remove();
        }
    }

    getHintIcon(type) {
        const icons = {
            'success': 'check-circle',
            'error': 'exclamation-circle',
            'warning': 'exclamation-triangle',
            'info': 'info-circle'
        };
        return icons[type] || 'info-circle';
    }

    addEnterHints() {
        const roleSection = document.querySelector('.role-selection');
        if (roleSection && !roleSection.querySelector('.keyboard-hint')) {
            const hintElement = document.createElement('div');
            hintElement.className = 'keyboard-hint mt-2';
            hintElement.innerHTML = '<small class="text-muted"><i class="fas fa-keyboard me-1"></i>ใช้ Enter เพื่อเลือก, ลูกศรเพื่อเลื่อน</small>';
            roleSection.appendChild(hintElement);
        }
    }
}

// Initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.userFormInstance = new CreateUserForm();
    });
} else {
    window.userFormInstance = new CreateUserForm();
}

window.CreateUserForm = CreateUserForm;