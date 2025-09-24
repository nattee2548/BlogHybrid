// ไฟล์: BlogHybrid.Web/wwwroot/js/admin/EditCategoryForm.js

class EditCategoryForm {
    constructor() {
        this.currentImagePath = null;
        this.lastCheckedSlug = null;
        this.originalData = {};

        // ตรวจสอบ adminNotyf
        if (typeof adminNotyf === 'undefined') {
            window.adminNotyf = {
                success: (message) => { alert('✅ ' + message); console.log('Success:', message); },
                error: (message) => { alert('❌ ' + message); console.error('Error:', message); },
                open: (options) => { alert(options.type + ': ' + options.message); }
            };
        }

        this.init();
    }

    init() {
        // รอให้แน่ใจว่า DOM โหลดเสร็จแล้ว
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.delayedInit();
            });
        } else {
            this.delayedInit();
        }

        this.setupFormValidation();
        this.setupEnterKeyNavigation();
        this.setupColorFieldNavigation();
        this.addTextareaHint();
        this.setupInputListeners();
    }

    delayedInit() {
        setTimeout(() => {
            console.log('=== DELAYED INIT START ===');
            this.captureOriginalData();
            this.initializeCurrentImage();
            this.updatePreview();
            console.log('=== DELAYED INIT END ===');
        }, 1000); // เพิ่มเวลารอเป็น 1 วินาที
    }

    // ===== CAPTURE ORIGINAL DATA =====
    captureOriginalData() {
        console.log('=== DEBUG ORIGINAL DATA ===');

        const nameInput = document.getElementById('Name');
        const descInput = document.getElementById('Description');
        const colorInput = document.getElementById('Color');
        const sortOrderInput = document.getElementById('SortOrder');
        const isActiveInput = document.getElementById('IsActive');
        const imageUrlInput = document.getElementById('ImageUrl');

        console.log('Name input:', nameInput, 'Value:', nameInput?.value);
        console.log('Description input:', descInput, 'Value:', descInput?.value);
        console.log('Color input:', colorInput, 'Value:', colorInput?.value);
        console.log('SortOrder input:', sortOrderInput, 'Value:', sortOrderInput?.value);
        console.log('IsActive input:', isActiveInput, 'Checked:', isActiveInput?.checked);
        console.log('ImageUrl input:', imageUrlInput, 'Value:', imageUrlInput?.value);

        this.originalData = {
            name: nameInput?.value || '',
            description: descInput?.value || '',
            color: colorInput?.value || '#0066cc',
            sortOrder: sortOrderInput?.value || '1',
            isActive: isActiveInput?.checked || false,
            imageUrl: imageUrlInput?.value || ''
        };

        console.log('Original data captured:', this.originalData);
        this.currentImagePath = imageUrlInput?.value || null;
    }

    // ===== CONFIRMATION MODAL =====
    forceCancel() {
        if (this.hasUnsavedChanges()) {
            this.createCenteredConfirmation();
        } else {
            adminNotyf.open({
                type: 'info',
                message: '<i class="fas fa-spinner fa-spin me-2"></i>กำลังกลับสู่หน้ารายการ...',
                duration: 1500
            });
            this.performCancel();
        }
    }

    hasUnsavedChanges() {
        const nameInput = document.getElementById('Name');
        const descInput = document.getElementById('Description');
        const colorInput = document.getElementById('Color');
        const sortOrderInput = document.getElementById('SortOrder');
        const isActiveInput = document.getElementById('IsActive');
        const imageUrlInput = document.getElementById('ImageUrl');

        return (
            nameInput?.value !== this.originalData.name ||
            descInput?.value !== this.originalData.description ||
            colorInput?.value !== this.originalData.color ||
            sortOrderInput?.value !== this.originalData.sortOrder ||
            isActiveInput?.checked !== this.originalData.isActive ||
            imageUrlInput?.value !== this.originalData.imageUrl
        );
    }

    createCenteredConfirmation() {
        const overlay = document.createElement('div');
        overlay.id = 'confirmationOverlay';
        overlay.style.cssText = `
            position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0, 0, 0, 0.5); display: flex; justify-content: center;
            align-items: center; z-index: 9999; backdrop-filter: blur(4px);
            animation: fadeIn 0.3s ease-out;
        `;

        const modal = document.createElement('div');
        modal.style.cssText = `
            background: white; border-radius: 12px; padding: 2rem; max-width: 400px;
            width: 90%; text-align: center; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            animation: slideIn 0.3s ease-out; position: relative;
        `;

        modal.innerHTML = this.getModalHTML();

        this.addModalStyles();
        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        this.setupModalEvents(overlay);
    }

    getModalHTML() {
        return `
            <div style="margin-bottom: 1.5rem;">
                <div style="width: 80px; height: 80px; margin: 0 auto 1rem;
                    background: linear-gradient(135deg, #f59e0b, #d97706); border-radius: 50%;
                    display: flex; align-items: center; justify-content: center;
                    color: white; font-size: 2rem;">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3 style="color: #1f2937; font-weight: 600; margin-bottom: 0.5rem; font-size: 1.25rem;">
                    ยกเลิกการแก้ไขหมวดหมู่?
                </h3>
                <p style="color: #6b7280; margin: 0; font-size: 0.95rem; line-height: 1.5;">
                    การเปลี่ยนแปลงที่คุณทำไว้จะหายไป<br>และไม่สามารถกู้คืนได้
                </p>
            </div>
            <div style="display: flex; gap: 0.75rem; justify-content: center; flex-wrap: wrap;">
                <button id="confirmCancelBtn" style="background: linear-gradient(135deg, #dc2626, #b91c1c);
                    color: white; border: none; padding: 0.75rem 1.5rem; border-radius: 8px;
                    cursor: pointer; font-weight: 600; font-size: 0.95rem; transition: all 0.2s ease;
                    min-width: 120px; box-shadow: 0 4px 12px rgba(220, 38, 38, 0.3);">
                    <i class="fas fa-check me-2"></i>ใช่, ยกเลิก
                </button>
                <button id="cancelCancelBtn" style="background: #f3f4f6; color: #374151;
                    border: 1px solid #d1d5db; padding: 0.75rem 1.5rem; border-radius: 8px;
                    cursor: pointer; font-weight: 600; font-size: 0.95rem; transition: all 0.2s ease;
                    min-width: 120px;">
                    <i class="fas fa-times me-2"></i>กลับไป
                </button>
            </div>
        `;
    }

    addModalStyles() {
        if (document.getElementById('modalStyles')) return;

        const style = document.createElement('style');
        style.id = 'modalStyles';
        style.textContent = `
            @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
            @keyframes slideIn { from { opacity: 0; transform: scale(0.8) translateY(-20px); } 
                                to { opacity: 1; transform: scale(1) translateY(0); } }
            @keyframes slideOut { from { opacity: 1; transform: scale(1) translateY(0); }
                                 to { opacity: 0; transform: scale(0.8) translateY(-20px); } }
            .modal-closing { animation: slideOut 0.2s ease-in !important; }
        `;
        document.head.appendChild(style);
    }

    setupModalEvents(overlay) {
        document.getElementById('confirmCancelBtn').addEventListener('click', () => {
            this.closeConfirmation(overlay, true);
        });

        document.getElementById('cancelCancelBtn').addEventListener('click', () => {
            this.closeConfirmation(overlay, false);
        });

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) this.closeConfirmation(overlay, false);
        });

        const handleEscape = (e) => {
            if (e.key === 'Escape') {
                this.closeConfirmation(overlay, false);
                document.removeEventListener('keydown', handleEscape);
            }
        };
        document.addEventListener('keydown', handleEscape);
    }

    closeConfirmation(overlay, confirmed) {
        const modal = overlay.querySelector('div');
        modal.classList.add('modal-closing');

        setTimeout(() => {
            document.body.removeChild(overlay);

            if (confirmed) {
                adminNotyf.open({
                    type: 'info',
                    message: '<i class="fas fa-spinner fa-spin me-2"></i>กำลังกลับสู่หน้ารายการ...',
                    duration: 1500
                });
                this.performCancel();
            } else {
                adminNotyf.success('กลับมาทำงานต่อ');
            }
        }, 200);
    }

    performCancel() {
        window.onbeforeunload = null;
        setTimeout(() => {
            window.location.href = '/Admin/Category';
        }, 800);
    }

    // ===== IMAGE UPLOAD FUNCTIONS =====
    async previewAndUploadImage() {
        const fileInput = document.getElementById('categoryImageFile');
        if (!fileInput) return;

        const file = fileInput.files[0];
        if (!file) return;

        if (!this.validateImageFile(file, fileInput)) return;

        this.showUploadProgress();
        this.clearImageValidationError();

        try {
            const result = await this.uploadImage(file);
            this.handleUploadSuccess(result, fileInput);
        } catch (error) {
            adminNotyf.error('เกิดข้อผิดพลาดในการอัพโหลด');
        } finally {
            this.hideUploadProgress();
        }
    }

    validateImageFile(file, fileInput) {
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            adminNotyf.error('กรุณาเลือกไฟล์รูปภาพ (.jpg, .png, .gif, .webp)');
            fileInput.value = '';
            return false;
        }

        if (file.size > 5 * 1024 * 1024) {
            adminNotyf.error('ขนาดไฟล์ต้องไม่เกิน 5MB');
            fileInput.value = '';
            return false;
        }
        return true;
    }

    async uploadImage(file) {
        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch(uploadImageUrl, {
            method: 'POST',
            body: formData
        });

        return await response.json();
    }

    handleUploadSuccess(result, fileInput) {
        if (result.success) {
            const imageUrlInput = document.getElementById('ImageUrl');
            if (imageUrlInput) imageUrlInput.value = result.imagePath;

            this.showCurrentImage(result.imageUrl || result.imagePath);
            this.currentImagePath = result.imagePath;
            adminNotyf.success('อัพโหลดรูปภาพสำเร็จ');
            fileInput.value = '';
        } else {
            adminNotyf.error(result.message || 'เกิดข้อผิดพลาดในการอัพโหลด');
        }
    }

    initializeCurrentImage() {
        const imageUrlInput = document.getElementById('ImageUrl');
        if (imageUrlInput?.value) {
            this.showCurrentImage(imageUrlInput.value);
        }
    }

    showCurrentImage(imageUrl) {
        const elements = {
            currentImage: document.getElementById('currentImage'),
            currentImageContainer: document.getElementById('currentImageContainer'),
            uploadContainer: document.getElementById('uploadContainer')
        };

        if (elements.currentImage) elements.currentImage.src = imageUrl;
        if (elements.currentImageContainer) elements.currentImageContainer.style.display = 'block';
        if (elements.uploadContainer) elements.uploadContainer.style.display = 'none';
    }

    removeCurrentImage() {
        const elements = {
            imageUrlInput: document.getElementById('ImageUrl'),
            currentImageContainer: document.getElementById('currentImageContainer'),
            uploadContainer: document.getElementById('uploadContainer')
        };

        if (elements.imageUrlInput) elements.imageUrlInput.value = '';
        if (elements.currentImageContainer) elements.currentImageContainer.style.display = 'none';
        if (elements.uploadContainer) elements.uploadContainer.style.display = 'block';

        this.currentImagePath = null;
        adminNotyf.success('ลบรูปภาพแล้ว');
        this.updatePreview();
    }

    clearImageValidationError() {
        const imageError = document.querySelector('span[data-valmsg-for="ImageUrl"]');
        const customError = document.getElementById('imageError');

        if (imageError) imageError.textContent = '';
        if (customError) customError.textContent = '';
    }

    showUploadProgress() {
        const progressContainer = document.getElementById('uploadProgress');
        if (progressContainer) progressContainer.style.display = 'block';
    }

    hideUploadProgress() {
        const progressContainer = document.getElementById('uploadProgress');
        if (progressContainer) progressContainer.style.display = 'none';
    }

    // ===== COLOR & PREVIEW FUNCTIONS =====
    updateColorPicker(color) {
        if (color.match(/^#[0-9A-F]{6}$/i)) {
            const colorInput = document.getElementById('Color');
            if (colorInput) {
                colorInput.value = color;
                this.updatePreview();
            }
        }
    }

    updatePreview() {
        const inputs = {
            name: document.getElementById('Name'),
            desc: document.getElementById('Description'),
            color: document.getElementById('Color'),
            isActive: document.getElementById('IsActive')
        };

        const previews = {
            name: document.getElementById('previewName'),
            desc: document.getElementById('previewDescription'),
            icon: document.getElementById('previewIcon'),
            status: document.getElementById('previewStatus')
        };

        if (previews.name && inputs.name) {
            previews.name.textContent = inputs.name.value || 'ชื่อหมวดหมู่';
        }

        if (previews.desc && inputs.desc) {
            previews.desc.textContent = inputs.desc.value || 'คำอธิบายหมวดหมู่';
        }

        if (previews.icon && inputs.color) {
            previews.icon.style.backgroundColor = inputs.color.value || '#0066cc';
        }

        if (previews.status && inputs.isActive) {
            const isActive = inputs.isActive.checked;
            previews.status.className = `badge ${isActive ? 'bg-success' : 'bg-secondary'}`;
            previews.status.textContent = isActive ? 'เปิดใช้งาน' : 'ปิดใช้งาน';
        }
    }

    showSlugInfo() {
        adminNotyf.open({
            type: 'info',
            message: 'URL Slug จะอัพเดทอัตโนมัติหากเปลี่ยนชื่อหมวดหมู่'
        });
    }

    // ===== NAVIGATION & VALIDATION =====
    setupEnterKeyNavigation() {
        const fieldOrder = ['Name', 'Description', 'categoryImageFile', 'ColorText', 'SortOrder', 'IsActive'];

        fieldOrder.forEach((fieldId, index) => {
            const field = document.getElementById(fieldId);
            if (!field) return;

            field.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    if (field.tagName === 'TEXTAREA' && !e.ctrlKey) return;

                    e.preventDefault();
                    this.focusNextField(fieldOrder, index);
                }
            });
        });
    }

    focusNextField(fieldOrder, currentIndex) {
        const nextIndex = currentIndex + 1;
        if (nextIndex < fieldOrder.length) {
            const nextField = document.getElementById(fieldOrder[nextIndex]);
            if (nextField) {
                if (nextField.type === 'checkbox') nextField.focus();
                else if (nextField.type === 'file') nextField.click();
                else {
                    nextField.focus();
                    if (nextField.select && nextField.type === 'text') nextField.select();
                }
            }
        } else {
            const submitBtn = document.querySelector('button[type="submit"]');
            if (submitBtn) submitBtn.focus();
        }
    }

    setupColorFieldNavigation() {
        const colorInput = document.getElementById('Color');
        if (colorInput) {
            colorInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const sortOrderField = document.getElementById('SortOrder');
                    if (sortOrderField) {
                        sortOrderField.focus();
                        sortOrderField.select();
                    }
                }
            });
        }
    }

    addTextareaHint() {
        const descField = document.getElementById('Description');
        if (descField) {
            const formText = descField.parentNode.querySelector('.form-text');
            if (formText) {
                formText.innerHTML += ' <span class="text-muted small">(กด Ctrl+Enter เพื่อไปต่อ)</span>';
            }
        }
    }

    setupFormValidation() {
        const form = document.querySelector('.admin-form');
        if (form) {
            form.addEventListener('submit', async (e) => {
                // ป้องกันการส่งซ้ำ
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn.disabled) {
                    e.preventDefault();
                    return false;
                }

                this.clearValidationStates();
                const errors = this.validateForm();

                if (errors.length > 0) {
                    e.preventDefault();
                    this.showValidationToast(errors);
                    return false;
                }

                // ✅ ผ่าน validation แล้ว - ให้ form submit
                console.log('Form validation passed, submitting to server...');

                // แสดง loading state
                if (submitBtn) {
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>กำลังบันทึก...';

                    // เปิดใช้งานกลับใน 5 วินาที เผื่อเกิด error
                    setTimeout(() => {
                        if (submitBtn.disabled) {
                            submitBtn.disabled = false;
                            submitBtn.innerHTML = '<i class="fas fa-save me-2"></i>บันทึกการเปลี่ยนแปลง';
                        }
                    }, 5000);
                }

                // ให้ form submit ตามปกติ (ไม่ preventDefault)
            });
        }
    }

    validateForm() {
        const errors = [];

        const nameInput = document.getElementById('Name');
        if (!nameInput?.value?.trim()) {
            errors.push({ field: nameInput, message: 'กรุณากรอกชื่อหมวดหมู่' });
        }

        const colorInput = document.getElementById('Color');
        if (!colorInput?.value) {
            errors.push({ field: colorInput, message: 'กรุณาเลือกสีประจำหมวดหมู่' });
        }

        const sortOrderInput = document.getElementById('SortOrder');
        if (sortOrderInput?.value && (isNaN(sortOrderInput.value) || parseInt(sortOrderInput.value) < 0)) {
            errors.push({ field: sortOrderInput, message: 'ลำดับการแสดงผลต้องเป็นตัวเลขบวกหรือ 0' });
        }

        return errors;
    }

    showValidationToast(errors) {
        if (!errors || errors.length === 0) return;

        const errorMessages = errors.map(error => `• ${error.message}`);
        const combinedMessage = `พบข้อผิดพลาด ${errors.length} รายการ:\n\n${errorMessages.join('\n')}`;

        adminNotyf.error(combinedMessage);

        errors.forEach(error => {
            if (error.field) this.highlightField(error.field);
        });

        if (errors[0]?.field) {
            errors[0].field.focus();
            errors[0].field.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    highlightField(field) {
        field.classList.add('is-invalid');

        const removeHighlight = () => {
            field.classList.remove('is-invalid');
            field.removeEventListener('input', removeHighlight);
            field.removeEventListener('change', removeHighlight);
        };

        field.addEventListener('input', removeHighlight);
        field.addEventListener('change', removeHighlight);
    }

    clearValidationStates() {
        document.querySelectorAll('.is-invalid').forEach(field => {
            field.classList.remove('is-invalid');
        });
    }

    setupInputListeners() {
        const nameInput = document.getElementById('Name');
        if (nameInput) {
            nameInput.addEventListener('input', () => this.updatePreview());
        }

        const descInput = document.getElementById('Description');
        if (descInput) {
            descInput.addEventListener('input', () => this.updatePreview());
        }

        const colorInput = document.getElementById('Color');
        if (colorInput) {
            colorInput.addEventListener('change', () => {
                const colorTextInput = document.getElementById('ColorText');
                if (colorTextInput) colorTextInput.value = colorInput.value;
                this.updatePreview();
            });
        }

        const colorTextInput = document.getElementById('ColorText');
        if (colorTextInput) {
            colorTextInput.addEventListener('input', () => {
                this.updateColorPicker(colorTextInput.value);
            });
        }

        const isActiveInput = document.getElementById('IsActive');
        if (isActiveInput) {
            isActiveInput.addEventListener('change', () => this.updatePreview());
        }
    }
}

// Global functions สำหรับเรียกจาก HTML onclick
let editCategoryFormInstance;

function forceCancel() {
    if (editCategoryFormInstance) editCategoryFormInstance.forceCancel();
}

function showSlugInfo() {
    if (editCategoryFormInstance) editCategoryFormInstance.showSlugInfo();
}

function previewAndUploadImage() {
    if (editCategoryFormInstance) editCategoryFormInstance.previewAndUploadImage();
}

function removeCurrentImage() {
    if (editCategoryFormInstance) editCategoryFormInstance.removeCurrentImage();
}

function updateColorPicker(color) {
    if (editCategoryFormInstance) editCategoryFormInstance.updateColorPicker(color);
}

// Initialize เมื่อ DOM ready
document.addEventListener('DOMContentLoaded', () => {
    editCategoryFormInstance = new EditCategoryForm();
});