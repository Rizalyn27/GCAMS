// wwwroot/js/Announcement/announcement-form.js
//
// Wires up the "Post now / Schedule it" toggle and the Delete button for the
// New Announcement form. Works whether the form lives on its own page
// (Create.cshtml) or was just injected into the Bootstrap modal on the
// Announcements page (Index.cshtml) — pass the container to scope the
// lookups, or call with no argument to search the whole document.
function initAnnouncementForm(root) {
    root = root || document;

    var timingNow = root.querySelector('#timingNow');
    var timingScheduled = root.querySelector('#timingScheduled');
    var schedulePicker = root.querySelector('#schedulePicker');
    var dateInput = schedulePicker ? schedulePicker.querySelector('input[type="date"]') : null;

    function todayIso() {
        var d = new Date();
        var mm = String(d.getMonth() + 1).padStart(2, '0');
        var dd = String(d.getDate()).padStart(2, '0');
        return d.getFullYear() + '-' + mm + '-' + dd;
    }

    function applyTimingState() {
        if (!schedulePicker || !timingScheduled) return;

        var isScheduled = timingScheduled.checked;
        schedulePicker.classList.toggle('show', isScheduled);

        // "Post now" always saves today's date. "Schedule it" keeps whatever
        // date the user picks in the field — it is never overwritten here.
        if (!isScheduled && dateInput) {
            dateInput.value = todayIso();
        }
    }

    if (timingNow && timingScheduled) {
        timingNow.addEventListener('change', applyTimingState);
        timingScheduled.addEventListener('change', applyTimingState);
        applyTimingState();
    }

    var deleteBtn = root.querySelector('.btn-delete');
    if (deleteBtn) {
        deleteBtn.addEventListener('click', function () {
            var deleteForm = root.querySelector('#deleteForm') || document.getElementById('deleteForm');
            if (deleteForm) {
                deleteForm.submit();
            }
        });
    }
}