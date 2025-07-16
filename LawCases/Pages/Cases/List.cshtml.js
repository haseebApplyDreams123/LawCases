// Select All Functionality
document.getElementById('selectAll').addEventListener('change', function () {
    const checkboxes = document.querySelectorAll('.case-checkbox');
    checkboxes.forEach(checkbox => {
        checkbox.checked = this.checked;
    });
});

// Delete Case Function
function deleteCase(caseId, caseTitle) {
    document.getElementById('clientNameToDelete').textContent = caseTitle;
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();

    document.getElementById('confirmDeleteBtn').onclick = function () {
        // Get the antiforgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch(`/Cases/List?handler=Delete&id=${caseId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Show success message
                    alert(data.message);
                    // Reload the page to refresh the list
                    location.reload();
                } else {
                    alert('Error: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Error deleting case');
            });

        deleteModal.hide();
    };
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

// Export to Excel
function exportToExcel() {
    window.location.href = '/Cases/List?handler=Export';
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