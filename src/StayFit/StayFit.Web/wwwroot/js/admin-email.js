(function () {
    var MAX_SUBJECT = 200;
    var MAX_BODY = 5000;

    var emailTemplates = {
        welcome: {
            subject: 'Добро пожалувати до StayFit!',
            body: '<p>Привіт!</p><p>Благодарим за реєстрацію в <b>StayFit</b> — програмі для контролю харчування та здорового способу життя.</p><p>Почніть з встановлення своїх цілей харчування і слідкуйте за прогресом кожного дня.</p><p><a href="https://stayfit.com/nutrition-goal" style="color: #007bff; text-decoration: none;">Встановити цілі</a></p>'
        },
        update: {
            subject: 'Нове оновлення системи',
            body: '<p>Привіт!</p><p>Ми щойно оновили <b>StayFit</b> з новими функціями:</p><ul><li>Новий інтерфейс відстеження води</li><li>Покращена статистика</li><li>Виправлення помилок</li></ul><p>Дякуємо за використання нашого сервісу!</p>'
        },
        maintenance: {
            subject: 'Планове технічне обслуговування',
            body: '<p>Привіт!</p><p>20 травня 2026 року з 22:00 до 23:00 (UTC) буде проведено планове технічне обслуговування серверів.</p><p>Під час цього часу сервіс може бути недоступний.</p><p>Вибачаємо за незручності!</p>'
        }
    };

    function showMessage(message, type) {
        var msgDiv = document.getElementById('formMessage');
        if (!msgDiv) return;
        msgDiv.className = 'alert alert-' + type;
        msgDiv.textContent = message;
        msgDiv.style.display = 'block';
    }

    function init() {
        var subjectInput = document.getElementById('subject');
        var bodyInput = document.getElementById('htmlBody');
        var form = document.getElementById('broadcastForm');

        if (subjectInput) {
            subjectInput.addEventListener('input', function (e) {
                var count = e.target.value.length;
                var counter = document.getElementById('subjectCount');
                var progress = document.getElementById('subjectProgress');
                if (counter) counter.textContent = count + ' / ' + MAX_SUBJECT;
                if (progress) progress.style.width = (count / MAX_SUBJECT * 100) + '%';
            });
        }

        if (bodyInput) {
            bodyInput.addEventListener('input', function (e) {
                var count = e.target.value.length;
                var counter = document.getElementById('bodyCount');
                var progress = document.getElementById('bodyProgress');
                if (counter) counter.textContent = count + ' / ' + MAX_BODY;
                if (progress) progress.style.width = (count / MAX_BODY * 100) + '%';
            });
        }

        if (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();

                var subject = document.getElementById('subject').value.trim();
                var htmlBody = document.getElementById('htmlBody').value.trim();

                if (!subject || !htmlBody) {
                    showMessage('Заповніть усі поля', 'warning');
                    return;
                }

                var sendBtn = document.getElementById('sendBtn');
                sendBtn.disabled = true;
                sendBtn.innerHTML = 'Відправлення...';

                fetch('/admin/email/send', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ subject: subject, htmlBody: htmlBody })
                })
                .then(function(response) {
                    return response.json().then(function(data) {
                        if (response.ok) {
                            showMessage(data.message, 'success');
                            form.reset();
                            document.getElementById('subjectCount').textContent = '0 / 200';
                            document.getElementById('bodyCount').textContent = '0 / 5000';
                            document.getElementById('subjectProgress').style.width = '0%';
                            document.getElementById('bodyProgress').style.width = '0%';
                            setTimeout(function() { location.reload(); }, 2000);
                        } else {
                            showMessage(data.message, 'danger');
                        }
                    });
                })
                ['catch'](function(error) {
                    showMessage('Помилка: ' + error.message, 'danger');
                })
                ['finally'](function() {
                    sendBtn.disabled = false;
                    sendBtn.innerHTML = 'Відправити';
                });
            });
        }

        // Global functions for buttons
        window.loadTemplate = function(templateName) {
            var template = emailTemplates[templateName];
            if (template) {
                var s = document.getElementById('subject');
                var b = document.getElementById('htmlBody');
                if (s) s.value = template.subject;
                if (b) b.value = template.body;
                if (s) s.dispatchEvent(new Event('input'));
                if (b) b.dispatchEvent(new Event('input'));
                showMessage('Шаблон завантажено', 'info');
            }
        };

        window.viewBroadcast = function(subject, htmlBody) {
            var ps = document.getElementById('previewSubject');
            var pb = document.getElementById('previewBody');
            if (ps) ps.textContent = subject;
            if (pb) pb.innerHTML = decodeURIComponent(htmlBody);
            var modalEl = document.getElementById('previewModal');
            if (modalEl && window.bootstrap && window.bootstrap.Modal) {
                var modal = new bootstrap.Modal(modalEl);
                modal.show();
            }
        };

        var previewBtn = document.querySelector('[data-bs-target="#previewModal"]');
        if (previewBtn) {
            previewBtn.addEventListener('click', function() {
                var subject = document.getElementById('subject').value.trim();
                var htmlBody = document.getElementById('htmlBody').value.trim();
                
                if (!subject || !htmlBody) {
                    showMessage('Заповніть тему та текст листа для перегляду', 'warning');
                    return false;
                }
                
                var ps = document.getElementById('previewSubject');
                var pb = document.getElementById('previewBody');
                if (ps) ps.textContent = subject;
                if (pb) pb.innerHTML = htmlBody;
            });
        }
    }

    document.addEventListener('DOMContentLoaded', init);
})();
