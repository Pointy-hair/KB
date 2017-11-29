$('#myModal').on('show.bs.modal', function (event) {
	var button = $(event.relatedTarget) // Button that triggered the modal
	var recipient = button.data('id') // Extract info from data-* attributes
	var modal = $(this)
	modal.find('#modal-delete-form').attr("action", "Users/Delete/" + recipient);
})