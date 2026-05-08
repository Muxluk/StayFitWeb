(function () {
    // --- LOGIN FORM ---
    var loginForm = document.getElementById('loginForm');
    if (loginForm) {
        var emailInput = document.getElementById('emailInput');
        var passwordInput = document.getElementById('passwordInput');
        var submitBtn = document.getElementById('submitBtn');
        var submitText = document.getElementById('submitText');
        var submitSpinner = document.getElementById('submitSpinner');
        var loginAlert = document.getElementById('loginAlert');
        var loginAlertText = document.getElementById('loginAlertText');
        var retryBtn = document.getElementById('retryBtn');

        var validateEmail = function(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
        };

        var checkValidity = function(element, validator) {
            if (element.value.trim() === '') {
                element.classList.remove('is-invalid', 'is-valid');
                return false;
            }
            var isValid = validator(element.value);
            if (isValid) {
                element.classList.remove('is-invalid');
                element.classList.add('is-valid');
            } else {
                element.classList.remove('is-valid');
                element.classList.add('is-invalid');
            }
            return isValid;
        };

        if (emailInput) {
            emailInput.addEventListener('input', function() {
                checkValidity(emailInput, validateEmail);
            });
        }
        if (passwordInput) {
            passwordInput.addEventListener('input', function() {
                checkValidity(passwordInput, function(val) { return val.length >= 8; });
            });
        }

        var submitLoginForm = function() {
            submitBtn.disabled = true;
            if (submitText) submitText.classList.add('opacity-0');
            if (submitSpinner) submitSpinner.classList.remove('d-none');
            if (loginAlert) loginAlert.classList.add('d-none');

            var formData = new FormData(loginForm);

            fetch(loginForm.action, {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
            .then(function(response) {
                if (response.redirected) {
                    submitBtn.classList.replace('btn-primary', 'btn-success');
                    if (submitSpinner) submitSpinner.classList.add('d-none');
                    if (submitText) {
                        submitText.innerHTML = '<i class="bi bi-check2-circle me-2"></i>Успішно';
                        submitText.classList.remove('opacity-0');
                    }
                    setTimeout(function() { window.location.href = response.url; }, 500);
                    return;
                }

                return response.text().then(function(html) {
                    var parser = new DOMParser();
                    var doc = parser.parseFromString(html, 'text/html');
                    var errors = doc.querySelector('.validation-summary-errors');
                    if (errors) {
                        if (loginAlertText) loginAlertText.textContent = errors.textContent.trim();
                        if (loginAlert) loginAlert.classList.remove('d-none');
                        emailInput.classList.add('is-invalid');
                        passwordInput.classList.add('is-invalid');
                    } else {
                        window.location.reload();
                    }
                });
            })
            ['catch'](function(error) {
                if (loginAlertText) loginAlertText.textContent = 'Помилка з\'єднання. Перевірте інтернет.';
                if (loginAlert) loginAlert.classList.remove('d-none');
            })
            ['finally'](function() {
                submitBtn.disabled = false;
                if (submitText) submitText.classList.remove('opacity-0');
                if (submitSpinner) submitSpinner.classList.add('d-none');
            });
        };

        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var isEmailValid = checkValidity(emailInput, validateEmail);
            var isPasswordValid = checkValidity(passwordInput, function(val) { return val.length >= 8; });
            if (!isEmailValid || !isPasswordValid) return;
            submitLoginForm();
        });

        if (retryBtn) {
            retryBtn.addEventListener('click', function() {
                loginAlert.classList.add('d-none');
                submitLoginForm();
            });
        }
    }

    // --- REGISTER FORM ---
    var regForm = document.getElementById('registerForm');
    if (regForm) {
        var passwordReg = document.getElementById('password');
        var confirmPasswordReg = document.getElementById('confirmPassword');
        var confirmErrorReg = document.getElementById('confirmPasswordError');

        var checkPasswordsReg = function() {
            if (confirmPasswordReg.value && passwordReg.value !== confirmPasswordReg.value) {
                confirmPasswordReg.classList.add('is-invalid');
                confirmErrorReg.textContent = "Паролі не співпадають";
                return false;
            } else if (confirmPasswordReg.value) {
                confirmPasswordReg.classList.replace('is-invalid', 'is-valid');
                confirmErrorReg.textContent = "";
                return true;
            }
            return true;
        };

        if (passwordReg) passwordReg.addEventListener('input', checkPasswordsReg);
        if (confirmPasswordReg) confirmPasswordReg.addEventListener('input', checkPasswordsReg);

        regForm.addEventListener('submit', function (e) {
            if (!regForm.checkValidity() || !checkPasswordsReg()) {
                e.preventDefault();
                e.stopPropagation();
                var elements = regForm.elements;
                for (var i = 0; i < elements.length; i++) {
                    if (!elements[i].checkValidity()) elements[i].classList.add('is-invalid');
                }
            } else {
                var btnReg = document.getElementById('submitBtn');
                if (btnReg) btnReg.disabled = true;
                var txtReg = document.getElementById('submitText');
                if (txtReg) txtReg.classList.add('opacity-0');
                var spnReg = document.getElementById('submitSpinner');
                if (spnReg) spnReg.classList.remove('d-none');
            }
            regForm.classList.add('was-validated');
        });
    }

    // --- FORGOT PASSWORD ---
    var forgotForm = document.getElementById('forgotForm');
    if (forgotForm) {
        var emailInputForgot = document.getElementById('emailInput');
        var submitBtnForgot = document.getElementById('submitBtn');
        var resendBtnForgot = document.getElementById('resendBtn');
        
        var submitForgot = function() {
            submitBtnForgot.disabled = true;
            var alertMsg = document.getElementById('alertMsg');
            if (alertMsg) alertMsg.classList.add('d-none');

            var formDataForgot = new FormData(forgotForm);

            fetch(forgotForm.action, {
                method: 'POST',
                body: formDataForgot,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
            .then(function(response) {
                if (response.ok || response.redirected) {
                    var step1 = document.getElementById('step1');
                    var step2 = document.getElementById('step2');
                    var sentEmail = document.getElementById('sentEmail');
                    if (step1) step1.classList.add('d-none');
                    if (step2) step2.classList.remove('d-none');
                    if (sentEmail) sentEmail.textContent = emailInputForgot.value;
                } else {
                    if (alertMsg) {
                        alertMsg.textContent = 'Не вдалося надіслати запит.';
                        alertMsg.classList.remove('d-none');
                    }
                }
            })
            ['catch'](function(err) {
                if (alertMsg) {
                    alertMsg.textContent = 'Помилка мережі.';
                    alertMsg.classList.remove('d-none');
                }
            })
            ['finally'](function() {
                submitBtnForgot.disabled = false;
            });
        };

        forgotForm.addEventListener('submit', function(e) {
            e.preventDefault();
            if (!emailInputForgot.value.trim() || !emailInputForgot.checkValidity()) {
                emailInputForgot.classList.add('is-invalid');
                return;
            }
            submitForgot();
        });

        if (resendBtnForgot) resendBtnForgot.addEventListener('click', submitForgot);
    }
})();
