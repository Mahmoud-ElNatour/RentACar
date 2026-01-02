$(document).ready(function () {
    // Intercept Filter Change (Dropdowns & Date Inputs) - Trigger Form Submit
    $('#filterForm select, #filterForm input[type="date"]').on('change', function (e) {
        $('#filterForm').trigger('submit');
    });

    // Intercept Filter Form Submit
    $('#filterForm').on('submit', function (e) {
        // Check if the submission was triggered by the Export button
        // Modern browsers populate e.originalEvent.submitter
        var submitter = e.originalEvent.submitter;

        // If the submitter is the export button, allow default submission
        // Check both name and inner text to be safe
        if (submitter && (
            (submitter.name === 'exportBtn') ||
            ($(submitter).attr('formaction') && $(submitter).attr('formaction').includes('ExportAuditLog')) ||
            $(submitter).text().trim().includes("Export")
        )) {
            return true;
        }

        // Otherwise, it's a filter/search action -> use AJAX to update table
        e.preventDefault();
        loadTable($(this).attr('action'), $(this).serialize());
    });

    // Dropdown change trigger logic is now handled in the '.dropdown-item' click handler above

    // Intercept Pagination Links (delegated)
    $(document).on('click', '#audit-log-table-container nav a', function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        if (url && url !== '#') {
            loadTable(url);
        }
    });

    // Intercept Reset Link
    $('a[href*="AuditLog"][class*="border"]').on('click', function (e) {
        e.preventDefault();
        // Reset form
        $('#filterForm')[0].reset();
        // Also reset custom dropdowns visually (optional, or just let the page reload handle it if not ajax?)
        // Since we use AJAX, we should reset the visual state or just reload the page for clean reset.
        // Ideally, loadTable() just re-renders the table, but the filters stay.
        // A full reset link usually reloads appropriately. 
        // Let's just manually clear the inputs to match reset()
        $('.custom-dropdown input[type="hidden"]').val('');
        $('.custom-dropdown span:first').each(function () {
            // Reset text to default based on container param
            var param = $(this).closest('.custom-dropdown').data('param');
            if (param === 'actionType') $(this).text('All Actions');
            if (param === 'entity') $(this).text('All Entities');
            if (param === 'status') $(this).text('All Statuses');
        });

        // Load base url
        loadTable($(this).attr('href'));
    });

    function loadTable(url, data) {
        $.ajax({
            url: url,
            type: 'GET',
            data: data,
            headers: { "X-Requested-With": "XMLHttpRequest" },
            success: function (result) {
                $('#audit-log-table-container').html(result);
            },
            error: function (xhr, status, error) {
                console.error("Error loading table:", error);
            }
        });
    }
});
