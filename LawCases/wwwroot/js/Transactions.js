function showDeleteModal(transactionId, transactionDate) {
    document.getElementById('transactionToDelete').textContent = transactionDate;
    document.getElementById('transactionIdToDelete').value = transactionId;
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

function showEditModal(transactionId, date, time, amount, notes) {
    document.getElementById('editTransactionId').value = transactionId;
    document.getElementById('editTransactionDate').value = date;
    document.getElementById('editAmount').value = amount;
    document.getElementById('editNotes').value = notes;

    const editModal = new bootstrap.Modal(document.getElementById('editModal'));
    editModal.show();
}



// Show transaction modal
function showTransactionModal(caseId) {
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
