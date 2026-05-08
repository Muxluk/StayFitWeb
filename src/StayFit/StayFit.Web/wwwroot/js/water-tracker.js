(function () {
    var STORAGE_KEY = 'sf_water_' + new Date().toISOString().slice(0, 10);
    var GOAL = 8;

    function load() {
        return parseInt(localStorage.getItem(STORAGE_KEY) || '0', 10);
    }

    function save(val) {
        localStorage.setItem(STORAGE_KEY, val);
    }

    function render(count) {
        var intake = document.getElementById('waterIntake');
        var dropsContainer = document.getElementById('waterDrops');
        if (!dropsContainer) return;

        var card = dropsContainer.closest('.card');
        var bar = card ? card.querySelector('.sf-progress-bar') : null;
        
        if (intake) intake.textContent = count;
        if (bar)    bar.style.width = Math.min(count / GOAL * 100, 100) + '%';
        
        dropsContainer.innerHTML = '';
        for (var i = 1; i <= GOAL; i++) {
            (function(idx) {
                var span = document.createElement('span');
                span.className = 'sf-drop' + (idx <= count ? ' active' : '');
                span.textContent = '○';
                span.title = idx + ' склянка';
                span.addEventListener('click', function () {
                    var cur = load();
                    var newVal = (idx <= cur && idx === cur) ? cur - 1 : idx;
                    save(newVal < 0 ? 0 : newVal);
                    render(load());
                });
                dropsContainer.appendChild(span);
            })(i);
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (!document.getElementById('waterDrops')) return;
        
        render(load());
        var resetBtn = document.getElementById('waterResetBtn');
        if (resetBtn) resetBtn.addEventListener('click', function () {
            save(0); render(0);
        });
    });
})();
