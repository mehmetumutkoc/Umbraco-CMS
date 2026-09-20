(function () {
    'use strict';
    const reduced = document.body.dataset.disableMotion === 'true' || window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduced) document.documentElement.style.scrollBehavior = 'auto';
    document.querySelectorAll('input[type=file]').forEach(input => {
        input.addEventListener('change', () => {
            const file = input.files[0];
            input.setCustomValidity(file && (!/\.pdf$/i.test(file.name) || file.size > 10 * 1024 * 1024) ? 'En fazla 10 MB boyutunda bir PDF dosyası seçin.' : '');
            input.reportValidity();
        });
    });
    const feedback = document.querySelector('.fuzul-form-status, .validation-summary-errors');
    if (feedback) { feedback.setAttribute('tabindex', '-1'); feedback.focus({ preventScroll: true }); feedback.scrollIntoView({ block: 'center', behavior: reduced ? 'instant' : 'smooth' }); }
    document.querySelectorAll('.cs_nav_list a').forEach(link => {
        link.addEventListener('click', () => {
            const toggle = document.querySelector('.cs_munu_toggle');
            if (toggle && toggle.classList.contains('cs_toggle_active')) toggle.click();
        });
    });
})();
