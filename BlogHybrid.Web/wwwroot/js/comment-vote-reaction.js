/* ================================================
   Comment Vote & Reaction System
   ================================================ */

// ========================================
// Vote Comment (Upvote/Downvote)
// ========================================
async function voteComment(commentId, voteType) {
    try {
        const response = await fetch(`/api/comment/${commentId}/vote`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ voteType: voteType })
        });

        const result = await response.json();

        if (result.success) {
            // อัปเดต UI
            updateVoteUI(commentId, result.data);
            
            // แสดง toast notification
            showToast(result.message, 'success');
        } else {
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error voting comment:', error);
        
        // เช็คว่าเป็น 401 Unauthorized หรือไม่
        if (error.status === 401) {
            // เปิด login modal หรือ redirect ไปหน้า login
            showLoginPrompt();
        } else {
            showToast('เกิดข้อผิดพลาดในการโหวต กรุณาลองใหม่อีกครั้ง', 'error');
        }
    }
}

/**
 * อัปเดต Vote UI
 */
function updateVoteUI(commentId, data) {
    const commentElement = document.getElementById(`comment-${commentId}`);
    if (!commentElement) return;

    // อัปเดตปุ่ม Upvote
    const upvoteBtn = commentElement.querySelector('.vote-btn.upvote');
    if (upvoteBtn) {
        if (data.currentUserVote === 'Upvote') {
            upvoteBtn.classList.add('active');
        } else {
            upvoteBtn.classList.remove('active');
        }
    }

    // อัปเดตปุ่ม Downvote
    const downvoteBtn = commentElement.querySelector('.vote-btn.downvote');
    if (downvoteBtn) {
        if (data.currentUserVote === 'Downvote') {
            downvoteBtn.classList.add('active');
        } else {
            downvoteBtn.classList.remove('active');
        }
    }

    // อัปเดต Vote Score
    const voteScoreElement = commentElement.querySelector('.vote-score');
    if (voteScoreElement) {
        voteScoreElement.textContent = data.voteScore;
        
        // เพิ่ม animation
        voteScoreElement.classList.add('vote-updated');
        setTimeout(() => voteScoreElement.classList.remove('vote-updated'), 300);
    }

    // อัปเดต Upvote/Downvote counts (ถ้ามี)
    const upvoteCount = commentElement.querySelector('.upvote-count');
    if (upvoteCount) {
        upvoteCount.textContent = data.upvoteCount;
    }

    const downvoteCount = commentElement.querySelector('.downvote-count');
    if (downvoteCount) {
        downvoteCount.textContent = data.downvoteCount;
    }
}

// ========================================
// React to Comment (Like/Love/Haha/Wow/Sad/Angry)
// ========================================
async function reactToComment(commentId, reactionType) {
    try {
        const response = await fetch(`/api/comment/${commentId}/react`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ reactionType: reactionType })
        });

        const result = await response.json();

        if (result.success) {
            // อัปเดต UI
            updateReactionUI(commentId, result.data);
            
            // แสดง toast notification
            showToast(result.message, 'success');
            
            // ปิด reaction picker
            hideReactionPicker(commentId);
        } else {
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error reacting to comment:', error);
        
        if (error.status === 401) {
            showLoginPrompt();
        } else {
            showToast('เกิดข้อผิดพลาดในการแสดงความรู้สึก กรุณาลองใหม่อีกครั้ง', 'error');
        }
    }
}

/**
 * อัปเดต Reaction UI
 */
function updateReactionUI(commentId, data) {
    const commentElement = document.getElementById(`comment-${commentId}`);
    if (!commentElement) return;

    // อัปเดตปุ่ม Reaction หลัก
    const reactionBtn = commentElement.querySelector('.reaction-btn');
    if (reactionBtn) {
        // อัปเดต icon ตาม current user reaction
        const icon = reactionBtn.querySelector('i');
        if (data.currentUserReaction) {
            // แสดง reaction ที่เลือก
            icon.className = getReactionIcon(data.currentUserReaction);
            reactionBtn.classList.add('active');
        } else {
            // กลับไปเป็น default icon
            icon.className = 'bi bi-emoji-smile';
            reactionBtn.classList.remove('active');
        }
    }

    // อัปเดต Reaction counts
    const reactionsElement = commentElement.querySelector('.reaction-counts');
    if (reactionsElement && data.reactions) {
        updateReactionCounts(reactionsElement, data.reactions, data.totalReactionCount);
    }

    // อัปเดต Total reaction count (ถ้ามี)
    const totalCount = commentElement.querySelector('.total-reaction-count');
    if (totalCount) {
        totalCount.textContent = data.totalReactionCount;
        
        // เพิ่ม animation
        totalCount.classList.add('count-updated');
        setTimeout(() => totalCount.classList.remove('count-updated'), 300);
    }
}

/**
 * อัปเดต Reaction Counts Display
 */
function updateReactionCounts(element, reactions, total) {
    if (total === 0) {
        element.innerHTML = '';
        element.style.display = 'none';
        return;
    }

    element.style.display = 'flex';
    
    // สร้าง HTML สำหรับแสดง reactions
    let html = '';
    const reactionEmojis = {
        'Like': '👍',
        'Love': '❤️',
        'Haha': '😂',
        'Wow': '😮',
        'Sad': '😢',
        'Angry': '😡'
    };

    // แสดงเฉพาะ reactions ที่มี count > 0
    const sortedReactions = Object.entries(reactions)
        .filter(([key, value]) => value > 0 && key !== 'totalCount')
        .sort((a, b) => b[1] - a[1]) // เรียงจากมากไปน้อย
        .slice(0, 3); // แสดงแค่ 3 อันดับแรก

    sortedReactions.forEach(([key, count]) => {
        const reactionName = key.replace('Count', ''); // เช่น "LikeCount" -> "Like"
        const emoji = reactionEmojis[reactionName];
        if (emoji) {
            html += `<span class="reaction-item">${emoji} ${count}</span>`;
        }
    });

    element.innerHTML = html;
}

/**
 * แสดง Reaction Picker
 */
function showReactionPicker(commentId) {
    const picker = document.getElementById(`reaction-picker-${commentId}`);
    if (picker) {
        // ปิด pickers อื่น ๆ ก่อน
        document.querySelectorAll('.reaction-picker').forEach(p => {
            if (p.id !== `reaction-picker-${commentId}`) {
                p.classList.remove('show');
            }
        });

        picker.classList.toggle('show');
    }
}

/**
 * ซ่อน Reaction Picker
 */
function hideReactionPicker(commentId) {
    const picker = document.getElementById(`reaction-picker-${commentId}`);
    if (picker) {
        picker.classList.remove('show');
    }
}

/**
 * ปิด Reaction Pickers ทั้งหมดเมื่อคลิกข้างนอก
 */
document.addEventListener('click', (e) => {
    if (!e.target.closest('.reaction-btn') && !e.target.closest('.reaction-picker')) {
        document.querySelectorAll('.reaction-picker.show').forEach(picker => {
            picker.classList.remove('show');
        });
    }
});

/**
 * Helper: แปลง Reaction Type เป็น Icon Class
 */
function getReactionIcon(reactionType) {
    const icons = {
        'Like': 'bi bi-hand-thumbs-up-fill',
        'Love': 'bi bi-heart-fill',
        'Haha': 'bi bi-emoji-laughing-fill',
        'Wow': 'bi bi-emoji-surprised-fill',
        'Sad': 'bi bi-emoji-frown-fill',
        'Angry': 'bi bi-emoji-angry-fill'
    };
    return icons[reactionType] || 'bi bi-emoji-smile';
}

/**
 * แสดง Login Prompt
 */
function showLoginPrompt() {
    // ถ้ามี login modal ให้เปิด
    const loginModal = document.getElementById('loginModal');
    if (loginModal) {
        const modal = new bootstrap.Modal(loginModal);
        modal.show();
    } else {
        // ไม่มี modal ให้ redirect
        const returnUrl = encodeURIComponent(window.location.pathname);
        window.location.href = `/Account/Login?returnUrl=${returnUrl}`;
    }
}

/**
 * Toast Notification (ใช้ function เดิมจาก post-detail.js)
 */
function showToast(message, type = 'info') {
    // ตรวจสอบว่ามี showToast function อยู่แล้วหรือไม่
    if (typeof window.showToast === 'function') {
        window.showToast(message, type);
        return;
    }

    // ถ้าไม่มี ให้สร้างใหม่
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type} show`;

    const icon = type === 'success' ? 'bi-check-circle-fill' :
        type === 'error' ? 'bi-exclamation-triangle-fill' :
            'bi-info-circle-fill';

    toast.innerHTML = `
        <i class="bi ${icon}"></i>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    // Auto hide after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}
