function deleteArea(areaId) {
    $.post(`delete/${areaId}`);
}

function updateArea(areaId) {
    var newName = $(`#newName-${areaId} input[name=name]`).val();
    $.post(`Update/${areaId}`, { newName: newName })
        .fail(function (response) {
            $(`#newName-${areaId}`).remove('.text-danger')
            $(`#newName-${areaId}`).append(generateErrorMessage(response));
        });
}

function generateErrorMessage(response) {
    var error = response.responseJSON.area.join('<br />');
    return `<div class='text-danger'>${error}</div >`;
}

function enableEdit(areaId) {
    $(`#newName-${areaId}`).show();
    $(`#oldName-${areaId}`).hide();
    var oldName = $(`#oldName-${areaId}`).text();
    $(`#newName-${areaId} input[name=name]`).val(oldName);
    $(`#newName-${areaId} input[name=name]`).focus()
}

function cancelEdit(areaId) {
    $(`#newName-${areaId}`).hide();
    $(`#oldName-${areaId}`).show();
}