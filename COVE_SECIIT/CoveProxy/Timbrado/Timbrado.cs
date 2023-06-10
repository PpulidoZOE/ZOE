using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Xml;
using System.ServiceModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using CoveProxy.Timbrado;
using CoveProxy.FacturacionModernaTimbradoServicio;
using System.Xml.Schema;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SW.Helpers;
using SW.Services.Stamp;
using SW.Services.Authentication;
using RestSharp;
using static CoveProxy.Timbrado.Helper;
using static CoveProxy.Timbrado.pdfSW;
using CoveProxy.BuzonE;
using System.Net.Mail;
using Microsoft.SqlServer.Server;

namespace CoveProxy.Timbrado
{
    public class Timbrador
    {
        static StringBuilder sb = new StringBuilder();
        static bool errorEnEsquema = false;
        string sError;

        #region "Validar esquema de XML"
        internal void ValidarEsquema(string archivoXML)
        {
            // Set the validation settings.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            using (XmlReader reader = XmlReader.Create(archivoXML, settings))
            {
                // Parse the file. 
                while (reader.Read()) ;

                reader.Close();
            }
            //XmlReader reader = XmlReader.Create(archivoXML, settings);
        }

        // Display any warnings or errors.
        internal static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine("\tAdvertencia: No se encontro el archivo esquema, No se realizo la validacion:" + args.Message);
                throw new Exception("\tAdvertencia: No se encontro el archivo esquema, No se realizo la validacion:" + args.Message);
            }
            else
            {
                errorEnEsquema = true;
                sb.AppendLine(args.Message);
            }
        }
        #endregion

        internal void CrearXML(string layout, string rutaArchivoLayout, string nombreArchivoXML, string prefijo)
        {
            #region CrearXMLConError
            XmlDocument documentoXML = new XmlDocument();
            documentoXML.LoadXml(layout);
            documentoXML.Save(rutaArchivoLayout + "\\" + prefijo + nombreArchivoXML);
            documentoXML = null;
            #endregion
        }

        internal void CrearXMLConError(string layout, string rutaArchivoLayout, string nombreArchivoXML)
        {
            #region CrearXMLConError
            CrearXML(layout, rutaArchivoLayout, nombreArchivoXML, "CON_ERROR_");
            #endregion
        }

        internal string TimbrarPACFOLIODIGITAL(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userID, string userPass, string urlTimbrado, string referencia)
        {
            #region TimbrarPACFOLIODIGITAL
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;
            string layout = string.Empty;
            Comprobante.Utilidades util = new Comprobante.Utilidades();
            XmlDocument documentoXML = new XmlDocument();
            XmlDocument xmlFactura = new XmlDocument();
            XmlElement xmlElementTimbre;
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri uriServicio = new Uri(urlTimbrado);
            EndpointAddress endpoint = new EndpointAddress(uriServicio);
            string uuid = string.Empty;
            const string FOLIOCFDI_EXISTENTE = "801";


            switch (uriServicio.Scheme)
            {
                case "http":
                    SetBinding(binding, BasicHttpSecurityMode.None);
                    break;
                case "https":
                    SetBinding(binding, BasicHttpSecurityMode.Transport);
                    break;
                default:
                    SetBinding(binding);
                    break;
            }

            if (File.Exists(archivoXML))
            {
                System.Diagnostics.Trace.WriteLine(string.Format("-------------------Inicia proceso timbrado Folio Digital-------------------- {0}", System.DateTime.Now.ToShortTimeString()));
                System.Diagnostics.Trace.WriteLine(string.Format("Inicia validacion de esquema: {0}", System.DateTime.Now.ToShortTimeString()));
                ValidarEsquema(archivoXML);
                System.Diagnostics.Trace.WriteLine(string.Format("Termina validacion de esquema: {0}", System.DateTime.Now.ToShortTimeString()));

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);

                    try
                    {
                        var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                        System.Diagnostics.Trace.WriteLine(string.Format("Inicia agregar sello: {0}", System.DateTime.Now.ToShortTimeString()));
                        layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);
                        CrearXML(layout, rutaArchivoLayout, nombreArchivoLayout, "FIRMADO_");
                        System.Diagnostics.Trace.WriteLine(string.Format("Termina agregar sello: {0}", System.DateTime.Now.ToShortTimeString()));

                        WSFD.RespuestaTFD respuestaTimbrado = new WSFD.RespuestaTFD();
                        WSFD.RespuestaTFD respuestaPDF = new WSFD.RespuestaTFD();
                        WSFD.WSTFDClient servicioFD = new WSFD.WSTFDClient(binding, endpoint);

                        WSFD33.RespuestaTFD33 respuestaTimbrado33 = new WSFD33.RespuestaTFD33();
                        WSFD33.RespuestaTFD33 respuestaPDF33 = new WSFD33.RespuestaTFD33();
                        WSFD33.WSCFDI33Client servicioFD33 = new WSFD33.WSCFDI33Client(binding, endpoint);

                        if (versionCFDI == "3.3")
                        {
                            System.Diagnostics.Trace.WriteLine(string.Format("Inicia llamada a servicio para timbrar: {0}", System.DateTime.Now.ToShortTimeString()));
                            respuestaTimbrado33 = servicioFD33.TimbrarCFDI(userID, userPass, layout, referencia);
                            System.Diagnostics.Trace.WriteLine(string.Format("Termina llamada a servicio para timbrar: {0}", System.DateTime.Now.ToShortTimeString()));

                            respuestaTimbrado.CodigoRespuesta = respuestaTimbrado33.CodigoRespuesta;
                            respuestaTimbrado.MensajeError = respuestaTimbrado33.MensajeError;
                            respuestaTimbrado.MensajeErrorDetallado = respuestaTimbrado33.MensajeErrorDetallado;
                            respuestaTimbrado.OperacionExitosa = respuestaTimbrado33.OperacionExitosa;
                            respuestaTimbrado.XMLResultado = respuestaTimbrado33.XMLResultado;

                            if (respuestaTimbrado33.OperacionExitosa)
                            {
                                respuestaTimbrado.PDFResultado = respuestaTimbrado33.PDFResultado;
                                respuestaTimbrado.Timbre = new WSFD.Timbre
                                {
                                    UUID = respuestaTimbrado33.Timbre.UUID,
                                    FechaTimbrado = respuestaTimbrado33.Timbre.FechaTimbrado,
                                    SelloCFD = respuestaTimbrado33.Timbre.SelloCFD,
                                    NumeroCertificadoSAT = respuestaTimbrado33.Timbre.NumeroCertificadoSAT,
                                    Estado = respuestaTimbrado33.Timbre.Estado,
                                    SelloSAT = respuestaTimbrado33.Timbre.SelloSAT
                                };
                            }
                        }
                        else
                        {
                            System.Diagnostics.Trace.WriteLine(string.Format("Inicia llamada a servicio para timbrar: ", System.DateTime.Now.ToShortTimeString()));
                            respuestaTimbrado = servicioFD.TimbrarCFDI(userID, userPass, layout, referencia);
                            System.Diagnostics.Trace.WriteLine(string.Format("Termina llamada a servicio para timbrar: ", System.DateTime.Now.ToShortTimeString()));
                        }

                        if (respuestaTimbrado.OperacionExitosa == true || respuestaTimbrado.CodigoRespuesta == FOLIOCFDI_EXISTENTE)
                        {
                            documentoXML.LoadXml(respuestaTimbrado.XMLResultado);

                            if (respuestaTimbrado.OperacionExitosa)
                            {
                                uuid = respuestaTimbrado.Timbre.UUID;
                            }
                            else
                            {
                                xmlElementTimbre = (XmlElement)documentoXML.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                                uuid = xmlElementTimbre.GetAttribute("UUID");
                            }

                            //Guardo el XML timbrado.
                            System.Diagnostics.Trace.WriteLine(string.Format("Creando XML timbrado: {0}", System.DateTime.Now.ToShortTimeString()));
                            documentoXML.Save(rutaArchivoLayout + "\\" + uuid + ".xml");

                            if (versionCFDI == "3.3")
                            {
                                System.Diagnostics.Trace.WriteLine(string.Format("Inicia llamado a servicio para obtener PDF: {0}", System.DateTime.Now.ToShortTimeString()));
                                respuestaPDF33 = servicioFD33.ObtenerPDF(userID, userPass, uuid, "");
                                System.Diagnostics.Trace.WriteLine(string.Format("Termina llamado a servicio para obtener PDF: {0}", System.DateTime.Now.ToShortTimeString()));

                                respuestaPDF.PDFResultado = respuestaPDF33.PDFResultado;
                                respuestaPDF.OperacionExitosa = respuestaPDF33.OperacionExitosa;
                                respuestaPDF.MensajeError = respuestaPDF33.MensajeError;
                                respuestaPDF.MensajeErrorDetallado = respuestaPDF33.MensajeErrorDetallado;
                            }
                            else
                            {
                                System.Diagnostics.Trace.WriteLine(string.Format("Inicia llamado a servicio para obtener PDF: {0}", System.DateTime.Now.ToShortTimeString()));
                                respuestaPDF = servicioFD.ObtenerPDF(userID, userPass, uuid, "");
                                System.Diagnostics.Trace.WriteLine(string.Format("Termina llamado a servicio para obtener PDF: {0}", System.DateTime.Now.ToShortTimeString()));
                            }

                            if (respuestaPDF.OperacionExitosa == true || respuestaTimbrado.CodigoRespuesta == FOLIOCFDI_EXISTENTE)
                            {
                                System.Diagnostics.Trace.WriteLine(string.Format("Creando PDF: {0}", System.DateTime.Now.ToShortTimeString()));
                                Utilerias.ManejoArchivos.Base64ToFile(respuestaPDF.PDFResultado, rutaArchivoLayout + "\\" + uuid + ".pdf");
                            }
                            else
                            {
                                return string.Format("ERROR El timbrado fue exitoso pero con error al obtener el PDF mensaje error {0}", respuestaPDF.MensajeErrorDetallado);
                            }

                            System.Diagnostics.Trace.WriteLine(string.Format("-------------------Termina proceso timbrado Folio Digital-------------------- {0}", System.DateTime.Now.ToShortTimeString()));
                            return uuid;
                        }
                        else
                        {
                            System.Diagnostics.Trace.WriteLine(string.Format("Creando XML con error: {0}", System.DateTime.Now.ToShortTimeString()));
                            CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                            return string.Format("ERROR codigo {0} mensaje error {1}", respuestaTimbrado.CodigoRespuesta, respuestaTimbrado.MensajeError);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Creando XML con error: {0}", System.DateTime.Now.ToShortTimeString()));
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        return string.Format("ERROR mensaje error {0}, stacktrace {1}", e.Message, e.StackTrace);
                    }
                }
            }
            else
            {
                return string.Format("ERROR no se encontro el archivo {0}", archivoXML);
            }
            #endregion
        }

        internal string TimbrarPACDiversa(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string rfc, string userPass, string urlTimbrado, string referencia, string userId)
        {
            #region TimbrarPACDiversa
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;
            string cfdiReturn = string.Empty;
            string cfd = string.Empty;
            string layout = string.Empty;
            WebClient webClient = new WebClient();
            Comprobante.Utilidades util = new Comprobante.Utilidades();
            XmlElement xmlElementTimbre;
            XmlDocument xmlFactura = new XmlDocument();
            XmlDocument doc = new XmlDocument();
            string uuid = string.Empty;


            if (File.Exists(archivoXML))
            {
                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);

                    try
                    {
                        var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                        layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);

                        if (versionCFDI == "3.3")
                        {
                            //Omitir la verificacion de certificado SSL valido
                            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                            using (var httpClient = new HttpClient())
                            {
                                var mensaje = string.Empty;
                                byte[] bytes = Encoding.UTF8.GetBytes(layout);
                                string base64 = Convert.ToBase64String(bytes);

                                doc.LoadXml(layout);
                                var complementoXML = doc.GetElementsByTagName("cfdi:Complemento");

                                if (complementoXML != null && complementoXML.Count > 0)
                                {
                                    mensaje = "{\"credentials\": {\"id\": \"" + userId + "\", \"token\":\"" + userPass + "\"},  \"issuer\": {\"rfc\": \"" + rfc + "\"  },  \"receiver\": {    \"emails\": [   { \"email\": \"email@domain.com\",   \"format\": \"xml+pdf\",  \"template\": \"letter\" } ]  },  \"document\": { \"ref-id\": \"" + referencia + "\",  \"certificate-number\": \"" + util.getCertificateNumber() + "\",  \"section\": \"all\", \"format\": \"xml\",    \"template\": \"letter\",  \"type\": \"application/vnd.diverza.cfdi_3.3_complemento+xml\",  \"content\": \"" + base64 + "\"  }}";
                                }
                                else
                                {
                                    mensaje = "{\"credentials\": {\"id\": \"" + userId + "\", \"token\":\"" + userPass + "\"},  \"issuer\": {\"rfc\": \"" + rfc + "\"  },  \"receiver\": {    \"emails\": [   { \"email\": \"email@domain.com\",   \"format\": \"xml+pdf\",  \"template\": \"letter\" } ]  },  \"document\": { \"ref-id\": \"" + referencia + "\",  \"certificate-number\": \"" + util.getCertificateNumber() + "\",  \"section\": \"all\", \"format\": \"xml\",    \"template\": \"letter\",  \"type\": \"application/vnd.diverza.cfdi_3.3+xml\",  \"content\": \"" + base64 + "\"  }}";
                                }

                                using (var content = new StringContent(mensaje, System.Text.Encoding.UTF8, "application/json"))
                                {
                                    //using (var response = httpClient.PostAsync("api/v1/documents/stamp", content).Result)
                                    using (var response = httpClient.PostAsync(urlTimbrado, content).Result)
                                    {
                                        var responseData = response.Content.ReadAsStringAsync().Result;
                                        var jsonData = new JObject();

                                        if (responseData.Length > 0)
                                        {
                                            jsonData = JObject.Parse(responseData);
                                        }
                                        else
                                        {
                                            throw new Exception("La llamada al servicio fue exitosa, pero el servicio no regreso informacion");
                                        }

                                        if (response.IsSuccessStatusCode)
                                        {
                                            uuid = (string)jsonData.SelectToken("uuid");
                                            Utilerias.ManejoArchivos.Base64ToFile((string)jsonData.SelectToken("content"), rutaArchivoLayout + "\\" + uuid + ".xml");
                                        }
                                        else
                                        {
                                            File.WriteAllText(rutaArchivoLayout + "\\ERRORES_" + referencia + ".txt", responseData);
                                            uuid = string.Format("ERROR codigo HTTP {0}, mensaje error: {1}, error detallado: {2}", response.StatusCode, response.ReasonPhrase, (string)jsonData.SelectToken("error_details"));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            webClient.Headers.Add("x-auth-token", userPass);
                            webClient.Encoding = System.Text.Encoding.GetEncoding("UTF-8");

                            cfdiReturn = webClient.UploadString(urlTimbrado, "POST", layout);

                            doc.LoadXml(cfdiReturn);

                            xmlElementTimbre = (XmlElement)doc.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                            uuid = xmlElementTimbre.GetAttribute("UUID");

                            if (doc != null)
                            {
                                doc.Save(rutaArchivoLayout + "\\" + uuid + ".xml");
                            }
                        }

                        return uuid;

                    }
                    catch (WebException e)
                    {
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);

                        // En caso de error podemos obtener el codigo de estatus de la petición consultando la excepción que
                        // arrojada por la instancia del web client. 
                        HttpWebResponse httpWebResponse = ((HttpWebResponse)e.Response);
                        if (httpWebResponse != null)
                        {
                            cfdiReturn = string.Format("ERROR Codigo HTTP: {0} {1}", httpWebResponse.StatusCode, httpWebResponse.StatusDescription);
                        }
                        else
                        {
                            cfdiReturn = String.Format("ERROR {0}, stacktrace {1}", e.Message, e.StackTrace);
                        }
                    }
                    catch (Exception ex)
                    {
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        cfdiReturn = String.Format("ERROR {0}, stacktrace {1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            else
            {
                cfdiReturn = String.Format("ERROR No se encontro archivo {0}", layout);
            }
            return cfdiReturn;
            #endregion
        }

        internal string TimbrarPACModerna(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userID, string userPass, string emisorRFC, string urlTimbrado)
        {
            #region TimbrarPACModerna
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;
            string layout = string.Empty;
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri uriServicio = new Uri(urlTimbrado);
            EndpointAddress endpoint = new EndpointAddress(uriServicio);
            Resultados result = new Resultados();
            requestTimbrarCFDI RequestTimbrarCFDI = new requestTimbrarCFDI();
            Comprobante.Utilidades util = new Comprobante.Utilidades();

            switch (uriServicio.Scheme)
            {
                case "http":
                    SetBinding(binding, BasicHttpSecurityMode.None);
                    break;
                case "https":
                    SetBinding(binding, BasicHttpSecurityMode.Transport);
                    break;
                default:
                    SetBinding(binding);
                    break;
            }

            RequestTimbrarCFDI.emisorRFC = emisorRFC;
            RequestTimbrarCFDI.urlTimbrado = urlTimbrado;
            RequestTimbrarCFDI.UserID = userID;
            RequestTimbrarCFDI.UserPass = userPass;
            RequestTimbrarCFDI.generarCBB = true;
            RequestTimbrarCFDI.generarPDF = true;

            if (File.Exists(archivoXML))
            {
                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);

                    try
                    {
                        var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                        layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);

                        layout = Convert.ToBase64String(Encoding.UTF8.GetBytes(layout));

                        RequestTimbrarCFDI.text2CFDI = layout;

                        // indicar que no deseamos esperar confirmación del server, sino que debe enviar los datos al mismo tiempo que se realiza la solicitud.
                        System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidarCertificado);

                        //Utilizar proxy
                        if (RequestTimbrarCFDI.proxy_url != "")
                        {
                            WebRequest.DefaultWebProxy = new WebProxy(new Uri(RequestTimbrarCFDI.proxy_url + ":" + RequestTimbrarCFDI.proxy_port))
                            {
                                Credentials = new NetworkCredential(RequestTimbrarCFDI.proxy_user, RequestTimbrarCFDI.proxy_pass),
                            };
                        }

                        Timbrado_ManagerPort WSFModerna = new Timbrado_ManagerPortClient(binding, endpoint);

                        // Ejecutar servicio de Timbrado
                        Object objResponse = WSFModerna.requestTimbrarCFDI(RequestTimbrarCFDI);

                        if (objResponse != null)
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            XmlDeclaration xmlDeclaration;
                            XmlElement xmlElementBody;
                            xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "uft-8", "no");
                            xmlElementBody = xmlDoc.CreateElement("Container");
                            xmlDoc.InsertBefore(xmlElementBody, xmlDoc.DocumentElement);
                            XmlElement xmlParentNode;
                            xmlParentNode = xmlDoc.CreateElement("responseSoap");
                            xmlDoc.DocumentElement.PrependChild(xmlParentNode);
                            XmlNode[] nodosXmlResponse = (XmlNode[])objResponse;
                            foreach (XmlNode nodo in nodosXmlResponse)
                            {
                                if (nodo.InnerText.Length >= 1)
                                {
                                    XmlElement xmlElemetResponse;
                                    xmlElemetResponse = xmlDoc.CreateElement(nodo.Name.ToString());
                                    XmlText xmlTextNode;
                                    xmlTextNode = xmlDoc.CreateTextNode(nodo.InnerText.ToString());
                                    xmlParentNode.AppendChild(xmlElemetResponse);
                                    xmlElemetResponse.AppendChild(xmlTextNode);
                                }
                            }

                            //-->>Accedemos a los nodos de la respuesta del xml para obenter los valores retornados en base64 (xml, pdf, cbb, txt)
                            XmlElement xmlElementCFDI;
                            //-->>Xml certificado (CFDI)
                            xmlElementCFDI = (XmlElement)xmlDoc.GetElementsByTagName("xml").Item(0);

                            // Obtener UUID del Comprobante
                            XmlDocument cfdiXML = new XmlDocument();
                            byte[] binary = Convert.FromBase64String(xmlElementCFDI.InnerText);
                            String strOriginal = System.Text.Encoding.UTF8.GetString(binary);
                            cfdiXML.LoadXml(strOriginal);
                            XmlElement xmlElementTimbre;
                            xmlElementTimbre = (XmlElement)cfdiXML.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                            result.uuid = xmlElementTimbre.GetAttribute("UUID");

                            result.xmlBase64 = xmlElementCFDI.InnerText;
                            Utilerias.ManejoArchivos.Base64ToFile(result.xmlBase64, rutaArchivoLayout + "\\" + result.uuid + ".xml");

                            //-->>Representación impresa del CFDI en formato PDF
                            if (RequestTimbrarCFDI.generarPDF)
                            {
                                XmlElement xmlElementPDF = (XmlElement)xmlDoc.GetElementsByTagName("pdf").Item(0);
                                result.pdfBase64 = xmlElementPDF.InnerText;
                                Utilerias.ManejoArchivos.Base64ToFile(result.pdfBase64, rutaArchivoLayout + "\\" + result.uuid + ".pdf");
                            }

                            //-->>Representación impresa del CFDI en formato TXT
                            if (RequestTimbrarCFDI.generarTXT)
                            {
                                XmlElement xmlElementTXT = (XmlElement)xmlDoc.GetElementsByTagName("txt").Item(0);
                                result.txtBase64 = xmlElementTXT.InnerText;
                            }

                            //-->>Representación impresa del CFDI en formato PNG
                            if (RequestTimbrarCFDI.generarCBB)
                            {
                                XmlElement xmlElementCBB = (XmlElement)xmlDoc.GetElementsByTagName("png").Item(0);
                                result.cbbBase64 = xmlElementCBB.InnerText;
                                Utilerias.ManejoArchivos.Base64ToFile(result.cbbBase64, rutaArchivoLayout + "\\" + result.uuid + ".png");
                            }
                            result.code = "T000";
                            result.message = "Comprobante Generado con exito";
                            result.status = true;
                            return result.uuid;
                        }
                        else
                        {
                            CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                            result.code = "T00N";
                            result.message = "El servicio de timbrado respondio con NULL";
                            result.status = false;
                            return string.Format("ERROR codigo {0} El servicio de timbrado respondio con NULL", result.code);
                        }
                    }
                    catch (Exception e)
                    {
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        result.code = "EX-001";
                        result.message = "Error: " + e.Message;
                        result.status = false;
                        return string.Format("ERROR codigo {0} mensaje error {1}, stacktrace {2}", result.code, e.Message, e.StackTrace);
                    }
                }
            }
            else
            {
                return string.Format("ERROR no se encontro el archivo {0}", archivoXML);
            }

            #endregion
        }

        internal string TimbrarPACCarvajal(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userID, string userPass, string emisorRFC, string urlTimbrado)
        {
            #region TimbrarPACCarvajal
            Comprobante.Utilidades util = new Comprobante.Utilidades();
            CarvajalCFDIServicio.CFDIWebServiceHeader loginHeader = new CarvajalCFDIServicio.CFDIWebServiceHeader();
            XmlElement loginResponse;
            XmlElement result;
            XmlElement xmlElementTimbre;
            XmlDocument doc = new XmlDocument();
            XmlDocument docTimbrado = new XmlDocument();
            string uuid = string.Empty;
            string layout = string.Empty;
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri uriServicio = new Uri(urlTimbrado);
            EndpointAddress endpoint = new EndpointAddress(uriServicio);
            bool success;
            string message;
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;


            switch (uriServicio.Scheme)
            {
                case "http":
                    SetBinding(binding, BasicHttpSecurityMode.None);
                    break;
                case "https":
                    SetBinding(binding, BasicHttpSecurityMode.Transport);
                    break;
                default:
                    SetBinding(binding);
                    break;
            }

            // Direccion del servicio
            CarvajalCFDIServicio.CFDIServiceSoapClient proxyClient = new CarvajalCFDIServicio.CFDIServiceSoapClient(binding, endpoint);

            //Se asigna el nombre de usuario y la contraseña
            loginHeader.Username = userID;
            loginHeader.Password = userPass;

            rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
            nombreArchivoLayout = Path.GetFileName(archivoXML);

            try
            {
                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                    layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);

                    //Se llama la operación login para autenticarse en el sistema
                    loginResponse = proxyClient.Login(loginHeader);

                    success = bool.Parse(loginResponse.Attributes["success"].Value);
                    message = loginResponse["message"].InnerText;

                    if (!success)
                    {
                        loginHeader.AuthenticatedToken = string.Empty;
                        return String.Format("ERROR {0}", message);
                    }
                    else
                    {
                        loginHeader.Username = string.Empty;
                        loginHeader.Password = string.Empty;

                        //Se obtiene el token de autenticación
                        loginHeader.AuthenticatedToken = loginResponse["token"].InnerText;

                        doc.PreserveWhitespace = true;
                        doc.LoadXml(layout);

                        result = proxyClient.GetTimbre(loginHeader, doc.DocumentElement);

                        if (result.InnerText.ToUpper().Contains("ERROR"))
                        {
                            uuid = result.InnerText;
                            CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        }
                        else
                        {
                            xmlElementTimbre = (XmlElement)result.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                            if (xmlElementTimbre == null)
                            {
                                uuid = string.Format("ERROR {0}", result.InnerText);
                                CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                            }
                            else
                            {
                                uuid = xmlElementTimbre.GetAttribute("UUID");
                                docTimbrado.LoadXml(result.OuterXml);
                                docTimbrado.Save(rutaArchivoLayout + "\\" + uuid + ".xml");
                            }
                        }

                        return uuid;
                    }
                }
            }
            catch (Exception ex)
            {
                CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                return String.Format("ERROR {0}, STACKTRACE {1}", ex.Message, ex.StackTrace);
            }

            #endregion
        }

        internal string TimbrarPACEkomercio(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userID, string userPass, string emisorRFC, string urlTimbrado)
        {
            #region TimbrarPACEkomercio
            Comprobante.Utilidades util = new Comprobante.Utilidades();
            string uuid = string.Empty;
            string layout = string.Empty;
            BasicHttpBinding binding = new BasicHttpBinding();
            XmlElement xmlElementBusqueda;
            XmlDocument doc = new XmlDocument();
            Uri uriServicio = new Uri(urlTimbrado);
            EndpointAddress endpoint = new EndpointAddress(uriServicio);
            string urlPDF;
            string resultado;
            string resultadoPDF;
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;

            switch (uriServicio.Scheme)
            {
                case "http":
                    SetBinding(binding, BasicHttpSecurityMode.None);
                    break;
                case "https":
                    SetBinding(binding, BasicHttpSecurityMode.Transport);
                    break;
                default:
                    SetBinding(binding);
                    break;
            }

            // Direccion del servicio
            EkomercioServicio.WSCFDBuilderPlusSoapClient proxyClient = new EkomercioServicio.WSCFDBuilderPlusSoapClient(binding, endpoint);

            rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
            nombreArchivoLayout = Path.GetFileName(archivoXML);

            try
            {
                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                    layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);
                    resultado = proxyClient.getCFDI(userID, userPass, layout);

                    doc.LoadXml(resultado);

                    if (doc.GetElementsByTagName("ErrorMessage").Count > 0)
                    {
                        xmlElementBusqueda = (XmlElement)doc.GetElementsByTagName("ErrorMessage").Item(0);
                        uuid = string.Format("ERROR {0}", xmlElementBusqueda.InnerText);
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);

                        return uuid;
                    }
                    else
                    {
                        xmlElementBusqueda = (XmlElement)doc.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                        uuid = xmlElementBusqueda.GetAttribute("UUID");

                        //Generar XML fimado
                        doc.Save(rutaArchivoLayout + "\\" + uuid + ".xml");

                        //Obtener PDF
                        //resultadoPDF = proxyClient.generaPDF(userID, userPass, emisorRFC, uuid);
                        //urlPDF = Utilerias.ManejoArchivos.ObtenerURLEkomercioPDF(archivoCertificado);

                        //if (!string.IsNullOrEmpty(urlPDF))
                        //{
                        //    urlPDF = string.Format("{0}{1}_{2}.PDF", urlPDF, emisorRFC, uuid);

                        //    using (var client = new WebDownload())
                        //    {
                        //        client.DownloadFile(urlPDF, "a.PDF");
                        //    }
                        //}

                        return uuid;
                    }
                }
            }
            catch (Exception ex)
            {
                CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                return String.Format("ERROR {0}, STACKTRACE {1}", ex.Message, ex.StackTrace);
            }
            #endregion
        }

        internal string TimbrarPACPaxFacturacion(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userId, string userPass, string urlTimbrado, string referencia)
        {
            #region TimbrarPACPaxFacturacion
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;
            string cfdiReturn = string.Empty;
            string cfd = string.Empty;
            string layout = string.Empty;
            string respuestaServicio = string.Empty;
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri uriServicio = new Uri(urlTimbrado);
            EndpointAddress endpoint = new EndpointAddress(uriServicio);
            Comprobante.Utilidades util = new Comprobante.Utilidades();
            XmlDocument documentoXML = new XmlDocument();
            XmlElement xmlElementTimbre;
            string uuid = string.Empty;


            if (File.Exists(archivoXML))
            {
                switch (uriServicio.Scheme)
                {
                    case "http":
                        SetBinding(binding, BasicHttpSecurityMode.None);
                        break;
                    case "https":
                        SetBinding(binding, BasicHttpSecurityMode.Transport);
                        break;
                    default:
                        SetBinding(binding);
                        break;
                }

                PaxFacturacionServicio.wcfRecepcionASMXSoapClient servicio = new PaxFacturacionServicio.wcfRecepcionASMXSoapClient(binding, endpoint);

                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);

                    try
                    {
                        var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                        layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);

                        if (versionCFDI == "3.3")
                        {
                            respuestaServicio = servicio.fnEnviarXML(layout, "01", 0, userId, userPass, "3.3");

                            try
                            {
                                documentoXML.LoadXml(respuestaServicio);
                                xmlElementTimbre = (XmlElement)documentoXML.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                                uuid = xmlElementTimbre.GetAttribute("UUID");

                                // Generar XML fimado
                                documentoXML.Save(rutaArchivoLayout + "\\" + uuid + ".xml");
                            }
                            catch (Exception)
                            {
                                uuid = "ERROR: " + respuestaServicio;
                                CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                            }
                        }
                        else
                        {
                            uuid = "ERROR: Otras versiones de CFDI diferentes a 3.3 no es soportada";
                        }

                        return uuid;

                    }
                    catch (Exception ex)
                    {
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        cfdiReturn = String.Format("ERROR {0}, stacktrace {1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            else
            {
                cfdiReturn = String.Format("ERROR No se encontro archivo {0}", layout);
            }
            return cfdiReturn;
            #endregion
        }

        internal string TimbrarPACEDICOM(string archivoXML, string archivoXslt, string folderEntradaPAC, string folderSalidaPAC, string tiempoEspera, string reintentos)
        {
            #region Timbar PAC EDICOM
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;
            string cfdiReturn = string.Empty;
            int waitMileSecond;
            int retries;
            int reintentosCount = 0;
            bool archivoProcesado = false;

            var exito = int.TryParse(tiempoEspera, out waitMileSecond);
            if (!exito)
            {
                return String.Format("ERROR tiempo de espera invalido {0}", tiempoEspera); ;
            }

            exito = int.TryParse(reintentos, out retries);
            if (!exito)
            {
                return String.Format("ERROR valor de reintentos invalido {0}", reintentos); ;
            }

            if (File.Exists(archivoXML))
            {
                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);
                    var nombreArchivoTxt = Path.GetFileNameWithoutExtension(archivoXML);

                    try
                    {
                        File.Copy(archivoXML, string.Format(@"{0}\{1}", folderEntradaPAC, nombreArchivoLayout), overwrite: true);
                        using (var resource = File.CreateText(string.Format(@"{0}\{1}.txt", folderEntradaPAC, nombreArchivoTxt)))
                        {
                        };

                        var archivoPacSalida = string.Format(@"{0}\{1}", folderSalidaPAC, nombreArchivoLayout);

                        while (reintentosCount < retries)
                        {
                            System.Threading.Thread.Sleep(waitMileSecond);

                            if (File.Exists(archivoPacSalida))
                            {
                                archivoProcesado = true;

                                try
                                {
                                    XmlDocument documentoXML = new XmlDocument();
                                    documentoXML.Load(archivoPacSalida);
                                    var xmlElementTimbre = (XmlElement)documentoXML.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);
                                    if (xmlElementTimbre != null)
                                    {
                                        cfdiReturn = xmlElementTimbre.GetAttribute("UUID");
                                    }
                                    else
                                    {
                                        cfdiReturn = string.Format("ERROR: al tratar de obtener el UUID del archivo:{0}, error: No se encontro el elemento UUID en el XML", archivoPacSalida);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    cfdiReturn = string.Format("ERROR: al tratar de obtener el UUID del archivo:{0}, error:{1}", archivoPacSalida, ex.Message);
                                    break;
                                }
                                break;
                            }

                            reintentosCount++;
                        }

                        if (!archivoProcesado)
                        {
                            cfdiReturn = String.Format("ERROR No se encontro archivo de salida del PAC {0}, despues de {1} intentos", archivoPacSalida, reintentos);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        cfdiReturn = String.Format("ERROR No se tiene acceso para escribir en {0}", folderEntradaPAC);
                    }
                    catch (PathTooLongException)
                    {
                        cfdiReturn = String.Format("ERROR longitud de ruta demasiado larga del folder entrada PAC {0}", folderEntradaPAC);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        cfdiReturn = String.Format("ERROR No se encontro folder entrada PAC {0}", folderEntradaPAC);
                    }
                    catch (Exception ex)
                    {
                        cfdiReturn = String.Format("ERROR al tratar de copiar el archivo al folder {0}, error: {1}", folderEntradaPAC, ex.Message);
                    }
                }
            }
            else
            {
                cfdiReturn = String.Format("ERROR No se encontro archivo {0}", archivoXML);
            }
            return cfdiReturn;
            #endregion
        }

        internal string TimbrarPACBuzonE(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userId, string userPass, string urlTimbrado, string referencia)
        {
            #region TimbrarPACBuzonE
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;
            string cfdiReturn = string.Empty;
            string cfd = string.Empty;
            string layout = string.Empty;
            string respuestaServicio = string.Empty;
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri uriServicio = new Uri(urlTimbrado);
            EndpointAddress endpoint = new EndpointAddress(uriServicio);
            Comprobante.Utilidades util = new Comprobante.Utilidades();
            XmlDocument documentoXML = new XmlDocument();
            string uuid = string.Empty;

            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            if (File.Exists(archivoXML))
            {
                switch (uriServicio.Scheme)
                {
                    case "http":
                        SetBinding(binding, BasicHttpSecurityMode.None);
                        break;
                    case "https":
                        SetBinding(binding, BasicHttpSecurityMode.Transport);
                        break;
                    default:
                        SetBinding(binding);
                        break;
                }

                var servicio = new BuzonE.TimbradoCFDClient(binding, endpoint);

                ValidarEsquema(archivoXML);

                if (errorEnEsquema)
                {
                    return String.Format("ERROR EN ESQUEMA {0}", sb.ToString());
                }
                else
                {
                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);

                    try
                    {
                        var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                        layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);

                        if (versionCFDI == "3.3")
                        {
                            var timbradoRequest = new BuzonE.TimbradoRequest()
                            {
                                usuario = userId,
                                password = userPass,
                                returnCFDTimbrado = 1,
                                xmlCFD = layout
                            };
                            var timbradoResponse = servicio.timbradoCFD(timbradoRequest);

                            try
                            {
                                if (timbradoResponse.datosAdicionales.status.ToUpperInvariant().Equals("OK", StringComparison.InvariantCulture))
                                {
                                    uuid = timbradoResponse.UUID;

                                    // Generar XML fimado
                                    if (timbradoResponse.xmlCFDTimbrado != null)
                                    {
                                        var strXml = new StringBuilder();
                                        foreach (var nodo in ((XmlNode[])timbradoResponse.xmlCFDTimbrado))
                                        {
                                            if (nodo.InnerText.Length >= 1)
                                            {
                                                strXml.Append(nodo.InnerText);
                                            }
                                        }
                                        File.WriteAllText(rutaArchivoLayout + "\\" + uuid + ".xml", strXml.ToString());

                                        //documentoXML.LoadXml(strXml.ToString());
                                        //documentoXML.Save(rutaArchivoLayout + "\\" + uuid + ".xml");
                                    }
                                }
                                else
                                {
                                    uuid = "ERROR: " + (timbradoResponse.datosAdicionales?.errorList?.Count() > 0 ?
                                        string.Join(",", timbradoResponse.datosAdicionales.errorList.Select(e => e.message)) :
                                        timbradoResponse.datosAdicionales.status);

                                    CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                                }
                            }
                            catch (Exception exCall)
                            {
                                uuid = "ERROR: " + exCall.Message;
                                CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                            }
                        }
                        else
                        {
                            uuid = "ERROR: Otras versiones de CFDI diferentes a 3.3 no es soportada";
                        }

                        return uuid;
                    }
                    catch (Exception ex)
                    {
                        CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        cfdiReturn = String.Format("ERROR {0}, stacktrace {1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            else
            {
                cfdiReturn = String.Format("ERROR No se encontro archivo {0}", layout);
            }
            return cfdiReturn;
            #endregion
        }
        
        internal string TimbrarPACSW(string archivoXML, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userId, string userPass, string urlTimbrado, string referencia)
        {

            if (userId == "PDF")
            {
                return PDFSW(archivoXML, archivoXslt, archivoCertificado, urlTimbrado, archivoKey, userId, referencia);
            }
            else if (userId == "EMAIL")
            {
                //archivoXML = Ruta del archivo de xml para extrar la ruta de la carpeta
                return EnviarCorreo(archivoXML);
            }
            else
            {
                sError = "Entrando a Timbrar: PAC SW - ";
                string uuid = string.Empty;
                string layout = string.Empty;
                Uri uriServicio = new Uri(urlTimbrado);
                Comprobante.Utilidades util = new Comprobante.Utilidades();
                string token = string.Empty;
                string rutaArchivoLayout = string.Empty;
                string nombreArchivoLayout = string.Empty;
                sError += "Definición de Variables Correcta - ";

                try
                {

                    rutaArchivoLayout = Path.GetDirectoryName(archivoXML);
                    nombreArchivoLayout = Path.GetFileName(archivoXML);

                    var versionCFDI = util.ObtenerVersinCFDI(archivoXML);

                    sError += "Versión de CFDI - ";

                    if (versionCFDI == "3.3" || versionCFDI == "4.0")
                    {
                        layout = util.AgregarSello(archivoXML, archivoXslt, archivoCertificado, archivoKey, certificadoPass, versionCFDI);

                        sError += "Generación de Sello - ";
                        sError += String.Format("URL: {0}, User: {1}, Pass: {2}", uriServicio.AbsoluteUri.Replace(uriServicio.AbsolutePath, ""), userId, userPass);
                        //Authentication auth = new Authentication(uriServicio.AbsoluteUri.Replace(uriServicio.AbsolutePath, ""), userId, userPass);
                        //AuthResponse authResponse = auth.GetToken();
                        //token = authResponse.data.token;
                        token = referencia;
                        sError += "Autentifación obtenida - ";
                        var client = new RestClient(urlTimbrado);
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Authorization", string.Format("bearer {0}", token));
                        request.AddHeader("Content-Type", "multipart/form-data; boundary=\"----=_Part_11_11939969.1490230712432\"");
                        var body = string.Format(@"------=_Part_11_11939969.1490230712432
Content-Type: text/xml
Content-Transfer-Encoding: binary
Content-Disposition: form-data; name=xml; filename=xml

{0}
------=_Part_11_11939969.1490230712432--", layout);

                        request.AddParameter("multipart/form-data; boundary=\"---- = _Part_11_11939969.1490230712432\"", body, ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);

                        sError += "Timbrado - ";

                        var jsonData = new JObject();

                        if (response.IsSuccessful)
                        {
                            sError += "Response - Successful: " + response.Content;

                            jsonData = JObject.Parse(response.Content);
                            var t = (string)jsonData.SelectToken("data").SelectToken("cfdi");

                            XmlDocument xml = new XmlDocument();
                            xml.LoadXml(t);

                            var xmlElementTimbre = (XmlElement)xml.GetElementsByTagName("tfd:TimbreFiscalDigital").Item(0);

                            if (xmlElementTimbre == null)
                            {
                                uuid = string.Format("ERROR No se encontro el elemento TimbreFiscalDigital, Detalle: {0}", response.ErrorMessage);
                                CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                            }
                            else
                            {
                                uuid = xmlElementTimbre.GetAttribute("UUID");
                                xml.Save(rutaArchivoLayout + "\\" + uuid + ".xml");
                                util.ArchivoCadenaOriginal(archivoXML, archivoXslt, uuid, rutaArchivoLayout);

                            }
                            uuid = xmlElementTimbre.GetAttribute("UUID");

                        }
                        else
                        {
                            sError += "Response - Unsuccessful";
                            File.WriteAllText(string.Format("{0}\\ERRORES_{1}.txt", rutaArchivoLayout, Path.GetFileNameWithoutExtension(archivoXML)), response.Content);
                            uuid = string.Format("ERROR Codigo HTTP {0}, Mensaje Error: {1}, Error Detallado: {2}", (int)response.StatusCode, (string)jsonData.SelectToken("message"), (string)jsonData.SelectToken("messageDetail"));
                            CrearXMLConError(layout, rutaArchivoLayout, nombreArchivoLayout);
                        }
                    }
                    else
                    {
                        uuid = "ERROR: Otras versiones de CFDI diferentes a 3.3 no es soportada";
                    }

                }
                catch (Exception ex)
                {
                    uuid = string.Format("ERROR {0}, stacktrace {1}, Log = {2}",
                        string.Format("Mensaje: {0}, InnerException: {1} ", ex.Message, (ex.InnerException != null ? ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message : "")),
                         ex.StackTrace, sError);
                }
                return uuid;
            }
        }

        public string PDFSW(string xmlTimbrado, string uuid, string logo, string urlTimbrado, string urlPDF, string userId, string userPass) {

            string messageResp = "";
            string token = "";
            string logoB64= "";
            string layout = string.Empty;
            string rutaArchivoLayout = string.Empty;
            string nombreArchivoLayout = string.Empty;

            rutaArchivoLayout = Path.GetDirectoryName(xmlTimbrado);

            //Uri uriServicio = new Uri(urlTimbrado);
            //Authentication auth = new Authentication(uriServicio.AbsoluteUri.Replace(uriServicio.AbsolutePath, ""), userId, userPass);
            //AuthResponse authResponse = auth.GetToken();
            //token = authResponse.data.token;
            token = userPass;

            if (File.Exists(xmlTimbrado))
            {
                using (StreamReader objReader = new StreamReader(xmlTimbrado, Encoding.UTF8))
                {
                    xmlTimbrado = objReader.ReadToEnd();
                    objReader.Close();
                }
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlTimbrado);

            layout = xmlDoc.InnerXml;

            if (File.Exists(logo))
            {
                logoB64 = Utilerias.ManejoArchivos.FileToBase64(logo);
            }

            pdfSW pdfse = new pdfSW();
            pdfse.xmlContent = layout;
            pdfse.templateId = "cfdi40";
            pdfse.logo = logoB64;

            string json = JsonConvert.SerializeObject(pdfse);

            var client = new RestClient(urlPDF);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", string.Format("bearer {0}", token));
            request.AddHeader("Content-Type", "application/json; ");
            var body = string.Format(@"{0}", json);
            request.AddParameter("application/json; ", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            var jsonData = new JObject();
            jsonData = JObject.Parse(response.Content);

            if (response.IsSuccessful)
            {

                var t = (string)jsonData.SelectToken("data").SelectToken("contentB64");
                messageResp = rutaArchivoLayout + "\\" + uuid + ".pdf";
                Utilerias.ManejoArchivos.Base64ToFile(t, messageResp);
            }
            else
            {
                messageResp = string.Format("ERROR Codigo HTTP {0}, Mensaje Error: {1}, Error Detallado: {2}", (int)response.StatusCode, (string)jsonData.SelectToken("message"), (string)jsonData.SelectToken("messageDetail"));
                
            }

            return messageResp;
        }

        public string EnviarCorreo(string xmlfilepath)
        {
            EmailSettings mail = new EmailSettings();
            return mail.getEmailData(xmlfilepath);
        }

        public string CrearEkomercioPDF(string emisorRFC, string uuid, string rutaNombreArchivoPDF)
        {
            #region CrearEkomercioPDF
            string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string urlPDF = Utilerias.ManejoArchivos.ObtenerParametroArchivoIni(currentAssemblyDirectoryName, "URLPDF");
            string rutaPDFCreado = string.Empty;

            if (!string.IsNullOrEmpty(urlPDF))
            {
                rutaPDFCreado = string.Format(@"{0}\{1}.pdf", currentAssemblyDirectoryName, uuid);
                urlPDF = string.Format("{0}{1}_{2}.PDF", urlPDF, emisorRFC, uuid);

                try
                {
                    using (var client = new WebDownload())
                    {
                        client.DownloadFile(urlPDF, rutaNombreArchivoPDF);
                    }
                }
                catch (Exception ex)
                {
                    return string.Format("ERROR {0}, STACKTRACE {1}", ex.Message, ex.StackTrace);
                }
            }

            return rutaPDFCreado;
            #endregion
        }

        private Boolean ValidarCertificado(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void SetBinding(BasicHttpBinding binding)
        {
            #region setBinding
            SetBinding(binding, BasicHttpSecurityMode.Transport);
            #endregion
        }

        private void SetBinding(BasicHttpBinding binding, BasicHttpSecurityMode modo)
        {
            #region SetBinding
            binding.CloseTimeout = System.TimeSpan.Parse("00:02:30");
            binding.OpenTimeout = System.TimeSpan.Parse("00:02:30");
            binding.ReceiveTimeout = System.TimeSpan.Parse("00:02:30");
            binding.SendTimeout = System.TimeSpan.Parse("00:02:30");
            binding.AllowCookies = true;
            binding.BypassProxyOnLocal = true;
            binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
            binding.MaxBufferSize = 524288;
            binding.MaxBufferPoolSize = 524288;
            binding.MaxReceivedMessageSize = 524288;
            binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
            binding.TextEncoding = System.Text.Encoding.UTF8;
            binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.ReaderQuotas.MaxDepth = 32;
            binding.ReaderQuotas.MaxStringContentLength = 5242880;
            binding.ReaderQuotas.MaxArrayLength = 16384;
            binding.ReaderQuotas.MaxBytesPerRead = 4096;
            binding.ReaderQuotas.MaxNameTableCharCount = 16384;
            binding.Security.Mode = modo;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Transport.Realm = "";
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
            #endregion
        }
    }

    public class Cancelado
    {
        public Resultados Cancelar(requestCancelarCFDI RequestCancelarCFDI, string uuid)
        {
            #region Cancelar
            Resultados result = new Resultados();

            try
            {
                RequestCancelarCFDI.uuid = uuid;

                // set this before any web requests or WCF calls
                if (RequestCancelarCFDI.proxy_url != "")
                {
                    WebRequest.DefaultWebProxy = new WebProxy(new Uri(RequestCancelarCFDI.proxy_url + ":" + RequestCancelarCFDI.proxy_port))
                    {
                        Credentials = new NetworkCredential(RequestCancelarCFDI.proxy_user, RequestCancelarCFDI.proxy_pass),
                    };
                }

                //  Conexion con el WS de Facturacion Moderna
                BasicHttpBinding binding = new BasicHttpBinding();
                setBinding(binding);

                // Direccion del servicio SOAP de Prueba
                EndpointAddress endpoint = new EndpointAddress(RequestCancelarCFDI.urlCancelado);

                // Crear instancia al servisio SOAP de cancelado
                Timbrado_ManagerPort WSFModerna = new Timbrado_ManagerPortClient(binding, endpoint);

                // Ejecutar servicio de Cancelado
                Object response = null;//WSFModerna.requestCancelarCFDI(RequestCancelarCFDI);
                if (response != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlDeclaration xmlDeclaration;
                    XmlElement xmlElementBody;
                    xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "uft-8", "no");
                    xmlElementBody = xmlDoc.CreateElement("Container");
                    xmlDoc.InsertBefore(xmlElementBody, xmlDoc.DocumentElement);
                    XmlElement xmlParentNode;
                    xmlParentNode = xmlDoc.CreateElement("responseSoap");
                    xmlDoc.DocumentElement.PrependChild(xmlParentNode);
                    XmlNode[] nodosXmlResponse = (XmlNode[])response;
                    foreach (XmlNode nodo in nodosXmlResponse)
                    {
                        if (nodo.InnerText.Length >= 1)
                        {
                            XmlElement xmlElemetResponse;
                            xmlElemetResponse = xmlDoc.CreateElement(nodo.Name.ToString());
                            XmlText xmlTextNode;
                            xmlTextNode = xmlDoc.CreateTextNode(nodo.InnerText.ToString());
                            xmlParentNode.AppendChild(xmlElemetResponse);
                            xmlElemetResponse.AppendChild(xmlTextNode);
                        }
                    }
                    XmlElement xmlElementMsg = (XmlElement)xmlDoc.GetElementsByTagName("Message").Item(0);
                    result.message = xmlElementMsg.InnerText;
                    result.code = "C000";
                    result.status = true;
                    return result;
                }
                else
                {
                    result.code = "C00N";
                    result.message = "El servicio de Cancelado respondio con NULL";
                    result.status = false;
                    return result;
                }
            }
            catch (Exception e)
            {
                result.code = "EX-001";
                result.message = "Error: " + e.Message;
                result.status = false;
                return result;
            }
            #endregion 
        }

        private void setBinding(BasicHttpBinding binding)
        {
            #region setBinding
            // Crear archivo app.config de forma manual
            binding.Name = "Timbrado_ManagerBinding";
            binding.CloseTimeout = System.TimeSpan.Parse("00:01:00");
            binding.OpenTimeout = System.TimeSpan.Parse("00:01:00");
            binding.ReceiveTimeout = System.TimeSpan.Parse("00:10:00");
            binding.SendTimeout = System.TimeSpan.Parse("00:01:00");
            binding.AllowCookies = true;
            binding.BypassProxyOnLocal = true;
            binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
            binding.MaxBufferSize = 524288;
            binding.MaxBufferPoolSize = 524288;
            binding.MaxReceivedMessageSize = 524288;
            binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
            binding.TextEncoding = System.Text.Encoding.UTF8;
            binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.ReaderQuotas.MaxDepth = 32;
            binding.ReaderQuotas.MaxStringContentLength = 8192;
            binding.ReaderQuotas.MaxArrayLength = 16384;
            binding.ReaderQuotas.MaxBytesPerRead = 4096;
            binding.ReaderQuotas.MaxNameTableCharCount = 16384;
            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Transport.Realm = "";
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
            #endregion
        }
    }

    public class Activacion
    {
        public Resultados ActivarCancelado(activarCancelacion activarCancelacion)
        {
            #region ActivarCancelado
            Resultados result = new Resultados();
            string cer = activarCancelacion.archivoCer;
            string key = activarCancelacion.archivoKey;
            string clv = activarCancelacion.clave;
            try
            {
                if (File.Exists(cer))
                {
                    X509Certificate2 cert = new X509Certificate2(cer);
                    cer = Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks);
                    cer = cer.Replace("\n", "");
                }
                else
                    // Codificar a base 64 el contenido del certificado
                    cer = Convert.ToBase64String(Encoding.UTF8.GetBytes(cer));
                // Agregar el certificado codificado en base64 a la peticion SOAP
                activarCancelacion.archivoCer = cer;

                if (File.Exists(key))
                {
                    byte[] llavePrivadaBytes = System.IO.File.ReadAllBytes(@key);
                    key = Convert.ToBase64String(llavePrivadaBytes);

                }
                else
                    // Codificar a base 64 el contenido del archivo  key
                    key = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));
                // Agregar el certificado codificado en base64 a la peticion SOAP
                activarCancelacion.archivoKey = key;

                //  Conexion con el WS de Facturacion Moderna
                BasicHttpBinding binding = new BasicHttpBinding();
                setBinding(binding);

                // Direccion del servicio SOAP de Prueba
                EndpointAddress endpoint = new EndpointAddress(activarCancelacion.urlActivarCancelacion);

                // Crear instancia al servisio SOAP de cancelado
                Timbrado_ManagerPort WSFModerna = new Timbrado_ManagerPortClient(binding, endpoint);

                // Ejecutar servicio de Cancelado
                Object response = null; // WSFModerna.activarCancelacion(activarCancelacion);
                if (response != null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    XmlDeclaration xmlDeclaration;
                    XmlElement xmlElementBody;
                    xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "uft-8", "no");
                    xmlElementBody = xmlDoc.CreateElement("Container");
                    xmlDoc.InsertBefore(xmlElementBody, xmlDoc.DocumentElement);
                    XmlElement xmlParentNode;
                    xmlParentNode = xmlDoc.CreateElement("responseSoap");
                    xmlDoc.DocumentElement.PrependChild(xmlParentNode);
                    XmlNode[] nodosXmlResponse = (XmlNode[])response;
                    foreach (XmlNode nodo in nodosXmlResponse)
                    {
                        if (nodo.InnerText.Length >= 1)
                        {
                            XmlElement xmlElemetResponse;
                            xmlElemetResponse = xmlDoc.CreateElement(nodo.Name.ToString());
                            XmlText xmlTextNode;
                            xmlTextNode = xmlDoc.CreateTextNode(nodo.InnerText.ToString());
                            xmlParentNode.AppendChild(xmlElemetResponse);
                            xmlElemetResponse.AppendChild(xmlTextNode);
                        }
                    }
                    XmlElement xmlElementMsg = (XmlElement)xmlDoc.GetElementsByTagName("mensaje").Item(0);
                    XmlElement xmlElementCode = (XmlElement)xmlDoc.GetElementsByTagName("codigo").Item(0);
                    result.message = xmlElementMsg.InnerText;
                    result.code = xmlElementCode.InnerText;
                    result.status = true;
                    return result;
                }
                else
                {
                    result.code = "C00N";
                    result.message = "El servicio de Cancelado respondio con NULL";
                    result.status = false;
                    return result;
                }
            }
            catch (Exception e)
            {
                result.code = "EX-001";
                result.message = "Error: " + e.Message;
                result.status = false;
                return result;
            }
            #endregion
        }

        private void setBinding(BasicHttpBinding binding)
        {
            #region setBinding
            // Crear archivo app.config de forma manual
            binding.Name = "Timbrado_ManagerBinding";
            binding.CloseTimeout = System.TimeSpan.Parse("00:01:00");
            binding.OpenTimeout = System.TimeSpan.Parse("00:01:00");
            binding.ReceiveTimeout = System.TimeSpan.Parse("00:10:00");
            binding.SendTimeout = System.TimeSpan.Parse("00:01:00");
            binding.AllowCookies = true;
            binding.BypassProxyOnLocal = true;
            binding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
            binding.MaxBufferSize = 524288;
            binding.MaxBufferPoolSize = 524288;
            binding.MaxReceivedMessageSize = 524288;
            binding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
            binding.TextEncoding = System.Text.Encoding.UTF8;
            binding.TransferMode = System.ServiceModel.TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;
            binding.ReaderQuotas.MaxDepth = 32;
            binding.ReaderQuotas.MaxStringContentLength = 8192;
            binding.ReaderQuotas.MaxArrayLength = 16384;
            binding.ReaderQuotas.MaxBytesPerRead = 4096;
            binding.ReaderQuotas.MaxNameTableCharCount = 16384;
            binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Transport.Realm = "";
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
            #endregion
        }
    }

}
