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