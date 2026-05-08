(function () {
    // --- LOGIN FORM ---
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        const emailInput = document.getElementById('emailInput');
        const passwordInput = document.getElementById('passwordInput');
        const submitBtn = document.getElementById('submitBtn');
        const submitText = document.getElementById('submitText');
        const submitSpinner = document.getElementById('submitSpinner');
        const loginAlert = document.getElementById('loginAlert');
        const loginAlertText = document.getElementById('loginAlertText');
        const retryBtn = document.getElementById('retryBtn');

        const validateEmail = (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

        const checkValidity = (element, validator) => {
            if (element.value.trim() === '') {
                element.classList.remove('is-invalid', 'is-valid');
                return false;
            }
            const isValid = validator(element.value);
            if (isValid) {
                element.classList.remove('is-invalid');
                element.classList.add('is-valid');
            } else {
                element.classList.remove('is-valid');
                element.classList.add('is-invalid');
            }
            return isValid;
        };

        if (emailInput) emailInput.addEventListener('input', () => checkValidity(emailInput, validateEmail));
        if (passwordInput) passwordInput.addEventListener('input', () => checkValidity(passwordInput, val => val.length >= 8));

        loginForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            const isEmailValid = checkValidity(emailInput, validateEmail);
            const isPasswordValid = checkValidity(passwordInput, val => val.length >= 8);
            if (!isEmailValid || !isPasswordValid) return;
            submitLoginForm();
        });

        if (retryBtn) retryBtn.addEventListener('click', () => {
            loginAlert.classList.add('d-none');
            submitLoginForm();
        });

        async function submitLoginForm() {
            submitBtn.disabled = true;
            if (submitText) submitText.classList.add('opacity-0');
            if (submitSpinner) submitSpinner.classList.remove('d-none');
            if (loginAlert) loginAlert.classList.add('d-none');

            try {
                const response = await fetch(loginForm.action, {
                    method: 'POST',
                    body: new FormData(loginForm),
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (response.redirected) {
                    submitBtn.classList.replace('btn-primary', 'btn-success');
                    if (submitSpinner) submitSpinner.classList.add('d-none');
                    if (submitText) {
                        submitText.innerHTML = '<i class="bi bi-check2-circle me-2"></i>Успішно';
                        submitText.classList.remove('opacity-0');
                    }
                    setTimeout(() => window.location.href = response.url, 500);
                    return;
                }

                const html = await response.text();
                const doc = new DOMParser().parseFromString(html, 'text/html');
                const errors = doc.querySelector('.validation-summary-errors');
                if (errors) {
                    if (loginAlertText) loginAlertText.textContent = errors.textContent.trim();
                    if (loginAlert) loginAlert.classList.remove('d-none');
                    emailInput.classList.add('is-invalid');
                    passwordInput.classList.add('is-invalid');
                } else {
                    window.location.reload();
                }
            } catch (error) {
                if (loginAlertText) loginAlertText.textContent = 'Помилка з\'єднання. Перевірте інтернет.';
                if (loginAlert) loginAlert.classList.remove('d-none');
            } finally {
                submitBtn.disabled = false;
                if (submitText) submitText.classList.remove('opacity-0');
                if (submitSpinner) submitSpinner.classList.add('d-none');
            }
        }
    }

    // --- REGISTER FORM ---
    const regForm = document.getElementById('registerForm');
    if (regForm) {
        const password = document.getElementById('password');
        const confirmPassword = document.getElementById('confirmPassword');
        const confirmError = document.getElementById('confirmPasswordError');

        const checkPasswords = () => {
            if (confirmPassword.value && password.value !== confirmPassword.value) {
                confirmPassword.classList.add('is-invalid');
                confirmError.textContent = "Паролі не співпадають";
                return false;
            } else if (confirmPassword.value) {
                confirmPassword.classList.replace('is-invalid', 'is-valid');
                confirmError.textContent = "";
                return true;
            }
            return true;
        };

        if (password) password.addEventListener('input', checkPasswords);
        if (confirmPassword) confirmPassword.addEventListener('input', checkPasswords);

        regForm.addEventListener('submit', function (e) {
            if (!regForm.checkValidity() || !checkPasswords()) {
                e.preventDefault();
                e.stopPropagation();
                Array.from(regForm.elements).forEach(input => {
                    if (!input.checkValidity()) input.classList.add('is-invalid');
                });
            } else {
                const btn = document.getElementById('submitBtn');
                if (btn) btn.disabled = true;
                const txt = document.getElementById('submitText');
                if (txt) txt.classList.add('opacity-0');
                const spn = document.getElementById('submitSpinner');
                if (spn) spn.classList.remove('d-none');
            }
            regForm.classList.add('was-validated');
        });
    }

    // --- FORGOT PASSWORD ---
    const forgotForm = document.getElementById('forgotForm');
    if (forgotForm) {
        const emailInput = document.getElementById('emailInput');
        const submitBtn = document.getElementById('submitBtn');
        const resendBtn = document.getElementById('resendBtn');
        
        forgotForm.addEventListener('submit', e => {
            e.preventDefault();
            if (!emailInput.value.trim() || !emailInput.checkValidity()) {
                emailInput.classList.add('is-invalid');
                return;
            }
            submitForgot();
        });

        if (resendBtn) resendBtn.addEventListener('click', submitForgot);

        async function submitForgot() {
            submitBtn.disabled = true;
            const alertMsg = document.getElementById('alertMsg');
            if (alertMsg) alertMsg.classList.add('d-none');

            try {
                const response = await fetch(forgotForm.action, {
                    method: 'POST',
                    body: new FormData(forgotForm),
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                if (response.ok || response.redirected) {
                    document.getElementById('step1')?.classList.add('d-none');
                    document.getElementById('step2')?.classList.remove('d-none');
                    document.getElementById('sentEmail').textContent = emailInput.value;
                } else {
                    if (alertMsg) {
                        alertMsg.textContent = 'Не вдалося надіслати запит.';
                        alertMsg.classList.remove('d-none');
                    }
                }
            } catch (err) {
                if (alertMsg) {
                    alertMsg.textContent = 'Помилка мережі.';
                    alertMsg.classList.remove('d-none');
                }
            } finally {
                submitBtn.disabled = false;
            }
        }
    }
})();
