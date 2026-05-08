(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initExport();
    });

    function initExport() {
        var radios = document.querySelectorAll('.export-format-option input[type="radio"]');
        for (var i = 0; i < radios.length; i++) {
            radios[i].addEventListener('change', function() {
                var options = document.querySelectorAll('.export-format-option');
                for (var j = 0; j < options.length; j++) {
                    options[j].classList.remove('active');
                }
                this.closest('.export-format-option').classList.add('active');
            });
        }

        var fromInput = document.querySelector('input[name="From"]');
        var toInput = document.querySelector('input[name="To"]');
        var info = document.getElementById('recordCountInfo');

        var updateCount = function () {
            var fromDate = fromInput ? fromInput.value : null;
            var toDate = toInput ? toInput.value : null;

            if (fromDate && toDate && info) {
                fetch('/export/record-count?from=' + encodeURIComponent(fromDate) + '&to=' + encodeURIComponent(toDate))
                    .then(function(response) { 
                        if (response.ok) return response.json();
                        throw new Error('Network response was not ok');
                    })
                    .then(function(data) {
                        if (data.count > 0) {
                            if (data.count > 10000) {
                                info.className = 'alert alert-warning mt-3 small';
                                info.innerHTML = '<i class="bi bi-exclamation-triangle"></i> <strong>Велика кількість записів (' + data.count + ')!</strong> Файл може бути великим (орієнтовно ' + Math.round(data.count / 1000) + 'MB). Це може зайняти деякий час.';
                            } else {
                                info.className = 'alert alert-success mt-3 small';
                                info.innerHTML = '<i class="bi bi-check-circle"></i> Буде експортовано <strong>' + data.count + '</strong> записів';
                            }
                        } else {
                            info.className = 'alert alert-info mt-3 small';
                            info.innerHTML = '<i class="bi bi-info-circle"></i> За обраним періодом немає записів';
                        }
                    })
                    .catch(function(error) {
                        console.error('Error fetching record count:', error);
                    });
            }
        };

        if (fromInput) fromInput.addEventListener('change', updateCount);
        if (toInput) toInput.addEventListener('change', updateCount);

        if (fromInput && fromInput.value && toInput && toInput.value) {
            updateCount();
        }
    }
})();
