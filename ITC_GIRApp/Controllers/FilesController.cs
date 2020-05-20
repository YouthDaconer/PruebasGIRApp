using Microsoft.AspNetCore.Mvc;
using ITC_GIRApp.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Linq;
using ITC_DBConnection;
using System;
using System.Linq;
using OfficeOpenXml;

namespace ITC_GIRApp.Controllers
{
    public class FilesController : Controller
    {
        private readonly IFileProvider fileProvider;

        public FilesController(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }

        /*
            Author: DHernandez
            Date: 20/12/2019
            Desc: Upload a client file to the server
            Type: Post
            Params: IFormFile (Only the first is processed)
            Response: IActionResult
        */
        [HttpPost]
        public async Task<IActionResult> UploadClient(List<IFormFile> files)
        {

            Response objResponse = new Response();

            try
            {
                int nuValidCol = 0;
                int rowCount = 0;
                int ColCount = 0;

                string strNIT = "";
                string strSAN = "";

                try
                {
                    strSAN = HttpContext.Session.GetString("SAMACCOUNTNAME").ToString();
                    strNIT = HttpContext.Session.GetString("NIT").ToString();
                }
                catch(Exception)
                {

                }

                if (strSAN.Equals(""))
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "Tu sesión ha caducado, por favor ingresa nuevamente a la aplicación.";
                    objResponse.Type = "Session";
                }else if (strNIT.Equals(""))
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No fue posible crear una carpeta ya que no tienes asociado un NIT.";
                    objResponse.Type = "Warning";
                }
                else if (files == null)
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No fue posible leer el contenido del archivo";
                    objResponse.Type = "Error";
                }
                else
                {

                    string strFileExt = files[0].FileName.Substring(files[0].FileName.Length-4);

                    if (strFileExt.ToUpper().Equals("XLSX"))
                    {
                        strFileExt = "."+strFileExt;
                    }

                    DateTime dtDate = DateTime.Now;

                    int nuConsec = int.Parse(HttpContext.Session.GetString("CONSEC").ToString());

                    string strFileName = strNIT + @"\" +dtDate.ToString("yyyyMMdd") +"-"+ nuConsec + "-"+ files[0].FileName;
                    System.IO.Directory.CreateDirectory(@Config.LocalPath+ @"ClientUploads\"+ strNIT);
                    nuConsec++;

                    HttpContext.Session.SetString("CONSEC", "" + nuConsec);

                    string path = @Config.LocalPath + @"ClientUploads\" + strFileName;

                    objResponse.Menu = path;

                    List<string> lstCol = new List<string>()
                    {
                        "IDENTIFICADOR",
                        "FECHA_INGRESO",
                        "FECHA_FIN",
                        "ESTADO_CONTRATO",
                        "COD_DIAGNOSTICO",
                        "CONTINGENCIA",
                        "DIAS_INCAPACIDAD",
                        "FECHA_INICIO_INCAPACIDAD",
                        "FECHA_FIN_INCAPACIDAD",
                        "DESCRIPCION_DIAGNOSTICO",
                        "FECHA_NACIMIENTO",
                        "GENERO",
                        "ZONA",
                        "CARGO"
                    };

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await files[0].CopyToAsync(stream);

                        if (strFileExt.ToUpper().Equals(".CSV"))
                        {
                            string strFileHeader = "";
                            string strFileContent = "";

                            using (var reader = new StreamReader(files[0].OpenReadStream()))
                            {
                                while (!reader.EndOfStream) {

                                    if (rowCount == 0)
                                    {
                                        strFileHeader = reader.ReadLine();
                                    }
                                    else
                                    {
                                        strFileContent = reader.ReadLine();
                                    }

                                    rowCount++;
                                }

                                string[] arrayCols = strFileHeader.Split(";");

                                ColCount = arrayCols.Length;

                                List<string> lstFileCol = new List<string>();

                                for (int i = 0; i < arrayCols.Length; i++)
                                {
                                    string strCol = arrayCols[i].ToUpper();

                                    if (lstCol.IndexOf(strCol) != -1)
                                    {
                                        nuValidCol++;
                                    }
                                }

                                objResponse.Count = rowCount;
                                objResponse.Index = ColCount;
                                objResponse.Data = strFileName;

                            }

                        }

                        if (strFileExt.ToUpper().Equals(".XLSX"))
                        {
                            #region xlsx
                            using (ExcelPackage package = new ExcelPackage(stream))
                            {
                                ExcelWorksheet worksheet = null;

                                bool blSheet = false;

                                if (package.Workbook.Worksheets["Cliente"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Cliente"];
                                    blSheet = true;
                                }
                                if (package.Workbook.Worksheets["Client"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Client"];
                                    blSheet = true;
                                }

                                if (package.Workbook.Worksheets["Resultado"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Resultado"];
                                    blSheet = true;
                                }
                                if (package.Workbook.Worksheets["Result"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Result"];
                                    blSheet = true;
                                }

                                if (package.Workbook.Worksheets["Sheet1"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Sheet1"];
                                    blSheet = true;
                                }
                                if (package.Workbook.Worksheets["Hoja1"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Hoja1"];
                                    blSheet = true;
                                }

                                if (blSheet)
                                {
                                    rowCount = worksheet.Dimension.Rows;
                                    ColCount = worksheet.Dimension.Columns;

                                    List<string> lstFileCol = new List<string>();

                                    for (int col = 1; col <= ColCount; col++)
                                    {
                                        string strCol = "";
                                        if (worksheet.Cells[1, col].Value != null)
                                        {
                                            strCol = worksheet.Cells[1, col].Value.ToString().ToUpper();
                                        }

                                        if (lstCol.IndexOf(strCol) != -1)
                                        {
                                            nuValidCol++;
                                        }
                                    }

                                    objResponse.Count = rowCount;
                                    objResponse.Index = ColCount;
                                    objResponse.Data = strFileName;

                                }
                                else
                                {
                                    objResponse.Resp = false;
                                    objResponse.Msg = "No fue posible leer la hoja de excel.";
                                }
                            }
                            #endregion
                        }

                        if (nuValidCol >= 13)
                        {
                            objResponse.Resp = true;
                            objResponse.Count = rowCount - 1;
                            objResponse.Index = ColCount;
                            objResponse.Data = strFileName;
                        }
                        else
                        {
                            objResponse.Resp = false;
                            objResponse.Msg = "El archivo no cumple con el formato valido. "+ nuValidCol + " Columnas Validas";
                            objResponse.Count = 0;
                        }
                    }

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
            Date: 23/12/2019
            Desc: Upload a result file to the server
            Type: Post
            Params: IFormFile (Only the first is processed)
            Response: IActionResult
        */
        [HttpPost]
        public async Task<IActionResult> UploadResult(List<IFormFile> files)
        {

            string strUser = "";

            Response objResponse = new Response();

            try
            {
                strUser = HttpContext.Session.GetString("ITC_USERID").ToString();
            }
            catch (Exception ex)
            {
                strUser = "";
            }

            try
            {

                int nuValidCol = 0;
                int rowCount = 0;
                int ColCount = 0;

                if (strUser == "")
                {
                    objResponse.Resp = false;
                    objResponse.Type = "Session";
                }
                else if (files == null)
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No fue posible leer el contenido del archivo";
                }
                else
                {
                    Guid guidFile = Guid.NewGuid();

                    string strFileExt = files[0].FileName.Substring(files[0].FileName.Length - 4);

                    if (strFileExt.ToUpper().Equals("XLSX"))
                    {
                        strFileExt = "." + strFileExt;
                    }

                    string strFileName = guidFile.ToString() + "." + files[0].FileName.Substring(files[0].FileName.Length - 4).Replace(".", "");

                    string path = @Config.LocalPath + @"ResultUploads\" + strFileName;

                    using (var stream = new FileStream(path, FileMode.Create))
                    {

                        await files[0].CopyToAsync(stream);

                        List<string> lstCol = new List<string>(new string[] { "FECHA_ANALISIS", "IDENTIFICADOR", "NOMBRE_COMPLETO", "GENERO", "EDAD", "COD_CLIENTE", "EMPRESA", "NEGOCIO", "ZONA", "CARGO", "ELR", "ANTIGUEDAD_DIAS", "ANTIGUEDAD_ANOS", "CRITICO", "DIAGNOSTICO_1", "DIAGNOSTICO_2", "DIAGNOSTICO_3", "DIAGNOSTICO_4", "DIAGNOSTICO_5", "DIAGNOSTICO_6", "DIAGNOSTICO_7", "DIAGNOSTICO_8", "DIAGNOSTICO_9", "DIAGNOSTICO_10", "DIAGNOSTICO_11", "DIAGNOSTICO_12", "DIAGNOSTICO_13", "FRECUENCIA_1", "FRECUENCIA_2", "FRECUENCIA_3", "FRECUENCIA_4", "FRECUENCIA_5", "FRECUENCIA_6", "FRECUENCIA_7", "FRECUENCIA_8", "FRECUENCIA_9", "FRECUENCIA_10", "FRECUENCIA_11", "FRECUENCIA_12", "FRECUENCIA_13", "DIAS_1", "DIAS_2", "DIAS_3", "DIAS_4", "DIAS_5", "DIAS_6", "DIAS_7", "DIAS_8", "DIAS_9", "DIAS_10", "DIAS_11", "DIAS_12", "DIAS_13", "AT_1", "AT_2", "AT_3", "AT_4", "AT_5", "AT_6", "AT_7", "AT_8", "AT_9", "AT_10", "AT_11", "AT_12", "AT_13", "PRONOSTICO", "CLASIFICACION", "PRONOSTICO_DOCTOR", "RIESGO_REINTEGRO", "EXPECTATIVA_DE_MEJORA", "RECOMENDACIONES", "COL_AD1", "COL_AD2", "COL_AD3", "COL_AD4", "COL_AD5", "COL_AD6", "COL_AD7", "COL_AD8", "COL_AD9", "COL_AD10" });

                        #region CSV
                        if (strFileExt.ToUpper().Equals(".CSV"))
                        {
                            string strFileHeader = "";
                            string strFileContent = "";

                            using (var reader = new StreamReader(files[0].OpenReadStream()))
                            {
                                while (!reader.EndOfStream)
                                {

                                    if (rowCount == 0)
                                    {
                                        strFileHeader = reader.ReadLine();
                                        strFileHeader = strFileHeader.ToUpper().Replace(" ","_").Replace("Ñ","N");
                                    }
                                    else
                                    {
                                        strFileContent = reader.ReadLine();
                                    }

                                    rowCount++;
                                }

                                string[] arrayCols = strFileHeader.Split(";");

                                ColCount = arrayCols.Length;

                                List<string> lstFileCol = new List<string>();

                                for (int i = 0; i < arrayCols.Length; i++)
                                {
                                    string strCol = arrayCols[i].ToUpper();

                                    if (lstCol.IndexOf(strCol) != -1)
                                    {
                                        nuValidCol++;
                                    }
                                }

                            }

                        }
                        #endregion

                        #region XLSX
                        if (strFileExt.ToUpper().Equals(".XLSX"))
                        {
                            using (ExcelPackage package = new ExcelPackage(stream))
                            {
                                ExcelWorksheet worksheet = null;

                                bool blSheet = false;

                                if (package.Workbook.Worksheets["Cliente"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Cliente"];
                                    blSheet = true;
                                }

                                if (package.Workbook.Worksheets["Sheet1"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Sheet1"];
                                    blSheet = true;
                                }

                                if (package.Workbook.Worksheets["Hoja1"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Hoja1"];
                                    blSheet = true;
                                }

                                if (package.Workbook.Worksheets["Result"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Result"];
                                    blSheet = true;
                                }

                                if (package.Workbook.Worksheets["Resultado"] != null)
                                {
                                    worksheet = package.Workbook.Worksheets["Resultado"];
                                    blSheet = true;
                                }

                                if (blSheet)
                                {
                                    rowCount = worksheet.Dimension.Rows;
                                    ColCount = worksheet.Dimension.Columns;

                                    List<string> lstFileCol = new List<string>();

                                    for (int col = 1; col <= ColCount; col++)
                                    {
                                        string strCol = "";

                                        if (worksheet.Cells[1, col].Value != null)
                                        {
                                            strCol = worksheet.Cells[1, col].Value.ToString();
                                        }

                                        strCol = strCol.ToUpper().Replace(" ", "_").Replace("Ñ", "N");

                                        if (lstCol.IndexOf(strCol) != -1)
                                        {
                                            nuValidCol++;
                                        }
                                    }

                                }
                                else
                                {
                                    objResponse.Resp = false;
                                    objResponse.Msg = "No fue posible leer la hoja de excel.";
                                }

                            }
                        }

                        #endregion

                        if (nuValidCol == 82)
                        {
                            objResponse.Resp = true;
                            objResponse.Count = rowCount - 1;
                            objResponse.Index = ColCount;
                            objResponse.Data = strFileName;
                        }
                        else
                        {
                            objResponse.Resp = false;
                            objResponse.Msg = "El archivo no cumple con el formato valido. "+ nuValidCol + " Columnas Validas";
                            objResponse.Count = 0;
                        }

                    }

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
            Date: 27/12/2019
            Desc: Download a file located in  @Config.LocalPath of a specific client
            Type: Get
            Params: strFileName
            Response: IActionResult
        */
        public async Task<IActionResult> DownloadAsync(string strFileName)
        {

            string path = @Config.LocalPath + @"ClientUploads\" + strFileName;

            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return File(memory, GetMimeTypes()[ext], Path.GetFileName(path));

        }

        /*
            Author: DHernandez
            Date: 23/12/2019
            Desc: Dictionary with the mimeTypes allowed
            Type: Dictionary
            Params: None
            Response: Dictionary<string, string>
        */
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        /*
            Author: DHernandez
            Date: 26/12/2019
            Desc: Read and upload line by line the client file to the db, aditionally calculate the completeness % of the upload file according to the logic rules.
            Type: Post
            Params: Request
            Response: JsonResult
        */
        [HttpPost]
        public JsonResult UploadFileResume([FromBody] Request objRequestParam)
        {
            JObject objRequestParams = JObject.Parse(objRequestParam.Data);

            Response objResponse = new Response();

            string strUserId = "";

            try
            {
                strUserId = HttpContext.Session.GetString("ITC_USERID").ToString();
            }
            catch (Exception)
            {
                strUserId = "";
            }

            try
            {
                if (!strUserId.Equals(""))
                {

                    try
                    {

                        strUserId = objRequestParams["UserId"].ToString();
                        string strDescription = objRequestParams["Description"].ToString();
                        string strFileName = objRequestParams["FileName"].ToString();
                        string strRows = objRequestParams["Rows"].ToString();
                        string strDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        List<string> lstColFile = new List<string>();
                        List<int> lstValComp = new List<int>(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                        List<string> lstColComp = new List<string>(new string[] { "IDENTIFICADOR", "FECHA_INGRESO", "FECHA_FIN", "ESTADO_CONTRATO", "COD_DIAGNOSTICO", "FECHA_INICIO_INCAPACIDAD", "FECHA_FIN_INCAPACIDAD", "FECHA_NACIMIENTO", "GENERO", "ZONA", "CARGO" });
                        List<string> lstCol = new List<string>(new string[] { "CARGO", "ZONA", "COD_DIAGNOSTICO", "NOMBRE_COMPLETO", "COL_AD1", "COL_AD2", "COL_AD3", "COL_AD4", "COL_AD5", "COL_AD6", "COL_AD7", "COL_AD8", "COL_AD9", "COL_AD10", "CONCEPTO_REHABILITACION", "CONTINGENCIA", "DESCRIPCION_DIAGNOSTICO", "DIAS_INCAPACIDAD", "ELR", "ESTADO_CONTRATO", "FECHA_FIN", "FECHA_FIN_INCAPACIDAD", "FECHA_INGRESO", "FECHA_INICIO_INCAPACIDAD", "FECHA_NACIMIENTO", "GENERO", "IDENTIFICADOR", "PCL","EMPRESA","NEGOCIO","LINEA_SERVICIO"});
                        List<List<string>> lstRows = new List<List<string>>();

                        if (strUserId.Equals(HttpContext.Session.GetString("ITC_USERID")))
                        {
                            string[] arrayRows = new string[7] { strDate, strFileName, strRows, "", "A", strUserId, strDescription };

                            Connection objConnection = new Connection(Config.SQLPass,Config.Server,Config.DBName,Config.SQLUser);

                            string strBaseQuery = "INSERT INTO ITC_CLIENTDATA(VCCREATEDON,VCPATH,VCCOUNT,VCCOMPLETENESS,VCSTATE,ITC_USERID,VCDESCRIPTION) ";
                            strBaseQuery += "OUTPUT INSERTED.ITC_CLIENTDATAID ";
                            strBaseQuery += "VALUES(@VCCREATEDON,@VCPATH,@VCCOUNT,@VCCOMPLETENESS,@VCSTATE,@ITC_USERID,@VCDESCRIPTION)";

                            string[] arrayParam = new string[7] { "@VCCREATEDON", "@VCPATH", "@VCCOUNT", "@VCCOMPLETENESS", "@VCSTATE", "@ITC_USERID", "@VCDESCRIPTION" };

                            ResponseDB objResponseDB = objConnection.InsData(strBaseQuery, arrayParam, arrayRows);

                            if (objResponseDB.guidResult == Guid.Empty)
                            {
                                objResponse.Count = 0;
                                objResponse.Resp = false;
                                objResponse.Type = "Error";
                                objResponse.Msg = "No fue posible crear el registro.";
                            }
                            else
                            {
                                objResponse.Id = objResponseDB.guidResult.ToString();
                                string ITC_CLIENTDATAID = objResponseDB.guidResult.ToString();

                                int rowCount = 0;
                                int ColCount = 0;
                                int nuValidCol = 0;

                                string strQueryCol = "ITC_CLIENTDATAID";
                                string strColParam = "@ITC_CLIENTDATAID";

                                string strFileExt = strFileName.Substring(strFileName.Length - 4);

                                if (strFileExt.ToUpper().Equals("XLSX"))
                                {
                                    strFileExt = "." + strFileExt;
                                }

                                string path = @Config.LocalPath+ @"ClientUploads\" + strFileName;

                                using (var stream = new FileStream(path, FileMode.Open))
                                {
                                    #region CSV
                                    if (strFileExt.ToUpper().Equals(".CSV"))
                                    {

                                        using (var reader = new StreamReader(stream))
                                        {
                                            bool blHeader = true;

                                            string strFileHeader = "";
                                            string strFileContent = "";

                                            List<string> lstHeaderFileTmp = new List<string>();
                                            List<string> lstColDataTmp = new List<string>();
                                            List<string> lstColDataAux = new List<string>();

                                            while (!reader.EndOfStream)
                                            {

                                                if (blHeader)
                                                {
                                                    strFileHeader = reader.ReadLine();
                                                    lstHeaderFileTmp = strFileHeader.Split(";").ToList();

                                                    for (int i = 0; i < lstHeaderFileTmp.Count; i++)
                                                    {
                                                        string strCol = lstHeaderFileTmp[i].ToUpper();

                                                        if (lstCol.IndexOf(strCol) != -1)
                                                        {
                                                            lstColFile.Add(lstHeaderFileTmp[i]);
                                                            strQueryCol += ", " + lstHeaderFileTmp[i];
                                                            strColParam += ", @" + lstHeaderFileTmp[i];
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    strFileContent = reader.ReadLine();

                                                    if (strFileContent.Trim() != "")
                                                    {
                                                        lstColDataTmp = strFileContent.Split(";").ToList();
                                                        lstColDataAux = new List<string>();

                                                        if ( lstColDataTmp.Count == lstHeaderFileTmp.Count )
                                                        {
                                                            for (int i = 0; i < lstColDataTmp.Count; i++)
                                                            {
                                                                string strCol = lstHeaderFileTmp[i].ToUpper();

                                                                if (lstCol.IndexOf(strCol) != -1)
                                                                {
                                                                    lstColDataAux.Add(lstColDataTmp[i]);

                                                                    if (lstColComp.IndexOf(strCol) != -1)
                                                                    {
                                                                        if (lstColDataTmp[i].Equals(""))
                                                                        {
                                                                            lstValComp[lstColComp.IndexOf(strCol)]++;
                                                                        }
                                                                    }
                                                                }

                                                            }

                                                            lstRows.Add(lstColDataAux);
                                                        }
                                                    }
                                                }

                                                blHeader = false;
                                            }

                                        }

                                    }
                                    #endregion

                                    #region XLSX
                                    if (strFileExt.ToUpper().Equals(".XLSX"))
                                    {
                                        using (ExcelPackage package = new ExcelPackage(stream))
                                        {
                                            ExcelWorksheet worksheet = null;

                                            if (package.Workbook.Worksheets["Hoja1"] != null)
                                            {
                                                worksheet = package.Workbook.Worksheets["Hoja1"];
                                            }

                                            if (package.Workbook.Worksheets["Sheet1"] != null)
                                            {
                                                worksheet = package.Workbook.Worksheets["Sheet1"];
                                            }

                                            if (package.Workbook.Worksheets["Cliente"] != null)
                                            {
                                                worksheet = package.Workbook.Worksheets["Cliente"];
                                            }

                                            if (package.Workbook.Worksheets["Client"] != null)
                                            {
                                                worksheet = package.Workbook.Worksheets["Client"];
                                            }

                                            rowCount = worksheet.Dimension.Rows;
                                            ColCount = worksheet.Dimension.Columns;

                                            List<string> lstHeaderFileTmp = new List<string>();
                                            List<string> lstColDataTmp = new List<string>();
                                            List<string> lstColDataAux = new List<string>();

                                            for (int col = 1; col <= ColCount; col++)
                                            {
                                                string strCol = "";

                                                if (worksheet.Cells[1, col].Value != null)
                                                {
                                                    strCol = worksheet.Cells[1, col].Value.ToString().ToUpper();
                                                }

                                                if (lstCol.IndexOf(strCol) != -1)
                                                {
                                                    nuValidCol++;

                                                    lstColFile.Add(strCol);

                                                    strQueryCol += ", " + strCol;
                                                    strColParam += ", @" + strCol;
                                                }

                                                lstHeaderFileTmp.Add(strCol);
                                            }

                                            for (int row = 2; row <= rowCount; row++)
                                            {
                                                List<string> lstRow = new List<string>();

                                                for (int col = 1; col <= ColCount; col++)
                                                {
                                                    if (lstCol.IndexOf(lstHeaderFileTmp[col-1]) != -1)
                                                    {
                                                        if (worksheet.Cells[row, col].Value != null)
                                                        {
                                                            lstRow.Add(worksheet.Cells[row, col].Value.ToString());
                                                        }
                                                        else
                                                        {
                                                            lstRow.Add("");
                                                        }

                                                        if (lstColComp.IndexOf(lstHeaderFileTmp[col-1]) != -1)
                                                        {
                                                            if(worksheet.Cells[row, col].Value == null || worksheet.Cells[row, col].Value.ToString().Equals("")) {
                                                                lstValComp[lstColComp.IndexOf(lstHeaderFileTmp[col-1])]++;
                                                            }
                                                        }

                                                    }
                                                }

                                                lstRows.Add(lstRow);

                                            }
                                        }
                                    }

                                    #endregion

                                    strBaseQuery = "INSERT INTO ITC_CLIENTDETAILDATA(" + strQueryCol + ") ";
                                    strBaseQuery += "VALUES(" + strColParam + ")";

                                    objResponseDB = objConnection.InsMultipleData(strBaseQuery, lstColFile, lstRows, ITC_CLIENTDATAID, "@ITC_CLIENTDATAID");

                                    objResponse.Resp = objResponseDB.Resp;
                                    objResponse.Count = objResponseDB.Count;
                                    objResponse.Index = ColCount;
                                    objResponse.Data = strFileName;

                                    if (objResponseDB.Count > 0)
                                    {
                                        double dbPerc = 0.0909;
                                        double dbCompleteness = 0;

                                        for (int i = 0; i < lstValComp.Count; i++)
                                        {
                                            double dcTmp = (double)lstValComp[i] / (double)objResponseDB.Count;
                                            dcTmp = dcTmp * dbPerc;
                                            dbCompleteness += dcTmp;
                                        }

                                        dbCompleteness = Math.Round((1 - dbCompleteness)*100,2);

                                        strBaseQuery = "UPDATE ITC_CLIENTDATA ";
                                        strBaseQuery += "SET VCCOMPLETENESS = @VCCOMPLETENESS ";
                                        strBaseQuery += "WHERE ITC_CLIENTDATAID = @ITC_CLIENTDATAID ";
                                        arrayParam = new string[2] { "@ITC_CLIENTDATAID", "@VCCOMPLETENESS"};

                                        string[] arrayValue = new string[2] { ITC_CLIENTDATAID, ""+dbCompleteness };

                                        objResponseDB = objConnection.UpdData(strBaseQuery, arrayParam, arrayValue);

                                        objResponse.Type = "Success";
                                    }
                                    else
                                    {
                                        objResponse.Type = "Error";
                                    }

                                }

                                objResponse.Resp = true;
                            }

                        }
                        else
                        {
                            objResponse.Resp = false;
                            objResponse.Msg = "No existe una sesión activa para validar el proceso.";
                            objResponse.Type = "Warning";
                        }

                    }
                    catch (Exception ex)
                    {
                        objResponse.Resp = false;
                        objResponse.Msg = "Ocurrió un error inesperado";
                        objResponse.Type = ex.Message;
                    }

                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No existe una sesión activa para validar el proceso.";
                    objResponse.Type = "Session";
                }
            }
            catch (Exception ex)
            {
                objResponse.Resp = false;
                objResponse.Type = "Error";
                objResponse.Msg = "Ocurrió un error inesperado, por favor consulte al administrador.";
                objResponse.Detail = ex.Message;
            }

            return Json(objResponse);
        }


        /*
            Author: DHernandez
            Date: 27/12/2019
            Desc: Read and upload line by line the result file to the db
            Type: Post
            Params: Request
            Response: JsonResult
        */
        [HttpPost]
        public JsonResult UploadFileResult([FromBody] Request objRequestParam)
        {

            string strUserId = "";

            Response objResponse = new Response();

            try
            {
                strUserId = HttpContext.Session.GetString("ITC_USERID").ToString();
            }
            catch (Exception)
            {
                strUserId = "";
            }

            try
            {
                if (!strUserId.Equals("")){

                    JObject objRequestParams = JObject.Parse(objRequestParam.Data);

                    strUserId = objRequestParams["UserId"].ToString();
                    string strFileName = objRequestParams["FileName"].ToString();
                    string strClientDataId = objRequestParams["ClientDataId"].ToString();

                    if (strUserId.Equals(HttpContext.Session.GetString("ITC_USERID")))
                    {

                        Connection objConnection = new Connection(Config.SQLPass,Config.Server,Config.DBName,Config.SQLUser);

                        string path = @Config.LocalPath + @"ResultUploads\" + strFileName;

                        int rowCount = 0;
                        int ColCount = 0;

                        string strQueryCol = "ITC_CLIENTDATAID";
                        string strColParam = "@ITC_CLIENTDATAID";

                        List<string> lstColFile = new List<string>();
                        List<string> lstCol = new List<string>(new string[] { "FECHA_ANALISIS", "IDENTIFICADOR", "NOMBRE_COMPLETO", "GENERO", "EDAD", "COD_CLIENTE", "EMPRESA", "NEGOCIO", "ZONA", "CARGO", "ELR", "ANTIGUEDAD_DIAS", "ANTIGUEDAD_ANOS", "CRITICO", "DIAGNOSTICO_1", "DIAGNOSTICO_2", "DIAGNOSTICO_3", "DIAGNOSTICO_4", "DIAGNOSTICO_5", "DIAGNOSTICO_6", "DIAGNOSTICO_7", "DIAGNOSTICO_8", "DIAGNOSTICO_9", "DIAGNOSTICO_10", "DIAGNOSTICO_11", "DIAGNOSTICO_12", "DIAGNOSTICO_13", "FRECUENCIA_1", "FRECUENCIA_2", "FRECUENCIA_3", "FRECUENCIA_4", "FRECUENCIA_5", "FRECUENCIA_6", "FRECUENCIA_7", "FRECUENCIA_8", "FRECUENCIA_9", "FRECUENCIA_10", "FRECUENCIA_11", "FRECUENCIA_12", "FRECUENCIA_13", "DIAS_1", "DIAS_2", "DIAS_3", "DIAS_4", "DIAS_5", "DIAS_6", "DIAS_7", "DIAS_8", "DIAS_9", "DIAS_10", "DIAS_11", "DIAS_12", "DIAS_13", "AT_1", "AT_2", "AT_3", "AT_4", "AT_5", "AT_6", "AT_7", "AT_8", "AT_9", "AT_10", "AT_11", "AT_12", "AT_13", "PRONOSTICO", "CLASIFICACION", "PRONOSTICO_DOCTOR", "RIESGO_REINTEGRO", "EXPECTATIVA_DE_MEJORA", "RECOMENDACIONES", "COL_AD1", "COL_AD2", "COL_AD3", "COL_AD4", "COL_AD5", "COL_AD6", "COL_AD7", "COL_AD8", "COL_AD9", "COL_AD10" });
                        List<List<string>> lstRows = new List<List<string>>();
                        int nuValidCol = 0;

                        string strFileExt = strFileName.Substring(strFileName.Length - 4);

                        if (strFileExt.ToUpper().Equals("XLSX"))
                        {
                            strFileExt = "." + strFileExt;
                        }

                        using (var stream = new FileStream(path, FileMode.Open))
                        {

                            if (strFileExt.ToUpper().Equals(".CSV"))
                            {

                                using (var reader = new StreamReader(stream))
                                {

                                    string strFileHeader = "";
                                    string strFileContent = "";

                                    while (!reader.EndOfStream)
                                    {
                                        if (rowCount == 0)
                                        {
                                            strFileHeader = reader.ReadLine();
                                            strFileHeader = strFileHeader.ToUpper().Replace(" ", "_").Replace("Ñ", "N");
                                        }
                                        else
                                        {
                                            strFileContent = reader.ReadLine();
                                            lstRows.Add(strFileContent.Split(";").ToList());
                                        }

                                        rowCount++;
                                    }

                                    lstColFile = strFileHeader.Split(";").ToList();

                                    for(int i=0; i < lstColFile.Count; i++)
                                    {
                                        strQueryCol += ", " + lstColFile[i];
                                        strColParam += ", @" + lstColFile[i];
                                    }

                                }

                            }

                            if (strFileExt.ToUpper().Equals(".XLSX"))
                            {
                                using (ExcelPackage package = new ExcelPackage(stream))
                                {
                                    ExcelWorksheet worksheet = null;

                                    string strTest = package.Workbook.Worksheets.ToString();

                                    if (package.Workbook.Worksheets["Hoja1"] != null)
                                    {
                                        worksheet = package.Workbook.Worksheets["Hoja1"];
                                    }

                                    if (package.Workbook.Worksheets["Sheet1"] != null)
                                    {
                                        worksheet = package.Workbook.Worksheets["Sheet1"];
                                    }

                                    if (package.Workbook.Worksheets["Client"] != null)
                                    {
                                        worksheet = package.Workbook.Worksheets["Client"];
                                    }
                                    if (package.Workbook.Worksheets["Cliente"] != null)
                                    {
                                        worksheet = package.Workbook.Worksheets["Cliente"];
                                    }

                                    if (package.Workbook.Worksheets["Resultado"] != null)
                                    {
                                        worksheet = package.Workbook.Worksheets["Resultado"];
                                    }
                                    if (package.Workbook.Worksheets["Result"] != null)
                                    {
                                        worksheet = package.Workbook.Worksheets["Result"];
                                    }

                                    rowCount = worksheet.Dimension.Rows;
                                    ColCount = worksheet.Dimension.Columns;

                                    for (int col = 1; col <= ColCount; col++)
                                    {
                                        string strCol = worksheet.Cells[1, col].Value.ToString().ToUpper().Replace("Ñ","N").Replace(" ","_");

                                        lstColFile.Add(strCol);

                                        strQueryCol += ", " + strCol;
                                        strColParam += ", @" + strCol;

                                        if (lstCol.IndexOf(strCol) != -1)
                                        {
                                            nuValidCol++;
                                        }
                                    }

                                    for (int row = 2; row <= rowCount; row++)
                                    {
                                        List<string> lstRow = new List<string>();

                                        for (int col = 1; col <= ColCount; col++)
                                        {

                                            if (worksheet.Cells[row, col].Value != null)
                                            {
                                                string strColName = lstColFile[col - 1];
                                        
                                                if(strColName.ToUpper() == "FECHA_ANALISIS")
                                                {
                                                    try
                                                    {
                                                        string strTmpFA = worksheet.Cells[row, col].Value.ToString().Trim();

                                                        if (strTmpFA.Length == 10)
                                                        {
                                                            string[] arrayDt = strTmpFA.Split("-");

                                                            if (arrayDt.Length == 3)
                                                            {
                                                                if(arrayDt[2].Length == 4) { 
                                                                    lstRow.Add(strTmpFA);
                                                                }
                                                                else
                                                                {
                                                                    lstRow.Add("");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                lstRow.Add("");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            DateTime dtFechaAnalisis = (DateTime)worksheet.Cells[row, col].Value;
                                                            lstRow.Add(dtFechaAnalisis.ToString("dd-MM-yyyy"));
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        lstRow.Add("");
                                                    }
                                                }
                                                else
                                                {
                                                    lstRow.Add(worksheet.Cells[row, col].Value.ToString());
                                                }
                                            }
                                            else
                                            {
                                                lstRow.Add("");
                                            }

                                        }

                                        lstRows.Add(lstRow);

                                    }

                                }
                            }

                            string strBaseQuery = "INSERT INTO ITC_CLIENTRESULTDATA(" + strQueryCol + ") ";
                            strBaseQuery += "VALUES(" + strColParam + ")";

                            ResponseDB objResponseDB = objConnection.InsMultipleData(strBaseQuery, lstColFile, lstRows, strClientDataId, "@ITC_CLIENTDATAID");

                            objResponse.Count = objResponseDB.Count;
                            objResponse.Index = ColCount;
                            objResponse.Resp = objResponseDB.Resp;

                            if(objResponseDB.Count > 0) {
                                objResponse.Type = "Success";
                            }
                            else
                            {
                                objResponse.Type = "Error";
                            }
                        }

                    }
                    else
                    {
                        objResponse.Resp = false;
                        objResponse.Msg = "No existe una sesión activa para validar el proceso.";
                        objResponse.Type = "Warning";
                    }
                }
                else
                {
                    objResponse.Resp = false;
                    objResponse.Msg = "No existe una sesión activa para validar el proceso.";
                    objResponse.Type = "Session";
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

