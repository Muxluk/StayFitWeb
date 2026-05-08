(function () {
    // --- SECURITY TAB ACTIVATION ---
    document.addEventListener('DOMContentLoaded', function() {
        const container = document.querySelector('[data-profile-security-active]');
        if (container && container.dataset.profileSecurityActive === 'true') {
            const securityTab = document.getElementById('security-tab');
            if (securityTab) {
                const tab = new bootstrap.Tab(securityTab);
                tab.show();
            }
        }
    });

    // --- FORM VALIDATIONS ---
    document.addEventListener('DOMContentLoaded', function() {
        const changePasswordForm = document.querySelector('form[action="/account-security/change-password"]');
        if (changePasswordForm) {
            changePasswordForm.addEventListener('submit', function (e) {
                const newPassword = document.getElementById('newPassword').value;
                const confirmPassword = document.getElementById('confirmPassword').value;

                if (newPassword !== confirmPassword) {
                    e.preventDefault();
                    alert('Паролі не збігаються!');
                    return false;
                }
            });
        }

        const deleteForm = document.querySelector('form[action="/account-security/delete-account"]');
        if (deleteForm) {
            deleteForm.addEventListener('submit', function (e) {
                const password = document.getElementById('deletePassword').value;
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
    async function loadSecurityLogs(page = 1) {
        const container = document.getElementById('securityLogs');
        if (!container) return;

        try {
            const eventType = document.getElementById('securityEventTypeFilter')?.value || '';
            let requestUrl = '/security-log/logs?page=' + page;

            if (eventType) {
                requestUrl += '&eventType=' + encodeURIComponent(eventType);
            }

            const response = await fetch(requestUrl);
            if (!response.ok) {
                let errorMessage = 'Помилка при завантаженні журналу';
                try {
                    const errorPayload = await response.json();
                    if (errorPayload?.message) {
                        errorMessage = errorPayload.message;
                    }
                } catch (_) {}
                throw new Error(errorMessage);
            }
            
            const data = await response.json();
            renderSecurityLogs(data);
        } catch (error) {
            console.error('Помилка завантаження логів:', error);
            container.innerHTML = 
                '<div class="alert alert-warning"><i class="bi bi-exclamation-triangle"></i> Не вдалося завантажити історію безпеки: ' + (error?.message || 'невідома помилка') + '</div>';
        }
    }

    function renderSecurityLogs(pagedResult) {
        const container = document.getElementById('securityLogs');
        if (!container) return;

        if (!pagedResult || typeof pagedResult !== 'object') {
            container.innerHTML = '<div class="alert alert-warning"><i class="bi bi-exclamation-triangle"></i> Сервер повернув некоректні дані журналу безпеки.</div>';
            return;
        }

        const items = pagedResult.items || pagedResult.Items || [];
        const pageNumber = pagedResult.pageNumber || pagedResult.PageNumber || 1;
        const totalPages = pagedResult.totalPages || pagedResult.TotalPages || 1;
        const totalCount = pagedResult.totalCount || pagedResult.TotalCount || 0;
        
        if (!items.length) {
            container.innerHTML = '<p class="text-muted">Немає записів у журналі безпеки.</p>';
            return;
        }

        let html = '<div class="table-responsive">';
        html += '<table class="table table-sm table-hover align-middle mb-0">';
        html += '<thead class="table-light"><tr>';
        html += '<th>Дата/Час</th><th>Тип івенту</th><th>Опис</th><th>Пристрій</th><th>IP адреса</th><th>Статус</th>';
        html += '</tr></thead><tbody>';

        items.forEach(log => {
            const date = new Date(log.createdAt);
            const formattedDate = date.toLocaleDateString('uk-UA') + ' ' + date.toLocaleTimeString('uk-UA');
            const statusBadge = log.status === 'Success' 
                ? '<span class="badge bg-success"><i class="bi bi-check-lg"></i> Успіх</span>'
                : '<span class="badge bg-danger"><i class="bi bi-x-lg"></i> Помилка</span>';
            
            let eventBadge = '';
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
        });

        html += '</tbody></table></div>';
        
        if (totalPages > 1) {
            html += '<p class="text-muted small mt-3">Сторінка ' + pageNumber + ' з ' + totalPages + 
                    ' (' + totalCount + ' записів)</p>';
        }
        
        container.innerHTML = html;
    }

    document.addEventListener('DOMContentLoaded', function() {
        const filter = document.getElementById('securityEventTypeFilter');
        if (filter) {
            filter.addEventListener('change', () => loadSecurityLogs(1));
        }
        if (document.getElementById('securityLogs')) {
            loadSecurityLogs(1);
        }
    });

    // --- METRICS & AUTO-SAVE ---
    document.addEventListener('DOMContentLoaded', function() {
        const inputs = ['Weight', 'Height', 'DateOfBirth', 'Gender', 'ActivityLevel', 'FullName'];
        const elements = {};
        inputs.forEach(id => {
            const el = document.getElementById(id);
            if (el) elements[id] = el;
        });

        const bmiEl = document.getElementById('calc-bmi');
        const bmrEl = document.getElementById('calc-bmr');
        const tdeeEl = document.getElementById('calc-tdee');
        const saveIndicator = document.getElementById('auto-save-indicator');

        if (!bmiEl) return; // Not on edit profile page or similar

        function calculateMetrics() {
            const weight = parseFloat(elements.Weight?.value);
            const height = parseFloat(elements.Height?.value);
            const gender = elements.Gender?.value;
            const dob = elements.DateOfBirth?.value;
            const activity = elements.ActivityLevel?.value;

            if (!weight || !height) {
                if (bmiEl) bmiEl.innerText = '-';
                if (bmrEl) bmrEl.innerText = '- ккал';
                if (tdeeEl) tdeeEl.innerText = '- ккал';
                return;
            }

            const heightM = height / 100;
            const bmi = weight / (heightM * heightM);
            if (bmiEl) bmiEl.innerText = bmi.toFixed(1);

            let age = 30;
            if (dob) {
                const birthDate = new Date(dob);
                const today = new Date();
                age = today.getFullYear() - birthDate.getFullYear();
                const m = today.getMonth() - birthDate.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                    age--;
                }
            }

            let bmr = 0;
            if (gender === 'Чоловік') {
                bmr = (10 * weight) + (6.25 * height) - (5 * age) + 5;
            } else if (gender === 'Жінка') {
                bmr = (10 * weight) + (6.25 * height) - (5 * age) - 161;
            } else {
                bmr = (10 * weight) + (6.25 * height) - (5 * age) - 78;
            }
            if (bmrEl) bmrEl.innerText = Math.round(bmr) + ' ккал';

            let activityMultiplier = 1.2;
            switch(activity) {
                case 'LightlyActive': activityMultiplier = 1.375; break;
                case 'ModeratelyActive': activityMultiplier = 1.55; break;
                case 'VeryActive': activityMultiplier = 1.725; break;
                case 'SuperActive': activityMultiplier = 1.9; break;
            }

            const tdee = bmr * activityMultiplier;
            if (tdeeEl) tdeeEl.innerText = Math.round(tdee) + ' ккал';
        }

        let saveTimeout = null;

        async function autoSave() {
            if (saveIndicator) {
                saveIndicator.innerText = "Зберігається...";
                saveIndicator.classList.remove('bg-success', 'd-none');
                saveIndicator.classList.add('bg-warning', 'text-dark');
            }

            const payload = {
                FullName: elements.FullName?.value || '',
                Weight: parseFloat(elements.Weight?.value) || null,
                Height: parseFloat(elements.Height?.value) || null,
                DateOfBirth: elements.DateOfBirth?.value || null,
                Gender: elements.Gender?.value || null,
                ActivityLevel: elements.ActivityLevel?.value || null
            };

            try {
                const response = await fetch('/profile/auto-save', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    },
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    if (saveIndicator) {
                        saveIndicator.innerText = "Збережено ✔️";
                        saveIndicator.classList.remove('bg-warning', 'text-dark');
                        saveIndicator.classList.add('bg-success');
                        setTimeout(() => saveIndicator.classList.add('d-none'), 3000);
                    }
                } else {
                    if (saveIndicator) {
                        saveIndicator.innerText = "Помилка";
                        saveIndicator.classList.remove('bg-warning', 'text-dark');
                        saveIndicator.classList.add('bg-danger');
                    }
                }
            } catch (error) {
                console.error("Auto-save error", error);
                if (saveIndicator) {
                    saveIndicator.innerText = "Помилка";
                    saveIndicator.classList.remove('bg-warning', 'text-dark');
                    saveIndicator.classList.add('bg-danger');
                }
            }
        }

        inputs.forEach(id => {
            if (elements[id]) {
                elements[id].addEventListener('input', function() {
                    calculateMetrics();
                    clearTimeout(saveTimeout);
                    saveTimeout = setTimeout(autoSave, 1500);
                });
            }
        });

        calculateMetrics();
    });
})();
