(() => {
    const tableBody = document.querySelector('#travelLogTable tbody');
    if (!tableBody) {
        return;
    }

    const refreshButton = document.getElementById('refreshTravelLogs');
    const filterForm = document.getElementById('travelLogFilters');
    const buildQuery = (params) => {
        const query = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
            if (value) {
                query.append(key, value);
            }
        });
        return query.toString();
    };

    const renderRows = (logs) => {
        tableBody.innerHTML = '';
        if (!logs?.length) {
            const row = document.createElement('tr');
            const cell = document.createElement('td');
            cell.colSpan = 8;
            cell.className = 'text-center text-muted py-4';
            cell.textContent = 'No travel activity found for the selected filters.';
            row.appendChild(cell);
            tableBody.appendChild(row);
            return;
        }

        logs.forEach((log) => {
            const row = document.createElement('tr');
            row.classList.add(log.isSuccessful ? 'success-row' : 'failure-row');
            row.innerHTML = `
                <td>${log.travelActionLogId}</td>
                <td>${log.customerUsername}</td>
                <td>${log.actionType}</td>
                <td>${log.actorUserName}</td>
                <td>${log.actorRole}</td>
                <td>${log.isSuccessful ? '<span class="badge bg-success">Success</span>' : '<span class="badge bg-danger">Failed</span>'}</td>
                <td>${log.providerReference ?? ''}</td>
                <td>${new Date(log.createdAtUtc).toLocaleString()}</td>`;
            row.title = log.failureReason ?? '';
            tableBody.appendChild(row);
        });
    };

    const loadLogs = async (filters = {}) => {
        const query = buildQuery({ limit: filters.limit ?? '100', ...filters });
        const response = await fetch(`/api/AhlaBhalTalle/logs?${query}`);
        if (!response.ok) {
            throw new Error('Unable to load travel activity.');
        }
        const data = await response.json();
        renderRows(data);
    };

    const getFiltersFromForm = () => {
        const username = filterForm?.querySelector('#filterUsername')?.value.trim();
        const from = filterForm?.querySelector('#filterFrom')?.value;
        const to = filterForm?.querySelector('#filterTo')?.value;
        const filters = {};
        if (username) {
            filters.customerUsername = username;
        }
        if (from) {
            filters.fromUtc = new Date(from).toISOString();
        }
        if (to) {
            filters.toUtc = new Date(to).toISOString();
        }
        return filters;
    };

    if (filterForm) {
        filterForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            try {
                await loadLogs(getFiltersFromForm());
            } catch (error) {
                console.error(error);
            }
        });
    }

    if (refreshButton) {
        refreshButton.addEventListener('click', async () => {
            try {
                await loadLogs(getFiltersFromForm());
            } catch (error) {
                console.error(error);
            }
        });
    }

    loadLogs().catch((error) => console.error(error));
})();
