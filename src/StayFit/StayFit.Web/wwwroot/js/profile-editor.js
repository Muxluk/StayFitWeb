(function () {
    'use strict';

    // --- SECURITY TAB ACTIVATION ---
    document.addEventListener('DOMContentLoaded', function() {
        var container = document.querySelector('[data-profile-security-active]');
        if (container && container.getAttribute('data-profile-security-active') === 'true') {
            var securityTab = document.getElementById('security-tab');
            if (securityTab && window.bootstrap && window.bootstrap.Tab) {
                var tab = new bootstrap.Tab(securityTab);
                tab.show();
            }
        }
    });

    // --- FORM VALIDATIONS ---
    document.addEventListener('DOMContentLoaded', function() {
        var changePasswordForm = document.querySelector('form[action="/account-security/change-password"]');
        if (changePasswordForm) {
            changePasswordForm.addEventListener('submit', function (e) {
                var newPassword = document.getElementById('newPassword').value;
                var confirmPassword = document.getElementById('confirmPassword').value;

                if (newPassword !== confirmPassword) {
                    e.preventDefault();
                    alert('Паролі не збігаються!');
                    return false;
                }
            });
        }

        var deleteForm = document.querySelector('form[action="/account-security/delete-account"]');
        if (deleteForm) {
            deleteForm.addEventListener('submit', function (e) {
                var password = document.getElementById('deletePassword').value;
                if (!password || password.trim() === '') {
                    e.preventDefault();
                    alert('Будь ласка, введіть пароль для підтвердження.');
                    return false;
                }
                
                if (!confirm('ОСТАННЄ ПОПЕРЕДЖЕННЯ! Ви дійсно хочете видалити акаунт назавжди?')) {
                    e.preventDefault();
                    return false;
                }
            });
        }
    });

    // --- SECURITY LOGS ---
    function loadSecurityLogs(page) {
        if (!page) page = 1;
        var container = document.getElementById('securityLogs');
        if (!container) return;

        var filterEl = document.getElementById('securityEventTypeFilter');
        var eventType = filterEl ? filterEl.value : '';
        var requestUrl = '/security-log/logs?page=' + page;

        if (eventType) {
            requestUrl += '&eventType=' + encodeURIComponent(eventType);
        }

        fetch(requestUrl)
            .then(function(response) {
                if (!response.ok) {
                    return response.json().then(function(errorPayload) {
                        throw new Error(errorPayload && errorPayload.message ? errorPayload.message : 'Помилка при завантаженні журналу');
                    })['catch'](function() {
                        throw new Error('Помилка при завантаженні журналу');
                    });
                }
                return response.json();
            })
            .then(function(data) {
                renderSecurityLogs(data);
            })
            ['catch'](function(error) {
                console.error('Помилка завантаження логів:', error);
                container.innerHTML = 
                    '<div class="alert alert-warning"><i class="bi bi-exclamation-triangle"></i> Не вдалося завантажити історію безпеки: ' + (error.message || 'невідома помилка') + '</div>';
            });
    }

    function renderSecurityLogs(pagedResult) {
        var container = document.getElementById('securityLogs');
        if (!container) return;

        if (!pagedResult || typeof pagedResult !== 'object') {
            container.innerHTML = '<div class="alert alert-warning"><i class="bi bi-exclamation-triangle"></i> Сервер повернув некоректні дані журналу безпеки.</div>';
            return;
        }

        var items = pagedResult.items || pagedResult.Items || [];
        var pageNumber = pagedResult.pageNumber || pagedResult.PageNumber || 1;
        var totalPages = pagedResult.totalPages || pagedResult.TotalPages || 1;
        var totalCount = pagedResult.totalCount || pagedResult.TotalCount || 0;
        
        if (!items.length) {
            container.innerHTML = '<p class="text-muted">Немає записів у журналі безпеки.</p>';
            return;
        }

        var html = '<div class="table-responsive">';
        html += '<table class="table table-sm table-hover align-middle mb-0">';
        html += '<thead class="table-light"><tr>';
        html += '<th>Дата/Час</th><th>Тип івенту</th><th>Опис</th><th>Пристрій</th><th>IP адреса</th><th>Статус</th>';
        html += '</tr></thead><tbody>';

        for (var i = 0; i < items.length; i++) {
            var log = items[i];
            var date = new Date(log.createdAt);
            var formattedDate = date.toLocaleDateString('uk-UA') + ' ' + date.toLocaleTimeString('uk-UA');
            var statusBadge = log.status === 'Success' 
                ? '<span class="badge bg-success"><i class="bi bi-check-lg"></i> Успіх</span>'
                : '<span class="badge bg-danger"><i class="bi bi-x-lg"></i> Помилка</span>';
            
            var eventBadge = '';
            switch(log.eventType) {
                case 'Login': eventBadge = '<span class="badge bg-info"><i class="bi bi-box-arrow-in-right"></i> Вхід</span>'; break;
                case 'Logout': eventBadge = '<span class="badge bg-secondary"><i class="bi bi-box-arrow-right"></i> Вихід</span>'; break;
                case 'PasswordChange': eventBadge = '<span class="badge bg-warning"><i class="bi bi-key"></i> Зміна пароля</span>'; break;
                case 'SessionTerminated': eventBadge = '<span class="badge bg-danger"><i class="bi bi-stop-circle"></i> Сеанс завершено</span>'; break;
                default: eventBadge = '<span class="badge bg-dark">' + log.eventType + '</span>';
            }
            
            html += '<tr>';
            html += '<td>' + formattedDate + '</td>';
            html += '<td>' + eventBadge + '</td>';
            html += '<td>' + log.description + '</td>';
            html += '<td>' + (log.deviceSummary || 'Unknown') + '</td>';
            html += '<td>' + (log.ipAddress || '—') + '</td>';
            html += '<td>' + statusBadge + '</td>';
            html += '</tr>';
        }

        html += '</tbody></table></div>';
        
        if (totalPages > 1) {
            html += '<p class="text-muted small mt-3">Сторінка ' + pageNumber + ' з ' + totalPages + 
                    ' (' + totalCount + ' записів)</p>';
        }
        
        container.innerHTML = html;
    }

    document.addEventListener('DOMContentLoaded', function() {
        var filter = document.getElementById('securityEventTypeFilter');
        if (filter) {
            filter.addEventListener('change', function() { loadSecurityLogs(1); });
        }
        if (document.getElementById('securityLogs')) {
            loadSecurityLogs(1);
        }
    });

    // --- METRICS & AUTO-SAVE ---
    document.addEventListener('DOMContentLoaded', function() {
        var inputIds = ['Weight', 'Height', 'DateOfBirth', 'Gender', 'ActivityLevel', 'FullName'];
        var elements = {};
        for (var i = 0; i < inputIds.length; i++) {
            var id = inputIds[i];
            var el = document.getElementById(id);
            if (el) elements[id] = el;
        }

        var bmiEl = document.getElementById('calc-bmi');
        var bmrEl = document.getElementById('calc-bmr');
        var tdeeEl = document.getElementById('calc-tdee');
        var saveIndicator = document.getElementById('auto-save-indicator');

        if (!bmiEl) return;

        function calculateMetrics() {
            var weight = parseFloat(elements.Weight ? elements.Weight.value : 0);
            var height = parseFloat(elements.Height ? elements.Height.value : 0);
            var gender = elements.Gender ? elements.Gender.value : null;
            var dob = elements.DateOfBirth ? elements.DateOfBirth.value : null;
            var activity = elements.ActivityLevel ? elements.ActivityLevel.value : null;

            if (!weight || !height) {
                if (bmiEl) bmiEl.innerText = '-';
                if (bmrEl) bmrEl.innerText = '- ккал';
                if (tdeeEl) tdeeEl.innerText = '- ккал';
                return;
            }

            var heightM = height / 100;
            var bmi = weight / (heightM * heightM);
            if (bmiEl) bmiEl.innerText = bmi.toFixed(1);

            var age = 30;
            if (dob) {
                var birthDate = new Date(dob);
                var today = new Date();
                age = today.getFullYear() - birthDate.getFullYear();
                var m = today.getMonth() - birthDate.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                    age--;
                }
            }

            var bmr = 0;
            if (gender === 'Чоловік') {
                bmr = (10 * weight) + (6.25 * height) - (5 * age) + 5;
            } else if (gender === 'Жінка') {
                bmr = (10 * weight) + (6.25 * height) - (5 * age) - 161;
            } else {
                bmr = (10 * weight) + (6.25 * height) - (5 * age) - 78;
            }
            if (bmrEl) bmrEl.innerText = Math.round(bmr) + ' ккал';

            var activityMultiplier = 1.2;
            switch(activity) {
                case 'LightlyActive': activityMultiplier = 1.375; break;
                case 'ModeratelyActive': activityMultiplier = 1.55; break;
                case 'VeryActive': activityMultiplier = 1.725; break;
                case 'SuperActive': activityMultiplier = 1.9; break;
            }

            var tdee = bmr * activityMultiplier;
            if (tdeeEl) tdeeEl.innerText = Math.round(tdee) + ' ккал';
        }

        var saveTimeout = null;

        function autoSave() {
            if (saveIndicator) {
                saveIndicator.innerText = "Зберігається...";
                saveIndicator.classList.remove('bg-success', 'd-none');
                saveIndicator.classList.add('bg-warning', 'text-dark');
            }

            var payload = {
                FullName: elements.FullName ? elements.FullName.value : '',
                Weight: parseFloat(elements.Weight ? elements.Weight.value : 0) || null,
                Height: parseFloat(elements.Height ? elements.Height.value : 0) || null,
                DateOfBirth: elements.DateOfBirth ? elements.DateOfBirth.value : null,
                Gender: elements.Gender ? elements.Gender.value : null,
                ActivityLevel: elements.ActivityLevel ? elements.ActivityLevel.value : null
            };

            var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            var token = tokenEl ? tokenEl.value : '';

            fetch('/profile/auto-save', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(payload)
            })
            .then(function(response) {
                if (response.ok) {
                    if (saveIndicator) {
                        saveIndicator.innerText = "Збережено ✔️";
                        saveIndicator.classList.remove('bg-warning', 'text-dark');
                        saveIndicator.classList.add('bg-success');
                        setTimeout(function() { saveIndicator.classList.add('d-none'); }, 3000);
                    }
                } else {
                    if (saveIndicator) {
                        saveIndicator.innerText = "Помилка";
                        saveIndicator.classList.remove('bg-warning', 'text-dark');
                        saveIndicator.classList.add('bg-danger');
                    }
                }
            })
            ['catch'](function(error) {
                console.error("Auto-save error", error);
                if (saveIndicator) {
                    saveIndicator.innerText = "Помилка";
                    saveIndicator.classList.remove('bg-warning', 'text-dark');
                    saveIndicator.classList.add('bg-danger');
                }
            });
        }

        for (var k = 0; k < inputIds.length; k++) {
            (function(id) {
                if (elements[id]) {
                    elements[id].addEventListener('input', function() {
                        calculateMetrics();
                        clearTimeout(saveTimeout);
                        saveTimeout = setTimeout(autoSave, 1500);
                    });
                }
            })(inputIds[k]);
        }

        calculateMetrics();
    });
})();
