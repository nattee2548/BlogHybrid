/* ================================================
   Modern Tag Input Component JavaScript
   Interactive tag management with autocomplete
   ================================================ */

class TagInput {
    constructor(options = {}) {
        this.container = options.container;
        this.input = options.input; // Hidden input for form submission
        this.maxTags = options.maxTags || 10;
        this.suggestions = options.suggestions || [];
        this.popularTags = options.popularTags || [];
        this.placeholder = options.placeholder || 'Type to add tags...';
        this.colorful = options.colorful !== false;
        this.allowDuplicates = options.allowDuplicates || false;
        this.caseSensitive = options.caseSensitive || false;
        this.onTagAdd = options.onTagAdd || null;
        this.onTagRemove = options.onTagRemove || null;
        this.onChange = options.onChange || null;

        this.tags = [];
        this.currentInput = '';
        this.suggestionIndex = -1;

        this.init();
    }

    init() {
        this.createHTML();
        this.bindEvents();
        this.loadExistingTags();
    }

    createHTML() {
        this.container.innerHTML = `
            <div class="tag-input-wrapper" id="tagInputWrapper">
                <!-- Tags will be rendered here -->
                <input type="text" 
                       class="tag-input-field" 
                       id="tagInputField"
                       placeholder="${this.placeholder}"
                       autocomplete="off" />
            </div>
            <div class="tag-input-footer">
                <span class="tag-counter" id="tagCounter">0 / ${this.maxTags}</span>
                <span class="tag-helper">
                    <i class="bi bi-info-circle"></i>
                    Press Enter or Comma to add
                </span>
            </div>
            <div class="tag-keyboard-hints">
                <span class="tag-hint">
                    <kbd>Enter</kbd> or <kbd>,</kbd> to add
                </span>
                <span class="tag-hint">
                    <kbd>Backspace</kbd> to remove last
                </span>
                <span class="tag-hint">
                    <kbd>↑</kbd> <kbd>↓</kbd> to navigate suggestions
                </span>
            </div>
            <div class="tag-suggestions" id="tagSuggestions"></div>
            ${this.popularTags.length > 0 ? this.createPopularTagsHTML() : ''}
        `;

        this.wrapper = document.getElementById('tagInputWrapper');
        this.field = document.getElementById('tagInputField');
        this.counter = document.getElementById('tagCounter');
        this.suggestionsEl = document.getElementById('tagSuggestions');
    }

    createPopularTagsHTML() {
        return `
            <div class="popular-tags-section">
                <div class="popular-tags-title">Popular Tags</div>
                <div class="popular-tags-list" id="popularTagsList">
                    ${this.popularTags.map(tag => `
                        <button type="button" 
                                class="popular-tag-btn" 
                                data-tag="${tag}">
                            ${tag}
                        </button>
                    `).join('')}
                </div>
            </div>
        `;
    }

    bindEvents() {
        // Focus wrapper on click
        this.wrapper.addEventListener('click', (e) => {
            if (e.target === this.wrapper || e.target === this.field) {
                this.field.focus();
            }
        });

        // Field focus/blur
        this.field.addEventListener('focus', () => {
            this.wrapper.classList.add('focused');
        });

        this.field.addEventListener('blur', () => {
            setTimeout(() => {
                this.wrapper.classList.remove('focused');
                this.hideSuggestions();
            }, 200);
        });

        // Input events
        this.field.addEventListener('input', (e) => {
            this.currentInput = e.target.value;
            this.showSuggestions();
        });

        // Keydown events
        this.field.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ',') {
                e.preventDefault();
                this.addTag(this.currentInput.trim());
                this.field.value = '';
                this.currentInput = '';
                this.hideSuggestions();
            } else if (e.key === 'Backspace' && this.currentInput === '') {
                e.preventDefault();
                this.removeLastTag();
            } else if (e.key === 'ArrowDown') {
                e.preventDefault();
                this.navigateSuggestions(1);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                this.navigateSuggestions(-1);
            } else if (e.key === 'Escape') {
                this.hideSuggestions();
            }
        });

        // Popular tags click
        if (this.popularTags.length > 0) {
            const popularList = document.getElementById('popularTagsList');
            if (popularList) {
                popularList.addEventListener('click', (e) => {
                    if (e.target.classList.contains('popular-tag-btn')) {
                        const tag = e.target.dataset.tag;
                        this.addTag(tag);
                    }
                });
            }
        }
    }

    loadExistingTags() {
        if (this.input && this.input.value) {
            const existingTags = this.input.value
                .split(',')
                .map(tag => tag.trim())
                .filter(tag => tag.length > 0);

            existingTags.forEach(tag => this.addTag(tag, false));
        }
    }

    addTag(tagText, updateInput = true) {
        if (!tagText || tagText.length === 0) return;

        // Check max tags
        if (this.tags.length >= this.maxTags) {
            this.showError(`Maximum ${this.maxTags} tags allowed`);
            return;
        }

        // Normalize tag
        const normalizedTag = this.caseSensitive
            ? tagText.trim()
            : tagText.trim().toLowerCase();

        // Check duplicates
        if (!this.allowDuplicates) {
            const exists = this.tags.some(tag =>
                this.caseSensitive ? tag === normalizedTag : tag.toLowerCase() === normalizedTag
            );

            if (exists) {
                this.showError('Tag already exists');
                return;
            }
        }

        // Add tag
        this.tags.push(normalizedTag);
        this.renderTags();
        this.updateCounter();
        this.updatePopularTags();

        if (updateInput) {
            this.updateHiddenInput();
        }

        // Callback
        if (this.onTagAdd) {
            this.onTagAdd(normalizedTag);
        }

        if (this.onChange) {
            this.onChange(this.tags);
        }
    }

    removeTag(index) {
        const removedTag = this.tags[index];
        const tagElement = this.wrapper.querySelectorAll('.tag-pill')[index];

        if (tagElement) {
            tagElement.classList.add('removing');
            setTimeout(() => {
                this.tags.splice(index, 1);
                this.renderTags();
                this.updateCounter();
                this.updatePopularTags();
                this.updateHiddenInput();

                // Callback
                if (this.onTagRemove) {
                    this.onTagRemove(removedTag);
                }

                if (this.onChange) {
                    this.onChange(this.tags);
                }
            }, 200);
        }
    }

    removeLastTag() {
        if (this.tags.length > 0) {
            this.removeTag(this.tags.length - 1);
        }
    }

    renderTags() {
        const tagsHTML = this.tags.map((tag, index) => {
            const colorClass = this.colorful
                ? `variant-${(index % 6) + 1}`
                : '';

            return `
                <div class="tag-pill ${colorClass}">
                    <span class="tag-pill-text">${this.escapeHtml(tag)}</span>
                    <button type="button" 
                            class="tag-pill-remove" 
                            data-index="${index}"
                            aria-label="Remove ${tag}">
                        <i class="bi bi-x"></i>
                    </button>
                </div>
            `;
        }).join('');

        // Update wrapper
        this.wrapper.innerHTML = tagsHTML + this.field.outerHTML;

        // Re-bind field reference
        this.field = document.getElementById('tagInputField');

        // Bind remove buttons
        this.wrapper.querySelectorAll('.tag-pill-remove').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const index = parseInt(btn.dataset.index);
                this.removeTag(index);
            });
        });

        // Re-bind field events
        this.bindEvents();
    }

    updateCounter() {
        const remaining = this.maxTags - this.tags.length;
        this.counter.textContent = `${this.tags.length} / ${this.maxTags}`;

        this.counter.classList.remove('warning', 'error');

        if (remaining === 0) {
            this.counter.classList.add('error');
            this.wrapper.classList.add('error');
        } else if (remaining <= 2) {
            this.counter.classList.add('warning');
            this.wrapper.classList.remove('error');
        } else {
            this.wrapper.classList.remove('error');
        }
    }

    updatePopularTags() {
        const popularButtons = document.querySelectorAll('.popular-tag-btn');
        popularButtons.forEach(btn => {
            const tag = btn.dataset.tag;
            const isAdded = this.tags.some(t =>
                this.caseSensitive ? t === tag : t.toLowerCase() === tag.toLowerCase()
            );

            if (isAdded || this.tags.length >= this.maxTags) {
                btn.classList.add('disabled');
                btn.disabled = true;
            } else {
                btn.classList.remove('disabled');
                btn.disabled = false;
            }
        });
    }

    updateHiddenInput() {
        if (this.input) {
            this.input.value = this.tags.join(', ');
        }
    }

    showSuggestions() {
        if (!this.currentInput || this.currentInput.length < 2) {
            this.hideSuggestions();
            return;
        }

        const query = this.currentInput.toLowerCase();
        const filtered = this.suggestions.filter(suggestion => {
            const name = suggestion.name || suggestion;
            return name.toLowerCase().includes(query) &&
                !this.tags.some(tag => tag.toLowerCase() === name.toLowerCase());
        });

        if (filtered.length === 0) {
            this.hideSuggestions();
            return;
        }

        const suggestionsHTML = filtered.slice(0, 5).map((suggestion, index) => {
            const name = suggestion.name || suggestion;
            const count = suggestion.count || '';

            return `
                <div class="tag-suggestion-item ${index === this.suggestionIndex ? 'highlighted' : ''}" 
                     data-index="${index}"
                     data-tag="${this.escapeHtml(name)}">
                    <div class="tag-suggestion-icon">
                        <i class="bi bi-hash"></i>
                    </div>
                    <div class="tag-suggestion-content">
                        <div class="tag-suggestion-name">${this.escapeHtml(name)}</div>
                        ${count ? `<div class="tag-suggestion-count">${count} posts</div>` : ''}
                    </div>
                </div>
            `;
        }).join('');

        this.suggestionsEl.innerHTML = suggestionsHTML;
        this.suggestionsEl.classList.add('active');

        // Bind click events
        this.suggestionsEl.querySelectorAll('.tag-suggestion-item').forEach(item => {
            item.addEventListener('click', () => {
                const tag = item.dataset.tag;
                this.addTag(tag);
                this.field.value = '';
                this.currentInput = '';
                this.hideSuggestions();
            });
        });
    }

    hideSuggestions() {
        this.suggestionsEl.classList.remove('active');
        this.suggestionIndex = -1;
    }

    navigateSuggestions(direction) {
        const items = this.suggestionsEl.querySelectorAll('.tag-suggestion-item');
        if (items.length === 0) return;

        this.suggestionIndex += direction;

        if (this.suggestionIndex < 0) {
            this.suggestionIndex = items.length - 1;
        } else if (this.suggestionIndex >= items.length) {
            this.suggestionIndex = 0;
        }

        // Update UI
        items.forEach((item, index) => {
            if (index === this.suggestionIndex) {
                item.classList.add('highlighted');
                item.scrollIntoView({ block: 'nearest' });

                // Update input with suggestion
                const tag = item.dataset.tag;
                this.field.value = tag;
                this.currentInput = tag;
            } else {
                item.classList.remove('highlighted');
            }
        });
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

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    getTags() {
        return [...this.tags];
    }

    setTags(tags) {
        this.tags = [];
        tags.forEach(tag => this.addTag(tag, false));
        this.updateHiddenInput();
    }

    clear() {
        this.tags = [];
        this.renderTags();
        this.updateCounter();
        this.updatePopularTags();
        this.updateHiddenInput();
    }

    destroy() {
        this.container.innerHTML = '';
    }
}

// ========================================
// Auto-initialize if data attributes present
// ========================================
document.addEventListener('DOMContentLoaded', function () {
    const tagInputs = document.querySelectorAll('[data-tag-input]');

    tagInputs.forEach(container => {
        const hiddenInput = document.getElementById(container.dataset.input);
        const maxTags = parseInt(container.dataset.maxTags) || 10;
        const colorful = container.dataset.colorful !== 'false';

        // Parse suggestions if provided
        let suggestions = [];
        if (container.dataset.suggestions) {
            try {
                suggestions = JSON.parse(container.dataset.suggestions);
            } catch (e) {
                console.warn('Invalid suggestions JSON:', e);
            }
        }

        // Parse popular tags if provided
        let popularTags = [];
        if (container.dataset.popularTags) {
            try {
                popularTags = JSON.parse(container.dataset.popularTags);
            } catch (e) {
                console.warn('Invalid popular tags JSON:', e);
            }
        }

        new TagInput({
            container: container,
            input: hiddenInput,
            maxTags: maxTags,
            suggestions: suggestions,
            popularTags: popularTags,
            colorful: colorful,
            placeholder: container.dataset.placeholder || 'Type to add tags...'
        });
    });
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TagInput;
}