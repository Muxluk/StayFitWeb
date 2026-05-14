(function () {
    // Character counter for notes
    var textareas = document.querySelectorAll('.meal-note');
    for (var i = 0; i < textareas.length; i++) {
        textareas[i].addEventListener('input', function() {
            var mealId = this.getAttribute('data-meal-id');
            var counter = document.getElementById('charcount_' + mealId);
            if (counter) counter.textContent = this.value.length;
        });
    }

    // Update note button handler
    var buttons = document.querySelectorAll('.update-note-btn');
    for (var j = 0; j < buttons.length; j++) {
        buttons[j].addEventListener('click', function() {
            var self = this;
            var mealId = self.getAttribute('data-meal-id');
            var noteInput = document.getElementById('note_' + mealId);
            var noteText = noteInput ? noteInput.value : '';
            var actionUrl = self.getAttribute('data-action-url');

            if (!actionUrl) {
                console.error('Action URL not found on button');
                return;
            }

            fetch(actionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: 'mealId=' + encodeURIComponent(mealId) + '&note=' + encodeURIComponent(noteText)
            })
            .then(function(response) { return response.json(); })
            .then(function(data) {
                if (data.success) {
                    alert('Нотатка збережена!');
                } else {
                    alert(data.error || 'Помилка при збереженні нотатки');
                }
            })
            ['catch'](function(error) {
                console.error('Error:', error);
                alert('Помилка при збереженні нотатки');
            });
        });
    }
})();
