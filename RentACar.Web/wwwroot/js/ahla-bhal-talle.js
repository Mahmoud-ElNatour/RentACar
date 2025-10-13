(() => {
    const panel = document.querySelector('.travel-panel');
    if (!panel) {
        return;
    }

    const canTarget = panel.dataset.canTarget === 'true';
    document.querySelectorAll('.employee-only').forEach(el => {
        if (canTarget) {
            el.classList.remove('d-none');
        }
    });

    const notificationContainer = document.getElementById('travelNotifications');

    const setDefaultDates = () => {
        const today = new Date();
        const toIsoDate = (date) => date.toISOString().split('T')[0];

        const setMinValue = (input, offsetDays) => {
            const target = new Date(today);
            target.setDate(target.getDate() + offsetDays);
            input.min = toIsoDate(target);
            if (!input.value) {
                input.value = toIsoDate(target);
            }
        };

        const checkIn = document.getElementById('hotelCheckIn');
        const checkOut = document.getElementById('hotelCheckOut');
        const flightDeparture = document.getElementById('flightDeparture');
        const flightReturn = document.getElementById('flightReturn');

        if (checkIn) {
            setMinValue(checkIn, 7);
        }
        if (checkOut) {
            setMinValue(checkOut, 9);
        }
        if (flightDeparture) {
            setMinValue(flightDeparture, 14);
        }
        if (flightReturn) {
            const defaultReturn = new Date(today);
            defaultReturn.setDate(defaultReturn.getDate() + 21);
            const minReturn = new Date(today);
            minReturn.setDate(minReturn.getDate() + 14);
            flightReturn.min = toIsoDate(minReturn);
            flightReturn.value = toIsoDate(defaultReturn);
        }
    };

    const showAlert = (type, message, reference) => {
        if (!notificationContainer) {
            return;
        }

        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `
            <div class="d-flex flex-column">
                <span>${message}</span>
                ${reference ? `<small class="text-muted mt-1">Provider reference: ${reference}</small>` : ''}
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>`;

        notificationContainer.innerHTML = '';
        notificationContainer.appendChild(alert);
    };

    const parseError = async (response) => {
        try {
            const data = await response.json();
            if (data?.message) {
                return data.message;
            }
            return JSON.stringify(data);
        } catch (e) {
            return response.statusText || 'Request failed';
        }
    };

    const postJson = async (url, payload) => {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            throw new Error(await parseError(response));
        }

        return response.json();
    };

    const buildHotelPayload = (form) => {
        const payload = {
            destination: form.destination.value.trim(),
            destinationCountryCode: form.destinationCountryCode.value.trim(),
            userCountryCode: form.userCountryCode.value.trim(),
            checkInDate: form.checkInDate.value,
            checkOutDate: form.checkOutDate.value,
            numberOfGuests: Number(form.numberOfGuests.value || '1')
        };

        if (canTarget && form.targetCustomerUsername?.value.trim()) {
            payload.targetCustomerUsername = form.targetCustomerUsername.value.trim();
        }

        return payload;
    };

    const buildFlightPayload = (form) => {
        const payload = {
            originAirportCode: form.originAirportCode.value.trim(),
            destinationAirportCode: form.destinationAirportCode.value.trim(),
            departureDate: form.departureDate.value,
            adults: Number(form.adults.value || '1'),
            children: Number(form.children.value || '0'),
            cabinClass: form.cabinClass.value
        };

        if (form.returnDate.value) {
            payload.returnDate = form.returnDate.value;
        }

        if (canTarget && form.targetCustomerUsername?.value.trim()) {
            payload.targetCustomerUsername = form.targetCustomerUsername.value.trim();
        }

        return payload;
    };

    const hotelForm = document.getElementById('hotelBookingForm');
    if (hotelForm) {
        hotelForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            try {
                const payload = buildHotelPayload(event.target);
                const response = await postJson('/api/AhlaBhalTalle/hotel', payload);
                showAlert('success', response.message || 'Hotel booking request sent successfully.', response.providerReference);
            } catch (error) {
                showAlert('danger', error.message || 'Unable to submit hotel booking.');
            }
        });
    }

    const flightForm = document.getElementById('flightBookingForm');
    if (flightForm) {
        flightForm.addEventListener('submit', async (event) => {
            event.preventDefault();
            try {
                const payload = buildFlightPayload(event.target);
                const response = await postJson('/api/AhlaBhalTalle/flight', payload);
                showAlert('success', response.message || 'Flight booking request sent successfully.', response.providerReference);
            } catch (error) {
                showAlert('danger', error.message || 'Unable to submit flight booking.');
            }
        });
    }

    setDefaultDates();
})();
