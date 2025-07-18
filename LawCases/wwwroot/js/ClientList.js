
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

// Change page size
function changePageSize(pageSize) {
    const url = new URL(window.location.href);
    url.searchParams.set('pageSize', pageSize);
    url.searchParams.set('pageNumber', 1); // Reset to first page
    window.location.href = url.toString();
}

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

