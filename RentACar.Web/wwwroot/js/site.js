// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Global popup notification helper shared across pages.
(function () {
    const TYPE_MAP = {
        success: {
            icon: 'fa-circle-check',
            label: 'Success',
        },
        error: {
            icon: 'fa-circle-xmark',
            label: 'Error',
        },
        warning: {
            icon: 'fa-triangle-exclamation',
            label: 'Warning',
        },
        info: {
            icon: 'fa-circle-info',
            label: 'Notice',
        },
    };

    const DEFAULT_DURATION = 4000;

    function ensureContainer() {
        let container = document.querySelector('.notification-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'notification-container';
            document.body.appendChild(container);
        }
        return container;
    }

    function normaliseType(type) {
        return TYPE_MAP[type] ? type : 'info';
    }

    function formatMessage(message) {
        if (message == null) {
            return '';
        }
        return String(message).replace(/\n/g, '<br />');
    }

    window.showPopup = function showPopup(message, type = 'info', options = {}) {
        const resolvedType = normaliseType(type);
        const duration = Math.max(1500, Number(options.duration) || DEFAULT_DURATION);
        const container = ensureContainer();
        const toast = document.createElement('div');
        toast.className = `notification-toast notification-${resolvedType}`;

        const iconWrapper = document.createElement('span');
        iconWrapper.className = 'notification-icon';
        const icon = document.createElement('i');
        icon.className = `fa-solid ${TYPE_MAP[resolvedType].icon}`;
        icon.setAttribute('aria-hidden', 'true');
        iconWrapper.appendChild(icon);

        const content = document.createElement('div');
        content.className = 'notification-content';

        const title = document.createElement('div');
        title.className = 'notification-title';
        title.textContent = TYPE_MAP[resolvedType].label;

        const body = document.createElement('div');
        body.className = 'notification-message';
        body.innerHTML = formatMessage(message);

        content.appendChild(title);
        content.appendChild(body);

        const dismiss = document.createElement('button');
        dismiss.type = 'button';
        dismiss.className = 'notification-close';
        dismiss.setAttribute('aria-label', 'Close notification');
        dismiss.innerHTML = '&times;';

        dismiss.addEventListener('click', () => removeToast(toast));

        toast.appendChild(iconWrapper);
        toast.appendChild(content);
        toast.appendChild(dismiss);
        container.appendChild(toast);

        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        const timeoutId = setTimeout(() => {
            removeToast(toast);
        }, duration);

        toast.dataset.timeoutId = String(timeoutId);

        toast.addEventListener('mouseenter', () => {
            const id = toast.dataset.timeoutId;
            if (id) {
                clearTimeout(Number(id));
            }
        });

        toast.addEventListener('mouseleave', () => {
            const current = toast.dataset.timeoutId;
            if (current) {
                clearTimeout(Number(current));
            }
            const id = setTimeout(() => removeToast(toast), 1500);
            toast.dataset.timeoutId = String(id);
        });

        toast.addEventListener('transitionend', (event) => {
            if (event.propertyName === 'opacity' && toast.classList.contains('hiding')) {
                toast.remove();
            }
        });
    };

    function removeToast(toast) {
        if (!toast || toast.classList.contains('hiding')) {
            return;
        }
        const timeoutId = toast.dataset.timeoutId;
        if (timeoutId) {
            clearTimeout(Number(timeoutId));
            delete toast.dataset.timeoutId;
        }
        toast.classList.add('hiding');
        toast.classList.remove('show');
    }
})();
