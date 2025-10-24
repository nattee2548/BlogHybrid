/* ================================================
   My Posts Page JavaScript (with View Toggle)
   ================================================ */

(function () {
    'use strict';

    // ========================================
    // View Toggle (List/Grid)
    // ========================================
    const VIEW_STORAGE_KEY = 'myPostsView';
    let currentView = localStorage.getItem(VIEW_STORAGE_KEY) || 'list'; // Default = list
    let postsContainer = null;
    let viewToggleButtons = null;

    function initializeViewToggle() {
        postsContainer = document.querySelector('.my-posts-container');
        viewToggleButtons = document.querySelectorAll('.view-toggle-btn');

        if (!postsContainer || !viewToggleButtons.length) return;

        // Set initial view from localStorage
        setView(currentView);

        // Add click handlers
        viewToggleButtons.forEach(btn => {
            btn.addEventListener('click', function() {
                const view = this.getAttribute('data-view');
                setView(view);
                
                // Save to localStorage
                localStorage.setItem(VIEW_STORAGE_KEY, view);
            });
        });
    }

    function setView(view) {
        if (!postsContainer) return;

        currentView = view;
        postsContainer.setAttribute('data-view', view);

        // Update active button
        viewToggleButtons.forEach(btn => {
            if (btn.getAttribute('data-view') === view) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });

        // Add animation
        postsContainer.style.opacity = '0';
        setTimeout(() => {
            postsContainer.style.opacity = '1';
        }, 50);
    }

    window.toggleView = function(view) {
        setView(view);
        localStorage.setItem(VIEW_STORAGE_KEY, view);
    };

    // ========================================
    // Delete Post Confirmation
    // ========================================
    let deleteModal = null;
    let postIdInput = null;
    let postTitleSpan = null;

    function initializeDeleteModal() {
        deleteModal = document.getElementById('deleteModal');
        postIdInput = document.getElementById('postId');
        postTitleSpan = document.getElementById('postTitle');
    }

    window.confirmDelete = function (postId, postTitle) {
        if (!deleteModal) {
            initializeDeleteModal();
        }

        postIdInput.value = postId;
        postTitleSpan.textContent = postTitle;
        deleteModal.classList.add('active');
        document.body.style.overflow = 'hidden';
    };

    window.closeDeleteModal = function () {
        if (deleteModal) {
            deleteModal.classList.remove('active');
            document.body.style.overflow = '';
        }
    };

    // Close modal on outside click
    document.addEventListener('click', function (e) {
        if (e.target === deleteModal) {
            closeDeleteModal();
        }
    });

    // Close modal on ESC key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && deleteModal && deleteModal.classList.contains('active')) {
            closeDeleteModal();
        }
    });

    // ========================================
    // Initialize
    // ========================================
    document.addEventListener('DOMContentLoaded', function () {
        initializeDeleteModal();
        initializeViewToggle();
        console.log('My Posts page loaded successfully - Current view:', currentView);
    });

})();
