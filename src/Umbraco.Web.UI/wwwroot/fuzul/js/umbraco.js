/* Umbraco integration: submit forms normally so server validation is retained. */
(function () {
  document.querySelectorAll('input[type="file"]').forEach(function (input) {
    input.setAttribute('aria-label', input.name === 'Presentation' ? 'Yatırım sunumu (PDF, en fazla 10 MB)' : 'Ek dosya (PDF, en fazla 10 MB)');
    input.addEventListener('change', function () {
      var file = input.files[0];
      input.setCustomValidity(file && (!/\.pdf$/i.test(file.name) || file.size > 10 * 1024 * 1024) ? 'En fazla 10 MB boyutunda bir PDF dosyası seçin.' : '');
      input.reportValidity();
    });
  });
  document.querySelectorAll('[role="button"]').forEach(function (button) {
    button.addEventListener('keydown', function (event) {
      if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); button.click(); }
    });
  });
  window.addEventListener('load', function () {
    var status = document.querySelector('.fuzul-form-status, .validation-summary-errors');
    if (status) { status.scrollIntoView({ block: 'center' }); }
    var toggle = document.querySelector('.cs_munu_toggle');
    if (toggle) {
      toggle.setAttribute('role', 'button'); toggle.setAttribute('tabindex', '0');
      toggle.setAttribute('aria-label', 'Menüyü aç veya kapat'); toggle.setAttribute('aria-expanded', 'false');
      toggle.addEventListener('click', function () { toggle.setAttribute('aria-expanded', String(toggle.classList.contains('cs_toggle_active'))); });
      toggle.addEventListener('keydown', function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); toggle.click(); } });
      document.querySelectorAll('.cs_nav_list a').forEach(function (link) {
        link.addEventListener('click', function () { if (toggle.classList.contains('cs_toggle_active')) { toggle.click(); } });
      });
    }
  });
})();
