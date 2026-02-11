/**
 * Navbar Search and Auto-suggest logic
 */

const allPages = [
    // Main
    { title: "Dashboard", url: "/Dashboard", icon: "dashboard", roles: ["Admin", "Employee", "Customer"] },
    { title: "Home", url: "/", icon: "home", roles: ["Admin", "Employee", "Customer", "Driver"] },
    { title: "Browse Cars", url: "/Browse", icon: "directions_car", roles: ["Admin", "Employee", "Customer"] },

    // Management (Admin/Employee)
    { title: "Control Panel Overview", url: "/ControlPanel", icon: "settings", roles: ["Admin", "Employee"] },
    { title: "Blacklist Management", url: "/Blacklist", icon: "block", roles: ["Admin", "Employee"] },
    { title: "Bookings Management", url: "/Booking", icon: "book_online", roles: ["Admin", "Employee"] },
    { title: "Cars Management", url: "/Car", icon: "directions_car", roles: ["Admin", "Employee"] },
    { title: "Categories Management", url: "/Category", icon: "category", roles: ["Admin", "Employee"] },
    { title: "Customers Management", url: "/Customer", icon: "group", roles: ["Admin", "Employee"] },
    { title: "Employees Management", url: "/Employee", icon: "badge", roles: ["Admin", "Employee"] },
    { title: "Bookings Payments", url: "/Payment", icon: "payments", roles: ["Admin", "Employee"] },
    { title: "Payment Methods", url: "/PaymentMethod", icon: "account_balance_wallet", roles: ["Admin", "Employee"] },
    { title: "Promocodes", url: "/Promocode", icon: "local_offer", roles: ["Admin", "Employee"] },
    { title: "Customer Ratings", url: "/ControlPanel/Ratings", icon: "star", roles: ["Admin", "Employee"] },

    // Admin Only
    { title: "Expenses Management", url: "/Expense", icon: "receipt_long", roles: ["Admin"] },
    { title: "Finance Dashboard", url: "/Finance", icon: "monitoring", roles: ["Admin"] },
    { title: "Audit Logs", url: "/Reports/AuditLog", icon: "history", roles: ["Admin"] },

    // Services
    { title: "Email Hub", url: "/EmailServices/EmailServicesHub", icon: "hub", roles: ["Admin", "Employee"] },
    { title: "Send Email", url: "/Admin/EmailServices/SendEmail", icon: "send", roles: ["Admin", "Employee"] },
    { title: "Support Inbox", url: "/Admin/SupportInbox", icon: "support_agent", roles: ["Admin", "Employee"] },
    { title: "Email Logs", url: "/Admin/Outbox", icon: "history", roles: ["Admin", "Employee"] },

    // Customer
    { title: "My Bookings", url: "/Bookings/MyBookings", icon: "calendar_month", roles: ["Customer"] },
    { title: "Help & Support", url: "/Support", icon: "help", roles: ["Customer"] },

    // Profile
    { title: "Profile Settings", url: "/Identity/Account/Manage", icon: "person", roles: ["Admin", "Employee", "Customer", "Driver"] }
];

let currentUserRole = "";

function initNavbarSearch(userRole) {
    currentUserRole = userRole;
    const searchInput = document.getElementById('navbarSearchInput');
    const resultsContainer = document.getElementById('navbarSearchResults');

    if (!searchInput || !resultsContainer) return;

    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase().trim();
        if (query.length < 1) {
            resultsContainer.classList.add('hidden');
            return;
        }

        const filtered = allPages.filter(page =>
            page.roles.includes(currentUserRole) &&
            (page.title.toLowerCase().includes(query) || page.url.toLowerCase().includes(query))
        );

        renderResults(filtered, query);
    });

    // Close on click outside
    document.addEventListener('click', (e) => {
        if (!searchInput.contains(e.target) && !resultsContainer.contains(e.target)) {
            resultsContainer.classList.add('hidden');
        }
    });

    searchInput.addEventListener('focus', () => {
        if (searchInput.value.trim().length > 0) {
            resultsContainer.classList.remove('hidden');
        }
    });

    // Keyboard navigation
    searchInput.addEventListener('keydown', (e) => {
        const items = resultsContainer.querySelectorAll('a');
        let activeIndex = Array.from(items).findIndex(el => el.classList.contains('bg-gold/20'));

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (items.length > 0) {
                if (activeIndex >= 0) items[activeIndex].classList.remove('bg-gold/20');
                const nextIndex = (activeIndex + 1) % items.length;
                items[nextIndex].classList.add('bg-gold/20');
                items[nextIndex].scrollIntoView({ block: 'nearest' });
            }
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (items.length > 0) {
                if (activeIndex >= 0) items[activeIndex].classList.remove('bg-gold/20');
                const nextIndex = (activeIndex - 1 + items.length) % items.length;
                items[nextIndex].classList.add('bg-gold/20');
                items[nextIndex].scrollIntoView({ block: 'nearest' });
            }
        } else if (e.key === 'Enter') {
            if (activeIndex >= 0) {
                items[activeIndex].click();
            } else if (items.length > 0) {
                items[0].click();
            }
        } else if (e.key === 'Escape') {
            resultsContainer.classList.add('hidden');
            searchInput.blur();
        }
    });
}

function renderResults(results, query) {
    const container = document.getElementById('navbarSearchResults');
    if (results.length === 0) {
        container.innerHTML = '<div class="p-4 text-gray-500 text-sm italic text-center">No pages found...</div>';
    } else {
        container.innerHTML = results.map(page => `
            <a href="${page.url}" class="flex items-center gap-3 px-4 py-3 hover:bg-gold/10 transition-colors group">
                <span class="material-symbols-outlined text-gray-400 group-hover:text-gold transition-colors">${page.icon}</span>
                <div class="flex flex-col">
                    <span class="text-sm font-medium text-white group-hover:text-gold transition-colors">${highlightMatch(page.title, query)}</span>
                    <span class="text-[10px] text-gray-500 truncate">${page.url}</span>
                </div>
            </a>
        `).join('');
    }
    container.classList.remove('hidden');
}

function highlightMatch(text, query) {
    const regex = new RegExp(`(${query})`, 'gi');
    return text.replace(regex, '<span class="text-gold font-bold">$1</span>');
}
