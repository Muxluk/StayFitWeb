(function () {
    // ── Категорія ─────────────────────────────────────────────────────────
    var catInput = document.getElementById('categoryInput');
    if (catInput) {
        var catBtns = document.querySelectorAll('.sf-cat-btn');
        for (var i = 0; i < catBtns.length; i++) {
            catBtns[i].addEventListener('click', function () {
                for (var j = 0; j < catBtns.length; j++) {
                    catBtns[j].classList.remove('active');
                }
                this.classList.add('active');
                catInput.value = this.getAttribute('data-value');
            });
        }
    }

    // ── Попередній перегляд порції ────────────────────────────────────────
    var inpCal  = document.getElementById('inp_cal');
    var inpProt = document.getElementById('inp_prot');
    var inpFat  = document.getElementById('inp_fat');
    var inpCarb = document.getElementById('inp_carb');
    var inpG    = document.getElementById('previewGrams');

    var prevCal  = document.getElementById('prev_cal');
    var prevProt = document.getElementById('prev_prot');
    var prevFat  = document.getElementById('prev_fat');
    var prevCarb = document.getElementById('prev_carb');

    function calcPreview() {
        if (!inpG || !prevCal) return;
        
        var g    = parseFloat(inpG.value)    || 0;
        var cal  = parseFloat(inpCal ? inpCal.value : 0)  || 0;
        var prot = parseFloat(inpProt ? inpProt.value : 0) || 0;
        var fat  = parseFloat(inpFat ? inpFat.value : 0)  || 0;
        var carb = parseFloat(inpCarb ? inpCarb.value : 0) || 0;
        var k = g / 100;
        
        if (prevCal) prevCal.textContent  = g ? (cal  * k).toFixed(0) : '–';
        if (prevProt) prevProt.textContent = g ? (prot * k).toFixed(1) : '–';
        if (prevFat) prevFat.textContent  = g ? (fat  * k).toFixed(1) : '–';
        if (prevCarb) prevCarb.textContent = g ? (carb * k).toFixed(1) : '–';
    }

    if (inpG) {
        var els = [inpCal, inpProt, inpFat, inpCarb, inpG];
        for (var k = 0; k < els.length; k++) {
            if (els[k]) els[k].addEventListener('input', calcPreview);
        }
        calcPreview();
    }
})();
