using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using Newtonsoft.Json;
using System.Data;

namespace Utilerias
{
    public class ManejoArchivos
    {
        const string NOMBRE_ARCHIVO_INI = "ZoeCOVE.ini";
        public static readonly string NOMBRE_ARCHIVO_LOG = "ZoeCoveTrace";

        public static void ConvertJSONToTabDelimitedFile(string jsonContent, string filePath)
        {
            DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(jsonContent, typeof(DataTable));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DataRow row in (InternalDataCollectionBase)dataTable.Rows)
                stringBuilder.AppendLine(string.Join("\t", row.ItemArray));
            File.WriteAllText(filePath, stringBuilder.ToString(), Encoding.GetEncoding(1252));
        }


        public static void BorrarArchivoLog(string nombreArchivoLog)
        {
            string pathActual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logFile = Path.Combine(pathActual, $"{nombreArchivoLog}.log");

            try
            {
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void EscribirArchivoLog(string nombreArchivoLog, string texto)
        {
            string pathActual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logFile = Path.Combine(pathActual, $"{nombreArchivoLog}.log");

            try
            {
                File.AppendAllText(logFile, $"{DateTime.Now.ToLongTimeString()} - {texto}");
                File.AppendAllText(logFile, Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

        private static string ObtenerRutaArchivoIni(string rutaCertificado)
        {
            #region ObtenerRutaArchivoIni
            string folderArchivoP12 = string.Empty;
            string pathArchivoCOVEPROXY = string.Empty;
            string pathActual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            pathArchivoCOVEPROXY = pathActual + "\\" + NOMBRE_ARCHIVO_INI;

            if (!File.Exists(pathArchivoCOVEPROXY))
            {
                folderArchivoP12 = Path.GetDirectoryName(rutaCertificado);
                pathArchivoCOVEPROXY = folderArchivoP12 + "\\" + NOMBRE_ARCHIVO_INI;
            }

            return pathArchivoCOVEPROXY;
            #endregion
        }

        internal static string ObtenerParametroArchivoIni(string rutaCertificado, string parametro)
        {
            #region ObtenerParametroArchivoIni
            string valorParametro = string.Empty;
            string pathArchivoCOVEPROXY = ObtenerRutaArchivoIni(rutaCertificado);

            List<IniInfo> listaIni = new List<IniInfo>();
            listaIni = ObtenerParametrosIni(pathArchivoCOVEPROXY);

            if (listaIni != null && listaIni.Count() > 0)
            {
                if (listaIni.Select(a => a.Clave == parametro.ToUpper()).Count() > 0)
                {
                    valorParametro = listaIni.Where(a => a.Clave == parametro.ToUpper()).FirstOrDefault().Valor;
                }
            }

            return valorParametro;
            #endregion
        }

        internal static ProxyInfo ObtenerURLProxy(string rutaCertificadoP12)
        {
            #region ObtenerURLProxy
            string pathArchivoCOVEPROXY = ObtenerRutaArchivoIni(rutaCertificadoP12);

            ProxyInfo proxyInfo = new ProxyInfo();

            List<IniInfo> listaIni = new List<IniInfo>();
            listaIni = ObtenerParametrosIni(pathArchivoCOVEPROXY);

            if (listaIni != null && listaIni.Count() > 0)
            {
                if (listaIni.Select(a => a.Clave == "PROXY").Count() > 0)
                {
                    proxyInfo.Url = listaIni.Where(a => a.Clave == "PROXY").FirstOrDefault().Valor;
                }

                if (listaIni.Select(a => a.Clave == "USER").Count() > 0)
                {
                    proxyInfo.Usuario = listaIni.Where(a => a.Clave == "USER").FirstOrDefault().Valor;
                }

                if (listaIni.Select(a => a.Clave == "PASS").Count() > 0)
                {
                    proxyInfo.Password = listaIni.Where(a => a.Clave == "PASS").FirstOrDefault().Valor;
                }

            }

            return proxyInfo;
            #endregion
        }

        private static List<IniInfo> ObtenerParametrosIni(string pathIni)
        {
            #region ObtenerParametrosIni
            List<IniInfo> listaIni = null;
            string[] lineaDividida = new string[] { };

            string[] lineasArchivo;

            if (File.Exists(pathIni))
            {
                listaIni = new List<IniInfo>();

                try
                {
                    lineasArchivo = File.ReadAllLines(pathIni);

                    foreach (var linea in lineasArchivo)
                    {
                        if (linea.Contains("="))
                        {
                            lineaDividida = linea.Split('=');
                            listaIni.Add(new IniInfo { Clave = lineaDividida[0].Trim(), Valor = lineaDividida[1].Trim() });
                        }
                    }
                }
                catch (Exception)
                {
                    listaIni = null;
                }
            }

            return listaIni;
            #endregion
        }

        public static byte[] FileBytes(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }
        }

        public static void BytesToFile(byte[] Buffer, string ToFile)
        {
            FileStream FS = new FileStream(ToFile, FileMode.Create);
            FS.Write(Buffer, 0, Buffer.Length);
            FS.Close();
        }

        public static string FileToBase64(string Path)
        {
            byte[] Val = FileBytes(Path);

            return Convert.ToBase64String(Val, Base64FormattingOptions.InsertLineBreaks);
        }

        public static void Base64ToFile(string base64, string ToFile)
        {
            File.WriteAllBytes(ToFile, Convert.FromBase64String(base64));
        }

        public static string GetEncodeWithCharArray(string inputFileName)
        {
            #region Get Encode With Char Array

            byte[] buffer;
            string sB64 = string.Empty;

            try
            {
                FileStream stream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                stream.Close();
            }
            catch (Exception exception)
            {
                throw exception;
            }
            long num = (long)(1.3333333333333333 * buffer.Length);
            if ((num % 4L) != 0L)
            {
                num += 4L - (num % 4L);
            }
            char[] outArray = new char[num];
            try
            {
                Convert.ToBase64CharArray(buffer, 0, buffer.Length, outArray, 0);
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Binary data array is null.");
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new Exception("Char Array is not large enough.");
            }
            try
            {
                //StreamWriter writer = new StreamWriter(@"C:\TempCert\Cert.b64", false, Encoding.ASCII);
                //writer.Write(outArray);
                //writer.Close();

                sB64 = new string(outArray);
            }
            catch (Exception exception2)
            {
                throw exception2;
            }

            return sB64;

            #endregion
        }

        public static string FileToStringHexa(string rutaArchivo)
        {
            byte[] archivo = FileBytes(rutaArchivo);
            string archivoString = ByteArrayToStringHex(archivo);
            return archivoString;
        }

        public static byte[] FileToByteArray(string rutaArchivo)
        {
            byte[] archivo = FileBytes(rutaArchivo);
            return archivo;
        }

        public static string ByteArrayToStringHex(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }


        public static string EscapeXML(string texto)
        {
            string textoEncoded = texto;
            StringBuilder encodedString = new StringBuilder(textoEncoded.Length);
            XmlWriterSettings XmlSettings = new XmlWriterSettings();
            XmlSettings.ConformanceLevel = ConformanceLevel.Auto;
            XmlSettings.Encoding = Encoding.GetEncoding(28591);
            using (var writer = XmlWriter.Create(encodedString, XmlSettings))
            {
                writer.WriteString(textoEncoded);
            }
            return textoEncoded;
        }
    }
    internal class IniInfo
    {
        public string Clave { get; set; }
        public string Valor { get; set; }
    }

    internal class ProxyInfo
    {
        public string Url { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
    }
}
