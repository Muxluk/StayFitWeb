(function () {
    // ── Категорія ─────────────────────────────────────────────────────────
    const catInput = document.getElementById('categoryInput');
    if (catInput) {
        document.querySelectorAll('.sf-cat-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                document.querySelectorAll('.sf-cat-btn').forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                catInput.value = this.dataset.value;
            });
        });
    }

    // ── Попередній перегляд порції ────────────────────────────────────────
    const inpCal  = document.getElementById('inp_cal');
    const inpProt = document.getElementById('inp_prot');
    const inpFat  = document.getElementById('inp_fat');
    const inpCarb = document.getElementById('inp_carb');
    const inpG    = document.getElementById('previewGrams');

    const prevCal  = document.getElementById('prev_cal');
    const prevProt = document.getElementById('prev_prot');
    const prevFat  = document.getElementById('prev_fat');
    const prevCarb = document.getElementById('prev_carb');

    function calcPreview() {
        if (!inpG || !prevCal) return;
        
        const g    = parseFloat(inpG.value)    || 0;
        const cal  = parseFloat(inpCal?.value)  || 0;
        const prot = parseFloat(inpProt?.value) || 0;
        const fat  = parseFloat(inpFat?.value)  || 0;
        const carb = parseFloat(inpCarb?.value) || 0;
        const k = g / 100;
        
        if (prevCal) prevCal.textContent  = g ? (cal  * k).toFixed(0) : '–';
        if (prevProt) prevProt.textContent = g ? (prot * k).toFixed(1) : '–';
        if (prevFat) prevFat.textContent  = g ? (fat  * k).toFixed(1) : '–';
        if (prevCarb) prevCarb.textContent = g ? (carb * k).toFixed(1) : '–';
    }

    if (inpG) {
        [inpCal, inpProt, inpFat, inpCarb, inpG].forEach(el => {
            if (el) el.addEventListener('input', calcPreview);
        });
        calcPreview();
    }
})();
