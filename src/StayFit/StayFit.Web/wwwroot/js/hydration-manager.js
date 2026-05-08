(function () {
    const STORAGE_KEY = 'sf_water_' + new Date().toISOString().slice(0, 10);
    const HISTORY_KEY = 'sf_water_history_' + new Date().toISOString().slice(0, 10);
    const GOAL = 8;

    function load() {
        return parseInt(localStorage.getItem(STORAGE_KEY) || '0', 10);
    }

    function save(val) {
        localStorage.setItem(STORAGE_KEY, Math.max(0, val));
    }

    function getHistory() {
        try {
            return JSON.parse(localStorage.getItem(HISTORY_KEY) || '[]');
        } catch {
            return [];
        }
    }

    function saveHistory(entries) {
        localStorage.setItem(HISTORY_KEY, JSON.stringify(entries));
    }

    function addToHistory(glasses) {
        const history = getHistory();
        history.push({
            time: new Date().toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' }),
            glasses: glasses
        });
        saveHistory(history);
    }

    function render(count) {
        const intake = document.getElementById('waterIntake');
        const drops = document.getElementById('waterDrops');
        const progressBar = document.querySelector('.sf-progress-bar');

        if (intake) intake.textContent = count;
        if (!drops) return;

        // Анімація прогрес бару
        if (progressBar) {
            progressBar.classList.add('pulse');
            setTimeout(() => progressBar.classList.remove('pulse'), 800);
        }

        // Крапельки
        drops.innerHTML = '';
        for (let i = 1; i <= GOAL; i++) {
            const span = document.createElement('span');
            span.className = 'sf-drop' + (i <= count ? ' active' : '');
            span.textContent = '💧';
            span.title = i + ' склянка';
            span.style.cursor = 'pointer';
            span.addEventListener('click', function () {
                const cur = load();
                const newVal = (i <= cur && i === cur) ? cur - 1 : i;
                const final = Math.max(0, newVal);
                const diff = final - cur;
                save(final);
                if (diff > 0) addToHistory(diff);
                render(final);
                updateHistory();
                updateProgressBar(final);
            });
            drops.appendChild(span);
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
        const history = getHistory();
        const container = document.getElementById('waterHistory');
        if (!container) return;
        
        if (history.length === 0) {
            container.innerHTML = '<div class="list-group-item text-muted text-center py-4">Поки немає записів. Почніть додавати воду!</div>';
            return;
        }

        container.innerHTML = history.map((entry, idx) => `
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                    <span class="fw-semibold">${entry.time}</span>
                    <span class="badge bg-info ms-2">${entry.glasses} ${entry.glasses === 1 ? 'склянка' : 'склянок'}</span>
                </div>
                <button class="btn btn-sm btn-outline-danger" data-idx="${idx}" onclick="removeHistoryEntry(${idx})">
                    ✕
                </button>
            </div>
        `).join('');
    }

    window.removeHistoryEntry = function(idx) {
        const history = getHistory();
        if (idx < 0 || idx >= history.length) return;
        history.splice(idx, 1);
        saveHistory(history);
        updateHistory();
        const total = history.reduce((sum, e) => sum + e.glasses, 0);
        save(total);
        render(total);
        updateProgressBar(total);
    };

    document.addEventListener('DOMContentLoaded', function () {
        if (!document.getElementById('waterHistory')) return;

        const count = load();
        render(count);
        updateHistory();
        updateProgressBar(count);

        document.querySelectorAll('.water-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                const glasses = parseInt(this.dataset.glasses, 10);
                const cur = load();
                const newVal = cur + glasses;
                save(newVal);
                addToHistory(glasses);
                render(newVal);
                updateHistory();
                updateProgressBar(newVal);
                
                this.classList.add('active');
                setTimeout(() => this.classList.remove('active'), 200);
            });
        });

        const resetBtn = document.getElementById('waterResetBtn');
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
