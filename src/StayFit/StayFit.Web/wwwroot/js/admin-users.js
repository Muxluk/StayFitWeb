(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initAdminUsers();
    });

    function initAdminUsers() {
        var timeout = null;
        var emailInput = document.getElementById('emailSearchInput');
        var autocompleteList = document.getElementById('emailAutocompleteList');
        var searchForm = document.getElementById('searchForm');
        
        if (!emailInput || !autocompleteList || !searchForm) return;

        document.addEventListener('click', function(e) {
            if (e.target !== emailInput && e.target !== autocompleteList) {
                autocompleteList.classList.add('d-none');
            }
        });

        emailInput.addEventListener('input', function() {
            clearTimeout(timeout);
            var query = emailInput.value.trim();
            
            if (query.length < 2) {
                autocompleteList.classList.add('d-none');
                autocompleteList.innerHTML = '';
                
                if (query.length === 0) {
                    timeout = setTimeout(function() { searchForm.submit(); }, 800);
                }
                return;
            }

            timeout = setTimeout(function() {
                fetch('/admin/users/autocomplete?q=' + encodeURIComponent(query))
                    .then(function(response) { return response.json(); })
                    .then(function(data) {
                        autocompleteList.innerHTML = '';
                        
                        if (data && data.length > 0) {
                            for (var i = 0; i < data.length; i++) {
                                (function(email) {
                                    var li = document.createElement('li');
                                    li.className = 'list-group-item list-group-item-action cursor-pointer';
                                    li.textContent = email;
                                    li.style.cursor = 'pointer';
                                    
                                    li.addEventListener('click', function() {
                                        emailInput.value = email;
                                        autocompleteList.classList.add('d-none');
                                        searchForm.submit();
                                    });
                                    
                                    autocompleteList.appendChild(li);
                                })(data[i]);
                            }
                            autocompleteList.classList.remove('d-none');
                        } else {
                            autocompleteList.classList.add('d-none');
                        }
                    })
                    .catch(function(err) { console.error('Autocomplete error:', err); });
            }, 300);
        });
    }
})();
