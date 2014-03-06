$(document).ready(function () {
    $('#modalcopyitemclose').click(function () {
        $("#modalcopyitem").modal('hide');
    });
    //-----------------------------------------------------------------------------------------
    $('#modalcopyitemresultclose').click(function () {
        $("#modalcopyitemresult").modal('hide');
    });
    //-----------------------------------------------------------------------------------------
    $('#modalcopybutton').click(function () {
        if ($('#copytospecialty').val() == 0) {
            return;
        }
        $("#modalcopyitem").modal('hide');
        switch (parseInt($('#typeid').val())) {
            case 1:
                CopyArticle();
                break;
            case 2:
                CopyNews();
                break;
        }
    });
});
//-----------------------------------------------------------------------------------------
function CopyItem() {
    $('#copytospecialty option').remove();
    switch(parseInt($('#typeid').val())){
        case 1:
            Loadcopytospecialty("/api/value?requestid=26&value=3");
            break;
        case 2:
            Loadcopytospecialty("/api/value?requestid=26&value=8");
            break;
    }
    $("#modalcopyitem").modal();

}
//-----------------------------------------------------------------------------------------
function Loadcopytospecialty(dropdownjson, dropdownid) {
    $.ajax({
        type: "GET",
        url: dropdownjson,
        dataType: "json",
        cache: false,
        async: false,
        contentType: "application/json",
        success: function (data) {
            $('#copytospecialty').append($('<option></option>').val(0).html(''));
            $.each(data.Data, function (i, item) {
                if ($('#specialtyid').val() != item.specialtyid) {
                    $('#copytospecialty').append($('<option></option>').val(item.specialtyid).html(item.specialtyname));
                }
            });
            $("#copytospecialty").trigger('change');
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Load' + dropdownid + ' Types: ' + xhr.status + ':' + error);
            }
        }
    });
}
//-----------------------------------------------------------------------------------------------
function CopyNews() {
    var newsdata = {"id": $('#id').val(), "specialtyid": $('#copytospecialty').val(), "actionflag": 74 };
    $.ajax({
        type: "POST",
        url: '/api/news',
        dataType: "json",
        cache: false,
        async: false,
        data: JSON.stringify(newsdata),
        contentType: "application/json",
        success: function (data) {
            if (data.success == true) {
                $('#copyresultheader').html('Copy Succesfull!');
                $('#opennewitem').text('Open New Item');
            }
            else {
                $('#copyresultheader').html('<span class="red">Item Already exists!</span>');
                $('#opennewitem').text('Open Existing Item');
            }
            $('#opennewitem').attr('href', '/ViewNews/' + data.contentitemid);
            $("#modalcopyitemresult").modal();
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Copy Item ' + xhr.status + ':' + error);
            }
        }
    });
}
//-----------------------------------------------------------------------------------------------
function CopyArticle() {
    $.ajax({
        type: "POST",
        url: '/api/JournalArticle?Id=' + $('#id').val() + '&specialtyid=' +  $('#copytospecialty').val(),
        cache: false,
        async: false,
        contentType: "application/json",
        success: function (data) {
            if (data.success == true) {
                $('#copyresultheader').html('Copy Succesfull!');
                $('#opennewitem').text('Open New Item');
            }
            else {
                $('#copyresultheader').html('<span class="red">Item Already exists!</span>');
                $('#opennewitem').text('Open Existing Item');
            }
            $('#opennewitem').attr('href', '/content/viewarticle/?id=' + data.contentitemid);
            $("#modalcopyitemresult").modal();
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Copy Item ' + xhr.status + ':' + error);
            }
        }
    });
}
