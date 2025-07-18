async function showNextDateModal(caseId) {
    try {
        // Fetch case details including start date and latest next date
        const response = await fetch(`/Cases/List?handler=CaseDates&caseId=${caseId}`);
        if (!response.ok) {
            throw new Error('Failed to fetch case dates');
        }

        const caseDates = await response.json();
        if (caseDates.error) {
            throw new Error(caseDates.error);
        }

        // Set the case ID in the hidden field
        document.getElementById('caseIdForNextDate').value = caseId;

        // Reset the form
        document.getElementById('nextDateInput').value = '';
        document.getElementById('commentInput').value = '';

        // Store validation dates as data attributes
        const nextDateInput = document.getElementById('nextDateInput');
        nextDateInput.dataset.startDate = caseDates.startDate;
        nextDateInput.dataset.latestNextDate = caseDates.latestNextDate || '';

        // Add validation event listener
        nextDateInput.addEventListener('change', validateNextDate);

        // Show the modal
        const modal = new bootstrap.Modal(document.getElementById('nextDateModal'));
        modal.show();

    } catch (error) {
        console.error('Error:', error);
        alert('Error loading case information: ' + error.message);
    }
}

// Date validation function
function validateNextDate() {
    const input = this;
    const selectedDate = new Date(input.value);
    const startDate = new Date(input.dataset.startDate);
    const latestNextDateStr = input.dataset.latestNextDate;

    // Clear previous validation
    input.classList.remove('is-invalid');
    const feedback = input.nextElementSibling;
    if (feedback && feedback.classList.contains('invalid-feedback')) {
        feedback.remove();
    }

    // Validate against start date
    if (selectedDate <= startDate) {
        showValidationError(input, 'Next date must be after the case start date');
        return false;
    }

    // Validate against previous next date (if exists)
    if (latestNextDateStr) {
        const latestNextDate = new Date(latestNextDateStr);
        if (selectedDate <= latestNextDate) {
            showValidationError(input, 'Next date must be after the previous next date');
            return false;
        }
    }

    return true;
}

function showValidationError(input, message) {
    input.classList.add('is-invalid');

    // Add feedback element if not exists
    if (!input.nextElementSibling || !input.nextElementSibling.classList.contains('invalid-feedback')) {
        const feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        feedback.textContent = message;
        input.parentNode.insertBefore(feedback, input.nextSibling);
    } else {
        input.nextElementSibling.textContent = message;
    }
}

// Modify form submission to prevent invalid submissions
document.querySelector('#nextDateModal form').addEventListener('submit', function (e) {
    const nextDateInput = document.getElementById('nextDateInput');
    if (!validateNextDate.call(nextDateInput)) {
        e.preventDefault();
    }
});