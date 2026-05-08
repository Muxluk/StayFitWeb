(function () {
    // Character counter for notes
    document.querySelectorAll('.meal-note').forEach(textarea => {
        textarea.addEventListener('input', function() {
            const mealId = this.getAttribute('data-meal-id');
            const counter = document.getElementById(`charcount_${mealId}`);
            if (counter) counter.textContent = this.value.length;
        });
    });

    // Update note button handler
    document.querySelectorAll('.update-note-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const mealId = this.getAttribute('data-meal-id');
            const noteText = document.getElementById(`note_${mealId}`).value;
            const actionUrl = this.getAttribute('data-action-url');

            if (!actionUrl) {
                console.error('Action URL not found on button');
                return;
            }

            fetch(actionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `mealId=${mealId}&note=${encodeURIComponent(noteText)}`
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Нотатка збережена!');
                } else {
                    alert(data.error || 'Помилка при збереженні нотатки');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Помилка при збереженні нотатки');
            });
        });
    });
})();
