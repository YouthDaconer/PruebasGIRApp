document.addEventListener("DOMContentLoaded", bootstrap);

var nuVerificationCode = 0;

/**
 *
 */
function bootstrap()
{
    // Add event listeners

    document.getElementById("imgCaptcha")
        .addEventListener("click", generateCaptcha);

    document.getElementById("btnLogin")
        .addEventListener("click", loginGIRApp);

    document.getElementById("btnSignup")
        .addEventListener("click", signupGIRApp);

    document.getElementById("aShowHidePassword")
        .addEventListener("click", togglePassword);

    document.getElementById("aShowHidePassword2")
        .addEventListener("click", togglePassword2);

    document.getElementById("lblSignup")
        .addEventListener("click",
            () => $("#divLogin").fadeOut(() => $("#divRegister").removeClass("d-none").fadeIn()));

    document.getElementById("lblLogin")
        .addEventListener("click",
            () => $("#divRegister").fadeOut(() => $("#divLogin").fadeIn()));

    document.getElementById("lblRecoverPassword")
        .addEventListener("click", forgotPassword);

    document.getElementById("btnRecoverPwd")
        .addEventListener("click", recoverPassword);

    document.getElementById("btnSendCode")
        .addEventListener("click", validateCode);

    document.getElementById("btnSendMailCode")
        .addEventListener("click", validateMailCode);

    document.getElementById("btnCloseSendMailCodeModal")
        .addEventListener("click", () => clearModalData());

    generateCaptcha();

    fetchRegions();

    /**
     * @type { JQueryValidation.RulesDictionary }
     */
    const objValidationRules =
        { username : { required : true },
          password : { required : true },
          captchaCode :
          { required : true,
            maxlength : 4,
            minlength: 4,
            number : true } };

    $("#frmLogin").validate(
        { rules: objValidationRules,
          errorClass : "is-invalid",
          errorPlacement : (error, element) => element.parent().find("div.invalid-feedback").text(error.text()),
          onkeyup : (element) => $(element).valid(),
          validClass : "is-valid",
          errorElement : "div",
          messages :
          { username : { required: "El nombre de usuario es requerido" },
            password : { required: "La contraseña es requerida" },
            captchaCode :
                { required: "Ingresa el código captcha de la imagen",
                  maxlength: "Ingresa los 4 dígitos del codigo",
                  minlength: $.validator.format("Ingresa los {0} dígitos del código"),
                  number: "Ingresa solo números." } } });

    /** @type { JQueryValidation.RulesDictionary } */
    const objValidationRules2 =
        { businessName : { required : true, minlength : 3 },
          documentNumber : { required : true, number : true, maxlength : 11 },
          contactName : { required : true, minlength : 3 },
          mobile : { required : true, number : true, minlength : 10 },
          email : { required : true, email : true },
          region : { required : true },
          password2 : { required : true, minlength : 14 } };

    $("#frmRegister").validate(
        { rules: objValidationRules2,
          errorClass : "is-invalid",
          errorPlacement : (error, element) => element.parent().find("div.invalid-feedback").text(error.text()),
          onkeyup : (element) => $(element).valid(),
          validClass : "is-valid",
          errorElement : "div",
          messages :
          {  businessName: { required: "La razón social es requerida", minlength: $.validator.format("Ingresa al menos {0} caracteres") },
             documentNumber: { required: "El nit es requerido.", number: "Ingresa solo números", maxlength : $.validator.format("No ingreses mas {0} caracteres") },
             contactName: { required: "El nombre de contacto es requerido", minlength: $.validator.format("Ingresa al menos {0} caracteres") },
             mobile: { required: "El número telefónico es requerido", number: "Ingresa solo números", minlength: $.validator.format("Ingresa al menos {0} números") },
             email: { required: "El correo electrónico es requerido", email: "Ingresa un email valido" },
             region: { required: "Selecciona una región" },
             password2 : { required : "La contraseña es requerida", minlength: $.validator.format("Ingresa al menos {0} caracteres") } } });

}

/**
 *
 */
function generateCaptcha()
{
    // Clear captcha code
    $("txtCaptcha").val("");

    var objRequestParams =
        { CBFunction : "cbGenerateCaptcha",
          Action : "Home/GenerateCaptcha",
          Loading : false };

    getDataFromADS(objRequestParams);

    // Generate a new captcha within an interval
    setTimeout(function () {
        window.location.reload();
    }, 180000);
}

/**
 *
 * @param { ApplicationResponse } jsonResponse
 */
function cbGenerateCaptcha(jsonResp)
{
    if (!isNull(jsonResp.data)) $("#imgCaptcha").prop("src", jsonResp.data)
}

/**
 *
 * @param { Event } event
 */
function loginGIRApp(event)
{
    event.preventDefault();

    if ($("#frmLogin").valid())
    {
        /**
         * @type { HTMLInputElement[] }
         */
        const [ tbxUsername, tbxPassword, tbxCaptcha, chkIsFirstTime ] = document.querySelectorAll("#txtUsuario, #txtContrasena, #txtCaptcha, #chkIsFirstTime");

        var objJsonParams = {
            SAMAccountName: tbxUsername.value,
            Password : tbxPassword.value,
            Captcha : tbxCaptcha.value,
            IsFirstTime: chkIsFirstTime.checked
        };

        var objRequestParams = {
            CBFunction: "cbLoginGIRApp",
            Action : "Home/Login",
            Data: JSON.stringify({Data : objJsonParams}),
            Loading: true
        };

        getDataFromADS(objRequestParams);
    }
}

/**
 *
 * @param { ApplicationResponse } objJsonResponse
 */
function cbLoginGIRApp(objJsonResponse)
{
    if (!objJsonResponse.resp)
    {
        showITCMessage({ Title: "Información", Msg: objJsonResponse.msg, Type: objJsonResponse.type })
    }
    else
    {
        window.location.href = isNullOrEmpty(objJsonResponse.redirect) ? "" : objJsonResponse.redirect;
    }
}

/**
 *
 * @param { Event } event
 */
function signupGIRApp(event)
{
    event.preventDefault();

    if ($("#frmRegister").valid())
    {
        /** @type { HTMLInputElement[] } */
        const [ txtBusinessName,
            txtDocNumber,
            txtContactName,
            txtMobile,
            txtEmail,
            txtPassword,
            slcRegion,
            chkAcceptTerms ] = document
            .querySelectorAll("#txtBusinessName, #txtDocumentNumber, #txtContactName, #txtMobile, #txtEmail, #slcRegion, #txtContrasena2, #chkAcceptTermsConditions");

        if (!$("#chkAcceptTermsConditions").prop('checked'))
        {
            showITCMessage({ Title : "Validación",
              Msg: "Debe aceptar los términos y condiciones para continuar.",
              Type: "Warning" });

            return;
        }

        var strPassword = $("#txtContrasena2").val();
        var strExpUpperCase = /[A-Z]/;
        var strExpLowerCase = /[a-z]/;
        var strExpNumber = /[0-9]/;
        //if (strPassword.length < 6
        if (strPassword.length < 14 || strPassword.length > 32 || strPassword.includes(txtDocNumber.value) || !strExpUpperCase.test(strPassword)
            || !strExpLowerCase.test(strPassword) || !strExpNumber.test(strPassword) || !validateCharacteres(strPassword))
        {
            var strHtmlList = '<ul>';
            strHtmlList += '<li>Mayúsculas</li>';
            strHtmlList += '<li>Minúsculas</li>';
            strHtmlList += '<li>Números</li>';
            strHtmlList += '<li>Entre 14 y 32 caracteres</li>';
            strHtmlList += '<li>Al menos dos símbolos: ! " # $ % & \' ( ) * + , - . / : ; < = > ? @[\] ^ _` { | } ~</li>';
            strHtmlList += '<li>No puede contener el NIT ingresado</li>';
            strHtmlList += '</ul>';

            showITCMessage({
                Title: "Validación",
                Msg: "Por favor validar las políticas de seguridad de la contraseña, recuerde que debe contener:<br/><br/>" + strHtmlList,
                Type: "Warning"
            });

            return;
        }

        const objJsonParams =
            { BusinessName : txtBusinessName.value,
              DocumentNumber : txtDocNumber.value,
              ContactName : txtContactName.value,
              Email : txtEmail.value,
              Mobile : txtMobile.value,
              Region: $("#slcRegion").find(":selected").val(),
            Password: strPassword
        };

        const objRequestParams = {
            CBFunction: "cbSignupGIRApp",
            Action: "Home/RegisterUser",
            Data: JSON.stringify({ Data : objJsonParams }),
            Loading: true
        };

        getDataFromADS(objRequestParams);
    }
}

function validateCharacteres(strValidate) {

    var strExpCharter = /[!"#$%&'()*+,-./:;<=>?@[\]^_`{|}~]/;
    var nuCount = 0;

    for (var x = 0; x < strValidate.length; x++) {
        var strCharacter = strValidate.substr(x, 1);
        if (strExpCharter.test(strCharacter)) {
            nuCount++;
        }
    }

    if (nuCount >= 2) {
        return true;
    }
    else {
        return false;
    }
}

/**
 *
 * @param { ApplicationResponse } objJsonResponse
 */
function cbSignupGIRApp(objJsonResponse)
{
    if (!objJsonResponse.resp)
    {
        showITCMessage({ Title: "Información", Msg: objJsonResponse.msg, Type: "Error" })
    }
    else
    {
        showITCMessage({
            Title: "Registro",
            Msg: objJsonResponse.msg,
            Type: "Success"
        });

        setTimeout(() => { window.location.href = isNullOrEmpty(objJsonResponse.redirect) ? "" : objJsonResponse.redirect; }, 2000);

    }
}

/**
 *
 * @param { Event } event
 */
function forgotPassword(event)
{
    event.preventDefault();

    if ($("#txtUsuario").val() !== "" && $("#txtCaptcha").val() !== "") {
        $("#txtEmailConfirm").val("");
        $("#recoverPasswordModal").modal("toggle");
    }
    else
    {
        var jsonMsg = {
            Title: "Validación",
            Msg: "Usuario y captcha son obligatorios para restablecer tu contraseña.",
            Type: "Warning"
        }
        showITCMessage(jsonMsg);
    }
}

/**
 *
 * @param { EventTargetType<HTMLButtonElement> } event
 */
function recoverPassword(event)
{
    event.preventDefault();

    $("#frmForgotPwd").validate({
        rules : { email : { required : true, email : true } },
        errorClass : "is-invalid",
        errorPlacement : (error, element) => element.parent().find("div.invalid-feedback").text(error.text()),
        onkeyup : (element) => $(element).valid(),
        validClass : "is-valid",
        errorElement : "div",
        messages : {
            email: {
                required: "El correo electrónico es requerido",
                email: "Ingresa un email valido"
            }
        }
    });

    if ($("#frmForgotPwd").valid())
    {
        const objJsonParams =
        {
            Email: $("#txtEmailConfirm").val(),
            SAMAccountName: $("#txtUsuario").val(),
            Captcha: $("#txtCaptcha").val()
        };

        const objRequestParams =
        {
            CBFunction: "cbRecoverPassword",
            Action: "Home/SendCode",
            Data: JSON.stringify({ Data: objJsonParams})
        };

        getDataFromADS(objRequestParams);
    }
}

/**
 *
 * @param { applicationResponse } objJsonResp
 */
function cbRecoverPassword(objJsonResp)
{

    if (objJsonResp.resp && objJsonResp.type === "Success") {

        showITCMessage(
            {
                Title: "Validación",
                Msg: objJsonResp.msg,
                Type: objJsonResp.type
            });

        $("#txtEmailConfirm").val("");
        $("#txtNewPassword").val("");
        $("#txtNewPasswordConfirmation").val("");
        $("#txtCode").val("");
        $("#recoverPasswordModal").modal('hide');
        $("#sendCodeModal").modal('show');
    }
    else {
        showITCMessage(
            {
                Title: "Validación",
                Msg: objJsonResp.msg,
                Type: objJsonResp.type
            });
    }
}

/**
 *
 * @param { Event } event
 */
function validateCode(event) {

    event.preventDefault();

    $("#frmSendCode").validate({
        rules: { code: { required: true, number: true }, NewPassword: { required: true, minlength: 14, maxlength: 32 }, NewPasswordConfirmation: { required: true, minlength: 14, maxlength: 32} },
        errorClass: "is-invalid",
        errorPlacement: (error, element) => element.parent().find("div.invalid-feedback").text(error.text()),
        onkeyup: (element) => $(element).valid(),
        validClass: "is-valid",
        errorElement: "div",
        messages: {
            code: {
                required: "Ingrese el código de verificación",
                number:"Ingrese solo números"
            },
            NewPassword: {
                required: "Ingrese la nueva contraseña",
                minlength: $.validator.format("Ingresa al menos {0} caracteres"),
                maxlength: $.validator.format("Ingresa menos de {0} caracteres")
            },
            NewPasswordConfirmation: {
                required: "Confirme su nueva contraseña",
                minlength: $.validator.format("Ingresa al menos {0} caracteres"),
                maxlength: $.validator.format("Ingresa menos de {0} caracteres")
            }
        }
    });

    var strPassword = $("#txtNewPassword").val();
    var strConfirmPassword = $("#txtNewPasswordConfirmation").val();

    if (strPassword != strConfirmPassword) {

        showITCMessage({
            Title: "Validación",
            Msg: "Las contraseñas ingresadas no coinciden.",
            Type: "Warning"
        });

        return;
    }

    var strExpUpperCase = /[A-Z]/;
    var strExpLowerCase = /[a-z]/;
    var strExpNumber = /[0-9]/;

    if (strPassword.length < 14 || strPassword.length > 32 || strPassword.includes($("#txtUsuario").val()) || !strExpUpperCase.test(strPassword)
        || !strExpLowerCase.test(strPassword) || !strExpNumber.test(strPassword) || !validateCharacteres(strPassword)) {
        var strHtmlList = '<ul>';
        strHtmlList += '<li>Mayúsculas</li>';
        strHtmlList += '<li>Minúsculas</li>';
        strHtmlList += '<li>Números</li>';
        strHtmlList += '<li>Entre 14 y 32 caracteres</li>';
        strHtmlList += '<li>Al menos dos símbolos: ! " # $ % & \' ( ) * + , - . / : ; < = > ? @[\] ^ _` { | } ~</li>';
        strHtmlList += '<li>No puede contener el nombre de usuario</li>';
        strHtmlList += '</ul>';

        showITCMessage({
            Title: "Validación",
            Msg: "Por favor validar las políticas de seguridad de la contraseña, recuerde que debe contener:<br/><br/>" + strHtmlList,
            Type: "Warning"
        });

        return;
    }

    if ($("#frmSendCode").valid()) {

        const objJsonData =
        {
            SAMAccountName: $("#txtUsuario").val(),
            Password: $("#txtNewPassword").val(),
            CodeVerification: $("#txtCode").val()
        };

        const objRequestParams =
        {
            CBFunction: "cbValidateCode",
            Action: "Home/ValidateCode",
            Data: JSON.stringify({ Data: objJsonData }),
            Loading : true
        };

        getDataFromADS(objRequestParams);
    }
}

/**
 *
 * @param { ApplicationResponse } objJsonResp
 */
function cbValidateCode(objJsonResp) {

    if (objJsonResp.resp && objJsonResp.type === "Success") {

        showITCMessage(
            {
                Title: "Validación",
                Msg: objJsonResp.msg,
                Type: objJsonResp.type
            });

        $("#sendCodeModal").modal('hide');
    }
    else {
        showITCMessage(
            {
                Title: "Validación",
                Msg: objJsonResp.msg,
                Type: objJsonResp.type
            });
    }
}

/**
 *
 */
function fetchRegions()
{
    const objRequestParams =
        { CBFunction: "cbFetchRegions",
          Action : "Home/GetRegions",
          Data: JSON.stringify({}),
          Loading : true };

    getDataFromADS(objRequestParams);
}

/**
 *
 * @param { ApplicationResponse } objJsonResponse
 */
function cbFetchRegions(objJsonResponse)
{

    if ( objJsonResponse.resp == true )
    {
        /**
         * @type { Json }
         */
        const arrayRegions = tryParseJson(objJsonResponse.data, identity, () => []);

        $("#slcRegion")
            .attr("disabled", false)
            .attr("readonly", false)
            .empty()
            .append(arrayRegions
                .map((region) => pair(`${region.Department} - ${region.City}`, region.ID))
                .map((pairRegion) =>
                    $("<option><option").text(fst(pairRegion)).val(snd(pairRegion))));
    }
    else
        showITCMessage({ Title: "Información", Msg: objJsonResponse.msg, Type: objJsonResponse.type });
}

/**
 *
 * @param { Event } event
 */
function togglePassword(event)
{
    event.preventDefault();

    const tbxPassword = $("#show_hide_password input");

    if ( tbxPassword.attr("type") == "text" )
    {
        tbxPassword
            .attr("type", "password")
            .siblings("#aShowHidePassword")
            .empty()
            .append($("<i></i>").addClass("fa fa-eye-slash").attr("aria-hidden", true));

    } else if ( tbxPassword.attr("type") == "password" )
    {
        tbxPassword
            .attr("type", "text")
            .siblings("#aShowHidePassword")
            .empty()
            .append($("<i></i>").addClass("fa fa-eye").attr("aria-hidden", true));
    }
}


/**
 *
 * @param { Event } event
 */
function togglePassword2(event)
{
    event.preventDefault();

    const tbxPassword = $("#show_hide_password2 input");

    if (tbxPassword.attr("type") == "text")
    {
        tbxPassword
            .attr("type", "password")
            .siblings("div")
            .find("#aShowHidePassword2")
            .empty()
            .append($("<i></i>").addClass("fa fa-eye-slash").attr("aria-hidden", true));

    } else if (tbxPassword.attr("type") == "password")
    {
        tbxPassword
            .attr("type", "text")
            .siblings("div")
            .find("#aShowHidePassword2")
            .empty()
            .append($("<i></i>").addClass("fa fa-eye").attr("aria-hidden", true));
    }
}

/**
 *
 * @param { Event } event
 */
function validateMail()
{

    if ($('#divRegister').css('display') !== "none"){

        const txtEmail = document.getElementById("txtEmail");

        console.log($("#frmRegister").data("validator").check("#txtEmail"));

        if ($("#frmRegister").data("validator").check("#txtEmail")){

            const objJsonParams = {Data: txtEmail.value};

            const objRequestParams = {
                CBFunction: "cbValidateEmail",
                Action: "Home/ValidateEmail",
                Data: JSON.stringify(objJsonParams),
                Loading: true
            };

            getDataFromADS(objRequestParams);

        }
    }
}

/**
 *
 * @param { ApplicationResponse } objJsonResponse
 */
function cbValidateEmail(objJsonResponse)
{
    if (objJsonResponse.resp == true) {
        nuVerificationCode = +objJsonResponse.data;
        $("#modalSendMailCode").modal("toggle");
    }
    else {
        showITCMessage({ Title: "Información", Msg: "No fue posible enviar el código de verificación, intente de nuevo", Type: "Error" });
    }
}

/**
 *
 * @param { Event } event
 */
function validateMailCode(event)
{
    event.preventDefault();

    if ( nuVerificationCode == $("#txtMailCode").val())
    {
        $("#btnSignup")
            .attr("disabled", false)
            .removeClass("btn-secondary")
            .addClass("btn-primary");

        $("#modalSendMailCode").modal("toggle");

        $("#txtEmail").attr("readonly", true);

        $("#btnSendMailCode").attr("disabled", true);
    }
    else
    {
        $("#codeEmailError").show("slow");
    }
}

function clearModalData()
{
     $("#codeEmailError").hide();

     $("#txtMailCode").val("");

     nuVerificationCode = 0;
}