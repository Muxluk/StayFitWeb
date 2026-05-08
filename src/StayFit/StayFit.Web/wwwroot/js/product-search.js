(function () {
    var form     = document.getElementById('sf-ps-form');
    var results  = document.getElementById('sf-ps-results');
    var skeleton = document.getElementById('sf-ps-skeleton');
    var btn      = document.getElementById('sf-ps-btn');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        var action = form.getAttribute('action') || location.href;
        var query = "";
        var formData = new FormData(form);
        
        // Manual query string building for ES5 compatibility (avoiding URL object if possible)
        // or just use location.origin if URL is available (IE11 doesn't have it, but minifier might be the issue)
        var url = new URL(action, location.origin);
        url.searchParams.delete('page');
        
        // FormData.forEach is ES6
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

        history.pushState(null, '', url.toString());

        fetch(url.toString(), { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function(r) { return r.text(); })
            .then(function(html) {
                var parser = new DOMParser();
                var doc   = parser.parseFromString(html, 'text/html');
                var fresh = doc.getElementById('sf-ps-results');
                if (fresh && results) results.innerHTML = fresh.innerHTML;
            })
            ['catch'](function() { location.href = url.toString(); })
            ['finally'](function() {
                if (skeleton) skeleton.classList.add('d-none');
                if (results) results.classList.remove('d-none');
                if (btn) btn.disabled = false;
            });
    });
})();
