using System;

namespace ITC_GIRApp.Models
{
    /// <summary>
    /// Summary description for Response
    /// </summary>
    public class Request
    {
        public Request()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Propiedades

        /// <summary>
        /// Identificador de la respuesta.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Contenido del mensaje.
        /// </summary>
        public string Msg
        {
            get;
            set;
        }

        /// <summary>
        /// Validación de la licensia.
        /// </summary>
        public bool License
        {
            get;
            set;
        }

        /// <summary>
        /// Tipo del mensaje.
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Redirect to Page?
        /// </summary>
        public string Redirect
        {
            get;
            set;
        }
        /// <summary>
        /// Json Data Resp
        /// </summary>
        public string Data
        {
            get;
            set;
        }
        /// <summary>
        /// Json Menu
        /// </summary>
        public string Menu
        {
            get;
            set;
        }

        /// <summary>
        /// Json Labels
        /// </summary>
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// Json Aditional Data
        /// </summary>
        public String Json
        {
            get;
            set;
        }

        #endregion
    }
}