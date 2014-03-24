function ShowWorkFlowModel(entitytitle, workflowtitle) {

    var x = document.getElementById("workflow_select").selectedIndex;
    var y = document.getElementById("workflow_select").options;
    $('#workflowchangemodaltitle').html('Change ' + entitytitle + ' from ' + y[x].text + ' to ' + y[x + 1].text);
 
    $('#workflowchangemodal').modal({
        backdrop: 'static'     //Do not Allow close on background click
    });

    //alert("Next workflow item is " + y[x+1].text);

}

function ShowLineItemModel(title) {

    $('#lineitemmodaltitle').html("Add "+ title);

    $('#lineitemmodal').modal({
        backdrop: 'static'     //Do not Allow close on background click
    });


}
