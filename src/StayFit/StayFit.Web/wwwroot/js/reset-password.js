(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        initResetPassword();
    });

    function initResetPassword() {
        var form = document.getElementById('resetForm');
        var passInput = document.getElementById('newPassword');
        var confInput = document.getElementById('confirmPassword');
        var submitBtn = document.getElementById('submitBtn');

        if (!passInput || !confInput || !submitBtn) return;

        var reqLength = document.getElementById('reqLength');
        var reqUpper = document.getElementById('reqUpper');
        var reqLower = document.getElementById('reqLower');
        var reqDigit = document.getElementById('reqDigit');

        var setReq = function (el, valid) {
            if (!el) return;
            var icon = el.querySelector('i');
            if (valid) {
                if (icon) {
                    icon.classList.remove('bi-x-circle', 'text-danger');
                    icon.classList.add('bi-check-circle-fill', 'text-success');
                }
                el.classList.remove('text-muted');
                el.classList.add('text-success');
            } else {
                if (icon) {
                    icon.classList.remove('bi-check-circle-fill', 'text-success');
                    icon.classList.add('bi-x-circle', 'text-danger');
                }
                el.classList.remove('text-success');
                el.classList.add('text-muted');
            }
        };

        var validatePassword = function () {
            var val = passInput.value;
            var lenValid = val.length >= 8;
            var upperValid = /[A-Z]/.test(val);
            var lowerValid = /[a-z]/.test(val);
            var digitValid = /[0-9]/.test(val);

            setReq(reqLength, lenValid);
            setReq(reqUpper, upperValid);
            setReq(reqLower, lowerValid);
            setReq(reqDigit, digitValid);

            return lenValid && upperValid && lowerValid && digitValid;
        };

        var validateMatch = function () {
            if (!confInput.value) {
                confInput.classList.remove('is-invalid', 'is-valid');
                return false;
            }
            if (passInput.value === confInput.value) {
                confInput.classList.remove('is-invalid');
                confInput.classList.add('is-valid');
                return true;
            } else {
                confInput.classList.add('is-invalid');
                confInput.classList.remove('is-valid');
                return false;
            }
        };

        var checkAll = function () {
            var isValidPass = validatePassword();
            var isValidMatch = validateMatch();
            
            if (isValidPass && isValidMatch) {
                submitBtn.classList.remove('disabled');
            } else {
                submitBtn.classList.add('disabled');
            }
        };

        passInput.addEventListener('input', checkAll);
        confInput.addEventListener('input', checkAll);
        
        checkAll();
    }
})();
