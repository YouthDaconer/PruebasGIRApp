/**
 * Descripcion: Se define libreria para implementación de funcionalidades genericas
 * Autor: Doney Hernandez
 * Fecha: 08 de Marzo de 2019
 * Version 1.0
*/

/**
 * Descripcion: Metodo que permite consultar un servicio por medio de una url y unos parametros, los cuales deben ir por medio de un obj json
 * Autor: Doney Hernandez
 * Fecha: 20 de Marzo de 2019
 * Version 1.0
 * Params: Objeto json con las siguientes propiedades
 * LoaderText: Texto a desplegar en el loader
 * Loading: Variable que permite definir si se muestra o no la animación del loading.
 * Data: Obj json con los parametros que recibe el servicio.
 * CBFunction: Nombre de metodo que sirve como callback para un evento de respuesta exitoso.
*/
function getDataFromADS(jsonParams) {

    var strLoaderText = "<p>Consultando.<br/><small>Por favor espere</small></p>";

    if (typeof (jsonParams.LoaderText) !== "undefined") {
        strLoaderText = jsonParams.LoaderText;
    }

    $('#divLoaderText').html(strLoaderText);

    if (typeof (jsonParams.TYPE) === "undefined") {
        jsonParams.TYPE = "POST";
    }

    if (typeof (jsonParams.Loading) === "undefined") {
        jsonParams.Loading = true;
    }

    if (typeof (jsonParams.CBErrorFunction) === "undefined") {
        jsonParams.CBErrorFunction = "";
    }

    if (typeof (jsonParams.Data) === "undefined") {
        jsonParams.Data = "";
    }

    if (typeof (jsonParams.DataType) === "undefined") {
        jsonParams.DataType = "json";
    }

    if (jsonParams.Loading) {
        $('#divModalLoading').modal('show');
    }

    try {
        $.ajax({
            type: jsonParams.TYPE,
            url: jsonParams.Action,
            contentType: "application/json; charset=utf-8",
            data: jsonParams.Data,
            dataType: jsonParams.DataType,
            success: function (msg) {
                
                setTimeout(function () {
                    $('#divModalLoading').modal('hide');
                }, 500);

                if (typeof (msg.d) !== "undefined") {

                    if (msg.d) {

                        var jsonResp = eval("(" + msg.d + ")");

                        window[jsonParams.CBFunction](jsonResp);

                    } else {
                        cbErrorFromADS(jsonParams.CBErrorFunction);
                    }
                }
                else{
                    
                    window[jsonParams.CBFunction](msg);

                }
            },
            error: function (req, status, error) {
                setTimeout(function () {
                    $('#divModalLoading').modal('hide');
                }, 500);
                cbErrorFromADS(jsonParams.CBErrorFunction);
            }
        });
    }
    catch (error) {

        setTimeout(function () {
            $('#divModalLoading').modal('hide');
        }, 500);
        console.error(error);
    }
}

function cbErrorFromADS(strErrorMsg) {

    var strError = "Ocurrió un error inesperado, por favor consulte al administrador.";

    if (strErrorMsg !== null && strErrorMsg !== "")
    {
        strError = strErrorMsg;
    }

    var jsonMsg = {
        Title: "Error",
        Type: "Error",
        Msg: strError
    };
    showITCMessage(jsonMsg);
}


/**
 * Descripcion: Se metodo que permite mostrar una ventana modal con un mensaje customizado por el usuario
 * Autor: Doney Hernandez
 * Fecha: 08 de Marzo de 2019
 * Version 1.0
 * Params: Objeto json con las siguientes propiedades
 * Title: Titulo para las ventanas modales
 * Msg: mensaje a desplegar al usuario
 * Type: Tipo del mensaje: Warning, Error, Success
*/
function showITCMessage(jsonMsg) {

    var strMsg = "";

    switch (jsonMsg.Type) {
        case "Success":
            strMsg += '<div class="card border-left-success shadow py-2">';// h-50
            strMsg += '<div class="card-body">';
            strMsg += '<div class="row no-gutters align-items-center">';
            strMsg += '<div class="col mr-2">';
            strMsg += '<div class="text-xs font-weight-bold text-success text-uppercase mb-1">' + jsonMsg.Msg + '</div>';
            strMsg += '</div>';
            strMsg += '<div class="col-auto">';
            strMsg += '<i class="fas fa-check-circle fa-2x" style="color:#1cc88a;"></i>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            break;
        case "Warning":
            strMsg += '<div class="card border-left-warning shadow py-2">';
            strMsg += '<div class="card-body">';
            strMsg += '<div class="row no-gutters align-items-center">';
            strMsg += '<div class="col mr-2">';
            strMsg += '<div class="text-xs font-weight-bold text-warning text-uppercase mb-1">' + jsonMsg.Msg + '</div>';
            strMsg += '</div>';
            strMsg += '<div class="col-auto">';
            strMsg += '<i class="fas fa-info-circle fa-2x" style="color:#f6c23e;"></i>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            break;
        case "Error":
            strMsg += '<div class="card border-left-danger shadow py-2">';
            strMsg += '<div class="card-body">';
            strMsg += '<div class="row no-gutters align-items-center">';
            strMsg += '<div class="col mr-2">';
            strMsg += '<div class="text-xs font-weight-bold text-danger text-uppercase mb-1">' + jsonMsg.Msg + '</div>';
            strMsg += '</div>';
            strMsg += '<div class="col-auto">';
            strMsg += '<i class="fas fa-window-close fa-2x" style="color:#e74a3b;"></i>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            strMsg += '</div>';
            break;
        default: "";
    }

    $('#h5ModalTitle').text(jsonMsg.Title);
    $('#divModalBody').html(strMsg);
    $('#divModalMsg').modal('show');
}

function validateEmptyFields(arrayFields) {

    var blEmpty = true;

    for (var i = 0; i < arrayFields.length; i++) {

        var strField = $("#" + arrayFields[i]).val();
        if (strField.trim() == "") {
            if (blEmpty) {
                blEmpty = false;
            }
        }
    }

    return blEmpty;
}

function showITCConfirm(jsonConfirm) {

    var strData = "";

    if (typeof (jsonConfirm.Data) !== "undefined") {
        strData = jsonConfirm.Data;
    }

    $('#h5ConfirmModalTitle').text(jsonConfirm.Title);
    $('#divConfirmModalBody').html(jsonConfirm.Body);
    $('#btnConfirmModal').off("click");
    $('#btnConfirmModal').click(function () { window[jsonConfirm.CBFunction](strData);});
    $('#divConfirmModal').modal('show');
}

function showITCModalTable(jsonModalTable) {

    var strData = "";

    if (typeof (jsonModalTable.Data) !== "undefined") {
        strData = jsonModalTable.Data;
    }

    var strLabelButton = "Aceptar";

    if (typeof (jsonModalTable.LabelButton) !== "undefined") {
        strLabelButton = jsonModalTable.LabelButton;
    }

    $('#btnModalTable').html(strLabelButton);

    $('#h5ModalTableTitle').text(jsonModalTable.Title);
    $('#divModalTableBody').html(jsonModalTable.Body);

    if (typeof (jsonModalTable.CBFunction) !== "undefined") {
        $('#btnModalTable').off("click");
        $('#btnModalTable').click(function () { window[jsonModalTable.CBFunction](strData); });
    }

    if (typeof (jsonModalTable.ShowBtnExp) !== "undefined") {
        if (jsonModalTable.ShowBtnExp) {
            $('#btnModalTable').show();
        } else {
            $('#btnModalTable').hide();
        }
    } else {
        $('#btnModalTable').show();
    }

    $('#divModalTable').modal('show');
}


function buildTbl(jsonTbl) {

    var nuPageLength = 10;
    var nuRows = 0;
    var nuPage = 0;

    var strHtmlTable = "";
    var strHtmlHeader = "";
    var strHtmlBody = "";

    var blPaginate = true;
    if (typeof (jsonTbl.blPaginate) !== "undefined") {
        blPaginate = jsonTbl.blPaginate;
    }

    if (typeof (jsonTbl.ROWS) !== "undefined") {
        nuRows = jsonTbl.ROWS;
    }

    if (typeof (jsonTbl.PAGE) !== "undefined") {
        nuPage = jsonTbl.PAGE;
    }

    var blSearch = true;
    if (typeof (jsonTbl.blSearch) !== "undefined") {
        blSearch = jsonTbl.blSearch;
    }

    var blInfo = true;
    if (typeof (jsonTbl.blInfo) !== "undefined") {
        blInfo = jsonTbl.blInfo;
    }

    var blButton = false;
    if (typeof (jsonTbl.PAGELENGTH) !== "undefined") {
        nuPageLength = jsonTbl.PAGELENGTH;
    }

    if (typeof (jsonTbl.BUTTON) !== "undefined") {
        if (jsonTbl.BUTTON.length > 0) {
            blButton = true;
        }
    }

    var blButtons = false;
    if (typeof (jsonTbl.BUTTONS) !== "undefined") {
        if (jsonTbl.BUTTONS.length > 0) {
            blButtons = true;
        }
    }

    var blChk = false;
    if (typeof (jsonTbl.CHECKBOX) !== "undefined") {
        if (jsonTbl.CHECKBOX) {
            blChk = true;
        }
    }

    var blFixedHeader = false;
    if (typeof (jsonTbl.FIXEDHEADER) !== "undefined") {
        if (jsonTbl.FIXEDHEADER) {
            blFixedHeader = false;
        }
    }

    for (var i = 0; i < jsonTbl.LABEL.length; i++) {

        if (i === 0 && blChk) {
            strHtmlHeader += "<th align='center'><input type='checkbox' id='chk" + jsonTbl.ID + "' class='' onClick='checkAll(\"" + jsonTbl.ID + "\")' /></th>";
        } else {
            strHtmlHeader += "<th>" + jsonTbl.LABEL[i] + "</th>";
        }

    }

    strHtmlHeader = "<thead><tr>" + strHtmlHeader + "</tr></thead>";

    var arrayCols = new Array();

    if (typeof (jsonTbl.COLS) !== "undefined") {
        for (var j = 0; j < jsonTbl.COLS.length; j++) {
            arrayCols.push(jsonTbl.COLS[j]);
        }
    } else {
        for (var j = 0; j < jsonTbl.LABEL.length; j++) {
            arrayCols.push(j);
        }
    }
    

    for (var i = 0; i < jsonTbl.DATA.length; i++) {

        strHtmlBody += "<tr>";

        for (var j = 0; j < arrayCols.length; j++) {

            if (j === 0 && blChk) {

                strHtmlBody += "<td align='center'><input type='checkbox' class='' value='" + JSON.stringify(jsonTbl.DATA[i])+"' /></td>";
            }

            strHtmlBody += "<td title='"+jsonTbl.LABEL[j]+"'>" + jsonTbl.DATA[i][arrayCols[j]] + "</td>";

            if (j === (arrayCols.length - 1) && blButton) {

                for (var x = 0; x < jsonTbl.BUTTON.length;x++) {
                    strHtmlBody += "<td align='center'><a href='#!' onclick='" + jsonTbl.BUTTON[x].EVENT + "(" + JSON.stringify(jsonTbl.DATA[i]) + ")' class='btn btn-info btn-circle btn-sm'  title='" + jsonTbl.BUTTON[x].TITLE +"' ><i class='fas " + jsonTbl.BUTTON[x].ICON+"'></i></a></td>";
                }

            }

            if (j === (arrayCols.length - 1) && blButtons) {

                if (typeof (jsonTbl.BUTTONS) !== "undefined") {
                    strHtmlBody += "<td align='center'>";
                    for (var x = 0; x < jsonTbl.BUTTONS.length; x++) {
                        if (x > 0) {
                            strHtmlBody += "&nbsp;";
                        }
                        strHtmlBody += "<a href='#!' onclick='" + jsonTbl.BUTTONS[x].EVENT + "(" + JSON.stringify(jsonTbl.DATA[i]) + ")' class='btn btn-info btn-circle btn-sm'  title='" + jsonTbl.BUTTONS[x].TITLE + "' ><i class='fas " + jsonTbl.BUTTONS[x].ICON+"'></i></a>";
                    }
                    strHtmlBody += "</td>";
                }

            }


        }
        strHtmlBody += "</tr>";
    }

    strHtmlBody = "<tbody>" + strHtmlBody + "</tbody>";

    strHtmlTable = "<table id='tbl" + jsonTbl.ID + "' class='table table-bordered'>" + strHtmlHeader + "" + strHtmlBody + "</table>"
    $("#divTable" + jsonTbl.ID).html(strHtmlTable);

    //"scrollX": true

    $("#tbl" + jsonTbl.ID).DataTable({
        "pageLength": nuPageLength,
        "paging": blPaginate,
        "searching": blSearch,
        "info": blInfo
    });

    if (typeof (jsonTbl.CBPaginate) !== "undefined") {

        var nuLength = $("#slctbl" + jsonTbl.ID + "_length").val();

        var nuFrom = ((nuLength * 1) * nuPage) + 1;
        var nuTo = (nuFrom + (nuLength * 1)) - 1;
        if (nuTo > nuRows) {
            nuTo = nuRows;
        }
        $("#tbl" + jsonTbl.ID + "_paginate li.active").find("a").text((nuPage+1));
        $("#tbl" + jsonTbl.ID + "_info").html("Mostrando " + nuFrom + " a " + nuTo + " de " + nuRows + " registros");

        setTimeout(function () {

            $("#tbl" + jsonTbl.ID + "_paginate li.active").find("a").text(nuPage + 1);
            $("#tbl" + jsonTbl.ID + "_info").html("Mostrando " + nuFrom + " a " + nuTo + " de " + nuRows + " registros");

            $("#tbl" + jsonTbl.ID + "_next").removeClass("disabled");
            $("#tbl" + jsonTbl.ID + "_previous").removeClass("disabled");

            $("#tbl" + jsonTbl.ID + "_next").find("a").click(function () {

                var nuLength = $("#slctbl" + jsonTbl.ID + "_length").val();
                console.log(nuPage + ", " + nuLength);
                if (nuRows > nuLength && (nuRows - (nuPage * nuLength)) > 0) {
                    $("#tbl" + jsonTbl.ID + "_info").html("");
                    nuPage++;
                    window[jsonTbl.CBPaginate](nuPage, nuLength);
                }
            });

            $("#tbl" + jsonTbl.ID + "_previous").find("a").click(function () {
                var nuLength = $("#slctbl" + jsonTbl.ID + "_length").val();
                
                if (nuPage > 0) {
                    $("#tbl" + jsonTbl.ID + "_info").html("");
                    nuPage--;
                    window[jsonTbl.CBPaginate](nuPage, nuLength);
                }

            });

            $("#slctbl" + jsonTbl.ID + "_length").change(function () {
                var nuLength = $("#slctbl" + jsonTbl.ID + "_length").val();
                //console.log("Change: "+nuLength);
                window[jsonTbl.CBPaginate](0, nuLength);
            });

        }, 500);
    }
}

function checkAll(strTblId) {

    var blCheck = $("#chk" + strTblId).prop("checked");

    $("#tbl" + strTblId + " > tbody").find("input[type=checkbox]").each(function () {

        $(this).prop("checked", blCheck);

    });


    $("#chk" + strTblId).prop("checked", blCheck);
        
}

function loginGIRApp() {

    var blValidateFields = validateEmptyFields(["tbxContrasena", "tbxUsuario", "tbxCaptcha"]);

    if (blValidateFields) {

        var strUser = $("#tbxUsuario").val();
        var strPassword = $("#tbxContrasena").val();
        var strCaptcha = $("#tbxCaptcha").val();

        var jsonData = {
            strUser: strUser,
            strPassword: strPassword,
            strCaptcha: strCaptcha
        };

        var jsonParams = {
            CBFunction: 'cbLoginGIRApp',
            Action: 'wfLogin.aspx/LoginGIRApp',
            Data: JSON.stringify(jsonData)
        };

        getDataFromADS(jsonParams);

    }
    else {
        var jsonMsg = {
            Title: "Validación",
            Msg: "Usuario, contraseña y captcha son obligatorios",
            Type: "Warning"
        };
        showITCMessage(jsonMsg);
    }
}