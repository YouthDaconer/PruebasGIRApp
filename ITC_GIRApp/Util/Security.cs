using System;
using System.Security.Cryptography;
using System.Text;

namespace ITC_GIRApp.Util
{
    public class Security
    {
        /// <summary>
        ///  @Author: DHernandez
        ///  @Date: 25/11/2019
        ///  @Desc: Metodo que encripta una cadena
        ///  @Retuns: Cadena encriptada.
        public string HashText(string strText)
        {
            SHA1CryptoServiceProvider objHasher = new SHA1CryptoServiceProvider();
            byte[] textWithSaltBytes = Encoding.UTF8.GetBytes(string.Concat(strText, strSalt));
            byte[] hashedBytes = objHasher.ComputeHash(textWithSaltBytes);
            objHasher.Clear();

            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        ///  @Author: DHernandez
        ///  @Date: 25/11/2019
        ///  @Desc: Metodo que compara una cadena encriptada con una cadena sin encriptar.
        ///  @Retuns: True/False.
        public bool ValidateText(string strText, string strCryptedText)
        {
            bool blCompare = false;
            SHA1CryptoServiceProvider objHasher = new SHA1CryptoServiceProvider();
            byte[] textWithSaltBytes = Encoding.UTF8.GetBytes(string.Concat(strText, strSalt));
            byte[] hashedBytes = objHasher.ComputeHash(textWithSaltBytes);
            objHasher.Clear();
            string str = Convert.ToBase64String(hashedBytes);

            if (Convert.ToBase64String(hashedBytes).Equals(strCryptedText))
            {
                blCompare = true;
            }

            return blCompare;
        }

        public string strSalt = "ITC_D4t34_2019";
    }
}