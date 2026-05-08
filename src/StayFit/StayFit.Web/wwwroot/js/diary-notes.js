// Diary Notes Handler
// Handles character counting and note saving for food logs

(function() {
    'use strict';

    // Character counter for notes
    var textareas = document.querySelectorAll('.note-textarea');
    for (var i = 0; i < textareas.length; i++) {
        textareas[i].addEventListener('input', function() {
            var logId = this.getAttribute('data-log-id');
            var charCount = document.getElementById('charcount_' + logId);
            if (charCount) {
                charCount.textContent = this.value.length;
            }
        });
    }

    // Save note button handler
    var buttons = document.querySelectorAll('.save-note-btn');
    for (var j = 0; j < buttons.length; j++) {
        buttons[j].addEventListener('click', function() {
            var self = this;
            var logId = self.getAttribute('data-log-id');
            var textarea = document.getElementById('note_' + logId);
            if (!textarea) {
                return;
            }

            var noteText = textarea.value;
            var originalButtonText = self.innerHTML;
            self.disabled = true;
            self.innerHTML = '<i class="bi bi-hourglass-split"></i> Збереження...';

            fetch(window.diaryUpdateUrl || '/Diary/UpdateFoodLogNote', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: 'logId=' + encodeURIComponent(logId) + '&note=' + encodeURIComponent(noteText)
            })
            .then(function(response) {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(function(data) {
                if (data.success) {
                    self.innerHTML = '<i class="bi bi-check-circle"></i> Збережено!';
                    setTimeout(function() {
                        self.innerHTML = originalButtonText;
                        self.disabled = false;
                    }, 2000);
                } else {
                    throw new Error(data.error || 'Помилка при збереженні нотатки');
                }
            })
            .catch(function(error) {
                console.error('Error saving note:', error);
                self.innerHTML = '<i class="bi bi-exclamation-circle"></i> Помилка';
                self.classList.add('btn-danger');
                self.classList.remove('btn-outline-success');
                setTimeout(function() {
                    self.innerHTML = originalButtonText;
                    self.classList.remove('btn-danger');
                    self.classList.add('btn-outline-success');
                    self.disabled = false;
                }, 3000);
            });
        });
    }
})();
