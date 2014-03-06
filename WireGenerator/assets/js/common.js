var CCS;
var active;
var TinyMCEImageList = [];
var PageMenuOBJ = [];
var ActionOBJ = [];
//**********************************************************************************************
// Common Doc Ready
//**********************************************************************************************
$(document).ready(function () {
    //Begin Bootstrap Init
    $("body").tooltip({ selector: '[data-toggle="tooltip"]' });
    $('body').popover({ trigger: 'hover', selector: '[data-toggle="popover"]', html: true })

    //End Bootstrap Init

    $('#specialty').change(function () {
        setCookie('sp', $(this).val(), '', '/');
    });

    //----------------------------------------------------------------------------------
    //$('#SideBarMenu').on('click', '.sidebarmenuitem', function (e) {
    //    $(this).find('i').removeClass().addClass('fa fa-spinner fa-spin');
    //    $(this).blur();
    //});

    $('body').on('click', 'th.sort', function (e) {
        var tmpdirection = '';
        var tmporderby = this.id;
        tmporderby = tmporderby.replace('sort_', '');

        if ($('#orderby').val() == tmporderby) {
            if ($('#orderbydir').val() == 'asc') {
                tmpdirection = 'desc';
            }
            else {
                tmpdirection = 'asc';
            }
        }
        else {
            tmpdirection = 'desc';
        }      
        SetTableSort(tmporderby, tmpdirection);
        CallLoadTable();
    });

    $('#channelselectdropdownbuttonmain').click(function (e) {
        e.stopPropagation();
        $('#channelselectdropdowncaret').trigger('click');
    });

    //----------------------------------------------------------------------------------
    LoadNavBar();
    LoadWatchDog(7200000);  //7200000 = 2 Hour

    $("#appversion").html('Version:&nbsp;' + $("#Version").val());
    $("#appversion").prop('title', 'Build:' + $("#Build").val());
    if ($("#Environment").val() != "Prod") {
        $("#appname").html("JUNEODEV");
    }

    PageMenuSlide();

    $(window).bind('resize', function () {
        PageMenuSlide();
    });

    if ($('#pagemenudiv').length > 0 && $(window).width() > 767) {
        (function () {
            var timer;
            $(window).bind('scroll', function () {
                clearTimeout(timer);
                timer = setTimeout(refresh, 150);
            });
            var refresh = function () {
                PageMenuSlide();
            };
        })();
    }

});
//------------------------------------------------------------------------------------
function SetTableSort(id, direction) {
    $('#orderby').val(id);
    $('#orderbydir').val(direction);
}
//------------------------------------------------------------------------------------
function SetTableSortDisplay() {   
    var sorticon;
    var orderby = $('#orderby').val();
    if ($('#orderbydir').val() == 'asc') {
        sorticon = '<span>&nbsp;</span><i class="iconsortdir fa fa-sort-up"></i>';
    }
    else {
        sorticon = '<span>&nbsp;</span><i class="iconsortdir fa fa-sort-down"></i>';
    }
    sorticon = $(sorticon);

    if (orderby.indexOf('.') != -1) {
        sorticon.appendTo('#sort_' + orderby.toString().replace(/\./g, '\\.'));
    } else {
        sorticon.appendTo('#sort_' + orderby);
    }
}
//------------------------------------------------------------------------------------
function PageMenuSlide() {
    if ($('#pagemenudiv').length > 0 && $(window).width() > 767) {
        var x = window.pageYOffset + 'px';
        $('#pagemenudiv').animate({ top: x }, 50);

        var windowh = $(window).height();
        var bodyh = $('body').height();
        var sidebarh = $('.sidebar').height();
        if ((windowh > bodyh)) {
            if (sidebarh < windowh) {
                $('.sidebar').height(windowh)
            }
        }
        else {
            if (sidebarh < bodyh) {
                $('.sidebar').height(bodyh)
            }
        }
    }
    else {
        $('.sidebar').height('auto');
    }
}
//**********************************************************************************************
// NavBar
//**********************************************************************************************
function LoadNavBar() {
    CacheControlSite();
    var navbarcontent;
    var liclass;
    if ($("#JuneoNavBar").length) {
        $.ajax({
            type: "GET",
            url: "/api/navigation/?ccs=" + CCS,
            dataType: "json",
            cache: true,
            contentType: "application/json",
            success: function (response) {
                var data = response.Data;
                navbarcontent = '';
                $.each(data, function (i, item) {
                    if (item.dropdown) { liclass = 'class="dropdown"'; } else { ddclass = ''; }
                    navbarcontent += '<li id="li_' + item.controller + '" ' + liclass + '><a class="dropdown-toggle" data-hover="dropdown" data-toggle="dropdown" href="' + item.url + '">' + item.text + ' <b class="caret"></b></a>'
                    if (liclass == 'class="dropdown"') {
                        navbarcontent += '<ul class="dropdown-menu">';
                        $.each(item.dropdown, function (x, subitem) {
                            navbarcontent += '<li><a href="' + subitem.url + '"><i class="' + subitem.icon + '"></i>&nbsp;' + subitem.text + '</a></li>'
                        });
                        navbarcontent += '</ul></li>';
                    }
                    else {
                        navbarcontent += '</li>';
                    }
                });
                navbarcontent += '</ul>';
                $(navbarcontent).appendTo('ul#leftnavbar');

                $('[data-hover="dropdown"]').dropdownHover();

                //Set the Nav Bar Active Location
                var currentURL = window.location.pathname;
                currenturl = currentURL.toLowerCase()
                if (currenturl.indexOf('/viewarticle/') == 0) {
                    $('#li_content').addClass('active');
                }
                if (currenturl.indexOf('/viewnews/') == 0) {
                    $('#li_content').addClass('active');
                }
                if (currenturl.indexOf('/viewexpertopinion/') == 0) {
                    $('#li_content').addClass('active');
                }
                if (currenturl.indexOf('/viewzone/') == 0) {
                    $('#li_admin').addClass('active');
                }
                else if (currenturl.indexOf('/admin/') == 0) {
                    $('#li_admin').addClass('active');
                }
                else if (currenturl.indexOf('/dashboard/') == 0) {
                    $('#li_dashboard').addClass('active');
                }
                else if (currenturl.indexOf('/support/') == 0) {
                    $('#li_support').addClass('active');
                }
            },
            error: function (xhr, status, error) {
                if (xhr.status != 0) {
                    alert('Ajax call to Load NavBar: ' + xhr.status + ':' + error);
                }
            }
        });
    }
}
//**********************************************************************************************
// Common Ajax Functions
//**********************************************************************************************
function LoadSpecialtyDropDown(SpecialtiesURL) {
    var cookiesp = getCookie('sp');
    var firstspecialty = null;
    $.ajax({
        type: "GET",
        url: SpecialtiesURL,
        dataType: "json",
        cache: false,
        async: false,
        contentType: "application/json",
        success: function (data) {
            $.each(data.Data, function (i, item) {
                if (firstspecialty == null) {
                    firstspecialty = item.specialtyid;
                }
                $('#specialty').append($('<option></option>').val(item.specialtyid).html(item.specialtyname));
            });

            if ($("#specialty option[value='0']").length > 0 && $('#specialty option').size() == 2) {
                $("#specialty option[value='0']").remove();
            }

            if (data.sp_selected) {
                activespecialty = data.sp_selected;
                $("#specialty").val(activespecialty);
            }
            else {
                if ($("#specialty option[value='" + cookiesp + "']").length > 0) {
                    $("#specialty").val(cookiesp);
                }
                else {
                    if ($("#specialty option[value='0']").length > 0) {
                        $("#specialty").val('0');
                    }
                    else {
                        $("#specialty").val(firstspecialty);
                    }
                }
            }
            $("#specialty").trigger('change');
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Load Specialty DropDown: ' + xhr.status + ':' + error);
            }
        }
    });
}
//------------------------------------------------------------------------------------
function ClearPageMenu() {
    PageMenuOBJ.length=0;
    $('#pagemenu').empty();
    $('#pagemenu').parent().addClass('hide');
}
//------------------------------------------------------------------------------------
function AddPageMenu(id, name, href, gridtemplate) {
    var item = {};
    item["id"] = id;
    item["name"] = name;
    if (href == null) {
        href = 'javascript:void(0);';
    }
    item["href"] = href;
    item["gridtemplate"] = gridtemplate;
    PageMenuOBJ.push(item);
}
//------------------------------------------------------------------------------------
function CreatePageMenu(selectedid) {
    var gridtemplate='0';
    var active='';
    if (PageMenuOBJ.length > 0) {
        $.each(PageMenuOBJ, function (i, item) {
            active = '';
            if (item.gridtemplate != null) {
                gridtemplate = item.gridtemplate
            }
            if (item.id == selectedid) {
                $('#pagemenuid').val(item.id);
                active = ' active';
            }
            $('#pagemenu').append('<a class="list-group-item' + active + '" href="' + item.href + '" data-gridtemplate="' + gridtemplate + '" data-pagemenuid="' + item.id + '">' + item.name + '</a>');
        });
        $('#pagemenu').parent().removeClass('hide');
    }
}
//------------------------------------------------------------------------------------
function LoadSpecialties(SpecialtiesURL,hideallchannels) {
    if (typeof hideallchannels == "undefined") { hideallchannels = false; }
    var cookiesp = getCookie('sp');
    var firstspecialty = null;
    var activespecialtyname;
    $('#channelselectdropdownbutton').parent().removeClass('hide');
    if (SpecialtiesURL == null) {
        $('#channelselectdropdownbuttontext').html('All Channels').data('dropdown', '');
        $('#specialty').val(0);
    }
    else {
        $.ajax({
            type: "GET",
            url: SpecialtiesURL,
            dataType: "json",
            cache: false,
            async: false,
            contentType: "application/json",
            success: function (data) {
                if (!hideallchannels) {
                    $("#channelselectdropdownul").append('<li><a href="javascript:SelectChannel(0);" data-channelid="0">All Channels</a></li>');
                    $("#channelselectdropdownul").append('<li class="divider"></li>');
                }
                $.each(data.Data, function (i, item) {
                    if (firstspecialty == null) {
                        firstspecialty = item.specialtyid;
                    }
                    $("#channelselectdropdownul").append('<li><a href="javascript:SelectChannel(' + item.specialtyid + ');" data-channelid="' + item.specialtyid + '">' + item.specialtyname + '</a></li>');
                });

                if (data.sp_selected) {
                    activespecialty = data.sp_selected;
                    $("#specialty").val(activespecialty);
                }
                else {
                    if ($('#channelselectdropdownul li').find('[data-channelid="' + cookiesp + '"]').length > 0) {
                        $("#specialty").val(cookiesp);
                    }
                    else {
                        if ($('#channelselectdropdownul li').find('[data-channelid="0"]').length > 0) {
                            $("#specialty").val('0');
                        }
                        else {
                            $("#specialty").val(firstspecialty);
                        }
                    }
                }
                $('#channelselectdropdownbuttontext').html($('#channelselectdropdownul li').find('[data-channelid="' + $("#specialty").val() + '"]').html());
                $("#specialty").trigger('change');

            },
            error: function (xhr, status, error) {
                if (xhr.status != 0) {
                    alert('Ajax call to Load Channel Data: ' + xhr.status + ':' + error);
                }
            }
        });
    }
}
//------------------------------------------------------------------------------------
function SelectChannel(id) {
    $('#channelselectdropdownbuttontext').html($('#channelselectdropdownul li').find('[data-channelid="' + id + '"]').html());
    $("#specialty").val(id).trigger('change');
}
//------------------------------------------------------------------------------------
function AddActionItem(url, icon, text) {
    var item = {};
    item["icon"] = icon;
    item["text"] = text;
    if (url == null) {
        url = 'javascript:void(0);';
    }
    item["url"] = url;
    ActionOBJ.push(item);
}
//------------------------------------------------------------------------------------
function ClearActionMenu() {
    ActionOBJ.length = 0;
    $("#actionmenu").empty();
    $('#actionmenu').parent().addClass('hide');
}
//------------------------------------------------------------------------------------
function CreateActionMenu() {
    $.each(ActionOBJ, function (i, item) {
        if (item.text == 'divider') {
            $('#actionmenu').append('<li class="divider"></li>');
        }
        else {
            $('#actionmenu').append('<li><a href="' + item.url + '"><i class="' + item.icon + '"></i>&nbsp;' + item.text + '</a></li>');
        }
    });
    if (ActionOBJ.length > 0) {
        $('#actionmenu').parent().removeClass('hide');
    }
}
//------------------------------------------------------------------------------------
function LoadActions(actionurl) {

    //Clear the existing nav
    ClearActionMenu();

    if (actionurl != null) {
        $.ajax({
            type: "GET",
            url: actionurl,
            dataType: "json",
            cache: false,
            async: false,
            contentType: "application/json",
            success: function (data) {
                $.each(data, function (i, item) {
                    AddActionItem(item.url, item.icon, item.text);
                });
                CreateActionMenu();
            },
            error: function (xhr, status, error) {
                if (xhr.status != 0) {
                    alert('Ajax call to Load Actions<br>' + xhr.status + ':' + error + '<br>' + actionurl);
                }
            }
        });
    }
}
//------------------------------------------------------------------------------------
function AddtoSideBarMenu(url, icon, text) {
    var tempstring = '<li><a class="sidebarmenuitem" href="' + url + '"><i class="' + icon + '"></i> ' + text + '</a></li>';
    $('#SideBarMenu').append(tempstring).removeClass('hide');
}
//------------------------------------------------------------------------------------
function ClearSideBarMenu() {
    $("#SideBarMenu").empty();
    $('#SideBarMenu').addClass('hide');
}
//------------------------------------------------------------------------------------
function LoadSideBarMenu(SideBarMenuURL) {
    var tempstring;

    //Clear the existing SideBarMenu
    ClearSideBarMenu();

    if (SideBarMenuURL != null) {
        $.ajax({
            type: "GET",
            url: SideBarMenuURL,
            dataType: "json",
            cache: false,
            async: false,
            contentType: "application/json",
            success: function (data) {
                tempstring = '';
                $.each(data, function (i, item) {
                    if (item.text != 'divider') {
                        tempstring += '<li><a class="sidebarmenuitem" href="' + item.url + '"><i class="' + item.icon + '"></i> ' + item.text + '</a></li>';
                    }
                });
                $('#SideBarMenu').html(tempstring).removeClass('hide');
            },
            error: function (xhr, status, error) {
                if (xhr.status != 0) {
                    alert('Ajax call to Load SideBarMenu<br>' + xhr.status + ':' + error + '<br>' + ButtonBarurl);
                }
            }
        });
    }
}
//------------------------------------------------------------------------------------
function LoadTable(tableurl, template, tblid) {
    var RecordsExist = false;
    //Clear the existing table    
    $('#DataTable').html('<img src="/assets/images/loading.gif">');

    $.ajax({
        type: "GET",
        url: tableurl,
        dataType: "json",
        cache: false,
        contentType: "application/json",
        success: function (data) {
            if (data.Pagination) {
                if (data.Pagination.TotalPages != 0) {
                    RecordsExist = true;
                }
            }
            else {
                if (data.length > 0) {
                    RecordsExist = true;
                }
            }
            if (RecordsExist) {
                if (tblid != undefined) {
                    //display the json via native JS
                    ShowTable(tblid, data);
                    if (data.Pagination != null) {
                        CreatePager(data.Pagination, tableurl);
                    }
                    DataFound(true);
                }
                else {
                    //load the json into the mustache template
                    $.get('/assets/mustache/' + template + '?cc=' + $("#Build").val(), function (template) {
                        var html = Mustache.to_html(template, { 'Json': data });
                        $('#DataTable').html(html);
                        if (data.Pagination != null) {
                            CreatePager(data.Pagination, tableurl);
                        }
                        DataFound(true);
                    });
                }
                PageMenuSlide();
            }
            else {
                DataFound(false);
            }
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Load Table Data: ' + xhr.status + ':' + error);
            }
        }
    });
}

//------------------------------------------------------------------------------
//This will create the pager based on CurrentPageNo and TotalPages.
//pages variable repersents the no. of links to display for pager
function CreatePager(pagination, tableurl) {
    var currentPageNo = pagination.CurrentPageNo;
    var totalPages = pagination.TotalPages;
    var pages = 10;

    if (totalPages == 1 || totalPages == 0) {
        $('#Pager').html('');
        return; //Do not show a pager if there are no records or only one page
    }

    var pageSeries = Math.floor(currentPageNo / pages) * 10;
    var isLastSeries = pageSeries + 10 > totalPages;

    if (currentPageNo == totalPages) {
        pageSeries = (Math.floor(currentPageNo / pages) * 10) - 1;
        if (pageSeries < 1) {
            pageSeries = 1;
        }
    }
    var pagerHtml = "<ul class='pagination'>";
    if (currentPageNo <= 1) {
        pagerHtml += "<li class='arrow unavailable'><a id='larrow' href=''>&laquo;</a></li>";
    }
    else {
        pagerHtml += "<li class='arrow'><a id='larrow' href=''>&laquo;</a></li>";
    }
    for (i = pageSeries; i <= pageSeries + 10 && i <= totalPages; i++) {
        if (i == 0);
        else if (currentPageNo == i) {
            pagerHtml += "<li class='active'><a href=''>" + i + "</a></li>"
        }
        else {
            pagerHtml += "<li><a href=''>" + i + "</a></li>";
        }
    }
    if (isLastSeries) {
        pagerHtml += "<li class='arrow unavailable'><a id='rarrow' href=''>&raquo;</a></li>";
    }
    else {
        pagerHtml += "<li class='unavailable'><a href=''>&hellip;</a></li>";
        pagerHtml += "<li><a href=''>" + totalPages + "</a></li>";
        pagerHtml += "<li class='arrow'><a id='rarrow' href=''>&raquo;</a></li>";
    }
    pagerHtml += "</ul>";
    $('#Pager').html(pagerHtml);

    $(".pagination li a").click(function () {
        var currentPage = parseInt($('.pagination li.active a').text(), 10);
        //make the api call for the particular page       
        if (this.id == 'larrow') {
            if (currentPage != 1) {
                CreateTable(tableurl, currentPage, $(this));
            }
        }
        else if (this.id == 'rarrow') {
            if (currentPage != totalPages) {
                CreateTable(tableurl, currentPage, $(this));
            }
        }
        else if (currentPage != $(this).text()) {
            CreateTable(tableurl, currentPage, $(this));
        }
        $('html, body').animate({ scrollTop: 0 }, '400');
        return false;
    });
}

function getUrlParamValueByName(url, name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(url);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}

function CreateTable(tableurl, currentPage, obj) {
    var actionflagid = getUrlParamValueByName(tableurl.toLowerCase(), 'actionflagid');
    var dashboardid = getUrlParamValueByName(tableurl.toLowerCase(), 'dashboardid');
    var qnIndex = tableurl.indexOf('?');
    if (qnIndex != -1)
        tableurl = tableurl.substring(0, qnIndex);

    currentPage = obj.attr('id') != undefined ? obj.attr('id') == 'larrow' ? (currentPage - 1) : (currentPage + 1) : obj.text();
    if(parseInt(currentPage, 10).toString() != 'NaN')
    switch (tableurl.toLowerCase()) {
        case "/api/user":
            {
                LoadTable('/api/user?specialityid=' + ($('#specialty').val() || 0) + '&search=' + ($('#txtUserSearch').val() || '' + '&pageNo=' + currentPage), 'JuneoUserstable.html');
                break;
            }
        case "/api/journal":
            {
                $('#pageno').val(currentPage);
                CallLoadTable();
                break;
            }
        case "/api/specialty":
            {
                LoadTable('/api/specialty?pageNo=' + currentPage, 'SpecialtyTable.html');
                break;
            }
        case "/api/journalarticle":
            {
                var orderQuery = '&orderBy=' + ($('#orderby').val() || '') + '&orderByDir=' + ($('#orderbydir').val() || '');
                var gridTemplateName = $('#phase').find('.active').attr('title')
                $('#pageno').val(currentPage);
                switch (actionflagid) {
                    case '62': // Search journalarticles
                        {
                            var tmpsearchstring = $.trim(getCookie('searcharticle'));
                            $("#txtSearch").val(tmpsearchstring);
                            tmpsearchstring = tmpsearchstring.replace('1-', '');
                            LoadTable('/api/journalarticle?specialtyid=0&actionFlagId=62&gridTemplateName=null&search=' + encodeURIComponent(tmpsearchstring) + '&pageNo=' + currentPage + orderQuery, 'searcharticles.html', 101);
                            break;
                        }
                    default:
                        //Journal Artile List
                        //Journal article with review and edboard phase
                        {
                            $('#pageno').val(currentPage);
                            CallLoadTable();
                            break;
                        }
                }
                break;
            }
        case "/api/zone":
            {
                $('#pageno').val(currentPage);
                CallLoadTable();
                break;
            }

        case "/api/news":
            {

                var orderQuery = '&orderBy=' + ($('#orderby').val() || '') + '&orderByDir=' + ($('#orderbydir').val() || '');
                var specialty = $('#specialty').val();
                $('#pageno').val(currentPage);
                if (specialty == 'All Channels') {
                    specialty = 0;
                }
                switch (actionflagid) {
                    case '48':
                    case '49': //Load for News
                        { 
                            $('#pageno').val(currentPage);
                            CallLoadTable();
                            break;
                        }
                    case '60':  //Search news
                        {
                            var tmpsearchstring = $.trim(getCookie('searchnews'));
                            $("#txtSearch").val(tmpsearchstring);
                            tmpsearchstring = tmpsearchstring.replace('2-', '');
                            LoadTable('/api/news?id=0&phasename=&search=' + encodeURIComponent(tmpsearchstring) + '&pageNo=' + currentPage + '&actionflagid=60' + orderQuery, 'SearchNews.html');
                            break;
                        }
                    default:
                        {                           
                            LoadTable("/api/news?id=" + $("#specialty").val() + "&actionflagid=" + actionflagid + "&search=''&pageNo=" + currentPage + "&phasename=newsproduction" , "NewsDashboard.html");
                        }
                }
                break;
            }
        case "/api/expertopinion":
            {
                $('#pageno').val(currentPage);
                CallLoadTable();
                break;
            }
        case "/api/edboard":
            {
                $('#pageno').val(currentPage);
                CallLoadTable();                
                break;
            }
        case "/api/siteuser":
            {               
                $('#pageno').val(currentPage);
                CallLoadTable();
                break;
            }
        case "/api/bookmark":
            {
                $('#pageno').val(currentPage);
                CallLoadTable();
                break;
            }
        case "/api/commentary":
            {
                $('#pageno').val(currentPage);
                CallLoadTable();
                break;
            }
        default:
            {
                alert('No api call found.');
                break;
            }
    }

}

//**********************************************************************************************
// Common Generic Functions
//**********************************************************************************************

function addCommas(nStr) {
    if (nStr == null) { nStr = '';}
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}

//**********************************************************************************************

function getQuerystring(key, default_) {
    if (default_ == null) default_ = '';
    key = key.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + key + "=([^&#]*)");
    var qs = regex.exec(window.location.href);
    if (qs == null)
        return default_;
    else
        return qs[1];
}
//**********************************************************************************************
function LoadDropDown(dropdownjson, dropdownid) {
    $.ajax({
        type: "GET",
        url: dropdownjson,
        dataType: "json",
        cache: false,
        async: false,
        contentType: "application/json",
        success: function (data) {
            $.each(data.Data, function (i, item) {
                $('#' + dropdownid).append($('<option></option>').val(item.id).html(item.name));
            });
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Load' + dropdownid + ' Types: ' + xhr.status + ':' + error);
            }
        }
    });
}
//**********************************************************************************************
function UpdateFullTextLabel(id, LabelText) {
    switch (id) {
        case 1:     //No Access
            $("#FullText").text(LabelText);
            $("#FullText").removeClass("success");
            $("#FullText").addClass("alert");
            $("#FullTextDiv").removeClass('hide');
            break;
        case 2:     //Free Access
            $("#FullText").text(LabelText);
            $("#FullText").removeClass("alert");
            $("#FullText").addClass("success");
            $("#FullTextDiv").removeClass('hide');
            break;
        case 3:     // 30 Day Free Access
            $("#FullText").text(LabelText);
            $("#FullText").removeClass("alert");
            $("#FullText").addClass("success");
            $("#FullTextDiv").removeClass('hide');
            break;
        case 4:     // Limited
            $("#FullText").text(LabelText);
            $("#FullText").removeClass("success");
            $("#FullText").addClass("alert");
            $("#FullTextDiv").removeClass('hide');
            break;
    }
}
//--------------------------------------------------------------------------------
// Strips HTML and PHP tags from a string 
// returns 1: 'Kevin <b>van</b> <i>Zonneveld</i>'
// example 2: strip_tags('<p>Kevin <img src="someimage.png" onmouseover="someFunction()">van <i>Zonneveld</i></p>', '<p>');
// returns 2: '<p>Kevin van Zonneveld</p>'
// example 3: strip_tags("<a href='http://kevin.vanzonneveld.net'>Kevin van Zonneveld</a>", "<a>");
// returns 3: '<a href='http://kevin.vanzonneveld.net'>Kevin van Zonneveld</a>'
// example 4: strip_tags('1 < 5 5 > 1');
// returns 4: '1 < 5 5 > 1'
function strip_tags(str, allowed_tags) {

    var key = '', allowed = false;
    var matches = []; var allowed_array = [];
    var allowed_tag = '';
    var i = 0;
    var k = '';
    var html = '';
    var replacer = function (search, replace, str) {
        return str.split(search).join(replace);
    };
    // Build allowes tags associative array
    if (allowed_tags) {
        allowed_array = allowed_tags.match(/([a-zA-Z0-9]+)/gi);
    }
    str += '';

    // Match tags
    matches = str.match(/(<\/?[\S][^>]*>)/gi);
    // Go through all HTML tags
    for (key in matches) {
        if (isNaN(key)) {
            // IE7 Hack
            continue;
        }

        // Save HTML tag
        html = matches[key].toString();
        // Is tag not in allowed list? Remove from str!
        allowed = false;

        // Go through all allowed tags
        for (k in allowed_array) {            // Init
            allowed_tag = allowed_array[k];
            i = -1;

            if (i != 0) { i = html.toLowerCase().indexOf('<' + allowed_tag + '>'); }
            if (i != 0) { i = html.toLowerCase().indexOf('<' + allowed_tag + ' '); }
            if (i != 0) { i = html.toLowerCase().indexOf('</' + allowed_tag); }

            // Determine
            if (i == 0) {
                allowed = true;
                break;
            }
        }
        if (!allowed) {
            str = replacer(html, "", str); // Custom replace. No regexing
        }
    }
    return str;
}
//------------------------------------------------------------------------------
function BuildImageList(jsonimages) {
    if (jsonimages == null) {
        return;
    }
    var imgvalue;
    $.each(jsonimages, function (i, item) {
        imgvalue = '/content/getcontentimage/' + item.contentimageid;
        TinyMCEImageList.push({ title: item.imagename, value: imgvalue });
    });
}
//--------------------------------------------------------------------------------
function InitializeTinyMCE(iheight, tinymceselector, braunwald) {
    if (iheight == null) { iheight = 300; }
    if (typeof tinymceselector == "undefined") { tinymceselector = 'textarea.mceEditor'; }
    if (typeof braunwald == "undefined") { braunwald = false; }
    var pluginarray = [];
    var toolbar1string;
    var toolbar2string;

    toolbar1string = "undo | cut copy paste | bold italic superscript subscript strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | outdent indent";
    toolbar2string = "code visualchars | styleselect | customH3 | customP | customSmall | link image | table bullist numlist";

    pluginarray.push('advlist autolink lists link charmap preview hr anchor pagebreak');
    pluginarray.push('searchreplace wordcount visualblocks visualchars code');
    pluginarray.push('insertdatetime media nonbreaking save table contextmenu directionality');
    pluginarray.push('template paste textcolor image');

    if ($(tinymceselector).length > 0) {
        CacheControlSite();
        var tinymcecss = '/assets/bootstrap/css/bootstrap.css?ccs=' + CCS;
        tinymcecss += ',/assets/bootstrap/bootstrap.Juneotheme.css?ccs=' + CCS;
        tinymcecss += ',/assets/css/tinymce-iefix.css?ccs=' + CCS;


        if (braunwald) {
            tinymce.PluginManager.add('braunwald', function (editor, url) {
                editor.addButton('braunwald', {
                    title: 'Add Braunwald Links',
                    icon: true,
                    image: '/content/tinymce/braunwald.png',
                    onclick: function () {
                        editor.windowManager.open({
                            url: '/content/tinymce/Braunwaldlinks.htm',
                            title: 'Add Braunwald Links',
                            width: 750,
                            height: 390
                        });

                    }
                });
            });
            pluginarray.push('braunwald');
            toolbar2string += ' | braunwald'

            tinymce.PluginManager.add('braunwaldremove', function (editor, url) {
                editor.addButton('braunwaldremove', {
                    title: 'Remove Braunwald Links',
                    icon: true,
                    image: '/content/tinymce/braunwaldremove.png',
                    onclick: function () {
                        editor.dom.remove(editor.dom.select('section.braunwald'));
                        }
                });
            });
            pluginarray.push('braunwaldremove');
            toolbar2string += ' | braunwaldremove'

            
        }

        tinymce.init(
            {
                selector: tinymceselector,
                menubar: false,
                resize: false,
                theme: "modern",
                plugins: pluginarray,
                toolbar1: toolbar1string,
                toolbar2: toolbar2string, 
                style_formats: [
                    {
                        title: 'Headers', items: [
                           { title: 'Header (h3)', block: 'h3' },
                           { title: 'SubHeader (h4)', block: 'h4' }
                        ]
                    },
                    {
                        title: 'Blocks', items: [
                            { title: 'Blockquote (default)', block: 'blockquote', wrapper: true, classes: 'bq-default' },
                            { title: 'Blockquote Left', block: 'blockquote', wrapper: true, classes: 'bq-right' },
                            { title: 'Blockquote Right', block: 'blockquote', wrapper: true, classes: 'bq-left' },
                            { title: 'Div', block: 'div' },
                            { title: 'Paragraph', block: 'p' },
                            { title: 'Pre', block: 'pre' }
                        ]
                    }
                ],
                image_advtab: false,
                image_list: TinyMCEImageList,
                browser_spellcheck: true,
                content_css: tinymcecss,
                height: iheight,
                body_class: "mceEditorbody",
                statusbar: false,
                setup: function (ed) {
                    ed.addButton('customH3', {
                        title: 'Change selection to Header',
                        icon: true,
                        image: '/content/tinymce/h3.png',
                        onclick: function () {
                            var x = ed.selection.getContent();
                            if (x != '') {
                                ed.selection.setContent('<h3>' + x + '</h3>');
                            }
                            else {
                                ed.execCommand('FormatBlock', false, 'h3');
                            }
                        }
                    });
                    ed.addButton('customP', {
                        title: 'Paragraph',
                        icon: true,
                        image: '/content/tinymce/p.png',
                        onclick: function () {
                            ed.execCommand('FormatBlock', false, 'p');
                        }
                    });
                    ed.addButton('customSmall', {
                        title: 'Change selection to a small fontsize',
                        image: '/content/tinymce/sm.png',
                        onclick: function () {
                            var x = ed.selection.getContent();
                            if (x != '') {
                                ed.selection.setContent('<small>' + x + '</small>');
                            }
                            else {
                                ed.execCommand('FormatBlock', false, 'small');
                            }
                        }
                    });
                    ed.on('init', function () {
                        $('#tinymceinit').val('1');
                    });
                }
            }
        );
    }

}
//--------------------------------------------------------------------------------
function isValidEmailAddress(emailAddress) {
    var pattern = new RegExp(/^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i);
    return pattern.test(emailAddress);
};
//--------------------------------------------------------------------------------
function isEmpty(FieldValue) {
    FieldValue = $.trim(FieldValue);
    if (FieldValue) {
        return false;
    }
    else {
        return true;
    }
}
//--------------------------------------------------------------------------------
function ConfirmPassword(Value1, Value2) {

    if (isEmpty(Value1)) {
        return 3;   //Password is empty
    }

    if (Value1.indexOf(' ') >= 0) {
        return 2;   //Blanks cannot be included in the Password
    }

    if (Value1 === Value2 && Value1.length === Value2.length) {
        //Values match, move on
    }
    else {
        return 4;   //Values do not match
    }

    return 1;  //All OK, Password accepted
}
//--------------------------------------------------------------------------------
function ShowChangeMyPassword() {
    $("#currentpassword").val('');
    $("#newpassword").val('');
    $("#confirmnewpassword").val('');
    $("#ChangeMyPasswordError").addClass('hide');
    $(".ChangeMyPasswordSuccess").addClass('hide');
    $(".ChangeMyPasswordDiv").removeClass('hide');
    $('#modalChangeMyPassword').modal({
        backdrop: 'static'     //Do not Allow close on background click
        });
    $("#currentpassword").focus();
}
//--------------------------------------------------------------------------------
function ChangeMyPassword() {
    //Reset the error div
    $("#ChangeMyPasswordError").addClass('hide');
    $("#ChangeMyPasswordErrorUL li").remove();

    //Validation of the form
    var errorfound = 0;

    if (isEmpty($("#currentpassword").val())) {
        errorfound = 1;
        $('<li>Your current password must be entered to continue</li>').appendTo('ul#ChangeMyPasswordErrorUL');
    }

    if (ConfirmPassword($("#newpassword").val(), $("#currentpassword").val()) == 1) {
        errorfound = 1;
        $('<li>Your Current password is the same as your New Password!</li>').appendTo('ul#ChangeMyPasswordErrorUL');
    }

    switch (ConfirmPassword($("#newpassword").val(), $("#confirmnewpassword").val())) {
        case 1:
            //Valid Password
            break;
        case 2:
            errorfound = 1;
            $('<li>New Password cannot contain blanks</li>').appendTo('ul#ChangeMyPasswordErrorUL');
            break;
        case 3:
            errorfound = 1;
            $('<li>A new password must be entered</li>').appendTo('ul#ChangeMyPasswordErrorUL');
            break;
        case 4:
            errorfound = 1;
            $('<li>New Password and Confirm Password does not match</li>').appendTo('ul#ChangeMyPasswordErrorUL');
            break;
    }

    if (errorfound == 1) {
        $("#ChangeMyPasswordError").removeClass('hide');
        return;
    }

    $(".ChangeMyPasswordDiv").addClass('hide');

    //Call the API to change the password
    //API Should verify current password for the user.  If correct, change the password to the new value
    var content = '{"currentpassword":"' + $("#currentpassword").val() + '","password":"' + $("#newpassword").val() + '"}';

    $.ajax({
        type: "POST",
        url: '/api/UserPassword/',   //Need a new API (Permissions: allowed for all user roles)
        dataType: "json",
        data: content,
        cache: false,
        contentType: "application/json",
        success: function (response) {
            //Handle ReturnMsg Here
            //ReturnMsg 1=Ok, Password Changed
            //ReturnMsg 2=ERROR, Current Password is not correct

            if (response.Data == 1) {
                $(".ChangeMyPasswordSuccess").removeClass('hide');
            }
            else {
                $('<li>Your current password is not correct</li>').appendTo('ul#ChangeMyPasswordErrorUL');
                $("#ChangeMyPasswordErrorDiv").removeClass('hide');
                $(".ChangeMyPasswordDiv").removeClass('hide');
            }
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Change my Password: ' + xhr.status + ':' + error);
            }
            $('.ChangeMyPasswordDiv').removeClass('hide');
        }
    });

}
//--------------------------------------------------------------------------------
function GetRole(specialtyid) {
    var tmpRole;
    $.ajax({
        type: "GET",
        url: "/Api/Role?specialtyId=" + specialtyid,
        dataType: "json",
        cache: false,
        async: false,
        contentType: "application/json",
        success: function (json) {
            $.each(json.Roles, function (i, item) {
                tmpRole = item.roleid;
            });
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Load Role:' + xhr.status + ':' + error);
            }
        }
    });
    return parseInt(tmpRole);
}
//--------------------------------------------------------------------------------
function LogOff() {
    //Erase the session cookies
    setCookie("nph", null, -1, "/Content/News/");
    setCookie("japh", null, -1, "/Content/Articles/");
    setCookie("eoph", null, -1, "/Content/expertopinion/");
    setCookie("sp", null, -1, "/");
    window.location = "/Home/LogOff/";
}
//--------------------------------------------------------------------------------
function eraseCookie(name) {
    setCookie(name, '', -1,'/');
}
//--------------------------------------------------------------------------------
function setCookie(c_name, value, exdays, path) {
    if (exdays != "") {
        var exdate = new Date();
        exdate.setDate(exdate.getDate() + exdays);
        var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    }
    else {
        var c_value = escape(value);
    }
    if (path != "") {
        c_value += "; path=" + path;
    }
    document.cookie = c_name + "=" + c_value;
}
//--------------------------------------------------------------------------------
function getCookie(c_name) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == c_name) {
            return unescape(y);
        }
    }
}
//--------------------------------------------------------------------------------
function IsValidURL(value) {
    var urlregex = new RegExp("^(http:\/\/|https:\/\/|ftp:\/\/){1}([0-9A-Za-z()]+\.)");
    if (urlregex.test(value)) {
        return (true);
    }
    return (false);
}
//--------------------------------------------------------------------------------
function CacheControlSite() {
    var cachevalue;
    var currentdate = new Date();

    cachevalue = getCookie('ccsite');
    if (cachevalue == undefined) {
        cachevalue = currentdate.getTime();
    }
    else {
        if ((parseInt(cachevalue) + 300000) < currentdate.getTime()) {
            cachevalue = currentdate.getTime()
        }
    }
    setCookie('ccsite', cachevalue, '', '/');
    CCS = cachevalue;
}
//**********************************************************************************************
// WatchDog
//**********************************************************************************************
var WatchDogTimer;
var duration = '0:60'
var cmin;
var csec;
//-------------------------------------------------------------------
function Minutes(data) {
    for (var i = 0; i < data.length; i++) {
        if (data.substring(i, i + 1) == ":") {
            break;
        }
    }
    return (data.substring(0, i));
}
//-------------------------------------------------------------------
function Seconds(data) {
    for (var i = 0; i < data.length; i++) {
        if (data.substring(i, i + 1) == ":") {
            break;
        }
    }
    return (data.substring(i + 1, data.length));
}
//-------------------------------------------------------------------
function Display(min, sec) {
    var disp;
    if (min <= 9) {
        disp = "0" + min + ":";
    } else {
        disp = min + ":";
    }
    if (sec <= 9) {
        disp += "0" + sec;
    } else {
        disp += sec;
    }
    return (disp);
}
//-------------------------------------------------------------------
function StartWatchDogTimer() {
    cmin = 1 * Minutes(duration);
    csec = 0 + Seconds(duration);
    WatchDogTimerRepeat();
}
//-------------------------------------------------------------------
function WatchDogTimerRepeat() {
    csec--;
    if (csec == -1) {
        csec = 59;
        cmin--;
    }
    $("#TimeRemaining").html(Display(cmin, csec));
    if ((cmin == 0) && (csec == 0)) {
        clearTimeout(WatchDogTimer);
        LogOff();
    } else {
        WatchDogTimer = setTimeout("WatchDogTimerRepeat()", 1000);
    }
}
//-------------------------------------------------------------------
function LoadWatchDog(i) {
    if ($("#modalWatchDog").length) {
        setInterval(function () { ShowWatchDog() }, i);
    }
}
//-------------------------------------------------------------------
function ShowWatchDog() {
    StartWatchDogTimer();
    $('#modalWatchDog').modal({
        backdrop: 'static'     //Do not Allow close on background click
        });
}
//-------------------------------------------------------------------
function WatchDogConfirm() {
    $('#modalWatchDog').modal('hide')
    clearTimeout(WatchDogTimer);
    //Make an API call to keep the session alive
    $.ajax({
        type: "GET",
        url: "/api/navigation/",
        dataType: "json",
        cache: false,
        contentType: "application/json"
    });
}
//-------------------------------------------------------------------
function NoConvertDate(date) {
    var offset = new Date().getTimezoneOffset();
    return new Date(date.getTime() + offset * 60000);
}
//-------------------------------------------------------------------
function CheckForNULL(x) {
    if (x == null) {
        return null;
    }
    if ($.trim(x) == '') {
        return null;
    }
    else {
        return x;
    }

}
//-------------------------------------------------------------------
function ViewContentItem(typeid, id, navagationfrom, viewid, specialty) {
    if (typeof viewid == "undefined") { viewid = 0; }
    if (typeof specialty == "undefined") { specialty = 0; }

    if (navagationfrom != null) {
        setCookie('viewitemid', typeid + '-' + id, '', '/');
        switch (parseInt(typeid)) {
            case 1:
                setCookie('articlelist', navagationfrom, '', '/');
                break;
            case 2:
                setCookie('newslist', navagationfrom, '', '/');
                break;
            case 3:
                setCookie('eolist', navagationfrom, '', '/');
                break;
        }
    }

    if (specialty == 0) {
        specialty = $('#specialtyid').val();
        if (typeof specialty == "undefined") {
            specialty = $('#specialty').val();
        }
        if (typeof specialty == "undefined") {
            specialty = 0;
        }
    }

    switch (parseInt(typeid)) {
        case 1:
            window.location.href = "/ViewArticle/" + id + "/" + specialty + "/" + viewid;
            break;
        case 2:
            window.location.href = "/ViewNews/" + id + "/" + specialty + "/" + viewid;
            break;
        case 3:
            window.location.href = "/ViewExpertOpinion/" + id + "/" + specialty + "/" + viewid;
            break;
    }
}
//-------------------------------------------------------------------
function DownloadAttachment(id) {
    window.location = "/Export/Attachment?id=" + id;
}
//-------------------------------------------------------------------
function IsDateGreaterThanToday(d) {
    var Today = new Date();
    d.setHours(0, 0, 0, 0);
    Today.setHours(0, 0, 0, 0);
    if (d > Today) {
        return true;
    }
    else {
        return false;
    }
}
//-------------------------------------------------------------------
function IsDateGreater(d1,d2) {
    d1.setHours(0, 0, 0, 0);
    d2.setHours(0, 0, 0, 0);
    if (d1 > d2) {
        return true;
    }
    else {
        return false;
    }
}
//-------------------------------------------------------------------
function GetFileIcon(filename) {
    var icon;
    filename = filename.toLowerCase().split('.');
    switch (filename[1]) {
        case 'pdf':
            icon = '<img class="valign" src="/assets/icons/Adobe-PDF-Document-icon.png">';
            break;
        case 'doc':
        case 'docx':
            icon = '<img class="valign" src="/assets/icons/Microsoft-Word-icon.png">';
            break;
        default:
            icon = '<i class="fa fa-file-o"></i>';
            break;
    }
    return icon;
}
//-------------------------------------------------------------------
$.fn.isOnScreen = function () {

    var win = $(window);

    var viewport = {
        top: win.scrollTop(),
        left: win.scrollLeft()
    };
    viewport.right = viewport.left + win.width();
    viewport.bottom = viewport.top + win.height();

    var bounds = this.offset();
    bounds.right = bounds.left + this.outerWidth();
    bounds.bottom = bounds.top + this.outerHeight();

    return (!(viewport.right < bounds.left || viewport.left > bounds.right || viewport.bottom < bounds.top || viewport.top > bounds.bottom));

};
//-------------------------------------------------------------------
function ScrolltoViewRow(viewitem) {
    if (typeof viewitem == "undefined") {
        viewitem = getCookie("viewitemid");
    }
    eraseCookie('viewitemid');
    if (viewitem != undefined) {
        if ($('#vr' + viewitem).length > 0) {
            if ($('#vr' + viewitem).isOnScreen() == false) {
                $('html, body').animate({ scrollTop: $('#vr' + viewitem).offset().top - $('.navbarwrapper').height() }, 500);
            }
        }
    }
}
//-------------------------------------------------------------------
function DisplayEditorialItemAuthors(data) {
    var currentauthortypeid;
    var temptext='';
    $.each(data, function (ai, aitem) {
        if (CheckForNULL(aitem.lastname) != null) {
            currentauthortypeid = null;
            //Need to add logic here to handle multiple commentaries (only show written by, combined for all)
            if (currentauthortypeid != aitem.authortypeid) {
                currentauthortypeid = aitem.authortypeid;
                temptext += '<dt class="list-label ' + aitem.cssclass + '">' + aitem.displaytext + '</dt>';
            }
            temptext += '<dd class="list-item ' + aitem.cssclass + '">' + GetVcard(aitem.authorid, aitem.firstname, aitem.middlename, aitem.lastname, aitem.suffix) + '</dd>';
        }
    });
    return temptext;
}
//-------------------------------------------------------------------
function GetVcard(userid,firstname,middlename,lastname,suffix) {
    var vcard = '<span class="vcard">';
    vcard += '<img class="avatar a20" src="' + $("#ProductionURL").val() + '/Explore/GetThumbNail/' + userid + '" />';
    vcard += '<a class="fn n" href="javascript:DisplayProfile(' + userid + ');">';
    if (firstname != null) {
        vcard += '<span class="given-name">' + firstname + '</span>';
    }
    if (middlename != null) {
        vcard += '<span class="additional-name">' + middlename + '</span>';
    }
    if (lastname != null) {
        vcard += '<span class="family-name">' + lastname + '</span>';
    }
    if (suffix != null) {
        vcard += '<span class="honorific-suffix">' + suffix + '</span>';
    }
    vcard += '</a></span>';
    return vcard;
}
//-------------------------------------------------------------------
function DisplayProfile(id) {
    window.open($("#ProductionURL").val() + '/Profile/' + id, '_blank');
}
//-------------------------------------------------------------------
function ClearNumbers(string) {
    var string = string.replace(/[0123456789]/g, "");
    while (string.indexOf(',,') != -1) {
        string = string.replace(',,', ',');
    }
    return string;
}
//-------------------------------------------------------------------
function CheckForNumbers(string) {
    if (string.indexOf("0") >= 0) { return true; }
    if (string.indexOf("1") >= 0) { return true; }
    if (string.indexOf("2") >= 0) { return true; }
    if (string.indexOf("3") >= 0) { return true; }
    if (string.indexOf("4") >= 0) { return true; }
    if (string.indexOf("5") >= 0) { return true; }
    if (string.indexOf("6") >= 0) { return true; }
    if (string.indexOf("7") >= 0) { return true; }
    if (string.indexOf("8") >= 0) { return true; }
    if (string.indexOf("9") >= 0) { return true; }
    return false;
}
//-------------------------------------------------------------------
function GetEditorialType(id) {
    var type;
    switch (id) {
        case 1:
            type = 'Take-Home Message';
            break;
        case 2:
            type = 'Commentary';
            break;
        default:
            type = 'Unknown';
            break;
    }
    return type;
}
//-----------------------------------------------------------------------------------------
function LoadAuthorsDropDown(dropdownjson, dropdownid) {
    $('#' + dropdownid).children().remove();
    $.ajax({
        type: "GET",
        url: dropdownjson,
        dataType: "json",
        cache: false,
        async: false,
        contentType: "application/json",
        success: function (data) {
            $.each(data.Data, function (i, item) {
                if ((CheckForNULL(item.lastname) == null) && (CheckForNULL(item.lastname) == null)) {
                    $('#' + dropdownid).append($('<option></option>').val(item.id).html('- none -'));
                }
                else {
                    $('#' + dropdownid).append($('<option></option>').val(item.id).html(item.lastname + ', ' + item.firstname));
                }
            });
        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to Load' + dropdownid + ' Types: ' + xhr.status + ':' + error);
            }
        }
    });
}
//-------------------------------------------------------------------------------------------------
function DeleteCarousel(id,returnid) {

    var ajaxdata = JSON.stringify({
        "ActionFlag": 85,
        "CarouselId": id
    });

    $.ajax({
        type: "POST",
        url: "/api/Carousel",
        dataType: "json",
        data: ajaxdata,
        cache: false,
        async: true,
        contentType: "application/json",
        success: function (json) {
            switch (parseInt(returnid)) {
                case 1:
                    var currentview = $('.side-nav .active').attr('id');
                    currentview = currentview.replace('li', '');
                    LoadCarouselContent(currentview);
                    break;
                case 2:
                    ViewContentItem($('#typeid').val(), $('#id').val(), null, 7);
                    break;
            }

        },
        error: function (xhr, status, error) {
            if (xhr.status != 0) {
                alert('Ajax call to DeleteCarousel:' + xhr.status + ':' + error);
            }
        }
    });

}
//-------------------------------------------------------------------------------------------------
function DeleteCarouselPrompt(id, returnid) {
    var TmpTitle;
    if (returnid == 2) {
        id = $('#carouselid').val();
        TmpTitle = $("#title").text();
        if ($.trim(TmpTitle) == "") {
            TmpTitle = $("#JournalScanTitle").text();
            if ($.trim(TmpTitle) == "") {
                TmpTitle = $("#title").text()
            }
        }
    }
    else {
        TmpTitle = $("#Title" + id).text();
    }
    
    $("#RemoveTitle").html(TmpTitle);
    $("#RemoveModalTitle").html("Remove Carousel");
    $("#RemovePrompt").html("Are you sure you would like to remove the Carousel for the following:");
    $("#RemoveTitleButton").attr("href", "javascript:DeleteCarousel(" + id + "," + returnid + ");");
    $('#modalremove').modal();
}
//-------------------------------------------------------------------------------------------------