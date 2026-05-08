(function () {
    const form     = document.getElementById('sf-fi-form');
    const skeleton = document.getElementById('sf-fi-skeleton');
    const results  = document.getElementById('sf-fi-results');
    const btn      = document.getElementById('sf-fi-btn');
    const btnText  = document.getElementById('sf-fi-btn-text');
    const btnSpin  = document.getElementById('sf-fi-btn-spinner');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        const url = new URL(form.action || location.href, location.origin);
        new FormData(form).forEach((v, k) => {
            if (v) url.searchParams.set(k, v);
            else   url.searchParams.delete(k);
        });

        if (results) results.classList.add('d-none');
        if (skeleton) skeleton.classList.remove('d-none');
        if (btn) btn.disabled = true;
        if (btnText) btnText.classList.add('d-none');
        if (btnSpin) btnSpin.classList.remove('d-none');

        history.pushState(null, '', url.toString());

        fetch(url.toString(), { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(r => r.text())
            .then(html => {
                const doc   = new DOMParser().parseFromString(html, 'text/html');
                const fresh = doc.getElementById('sf-fi-results');
                if (fresh && results) results.innerHTML = fresh.innerHTML;
            })
            .catch(() => { location.href = url.toString(); })
            .finally(() => {
                if (skeleton) skeleton.classList.add('d-none');
                if (results) results.classList.remove('d-none');
                if (btn) btn.disabled = false;
                if (btnText) btnText.classList.remove('d-none');
                if (btnSpin) btnSpin.classList.add('d-none');
            });
    });
})();
