// wwwroot/js/CaseNotes/casenote-form.js
//
// Keeps the follow-up time inside counseling hours. Works whether the form
// is on its own page (Create.cshtml) or was injected into the Bootstrap
// modal on the student profile — pass the container to scope the lookup.
function initCaseNoteForm(root) {
    root = root || document;

    // NOTE: this used to look for #fieldAppointmentDate, which doesn't exist
    // on this form, so the hours check never ran.
    var dateInput = root.querySelector('#fieldFollowUpDate');
    if (!dateInput) return;

    var allowedHours = [8, 9, 10, 11, 13, 14, 15, 16];

    function isWithinWorkingHours(value) {
        if (!value) return true;          // let required-field validation handle empties
        var parts = value.split('T');
        if (parts.length < 2) return true;
        var hour = parseInt(parts[1].split(':')[0], 10);
        return allowedHours.indexOf(hour) !== -1;
    }

    dateInput.addEventListener('change', function () {
        if (!isWithinWorkingHours(this.value)) {
            this.setCustomValidity('Follow-up sessions run 8–11 AM or 1–4 PM.');
        } else {
            this.setCustomValidity('');
        }
        this.reportValidity();
    });
}
