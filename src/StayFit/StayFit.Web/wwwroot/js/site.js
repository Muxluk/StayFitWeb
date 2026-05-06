// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ─── Система сповіщень ──────────────────────────────────────────────────────

function NotificationManager() {
    this.notifications = [];
    this.loadInterval = null;
    this.init();
}

function applyProgressWidths() {
    var bars = document.querySelectorAll("[data-progress-width]");
    var i;
    var bar;
    var width;

    for (i = 0; i < bars.length; i++) {
        bar = bars[i];
        width = bar.getAttribute("data-progress-width");

        if (width) {
            bar.style.width = width + "%";
        }
    }
}

function wireAutoSubmitForms() {
    var forms = document.querySelectorAll("[data-auto-submit-change]");
    var i;
    var form;

    for (i = 0; i < forms.length; i++) {
        form = forms[i];
        form.addEventListener("change", function () {
            this.submit();
        });
    }
}

function wireConfirmModals() {
    var modalElement = document.getElementById("confirmModal");
    var triggers;
    var index;
    var pendingForm = null;
    var modalInstance;
    var modalTitle;
    var modalMessage;
    var modalAction;

    if (!modalElement || !window.bootstrap || !bootstrap.Modal) {
        return;
    }

    modalInstance = bootstrap.Modal.getOrCreateInstance(modalElement);
    modalTitle = modalElement.querySelector("[data-confirm-modal-title]");
    modalMessage = modalElement.querySelector("[data-confirm-modal-message]");
    modalAction = modalElement.querySelector("[data-confirm-modal-action]");
    triggers = document.querySelectorAll("[data-confirm-message]");

    function resetPendingConfirm() {
        pendingForm = null;
    }

    for (index = 0; index < triggers.length; index++) {
        triggers[index].addEventListener("click", function (event) {
            var title =
                this.getAttribute("data-confirm-title") || "Підтвердження";
            var message = this.getAttribute("data-confirm-message") || "";
            var buttonText =
                this.getAttribute("data-confirm-button-text") || "Підтвердити";
            var buttonClass =
                this.getAttribute("data-confirm-button-class") || "btn-danger";
            var form = this.form;

            if (!form) {
                return;
            }

            event.preventDefault();
            pendingForm = form;

            if (modalTitle) {
                modalTitle.textContent = title;
            }

            if (modalMessage) {
                modalMessage.textContent = message;
            }

            if (modalAction) {
                modalAction.textContent = buttonText;
                modalAction.className = "btn " + buttonClass;
            }

            modalInstance.show();
        });
    }

    if (modalAction) {
        modalAction.addEventListener("click", function () {
            if (!pendingForm) {
                return;
            }

            pendingForm.submit();
            modalInstance.hide();
            resetPendingConfirm();
        });
    }

    modalElement.addEventListener("hidden.bs.modal", resetPendingConfirm);
}

NotificationManager.prototype.init = function () {
    var bell = document.getElementById("notificationBell");
    var markAllReadBtn = document.getElementById("markAllReadBtn");
    var clearAllBtn = document.getElementById("clearAllBtn");

    if (!bell) {
        return;
    }

    this.loadNotifications();

    var self = this;
    this.loadInterval = setInterval(function () {
        self.loadNotifications();
    }, 30000);

    if (markAllReadBtn) {
        markAllReadBtn.addEventListener("click", function () {
            self.markAllAsRead();
        });
    }

    if (clearAllBtn) {
        clearAllBtn.addEventListener("click", function () {
            self.clearAllNotifications();
        });
    }
};

NotificationManager.prototype.loadNotifications = function () {
    var self = this;

    fetch("/notifications/unread")
        .then(function (response) {
            if (!response.ok) {
                throw new Error("Failed to load notifications");
            }

            return response.json();
        })
        .then(function (data) {
            self.notifications = data;
            self.renderNotifications();
            self.updateBadge();
        })
        .catch(function (error) {
            console.error("Error loading notifications:", error);
        });
};

NotificationManager.prototype.updateBadge = function () {
    fetch("/notifications/unread-count")
        .then(function (response) {
            if (!response.ok) {
                throw new Error("Failed to load count");
            }

            return response.json();
        })
        .then(function (data) {
            var badgeElement = document.getElementById("notificationBadge");
            var countElement = document.getElementById("notificationCount");

            if (!badgeElement || !countElement) {
                return;
            }

            if (data.count > 0) {
                countElement.textContent = data.count;
                badgeElement.style.display = "block";
            } else {
                badgeElement.style.display = "none";
            }
        })
        .catch(function (error) {
            console.error("Error updating badge:", error);
        });
};

NotificationManager.prototype.renderNotifications = function () {
    var list = document.getElementById("notificationList");
    var clearBtn = document.getElementById("clearAllBtn");
    var markAllReadBtn = document.getElementById("markAllReadBtn");
    var html = [];
    var i;
    var notification;

    if (!list || !clearBtn || !markAllReadBtn) {
        return;
    }

    if (!this.notifications || this.notifications.length === 0) {
        list.innerHTML =
            '<div class="text-center text-muted">Немає сповіщень</div>';
        clearBtn.style.display = "none";
        markAllReadBtn.style.display = "none";
        return;
    }

    clearBtn.style.display = "block";
    markAllReadBtn.style.display = "block";

    for (i = 0; i < this.notifications.length; i++) {
        notification = this.notifications[i];
        html.push(
            '<div class="notification-item p-2 border-bottom d-flex justify-content-between align-items-start">' +
                '<div class="flex-grow-1">' +
                '<div class="fw-bold">' +
                this.escapeHtml(notification.title) +
                "</div>" +
                '<small class="text-muted d-block">' +
                this.escapeHtml(notification.message) +
                "</small>" +
                '<small class="text-secondary">' +
                this.formatDate(notification.createdAt) +
                "</small>" +
                "</div>" +
                '<button type="button" class="btn btn-sm btn-outline-secondary ms-2" ' +
                'onclick="notificationManager.markAsRead(' +
                notification.id +
                ')" ' +
                'title="Позначити як прочитане">' +
                "✓" +
                "</button>" +
                "</div>",
        );
    }

    list.innerHTML = html.join("");
};

NotificationManager.prototype.markAsRead = function (notificationId) {
    var self = this;

    fetch("/notifications/" + notificationId + "/mark-as-read", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
    })
        .then(function (response) {
            if (response.ok) {
                self.loadNotifications();
            } else {
                console.error("Failed to mark notification as read");
            }
        })
        .catch(function (error) {
            console.error("Error marking notification as read:", error);
        });
};

NotificationManager.prototype.markAllAsRead = function () {
    var self = this;

    fetch("/notifications/mark-all-as-read", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
    })
        .then(function (response) {
            if (response.ok) {
                self.loadNotifications();
            } else {
                console.error("Failed to mark all notifications as read");
            }
        })
        .catch(function (error) {
            console.error("Error marking all notifications as read:", error);
        });
};

NotificationManager.prototype.clearAllNotifications = function () {
    var self = this;

    if (!confirm("Ви впевнені, що хочете очистити всі сповіщення?")) {
        return;
    }

    fetch("/notifications/clear-all", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
    })
        .then(function (response) {
            if (response.ok) {
                self.loadNotifications();
            } else {
                console.error("Failed to clear notifications");
            }
        })
        .catch(function (error) {
            console.error("Error clearing notifications:", error);
        });
};

NotificationManager.prototype.formatDate = function (dateString) {
    var date = new Date(dateString);
    var now = new Date();
    var diffMs = now - date;
    var diffMins = Math.floor(diffMs / 60000);
    var diffHours = Math.floor(diffMs / 3600000);
    var diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) {
        return "щойно";
    }

    if (diffMins < 60) {
        return diffMins + "м назад";
    }

    if (diffHours < 24) {
        return diffHours + "г назад";
    }

    if (diffDays < 7) {
        return diffDays + "д назад";
    }

    return date.toLocaleDateString("uk-UA");
};

NotificationManager.prototype.escapeHtml = function (text) {
    var div = document.createElement("div");

    div.textContent = text;
    return div.innerHTML;
};

document.addEventListener("DOMContentLoaded", function () {
    window.notificationManager = new NotificationManager();
    applyProgressWidths();
    wireAutoSubmitForms();
    wireConfirmModals();
});
