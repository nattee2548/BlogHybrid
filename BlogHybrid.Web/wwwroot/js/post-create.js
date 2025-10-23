/* ================================================
   Create Post Page JavaScript
   With Image Upload & Modal Selectors
   ================================================ */

(function() {
    'use strict';

    // ========================================
    // Global State
    // ========================================
    let selectedCategoryId = null;
    let selectedCategoryName = null;
    let selectedCommunityId = null;
    let selectedCommunityName = null;
    let uploadedImageFile = null;

    // ========================================
    // Form Elements
    // ========================================
    const form = document.getElementById('createPostForm');
    const titleInput = document.querySelector('[name="Title"]');
    const contentTextarea = document.querySelector('[name="Content"]');
    const excerptTextarea = document.querySelector('[name="Excerpt"]');
    const tagsInput = document.querySelector('[name="Tags"]');
    const featuredImageFileInput = document.getElementById('featuredImageFileInput');
    const featuredImageUrlInput = document.getElementById('featuredImageUrlInput');
    const imagePreview = document.getElementById('imagePreview');
    const previewImage = document.getElementById('previewImage');
    const validationMessage = document.getElementById('selectionValidation');
    const submitButton = document.querySelector('.btn-create');

    // Hidden inputs
    const categoryIdInput = document.getElementById('categoryIdInput');
    const communityIdInput = document.getElementById('communityIdInput');

    // Display elements
    const categoryDisplay = document.getElementById('categoryDisplay');
    const communityDisplay = document.getElementById('communityDisplay');

    // ========================================
    // Initialize
    // ========================================
    document.addEventListener('DOMContentLoaded', function() {
        initializeForm();
        setupImageUpload();
        setupAutoResize();
        setupAutoExcerpt();
        setupCharacterCounters();
        setupTagManagement();
        setupKeyboardShortcuts();
        setupUnsavedWarning();
        loadInitialValues();
    });

    function loadInitialValues() {
        // Load pre-selected category (if any)
        if (categoryIdInput && categoryIdInput.value) {
            selectedCategoryId = parseInt(categoryIdInput.value);
            const categoryItem = document.querySelector(`[data-id="${selectedCategoryId}"]`);
            if (categoryItem) {
                selectedCategoryName = categoryItem.dataset.name;
                updateCategoryDisplay();
            }
        }

        // Load pre-selected community (if any)
        if (communityIdInput && communityIdInput.value) {
            selectedCommunityId = parseInt(communityIdInput.value);
            const communityItem = document.querySelector(`#communityList [data-id="${selectedCommunityId}"]`);
            if (communityItem) {
                selectedCommunityName = communityItem.dataset.name;
                updateCommunityDisplay();
            }
        }
    }

    // ========================================
    // Form Validation
    // ========================================
    function initializeForm() {
        if (form) {
            form.addEventListener('submit', async function(e) {
                e.preventDefault();

                // Validate Category or Community selection
                if (!selectedCategoryId && !selectedCommunityId) {
                    showValidationMessage();
                    return false;
                }

                // Show loading state
                if (submitButton) {
                    submitButton.classList.add('loading');
                    submitButton.disabled = true;
                }

                // Upload image if file is selected
                if (uploadedImageFile) {
                    try {
                        const imageUrl = await uploadImage(uploadedImageFile);
                        featuredImageUrlInput.value = imageUrl;
                    } catch (error) {
                        console.error('Error uploading image:', error);
                        showToast('ไม่สามารถอัปโหลดรูปภาพได้', 'error');
                        submitButton.classList.remove('loading');
                        submitButton.disabled = false;
                        return false;
                    }
                }

                // Update hidden inputs
                categoryIdInput.value = selectedCategoryId || '';
                communityIdInput.value = selectedCommunityId || '';

                // Submit form
                form.submit();
            });
        }
    }

    function showValidationMessage() {
        if (validationMessage) {
            validationMessage.style.display = 'flex';
            validationMessage.scrollIntoView({ 
                behavior: 'smooth', 
                block: 'center' 
            });
        }
    }

    function hideValidationMessage() {
        if (validationMessage) {
            validationMessage.style.display = 'none';
        }
    }

    // ========================================
    // Image Upload to Cloudflare R2
    // ========================================
    function setupImageUpload() {
        if (!featuredImageFileInput) return;

        // File input change
        featuredImageFileInput.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                handleImageSelect(file);
            }
        });

        // Drag & Drop
        const uploadLabel = document.querySelector('label[for="featuredImageFileInput"]');
        if (uploadLabel) {
            uploadLabel.addEventListener('dragover', function(e) {
                e.preventDefault();
                this.classList.add('dragover');
            });

            uploadLabel.addEventListener('dragleave', function() {
                this.classList.remove('dragover');
            });

            uploadLabel.addEventListener('drop', function(e) {
                e.preventDefault();
                this.classList.remove('dragover');
                
                const file = e.dataTransfer.files[0];
                if (file && file.type.startsWith('image/')) {
                    handleImageSelect(file);
                }
            });
        }
    }

    function handleImageSelect(file) {
        // Validate file
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            showToast('รองรับเฉพาะไฟล์ JPG, PNG, GIF, WebP', 'error');
            return;
        }

        if (file.size > 5 * 1024 * 1024) { // 5MB
            showToast('ขนาดไฟล์ต้องไม่เกิน 5MB', 'error');
            return;
        }

        // Store file for upload on submit
        uploadedImageFile = file;

        // Show preview
        const reader = new FileReader();
        reader.onload = function(e) {
            previewImage.src = e.target.result;
            imagePreview.style.display = 'block';
        };
        reader.readAsDataURL(file);
    }

    async function uploadImage(file) {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('folder', 'posts');

        try {
            const response = await fetch('/api/upload-image', {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                }
            });

            if (!response.ok) {
                throw new Error('Upload failed');
            }

            const data = await response.json();
            return data.url;
        } catch (error) {
            console.error('Upload error:', error);
            throw error;
        }
    }

    window.removeImage = function() {
        uploadedImageFile = null;
        featuredImageFileInput.value = '';
        featuredImageUrlInput.value = '';
        imagePreview.style.display = 'none';
        previewImage.src = '';
    };

    // ========================================
    // Category Modal
    // ========================================
    window.openCategoryModal = function() {
        const modal = document.getElementById('categoryModal');
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
        hideValidationMessage();
    };

    window.closeCategoryModal = function() {
        const modal = document.getElementById('categoryModal');
        modal.classList.remove('active');
        document.body.style.overflow = '';
    };

    window.selectCategory = function(id, name, parentName = null) {
        selectedCategoryId = id;
        selectedCategoryName = parentName ? `${parentName} > ${name}` : name;
        
        updateCategoryDisplay();
        closeCategoryModal();
        hideValidationMessage();
    };

    function updateCategoryDisplay() {
        if (categoryDisplay) {
            if (selectedCategoryName) {
                categoryDisplay.textContent = selectedCategoryName;
                categoryDisplay.parentElement.classList.add('selected');
            } else {
                categoryDisplay.textContent = 'เลือกหมวดหมู่';
                categoryDisplay.parentElement.classList.remove('selected');
            }
        }

        // Update UI in modal
        document.querySelectorAll('#categoryList .selector-item').forEach(item => {
            if (parseInt(item.dataset.id) === selectedCategoryId) {
                item.classList.add('selected');
            } else {
                item.classList.remove('selected');
            }
        });
    }

    window.searchCategories = function(query) {
        const searchTerm = query.toLowerCase().trim();
        const items = document.querySelectorAll('#categoryList .selector-item');

        items.forEach(item => {
            const name = item.dataset.name.toLowerCase();
            const parent = item.dataset.parent ? item.dataset.parent.toLowerCase() : '';
            
            if (name.includes(searchTerm) || parent.includes(searchTerm)) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });

        // Show/hide groups
        document.querySelectorAll('#categoryList .selector-group').forEach(group => {
            const visibleItems = group.querySelectorAll('.selector-item:not([style*="display: none"])');
            if (visibleItems.length > 0) {
                group.style.display = '';
            } else {
                group.style.display = 'none';
            }
        });
    };

    window.clearCategorySearch = function() {
        const searchInput = document.getElementById('categorySearchInput');
        if (searchInput) {
            searchInput.value = '';
            searchCategories('');
        }
    };

    // ========================================
    // Community Modal
    // ========================================
    window.openCommunityModal = function() {
        const modal = document.getElementById('communityModal');
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
        hideValidationMessage();
    };

    window.closeCommunityModal = function() {
        const modal = document.getElementById('communityModal');
        modal.classList.remove('active');
        document.body.style.overflow = '';
    };

    window.selectCommunity = function(id, name) {
        selectedCommunityId = id;
        selectedCommunityName = name;
        
        updateCommunityDisplay();
        closeCommunityModal();
        hideValidationMessage();
    };

    function updateCommunityDisplay() {
        if (communityDisplay) {
            if (selectedCommunityName) {
                communityDisplay.textContent = selectedCommunityName;
                communityDisplay.parentElement.classList.add('selected');
            } else {
                communityDisplay.textContent = 'เลือกชุมชน';
                communityDisplay.parentElement.classList.remove('selected');
            }
        }

        // Update UI in modal
        document.querySelectorAll('#communityList .selector-item').forEach(item => {
            if (parseInt(item.dataset.id) === selectedCommunityId) {
                item.classList.add('selected');
            } else {
                item.classList.remove('selected');
            }
        });
    }

    window.searchCommunities = function(query) {
        const searchTerm = query.toLowerCase().trim();
        const items = document.querySelectorAll('#communityList .selector-item');

        items.forEach(item => {
            const name = item.dataset.name.toLowerCase();
            
            if (name.includes(searchTerm)) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });
    };

    window.clearCommunitySearch = function() {
        const searchInput = document.getElementById('communitySearchInput');
        if (searchInput) {
            searchInput.value = '';
            searchCommunities('');
        }
    };

    // Close modals on ESC key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeCategoryModal();
            closeCommunityModal();
        }
    });

    // ========================================
    // Auto-resize Textarea (Excerpt only)
    // ======================================== */
    function setupAutoResize() {
        function autoResize(textarea) {
            if (!textarea) return;
            textarea.style.height = 'auto';
            textarea.style.height = textarea.scrollHeight + 'px';
        }

        // Only for Excerpt textarea (Content uses Quill)
        if (excerptTextarea) {
            excerptTextarea.addEventListener('input', function() {
                autoResize(this);
            });
            autoResize(excerptTextarea);
        }
    }

    // ========================================
    // Auto-generate Excerpt from Quill Content
    // ========================================
    function setupAutoExcerpt() {
        if (!excerptTextarea) return;

        // Listen for Quill content changes
        const contentTextarea = document.getElementById('contentTextarea');
        if (!contentTextarea) return;

        // Use MutationObserver to detect Quill updates
        const observer = new MutationObserver(() => {
            if (excerptTextarea.value.trim() === '') {
                generateExcerpt();
            }
        });

        // Observe hidden textarea changes
        if (contentTextarea) {
            observer.observe(contentTextarea, { 
                attributes: true, 
                childList: true, 
                subtree: true 
            });
        }

        // Also trigger on blur
        excerptTextarea.addEventListener('focus', () => {
            if (excerptTextarea.value.trim() === '') {
                generateExcerpt();
            }
        });
    }

    function generateExcerpt() {
        const contentTextarea = document.getElementById('contentTextarea');
        if (!contentTextarea) return;

        const htmlContent = contentTextarea.value.trim();
        
        if (htmlContent.length > 50 && excerptTextarea.value.trim() === '') {
            // Strip HTML tags
            const plainText = htmlContent.replace(/<[^>]*>/g, ' ')
                                         .replace(/\s+/g, ' ')
                                         .trim();
            
            let excerpt = plainText.substring(0, 300);
            
            const lastSpace = excerpt.lastIndexOf(' ');
            if (lastSpace > 200) {
                excerpt = excerpt.substring(0, lastSpace);
            }
            
            excerptTextarea.value = excerpt + '...';
            excerptTextarea.dispatchEvent(new Event('input'));
        }
    }

    // ========================================
    // Character Counters
    // ========================================
    function setupCharacterCounters() {
        function addCounter(input, maxLength) {
            if (!input) return;

            const counter = document.createElement('div');
            counter.className = 'character-counter';
            counter.style.cssText = `
                font-size: var(--text-xs);
                color: var(--text-tertiary);
                text-align: right;
                margin-top: 0.25rem;
            `;

            input.parentElement.appendChild(counter);

            function update() {
                const remaining = maxLength - input.value.length;
                counter.textContent = `${input.value.length} / ${maxLength}`;
                
                if (remaining < 0) {
                    counter.style.color = 'var(--danger)';
                } else if (remaining < maxLength * 0.1) {
                    counter.style.color = 'var(--warning)';
                } else {
                    counter.style.color = 'var(--text-tertiary)';
                }
            }

            input.addEventListener('input', update);
            update();
        }

        if (titleInput) addCounter(titleInput, 200);
        if (excerptTextarea) addCounter(excerptTextarea, 500);
    }

    // ========================================
    // Tag Management
    // ========================================
    function setupTagManagement() {
        if (!tagsInput) return;

        tagsInput.addEventListener('blur', function() {
            const tags = this.value
                .split(',')
                .map(tag => tag.trim())
                .filter(tag => tag.length > 0)
                .filter((tag, index, self) => self.indexOf(tag) === index)
                .slice(0, 10);

            this.value = tags.join(', ');
        });

        tagsInput.addEventListener('input', function() {
            const tagCount = this.value
                .split(',')
                .filter(tag => tag.trim().length > 0)
                .length;

            let helper = tagsInput.parentElement.querySelector('.tag-count-helper');
            
            if (!helper) {
                helper = document.createElement('div');
                helper.className = 'tag-count-helper';
                helper.style.cssText = `
                    font-size: var(--text-xs);
                    color: var(--text-tertiary);
                    margin-top: 0.25rem;
                `;
                tagsInput.parentElement.appendChild(helper);
            }

            helper.textContent = `${tagCount} / 10 แท็ก`;
            helper.style.color = tagCount > 10 ? 'var(--danger)' : 'var(--text-tertiary)';
        });
    }

    // ========================================
    // Toast Notification
    // ========================================
    window.showToast = function(message, type = 'error') {
        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.style.display = 'flex';
        
        const icon = type === 'error' 
            ? '<i class="bi bi-exclamation-triangle-fill"></i>'
            : '<i class="bi bi-check-circle-fill"></i>';
        
        toast.innerHTML = `${icon}<span>${message}</span>`;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 5000);
    };

    // Show existing error toast
    const errorToast = document.getElementById('errorToast');
    if (errorToast) {
        errorToast.style.display = 'flex';
        setTimeout(() => {
            errorToast.style.opacity = '0';
            setTimeout(() => errorToast.style.display = 'none', 300);
        }, 5000);
    }

    // ========================================
    // Keyboard Shortcuts
    // ========================================
    function setupKeyboardShortcuts() {
        document.addEventListener('keydown', function(e) {
            // Ctrl/Cmd + S to save
            if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                e.preventDefault();
                if (form) {
                    form.dispatchEvent(new Event('submit', { 
                        cancelable: true, 
                        bubbles: true 
                    }));
                }
            }

            // Ctrl/Cmd + Enter to submit
            if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
                if (submitButton) {
                    submitButton.click();
                }
            }
        });
    }

    // ========================================
    // Unsaved Changes Warning
    // ========================================
    function setupUnsavedWarning() {
        let formChanged = false;
        let formSubmitted = false;

        if (form) {
            form.addEventListener('input', function() {
                formChanged = true;
            });

            form.addEventListener('submit', function() {
                formSubmitted = true;
            });

            window.addEventListener('beforeunload', function(e) {
                if (formChanged && !formSubmitted) {
                    e.preventDefault();
                    e.returnValue = '';
                    return '';
                }
            });
        }
    }

    // ========================================
    // Console Info
    // ========================================
    console.log('Create Post Page initialized');
    console.log('Features: Image Upload, Category/Community Selector, Auto-save');
    console.log('Shortcuts: Ctrl+S (save), Ctrl+Enter (submit), Esc (close modals)');

})();
