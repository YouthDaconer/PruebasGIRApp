using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

using System.Data;
using System.Net.Http;
using System.Threading.Tasks;

using ITC_GIRApp.Models;

using ITC_DBConnection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using ITC_GIRApp.Util;

namespace ITC_GIRApp.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Vista principal
        /// </summary>
        /// <returns></returns>
        [HttpGet("/")]
        public IActionResult Index() => View();

        /// <summary>
        /// Vista privacidad
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Privacy")]
        public IActionResult Privacy() => View();

        /// <summary>
        /// Vista error
        /// </summary>
        /// <returns></returns>
        [HttpGet("/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        /*
            Author: DHernandez
            Date: 09/12/2019
            Desc: Validate Login
            Type: Post
            Params: objRequestParam<User>
            Response: Json Object Response
        */
        [HttpPost("Login")]
        public JsonResult Login([FromBody] TypedRequest<User> objRequestParam)
        {
            Connection objConnection = new Connection(Config.SQLPass,Config.Server,Config.DBName,Config.SQLUser);
            
            Response objResponse = new Response()
            {
                Resp = false,
                Type = "Error",
                Redirect = null
            };

            if (objRequestParam.Data is null) {
                throw new InvalidOperationException();
            }
            
            if (HttpContext.Session.GetString("CAPTCHA") != objRequestParam.Data.Captcha )
            {
                objResponse.Resp = false;

                objResponse.Msg = "Verifique el código captcha.";

                return Json(objResponse);
            }

            try
            {
                var objQueryParams = new
                {
                    Params = new string[] { "@ACCOUNT_NAME", "@DOC_ID", "@EMAIL" },
                    Values = Enumerable.Repeat(objRequestParam.Data.SAMAccountName, 3).ToArray()
                };

                var objDbResponse = objConnection.getRespFromQuery(0, 100, "", "SELECT ITC_USERID, VCDOC_NUMBER, SAMACCOUNTNAME, VCPASSWORD, VCSTATE FROM ITC_USER WHERE SAMACCOUNTNAME = @ACCOUNT_NAME or VCDOC_NUMBER=@DOC_ID OR VCEMAIL=@EMAIL", objQueryParams.Params, objQueryParams.Values, "NO_PAGINATE", "DataTable");

                if (objDbResponse.Resp) {

                    if (objDbResponse.Count == 0)
                    {
                        objResponse.Resp = false;
                        objResponse.Msg = "El usuario no se encuentra registrado.";
                        objResponse.Type = "Warning";
                    }
                    else
                    {
                        DataTable dtResult = objDbResponse.dtResult;

                        string strNit = dtResult.Rows[0]["VCDOC_NUMBER"].ToString();
                        objRequestParam.Data.SAMAccountName = dtResult.Rows[0]["SAMACCOUNTNAME"].ToString();
                        string strState = dtResult.Rows[0]["VCSTATE"].ToString();

                        if (strState == "I")
                        {
                            objResponse.Resp = false;
                            objResponse.Msg = "No se pudo iniciar sesión, el usuario se encuentra inhabilitado.";
                            objResponse.Type = "Warning";
                        }
                        else
                        {
                            bool blLogin = false;

                            Security objSecurity = new Security();

                            if (objSecurity.ValidateText(objRequestParam.Data.Password, dtResult.Rows[0]["VCPASSWORD"].ToString()))
                            {
                                blLogin = true;
                            }
                            else
                            {
                                blLogin = false;
                            }

                            if (blLogin)
                            {

                                objResponse.Type = "Success";

                                objResponse.Resp = true;

                                objResponse.Redirect = "Main";

                                objResponse.Msg = "Se ha iniciado sesion correctamente";

                                HttpContext.Session.SetString("CONSEC", "0");

                                HttpContext.Session.SetString("NIT", strNit);

                                HttpContext.Session.SetString("SAMACCOUNTNAME", objRequestParam.Data.SAMAccountName);
                            }
                            else
                            {
                                objResponse.Resp = false;
                                objResponse.Msg = "No se pudo iniciar sesión, usuario o contraseña incorrectas.";
                                objResponse.Type = "Warning";
                            }

                        }
                    }
                
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No fue posible conectarse a la base de datos.";
                    objResponse.Type = "Warning";
                }

            }
            catch(InvalidOperationException ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
            }

            return Json(objResponse);
        }

        /*
            Author: DHernandez
            Date: 01/12/2019
            Desc: Create user in DB
            Type: NA
            Params: User
            Response: Boolean true/false
        */
        private bool CreateUserDB(User newUser)
        {
            bool blResp = false;

            try
            {
                var objConnection = new Connection(Config.SQLPass,Config.Server,Config.DBName,Config.SQLUser);

                string strRolID = "";

                var objQueryParams = new
                {
                    Params = new string[] { "@ROL" },
                    Values = new string[] { "Cliente" },
                };

                var objDbResponse = objConnection.getRespFromQuery(0, 100, "","SELECT ITC_ROLEID FROM ITC_ROLE WHERE VCDESC = @ROL AND VCSTATE = 'A'", objQueryParams.Params, objQueryParams.Values, "NO_PAGINATE", "DataTable");

                if (objDbResponse.Count == 0)
                {
                    throw new InvalidOperationException("No se encuentra el rol registrado.");
                }
                else
                {
                    DataTable dtResult = objDbResponse.dtResult;
                    strRolID = dtResult.Rows[0]["ITC_ROLEID"].ToString();
                }

                if (!string.IsNullOrEmpty(newUser.Password))
                {
                    Security objSecurity = new Security();
                    newUser.Password = objSecurity.HashText(newUser.Password);
                }

                objQueryParams = new
                {
                    Params = new string[] { "@ACCOUNT_NAME", "@DOC_ID", "@EMAIL", "@Mobile", "@Region", "@BusinessName", "@ContactName", "@Rol", "@Password"},
                    Values = new string[] { newUser.SAMAccountName, newUser.DocumentNumber.ToString(), newUser.Email, newUser.Mobile, newUser.Region, newUser.BusinessName, newUser.ContactName, strRolID, newUser.Password }
                };

                string strQueryValues = "VALUES('A',";
                string strBaseQuery = "INSERT INTO ITC_USER(VCSTATE,";

                if (!string.IsNullOrEmpty(newUser.BusinessName))
                {
                    strBaseQuery += "VCBUSINESSNAME,";
                    strQueryValues += "@BusinessName,";
                }
                if (newUser.DocumentNumber != "0")
                {
                    strBaseQuery += "VCDOC_NUMBER,";
                    strQueryValues += "@DOC_ID,";
                }
                if (!string.IsNullOrEmpty(newUser.ContactName))
                {
                    strBaseQuery += "VCCONTACTNAME,";
                    strQueryValues += "@ContactName,";
                }
                if (!string.IsNullOrEmpty(newUser.Email))
                {
                    strBaseQuery += "VCEMAIL,";
                    strQueryValues += "@EMAIL,";
                }
                if (!string.IsNullOrEmpty(newUser.Mobile))
                {
                    strBaseQuery += "VCMOBILE,";
                    strQueryValues += "@Mobile,";
                }

                Guid guidRegion = Guid.Empty;

                if (Guid.TryParse(newUser.Region, out guidRegion))
                {
                    strBaseQuery += "ITC_REGIONID,";
                    strQueryValues += "@Region,";
                }
                if (!string.IsNullOrEmpty(strRolID))
                {
                    strBaseQuery += "ITC_ROLEID,";
                    strQueryValues += "@Rol,";
                }
                if (!string.IsNullOrEmpty(newUser.SAMAccountName))
                {
                    strBaseQuery += "SAMACCOUNTNAME,";
                    strQueryValues += "@ACCOUNT_NAME,";
                }

                if (!string.IsNullOrEmpty(newUser.Password))
                {
                    strBaseQuery += "VCPASSWORD)";
                    strQueryValues += "@Password)";
                }

                strBaseQuery += " OUTPUT INSERTED.ITC_USERID ";
                strBaseQuery += strQueryValues;

                ResponseDB objResponseDB = objConnection.InsData(strBaseQuery, objQueryParams.Params, objQueryParams.Values);

                string strId = objResponseDB.guidResult.ToString();

                if (objResponseDB.Resp && strId != "00000000-0000-0000-0000-000000000000")
                {
                    blResp = true;
                }
                else
                {
                    blResp = false;
                }
            }
            catch(Exception exc)
            {
                blResp = false;
            }

            return blResp;
        }

        /*
            Author: DHernandez
            Date: 20/12/2019
            Desc: Register a user in the DB
            Type: Post
            Params: objRequestParam<User>
            Response: Response
        */
        [HttpPost("RegisterUser")]
        public JsonResult RegisterUser([FromBody] TypedRequest<User> objRequestParam)
        {

            var objResponse = new Response()
            {
                Resp = false,
                Type = "Error",
                Redirect = null
            };

            if (objRequestParam.Data is null) { 
                throw new InvalidOperationException();
            }

            try
            {
                
                string strUserName = objRequestParam.Data.DocumentNumber;

                var newUser = new User
                {
                    SAMAccountName = strUserName,
                    DocumentNumber = objRequestParam.Data.DocumentNumber,
                    BusinessName = objRequestParam.Data.BusinessName,
                    Mobile = objRequestParam.Data.Mobile,
                    ContactName = objRequestParam.Data.ContactName,
                    Email = objRequestParam.Data.Email,
                    Password = objRequestParam.Data.Password,
                    Region = objRequestParam.Data.Region
                };

                bool blCreateUserDB = CreateUserDB(newUser);

                if(blCreateUserDB)
                {
                    objResponse.Redirect = "";
                    objResponse.Resp = true;
                    objResponse.Msg = "La cuenta fue registrada de forma exitosa.";
                    return Json(objResponse);
                }
                else
                {
                    throw new InvalidOperationException("No se pudo realizar la creación de la cuenta en la base de datos, por favor comuníquese con el administrador.");
                }
                
            }
            catch (InvalidOperationException ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
            }

            return Json(objResponse);
        }

        /*
            Author: DHernandez
            Date: 21/12/2019
            Desc: Get al the regions
            Type: Post
            Params: objRequestParam<null>
            Response: Response
        */
        [HttpPost("GetRegions")]
        public JsonResult GetRegions([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response()
            {
                Type = "Success",
                Resp = false,
                Msg = ""
            };

            try
            {
                Connection objConnection = new Connection(Config.SQLPass, Config.Server, Config.DBName, Config.SQLUser);

                ResponseDB dbResponse = objConnection.getRespFromQuery(0, 1500, "SELECT ITC_REGIONID AS ID, VCNOMDPTO AS Department, VCNOMMUNICIPIO AS City ", "FROM ITC_REGION", null, null, "VCNOMDPTO ASC, VCNOMMUNICIPIO ASC", "DataTable");

                if (dbResponse.Resp){

                    if (dbResponse.Count > 0) { 
                        objResponse.Data = JsonConvert.SerializeObject(dbResponse.dtResult, Formatting.None);
                        objResponse.Resp = true;
                        objResponse.Type = "Success";
                    }
                    else
                    {

                        dbResponse = objConnection.getRespFromQuery(0, 1500, "SELECT ITC_REGIONID AS ID, VCNOMDPTO AS Department, VCNOMMUNICIPIO AS City ", "FROM ITC_REGION", null, null, "VCNOMDPTO ASC, VCNOMMUNICIPIO ASC", "DataTable");

                        if (dbResponse.Count > 0)
                        {
                            objResponse.Data = JsonConvert.SerializeObject(dbResponse.dtResult, Formatting.None);
                            objResponse.Data = JsonConvert.SerializeObject(dbResponse.dtResult, Formatting.None);
                            objResponse.Resp = true;
                            objResponse.Type = "Success";
                        }
                        else
                        {
                            objResponse.Type = "Error";
                            objResponse.Msg = "No se encontraron regiones disponibles.";
                            objResponse.Detail = dbResponse.Msg;
                        }
                    }
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No fue posible conectarse a la base de datos. "+ dbResponse.Msg;
                    objResponse.Type = "Warning";
                }
            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
            }

            return Json(objResponse);
        }

        /*
            Author: DHernandez
            Date: 22/12/2019
            Desc: Captcha generation and initial configuration loaded
            Type: Post
            Params: objRequestParam<null>
            Response: Response
        */
        [HttpPost("GenerateCaptcha")]
        public JsonResult GenerateCaptcha([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response();
            try
            {
                Config.UrlServiceAD = _configuration.GetValue<string>("GIRAppConfig:UrlServiceAD");
                Config.OUCreateAD = _configuration.GetValue<string>("GIRAppConfig:OUCreateAD");
                Config.DomainCreateAD = _configuration.GetValue<string>("GIRAppConfig:DomainCreateAD");
                Config.LocalPath = _configuration.GetValue<string>("GIRAppConfig:LocalPath");
                Config.UserSend = _configuration.GetValue<string>("GIRAppConfig:UserSend");
                Config.UserSendName = _configuration.GetValue<string>("GIRAppConfig:UserSendName");
                Config.UserReceiveName = _configuration.GetValue<string>("GIRAppConfig:UserReceiveName");
                Config.Port = _configuration.GetValue<int>("GIRAppConfig:Port");
                Config.Host = _configuration.GetValue<string>("GIRAppConfig:Host");
                Config.UserName = _configuration.GetValue<string>("GIRAppConfig:UserName");
                Config.Password = _configuration.GetValue<string>("GIRAppConfig:Password");

                Config.Register = _configuration.GetValue<string>("GIRAppConfig:Register");
                Config.Forgot = _configuration.GetValue<string>("GIRAppConfig:Forgot");

                //Database Param
                Config.Server = _configuration.GetValue<string>("GIRAppConfig:Server");
                Config.DBName = _configuration.GetValue<string>("GIRAppConfig:DBName");
                Config.SQLUser = _configuration.GetValue<string>("GIRAppConfig:SQLUser");
                Config.SQLPass = _configuration.GetValue<string>("GIRAppConfig:SQLPass");

                Random random = new Random();

                objResponse.Resp = true;

                objResponse.Type = "Success";

                Font font = new Font("Times New Roman", 12.0f);

                string strRandom = "" + random.Next(1000, 9999);

                HttpContext.Session.Clear();

                HttpContext.Session.SetString("CAPTCHA", strRandom);
                HttpContext.Session.SetString("SAMACCOUNTNAME","");
                HttpContext.Session.SetString("CONSEC", "");
                HttpContext.Session.SetString("NIT", "");

                objResponse.Data = "data:image/jpg;base64," + DrawText(strRandom, font, Color.Bisque, Color.White);

            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
            }

            return Json(objResponse);
        }

        /*
            Author: DHernandez
            Date: 23/12/2019
            Desc: Create user in the AD
            Type: Post
            Params: objRequestParam<User>
            Response: true/false
        */
        public static bool CreateUserAD(TypedRequest<User> objRequestParam)
        {
            var strUrlServiceAD = Config.UrlServiceAD;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("ITC_Authorization", "ITC_SecurityString*2019.");

                string strUserName = "GIRApp" + objRequestParam.Data.DocumentNumber.ToString();

                //Creamos el array con los datos del usuario y su respectivo mapeo al servicio del AD
                JArray arrayProperties = new JArray();
                arrayProperties.Add(new JObject(new JProperty("Name", "NOMBRE"), new JProperty("Value", objRequestParam.Data.ContactName.ToString())));
                arrayProperties.Add(new JObject(new JProperty("Name", "CORREO"), new JProperty("Value", objRequestParam.Data.Email.ToString())));
                arrayProperties.Add(new JObject(new JProperty("Name", "CEDULA"), new JProperty("Value", objRequestParam.Data.DocumentNumber.ToString())));
                arrayProperties.Add(new JObject(new JProperty("Name", "EMPRESA"), new JProperty("Value", objRequestParam.Data.BusinessName.ToString())));
                arrayProperties.Add(new JObject(new JProperty("Name", "REGIONAL"), new JProperty("Value", objRequestParam.Data.Region.ToString())));
                arrayProperties.Add(new JObject(new JProperty("Name", "CELULAR"), new JProperty("Value", objRequestParam.Data.Mobile.ToString())));
                arrayProperties.Add(new JObject(new JProperty("Name", "USERLOGON"), new JProperty("Value", strUserName + Config.DomainCreateAD)));

                JObject jsonData = new JObject();
                jsonData.Add(new JProperty("User", strUserName));
                jsonData.Add(new JProperty("Password", objRequestParam.Data.Password));
                jsonData.Add(new JProperty("OU", Config.OUCreateAD));
                jsonData.Add(new JProperty("Properties", arrayProperties));

                strUrlServiceAD += "api/ADCreate?strParam=" + JsonConvert.SerializeObject(jsonData);

                HttpResponseMessage httpResponse = httpClient.GetAsync(strUrlServiceAD).Result;
                httpResponse.EnsureSuccessStatusCode();

                string strJsonData = httpResponse.Content.ReadAsStringAsync().Result;
                strJsonData = strJsonData.Substring(1, strJsonData.Length - 2).Replace("\\\"", "\"");

                JObject jsonResult = JObject.Parse(strJsonData);

                if (jsonResult["Status"].ToString().ToUpper().Equals("TRUE"))
                {
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("No se pudo realizar la creación de la cuenta, "+ jsonResult["Result"].ToString());
                }
            }
        }


        /*
            Author: DHernandez
            Date: 27/12/2019
            Desc: Draw a custom text in an image
            Type: Post
            Params: Text, Font Type, Text Color, Background Color
            Response: string<base64>
        */
        public static string DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);

            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();

            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();

            drawing.Dispose();

            MemoryStream ms = new MemoryStream();

            img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

            return Convert.ToBase64String(ms.ToArray());

        }


        /*
            Author: DHernandez
            Date: 27/12/2019
            Desc: Generate a random code and send it by a smtp
            Type: Post
            Params: objRequestParam
            Response: Response
        */
        [HttpPost("ValidateEmail")]
        public JsonResult ValidateEmail([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response();

            try {

                var strCode = new Random().Next(0, 10) + "" + new Random().Next(0, 10) + "" + new Random().Next(0, 10) + "" + new Random().Next(0, 10);

                objResponse.Resp = false;

            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador";
                objResponse.Detail = ex.Message;
                objResponse.Data = "0";
            }

            return Json(objResponse);
        }

    }
}
