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

    // Modal Interaction
    $(document).on('click', '.view-details-btn', function () {
        var btn = $(this);
        var id = btn.data('id');
        var action = btn.data('action');
        var entity = btn.data('entity');
        var targetId = btn.data('targetid');
        var outcome = btn.data('outcome');

        // Populate Basic Info
        $('#modal-log-id').text(id);
        $('#modal-action').text(action);
        $('#modal-entity').text(entity);
        $('#modal-target-id').text(targetId || '-');
        $('#modal-outcome').text(outcome || '-');

        // Helper to safe parse and format
        function setJson(elementId, containerId, rawData) {
            var el = $(elementId);
            var container = $(containerId);

            if (!rawData || rawData === 'null' || rawData === 'undefined') {
                container.addClass('hidden');
                el.text('');
                return;
            }

            try {
                var obj = (typeof rawData === 'string') ? JSON.parse(rawData) : rawData;
                // Check for empty object or empty array
                if (!obj || (Array.isArray(obj) && obj.length === 0) || (typeof obj === 'object' && Object.keys(obj).length === 0)) {
                    container.addClass('hidden');
                    return;
                }
                var formatted = JSON.stringify(obj, null, 2);
                el.text(formatted);
                container.removeClass('hidden');
            } catch (e) {
                // If simple string or parse error, show as is
                if (rawData && rawData.toString().trim() !== "") {
                    el.text(rawData);
                    container.removeClass('hidden');
                } else {
                    container.addClass('hidden');
                }
            }
        }

        setJson('#modal-details-json', '#modal-details-section', btn.data('details'));
        setJson('#modal-old-values-json', '#modal-old-values-section', btn.data('old'));
        setJson('#modal-new-values-json', '#modal-new-values-section', btn.data('new'));

        $('#audit-details-modal').removeClass('hidden');
    });

});

window.closeAuditModal = function () {
    $('#audit-details-modal').addClass('hidden');
};
