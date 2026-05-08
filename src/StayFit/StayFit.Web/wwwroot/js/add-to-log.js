(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initAddToLog();
    });

    function initAddToLog() {
        var qtyEl = document.getElementById('sf-atl-qty');
        if (!qtyEl) return;

        var recalc = function () {
            var kcalEl = document.getElementById('sf-atl-calc-kcal');
            if (!kcalEl) return;

            var kcalPer100 = parseFloat(kcalEl.getAttribute('data-per100')) || 0;
            var protEl = document.getElementById('sf-atl-calc-protein');
            var fatEl = document.getElementById('sf-atl-calc-fat');
            var carbEl = document.getElementById('sf-atl-calc-carbs');

            var protPer100 = protEl ? (parseFloat(protEl.getAttribute('data-per100')) || 0) : 0;
            var fatPer100 = fatEl ? (parseFloat(fatEl.getAttribute('data-per100')) || 0) : 0;
            var carbPer100 = carbEl ? (parseFloat(carbEl.getAttribute('data-per100')) || 0) : 0;

            var qty = parseFloat(qtyEl.value) || 0;
            var factor = qty / 100;

            kcalEl.textContent = (kcalPer100 * factor).toFixed(0);
            if (protEl) protEl.textContent = (protPer100 * factor).toFixed(1);
            if (fatEl) fatEl.textContent = (fatPer100 * factor).toFixed(1);
            if (carbEl) carbEl.textContent = (carbPer100 * factor).toFixed(1);
        };

        qtyEl.addEventListener('input', recalc);
        
        recalc();

        var searchEl = document.getElementById('sf-atl-search');
        var resultsEl = document.getElementById('sf-atl-results');
        var hintEl = document.getElementById('sf-atl-hint');
        var searchTimer;

        if (searchEl) {
            searchEl.addEventListener('input', function () {
                clearTimeout(searchTimer);
                var q = searchEl.value.trim();
                if (q.length < 2) {
                    resultsEl.innerHTML = '';
                    if (hintEl) resultsEl.appendChild(hintEl);
                    return;
                }
                searchTimer = setTimeout(function () {
                    fetch('/Food/SearchJson?q=' + encodeURIComponent(q))
                        .then(function(r) { return r.json(); })
                        .then(function(data) { renderResults(data, resultsEl); })
                        .catch(function() { resultsEl.innerHTML = '<div class="text-danger small text-center py-3">Помилка пошуку</div>'; });
                }, 300);
            });
        }
    }

    function renderResults(items, resultsEl) {
        resultsEl.innerHTML = '';
        if (!items || items.length === 0) {
            resultsEl.innerHTML = '<div class="text-muted small text-center py-3">Нічого не знайдено</div>';
            return;
        }
        items.forEach(function(f) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action d-flex justify-content-between align-items-center';
            
            var category = f.category ? f.category : '';
            btn.innerHTML = '<div>' +
                  '<div class="fw-semibold">' + escHtml(f.name) + '</div>' +
                  '<small class="text-muted">' + Math.round(f.caloriesPer100g) + ' ккал / 100г</small>' +
                '</div>' +
                '<span class="badge bg-primary-subtle text-primary rounded-pill">' + escHtml(category) + '</span>';
            
            btn.addEventListener('click', function () {
                selectFood(f);
                var listItems = resultsEl.querySelectorAll('.list-group-item');
                for (var i = 0; i < listItems.length; i++) {
                    listItems[i].classList.remove('active');
                }
                btn.classList.add('active');
            });
            resultsEl.appendChild(btn);
        });
    }

    function selectFood(food) {
        var kcalEl = document.getElementById('sf-atl-calc-kcal');
        if (!kcalEl) return;

        kcalEl.setAttribute('data-per100', food.caloriesPer100g);
        document.getElementById('sf-atl-calc-protein').setAttribute('data-per100', food.proteinPer100g);
        document.getElementById('sf-atl-calc-fat').setAttribute('data-per100', food.fatPer100g);
        document.getElementById('sf-atl-calc-carbs').setAttribute('data-per100', food.carbsPer100g);

        document.getElementById('sf-atl-foodId').value = food.id;
        document.getElementById('sf-atl-name').textContent = food.name;
        document.getElementById('sf-atl-category').textContent = food.category ? food.category : '';
        document.getElementById('sf-atl-kcal').textContent = Math.round(food.caloriesPer100g);
        document.getElementById('sf-atl-protein').textContent = food.proteinPer100g.toFixed(1);
        document.getElementById('sf-atl-fat').textContent = food.fatPer100g.toFixed(1);
        document.getElementById('sf-atl-carbs').textContent = food.carbsPer100g.toFixed(1);

        document.getElementById('sf-atl-no-selection').classList.add('d-none');
        document.getElementById('sf-atl-form-container').classList.remove('d-none');
        
        var qtyEl = document.getElementById('sf-atl-qty');
        if (qtyEl) {
            qtyEl.value = 100;
            qtyEl.dispatchEvent(new Event('input'));
        }
    }

    function escHtml(str) {
        return String(str).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
    }

})();
