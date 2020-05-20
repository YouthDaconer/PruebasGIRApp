using System;

namespace ITC_GIRApp.Models
{
    /// <summary>
    /// Summary description for Response
    /// </summary>
    public static class Config
    {

        #region Propiedades

        /*
            //Intelecto
            "UrlServiceAD": "http://azuredev.intelecto.co:9095/ITC_SyncAD/",
            "OUCreateAD": "/OU=Cali,OU=Colombia",
            "DomainCreateAD": "@pruebasitc.net",
            "LocalPath": "E:\\GIRApp\\",
            "UserSend": "noreply_tonline@ezenza.co",
            "UserSendName": "GIRApp",
            "UserReceiveName": "Usuario",
            "Port": 587,
            "Host": "pro.turbo-smtp.com",
            "UserName": "noreply_tonline@ezenza.co",
            "Password": "$Support$.2019",
            "SQLPass": "Intelecto2019",
            "Server": "DESKTOP-74FRG4P",
            "DBName": "ITC_GIRApp",
            "SQLUser": "dhernandez"

            //Eficacia
                "UrlServiceAD": "http://localhost:8080/",
                "OUCreateAD": "/OU=Intelecto,OU=OTROS,OU=Eficacia",
                "DomainCreateAD": "@Eficacia.net",
                "LocalPath": "F:\\GIRApp\\",
                "UserSend": "noreply_tonline@ezenza.co",
                "UserSendName": "GIRApp",
                "UserReceiveName": "Usuario",
                "Port": 587,
                "Host": "pro.turbo-smtp.com",
                "UserName": "noreply_tonline@ezenza.co",
                "Password": "$Support$.2019",
                "SQLPass": "Intelecto2019",
                "Server": "DESKTOP-74FRG4P",
                "DBName": "ITC_GIRApp",
                "SQLUser": "user_GIRApp"

        */

        public static string UrlServiceAD = "";
        public static string OUCreateAD = "";
        public static string DomainCreateAD = "";
        public static string LocalPath = "";

        public static string UserSend = "";
        public static string UserSendName = "";
        public static string UserReceiveName = "";
        public static int Port = 587;
        public static string Host = "";
        public static string UserName = "";
        public static string Password = "";

        public static string Register = "";
        public static string Forgot = "";

        public static string SQLPass = "";
        public static string Server = "";
        public static string DBName = "";
        public static string SQLUser = "";

        #endregion
    }
}