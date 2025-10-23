/* ================================================
   My Posts Page JavaScript
   ================================================ */

(function () {
    'use strict';

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
        console.log('My Posts page loaded successfully');
    });

})();