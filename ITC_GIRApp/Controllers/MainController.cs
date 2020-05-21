using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ITC_GIRApp.Models;
using ITC_DBConnection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITC_GIRApp.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            Response jsonResponseDepartamentos = new Response()
            {
                Resp = true
            };

            try
            {
                ViewBag.departamentos = GetDepartamentos();

                return View();

                /*
                if (HttpContext.Session.GetString("SAMACCOUNTNAME").Equals(""))
                {
                    return Error();
                }
                else
                {
                    return View();
                }
                */
            }
            catch (Exception)
            {
                return Error();
            }
        }

        public IActionResult Privacy()
        {
            return Redirect("../Privacy");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return Redirect("../");
        }

        /*
            Author: Carlos Caicedo
            Date: 19/05/2020
            Desc: 
            Type: Post
            Params: objRequestParam
            Response: Json Object Response
        */
        [HttpPost]
        public JsonResult GetProducto([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response()
            {
                Resp = true
            };

            try
            {
                //Conectar BD
                Connection objConnection = new Connection(Config.SQLPass, Config.Server, Config.DBName, Config.SQLUser);

                ResponseDB dbResponse = objConnection.getRespFromQuery(0, 500, "", "SELECT p.*, d.nombre AS nombreDepartamento FROM Producto p LEFT JOIN Departamento d ON p.departamentoId = d.departamentoId", null, null, "NO_PAGINATE", "DataTable");

                if (dbResponse.Count > 0)
                {
                    objResponse.Count = dbResponse.Count;
                    objResponse.Data = JsonConvert.SerializeObject(dbResponse.dtResult, Formatting.None);
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No se encontraron datos.";
                    objResponse.Data = JsonConvert.SerializeObject(Array.Empty<object>(), Formatting.None);
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
           Author: Carlos Caicedo
           Date: 19/05/2020
           Desc: 
           Type: Post
           Params: objRequestParam
           Response: Json Object Response
       */
        [HttpPost]
        public JsonResult CreateProducto([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response()
            {
                Resp = true
            };

            try
            {

                JObject objRequestParams = JObject.Parse(objRequestParam.Data);

                string strNombre = objRequestParams["Nombre"].ToString();
                string strTipo = objRequestParams["Tipo"].ToString();
                string strDescripcion = objRequestParams["Descripcion"].ToString();
                string strPeso = objRequestParams["Peso"].ToString();
                string strDepartamento = null;

                if (objRequestParams["departamentoId"] != null)
                {
                    strDepartamento = objRequestParams["departamentoId"].ToString();
                }

                //Conectar BD
                Connection objConnection = new Connection(Config.SQLPass, Config.Server, Config.DBName, Config.SQLUser);

                string strBaseQuery = "";
                string[] arrayValues = null;
                string[] arrayParam = null;

                if (strDepartamento == null)
                {
                    strBaseQuery += "INSERT INTO Producto(Nombre, Tipo, Descripcion, Peso) ";
                    strBaseQuery += "OUTPUT INSERTED.ProductoID ";
                    strBaseQuery += "VALUES(@P_NOMBRE,@P_TIPO,@P_DESCRIPCION,@P_PESO) ";
                    arrayValues = new string[4] { strNombre, strTipo, strDescripcion, strPeso };
                    arrayParam = new string[4] { "@P_NOMBRE", "@P_TIPO", "@P_DESCRIPCION", "@P_PESO" };
                }
                else
                {
                    strBaseQuery += "INSERT INTO Producto(Nombre, Tipo, Descripcion, Peso, departamentoId) ";
                    strBaseQuery += "OUTPUT INSERTED.ProductoID ";
                    strBaseQuery += "VALUES(@P_NOMBRE,@P_TIPO,@P_DESCRIPCION,@P_PESO,@P_DEPARTAMENTO) ";
                    arrayValues = new string[5] { strNombre, strTipo, strDescripcion, strPeso, strDepartamento };
                    arrayParam = new string[5] { "@P_NOMBRE", "@P_TIPO", "@P_DESCRIPCION", "@P_PESO", "@P_DEPARTAMENTO" };
                }

                ResponseDB objResponseDB = objConnection.InsData(strBaseQuery, arrayParam, arrayValues);

                if (objResponseDB.Count > 0)
                {
                    objResponse.Count = objResponseDB.Count;
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No se pudo insertar el registro.";
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
            Author: Carlos Caicedo
            Date: 19/05/2020
            Desc: 
            Type: Post
            Params: objRequestParam
            Response: Json Object Response
        */
        [HttpPost]
        public JsonResult RemoveProducto([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response()
            {
                Resp = true
            };

            try
            {

                JObject objRequestParams = JObject.Parse(objRequestParam.Data);

                string strProductoID = objRequestParams["ProductoID"].ToString();

                //Conectar BD
                Connection objConnection = new Connection(Config.SQLPass, Config.Server, Config.DBName, Config.SQLUser);

                string strBaseQuery = "";
                strBaseQuery += "DELETE FROM Producto ";
                strBaseQuery += "WHERE ProductoID = @P_ID";

                string[] arrayValues = new string[1] { strProductoID };
                string[] arrayParam = new string[1] { "@P_ID" };

                ResponseDB objResponseDB = objConnection.DelData(strBaseQuery, arrayParam, arrayValues);

                if (objResponseDB.Count > 0)
                {
                    objResponse.Count = objResponseDB.Count;
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No se pudo eliminar el registro.";
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
            Author: Carlos Caicedo
            Date: 19/05/2020
            Desc: 
            Type: Post
            Params: objRequestParam
            Response: Json Object Response
        */
        [HttpPost]
        public JsonResult UpdateProducto([FromBody] Request objRequestParam)
        {
            Response objResponse = new Response()
            {
                Resp = true
            };

            try
            {

                JObject objRequestParams = JObject.Parse(objRequestParam.Data);

                string strId = objRequestParams["PK"].ToString();
                string strNombre = objRequestParams["Nombre"].ToString();
                string strTipo = objRequestParams["Tipo"].ToString();
                string strDescripcion = objRequestParams["Descripcion"].ToString();
                string nuPeso = objRequestParams["Peso"].ToString();
                string strDepartamento = null;

                if (objRequestParams["departamentoId"] != null)
                {
                    strDepartamento = objRequestParams["departamentoId"].ToString();
                }

                //Conectar BD
                Connection objConnection = new Connection(Config.SQLPass, Config.Server, Config.DBName, Config.SQLUser);

                string[] arrayValues = null;
                string[] arrayParam = null;
                string strBaseQuery = "";

                if (strDepartamento == null)
                {
                    strBaseQuery += "UPDATE Producto ";
                    strBaseQuery += "SET Nombre = @P_Nombre , ";
                    strBaseQuery += "Tipo = @P_Tipo , ";
                    strBaseQuery += "Descripcion = @P_Descripcion , ";
                    strBaseQuery += "Peso = @P_Peso ";
                    strBaseQuery += "WHERE ProductoID = @P_ID";
                    arrayValues = new string[5] { strId, strNombre, strTipo, strDescripcion, nuPeso };
                    arrayParam = new string[5] { "@P_ID", "@P_Nombre", "@P_Tipo", "@P_Descripcion", "@P_Peso" };
                }
                else
                {
                    strBaseQuery += "UPDATE Producto ";
                    strBaseQuery += "SET Nombre = @P_Nombre , ";
                    strBaseQuery += "Tipo = @P_Tipo , ";
                    strBaseQuery += "Descripcion = @P_Descripcion , ";
                    strBaseQuery += "Peso = @P_Peso , ";
                    strBaseQuery += "departamentoId = @P_Departamento ";
                    strBaseQuery += "WHERE ProductoID = @P_ID";
                    arrayValues = new string[6] { strId, strNombre, strTipo, strDescripcion, nuPeso, strDepartamento };
                    arrayParam = new string[6] { "@P_ID", "@P_Nombre", "@P_Tipo", "@P_Descripcion", "@P_Peso", "@P_Departamento" };
                }

                ResponseDB objResponseDB = objConnection.UpdData(strBaseQuery, arrayParam, arrayValues);

                if (objResponseDB.Count > 0)
                {
                    objResponse.Count = objResponseDB.Count;
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No se pudo actualizar el registro.";
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
            Author: Carlos Caicedo
            Date: 19/05/2020
            Desc: 
            Type: Get
            Response: Json Object Response
        */
        [HttpGet]
        public JsonResult GetDepartamentos()
        {
            Response objResponse = new Response()
            {
                Resp = true
            };

            try
            {
                //Conectar BD
                Connection objConnection = new Connection(Config.SQLPass, Config.Server, Config.DBName, Config.SQLUser);

                ResponseDB dbResponse = objConnection.getRespFromQuery(0, 500, "", "SELECT * FROM Departamento", null, null, "NO_PAGINATE", "DataTable");

                if (dbResponse.Count > 0)
                {
                    objResponse.Count = dbResponse.Count;
                    objResponse.Data = JsonConvert.SerializeObject(dbResponse.dtResult, Formatting.None);
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No se encontraron datos.";
                    objResponse.Data = JsonConvert.SerializeObject(Array.Empty<object>(), Formatting.None);
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
    }
}
