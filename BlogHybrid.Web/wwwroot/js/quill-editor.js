/* ================================================
   Quill Rich Text Editor Integration
   With Image Upload to Cloudflare R2
   ================================================ */

class QuillEditor {
    constructor(options = {}) {
        this.container = options.container;
        this.textarea = options.textarea; // Hidden textarea for form submission
        this.placeholder = options.placeholder || 'เขียนเนื้อหาของคุณที่นี่...';
        this.minLength = options.minLength || 10;
        this.maxLength = options.maxLength || 50000;
        this.imageUploadUrl = options.imageUploadUrl || '/api/upload-image';
        this.uploadFolder = options.uploadFolder || 'posts/content';
        this.onChange = options.onChange || null;
        this.onImageUpload = options.onImageUpload || null;

        this.quill = null;
        this.characterCount = 0;
        this.wordCount = 0;

        this.init();
    }

    init() {
        this.createHTML();
        this.initializeQuill();
        this.setupEventListeners();
        this.loadExistingContent();
    }

    createHTML() {
        this.container.innerHTML = `
            <div class="quill-editor-wrapper" id="quillEditorWrapper">
                <!-- Toolbar -->
                <div id="quillToolbar">
                    <!-- Text formatting -->
                    <span class="ql-formats">
                        <button class="ql-bold" title="Bold (Ctrl+B)"></button>
                        <button class="ql-italic" title="Italic (Ctrl+I)"></button>
                        <button class="ql-underline" title="Underline (Ctrl+U)"></button>
                        <button class="ql-strike" title="Strikethrough"></button>
                    </span>

                    <!-- Headings -->
                    <span class="ql-formats">
                        <select class="ql-header" title="Heading">
                            <option value="1">Heading 1</option>
                            <option value="2">Heading 2</option>
                            <option value="3">Heading 3</option>
                            <option selected>Normal</option>
                        </select>
                    </span>

                    <!-- Colors -->
                    <span class="ql-formats">
                        <select class="ql-color" title="Text Color"></select>
                        <select class="ql-background" title="Background Color"></select>
                    </span>

                    <!-- Lists -->
                    <span class="ql-formats">
                        <button class="ql-list" value="ordered" title="Numbered List"></button>
                        <button class="ql-list" value="bullet" title="Bulleted List"></button>
                    </span>

                    <!-- Alignment -->
                    <span class="ql-formats">
                        <select class="ql-align" title="Text Align">
                            <option selected></option>
                            <option value="center"></option>
                            <option value="right"></option>
                            <option value="justify"></option>
                        </select>
                    </span>

                    <!-- Insert -->
                    <span class="ql-formats">
                        <button class="ql-link" title="Insert Link"></button>
                        <button class="ql-image" title="Insert Image"></button>
                        <button class="ql-video" title="Insert Video"></button>
                    </span>

                    <!-- More -->
                    <span class="ql-formats">
                        <button class="ql-blockquote" title="Blockquote"></button>
                        <button class="ql-code-block" title="Code Block"></button>
                    </span>

                    <!-- Clean -->
                    <span class="ql-formats">
                        <button class="ql-clean" title="Clear Formatting"></button>
                    </span>
                </div>

                <!-- Editor -->
                <div id="quillEditor"></div>

                <!-- Character Counter -->
                <div class="quill-character-counter">
                    <div class="counter-item">
                        <i class="bi bi-type"></i>
                        <span>Characters: <span class="counter-value" id="charCount">0</span> / ${this.maxLength}</span>
                    </div>
                    <div class="counter-item">
                        <i class="bi bi-file-text"></i>
                        <span>Words: <span class="counter-value" id="wordCount">0</span></span>
                    </div>
                    <div class="counter-item">
                        <i class="bi bi-check-circle"></i>
                        <span id="validationStatus">Ready to write</span>
                    </div>
                </div>
            </div>

            <!-- Keyboard Shortcuts -->
            <div class="quill-shortcuts-hint">
                <strong>Shortcuts:</strong>
                <kbd>Ctrl+B</kbd> Bold
                <kbd>Ctrl+I</kbd> Italic
                <kbd>Ctrl+U</kbd> Underline
                <kbd>Ctrl+K</kbd> Link
                <kbd>Ctrl+Z</kbd> Undo
                <kbd>Ctrl+Shift+Z</kbd> Redo
            </div>
        `;

        this.wrapper = document.getElementById('quillEditorWrapper');
        this.charCountEl = document.getElementById('charCount');
        this.wordCountEl = document.getElementById('wordCount');
        this.validationStatusEl = document.getElementById('validationStatus');
    }

    initializeQuill() {
        // Initialize Quill
        this.quill = new Quill('#quillEditor', {
            theme: 'snow',
            modules: {
                toolbar: {
                    container: '#quillToolbar',
                    handlers: {
                        image: () => this.handleImageUpload()
                    }
                },
                clipboard: {
                    matchVisual: false
                }
            },
            placeholder: this.placeholder,
            formats: [
                'header',
                'bold', 'italic', 'underline', 'strike',
                'color', 'background',
                'list', 'bullet',
                'align',
                'link', 'image', 'video',
                'blockquote', 'code-block'
            ]
        });

        // Set initial content if exists
        if (this.textarea && this.textarea.value) {
            this.quill.root.innerHTML = this.textarea.value;
        }
    }

    setupEventListeners() {
        // Text change event
        this.quill.on('text-change', () => {
            this.updateCharacterCount();
            this.updateTextarea();
            this.validateContent();

            if (this.onChange) {
                this.onChange(this.getContent());
            }
        });

        // Selection change (for toolbar state)
        this.quill.on('selection-change', (range) => {
            if (range) {
                this.wrapper.classList.add('focused');
            } else {
                this.wrapper.classList.remove('focused');
            }
        });

        // Paste event (handle images)
        this.quill.root.addEventListener('paste', (e) => {
            const items = e.clipboardData?.items;
            if (!items) return;

            for (let item of items) {
                if (item.type.indexOf('image') !== -1) {
                    e.preventDefault();
                    const file = item.getAsFile();
                    this.uploadImage(file);
                }
            }
        });

        // Drop event (handle images)
        this.quill.root.addEventListener('drop', (e) => {
            e.preventDefault();
            const files = e.dataTransfer?.files;
            if (!files || files.length === 0) return;

            for (let file of files) {
                if (file.type.indexOf('image') !== -1) {
                    this.uploadImage(file);
                }
            }
        });
    }

    loadExistingContent() {
        if (this.textarea && this.textarea.value) {
            this.quill.root.innerHTML = this.textarea.value;
            this.updateCharacterCount();
            this.validateContent();
        }
    }

    updateCharacterCount() {
        const text = this.quill.getText().trim();
        this.characterCount = text.length;
        this.wordCount = text.split(/\s+/).filter(word => word.length > 0).length;

        if (this.charCountEl) {
            this.charCountEl.textContent = this.characterCount;
            
            // Warning states
            if (this.characterCount > this.maxLength * 0.9) {
                this.charCountEl.classList.add('warning');
                this.charCountEl.classList.remove('error');
            } else if (this.characterCount > this.maxLength) {
                this.charCountEl.classList.add('error');
                this.charCountEl.classList.remove('warning');
            } else {
                this.charCountEl.classList.remove('warning', 'error');
            }
        }

        if (this.wordCountEl) {
            this.wordCountEl.textContent = this.wordCount;
        }
    }

    updateTextarea() {
        if (this.textarea) {
            this.textarea.value = this.quill.root.innerHTML;
        }
    }

    validateContent() {
        const isValid = this.characterCount >= this.minLength && 
                       this.characterCount <= this.maxLength;

        if (!this.validationStatusEl) return;

        if (this.characterCount === 0) {
            this.validationStatusEl.textContent = 'Start typing...';
            this.validationStatusEl.style.color = 'var(--text-tertiary)';
            this.wrapper.classList.remove('error');
        } else if (this.characterCount < this.minLength) {
            this.validationStatusEl.textContent = `Need ${this.minLength - this.characterCount} more characters`;
            this.validationStatusEl.style.color = 'var(--warning)';
            this.wrapper.classList.add('error');
        } else if (this.characterCount > this.maxLength) {
            this.validationStatusEl.textContent = `${this.characterCount - this.maxLength} characters over limit`;
            this.validationStatusEl.style.color = 'var(--danger)';
            this.wrapper.classList.add('error');
        } else {
            this.validationStatusEl.textContent = 'Looking good!';
            this.validationStatusEl.style.color = 'var(--success)';
            this.wrapper.classList.remove('error');
        }

        return isValid;
    }

    handleImageUpload() {
        const input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.click();

        input.onchange = () => {
            const file = input.files[0];
            if (file) {
                this.uploadImage(file);
            }
        };
    }

    async uploadImage(file) {
        // Validate file
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type)) {
            this.showError('รองรับเฉพาะไฟล์ JPG, PNG, GIF, WebP');
            return;
        }

        if (file.size > 5 * 1024 * 1024) { // 5MB
            this.showError('ขนาดไฟล์ต้องไม่เกิน 5MB');
            return;
        }

        // Show loading
        this.wrapper.classList.add('loading');

        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('folder', this.uploadFolder);

            const response = await fetch(this.imageUploadUrl, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            if (!response.ok) {
                throw new Error('Upload failed');
            }

            const data = await response.json();
            
            // Insert image into editor
            const range = this.quill.getSelection(true);
            this.quill.insertEmbed(range.index, 'image', data.url);
            this.quill.setSelection(range.index + 1);

            // Callback
            if (this.onImageUpload) {
                this.onImageUpload(data.url);
            }

        } catch (error) {
            console.error('Upload error:', error);
            this.showError('ไม่สามารถอัปโหลดรูปภาพได้');
        } finally {
            this.wrapper.classList.remove('loading');
        }
    }

    showError(message) {
        // Create temporary error toast
        const toast = document.createElement('div');
        toast.className = 'toast-notification toast-error';
        toast.style.cssText = 'position: fixed; top: 2rem; right: 2rem; z-index: 9999; display: flex;';
        toast.innerHTML = `
            <i class="bi bi-exclamation-triangle-fill"></i>
            <span>${message}</span>
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    getContent() {
        return {
            html: this.quill.root.innerHTML,
            text: this.quill.getText(),
            delta: this.quill.getContents()
        };
    }

    setContent(html) {
        this.quill.root.innerHTML = html;
        this.updateCharacterCount();
        this.validateContent();
    }

    getText() {
        return this.quill.getText();
    }

    getHTML() {
        return this.quill.root.innerHTML;
    }

    clear() {
        this.quill.setText('');
        this.updateCharacterCount();
        this.validateContent();
    }

    focus() {
        this.quill.focus();
    }

    isValid() {
        return this.validateContent();
    }

    destroy() {
        if (this.quill) {
            this.quill = null;
        }
        this.container.innerHTML = '';
    }
}

// ========================================
// Auto-initialize if data attributes present
// ========================================
document.addEventListener('DOMContentLoaded', function() {
    const quillContainers = document.querySelectorAll('[data-quill-editor]');
    
    quillContainers.forEach(container => {
        const textarea = document.getElementById(container.dataset.textarea);
        const minLength = parseInt(container.dataset.minLength) || 10;
        const maxLength = parseInt(container.dataset.maxLength) || 50000;
        const placeholder = container.dataset.placeholder || 'เขียนเนื้อหาของคุณที่นี่...';
        const uploadUrl = container.dataset.uploadUrl || '/api/upload-image';
        const uploadFolder = container.dataset.uploadFolder || 'posts/content';

        new QuillEditor({
            container: container,
            textarea: textarea,
            minLength: minLength,
            maxLength: maxLength,
            placeholder: placeholder,
            imageUploadUrl: uploadUrl,
            uploadFolder: uploadFolder
        });
    });
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = QuillEditor;
}
