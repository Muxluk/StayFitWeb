(function () {
    const form     = document.getElementById('sf-ps-form');
    const results  = document.getElementById('sf-ps-results');
    const skeleton = document.getElementById('sf-ps-skeleton');
    const btn      = document.getElementById('sf-ps-btn');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        const url = new URL(form.action || location.href, location.origin);
        url.searchParams.delete('page');
        new FormData(form).forEach((v, k) => {
            if (v) url.searchParams.set(k, v);
            else   url.searchParams.delete(k);
        });

        if (results) results.classList.add('d-none');
        if (skeleton) skeleton.classList.remove('d-none');
        if (btn) btn.disabled = true;

        history.pushState(null, '', url.toString());

        fetch(url.toString(), { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(r => r.text())
            .then(html => {
                const doc   = new DOMParser().parseFromString(html, 'text/html');
                const fresh = doc.getElementById('sf-ps-results');
                if (fresh && results) results.innerHTML = fresh.innerHTML;
            })
            .catch(() => { location.href = url.toString(); })
            .finally(() => {
                if (skeleton) skeleton.classList.add('d-none');
                if (results) results.classList.remove('d-none');
                if (btn) btn.disabled = false;
            });
    });
})();
