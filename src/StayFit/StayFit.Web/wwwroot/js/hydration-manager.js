(function () {
    var STORAGE_KEY = 'sf_water_' + new Date().toISOString().slice(0, 10);
    var HISTORY_KEY = 'sf_water_history_' + new Date().toISOString().slice(0, 10);
    var GOAL = 8;

    function load() {
        return parseInt(localStorage.getItem(STORAGE_KEY) || '0', 10);
    }

    function save(val) {
        localStorage.setItem(STORAGE_KEY, Math.max(0, val));
    }

    function getHistory() {
        try {
            return JSON.parse(localStorage.getItem(HISTORY_KEY) || '[]');
        } catch(e) {
            return [];
        }
    }

    function saveHistory(entries) {
        localStorage.setItem(HISTORY_KEY, JSON.stringify(entries));
    }

    function addToHistory(glasses) {
        var history = getHistory();
        history.push({
            time: new Date().toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' }),
            glasses: glasses
        });
        saveHistory(history);
    }

    function render(count) {
        var intake = document.getElementById('waterIntake');
        var drops = document.getElementById('waterDrops');
        var progressBar = document.querySelector('.sf-progress-bar');

        if (intake) intake.textContent = count;
        if (!drops) return;

        if (progressBar) {
            progressBar.classList.add('pulse');
            setTimeout(function() { progressBar.classList.remove('pulse'); }, 800);
        }

        drops.innerHTML = '';
        for (var i = 1; i <= GOAL; i++) {
            (function(idx) {
                var span = document.createElement('span');
                span.className = 'sf-drop' + (idx <= count ? ' active' : '');
                span.textContent = '💧';
                span.title = idx + ' склянка';
                span.style.cursor = 'pointer';
                span.addEventListener('click', function () {
                    var cur = load();
                    var newVal = (idx <= cur && idx === cur) ? cur - 1 : idx;
                    var finalValue = Math.max(0, newVal);
                    var diff = finalValue - cur;
                    save(finalValue);
                    if (diff > 0) addToHistory(diff);
                    render(finalValue);
                    updateHistory();
                    updateProgressBar(finalValue);
                });
                drops.appendChild(span);
            })(i);
        }
    }

    function updateProgressBar(count) {
        var pct = count / GOAL * 100;
        var displayPct = Math.min(pct, 100);
        var bar = document.querySelector('.sf-progress-bar');
        if (bar) {
            bar.style.width = displayPct + '%';
            bar.className = count > GOAL ? 'sf-progress-bar bg-danger' : 'sf-progress-bar bg-info';
        }
        var label = document.querySelector('.sf-progress-pct');
        if (label) label.textContent = displayPct.toFixed(0) + '%';
        var warning = document.getElementById('waterWarning');
        if (warning) warning.style.display = count > GOAL ? 'block' : 'none';
    }

    function updateHistory() {
        var history = getHistory();
        var container = document.getElementById('waterHistory');
        if (!container) return;
        
        if (history.length === 0) {
            container.innerHTML = '<div class="list-group-item text-muted text-center py-4">Поки немає записів. Почніть додавати воду!</div>';
            return;
        }

        var html = '';
        for (var i = 0; i < history.length; i++) {
            var entry = history[i];
            var label = entry.glasses === 1 ? 'склянка' : 'склянок';
            html += '<div class="list-group-item d-flex justify-content-between align-items-center">' +
                '<div>' +
                    '<span class="fw-semibold">' + entry.time + '</span>' +
                    '<span class="badge bg-info ms-2">' + entry.glasses + ' ' + label + '</span>' +
                '</div>' +
                '<button class="btn btn-sm btn-outline-danger" onclick="removeHistoryEntry(' + i + ')">' +
                    '✕' +
                '</button>' +
            '</div>';
        }
        container.innerHTML = html;
    }

    window.removeHistoryEntry = function(idx) {
        var history = getHistory();
        if (idx < 0 || idx >= history.length) return;
        history.splice(idx, 1);
        saveHistory(history);
        updateHistory();
        var total = 0;
        for (var i = 0; i < history.length; i++) {
            total += history[i].glasses;
        }
        save(total);
        render(total);
        updateProgressBar(total);
    };

    document.addEventListener('DOMContentLoaded', function () {
        if (!document.getElementById('waterHistory')) return;

        var count = load();
        render(count);
        updateHistory();
        updateProgressBar(count);

        var buttons = document.querySelectorAll('.water-btn');
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].addEventListener('click', function () {
                var self = this;
                var glasses = parseInt(self.getAttribute('data-glasses'), 10);
                var cur = load();
                var newVal = cur + glasses;
                save(newVal);
                addToHistory(glasses);
                render(newVal);
                updateHistory();
                updateProgressBar(newVal);
                
                self.classList.add('active');
                setTimeout(function() { self.classList.remove('active'); }, 200);
            });
        }

        var resetBtn = document.getElementById('waterResetBtn');
        if (resetBtn) {
            resetBtn.addEventListener('click', function () {
                if (confirm('Ви впевнені? Це скине всі записи на сьогодні.')) {
                    save(0);
                    localStorage.removeItem(HISTORY_KEY);
                    render(0);
                    updateHistory();
                    updateProgressBar(0);
                }
            });
        }
    });
})();
