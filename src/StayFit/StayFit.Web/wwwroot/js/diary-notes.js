// Diary Notes Handler
// Handles character counting and note saving for food logs

(function() {
    'use strict';

    // Character counter for notes
    document.querySelectorAll('.note-textarea').forEach(textarea => {
        textarea.addEventListener('input', function() {
            const logId = this.dataset.logId;
            const charCount = document.getElementById(`charcount_${logId}`);
            if (charCount) {
                charCount.textContent = this.value.length;
            }
        });
    });

    // Save note button handler
    document.querySelectorAll('.save-note-btn').forEach(button => {
        button.addEventListener('click', function() {
            const logId = this.dataset.logId;
            const textarea = document.getElementById(`note_${logId}`);
            if (!textarea) {
                return;
            }

            const noteText = textarea.value;
            const originalButtonText = this.innerHTML;
            this.disabled = true;
            this.innerHTML = '<i class="bi bi-hourglass-split"></i> Збереження...';

            fetch(window.diaryUpdateUrl || '/Diary/UpdateFoodLogNote', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: `logId=${encodeURIComponent(logId)}&note=${encodeURIComponent(noteText)}`
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    this.innerHTML = '<i class="bi bi-check-circle"></i> Збережено!';
                    setTimeout(() => {
                        this.innerHTML = originalButtonText;
                        this.disabled = false;
                    }, 2000);
                } else {
                    throw new Error(data.error || 'Помилка при збереженні нотатки');
                }
            })
            .catch(error => {
                console.error('Error saving note:', error);
                this.innerHTML = '<i class="bi bi-exclamation-circle"></i> Помилка';
                this.classList.add('btn-danger');
                this.classList.remove('btn-outline-success');
                setTimeout(() => {
                    this.innerHTML = originalButtonText;
                    this.classList.remove('btn-danger');
                    this.classList.add('btn-outline-success');
                    this.disabled = false;
                }, 3000);
            });
        });
    });
})();
