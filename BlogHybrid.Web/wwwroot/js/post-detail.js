/* ================================================
   Post Detail JavaScript
   ================================================ */

// ========================================
// Toggle Post Like
// ========================================
async function toggleLike(postId) {
    try {
        // TODO: Implement like API
        console.log('Toggle like for post:', postId);

        // ตัวอย่าง UI update
        const likeBtn = document.querySelector('.like-btn');
        if (likeBtn) {
            likeBtn.classList.toggle('liked');
            const icon = likeBtn.querySelector('i');
            icon.classList.toggle('bi-heart');
            icon.classList.toggle('bi-heart-fill');
        }
    } catch (error) {
        console.error('Error toggling like:', error);
    }
}

// ========================================
// Toggle Comment Like
// ========================================
async function toggleCommentLike(commentId) {
    try {
        // TODO: Implement comment like API
        console.log('Toggle like for comment:', commentId);

        // ตัวอย่าง UI update
        const commentLikeBtn = document.querySelector(`#comment-${commentId} .like-btn`);
        if (commentLikeBtn) {
            commentLikeBtn.classList.toggle('liked');
            const icon = commentLikeBtn.querySelector('i');
            icon.classList.toggle('bi-heart');
            icon.classList.toggle('bi-heart-fill');
        }
    } catch (error) {
        console.error('Error toggling comment like:', error);
    }
}

// ========================================
// Toggle Reply Form
// ========================================
function toggleReplyForm(commentId) {
    const replyForm = document.getElementById(`reply-form-${commentId}`);
    if (replyForm) {
        const isVisible = replyForm.style.display !== 'none';
        replyForm.style.display = isVisible ? 'none' : 'block';

        if (!isVisible) {
            // Focus textarea when showing
            const textarea = replyForm.querySelector('textarea');
            if (textarea) {
                setTimeout(() => textarea.focus(), 100);
            }
        }
    }
}

// ========================================
// Toggle Comment Menu
// ========================================
function toggleCommentMenu(commentId) {
    const menu = document.getElementById(`menu-${commentId}`);
    if (menu) {
        menu.classList.toggle('show');

        // Close menu when clicking outside
        if (menu.classList.contains('show')) {
            setTimeout(() => {
                document.addEventListener('click', function closeMenu(e) {
                    if (!menu.contains(e.target) && !e.target.closest('.btn-menu')) {
                        menu.classList.remove('show');
                        document.removeEventListener('click', closeMenu);
                    }
                });
            }, 0);
        }
    }
}

// ========================================
// Edit Comment
// ========================================
function editComment(commentId) {
    // TODO: Implement edit comment functionality
    console.log('Edit comment:', commentId);
    alert('ฟีเจอร์แก้ไขความคิดเห็นกำลังพัฒนา');
}

// ========================================
// Delete Comment
// ========================================
async function deleteComment(commentId) {
    if (!confirm('คุณต้องการลบความคิดเห็นนี้ใช่หรือไม่?')) {
        return;
    }

    try {
        // TODO: Implement delete comment API
        console.log('Delete comment:', commentId);

        // Remove comment from DOM
        const commentItem = document.getElementById(`comment-${commentId}`);
        if (commentItem) {
            commentItem.style.opacity = '0';
            commentItem.style.transform = 'translateY(-10px)';
            setTimeout(() => commentItem.remove(), 300);
        }

        showToast('ลบความคิดเห็นสำเร็จ', 'success');
    } catch (error) {
        console.error('Error deleting comment:', error);
        showToast('เกิดข้อผิดพลาดในการลบความคิดเห็น', 'error');
    }
}

// ========================================
// Share Functions
// ========================================
function sharePost() {
    const shareUrl = window.location.href;
    const shareTitle = document.querySelector('.post-title')?.textContent || '';

    // Check if Web Share API is available
    if (navigator.share) {
        navigator.share({
            title: shareTitle,
            url: shareUrl
        }).catch(err => console.log('Share cancelled:', err));
    } else {
        // Fallback to copy link
        copyLink();
    }
}

function shareToFacebook() {
    const url = encodeURIComponent(window.location.href);
    window.open(`https://www.facebook.com/sharer/sharer.php?u=${url}`, '_blank', 'width=600,height=400');
}

function shareToTwitter() {
    const url = encodeURIComponent(window.location.href);
    const text = encodeURIComponent(document.querySelector('.post-title')?.textContent || '');
    window.open(`https://twitter.com/intent/tweet?url=${url}&text=${text}`, '_blank', 'width=600,height=400');
}

function shareToLine() {
    const url = encodeURIComponent(window.location.href);
    window.open(`https://social-plugins.line.me/lineit/share?url=${url}`, '_blank', 'width=600,height=400');
}

async function copyLink() {
    try {
        await navigator.clipboard.writeText(window.location.href);
        showToast('คัดลอกลิงก์สำเร็จ!', 'success');
    } catch (error) {
        console.error('Error copying link:', error);
        showToast('ไม่สามารถคัดลอกลิงก์ได้', 'error');
    }
}

// ========================================
// Toast Notification
// ========================================
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type} show`;

    const icon = type === 'success' ? 'bi-check-circle-fill' :
        type === 'error' ? 'bi-exclamation-triangle-fill' : 'bi-info-circle-fill';

    toast.innerHTML = `
        <i class="bi ${icon}"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ========================================
// Auto-hide existing toasts
// ========================================
document.addEventListener('DOMContentLoaded', function () {
    const toasts = document.querySelectorAll('.toast-notification.show');
    toasts.forEach(toast => {
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    });

    // Smooth scroll to comment if hash exists
    if (window.location.hash) {
        const commentId = window.location.hash.substring(1);
        const commentElement = document.getElementById(commentId);
        if (commentElement) {
            setTimeout(() => {
                commentElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                commentElement.style.background = 'var(--primary-light)';
                setTimeout(() => {
                    commentElement.style.background = '';
                }, 2000);
            }, 500);
        }
    }
});