// เพิ่มใน wwwroot/js/admin.js หรือสร้างไฟล์ใหม่ wwwroot/js/admin/CategoryDeleteConfirm.js

class CategoryDeleteConfirm {
    constructor() {
        this.currentCategoryId = null;
        this.currentCategoryName = null;
        this.currentPostCount = null;

        // ตรวจสอบ adminNotyf
        if (typeof adminNotyf === 'undefined') {
            window.adminNotyf = {
                success: (message) => alert('✅ ' + message),
                error: (message) => alert('❌ ' + message),
                open: (options) => alert(options.type + ': ' + options.message)
            };
        }
    }

    show(categoryId, categoryName, postCount = 0) {
        this.currentCategoryId = categoryId;
        this.currentCategoryName = categoryName;
        this.currentPostCount = postCount;

        this.createDeleteConfirmation();
    }

    createDeleteConfirmation() {
        const overlay = document.createElement('div');
        overlay.id = 'deleteConfirmationOverlay';
        overlay.style.cssText = `
            position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0, 0, 0, 0.5); display: flex; justify-content: center;
            align-items: center; z-index: 9999; backdrop-filter: blur(4px);
            animation: fadeIn 0.3s ease-out;
        `;

        const modal = document.createElement('div');
        modal.style.cssText = `
            background: white; border-radius: 12px; padding: 2rem; max-width: 450px;
            width: 90%; text-align: center; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            animation: slideIn 0.3s ease-out; position: relative;
        `;

        modal.innerHTML = this.getDeleteModalHTML();

        this.addModalStyles();
        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        this.setupDeleteModalEvents(overlay);
    }

    getDeleteModalHTML() {
        const canDelete = this.currentPostCount === 0;

        return `
            <div style="margin-bottom: 1.5rem;">
                <div style="width: 80px; height: 80px; margin: 0 auto 1rem;
                    background: linear-gradient(135deg, #dc2626, #b91c1c); border-radius: 50%;
                    display: flex; align-items: center; justify-content: center;
                    color: white; font-size: 2.5rem; animation: pulse 2s infinite;">
                    <i class="fas fa-trash"></i>
                </div>
                <h3 style="color: #1f2937; font-weight: 600; margin-bottom: 0.5rem; font-size: 1.25rem;">
                    ยืนยันการลบหมวดหมู่
                </h3>
                <p style="color: #6b7280; margin: 0 0 1rem; font-size: 0.95rem; line-height: 1.5;">
                    คุณแน่ใจหรือไม่ที่ต้องการลบหมวดหมู่นี้?
                </p>
                
                <div style="background: #f8f9fa; border-radius: 8px; padding: 1rem; margin: 1rem 0; 
                           border-left: 4px solid #dc2626; text-align: left;">
                    <div style="font-weight: 600; color: #212529; margin-bottom: 0.5rem; font-size: 1.1rem;">
                        "${this.currentCategoryName}"
                    </div>
                    <div style="font-size: 0.9rem; color: #dc2626; font-weight: 500;">
                        ${canDelete
                ? '<i class="fas fa-info-circle me-1"></i>การดำเนินการนี้ไม่สามารถยกเลิกได้'
                : `<i class="fas fa-exclamation-triangle me-1"></i>หมวดหมู่นี้มีบทความ ${this.currentPostCount} รายการ ไม่สามารถลบได้`
            }
                    </div>
                </div>
            </div>
            
            <div style="display: flex; gap: 0.75rem; justify-content: center; flex-wrap: wrap;">
                ${canDelete ? `
                    <button id="confirmDeleteBtn" style="background: linear-gradient(135deg, #dc2626, #b91c1c);
                        color: white; border: none; padding: 0.75rem 1.5rem; border-radius: 8px;
                        cursor: pointer; font-weight: 600; font-size: 0.95rem; transition: all 0.2s ease;
                        min-width: 120px; box-shadow: 0 4px 12px rgba(220, 38, 38, 0.3);">
                        <i class="fas fa-trash me-2"></i>ลบหมวดหมู่
                    </button>
                ` : `
                    <button id="confirmDeleteBtn" disabled style="background: #9ca3af; color: white; 
                        border: none; padding: 0.75rem 1.5rem; border-radius: 8px; cursor: not-allowed;
                        font-weight: 600; font-size: 0.95rem; min-width: 120px;">
                        <i class="fas fa-ban me-2"></i>ไม่สามารถลบได้
                    </button>
                `}
                
                <button id="cancelDeleteBtn" style="background: #f3f4f6; color: #374151;
                    border: 1px solid #d1d5db; padding: 0.75rem 1.5rem; border-radius: 8px;
                    cursor: pointer; font-weight: 600; font-size: 0.95rem; transition: all 0.2s ease;
                    min-width: 120px;">
                    <i class="fas fa-times me-2"></i>ยกเลิก
                </button>
            </div>
        `;
    }

    addModalStyles() {
        if (document.getElementById('deleteModalStyles')) return;

        const style = document.createElement('style');
        style.id = 'deleteModalStyles';
        style.textContent = `
            @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
            @keyframes slideIn { from { opacity: 0; transform: scale(0.8) translateY(-20px); } 
                                to { opacity: 1; transform: scale(1) translateY(0); } }
            @keyframes slideOut { from { opacity: 1; transform: scale(1) translateY(0); }
                                 to { opacity: 0; transform: scale(0.8) translateY(-20px); } }
            @keyframes pulse {
                0% { box-shadow: 0 0 0 0 rgba(220, 38, 38, 0.4); }
                70% { box-shadow: 0 0 0 20px rgba(220, 38, 38, 0); }
                100% { box-shadow: 0 0 0 0 rgba(220, 38, 38, 0); }
            }
            .delete-modal-closing { animation: slideOut 0.2s ease-in !important; }
            
            #confirmDeleteBtn:hover:not(:disabled) {
                background: linear-gradient(135deg, #b91c1c, #991b1b) !important;
                transform: translateY(-2px);
                box-shadow: 0 8px 25px rgba(220, 38, 38, 0.4) !important;
            }
            
            #cancelDeleteBtn:hover {
                background: #e5e7eb !important;
                transform: translateY(-2px);
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1) !important;
            }
        `;
        document.head.appendChild(style);
    }

    setupDeleteModalEvents(overlay) {
        const confirmBtn = document.getElementById('confirmDeleteBtn');
        const cancelBtn = document.getElementById('cancelDeleteBtn');

        if (confirmBtn && !confirmBtn.disabled) {
            confirmBtn.addEventListener('click', () => {
                this.confirmDelete(overlay);
            });
        }

        cancelBtn.addEventListener('click', () => {
            this.closeDeleteConfirmation(overlay, false);
        });

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                this.closeDeleteConfirmation(overlay, false);
            }
        });

        const handleEscape = (e) => {
            if (e.key === 'Escape') {
                this.closeDeleteConfirmation(overlay, false);
                document.removeEventListener('keydown', handleEscape);
            }
        };
        document.addEventListener('keydown', handleEscape);
    }

    confirmDelete(overlay) {
        const confirmBtn = document.getElementById('confirmDeleteBtn');

        // แสดง loading state
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>กำลังลบ...';

        setTimeout(() => {
            this.closeDeleteConfirmation(overlay, true);
        }, 1000); // รอ 1 วินาที แล้วลบ
    }

    closeDeleteConfirmation(overlay, confirmed) {
        const modal = overlay.querySelector('div');
        modal.classList.add('delete-modal-closing');

        setTimeout(() => {
            document.body.removeChild(overlay);

            if (confirmed) {
                this.performDelete();
            }
        }, 200);
    }

    performDelete() {
        adminNotyf.open({
            type: 'info',
            message: '<i class="fas fa-spinner fa-spin me-2"></i>กำลังลบหมวดหมู่...',
            duration: 2000
        });

        // สร้าง form เพื่อส่งข้อมูลไปยัง controller
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `/Admin/Category/Delete/${this.currentCategoryId}`;

        const token = document.createElement('input');
        token.type = 'hidden';
        token.name = '__RequestVerificationToken';
        token.value = document.querySelector('input[name="__RequestVerificationToken"]').value;

        form.appendChild(token);
        document.body.appendChild(form);

        setTimeout(() => {
            form.submit();
        }, 800);
    }

    // สำหรับ bulk delete
    showBulkDelete(selectedCount) {
        const overlay = document.createElement('div');
        overlay.id = 'bulkDeleteConfirmationOverlay';
        overlay.style.cssText = `
            position: fixed; top: 0; left: 0; width: 100%; height: 100%;
            background: rgba(0, 0, 0, 0.5); display: flex; justify-content: center;
            align-items: center; z-index: 9999; backdrop-filter: blur(4px);
            animation: fadeIn 0.3s ease-out;
        `;

        const modal = document.createElement('div');
        modal.style.cssText = `
            background: white; border-radius: 12px; padding: 2rem; max-width: 450px;
            width: 90%; text-align: center; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            animation: slideIn 0.3s ease-out; position: relative;
        `;

        modal.innerHTML = `
            <div style="margin-bottom: 1.5rem;">
                <div style="width: 80px; height: 80px; margin: 0 auto 1rem;
                    background: linear-gradient(135deg, #f59e0b, #d97706); border-radius: 50%;
                    display: flex; align-items: center; justify-content: center;
                    color: white; font-size: 2.5rem; animation: pulse 2s infinite;">
                    <i class="fas fa-trash-alt"></i>
                </div>
                <h3 style="color: #1f2937; font-weight: 600; margin-bottom: 0.5rem; font-size: 1.25rem;">
                    ยืนยันการลบหลายรายการ
                </h3>
                <p style="color: #6b7280; margin: 0 0 1rem; font-size: 0.95rem; line-height: 1.5;">
                    คุณแน่ใจหรือไม่ที่ต้องการลบหมวดหมู่ทั้งหมด <strong>${selectedCount}</strong> รายการ?
                </p>
                
                <div style="background: #fef3c7; border-radius: 8px; padding: 1rem; margin: 1rem 0; 
                           border-left: 4px solid #f59e0b; text-align: left;">
                    <div style="font-size: 0.9rem; color: #92400e; font-weight: 500;">
                        <i class="fas fa-info-circle me-1"></i>การดำเนินการนี้ไม่สามารถยกเลิกได้
                    </div>
                </div>
            </div>
            
            <div style="display: flex; gap: 0.75rem; justify-content: center; flex-wrap: wrap;">
                <button id="confirmBulkDeleteBtn" style="background: linear-gradient(135deg, #f59e0b, #d97706);
                    color: white; border: none; padding: 0.75rem 1.5rem; border-radius: 8px;
                    cursor: pointer; font-weight: 600; font-size: 0.95rem; transition: all 0.2s ease;
                    min-width: 120px; box-shadow: 0 4px 12px rgba(245, 158, 11, 0.3);">
                    <i class="fas fa-trash me-2"></i>ลบทั้งหมด
                </button>
                
                <button id="cancelBulkDeleteBtn" style="background: #f3f4f6; color: #374151;
                    border: 1px solid #d1d5db; padding: 0.75rem 1.5rem; border-radius: 8px;
                    cursor: pointer; font-weight: 600; font-size: 0.95rem; transition: all 0.2s ease;
                    min-width: 120px;">
                    <i class="fas fa-times me-2"></i>ยกเลิก
                </button>
            </div>
        `;

        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        // Setup events
        document.getElementById('confirmBulkDeleteBtn').addEventListener('click', () => {
            this.closeBulkConfirmation(overlay, true);
        });

        document.getElementById('cancelBulkDeleteBtn').addEventListener('click', () => {
            this.closeBulkConfirmation(overlay, false);
        });

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) this.closeBulkConfirmation(overlay, false);
        });
    }

    closeBulkConfirmation(overlay, confirmed) {
        const modal = overlay.querySelector('div');
        modal.classList.add('delete-modal-closing');

        setTimeout(() => {
            document.body.removeChild(overlay);

            if (confirmed) {
                adminNotyf.open({
                    type: 'info',
                    message: 'ฟีเจอร์การลบหลายรายการจะพร้อมใช้งานในเร็วๆ นี้'
                });
            }
        }, 200);
    }
}

// สร้าง instance
let categoryDeleteConfirm;

// เริ่มต้นเมื่อ DOM โหลดเสร็จ
document.addEventListener('DOMContentLoaded', () => {
    categoryDeleteConfirm = new CategoryDeleteConfirm();
});

// ฟังก์ชันที่จะแทนที่ confirmDelete() เดิม
function confirmDelete(categoryId, categoryName, postCount) {
    if (categoryDeleteConfirm) {
        categoryDeleteConfirm.show(categoryId, categoryName, postCount);
    } else {
        // fallback ใช้ confirm() ธรรมดา
        if (postCount > 0) {
            if (window.adminNotyf) {
                window.adminNotyf.error(`ไม่สามารถลบได้ เนื่องจากมีบทความ ${postCount} รายการ`);
            }
            return;
        }

        if (confirm(`ต้องการลบหมวดหมู่ "${categoryName}" หรือไม่?\n\nการดำเนินการนี้ไม่สามารถยกเลิกได้`)) {
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = `/Admin/Category/Delete/${categoryId}`;

            const token = document.createElement('input');
            token.type = 'hidden';
            token.name = '__RequestVerificationToken';
            token.value = document.querySelector('input[name="__RequestVerificationToken"]').value;

            form.appendChild(token);
            document.body.appendChild(form);
            form.submit();
        }
    }
}

// ฟังก์ชันสำหรับ bulk delete
function bulkDelete() {
    const checkedBoxes = document.querySelectorAll('input[name="selectedCategories"]:checked');

    if (checkedBoxes.length === 0) {
        if (window.adminNotyf) {
            window.adminNotyf.error('กรุณาเลือกหมวดหมู่ที่ต้องการลบ');
        }
        return;
    }

    // ตรวจสอบว่ามีหมวดหมู่ใดที่มีโพสต์อยู่หรือไม่
    let hasPostsCount = 0;
    checkedBoxes.forEach(checkbox => {
        const row = checkbox.closest('tr');
        const postsBadge = row.querySelector('.badge.bg-info');
        if (postsBadge && parseInt(postsBadge.textContent) > 0) {
            hasPostsCount++;
        }
    });

    if (hasPostsCount > 0) {
        if (window.adminNotyf) {
            window.adminNotyf.error(`ไม่สามารถลบได้ เนื่องจากมี ${hasPostsCount} หมวดหมู่ที่มีบทความอยู่`);
        }
        return;
    }

    if (categoryDeleteConfirm) {
        categoryDeleteConfirm.showBulkDelete(checkedBoxes.length);
    } else {
        // fallback
        if (confirm(`ต้องการลบหมวดหมู่ ${checkedBoxes.length} รายการที่เลือกหรือไม่?\n\nการดำเนินการนี้ไม่สามารถยกเลิกได้`)) {
            if (window.adminNotyf) {
                window.adminNotyf.open({
                    type: 'info',
                    message: 'ฟีเจอร์การลบหลายรายการจะพร้อมใช้งานในเร็วๆ นี้'
                });
            }
        }
    }
}