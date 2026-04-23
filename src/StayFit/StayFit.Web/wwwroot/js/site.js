// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ─── Система сповіщень ──────────────────────────────────────────────────────

class NotificationManager {
    constructor() {
        this.notifications = [];
        this.loadInterval = null;
        this.init();
    }

    init() {
        const bell = document.getElementById('notificationBell');
        if (!bell) return;

        // Завантажити сповіщення при завантаженні сторінки
        this.loadNotifications();

        // Перезавантажувати кожні 30 секунд
        this.loadInterval = setInterval(() => this.loadNotifications(), 30000);

        // Позначити всі як прочитані
        document.getElementById('markAllReadBtn')?.addEventListener('click', () => this.markAllAsRead());

        // Очистити всі
        document.getElementById('clearAllBtn')?.addEventListener('click', () => this.clearAllNotifications());
    }

    async loadNotifications() {
        try {
            const response = await fetch('/notifications/unread');
            if (!response.ok) throw new Error('Failed to load notifications');

            this.notifications = await response.json();
            this.renderNotifications();
            this.updateBadge();
        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    async updateBadge() {
        try {
            const response = await fetch('/notifications/unread-count');
            if (!response.ok) throw new Error('Failed to load count');

            const data = await response.json();
            const badgeElement = document.getElementById('notificationBadge');
            const countElement = document.getElementById('notificationCount');

            if (data.count > 0) {
                countElement.textContent = data.count;
                badgeElement.style.display = 'block';
            } else {
                badgeElement.style.display = 'none';
            }
        } catch (error) {
            console.error('Error updating badge:', error);
        }
    }

    renderNotifications() {
        const list = document.getElementById('notificationList');
        const clearBtn = document.getElementById('clearAllBtn');
        const markAllReadBtn = document.getElementById('markAllReadBtn');

        if (!this.notifications || this.notifications.length === 0) {
            list.innerHTML = '<div class="text-center text-muted">Немає сповіщень</div>';
            clearBtn.style.display = 'none';
            markAllReadBtn.style.display = 'none';
            return;
        }

        clearBtn.style.display = 'block';
        markAllReadBtn.style.display = 'block';

        list.innerHTML = this.notifications.map(notification => `
            <div class="notification-item p-2 border-bottom d-flex justify-content-between align-items-start">
                <div class="flex-grow-1">
                    <div class="fw-bold">${this.escapeHtml(notification.title)}</div>
                    <small class="text-muted d-block">${this.escapeHtml(notification.message)}</small>
                    <small class="text-secondary">${this.formatDate(notification.createdAt)}</small>
                </div>
                <button type="button" class="btn btn-sm btn-outline-secondary ms-2" 
                    onclick="notificationManager.markAsRead(${notification.id})"
                    title="Позначити як прочитане">
                    ✓
                </button>
            </div>
        `).join('');
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch(`/notifications/${notificationId}/mark-as-read`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.loadNotifications();
            } else {
                console.error('Failed to mark notification as read');
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    async markAllAsRead() {
        try {
            const response = await fetch('/notifications/mark-all-as-read', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.loadNotifications();
            } else {
                console.error('Failed to mark all notifications as read');
            }
        } catch (error) {
            console.error('Error marking all notifications as read:', error);
        }
    }

    async clearAllNotifications() {
        if (!confirm('Ви впевнені, що хочете очистити всі сповіщення?')) {
            return;
        }

        try {
            const response = await fetch('/notifications/clear-all', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.loadNotifications();
            } else {
                console.error('Failed to clear notifications');
            }
        } catch (error) {
            console.error('Error clearing notifications:', error);
        }
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return 'щойно';
        if (diffMins < 60) return `${diffMins}м назад`;
        if (diffHours < 24) return `${diffHours}г назад`;
        if (diffDays < 7) return `${diffDays}д назад`;

        return date.toLocaleDateString('uk-UA');
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Ініціалізація менеджера сповіщень при завантаженні сторінки
document.addEventListener('DOMContentLoaded', function() {
    window.notificationManager = new NotificationManager();
});
