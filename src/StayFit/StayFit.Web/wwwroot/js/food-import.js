(function () {
    var form     = document.getElementById('sf-fi-form');
    var skeleton = document.getElementById('sf-fi-skeleton');
    var results  = document.getElementById('sf-fi-results');
    var btn      = document.getElementById('sf-fi-btn');
    var btnText  = document.getElementById('sf-fi-btn-text');
    var btnSpin  = document.getElementById('sf-fi-btn-spinner');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        var action = form.getAttribute('action') || location.href;
        var url = new URL(action, location.origin);
        var formData = new FormData(form);
        
        var entries = formData.entries();
        var entry = entries.next();
        while (!entry.done) {
            var k = entry.value[0];
            var v = entry.value[1];
            if (v) url.searchParams.set(k, v);
            else   url.searchParams.delete(k);
            entry = entries.next();
        }

        if (results) results.classList.add('d-none');
        if (skeleton) skeleton.classList.remove('d-none');
        if (btn) btn.disabled = true;
        if (btnText) btnText.classList.add('d-none');
        if (btnSpin) btnSpin.classList.remove('d-none');

        history.pushState(null, '', url.toString());

        fetch(url.toString(), { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function(r) { return r.text(); })
            .then(function(html) {
                var parser = new DOMParser();
                var doc   = parser.parseFromString(html, 'text/html');
                var fresh = doc.getElementById('sf-fi-results');
                if (fresh && results) results.innerHTML = fresh.innerHTML;
            })
            ['catch'](function() { location.href = url.toString(); })
            ['finally'](function() {
                if (skeleton) skeleton.classList.add('d-none');
                if (results) results.classList.remove('d-none');
                if (btn) btn.disabled = false;
                if (btnText) btnText.classList.remove('d-none');
                if (btnSpin) btnSpin.classList.add('d-none');
            });
    });
})();
