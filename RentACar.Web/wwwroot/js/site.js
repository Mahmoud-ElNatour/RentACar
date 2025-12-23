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

    const CONFIRM_THEME = {
        success: {
            header: 'confirm-success',
            button: 'btn-popup-success',
        },
        error: {
            header: 'confirm-error',
            button: 'btn-popup-error',
        },
        warning: {
            header: 'confirm-warning',
            button: 'btn-popup-warning',
        },
        info: {
            header: 'confirm-info',
            button: 'btn-popup-info',
        },
    };

    const DEFAULT_DURATION = 4000;

    function ensureContainer() {
        let container = document.querySelector('[data-popup-container]');
        if (!container) {
            container = document.createElement('div');
            container.className = 'notification-container';
            container.setAttribute('data-popup-container', '');
            container.setAttribute('aria-live', 'polite');
            container.setAttribute('aria-atomic', 'true');
            const sr = document.createElement('span');
            sr.className = 'visually-hidden';
            sr.setAttribute('data-popup-sr', '');
            sr.setAttribute('aria-live', 'polite');
            sr.setAttribute('aria-atomic', 'true');
            container.appendChild(sr);
            document.body.appendChild(container);
        } else if (!container.querySelector('[data-popup-sr]')) {
            const sr = document.createElement('span');
            sr.className = 'visually-hidden';
            sr.setAttribute('data-popup-sr', '');
            sr.setAttribute('aria-live', 'polite');
            sr.setAttribute('aria-atomic', 'true');
            container.appendChild(sr);
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

    function announceToSr(container, message) {
        const sr = container.querySelector('[data-popup-sr]');
        if (!sr) {
            return;
        }

        sr.textContent = '';
        // Force assistive tech to recognise update
        requestAnimationFrame(() => {
            sr.textContent = typeof message === 'string' ? message : String(message || '');
        });
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
        announceToSr(container, `${TYPE_MAP[resolvedType].label}: ${typeof message === 'string' ? message : ''}`);

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

    function ensureConfirmModal() {
        const modal = document.getElementById('globalConfirmModal');
        if (!modal) {
            return null;
        }

        if (!modal._popupConfirmCache) {
            modal._popupConfirmCache = {
                title: modal.querySelector('#globalConfirmTitle'),
                message: modal.querySelector('#globalConfirmMessage'),
                ok: modal.querySelector('#globalConfirmOk'),
                cancel: modal.querySelector('#globalConfirmCancel'),
                header: modal.querySelector('.modal-header'),
            };
        }

        return modal;
    }

    function applyConfirmTheme(modal, type) {
        const { header, ok } = modal._popupConfirmCache;
        const resolved = normaliseType(type);
        const theme = CONFIRM_THEME[resolved] || CONFIRM_THEME.info;

        header.classList.remove('confirm-success', 'confirm-error', 'confirm-warning', 'confirm-info');
        header.classList.add(theme.header);

        ok.classList.remove('btn-popup-success', 'btn-popup-error', 'btn-popup-warning', 'btn-popup-info');
        ok.classList.add(theme.button);
    }

    window.showPopupConfirm = function showPopupConfirm(message, options = {}) {
        const modal = ensureConfirmModal();
        if (!modal || !window.bootstrap || !window.bootstrap.Modal) {
            if (typeof window.confirm === 'function') {
                return Promise.resolve(window.confirm(String(message || '')));
            }
            return Promise.resolve(true);
        }

        const cache = modal._popupConfirmCache;
        const instance = window.bootstrap.Modal.getOrCreateInstance
            ? window.bootstrap.Modal.getOrCreateInstance(modal)
            : new window.bootstrap.Modal(modal);
        const title = options.title || 'Please Confirm';
        const confirmLabel = options.confirmLabel || 'Confirm';
        const cancelLabel = options.cancelLabel || 'Cancel';
        const type = options.type || 'warning';

        cache.title.textContent = title;
        cache.message.textContent = message != null ? String(message) : '';
        cache.ok.textContent = confirmLabel;
        cache.cancel.textContent = cancelLabel;
        applyConfirmTheme(modal, type);

        return new Promise((resolve) => {
            let resolved = false;

            const cleanup = (result) => {
                if (resolved) {
                    return;
                }
                resolved = true;
                resolve(result);
            };

            const handleHidden = () => cleanup(false);
            const handleOk = () => {
                cleanup(true);
                instance.hide();
            };
            const handleCancel = () => cleanup(false);

            modal.addEventListener('hidden.bs.modal', handleHidden, { once: true });
            cache.ok.addEventListener('click', handleOk, { once: true });
            cache.cancel.addEventListener('click', () => {
                handleCancel();
                instance.hide();
            }, { once: true });

            instance.show();
        });
    };
})();

(function () {
    function updateButtonState(button, input) {
        if (!button || !input) {
            return;
        }

        const isVisible = input.type === 'text';
        const icon = button.querySelector('i');
        if (icon) {
            icon.classList.toggle('fa-eye', !isVisible);
            icon.classList.toggle('fa-eye-slash', isVisible);
        }

        button.setAttribute('aria-pressed', String(isVisible));
        button.setAttribute('aria-label', isVisible ? 'Hide password' : 'Show password');
    }

    document.addEventListener('click', (event) => {
        const trigger = event.target.closest('[data-toggle-password]');
        if (!trigger) {
            return;
        }

        const selector = trigger.getAttribute('data-toggle-password');
        if (!selector) {
            return;
        }

        const input = document.querySelector(selector);
        if (!input) {
            return;
        }

        const nextType = input.type === 'password' ? 'text' : 'password';
        input.setAttribute('type', nextType);
        updateButtonState(trigger, input);
    });

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-toggle-password]').forEach((button) => {
            const selector = button.getAttribute('data-toggle-password');
            if (!selector) {
                return;
            }
            const input = document.querySelector(selector);
            if (input) {
                updateButtonState(button, input);
            }
        });
    });
})();

// Global Sidebar Toggle Scripts
function toggleSidebar() {
    const sidebar = document.getElementById('mainSidebar');
    const backdrop = document.getElementById('sidebarBackdrop');
    if (sidebar) sidebar.classList.toggle('open');
    if (backdrop) backdrop.classList.toggle('show');
}

function toggleSidebarDropdown(menuId, arrowId) {
    const menu = document.getElementById(menuId);
    const arrow = document.getElementById(arrowId);
    if (menu) {
        menu.classList.toggle('show');
        if (arrow) {
            if (menu.classList.contains('show')) {
                arrow.style.transform = 'rotate(180deg)';
            } else {
                arrow.style.transform = 'rotate(0deg)';
            }
        }
    }
}
