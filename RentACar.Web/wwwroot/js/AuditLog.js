$(document).ready(function () {
    // Initial Load
    loadTable();

    // Intercept Filter Change (Dropdowns & Date Inputs)
    $('#filterForm select, #filterForm input[type="date"]').on('change', function (e) {
        $('#filterForm').data('manual-submit', true); // Marker
        $('#filterForm').trigger('submit');
    });

    // Intercept Filter Form Submit
    $('#filterForm').on('submit', function (e) {
        var submitter = e.originalEvent ? e.originalEvent.submitter : null;

        // If the submitter is the export button, allow default submission (Form Post)
        if (submitter && (
            (submitter.name === 'exportBtn') ||
            ($(submitter).attr('formaction') && $(submitter).attr('formaction').includes('ExportAuditLog')) ||
            $(submitter).text().trim().includes("Export")
        )) {
            return true;
        }

        // Prevent default form submission (server reload)
        e.preventDefault();

        // Reset page to 1 on new search
        $('input[name="page"]').val(1);

        loadTable();
    });

    // Intercept Pagination Links (delegated)
    $(document).on('click', '#pagination-container a', function (e) {
        e.preventDefault();
        var page = $(this).data('page');
        if (page) {
            $('input[name="page"]').val(page);
            loadTable();
        }
    });

    // Reset Link
    $('a[href*="AuditLog"][class*="border"]').on('click', function (e) {
        e.preventDefault();
        // Reset form inputs
        $('#filterForm')[0].reset();
        $('input[name="page"]').val(1);

        // Reload
        loadTable();
    });

    function loadTable() {
        // Collect form data
        var formData = $('#filterForm').serialize();

        // Show loading
        $('#audit-log-table-container').html('<div class="flex justify-center p-12"><div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-gold"></div></div>');

        console.log("Fetching logs from server...", formData);

        $.ajax({
            url: '/api/AuditLog',
            type: 'GET',
            data: formData,
            success: function (response) {
                renderTable(response);
            },
            error: function (xhr, status, error) {
                console.error("Error loading table:", error);
                $('#audit-log-table-container').html('<div class="text-red-500 p-4 text-center">Error loading logs. Please try again.</div>');
            }
        });
    }

    function renderTable(data) {
        var container = $('#audit-log-table-container');

        if (!data.logs || data.logs.length === 0) {
            container.html('<div class="p-8 text-center text-white/50 bg-surface-dark rounded-xl">No logs found matching your criteria.</div>');
            return;
        }

        var html = `
        <div class="flex flex-col rounded-xl bg-surface-dark overflow-hidden shadow-2xl">
            <div class="overflow-x-auto custom-scrollbar">
                <table class="w-full text-left border-collapse">
                    <thead>
                        <tr class="border-b border-gold/10 bg-surface-dark-lighter">
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">#</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">Timestamp</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">Actor</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">Action</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">Entity</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">Summary</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold">IP Address</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold hidden 2xl:table-cell">Device</th>
                            <th class="py-4 px-6 text-xs font-bold uppercase tracking-wider text-gold text-right">Status</th>
                        </tr>
                    </thead>
                    <tbody class="text-sm divide-y divide-white/5">`;

        $.each(data.logs, function (index, log) {
            var rowNumber = ((data.page - 1) * data.pageSize) + index + 1;

            // Format ID
            var displayId = log.entityId;
            var isUser = log.entity === "User" || log.entity === "ApplicationUser";
            var isCustomer = log.targetType === "Customer" || log.entity === "Customer";
            var isEmployee = log.targetType === "Employee" || log.entity === "Employee";

            if (isCustomer) displayId = `cs-${log.entityId}`;
            else if (isEmployee) displayId = `Emp-${log.entityId}`;
            else if (isUser) displayId = "-";

            // Actor Initials
            var initial = log.actorName ? log.actorName.charAt(0).toUpperCase() : '?';

            // Action Badge Logic
            var actionColor = "text-white/70";
            var actionBg = "bg-white/5 border-white/10";
            var actionUpper = log.action ? log.action.toUpperCase() : "UNKNOWN";

            if (["CREATE", "ADDED"].includes(actionUpper)) {
                actionColor = "text-blue-400";
                actionBg = "bg-blue-500/10";
            } else if (["UPDATE", "MODIFIED"].includes(actionUpper)) {
                actionColor = "text-emerald-500";
                actionBg = "bg-emerald-500/10 border-emerald-500/20";
            } else if (["DELETE", "DELETED"].includes(actionUpper)) {
                actionColor = "text-red-400";
                actionBg = "bg-red-500/10 border-red-500/20";
            }

            // Status Logic
            var statusHtml = '';
            if (log.status === "Success") {
                statusHtml = `<span class="inline-flex items-center gap-1.5 rounded text-xs font-medium text-emerald-400">
                                <span class="h-1.5 w-1.5 rounded-full bg-emerald-400"></span> Success
                              </span>`;
            } else {
                statusHtml = `<span class="inline-flex items-center gap-1.5 rounded text-xs font-medium text-red-400">
                                <span class="h-1.5 w-1.5 rounded-full bg-red-400"></span> Failed
                              </span>`;
            }

            // Summary / Details Button
            var summaryHtml = `<span title="${escapeHtml(log.summary)}">${escapeHtml(log.summary)}</span>`;
            var isModified = actionUpper.includes("UPDATE") || actionUpper.includes("MODIFIED") || actionUpper.includes("CHANGE");

            if (isModified) {
                // We need to carefully escape attributes for JSON
                // Using single quotes for attributes, and escaping single quotes inside JSON
                summaryHtml = `
                    <button type="button"
                        class="view-details-btn bg-transparent border-0 p-0 text-gold underline underline-offset-2 hover:text-gold/80 transition-colors focus:outline-none"
                        title="Click to view changes"
                        data-id="${log.id}"
                        data-action="${escapeHtml(log.action)}"
                        data-entity="${escapeHtml(log.entity)}"
                        data-targetid="${escapeHtml(log.targetId || log.entityId)}"
                        data-outcome="${escapeHtml(log.outcome)}"
                        data-details='${escapeJsonAttribute(log.detailsJson)}'
                        data-old='${escapeJsonAttribute(log.oldValuesJson)}'
                        data-new='${escapeJsonAttribute(log.newValuesJson)}'>
                        ${escapeHtml(log.summary)}
                        <span class="inline-block ml-1 text-gold/70 align-middle">
                            <span class="material-symbols-outlined text-[14px]">visibility</span>
                        </span>
                    </button>
                `;
            }

            html += `
            <tr class="group hover:bg-white/[0.02] transition-colors">
                <td class="py-4 px-6 whitespace-nowrap text-white/50 font-mono text-xs">${rowNumber}</td>
                <td class="py-4 px-6 whitespace-nowrap text-white/70 font-mono text-xs">${formatDate(log.timestamp)}</td>
                <td class="py-4 px-6 whitespace-nowrap">
                    <div class="flex items-center gap-3">
                        <div class="h-8 w-8 rounded-full bg-gold/20 flex items-center justify-center text-gold font-bold text-xs">
                            ${initial}
                        </div>
                        <div class="flex flex-col">
                            <span class="text-white font-medium">${escapeHtml(log.actorName)}</span>
                            <span class="text-white/40 text-xs">${escapeHtml(log.actorRole)}</span>
                        </div>
                    </div>
                </td>
                <td class="py-4 px-6 whitespace-nowrap">
                    <span class="inline-flex items-center rounded-full ${actionBg} px-2.5 py-0.5 text-xs font-medium ${actionColor} border-none">
                        ${actionUpper}
                    </span>
                </td>
                <td class="py-4 px-6 whitespace-nowrap">
                    <div class="flex flex-col">
                        <span class="text-white">${escapeHtml(log.entity)}</span>
                        <span class="text-gold hover:underline text-xs" title="${escapeHtml(log.entityId)}">#${escapeHtml(displayId)}</span>
                    </div>
                </td>
                <td class="py-4 px-6 max-w-xs truncate text-white/80">
                    ${summaryHtml}
                </td>
                <td class="py-4 px-6 whitespace-nowrap text-white/50 font-mono text-xs">${escapeHtml(log.ipAddress)}</td>
                <td class="py-4 px-6 whitespace-nowrap text-white/50 text-xs hidden 2xl:table-cell">${escapeHtml(log.device || "Unknown")}</td>
                <td class="py-4 px-6 whitespace-nowrap text-right">
                    ${statusHtml}
                </td>
            </tr>`;
        });

        html += `</tbody></table></div>`;

        // Pagination
        html += renderPagination(data);

        html += `</div>`;

        container.html(html);
    }

    function renderPagination(data) {
        if (data.totalPages <= 1) return '';

        var page = data.page;
        var totalPages = data.totalPages;
        var startItem = (page - 1) * data.pageSize + 1;
        var endItem = Math.min(page * data.pageSize, data.totalCount);

        var paginationHtml = `
        <div class="flex items-center justify-between border-t border-gold/10 px-6 py-4 bg-surface-dark-lighter" id="pagination-container">
             <div class="flex flex-1 items-center justify-between">
                <div>
                    <p class="text-sm text-white/60">
                        Showing <span class="font-medium text-white">${startItem}</span> to <span class="font-medium text-white">${endItem}</span> of <span class="font-medium text-white">${data.totalCount}</span> results
                    </p>
                </div>
                <div>
                    <nav class="isolate inline-flex -space-x-px rounded-md shadow-sm">`;

        // Previous Button
        if (page > 1) {
            paginationHtml += `
                <a href="#" data-page="${page - 1}" class="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-white/10 hover:bg-white/5 focus:z-20 focus:outline-offset-0 bg-surface-dark">
                    <span class="sr-only">Previous</span>
                    <span class="material-symbols-outlined text-sm">chevron_left</span>
                </a>`;
        }

        // Logic for blocks of 5
        var blockSize = 5;
        var currentBlock = Math.ceil(page / blockSize);
        var startPage = (currentBlock - 1) * blockSize + 1;
        var endPage = Math.min(startPage + blockSize - 1, totalPages);

        if (startPage > 1) {
            paginationHtml += `<span class="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-500 ring-1 ring-inset ring-white/10 bg-surface-dark">...</span>`;
        }

        for (var i = startPage; i <= endPage; i++) {
            if (i === page) {
                paginationHtml += `<a href="#" data-page="${i}" aria-current="page" class="relative z-10 inline-flex items-center bg-gold px-4 py-2 text-sm font-semibold text-background-dark focus:z-20 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-gold">${i}</a>`;
            } else {
                paginationHtml += `<a href="#" data-page="${i}" class="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-white ring-1 ring-inset ring-white/10 hover:bg-white/5 focus:z-20 focus:outline-offset-0 bg-surface-dark">${i}</a>`;
            }
        }

        if (endPage < totalPages) {
            paginationHtml += `<span class="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-gray-500 ring-1 ring-inset ring-white/10 bg-surface-dark">...</span>`;
        }

        // Next Button
        if (page < totalPages) {
            paginationHtml += `
                <a href="#" data-page="${page + 1}" class="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-white/10 hover:bg-white/5 focus:z-20 focus:outline-offset-0 bg-surface-dark">
                    <span class="sr-only">Next</span>
                    <span class="material-symbols-outlined text-sm">chevron_right</span>
                </a>`;
        }

        paginationHtml += `
                    </nav>
                </div>
            </div>
        </div>`;
        return paginationHtml;
    }

    function formatDate(dateString) {
        if (!dateString) return '';
        var date = new Date(dateString);
        // Format: MMM dd, HH:mm
        var options = { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false };
        return date.toLocaleDateString('en-US', options);
    }

    function escapeHtml(text) {
        if (!text) return "";
        return text
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function escapeJsonAttribute(jsonString) {
        // If it's already a string, we need to make it safe for a single-quoted attribute
        if (!jsonString) return "";
        // Replace single quotes with HTML entity to prevent breaking the attribute
        return jsonString.replace(/'/g, "&apos;").replace(/"/g, "&quot;");
    }

    // Modal Interaction (Delegated)
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

        // Helper to safe parse and display
        function setJson(elementId, containerId, rawAttr) {
            var el = $(elementId);
            var container = $(containerId);

            if (!rawAttr || rawAttr === 'null' || rawAttr === 'undefined') {
                container.addClass('hidden');
                el.text('');
                return;
            }

            try {
                // If it was escaped, it might be a valid JSON string
                var obj = JSON.parse(rawAttr);
                // Check for empty boject
                if (!obj || (Array.isArray(obj) && obj.length === 0) || (typeof obj === 'object' && Object.keys(obj).length === 0)) {
                    container.addClass('hidden');
                    return;
                }

                var formatted = JSON.stringify(obj, null, 2);
                el.text(formatted);
                container.removeClass('hidden');
            } catch (e) {
                // Fallback
                if (rawAttr && rawAttr.toString().trim() !== "") {
                    el.text(rawAttr);
                    container.removeClass('hidden');
                } else {
                    container.addClass('hidden');
                }
            }
        }

        setJson('#modal-details-json', '#modal-details-section', btn.attr('data-details')); // Use attr to get raw string
        setJson('#modal-old-values-json', '#modal-old-values-section', btn.attr('data-old'));
        setJson('#modal-new-values-json', '#modal-new-values-section', btn.attr('data-new'));

        $('#audit-details-modal').removeClass('hidden');
    });

    // Close Modal Logic
    window.closeAuditModal = function () {
        $('#audit-details-modal').addClass('hidden');
    };
});
