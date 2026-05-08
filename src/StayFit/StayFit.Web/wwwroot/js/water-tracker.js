(function () {
    const STORAGE_KEY = 'sf_water_' + new Date().toISOString().slice(0, 10);
    const GOAL = 8;

    function load() {
        return parseInt(localStorage.getItem(STORAGE_KEY) || '0', 10);
    }

    function save(val) {
        localStorage.setItem(STORAGE_KEY, val);
    }

    function render(count) {
        const intake = document.getElementById('waterIntake');
        const drops  = document.getElementById('waterDrops');
        const dropsContainer = document.getElementById('waterDrops');
        if (!dropsContainer) return;

        const card = dropsContainer.closest('.card');
        const bar = card ? card.querySelector('.sf-progress-bar') : null;
        
        if (intake) intake.textContent = count;
        if (bar)    bar.style.width = Math.min(count / GOAL * 100, 100) + '%';
        
        dropsContainer.innerHTML = '';
        for (let i = 1; i <= GOAL; i++) {
            const span = document.createElement('span');
            span.className = 'sf-drop' + (i <= count ? ' active' : '');
            span.textContent = '○';
            span.title = i + ' склянка';
            span.addEventListener('click', function () {
                const cur = load();
                const newVal = (i <= cur && i === cur) ? cur - 1 : i;
                save(newVal < 0 ? 0 : newVal);
                render(load());
            });
            dropsContainer.appendChild(span);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (!document.getElementById('waterDrops')) return;
        
        render(load());
        const resetBtn = document.getElementById('waterResetBtn');
        if (resetBtn) resetBtn.addEventListener('click', function () {
            save(0); render(0);
        });
    });
})();
