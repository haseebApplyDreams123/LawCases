// Select All Functionality
document.getElementById('selectAll').addEventListener('change', function () {
    const checkboxes = document.querySelectorAll('.client-checkbox');
    checkboxes.forEach(checkbox => {
        checkbox.checked = this.checked;
    });
});

// Delete Client Function
// Show delete confirmation modal
function showDeleteModal(clientId, clientName) {
    document.getElementById('clientNameToDelete').textContent = clientName;
    document.getElementById('clientIdToDelete').value = clientId;
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}
// Bulk Delete Function
function bulkDeleteClients() {
    const selectedCheckboxes = document.querySelectorAll('.client-checkbox:checked');

    if (selectedCheckboxes.length === 0) {
        alert('Please select at least one client to delete');
        return;
    }

    const clientIds = Array.from(selectedCheckboxes).map(cb => parseInt(cb.value));

    if (confirm(`Are you sure you want to delete ${clientIds.length} client(s)?`)) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch('/Clients/List?handler=BulkDelete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(clientIds)
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
                alert('Error deleting clients');
            });
    }
}

// Export to Excel
function exportToExcel() {
    window.location.href = '/Clients/List?handler=Export';
}

// Print Table
function printTable() {
    window.print();
}

// Enhanced table search
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.querySelector('input[name="searchTerm"]');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            // Add real-time search functionality here if needed
        });
    }
});