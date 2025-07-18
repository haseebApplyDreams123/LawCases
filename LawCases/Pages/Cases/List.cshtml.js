

// Show transaction modal
async function showTransactionModal(caseId) {
    try {
        // Set the case ID in the hidden field
        document.getElementById('caseIdForTransaction').value = caseId;

        // Reset the form
        document.getElementById('transactionDateInput').value = '';
        document.getElementById('amountInput').value = '';
        document.getElementById('transactionCommentInput').value = '';

        // Set default date to today
        const today = new Date().toISOString().split('T')[0];
        document.getElementById('transactionDateInput').value = today;

        // Show the modal
        const modal = new bootstrap.Modal(document.getElementById('transactionModal'));
        modal.show();

    } catch (error) {
        console.error('Error:', error);
        alert('Error loading transaction form: ' + error.message);
    }
}

// Validate transaction form
function validateTransactionForm() {
    const dateInput = document.getElementById('transactionDateInput');
    const amountInput = document.getElementById('amountInput');

    // Clear previous validation
    dateInput.classList.remove('is-invalid');
    amountInput.classList.remove('is-invalid');
    document.querySelectorAll('.invalid-feedback').forEach(el => el.remove());

    let isValid = true;

    // Validate date
    if (!dateInput.value) {
        showValidationError(dateInput, 'Date is required');
        isValid = false;
    }

    // Validate amount
    if (!amountInput.value || parseFloat(amountInput.value) <= 0) {
        showValidationError(amountInput, 'Amount must be greater than 0');
        isValid = false;
    }

    return isValid;
}

// Add form submission handler
document.querySelector('#transactionModal form').addEventListener('submit', function (e) {
    if (!validateTransactionForm()) {
        e.preventDefault();
    }
});


// Show the next date modal with validation

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

function showDeleteModal(caseId, caseName) {

    // Set values before showing the modal
    document.getElementById('caseNameToDelete').textContent = caseName;
    document.getElementById('caseIdToDelete').value = caseId;


    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

// Bulk Delete Function
function bulkDeleteCases() {
    const selectedCheckboxes = document.querySelectorAll('.case-checkbox:checked');

    if (selectedCheckboxes.length === 0) {
        alert('Please select at least one case to delete');
        return;
    }

    const caseIds = Array.from(selectedCheckboxes).map(cb => parseInt(cb.value));

    if (confirm(`Are you sure you want to delete ${caseIds.length} case(s)?`)) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch('/Cases/List?handler=BulkDelete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(caseIds)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert(data.message);
                    location.reload();
                } else {
                    alert('Error: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error deleting cases');
            });
    }
}

function showReopenCaseModal(caseId, caseTitle) {
    document.getElementById('caseNameToReopen').textContent = caseTitle;
    document.getElementById('caseIdToReopen').value = caseId;
    const reopenModal = new bootstrap.Modal(document.getElementById('reopenCaseModal'));
    reopenModal.show();
}
// Ensure this is in a script tag at the bottom of your page or in a DOM-ready handler
function showCloseCaseModal(caseId, caseTitle) {
    try {
        // Wait briefly to ensure DOM is ready if needed
        setTimeout(() => {
            const modalElement = document.getElementById('closeCaseModal');
            if (!modalElement) {
                throw new Error('Modal element not found');
            }

            const nameElement = document.getElementById('caseNameToClose');
            const idElement = document.getElementById('caseIdToClose');

            if (nameElement) nameElement.textContent = caseTitle;
            if (idElement) idElement.value = caseId;

            // Initialize modal if not already done
            if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
                const modal = new bootstrap.Modal(modalElement);
                modal.show();
            } else {
                $(modalElement).modal('show'); // jQuery fallback
            }
        }, 50);
    } catch (error) {
        console.error('Error showing close modal:', error);
        alert('Error showing close case dialog. Please try again.');
    }
}
// Export to Excel
function exportToExcel() {
    window.location.href = '/Cases/List?handler=Export';
}

// Print Table
function printTable() {
    window.print();
}

// Change page size
function changePageSize(pageSize) {
    const url = new URL(window.location.href);
    url.searchParams.set('pageSize', pageSize);
    url.searchParams.set('pageNumber', 1); // Reset to first page
    window.location.href = url.toString();
}

document.addEventListener('DOMContentLoaded', function () {
    const rows = document.querySelectorAll('.clickable-row');
    rows.forEach(row => {
        row.addEventListener('click', function (e) {
            // Don't navigate if the click was on the dropdown or a button
            if (e.target.closest('.dropdown, .dropdown-toggle, .dropdown-item, button')) {
                return;
            }
            window.location.href = this.dataset.href;
        });
    });
});

// Sorting functionality
document.addEventListener('DOMContentLoaded', function () {
    // Highlight current sort column
    const sortLinks = document.querySelectorAll('.sort-link');
    sortLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            window.location.href = this.getAttribute('href');
        });
    });
});

// Enhanced table search
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.querySelector('input[name="searchTerm"]');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            // Add real-time search functionality here if needed
        });
    }
});
