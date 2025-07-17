function showDeleteModal(dateId, dateName) {
    // Set values before showing the modal
    document.getElementById('dateToDelete').textContent = dateName;
    document.getElementById('dateIdToDelete').value = dateId;
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    deleteModal.show();
}

function showEditModal(dateId, date, time, comment, judgeRemarks, clientRemarks, conclusion) {
    // Set values before showing the modal
    document.getElementById('editCaseDateId').value = dateId;
    document.getElementById('editNextDate').value = date;
    document.getElementById('editNextTime').value = time;
    document.getElementById('editComment').value = comment;
    document.getElementById('editJudgeRemarks').value = judgeRemarks;
    document.getElementById('editClientRemarks').value = clientRemarks;
    document.getElementById('editConclusion').value = conclusion;

    const editModal = new bootstrap.Modal(document.getElementById('editModal'));
    editModal.show();
}