function ShowWorkFlowModel(entitytitle, workflowtitle) {

    $('#workflowchangemodaltitle').html(workflowtitle + ': ' + entitytitle);
 
    $('#workflowchangemodal').modal({
        backdrop: 'static'     //Do not Allow close on background click
    });

}

function ShowLineItemModel(title) {

    $('#lineitemmodaltitle').html("Add "+ title);

    $('#lineitemmodal').modal({
        backdrop: 'static'     //Do not Allow close on background click
    });

}