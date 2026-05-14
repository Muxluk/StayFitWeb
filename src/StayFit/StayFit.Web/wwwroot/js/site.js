// StayFit - Site-wide JavaScript
// Initializes global event listeners, modal confirm logic, and SignalR notifications.

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        initModalConfirm();
        initAutoSubmit();
        initNotifications();
    });

    // --- Modal Confirm ---
    function initModalConfirm() {
        var modalEl = document.getElementById("modalConfirm");
        if (!modalEl) return;

        if (!window.bootstrap || !window.bootstrap.Modal) return;

        var bsModal = new bootstrap.Modal(modalEl);
        var confirmBtn = modalEl.querySelector(".modal-confirm-btn");
        var titleEl = modalEl.querySelector(".modal-title");
        var bodyEl = modalEl.querySelector(".modal-body p, .modal-body");
        var pendingForm = null;
        var pendingHref = null;

        document.addEventListener("click", function (e) {
            var target = e.target;
            while (target && target !== document.body) {
                if (target.hasAttribute("data-confirm-title")) {
                    var trigger = target;
                    e.preventDefault();
                    e.stopPropagation();

                    var title =
                        trigger.getAttribute("data-confirm-title") ||
                        "Підтвердження";
                    var message =
                        trigger.getAttribute("data-confirm-message") ||
                        "Ви впевнені?";
                    var btnText =
                        trigger.getAttribute("data-confirm-btn") ||
                        "Підтвердити";

                    if (titleEl) titleEl.textContent = title;
                    if (bodyEl) bodyEl.textContent = message;
                    if (confirmBtn) confirmBtn.textContent = btnText;

                    var form = trigger.closest("form");
                    if (trigger.tagName === "A" && trigger.href) {
                        pendingHref = trigger.href;
                        pendingForm = null;
                    } else if (form) {
                        pendingForm = form;
                        pendingHref = null;
                    }

                    bsModal.show();
                    return;
                }
                target = target.parentNode;
            }
        });

        if (confirmBtn) {
            confirmBtn.addEventListener("click", function () {
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
        var selects = document.querySelectorAll("select[data-auto-submit]");
        for (var i = 0; i < selects.length; i++) {
            selects[i].addEventListener("change", function () {
                var form = this.closest("form");
                if (form) form.submit();
            });
        }
    }

    // --- SignalR Notifications ---
    function initNotifications() {
        if (typeof signalR === "undefined") return;

        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", function (notification) {
            addNotificationToList(notification);
            updateBadge(1);
            showToast(notification);
        });

        connection.start()["catch"](function (err) {
            console.error("SignalR Connection Error: ", err.toString());
        });

        fetchNotifications();

        var markAllBtn = document.getElementById("markAllReadBtn");
        if (markAllBtn) {
            markAllBtn.addEventListener("click", function () {
                fetch("/notifications/mark-all-as-read", {
                    method: "POST",
                }).then(function (r) {
                    if (r.ok) fetchNotifications();
                });
            });
        }

        var clearAllBtn = document.getElementById("clearAllBtn");
        if (clearAllBtn) {
            clearAllBtn.addEventListener("click", function () {
                fetch("/notifications/clear-all", { method: "POST" }).then(
                    function (r) {
                        if (r.ok) fetchNotifications();
                    },
                );
            });
        }
    }

    function fetchNotifications() {
        fetch("/notifications/unread")
            .then(function (response) {
                return response.json();
            })
            .then(function (data) {
                var list = document.getElementById("notificationList");
                if (!list) return;

                list.innerHTML = "";
                if (data.length === 0) {
                    list.innerHTML =
                        '<div class="text-center text-muted">Немає сповіщень</div>';
                    updateBadge(0, true);
                    toggleActionButtons(false);
                } else {
                    for (var i = 0; i < data.length; i++) {
                        addNotificationToList(data[i]);
                    }
                    updateBadge(data.length, true);
                    toggleActionButtons(true);
                }
            });
    }

    function addNotificationToList(n) {
        var list = document.getElementById("notificationList");
        if (!list) return;

        var emptyMsg = list.querySelector(".text-muted");
        if (emptyMsg) emptyMsg.remove();

        var item = document.createElement("div");
        item.className = "notification-item mb-2 p-2 border-bottom";
        item.innerHTML =
            '<div class="d-flex justify-content-between">' +
            '<strong class="small">' +
            n.title +
            "</strong>" +
            '<span class="text-muted smaller">' +
            new Date(n.createdAt).toLocaleTimeString() +
            "</span>" +
            "</div>" +
            '<div class="smaller">' +
            n.message +
            "</div>";

        list.insertBefore(item, list.firstChild);
        toggleActionButtons(true);
    }

    function updateBadge(count, isAbsolute) {
        var badge = document.getElementById("notificationBadge");
        var countEl = document.getElementById("notificationCount");
        if (!badge || !countEl) return;

        var current = isAbsolute ? 0 : parseInt(countEl.textContent) || 0;
        var total = current + count;

        countEl.textContent = total;
        if (total > 0) {
            badge.classList.remove("d-none");
        } else {
            badge.classList.add("d-none");
        }
    }

    function toggleActionButtons(show) {
        var btns = ["markAllReadBtn", "clearAllBtn"];
        for (var i = 0; i < btns.length; i++) {
            var el = document.getElementById(btns[i]);
            if (el) {
                if (show) el.classList.remove("d-none");
                else el.classList.add("d-none");
            }
        }
    }

    function showToast(n) {
        console.log("New Notification:", n.title, n.message);
    }
})();

// --- Theme Toggle ---
function sfToggleTheme() {
    var html = document.documentElement;
    var isDark = html.getAttribute("data-bs-theme") === "dark";
    var next = isDark ? "light" : "dark";
    html.setAttribute("data-bs-theme", next);
    localStorage.setItem("sf-theme", next);
    var icon = document.getElementById("sf-theme-icon");
    if (icon) {
        icon.className = next === "dark" ? "bi bi-sun-fill" : "bi bi-moon-fill";
    }
}

// Sync icon on load
(function () {
    var t = document.documentElement.getAttribute("data-bs-theme");
    var icon = document.getElementById("sf-theme-icon");
    if (icon)
        icon.className = t === "dark" ? "bi bi-sun-fill" : "bi bi-moon-fill";
})();
