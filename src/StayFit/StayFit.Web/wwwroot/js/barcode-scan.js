(function () {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function () {
        var form = document.getElementById('sf-bc-form');
        var btn = document.getElementById('sf-bc-btn');
        var btnText = document.getElementById('sf-bc-btn-text');
        var btnSpin = document.getElementById('sf-bc-btn-spin');
        
        if (!form || !btn) return;
        
        form.addEventListener('submit', function () {
            btn.disabled = true;
            if (btnText) btnText.classList.add('d-none');
            if (btnSpin) btnSpin.classList.remove('d-none');
        });
    });
})();
