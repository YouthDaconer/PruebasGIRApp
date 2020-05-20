// Set new default font family and font color to mimic Bootstrap's default styling
Chart.defaults.global.defaultFontFamily = 'Nunito', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';
/**
 * Desc: Metodo que se encarga de formatear un numero, para incluirlo en los labels de los graficos.
 * @param {any} number
 * @param {any} decimals
 * @param {any} dec_point
 * @param {any} thousands_sep
 **/
function number_format(number, decimals, dec_point, thousands_sep) {
    // *     example: number_format(1234.56, 2, ',', ' ');
    // *     return: '1 234,56'
    number = (number + '').replace(',', '').replace(' ', '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
        sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep,
        dec = (typeof dec_point === 'undefined') ? '.' : dec_point,
        s = '',
        toFixedFix = function (n, prec) {
            var k = Math.pow(10, prec);
            return '' + Math.round(n * k) / k;
        };
    // Fix for IE parseFloat(0.55).toFixed(0) = 0;
    s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.');
    if (s[0].length > 3) {
        s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    }
    if ((s[1] || '').length < prec) {
        s[1] = s[1] || '';
        s[1] += new Array(prec - s[1].length + 1).join('0');
    }
    return s.join(dec);
}

/**
 *
 * @param {any} jsonChart
 * jsonChart.Id: id del canvas donde se pintara el grafico
 * jsonChart.Labels: Array donde se envian los labels a mostrar en las barras
 * jsonChart.Data: Array donde se envian los valores a mostrar en las barras
 * jsonChart.Label: Label general del grafico
 */
function buildBarChart(jsonChart) {

    for (var i = 0; i < jsonChart.Labels.length; i++){
        jsonChart.Labels[i] = lowerCaseAllWordsExceptFirstLetters(jsonChart.Labels[i]) + " (" + jsonChart.Data[i]+")";
    }

    var nuMin = 0;
    var nuMax = 10;

    if (typeof (jsonChart.Data) !== "undefined"){
        nuMax = Math.max.apply(null, jsonChart.Data);
    }

    // Bar Chart Example
    var ctx = document.getElementById(jsonChart.canvasId);
    var strFormatLabel = "";

    var myBarChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: jsonChart.Labels,
            datasets: [{
                label: jsonChart.Label,
                backgroundColor: "#00afef",
                hoverBackgroundColor: "#10963B",
                borderColor: "#00afef",
                data: jsonChart.Data
            }],
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                xAxes: [{
                    gridLines: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        maxTicksLimit: 10,
                        fontSize: 9
                    },
                    maxBarThickness: 50,
                }],
                yAxes: [{
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 8,
                        max: nuMax,
                        min: nuMin
                    },
                    gridLines: {
                        color: "rgb(234, 236, 244)",
                        zeroLineColor: "rgb(234, 236, 244)",
                        drawBorder: false,
                        borderDash: [2],
                        zeroLineBorderDash: [2]
                    }
                }],
            },
            legend: {
                display: false
            },
            tooltips: {
                titleMarginBottom: 10,
                titleFontColor: '#6e707e',
                titleFontSize: 14,
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                caretPadding: 10,
                callbacks: {
                    label: function (tooltipItem, chart) {
                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
                        return datasetLabel + ': ' +strFormatLabel+ + number_format(tooltipItem.yLabel);
                    }
                }
            },
        }

    });

}

/**
 * @param {any} jsonChart
 * jsonChart.Id: id del canvas donde se pintara el grafico
 * jsonChart.Labels: Array donde se envian los labels a mostrar en las barras
 * jsonChart.Data: Array donde se envian los valores a mostrar en las barras
 * jsonChart.Label: Label general del grafico
 */
function buildPieChart(jsonChart) {

    if (typeof (jsonChart.LblPos) === 'undefined') {
        jsonChart.LblPos = 'top';
    }

    // Bar Chart Example
    var ctx = document.getElementById(jsonChart.canvasId);
    var strFormatLabel = "";

    if (typeof (jsonChart.Size) !== 'undefined') {
        ctx.height = (ctx.height * jsonChart.Size);
    } else {
        ctx.height = (ctx.height * 1.35);
    }

    var myPieChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: jsonChart.Labels,
            datasets: [{
                data: jsonChart.Data,
                backgroundColor: ["#295ece", "#e0481d", "#882192", "#FF851B", "#7FDBFF", "#B10DC9", "#FFDC00", "#001f3f", "#39CCCC", "#01FF70", "#85144b", "#F012BE", "#3D9970", "#11BE70", "#BEAA70", "#85FF70", "#85854b", "#F085BE", "#3D8570", "#118570", "#BE8570", "#BEA070","#00afef", "#FF4136", "#2ECC40", "#FF851B", "#7FDBFF", "#B10DC9", "#FFDC00", "#001f3f", "#39CCCC", "#01FF70", "#85144b", "#F012BE", "#3D9970", "#11BE70", "#BEAA70", "#85FF70", "#85854b", "#F085BE", "#3D8570", "#118570", "#BE8570", "#BEA070"],
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }],
        },
        options: {
            maintainAspectRatio: false,
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                caretPadding: 10,
            },
            legend: {
                display: true,
                position: jsonChart.LblPos
            },
            cutoutPercentage: 60,
        },
    });

}

/**
 * @param {any} jsonChart
 * jsonChart.Id: id del canvas donde se pintara el grafico
 * jsonChart.Labels: Array donde se envian los labels a mostrar en las barras
 * jsonChart.Data: Array donde se envian los valores a mostrar en las barras
 * jsonChart.Label: Label general del grafico
 */
function buildLineChart(jsonChart) {

    // Bar Chart Example
    var ctx = document.getElementById(jsonChart.canvasId);
    var strFormatLabel = "";

    var myLineChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: jsonChart.Labels,
            datasets: [{
                label: jsonChart.Label,
                lineTension: 0.3,
                backgroundColor: "rgba(78, 115, 223, 0.05)",
                borderColor: "rgba(78, 115, 223, 1)",
                pointRadius: 3,
                pointBackgroundColor: "rgba(78, 115, 223, 1)",
                pointBorderColor: "rgba(78, 115, 223, 1)",
                pointHoverRadius: 3,
                pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                pointHitRadius: 10,
                pointBorderWidth: 2,
                data: jsonChart.Data
            }],
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                xAxes: [{
                    time: {
                        unit: 'date'
                    },
                    gridLines: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        maxTicksLimit: 7
                    }
                }],
                yAxes: [{
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10,
                        // Include a dollar sign in the ticks
                        callback: function (value, index, values) {
                            return '' + number_format(value);
                        }
                    },
                    gridLines: {
                        color: "rgb(234, 236, 244)",
                        zeroLineColor: "rgb(234, 236, 244)",
                        drawBorder: false,
                        borderDash: [2],
                        zeroLineBorderDash: [2]
                    }
                }],
            },
            legend: {
                display: false
            },
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                titleMarginBottom: 10,
                titleFontColor: '#6e707e',
                titleFontSize: 14,
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                intersect: false,
                mode: 'index',
                caretPadding: 10
            }
        }
    });

}

/**
 * @param {any} jsonChart
 * jsonChart.Id: id del canvas donde se pintara el grafico
 * jsonChart.Labels: Array donde se envian los labels a mostrar en las barras
 * jsonChart.Module: Array donde se envian los labels a mostrar en las barras
 * jsonChart.Data0...5: Array donde se envian los valores a mostrar en las barras
 * jsonChart.Label: Label general del grafico
 */
function buildMultiLineChart(jsonChart) {

    // Bar Chart Example
    var ctx = document.getElementById(jsonChart.canvasId);
    var strFormatLabel = "";

    var myLineChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: jsonChart.Labels,
            datasets: [
                {
                    label: jsonChart.Module[0],
                    lineTension: 0.3,
                    backgroundColor: "rgba(78, 115, 223, 0.05)",
                    borderColor: "rgba(0, 78, 78, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data0
                },
                {
                    label: jsonChart.Module[1],
                    lineTension: 0.3,
                    backgroundColor: "rgba(255, 0, 0, 0.05)",
                    borderColor: "rgba(255, 0, 0, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data1
                },
                {
                    label: jsonChart.Module[2],
                    lineTension: 0.3,
                    backgroundColor: "rgba(0, 255, 0, 0.05)",
                    borderColor: "rgba(0, 255, 0, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data2
                },
                {
                    label: jsonChart.Module[3],
                    lineTension: 0.3,
                    backgroundColor: "rgba(0, 0, 255, 0.05)",
                    borderColor: "rgba(0, 0, 255, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data3
                },
                {
                    label: jsonChart.Module[4],
                    lineTension: 0.3,
                    backgroundColor: "rgba(78, 115, 223, 0.05)",
                    borderColor: "rgba(115, 115, 0, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data4
                },
                {
                    label: jsonChart.Module[5],
                    lineTension: 0.3,
                    backgroundColor: "rgba(20, 223, 20, 0.05)",
                    borderColor: "rgba(115, 115, 0, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data5
                },
                {
                    label: jsonChart.Module[6],
                    lineTension: 0.3,
                    backgroundColor: "rgba(223, 20, 20, 0.05)",
                    borderColor: "rgba(115, 115, 0, 1)",
                    pointRadius: 3,
                    pointBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointBorderColor: "rgba(78, 115, 223, 1)",
                    pointHoverRadius: 3,
                    pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
                    pointHoverBorderColor: "rgba(78, 115, 223, 1)",
                    pointHitRadius: 10,
                    pointBorderWidth: 2,
                    data: jsonChart.Data5
                }],
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                xAxes: [{
                    time: {
                        unit: 'date'
                    },
                    gridLines: {
                        display: false,
                        drawBorder: false
                    },
                    ticks: {
                        maxTicksLimit: 7
                    }
                }],
                yAxes: [{
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10
                    },
                    gridLines: {
                        color: "rgb(234, 236, 244)",
                        zeroLineColor: "rgb(234, 236, 244)",
                        drawBorder: false,
                        borderDash: [2],
                        zeroLineBorderDash: [2]
                    }
                }],
            },
            legend: {
                display: false
            },
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                titleMarginBottom: 10,
                titleFontColor: '#6e707e',
                titleFontSize: 14,
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                intersect: false,
                mode: 'index',
                caretPadding: 10
            }
        }
    });

}

function lowerCaseAllWordsExceptFirstLetters(string) {
    return string.replace(/\w\S*/g, function (word) {
        return word.charAt(0) + word.slice(1).toLowerCase();
    });
}