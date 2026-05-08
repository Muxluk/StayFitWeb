// StayFit - Site-wide JavaScript
// Initializes global event listeners, modal confirm logic, and SignalR notifications.

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initModalConfirm();
        initAutoSubmit();
        initNotifications();
    });

    // --- Modal Confirm ---
    function initModalConfirm() {
        var modalEl = document.getElementById('modalConfirm');
        if (!modalEl) return;

        var bsModal = new bootstrap.Modal(modalEl);
        var confirmBtn = modalEl.querySelector('.modal-confirm-btn');
        var titleEl = modalEl.querySelector('.modal-title');
        var bodyEl = modalEl.querySelector('.modal-body p, .modal-body');
        var pendingForm = null;
        var pendingHref = null;

        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-confirm-title]');
            if (!trigger) return;

            e.preventDefault();
            e.stopPropagation();

            var title = trigger.getAttribute('data-confirm-title') || 'Підтвердження';
            var message = trigger.getAttribute('data-confirm-message') || 'Ви впевнені?';
            var btnText = trigger.getAttribute('data-confirm-btn') || 'Підтвердити';

            if (titleEl) titleEl.textContent = title;
            if (bodyEl) bodyEl.textContent = message;
            if (confirmBtn) confirmBtn.textContent = btnText;

            var form = trigger.closest('form');
            if (trigger.tagName === 'A' && trigger.href) {
                pendingHref = trigger.href;
                pendingForm = null;
            } else if (form) {
                pendingForm = form;
                pendingHref = null;
            }

            bsModal.show();
        });

        if (confirmBtn) {
            confirmBtn.addEventListener('click', function () {
                bsModal.hide();
                if (pendingForm) pendingForm.submit();
                else if (pendingHref) window.location.href = pendingHref;
                pendingForm = null;
                pendingHref = null;
            });
        }
    }

    // --- Auto Submit ---
    function initAutoSubmit() {
        document.querySelectorAll('select[data-auto-submit]').forEach(function (select) {
            select.addEventListener('change', function () {
                var form = this.closest('form');
                if (form) form.submit();
            });
        });
    }

    // --- SignalR Notifications ---
    function initNotifications() {
        if (typeof signalR === 'undefined') return;

        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", function (notification) {
            addNotificationToList(notification);
            updateBadge(1);
            showToast(notification);
        });

        connection.start().catch(function (err) {
            console.error("SignalR Connection Error: ", err.toString());
        });

        // Initial fetch
        fetchNotifications();

        // Event listeners for actions
        var markAllBtn = document.getElementById('markAllReadBtn');
        if (markAllBtn) {
            markAllBtn.addEventListener('click', function() {
                fetch('/notifications/mark-all-as-read', { method: 'POST' })
                    .then(r => r.ok && fetchNotifications());
            });
        }

        var clearAllBtn = document.getElementById('clearAllBtn');
        if (clearAllBtn) {
            clearAllBtn.addEventListener('click', function() {
                fetch('/notifications/clear-all', { method: 'POST' })
                    .then(r => r.ok && fetchNotifications());
            });
        }
    }

    function fetchNotifications() {
        fetch('/notifications/unread')
            .then(response => response.json())
            .then(data => {
                var list = document.getElementById('notificationList');
                if (!list) return;

                list.innerHTML = '';
                if (data.length === 0) {
                    list.innerHTML = '<div class="text-center text-muted">Немає сповіщень</div>';
                    updateBadge(0, true);
                    toggleActionButtons(false);
                } else {
                    data.forEach(n => addNotificationToList(n));
                    updateBadge(data.length, true);
                    toggleActionButtons(true);
                }
            });
    }

    function addNotificationToList(n) {
        var list = document.getElementById('notificationList');
        if (!list) return;

        // Remove empty message if present
        var emptyMsg = list.querySelector('.text-muted');
        if (emptyMsg) emptyMsg.remove();

        var item = document.createElement('div');
        item.className = 'notification-item mb-2 p-2 border-bottom';
        item.innerHTML = `
            <div class="d-flex justify-content-between">
                <strong class="small">${n.title}</strong>
                <span class="text-muted smaller">${new Date(n.createdAt).toLocaleTimeString()}</span>
            </div>
            <div class="smaller">${n.message}</div>
        `;
        list.prepend(item);
        toggleActionButtons(true);
    }

    function updateBadge(count, isAbsolute) {
        var badge = document.getElementById('notificationBadge');
        var countEl = document.getElementById('notificationCount');
        if (!badge || !countEl) return;

        var current = isAbsolute ? 0 : parseInt(countEl.textContent) || 0;
        var total = current + count;

        countEl.textContent = total;
        if (total > 0) {
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    }

    function toggleActionButtons(show) {
        var btns = ['markAllReadBtn', 'clearAllBtn'];
        btns.forEach(id => {
            var el = document.getElementById(id);
            if (el) {
                if (show) el.classList.remove('d-none');
                else el.classList.add('d-none');
            }
        });
    }

    function showToast(n) {
        // Simple console log for now, or use a library like toastr if available
        console.log("New Notification:", n.title, n.message);
    }
})();
