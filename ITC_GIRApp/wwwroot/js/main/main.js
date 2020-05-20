$(document).ready(function () {
    
    $('#divResumenClientData').hide();

    document.getElementById("searchClientDataResult").addEventListener("click", () => getPageClientResultData(0, nuPageLengthClientData));

    $("#slcDx").val("");
    $("#select2-slcDx-container").attr("title", "Seleccione Diagnóstico");
    $("#select2-slcDx-container").text("Seleccione Diagnóstico");

    $("#slcDays").val("");
    $("#select2-slcDays-container").attr("title", "Seleccione Antigüedad");
    $("#select2-slcDays-container").text("Seleccione Antigüedad");
});

function getClientDataFromRange() {
    clearHideAll();
    getClientData(0, nuPageLengthClientData);
}

var nuPageLengthClientData = 10;

function getClientData(nuPage, nuRowsPerPage){

    nuPageLengthClientData = nuRowsPerPage;

    var strFromDate = $("#txtFromDate").val();
    var strToDate = $("#txtToDate").val();

    if (strFromDate !== "" && strToDate !== "") {

        var objRequestParam = {
            Data: JSON.stringify({
                UserId: ITC_USERID,
                FromDate: strFromDate,
                ToDate: strToDate,
                Page: nuPage,
                RowsPerPage: nuRowsPerPage
            })
        };

        var jsonParams = {
            CBFunction: 'cbGetClientData',
            Action: '../Client/GetClientData',
            Data: JSON.stringify(objRequestParam),
            Loading: false
        };

        getDataFromADS(jsonParams);

    }

}

function cbGetClientData(jsonResp) {

    if (jsonResp.resp) {

        if (jsonResp.data != null) {

            $("#divClientData").show("slow");

            if ($("#aTabClientData").hasClass("collapsed")) {
                $("#aTabClientData").click();
            }

            var arrayDataTbl = JSON.parse(jsonResp.data);
            var arrayEvents = new Array();

            switch (ITC_ROLE) {
                case "CLIENTE":
                    arrayEvents = [{ ICON: "fa-search", EVENT: "getClientResultData", TITLE: "Click para ver el análisis." }];
                    break;
                case "ADMINISTRACIÓN ANÁLITICA":
                    arrayEvents = [{ ICON: "fa-upload", EVENT: "uploadClientResult", TITLE: "Click para cargar el análisis." }, { ICON: "fa-file", EVENT: "getClientFile", TITLE: "Click para descargar el archivo." }, { ICON: "fa-search", EVENT: "getClientResultData", TITLE: "Click para ver el análisis." }, { ICON: "fa-trash", EVENT: "removeClientResultData", TITLE: "Click para eliminar el análisis." }];
                    break;
                case "MÉDICO":
                    break;
                case "ADMINISTRADOR":
                    arrayEvents = [{ ICON: "fa-upload", EVENT: "uploadClientResult", TITLE: "Click para cargar el análisis." }, { ICON: "fa-file", EVENT: "getClientFile", TITLE: "Click para descargar el archivo." }, { ICON: "fa-search", EVENT: "getClientResultData", TITLE: "Click para ver el análisis." }, { ICON: "fa-trash", EVENT: "removeClientResultData", TITLE: "Click para eliminar el análisis." }];
                    break;
                case "ADMINISTRADOR DEL SERVICIO":
                    arrayEvents = [{ ICON: "fa-upload", EVENT: "uploadClientResult", TITLE: "Click para cargar el análisis." }, { ICON: "fa-file", EVENT: "getClientFile", TITLE: "Click para descargar el archivo." }, { ICON: "fa-search", EVENT: "getClientResultData", TITLE: "Click para ver el análisis." }];
                    break;
                default:

            }

            var jsonTable = {
                DATA: arrayDataTbl,
                LABEL: ["Fecha Creación", "Cliente", "Filas", "Completitud", "Descripción", "Eventos"],
                COLS: ["VCCREATEDON", "CLIENT", "VCCOUNT", "VCCOMPLETENESS", "VCDESCRIPTION"],
                ID: "ClientData",
                ORDERPOSITION: 0,
                BUTTONS: arrayEvents,
                CBPaginate: 'getClientData',
                ROWS: jsonResp.count,
                PAGE: jsonResp.index,
                PAGELENGTH: nuPageLengthClientData
            };

            buildTbl(jsonTable);

        } else {

            var jsonMsg = {
                Title: "Mensaje",
                Msg: "No se encontraron registros para el rango de fechas seleccionadas.",
                Type: "Warning"
            };

            showITCMessage(jsonMsg);
        }
    } else {

        if (jsonResp.type === "Session") {
            $('#divModalMsgSession').modal('show');
        } else {

            var jsonMsg = {
                Title: "Mensaje",
                Msg: jsonResp.msg,
                Type: jsonResp.type
            };

            showITCMessage(jsonMsg);
        }
    }

}

function getClientFile(jsonResp) {
    window.location.href = "../Files/Download?strFileName=" + jsonResp.VCPATH;
}

var nuPageLengthClientResult = 10;

function getPageClientResultData(nuPage, nuRowsPerPage) {


    var objFormFilter = document.getElementById("frmFilterClientData");

    var objFilters = formDataToJson(new FormData(objFormFilter));

    nuPageLengthClientResult = nuRowsPerPage;

    var objRequestParam = {
        Data: JSON.stringify({
            ClientDataId: $("#hddClientDataId").val(),
            Page: nuPage,
            RowsPerPage: nuRowsPerPage,
            ...objFilters
        })
    };

    var jsonParams = {
        CBFunction: 'cbGetClientResultData',
        Action: '../Client/GetClientResultData',
        Data: JSON.stringify(objRequestParam),
        Loading: true
    };

    getDataFromADS(jsonParams);
}

function getClientResultData(jsonResp) {

    //Clear Detail
    $('#frmFilterClientData').trigger("reset");

    $("#slcDx").val("");
    $("#select2-slcDx-container").attr("title", "Seleccione Diagnóstico");
    $("#select2-slcDx-container").text("Seleccione Diagnóstico");

    $("#slcDays").val("");
    $("#select2-slcDays-container").attr("title", "Seleccione Antigüedad");
    $("#select2-slcDays-container").text("Seleccione Antigüedad");

    $("#divUploadResultFile").hide();

    $("#slcDx").val("");
    $("#select2-slcDx-container").attr("title", "Seleccione Diagnóstico");
    $("#select2-slcDx-container").text("Seleccione Diagnóstico");

    $("#slcDays").val("");
    $("#select2-slcDays-container").attr("title", "Seleccione Antigüedad");
    $("#select2-slcDays-container").text("Seleccione Antigüedad");

    if (jsonResp.VCCREATEDON != '') {

        var arrayMonth = ["", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"];

        var arrayGIRAppna = jsonResp.VCCREATEDON.substring(0, 10).split("-");
        if (arrayGIRAppna.length > 1) {
            var strGIRAppna = arrayGIRAppna[2] + " " + arrayMonth[arrayGIRAppna[1] * 1] + " " + arrayGIRAppna[0];

            $("#divGIRAppna").html(strGIRAppna);
        }
    }

    $("#divRowsAna").html("Datos Analizados " + jsonResp.VCCOUNT);

    $("#hddClientDataId").val(jsonResp.ITC_CLIENTDATAID);

    getPageClientResultData(0, 10);
}

function cbGetClientResultData(jsonResp) {

    if (jsonResp.resp) {

        if (jsonResp.data != null) {

            if (!$("#aTabClientData").hasClass("collapsed")) {
                $("#aTabClientData").click();
            }

            $("#divAnaCharts").show("slow");
            $("#divClientResultData").show("slow");
            $("#divContentAnaData").show("slow");
            $("#divContentProbabilidad").show("slow");
            $("#divContentZona").show("slow");
            $("#divContentCargo").show("slow");
            $("#divContentDiagnostico").show("slow");
            $("#divClientDataFilter").show("slow");
            
            /** @type { ClientResultData[] } */
            var arrayDataTbl = JSON.parse(jsonResp.data) || [];

            arrayDataTbl = arrayDataTbl.map(({ ANTIGUEDAD_DIAS, ...rest }) => {
                const nuAntiguedadDias = toInt(ANTIGUEDAD_DIAS);

                const ANTIGUEDAD = nuAntiguedadDias < 30 ?
                    `${ANTIGUEDAD_DIAS} Días` :
                    nuAntiguedadDias >= 365 ?
                        Number.isInteger(nuAntiguedadDias / 365) ?
                            `${nuAntiguedadDias / 365} Años` :
                            `${(nuAntiguedadDias / 365).toFixed()} Años aproximados` :
                        `${(nuAntiguedadDias / 30.4375).toFixed()} Meses`;

                return { ANTIGUEDAD, ...rest };
            });

            var buildTable = {
                DATA: arrayDataTbl,
                LABEL: ["Identificador", "Cargo", "Zona", "Nivel Riesgo", "Expectativa Mejora", "Recomendaciones", "Género", "Edad", "Empresa", "Negocio", "Antiguedad", "Fecha de Análisis"],
                COLS: ["IDENTIFICADOR", "CARGO", "ZONA", "CLASIFICACION", "EXPECTATIVA_DE_MEJORA", "RECOMENDACIONES", "GENERO", "EDAD", "EMPRESA", "NEGOCIO", "ANTIGUEDAD", "FECHA_ANALISIS"],
                ID: "ClientResultData",
                ORDERPOSITION: 0,
                CBPaginate: 'getPageClientResultData',
                ROWS: jsonResp.count,
                PAGE: jsonResp.index,
                PAGELENGTH: nuPageLengthClientResult,
                FIXEDHEADER: true
            };

            buildTbl(buildTable);

            var jsonCharts = JSON.parse(jsonResp.json);

            /********************************************/
            /******************ZONA*********************/
            var arrayZona = JSON.parse(jsonCharts.ZONA);

            if (arrayZona.length > 0) {
                var arrayDataZona = new Array();
                var arrayLabelZona = new Array();

                for (var i = 0; i < arrayZona.length; i++) {
                    arrayDataZona.push(arrayZona[i].VALUE);
                    arrayLabelZona.push(arrayZona[i].LABEL);
                }
                
                $('#divBarChart001').html("<canvas id='canvasBarChart001'></canvas>");

                var jsonChart = {
                    canvasId: 'canvasBarChart001',
                    Label: 'Zona',
                    Labels: arrayLabelZona,
                    Data: arrayDataZona,
                    LblPos: 'left'
                };

                buildBarChart(jsonChart);
            }
            else {
                $('#divBarChart001').html("<div class='card mb-4 py-3 border-left-warning'><div class='card-body'>No se encontraron registros</div></div>");
            }

            /********************************************/
            /******************CARGO*********************/
            var arrayCargo = JSON.parse(jsonCharts.CARGO);

            if (arrayCargo.length > 0) {

                var arrayDataCargo = new Array();
                var arrayLabelCargo = new Array();

                var nuTot = 0;

                for (var i = 0; i < arrayCargo.length; i++) {

                    if (i > 9) {
                        if (i == 10) {
                            arrayDataCargo[10] = arrayCargo[i].VALUE

                            arrayLabelCargo.push("Todas");
                        }
                        else {
                            arrayDataCargo[10] += arrayCargo[i].VALUE;
                        }
                    }
                    else {
                        arrayDataCargo.push(arrayCargo[i].VALUE);
                        arrayLabelCargo.push(arrayCargo[i].LABEL.trim());
                    }

                    nuTot += arrayCargo[i].VALUE * 1;
                }

                var strHtml = "";

                var arrayBarColors = ["", "", "", "", "", "", "", "", "", ""];

                for (var i = 0; i < arrayDataCargo.length; i++) {

                    var nuPer = 0;

                    nuPer = Math.ceil((arrayDataCargo[i] / nuTot) * 100);

                    strHtml += "<h4 class='small font-weight-bold'>" + arrayLabelCargo[i] + " (" + arrayDataCargo[i] + ") <span class='float-right'>" + ((arrayDataCargo[i] / nuTot) * 100).toFixed(2) + "%</span></h4>";
                    strHtml += "<div class='progress mb-4'>";
                    strHtml += "<div class='progress-bar " + arrayBarColors[i] + "' role='progressbar' style='width: " + nuPer + "%' aria-valuenow='" + nuPer + "' aria-valuemin='0' aria-valuemax='100'></div>";
                    strHtml += "</div>";
                }

                $('#divCargo').html(strHtml);
            }
            else {
                $('#divCargo').html("<div class='card mb-4 py-3 border-left-warning'><div class='card-body'>No se encontraron registros</div></div>");
            }


            /*************************************************************/
            /******************CLASIFICACION*****************************/
            var arrayClasificacion = JSON.parse(jsonCharts.CLASIFICACION);
            var arrayDataClasificacion = new Array();
            var arrayLabelClasificacion = new Array();

            for (var i = 0; i < arrayClasificacion.length; i++) {
                arrayDataClasificacion.push(arrayClasificacion[i].VALUE);
                arrayLabelClasificacion.push(arrayClasificacion[i].LABEL);
            }

            $('#divPieChart001').html("<canvas id='canvasPieChart001'></canvas>");

            var jsonChart = {
                canvasId: 'canvasPieChart001',
                Label: 'Probabilidad',
                Labels: arrayLabelClasificacion,
                Data: arrayDataClasificacion,
                LblPos: 'left'
            };

            buildPieChart(jsonChart);


            /********************************************/
            /******************DIAGNOSTICO************/

            const arrayDataDiagnostic = tryParseJson(jsonCharts.DIAGNOSTICO, identity, () => []) || [];

            let arrayLabels = [];

            let arrayAlta = [];

            let arrayMedia = [];

            let arrayOtros = [];

            var blChartDiagnostic = false;

            for (var i = 0; i < arrayDataDiagnostic.length; i++) {
                if (arrayDataDiagnostic[i].FRECUENCIA > 0) {
                    blChartDiagnostic = true;
                    i = arrayDataDiagnostic.length;
                }
            }

            if (blChartDiagnostic) {

                for (const diagnostico of arrayDataDiagnostic) {

                    var nuIndex = arrayLabels.indexOf(diagnostico.DIAGNOSTICO);

                    if (nuIndex === -1) {

                        arrayLabels.push(diagnostico.DIAGNOSTICO);

                        if (diagnostico.CLASIFICACION === "Alta") {
                            arrayAlta.push(diagnostico.FRECUENCIA);

                            arrayMedia.push(0);
                        }

                        if (diagnostico.CLASIFICACION === "Media") {
                            arrayMedia.push(diagnostico.FRECUENCIA);

                            arrayAlta.push(0);
                        }

                        arrayOtros.push(diagnostico.FRECUENCIA);
                    }
                    else {
                        if (diagnostico.CLASIFICACION === "Alta") {
                            arrayAlta[nuIndex] = diagnostico.FRECUENCIA;
                        }

                        if (diagnostico.CLASIFICACION === "Media") {
                            arrayMedia[nuIndex] = diagnostico.FRECUENCIA;
                        }

                        arrayOtros[nuIndex] += diagnostico.FRECUENCIA;
                    }
                }

                $('#divBarChart002').html("<canvas id='canvasBarChart002'></canvas>");

                var ctx = document.getElementById('canvasBarChart002');

                var myChart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: arrayLabels,
                        datasets: [
                            {
                                label: 'Media',
                                data: arrayMedia,
                                backgroundColor: '#e0481d',
                            },
                            {
                                label: 'Alta',
                                data: arrayAlta,
                                backgroundColor: '#295ece',
                            },
                            {
                                label: "Total",
                                data: arrayOtros,
                                backgroundColor: "#882192"
                            }
                        ]
                    },
                    options: {
                        scales: {
                            xAxes: [{
                                stacked: true,
                                maxBarThickness: 100
                            }
                            ],
                            yAxes: [{ stacked: true }]
                        }
                    }
                });

            } else {
                $('#divBarChart002').html("<div class='card mb-4 py-3 border-left-warning'><div class='card-body'>No se encontraron registros</div></div>");
            }

        } else {

            var jsonMsg = {
                Title: "Mensaje",
                Msg: "No se encontraron datos procesados.",
                Type: "Warning"
            };

            showITCMessage(jsonMsg);
        }
    } else {

        $("#hddClientDataId").val("");

        if (jsonResp.type == "Session") {
            $('#divModalMsgSession').modal('show');
        } else {

            var jsonMsg = {
                Title: "Mensaje",
                Msg: jsonResp.Msg,
                Type: jsonResp.Type
            };

            showITCMessage(jsonMsg);
        }
    }

}

function sort_by_key(array, key) {
    return array.sort(function (a, b) {
        var x = a[key]; var y = b[key];
        return ((x < y) ? -1 : ((x > y) ? 1 : 0));
    });
}

function uploadClientResult(jsonResp) {

    clearHideAll();

    $("#hddClientDataId").val(jsonResp.ITC_CLIENTDATAID);
    $("#divClientData").show();
    $("#divUploadResultFile").show("slow");

    $('html,body').animate({
        scrollTop: $('#divUploadResultFile').offset().top
    }, 'slow');

}

function removeClientResultData(jsonResp) {

    clearHideAll();

    $("#divClientData").show();

    var objRequestParam = {
        Data: JSON.stringify({
            ClientDataId: jsonResp.ITC_CLIENTDATAID
        })
    };

    var jsonParams = {
        CBFunction: 'cbRemoveClientResultData',
        Action: '../Client/RemoveClientResultData',
        Data: JSON.stringify(objRequestParam),
        Loading: true
    };

    getDataFromADS(jsonParams);

}

function cbRemoveClientResultData(jsonResp) {

    if (jsonResp.resp) {

        clearHideAll();

        var jsonMsg = {
            Title: "Mensaje",
            Msg: "Los datos fueron eliminados con éxito.",
            Type: "Success"
        };
        showITCMessage(jsonMsg);

    } else {

        if(jsonResp.type == "Session") {
            $('#divModalMsgSession').modal('show');
        } else {
            var jsonMsg = {
                Title: "Mensaje",
                Msg: jsonResp.msg,
                Type: "Error"
            };
            showITCMessage(jsonMsg);
        }
    }

}

function clearHideAll() {

    $("#hddClientDataId").val("");

    //Clear Detail
    $('#frmFilterClientData').trigger("reset");

    $("#slcDx").val("");
    $("#select2-slcDx-container").attr("title", "Seleccione Diagnóstico");
    $("#select2-slcDx-container").text("Seleccione Diagnóstico");

    $("#slcDays").val("");
    $("#select2-slcDays-container").attr("title","Seleccione Antigüedad");
    $("#select2-slcDays-container").text("Seleccione Antigüedad");

    $("#divClientData").hide();

    $("#divAnaCharts").hide();
    $("#divContentAnaData").hide();
    $("#divClientResultData").hide();
    $("#divContentProbabilidad").hide();
    $("#divContentZona").hide();
    $("#divContentCargo").hide();
    $("#divContentDiagnostico").hide();
    $("#divClientDataFilter").hide();

    //Upload
    $("#divUploadResultFile").hide();

}

function setTodayDate() {

    var dtToday = new Date();
    var dd = dtToday.getDate();
    var mm = dtToday.getMonth() + 1;

    var dd2 = dtToday.getDate();

    var mm2 = dtToday.getMonth() + 1;

    var yyyy = dtToday.getFullYear();
    var yyyy2 = dtToday.getFullYear();

    if (mm === 1) {
        mm2 = 12;
        yyyy2 = yyyy2 - 1;
    }

    if (mm < 10) {
        mm = "0" + mm;
    }

    if (mm2 < 10) {

        if (mm2 === 2 && dd === 29) {

            dd = 28;
        }

        mm2 = "0" + mm2;
    }

    if (dd < 10) {
        dd = "0" + dd;
    }

    if (dd2 < 10) {
        dd2 = "0" + dd2;
    }

    var strIniDate = (yyyy * 1) - 1 + '-' + mm + '-' + dd;
    var strToDate = yyyy + '-' + mm + '-' + dd;

    $('#txtFromDate').val(strIniDate);
    $('#txtToDate').val(strToDate);

}