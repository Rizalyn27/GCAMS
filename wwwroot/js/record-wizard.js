/* =============================================================================
   record-wizard.js — shared step-form behaviour for GCAMS "Add record" pages.

   Used by:
     Views/Students/Create.cshtml    (6 steps)
     Views/Counselors/Create.cshtml  (4 steps)

   It is deliberately generic: the markup drives everything.
     .wiz-panel[data-step="N"]   one step
     .wiz-step-btn[data-goto=N]  rail button (also used by "Edit" links on Review)
     .repeater[data-prefix=".."] contact-number repeater
     .rev-val[data-src="id"]     review cell fed from a field id
     .rev-val[data-contacts=".."] review cell fed from a repeater

   Everything is progressive enhancement: with JS off, every panel stays visible
   and the form still posts normally, because panels are only hidden from here.
   ============================================================================= */

var GcamsWizard = (function () {
    'use strict';

    // Hidden panels must still be validated, otherwise a required field on a step
    // the user never opened is silently skipped client-side and only fails server-side.
    if (window.jQuery && jQuery.validator) {
        jQuery.validator.setDefaults({ ignore: [] });
    }

    function init(opts) {
        var page = document.getElementById(opts.page);
        var form = document.getElementById(opts.form);
        if (!page || !form) { return; }

        var panels = Array.prototype.slice.call(page.querySelectorAll('.wiz-panel'));
        var railBtns = Array.prototype.slice.call(page.querySelectorAll('.wiz-rail .wiz-step-btn'));
        var backBtn = document.getElementById('wizBack');
        var nextBtn = document.getElementById('wizNext');
        var submitBtn = document.getElementById('wizSubmit');
        var pctText = document.getElementById('wizPercentText');
        var pctBar = document.getElementById('wizPercentBar');
        var confirmBox = opts.confirmBox ? document.getElementById(opts.confirmBox) : null;

        var total = panels.length;
        var current = 0;
        var visited = {};   // steps the user has already opened keep their tick

        page.classList.add('js-wizard');

        // -------------------------------------------------------------- steps
        function show(index) {
            if (index < 0) { index = 0; }
            if (index > total - 1) { index = total - 1; }
            current = index;
            visited[index] = true;

            panels.forEach(function (p, i) { p.hidden = (i !== index); });

            railBtns.forEach(function (b, i) {
                b.classList.toggle('is-active', i === index);
                b.classList.toggle('is-done', i !== index && visited[i] === true);
            });

            if (backBtn) { backBtn.hidden = (index === 0); }
            if (nextBtn) { nextBtn.hidden = (index === total - 1); }
            if (submitBtn) { submitBtn.hidden = (index !== total - 1); }

            var pct = Math.round(((index + 1) / total) * 100);
            if (pctText) { pctText.textContent = pct + '%'; }
            if (pctBar) {
                pctBar.style.width = pct + '%';
                var wrap = pctBar.parentElement;
                if (wrap) { wrap.setAttribute('aria-valuenow', pct); }
            }

            if (index === total - 1) {
                paintReview();
                syncSubmitState();
            }

            window.scrollTo({ top: 0, behavior: 'smooth' });
        }

        // ---------------------------------------------------------- validation
        function validator() {
            if (!window.jQuery || !jQuery.fn.validate) { return null; }
            var v = jQuery(form).data('validator');
            if (v) { v.settings.ignore = []; }
            return v || null;
        }

        // Validates only the fields inside one panel, so "Next" does not complain
        // about steps the user has not reached yet.
        function panelIsValid(index) {
            var v = validator();
            if (!v) { return true; }

            var fields = panels[index].querySelectorAll('input, select, textarea');
            var ok = true;
            Array.prototype.forEach.call(fields, function (el) {
                if (!el.name || el.type === 'hidden' || el.disabled || el.readOnly) { return; }
                // element() returns false only for a genuine failure; fields with no
                // rules attached come back true, so untouched optional inputs pass.
                if (v.element(jQuery(el)) === false) { ok = false; }
            });
            return ok;
        }

        function stepOfFirstError() {
            var bad = form.querySelector('.input-validation-error, span.field-validation-error');
            if (!bad) { return -1; }
            var panel = bad.closest('.wiz-panel');
            return panel ? panels.indexOf(panel) : -1;
        }

        // -------------------------------------------------------------- review
        function readable(el) {
            if (!el) { return ''; }
            if (el.tagName === 'SELECT') {
                if (!el.value) { return ''; }
                var opt = el.options[el.selectedIndex];
                return opt ? opt.text.trim() : el.value;
            }
            var val = (el.value || '').trim();
            if (!val || val === '\u2014') { return ''; }
            if (el.type === 'date') {
                var d = new Date(val + 'T00:00:00');
                if (!isNaN(d.getTime())) {
                    return d.toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' });
                }
            }
            return val;
        }

        function contactsSummary(containerId) {
            var box = document.getElementById(containerId);
            if (!box) { return ''; }
            var out = [];
            Array.prototype.forEach.call(box.querySelectorAll('.contact-row'), function (row) {
                var num = row.querySelector('input[name$=".Number"]');
                if (!num || !num.value.trim()) { return; }
                var type = row.querySelector('select[name$=".Label"]');
                var label = type && type.value ? ' (' + type.options[type.selectedIndex].text.trim() + ')' : '';
                out.push(num.value.trim() + label);
            });
            return out.join(', ');
        }

        function paintReview() {
            Array.prototype.forEach.call(page.querySelectorAll('.rev-val'), function (cell) {
                var text = '';
                if (cell.dataset.src) {
                    text = readable(document.getElementById(cell.dataset.src));
                } else if (cell.dataset.contacts) {
                    text = contactsSummary(cell.dataset.contacts);
                }

                if (text) {
                    cell.textContent = text;
                    cell.classList.remove('is-empty');
                } else {
                    cell.textContent = 'Not provided';
                    cell.classList.add('is-empty');
                }
            });
        }

        function syncSubmitState() {
            if (!submitBtn || !confirmBox) { return; }
            submitBtn.disabled = !confirmBox.checked;
        }

        // ----------------------------------------------------------- repeaters
        // Names must stay contiguous (Prefix[0], Prefix[1], ...) or the model
        // binder stops at the first gap and silently drops later numbers.
        function reindex(box) {
            var prefix = box.dataset.prefix;
            Array.prototype.forEach.call(box.querySelectorAll('.contact-row'), function (row, i) {
                Array.prototype.forEach.call(row.querySelectorAll('[name]'), function (el) {
                    var suffix = el.name.substring(el.name.lastIndexOf('.') + 1);
                    el.name = prefix + '[' + i + '].' + suffix;
                });
            });
        }

        function addRow(box) {
            var rows = box.querySelectorAll('.contact-row');
            if (!rows.length) { return; }

            var clone = rows[rows.length - 1].cloneNode(true);
            Array.prototype.forEach.call(clone.querySelectorAll('input, select'), function (el) {
                if (el.type === 'hidden') { return; }   // keeps the fixed Father/Mother label
                el.value = '';
                el.classList.remove('input-validation-error', 'valid');
            });
            box.appendChild(clone);
            reindex(box);
        }

        page.addEventListener('click', function (e) {
            var add = e.target.closest('.add-contact');
            if (add) {
                e.preventDefault();
                var box = document.getElementById(add.dataset.target);
                if (box) { addRow(box); }
                return;
            }

            var remove = e.target.closest('.remove-contact');
            if (remove) {
                e.preventDefault();
                var row = remove.closest('.contact-row');
                var owner = row ? row.parentElement : null;
                if (!owner) { return; }

                if (owner.querySelectorAll('.contact-row').length > 1) {
                    row.remove();
                } else {
                    // Always leave one row behind — an empty repeater has no template to clone.
                    Array.prototype.forEach.call(row.querySelectorAll('input, select'), function (el) {
                        if (el.type !== 'hidden') { el.value = ''; }
                    });
                }
                reindex(owner);
                return;
            }

            var goTo = e.target.closest('[data-goto]');
            if (goTo && page.contains(goTo)) {
                e.preventDefault();
                show(parseInt(goTo.dataset.goto, 10));
            }
        });

        // ------------------------------------------------------------ controls
        if (nextBtn) {
            nextBtn.addEventListener('click', function () {
                if (panelIsValid(current)) { show(current + 1); }
            });
        }
        if (backBtn) {
            backBtn.addEventListener('click', function () { show(current - 1); });
        }
        if (confirmBox) {
            confirmBox.addEventListener('change', syncSubmitState);
        }

        // If a hidden step fails validation on submit, land the user on it.
        form.addEventListener('submit', function (e) {
            var v = validator();
            if (v && !jQuery(form).valid()) {
                e.preventDefault();
                var step = stepOfFirstError();
                if (step >= 0) { show(step); }
            }
        });

        // --------------------------------------------------------- age from DOB
        if (opts.birthdayField && opts.ageField) {
            var dob = document.getElementById(opts.birthdayField);
            var ageOut = document.getElementById(opts.ageField);

            var paintAge = function () {
                if (!dob || !ageOut) { return; }
                if (!dob.value) { ageOut.value = '\u2014'; return; }

                var b = new Date(dob.value + 'T00:00:00');
                if (isNaN(b.getTime())) { ageOut.value = '\u2014'; return; }

                var today = new Date();
                var age = today.getFullYear() - b.getFullYear();
                var m = today.getMonth() - b.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < b.getDate())) { age--; }

                ageOut.value = (age >= 0 && age < 130) ? String(age) : '\u2014';
            };

            if (dob) { dob.addEventListener('change', paintAge); dob.addEventListener('input', paintAge); }
            paintAge();
        }

        // ----------------------------------------------------- ID suggestion
        // Only ever a *suggestion*: the field stays editable so the real
        // school-issued number wins, and uniqueness is still the database's job.
        if (opts.generateBtn && opts.idField) {
            var genBtn = document.getElementById(opts.generateBtn);
            var idInput = document.getElementById(opts.idField);
            if (genBtn && idInput) {
                genBtn.addEventListener('click', function () {
                    var year = new Date().getFullYear();
                    var rand = Math.floor(100 + Math.random() * 900);
                    idInput.value = (opts.idPrefix || 'ID') + '-' + year + '-' + rand;
                    idInput.focus();
                });
            }
        }

        // --------------------------------------------------------------- start
        var errorStep = stepOfFirstError();
        show(errorStep >= 0 ? errorStep : 0);
        syncSubmitState();
    }

    return { init: init };
})();
