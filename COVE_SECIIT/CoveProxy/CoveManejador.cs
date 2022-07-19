using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Globalization;
using System.Data;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Net;
using CoveProxy.CoveServicio;
using System.Xml;
using System.Xml.Serialization;
using System.Security;
using Utilerias;

namespace CoveProxy
{
    [ComVisible(true)]
    [Guid("02262F3D-DB20-4EA0-B894-E76D0FD2B356")]
    [ProgId("ZOE.COVE")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CoveManejador : CoveProxy.ICoveManejador
    {
        private SimpleEventLogSpace.SimpleEventLog log;
        private string firmaEnCadena = string.Empty;
        const string EN_CONTINGENCIA = "CONTINGENCIA";
        //const string URLNS = "http://www.ventanillaunica.gob.mx/cove/ws/service/";
        const string URLNS = "http://schemas.xmlsoap.org/soap/envelope/";
        const short INDICADOR_CONTINGENCIA = 3;
        Utilerias.ProxyInfo _Proxy = null;

        public CoveManejador()
        {
            //log = new SimpleEventLogSpace.SimpleEventLog("Cove");
        }

        #region Metodos para el manejo del COVE
        internal BasicHttpBinding GenerarBinding()
        {
            #region GenerarBinding
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxBufferSize = 2147483647;
            binding.TransferMode = TransferMode.Buffered;
            binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            binding.ReaderQuotas.MaxArrayLength = 2147483647;
            binding.ReceiveTimeout = new TimeSpan(0, 2, 0);
            binding.OpenTimeout = new TimeSpan(0, 2, 0);
            binding.CloseTimeout = new TimeSpan(0, 2, 0);
            binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.UseDefaultWebProxy = true;
            binding.AllowCookies = false;
            binding.BypassProxyOnLocal = false;
            //binding.TextEncoding = Encoding.Unicode;

            if (!String.IsNullOrEmpty(_Proxy.Url))
            {
                /* Forma anterior sin usuario y pass
                binding.UseDefaultWebProxy = false;
                binding.ProxyAddress = new Uri(_urlProxy);
                */

                //binding.UseDefaultWebProxy = true;
                //binding.ProxyAddress = new Uri(_Proxy.Url);

                WebProxy webProxy = new WebProxy(_Proxy.Url);
                webProxy.Credentials = new NetworkCredential(_Proxy.Usuario, _Proxy.Password);
                WebRequest.DefaultWebProxy = webProxy;
            }

            return binding;
            #endregion
        }

        internal CoveServicio.ReceptorClient ObtenerServicioWeb(string urlServicio, string userName, string passwordServicioWeb)
        {
            #region Obtener el proxy del servicio web del SAT
            Uri uriServicio = new Uri(urlServicio);
            CoveServicio.ReceptorClient servicio = null;

            BasicHttpBinding binding = GenerarBinding();
            servicio = new CoveServicio.ReceptorClient(binding, new EndpointAddress(uriServicio));

            servicio.ClientCredentials.UserName.UserName = userName;
            servicio.ClientCredentials.UserName.Password = passwordServicioWeb;

            BindingElementCollection elements = servicio.Endpoint.Binding.CreateBindingElements();
            elements.Find<SecurityBindingElement>().IncludeTimestamp = false;
            servicio.Endpoint.Binding = new CustomBinding(elements);

            return servicio;
            #endregion
        }

        internal string CrearMensajeRespuesta(CoveRespuestaServicio.RespuestaPeticion respuesta)
        {
            #region Crear mensaje de respuesta
            string mensajeRespuesta = string.Empty;
            string indicadorError = "0";

            if (respuesta.respuestasOperaciones != null)
            {
                if (respuesta.respuestasOperaciones.Length > 0 && respuesta.respuestasOperaciones[0].contieneError)
                    indicadorError = "1";

                mensajeRespuesta = string.Format("|{0}|{1}|{2}|", indicadorError, respuesta.numeroOperacion,
                    respuesta.horaRecepcion.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture));

                if (respuesta.respuestasOperaciones.Length > 0)
                {
                    mensajeRespuesta = mensajeRespuesta + string.Format("{0}|{1}|{2}|{3}|", respuesta.respuestasOperaciones[0].numeroFacturaORelacionFacturas,
                        respuesta.respuestasOperaciones[0].eDocument, respuesta.leyenda, firmaEnCadena);

                    if (respuesta.respuestasOperaciones[0].errores != null)
                    {
                        foreach (var item in respuesta.respuestasOperaciones[0].errores)
                        {
                            mensajeRespuesta += item + "|";
                        }
                    }
                }
            }
            else
            {
                indicadorError = "2";

                mensajeRespuesta = string.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|", indicadorError, respuesta.numeroOperacion,
                    respuesta.horaRecepcion.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture), "", "", respuesta.leyenda, firmaEnCadena);
            }

            return mensajeRespuesta;
            #endregion
        }

        internal void AgregarTagEdocument(CoveServicio.ReceptorClient servicio, string eDocument, string urlNS)
        {
            #region Agregar Tag eDocument
            using (new OperationContextScope(servicio.InnerChannel))
            {
                MessageHeader headereDocument = MessageHeader.CreateHeader("e-document", urlNS, eDocument);
                OperationContext.Current.OutgoingMessageHeaders.Add(headereDocument);
            }
            #endregion
        }

        [ComVisible(true)]
        public string EnviarComprobante(string archivoCove, string userName, string passwordServicioWeb, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio, string urlServicioRespuesta)
        {
            #region Envio de comprovandes
            string respuesta = string.Empty;
            CoveServicio.ReceptorClient servicio = null;
            CoveServicio.Acuse acuseRecibo = null;

            _Proxy = Utilerias.ManejoArchivos.ObtenerURLProxy(rutaCertificadoP12);

            try
            {
                CoveServicio.ComprobanteValorElectronico[] coves = new CoveServicio.ComprobanteValorElectronico[1];

                CoveServicio.ComprobanteValorElectronico cove = LeerCove(archivoCove);

                cove.firmaElectronica.certificado = Utilerias.ManejoArchivos.FileToByteArray(rutaCertificadoCert);

                Encriptador encriptador = new Encriptador();
                cove.firmaElectronica.firma = encriptador.RSASignToByteArray(cove.firmaElectronica.cadenaOriginal, rutaCertificadoP12, passwordCertificado);
                //cove.firmaElectronica.firma = encriptador.RSASignToByteArraySHA256(cove.firmaElectronica.cadenaOriginal, rutaCertificadoP12, passwordCertificado);
                firmaEnCadena = Convert.ToBase64String(cove.firmaElectronica.firma);

                ManejoArchivos.BorrarArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG);

                ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, $"Creando archivo XML firmado");

                CrearArchivoXMLFirmado(cove);

                if (urlServicio.ToUpper() == EN_CONTINGENCIA)
                {
                    //respuesta = string.Format("{0}|{1}", INDICADOR_CONTINGENCIA, firmaEnCadena);
                    respuesta = string.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|", INDICADOR_CONTINGENCIA, "", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture), "", "", "", firmaEnCadena);
                }
                else
                {
                    coves[0] = cove;

                    servicio = ObtenerServicioWeb(urlServicio, userName, passwordServicioWeb);

                    using (new OperationContextScope(servicio.InnerChannel))
                    {
                        //Si tiene valoe cove.edocument, se trata de un adendum (adecuacion a un cove existente
                        if (!string.IsNullOrEmpty(cove.edocument))
                        {

                            MessageHeader headereDocument = MessageHeader.CreateHeader("e-document", URLNS, cove.edocument);
                            OperationContext.Current.OutgoingMessageHeaders.Add(headereDocument);
                        }

                        ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, $"Invocando servicio web Recibir Cove");
                        acuseRecibo = servicio.RecibirCove(coves);
                        var mensajeResultado = respuesta.Contains("ERROR") ? "Con error" : "Sin error";
                        ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, $"Servicio web Recibir Cove finalizo llamada {mensajeResultado}");
                    }

                    System.Threading.Thread.Sleep(3000);

                    ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, $"Invocando servicio web Obtener Respuesta");
                    respuesta = ObtenerRespuesta(userName, passwordServicioWeb, acuseRecibo.numeroDeOperacion, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicioRespuesta);
                    ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, $"Servicio web Obtener Respuesta finalizado, respuesta: {respuesta}");

                    if (respuesta.Contains("ERROR"))
                    {
                        ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, string.Format("Ocurrio un error al tratar de obtener la respuesta, error:{0}", respuesta));
                        Exception excepRespuesta = new Exception(string.Format("Ocurrio un error al tratar de obtener la respuesta, error:{0}", respuesta));
                        throw excepRespuesta;
                    }
                }
            }
            catch (FaultException faultExt)
            {
                if (servicio != null)
                    servicio.Abort();

                ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, String.Format("FaultAction:{0} FaultCode:{1} FaultReason:{2} FaultMessage:{3}", faultExt.Action, faultExt.Code, faultExt.Reason, faultExt.Message));

                //if (faultExt.InnerException != null)
                //log.WriteError(faultExt.InnerException.Message);
                respuesta = string.Format("ERROR {0}, STACKTRACE {1}", faultExt.Message, faultExt.StackTrace);
            }
            catch (Exception ex)
            {
                if (servicio != null)
                    servicio.Abort();

                //log.WriteError(ex.Message);
                //WriteLogFile(ex.Message);

                ManejoArchivos.EscribirArchivoLog(ManejoArchivos.NOMBRE_ARCHIVO_LOG, ex.Message);

                respuesta = string.Format("ERROR {0}, STACKTRACE {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (servicio != null)
                    servicio.Close();
            }
            return respuesta;
            #endregion
        }

        public void CrearArchivoXMLFirmado(ComprobanteValorElectronico cove)
        {
            try
            {
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string xmlFileName = Path.Combine(assemblyFolder, string.Format("{0}_{1}.xml", cove.emisor.identificacion, System.DateTime.Now.Ticks));

                CrearArchivoXMLFirmado(cove, xmlFileName);
            }
            catch (Exception)
            {
                
            }
        }

        public void CrearArchivoXMLFirmado(ComprobanteValorElectronico cove, string ruta)
        {
            XmlDocument xmlCove = new XmlDocument();
            XmlSerializer ser = new XmlSerializer(cove.GetType());

            try
            {
                using (MemoryStream memStm = new MemoryStream())
                {
                    ser.Serialize(memStm, cove);

                    memStm.Position = 0;

                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;

                    using (var xtr = XmlReader.Create(memStm, settings))
                    {
                        xmlCove.Load(xtr);
                    }

                    xmlCove.Save(ruta); //@"c:\temp\cove.xml"
                }
            }
            catch (Exception)
            {
                
            }
        }

        [ComVisible(true)]
        public string EnviarFacturasNOIA(string archivoFacturasNOIA, string userName, string passwordServicioWeb, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio, string urlServicioRespuesta)
        {
            #region Envio de facturas que no son de Industria Automotriz
            //WriteLogFile("Iniciando");

            string respuesta = string.Empty;
            CoveServicio.ReceptorClient servicio = null;
            CoveServicio.Acuse acuseRecibo = null;

            _Proxy = Utilerias.ManejoArchivos.ObtenerURLProxy(rutaCertificadoP12);

            try
            {
                CoveServicio.ComprobanteValorElectronicoNoIA[] coves = new CoveServicio.ComprobanteValorElectronicoNoIA[1];

                CoveServicio.ComprobanteValorElectronicoNoIA cove = LeerFacturasNOIA(archivoFacturasNOIA);

                cove.firmaElectronica.certificado = Utilerias.ManejoArchivos.FileToByteArray(rutaCertificadoCert);

                Encriptador encriptador = new Encriptador();
                cove.firmaElectronica.firma = encriptador.RSASignToByteArray(cove.firmaElectronica.cadenaOriginal, rutaCertificadoP12, passwordCertificado);
                firmaEnCadena = Convert.ToBase64String(cove.firmaElectronica.firma);

                if (urlServicio.ToUpper() == EN_CONTINGENCIA)
                {
                    //respuesta = string.Format("{0}|{1}", INDICADOR_CONTINGENCIA, firmaEnCadena);
                    respuesta = string.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|", INDICADOR_CONTINGENCIA, "", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture), "", "", "", firmaEnCadena);
                }
                else
                {
                    coves[0] = cove;

                    servicio = ObtenerServicioWeb(urlServicio, userName, passwordServicioWeb);

                    using (new OperationContextScope(servicio.InnerChannel))
                    {
                        //Si tiene valor cove.edocument, se trata de un adendum (adecuacion a un cove existente
                        if (!string.IsNullOrEmpty(cove.edocument))
                        {

                            MessageHeader headereDocument = MessageHeader.CreateHeader("e-document", URLNS, cove.edocument);
                            OperationContext.Current.OutgoingMessageHeaders.Add(headereDocument);
                        }
                        acuseRecibo = servicio.RecibirRelacionFacturasNoIA(coves);
                    }

                    respuesta = ObtenerRespuesta(userName, passwordServicioWeb, acuseRecibo.numeroDeOperacion, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicioRespuesta);

                    if (respuesta.Contains("ERROR"))
                    {
                        Exception excepRespuesta = new Exception(string.Format("Ocurrio un error al tratar de obtener la respuesta, error:{0}", respuesta));
                        throw excepRespuesta;
                    }
                }
            }
            catch (FaultException faultExt)
            {
                if (servicio != null)
                    servicio.Abort();
                //log.WriteError(String.Format("FaultAction:{0} FaultCode:{1} FaultReason:{2} FaultMessage:{3}", faultExt.Action, faultExt.Code, faultExt.Reason, faultExt.Message));

                //if (faultExt.InnerException != null)
                //log.WriteError(faultExt.InnerException.Message);
                respuesta = string.Format("ERROR {0}", faultExt.Message);

            }
            catch (Exception ex)
            {
                if (servicio != null)
                    servicio.Abort();
                //log.WriteError(ex.Message);
                //WriteLogFile(ex.Message);
                respuesta = string.Format("ERROR {0}", ex.Message);
            }
            finally
            {
                if (servicio != null)
                    servicio.Close();
            }
            return respuesta;
            #endregion
        }

        [ComVisible(true)]
        public string EnviarFacturasIA(string archivoFacturasIA, string userName, string passwordServicioWeb, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio, string urlServicioRespuesta)
        {
            #region Envio de facturas que son de Industria Automotriz
            //WriteLogFile("Iniciando");

            string respuesta = string.Empty;
            CoveServicio.ReceptorClient servicio = null;
            CoveServicio.Acuse acuseRecibo = null;

            _Proxy = Utilerias.ManejoArchivos.ObtenerURLProxy(rutaCertificadoP12);

            try
            {
                CoveServicio.ComprobanteValorElectronicoIA[] coves = new CoveServicio.ComprobanteValorElectronicoIA[1];

                CoveServicio.ComprobanteValorElectronicoIA cove = LeerFacturasIA(archivoFacturasIA);

                cove.firmaElectronica.certificado = Utilerias.ManejoArchivos.FileToByteArray(rutaCertificadoCert);

                Encriptador encriptador = new Encriptador();
                cove.firmaElectronica.firma = encriptador.RSASignToByteArray(cove.firmaElectronica.cadenaOriginal, rutaCertificadoP12, passwordCertificado);
                firmaEnCadena = Convert.ToBase64String(cove.firmaElectronica.firma);

                if (urlServicio.ToUpper() == EN_CONTINGENCIA)
                {
                    //respuesta = string.Format("{0}|{1}", INDICADOR_CONTINGENCIA, firmaEnCadena);
                    respuesta = string.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|", INDICADOR_CONTINGENCIA, "", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture), "", "", "", firmaEnCadena);
                }
                else
                {
                    coves[0] = cove;

                    servicio = ObtenerServicioWeb(urlServicio, userName, passwordServicioWeb);

                    using (new OperationContextScope(servicio.InnerChannel))
                    {
                        //Si tiene valor cove.edocument, se trata de un adendum (adecuacion a un cove existente
                        if (!string.IsNullOrEmpty(cove.edocument))
                        {

                            MessageHeader headereDocument = MessageHeader.CreateHeader("e-document", URLNS, cove.edocument);
                            OperationContext.Current.OutgoingMessageHeaders.Add(headereDocument);
                        }
                        acuseRecibo = servicio.RecibirRelacionFacturasIA(coves);
                    }

                    respuesta = ObtenerRespuesta(userName, passwordServicioWeb, acuseRecibo.numeroDeOperacion, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicioRespuesta);

                    if (respuesta.Contains("ERROR"))
                    {
                        Exception excepRespuesta = new Exception(string.Format("Ocurrio un error al tratar de obtener la respuesta, error:{0}", respuesta));
                        throw excepRespuesta;
                    }
                }
            }
            catch (FaultException faultExt)
            {
                if (servicio != null)
                    servicio.Abort();
                //log.WriteError(String.Format("FaultAction:{0} FaultCode:{1} FaultReason:{2} FaultMessage:{3}", faultExt.Action, faultExt.Code, faultExt.Reason, faultExt.Message));

                //if (faultExt.InnerException != null)
                //log.WriteError(faultExt.InnerException.Message);
                respuesta = string.Format("ERROR {0}", faultExt.Message);
            }
            catch (Exception ex)
            {
                if (servicio != null)
                    servicio.Abort();
                //log.WriteError(ex.Message);
                //WriteLogFile(ex.Message);
                respuesta = string.Format("ERROR {0}", ex.Message);
            }
            finally
            {
                if (servicio != null)
                    servicio.Close();
            }
            return respuesta;
            #endregion
        }

        [ComVisible(true)]
        public string ObtenerRespuesta(string userName, string password, string numeroOperacion, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio)
        {
            #region Consultar respuesta de la solicitud
            string respuesta = string.Empty;
            Encriptador encriptador = new Encriptador();
            Uri uriServicio = new Uri(urlServicio);
            CoveRespuestaServicio.ReceptorClient servicioRespuesta = null;
            CoveRespuestaServicio.RespuestaPeticion respuestaOperacion = new CoveRespuestaServicio.RespuestaPeticion();

            _Proxy = Utilerias.ManejoArchivos.ObtenerURLProxy(rutaCertificadoP12);
            BasicHttpBinding binding = GenerarBinding();

            try
            {
                servicioRespuesta = new CoveRespuestaServicio.ReceptorClient(binding, new EndpointAddress(uriServicio));

                servicioRespuesta.ClientCredentials.UserName.UserName = userName;
                servicioRespuesta.ClientCredentials.UserName.Password = password;

                BindingElementCollection elements = servicioRespuesta.Endpoint.Binding.CreateBindingElements();
                elements.Find<SecurityBindingElement>().IncludeTimestamp = false;
                servicioRespuesta.Endpoint.Binding = new CustomBinding(elements);

                CoveRespuestaServicio.SolicitarConsultarRespuestaCoveServicio datosSolicitud = new CoveRespuestaServicio.SolicitarConsultarRespuestaCoveServicio();
                datosSolicitud.firmaElectronica = new CoveRespuestaServicio.FirmaElectronica();
                datosSolicitud.firmaElectronica.cadenaOriginal = string.Format("|{0}|{1}|", numeroOperacion, userName);
                datosSolicitud.firmaElectronica.certificado = Utilerias.ManejoArchivos.FileToByteArray(rutaCertificadoCert);
                datosSolicitud.firmaElectronica.firma = encriptador.RSASignToByteArray(datosSolicitud.firmaElectronica.cadenaOriginal, rutaCertificadoP12, passwordCertificado); ;
                datosSolicitud.numeroOperacion = numeroOperacion;
                respuestaOperacion = servicioRespuesta.ConsultarRespuestaCove(datosSolicitud);
                respuesta = CrearMensajeRespuesta(respuestaOperacion);
            }
            catch (Exception ex)
            {
                if (servicioRespuesta != null)
                    servicioRespuesta.Abort();
                respuesta = String.Format("ERROR {0}", ex.Message);
            }
            finally
            {
                if (servicioRespuesta != null)
                    servicioRespuesta.Close();
            }
            return respuesta;
            #endregion
        }

        public CoveServicio.ComprobanteValorElectronico LeerCove(string archivoCove)
        {
            #region LeerCove
            DataSet dsCove = new DataSet();
            CoveServicio.ComprobanteValorElectronico cove = new CoveServicio.ComprobanteValorElectronico();

            try
            {
                var xmlFile = File.ReadAllText(archivoCove, Encoding.UTF8);                
                xmlFile = xmlFile.Replace("&", "&amp;");
                using (var txt = new StringReader(xmlFile))
                {
                    dsCove.ReadXml(txt);
                }                
            }
            catch (Exception ex)
            {
                Exception exXML = new Exception(string.Format("Error {0} al tratar de leer el archivo {1}", ex.Message, archivoCove), ex);
                throw exXML;
            }

            if (dsCove.Tables["comprobantes"].Rows.Count > 0)
            {
                cove.tipoFigura = Convert.ToInt32(dsCove.Tables["comprobantes"].Rows[0]["tipoFigura"].ToString());
                cove.fechaExpedicion = Convert.ToDateTime(dsCove.Tables["comprobantes"].Rows[0]["fechaExpedicion"].ToString(), CultureInfo.InvariantCulture);
                cove.observaciones = EscapeXMLValue(dsCove.Tables["comprobantes"].Rows[0]["observaciones"].ToString());
                cove.correoElectronico = EscapeXMLValue(dsCove.Tables["comprobantes"].Rows[0]["correoElectronico"].ToString());
                cove.numeroFacturaOriginal = dsCove.Tables["comprobantes"].Rows[0]["numeroFacturaOriginal"].ToString();
                cove.tipoOperacion = (CoveServicio.TipoOperacion)Convert.ToInt16(dsCove.Tables["comprobantes"].Rows[0]["tipoOperacion"]);

                if (dsCove.Tables["comprobantes"].Columns.Contains("edocument"))
                    cove.edocument = dsCove.Tables["comprobantes"].Rows[0]["edocument"].ToString();

                cove.firmaElectronica = new CoveServicio.FirmaElectronica();
                if (dsCove.Tables["firmaElectronica"].Rows.Count > 0)
                {
                    cove.firmaElectronica.cadenaOriginal = EscapeXMLValue(dsCove.Tables["firmaElectronica"].Rows[0]["cadenaOriginal"].ToString());
                }

                /* Patentes aduanales */
                if (dsCove.Tables.Contains("patentesAduanales") && dsCove.Tables["patentesAduanales"].Rows.Count > 0)
                {
                    cove.patenteAduanal = new string[dsCove.Tables["patentesAduanales"].Rows.Count];
                    int pos = 0;
                    foreach (DataRow item in dsCove.Tables["patentesAduanales"].Rows)
                    {
                        cove.patenteAduanal[pos] = item["patenteAduanal"].ToString();
                        pos = pos + 1;
                    }
                }
                else
                {
                    if (dsCove.Tables.Contains("patenteAduanal") && dsCove.Tables["patenteAduanal"].Rows.Count > 0)
                    {
                        cove.patenteAduanal = new string[dsCove.Tables["patenteAduanal"].Rows.Count];
                        int pos = 0;
                        foreach (DataRow item in dsCove.Tables["patenteAduanal"].Rows)
                        {
                            cove.patenteAduanal[pos] = item["patenteAduanal"].ToString();
                            pos = pos + 1;
                        }
                    }
                }

                /* RFC de consultas */
                if (dsCove.Tables.Contains("rfcConsultas") && dsCove.Tables["rfcConsultas"].Rows.Count > 0)
                {
                    cove.rfcConsulta = new string[dsCove.Tables["rfcConsultas"].Rows.Count];
                    int pos = 0;
                    foreach (DataRow item in dsCove.Tables["rfcConsultas"].Rows)
                    {
                        cove.rfcConsulta[pos] = item["rfcConsulta"].ToString();
                        pos = pos + 1;
                    }
                }
                else
                {
                    if (dsCove.Tables.Contains("rfcConsulta") && dsCove.Tables["rfcConsulta"].Rows.Count > 0)
                    {
                        cove.rfcConsulta = new string[dsCove.Tables["rfcConsulta"].Rows.Count];
                        int pos = 0;
                        foreach (DataRow item in dsCove.Tables["rfcConsulta"].Rows)
                        {
                            cove.rfcConsulta[pos] = item[0].ToString();
                            pos = pos + 1;
                        }
                    }
                }

                /* Datos de la factura */
                if (dsCove.Tables["factura"].Rows.Count > 0)
                {
                    cove.factura = new CoveServicio.FacturaCove();
                    cove.factura.certificadoOrigen = Convert.ToInt32(dsCove.Tables["factura"].Rows[0]["certificadoOrigen"]);
                    cove.factura.numeroExportadorAutorizado = dsCove.Tables["factura"].Rows[0]["numeroExportadorAutorizado"].ToString();
                    cove.factura.subdivision = Convert.ToInt32(dsCove.Tables["factura"].Rows[0]["subdivision"]);
                }

                /* Datos del emisor */
                if (dsCove.Tables["emisor"].Rows.Count > 0)
                {
                    cove.emisor = new CoveServicio.Emisor();
                    cove.emisor.nombre = EscapeXMLValue(dsCove.Tables["emisor"].Rows[0]["nombre"].ToString());
                    cove.emisor.apellidoPaterno = EscapeXMLValue(dsCove.Tables["emisor"].Rows[0]["apellidoPaterno"].ToString());
                    cove.emisor.apellidoMaterno = EscapeXMLValue(dsCove.Tables["emisor"].Rows[0]["apellidoMaterno"].ToString());
                    cove.emisor.tipoIdentificador = Convert.ToInt16(dsCove.Tables["emisor"].Rows[0]["tipoIdentificador"]);
                    cove.emisor.identificacion = dsCove.Tables["emisor"].Rows[0]["identificacion"].ToString();

                    DataRow[] emisorDomicilio = dsCove.Tables["domicilio"].Select(string.Format("emisor_Id=0"));
                    if (emisorDomicilio.Length > 0)
                    {
                        cove.emisor.domicilio = new CoveServicio.Domicilio();
                        cove.emisor.domicilio.calle = EscapeXMLValue(emisorDomicilio[0]["calle"].ToString());
                        cove.emisor.domicilio.codigoPostal = emisorDomicilio[0]["codigoPostal"].ToString();
                        cove.emisor.domicilio.colonia = EscapeXMLValue(emisorDomicilio[0]["colonia"].ToString());
                        cove.emisor.domicilio.numeroExterior = emisorDomicilio[0]["numeroExterior"].ToString();
                        cove.emisor.domicilio.numeroInterior = emisorDomicilio[0]["numeroInterior"].ToString();
                        cove.emisor.domicilio.pais = EscapeXMLValue(emisorDomicilio[0]["pais"].ToString());
                        cove.emisor.domicilio.entidadFederativa = EscapeXMLValue(emisorDomicilio[0]["entidadFederativa"].ToString());
                        cove.emisor.domicilio.municipio = EscapeXMLValue(emisorDomicilio[0]["municipio"].ToString());
                        cove.emisor.domicilio.localidad = EscapeXMLValue(emisorDomicilio[0]["localidad"].ToString());
                    }
                }

                /* Datos del destinatario */
                if (dsCove.Tables["destinatario"].Rows.Count > 0)
                {
                    cove.destinatario = new CoveServicio.Destinatario();
                    cove.destinatario.nombre = EscapeXMLValue(dsCove.Tables["destinatario"].Rows[0]["nombre"].ToString());
                    cove.destinatario.apellidoPaterno = EscapeXMLValue(dsCove.Tables["destinatario"].Rows[0]["apellidoPaterno"].ToString());
                    cove.destinatario.apellidoMaterno = EscapeXMLValue(dsCove.Tables["destinatario"].Rows[0]["apellidoMaterno"].ToString());
                    cove.destinatario.tipoIdentificador = Convert.ToInt16(dsCove.Tables["destinatario"].Rows[0]["tipoIdentificador"]);
                    cove.destinatario.identificacion = dsCove.Tables["destinatario"].Rows[0]["identificacion"].ToString();

                    DataRow[] destDomicilio = dsCove.Tables["domicilio"].Select(string.Format("destinatario_Id=0"));
                    cove.destinatario.domicilio = new CoveServicio.Domicilio();
                    cove.destinatario.domicilio.calle = EscapeXMLValue(destDomicilio[0]["calle"].ToString());
                    cove.destinatario.domicilio.codigoPostal = destDomicilio[0]["codigoPostal"].ToString();
                    cove.destinatario.domicilio.colonia = EscapeXMLValue(destDomicilio[0]["colonia"].ToString());
                    cove.destinatario.domicilio.numeroExterior = destDomicilio[0]["numeroExterior"].ToString();
                    cove.destinatario.domicilio.numeroInterior = destDomicilio[0]["numeroInterior"].ToString();
                    cove.destinatario.domicilio.pais = EscapeXMLValue(destDomicilio[0]["pais"].ToString());
                    cove.destinatario.domicilio.entidadFederativa = EscapeXMLValue(destDomicilio[0]["entidadFederativa"].ToString());
                    cove.destinatario.domicilio.municipio = EscapeXMLValue(destDomicilio[0]["municipio"].ToString());
                    cove.destinatario.domicilio.localidad = EscapeXMLValue(destDomicilio[0]["localidad"].ToString());
                }

                /* Mercancias */
                if (dsCove.Tables["mercancias"].Rows.Count > 0)
                {
                    int pos = 0;
                    cove.mercancias = new CoveServicio.Mercancia[dsCove.Tables["mercancias"].Rows.Count];
                    foreach (var item in cove.mercancias)
                    {
                        CoveServicio.Mercancia mercancia = new CoveServicio.Mercancia();

                        mercancia.descripcionGenerica = EscapeXMLValue(dsCove.Tables["mercancias"].Rows[pos]["descripcionGenerica"].ToString());
                        mercancia.claveUnidadMedida = dsCove.Tables["mercancias"].Rows[pos]["claveUnidadMedida"].ToString();
                        mercancia.cantidad = Convert.ToDecimal(dsCove.Tables["mercancias"].Rows[pos]["cantidad"]);
                        mercancia.valorUnitario = Convert.ToDecimal(dsCove.Tables["mercancias"].Rows[pos]["valorUnitario"]);
                        mercancia.valorTotal = Convert.ToDecimal(dsCove.Tables["mercancias"].Rows[pos]["valorTotal"]);
                        mercancia.valorDolares = Convert.ToDecimal(dsCove.Tables["mercancias"].Rows[pos]["valorDolares"]);
                        mercancia.tipoMoneda = dsCove.Tables["mercancias"].Rows[pos]["tipoMoneda"].ToString();

                        if (dsCove.Tables.Contains("descripcionesEspecificas") && dsCove.Tables["descripcionesEspecificas"].Rows.Count > 0)
                        {
                            string mercanciaId = dsCove.Tables["mercancias"].Rows[pos]["mercancias_Id"].ToString();

                            DataRow[] descMercancia = dsCove.Tables["descripcionesEspecificas"].Select(String.Format("mercancias_Id={0}", mercanciaId));
                            if (descMercancia.Length > 0)
                            {
                                CoveServicio.DescripcionMercancia[] descripcionesMercancia = new CoveServicio.DescripcionMercancia[descMercancia.Length];
                                int posDesc = 0;
                                foreach (var descripcion in descMercancia)
                                {
                                    CoveServicio.DescripcionMercancia descripcionMercancia = new CoveServicio.DescripcionMercancia();
                                    descripcionMercancia.marca = EscapeXMLValue(descripcion["marca"].ToString());
                                    descripcionMercancia.modelo = EscapeXMLValue(descripcion["modelo"].ToString());
                                    descripcionMercancia.subModelo = EscapeXMLValue(descripcion["subModelo"].ToString());
                                    descripcionMercancia.numeroSerie = EscapeXMLValue(descripcion["numeroSerie"].ToString());
                                    descripcionesMercancia[posDesc] = descripcionMercancia;
                                    posDesc++;
                                }
                                mercancia.descripcionesEspecificas = descripcionesMercancia;
                            }
                        }
                        cove.mercancias[pos] = mercancia;
                        pos++;
                    }
                }
            }

            return cove;
            #endregion
        }

        public CoveServicio.ComprobanteValorElectronicoNoIA LeerFacturasNOIA(string archivoFacturasNOIA)
        {
            #region LeerFacturasNOIA
            DataSet dsFacturasNOIA = new DataSet();
            CoveServicio.ComprobanteValorElectronicoNoIA cove = new CoveServicio.ComprobanteValorElectronicoNoIA();

            try
            {
                dsFacturasNOIA.ReadXml(archivoFacturasNOIA);
            }
            catch (Exception ex)
            {
                Exception exXML = new Exception(string.Format("Error {0} al tratar de leer el archivo {1}", ex.Message, archivoFacturasNOIA), ex);
                throw exXML;
            }

            if (dsFacturasNOIA.Tables["comprobantes"].Rows.Count > 0)
            {
                cove.firmaElectronica = new CoveServicio.FirmaElectronica();
                if (dsFacturasNOIA.Tables["firmaElectronica"].Rows.Count > 0)
                {
                    cove.firmaElectronica.cadenaOriginal = dsFacturasNOIA.Tables["firmaElectronica"].Rows[0]["cadenaOriginal"].ToString();
                }

                cove.fechaExpedicion = Convert.ToDateTime(dsFacturasNOIA.Tables["comprobantes"].Rows[0]["fechaExpedicion"].ToString(), CultureInfo.InvariantCulture);
                cove.observaciones = dsFacturasNOIA.Tables["comprobantes"].Rows[0]["observaciones"].ToString();
                cove.correoElectronico = dsFacturasNOIA.Tables["comprobantes"].Rows[0]["correoElectronico"].ToString();
                cove.numeroRelacionFacturas = dsFacturasNOIA.Tables["comprobantes"].Rows[0]["numeroRelacionFacturas"].ToString();
                cove.tipoFigura = Convert.ToInt16(dsFacturasNOIA.Tables["comprobantes"].Rows[0]["tipoFigura"]);
                cove.tipoOperacion = (CoveServicio.TipoOperacion)Convert.ToInt16(dsFacturasNOIA.Tables["comprobantes"].Rows[0]["tipoOperacion"]);

                #region Patentes aduanales
                if (dsFacturasNOIA.Tables.Contains("patentesAduanales") && dsFacturasNOIA.Tables["patentesAduanales"].Rows.Count > 0)
                {
                    cove.patenteAduanal = new string[dsFacturasNOIA.Tables["patentesAduanales"].Rows.Count];
                    int pos = 0;
                    foreach (DataRow item in dsFacturasNOIA.Tables["patentesAduanales"].Rows)
                    {
                        cove.patenteAduanal[pos] = item["patenteAduanal"].ToString();
                        pos = pos + 1;
                    }
                }
                else
                {
                    if (dsFacturasNOIA.Tables.Contains("patenteAduanal") && dsFacturasNOIA.Tables["patenteAduanal"].Rows.Count > 0)
                    {
                        cove.patenteAduanal = new string[dsFacturasNOIA.Tables["patenteAduanal"].Rows.Count];
                        int pos = 0;
                        foreach (DataRow item in dsFacturasNOIA.Tables["patenteAduanal"].Rows)
                        {
                            cove.patenteAduanal[pos] = item["patenteAduanal"].ToString();
                            pos = pos + 1;
                        }
                    }
                }
                #endregion

                #region RFC de consultas
                if (dsFacturasNOIA.Tables.Contains("rfcConsultas") && dsFacturasNOIA.Tables["rfcConsultas"].Rows.Count > 0)
                {
                    cove.rfcConsulta = new string[dsFacturasNOIA.Tables["rfcConsultas"].Rows.Count];
                    int pos = 0;
                    foreach (DataRow item in dsFacturasNOIA.Tables["rfcConsultas"].Rows)
                    {
                        cove.rfcConsulta[pos] = item["rfcConsulta"].ToString();
                        pos = pos + 1;
                    }
                }
                else
                {
                    if (dsFacturasNOIA.Tables.Contains("rfcConsulta") && dsFacturasNOIA.Tables["rfcConsulta"].Rows.Count > 0)
                    {
                        cove.rfcConsulta = new string[dsFacturasNOIA.Tables["rfcConsulta"].Rows.Count];
                        int pos = 0;
                        foreach (DataRow item in dsFacturasNOIA.Tables["rfcConsulta"].Rows)
                        {
                            cove.rfcConsulta[pos] = item[0].ToString();
                            pos = pos + 1;
                        }
                    }
                }
                #endregion

                #region Facturas
                if (dsFacturasNOIA.Tables["facturas"].Rows.Count > 0)
                {
                    cove.facturas = new CoveServicio.FacturaNoIA[dsFacturasNOIA.Tables["facturas"].Rows.Count];
                    for (int i = 0; i < dsFacturasNOIA.Tables["facturas"].Rows.Count; i++)
                    {
                        CoveServicio.FacturaNoIA factura = new CoveServicio.FacturaNoIA();
                        factura.numeroFactura = dsFacturasNOIA.Tables["facturas"].Rows[i]["numeroFactura"].ToString();

                        if (!dsFacturasNOIA.Tables["facturas"].Rows[i].IsNull("certificadoOrigen") &&
                            !string.IsNullOrEmpty(dsFacturasNOIA.Tables["facturas"].Rows[i]["certificadoOrigen"].ToString()))
                            factura.certificadoOrigen = Convert.ToInt16(dsFacturasNOIA.Tables["facturas"].Rows[i]["certificadoOrigen"]);

                        if (!dsFacturasNOIA.Tables["facturas"].Rows[i].IsNull("numeroExportadorAutorizado") &&
                            !string.IsNullOrEmpty(dsFacturasNOIA.Tables["facturas"].Rows[i]["numeroExportadorAutorizado"].ToString()))
                            factura.numeroExportadorAutorizado = dsFacturasNOIA.Tables["facturas"].Rows[i]["numeroExportadorAutorizado"].ToString();

                        if (!dsFacturasNOIA.Tables["facturas"].Rows[i].IsNull("subdivision") &&
                            !string.IsNullOrEmpty(dsFacturasNOIA.Tables["facturas"].Rows[i]["subdivision"].ToString()))
                            factura.subdivision = Convert.ToInt16(dsFacturasNOIA.Tables["facturas"].Rows[i]["subdivision"]);

                        /* Datos del emisor */
                        DataRow[] emisores = dsFacturasNOIA.Tables["emisor"].Select(String.Format("facturas_id={0}", i));
                        if (emisores.Length > 0)
                        {
                            factura.emisor = new CoveServicio.Emisor();
                            factura.emisor.nombre = emisores[0]["nombre"].ToString();

                            if (!emisores[0].IsNull("apellidoPaterno") &&
                                !string.IsNullOrEmpty(emisores[0]["apellidoPaterno"].ToString()))
                                factura.emisor.apellidoPaterno = emisores[0]["apellidoPaterno"].ToString();

                            if (!emisores[0].IsNull("apellidoMaterno") &&
                                !string.IsNullOrEmpty(emisores[0]["apellidoMaterno"].ToString()))
                                factura.emisor.apellidoMaterno = emisores[0]["apellidoMaterno"].ToString();

                            factura.emisor.tipoIdentificador = Convert.ToInt16(emisores[0]["tipoIdentificador"]);
                            factura.emisor.identificacion = emisores[0]["identificacion"].ToString();

                            DataRow[] emisorDomicilio = dsFacturasNOIA.Tables["domicilio"].Select(string.Format("emisor_Id={0}", i));
                            if (emisorDomicilio.Length > 0)
                            {
                                factura.emisor.domicilio = new CoveServicio.Domicilio();
                                factura.emisor.domicilio.calle = emisorDomicilio[0]["calle"].ToString();
                                factura.emisor.domicilio.codigoPostal = emisorDomicilio[0]["codigoPostal"].ToString();
                                factura.emisor.domicilio.colonia = emisorDomicilio[0]["colonia"].ToString();
                                factura.emisor.domicilio.numeroExterior = emisorDomicilio[0]["numeroExterior"].ToString();
                                factura.emisor.domicilio.numeroInterior = emisorDomicilio[0]["numeroInterior"].ToString();
                                factura.emisor.domicilio.pais = emisorDomicilio[0]["pais"].ToString();
                                factura.emisor.domicilio.entidadFederativa = emisorDomicilio[0]["entidadFederativa"].ToString();
                                factura.emisor.domicilio.municipio = emisorDomicilio[0]["municipio"].ToString();
                                factura.emisor.domicilio.localidad = emisorDomicilio[0]["localidad"].ToString();
                            }
                        }

                        /* Datos del destinatario */
                        DataRow[] destinatarios = dsFacturasNOIA.Tables["destinatario"].Select(String.Format("facturas_id={0}", i));
                        if (destinatarios.Length > 0)
                        {
                            factura.destinatario = new CoveServicio.Destinatario();
                            factura.destinatario.nombre = destinatarios[0]["nombre"].ToString();

                            if (!destinatarios[0].IsNull("apellidoPaterno") &&
                                !string.IsNullOrEmpty(destinatarios[0]["apellidoPaterno"].ToString()))
                                factura.destinatario.apellidoPaterno = destinatarios[0]["apellidoPaterno"].ToString();

                            if (!destinatarios[0].IsNull("apellidoMaterno") &&
                                !string.IsNullOrEmpty(destinatarios[0]["apellidoMaterno"].ToString()))
                                factura.destinatario.apellidoMaterno = destinatarios[0]["apellidoMaterno"].ToString();

                            factura.destinatario.tipoIdentificador = Convert.ToInt16(destinatarios[0]["tipoIdentificador"]);
                            factura.destinatario.identificacion = destinatarios[0]["identificacion"].ToString();

                            DataRow[] destDomicilio = dsFacturasNOIA.Tables["domicilio"].Select(string.Format("destinatario_Id={0}", i));
                            if (destDomicilio.Length > 0)
                            {
                                factura.destinatario.domicilio = new CoveServicio.Domicilio();
                                factura.destinatario.domicilio.calle = destDomicilio[0]["calle"].ToString();
                                factura.destinatario.domicilio.codigoPostal = destDomicilio[0]["codigoPostal"].ToString();
                                factura.destinatario.domicilio.colonia = destDomicilio[0]["colonia"].ToString();
                                factura.destinatario.domicilio.numeroExterior = destDomicilio[0]["numeroExterior"].ToString();
                                factura.destinatario.domicilio.numeroInterior = destDomicilio[0]["numeroInterior"].ToString();
                                factura.destinatario.domicilio.pais = destDomicilio[0]["pais"].ToString();
                                factura.destinatario.domicilio.entidadFederativa = destDomicilio[0]["entidadFederativa"].ToString();
                                factura.destinatario.domicilio.municipio = destDomicilio[0]["municipio"].ToString();
                                factura.destinatario.domicilio.localidad = destDomicilio[0]["localidad"].ToString();
                            }
                        }

                        /* Mercancias */
                        DataRow[] mercancias = dsFacturasNOIA.Tables["mercancias"].Select(String.Format("facturas_id={0}", i));
                        if (mercancias.Length > 0)
                        {
                            factura.mercancias = new CoveServicio.Mercancia[mercancias.Length];
                            for (int j = 0; j < mercancias.Length; j++)
                            {
                                CoveServicio.Mercancia mercancia = new CoveServicio.Mercancia();

                                mercancia.descripcionGenerica = mercancias[j]["descripcionGenerica"].ToString();
                                mercancia.claveUnidadMedida = mercancias[j]["claveUnidadMedida"].ToString();
                                mercancia.cantidad = Convert.ToDecimal(mercancias[j]["cantidad"]);
                                mercancia.valorUnitario = Convert.ToDecimal(mercancias[j]["valorUnitario"]);
                                mercancia.valorTotal = Convert.ToDecimal(mercancias[j]["valorTotal"]);
                                mercancia.valorDolares = Convert.ToDecimal(mercancias[j]["valorDolares"]);
                                mercancia.tipoMoneda = mercancias[j]["tipoMoneda"].ToString();

                                if (dsFacturasNOIA.Tables.Contains("descripcionesEspecificas") && dsFacturasNOIA.Tables["descripcionesEspecificas"].Rows.Count > 0)
                                {
                                    string mercanciaId = mercancias[j]["mercancias_Id"].ToString();

                                    DataRow[] descMercancia = dsFacturasNOIA.Tables["descripcionesEspecificas"].Select(String.Format("mercancias_Id={0}", mercanciaId));
                                    if (descMercancia.Length > 0)
                                    {
                                        CoveServicio.DescripcionMercancia[] descripcionesMercancia = new CoveServicio.DescripcionMercancia[descMercancia.Length];
                                        for (int d = 0; d < descMercancia.Length; d++)
                                        {
                                            CoveServicio.DescripcionMercancia descripcionMercancia = new CoveServicio.DescripcionMercancia();
                                            descripcionMercancia.marca = descMercancia[d]["marca"].ToString();
                                            descripcionMercancia.modelo = descMercancia[d]["modelo"].ToString();
                                            descripcionMercancia.subModelo = descMercancia[d]["subModelo"].ToString();
                                            descripcionMercancia.numeroSerie = descMercancia[d]["numeroSerie"].ToString();
                                            descripcionesMercancia[d] = descripcionMercancia;
                                        }
                                        mercancia.descripcionesEspecificas = descripcionesMercancia;
                                    }
                                }
                                factura.mercancias[j] = mercancia;
                            }
                        }
                        cove.facturas[i] = factura;
                    }
                }
                #endregion
            }

            return cove;
            #endregion
        }

        public CoveServicio.ComprobanteValorElectronicoIA LeerFacturasIA(string archivoFacturasIA)
        {
            #region LeerFacturasNOIA
            DataSet dsFacturasIA = new DataSet();
            CoveServicio.ComprobanteValorElectronicoIA cove = new CoveServicio.ComprobanteValorElectronicoIA();

            try
            {
                dsFacturasIA.ReadXml(archivoFacturasIA);
            }
            catch (Exception ex)
            {
                Exception exXML = new Exception(string.Format("Error {0} al tratar de leer el archivo {1}", ex.Message, archivoFacturasIA), ex);
                throw exXML;
            }

            if (dsFacturasIA.Tables["comprobantes"].Rows.Count > 0)
            {
                cove.firmaElectronica = new CoveServicio.FirmaElectronica();
                if (dsFacturasIA.Tables["firmaElectronica"].Rows.Count > 0)
                {
                    cove.firmaElectronica.cadenaOriginal = dsFacturasIA.Tables["firmaElectronica"].Rows[0]["cadenaOriginal"].ToString();
                }

                cove.fechaExpedicion = Convert.ToDateTime(dsFacturasIA.Tables["comprobantes"].Rows[0]["fechaExpedicion"].ToString(), CultureInfo.InvariantCulture);
                cove.observaciones = dsFacturasIA.Tables["comprobantes"].Rows[0]["observaciones"].ToString();
                cove.correoElectronico = dsFacturasIA.Tables["comprobantes"].Rows[0]["correoElectronico"].ToString();
                cove.numeroRelacionFacturas = dsFacturasIA.Tables["comprobantes"].Rows[0]["numeroRelacionFacturas"].ToString();
                cove.tipoFigura = Convert.ToInt16(dsFacturasIA.Tables["comprobantes"].Rows[0]["tipoFigura"]);
                cove.tipoOperacion = (CoveServicio.TipoOperacion)Convert.ToInt16(dsFacturasIA.Tables["comprobantes"].Rows[0]["tipoOperacion"]);

                #region Patentes aduanales
                if (dsFacturasIA.Tables.Contains("patentesAduanales") && dsFacturasIA.Tables["patentesAduanales"].Rows.Count > 0)
                {
                    cove.patenteAduanal = new string[dsFacturasIA.Tables["patentesAduanales"].Rows.Count];
                    int pos = 0;
                    foreach (DataRow item in dsFacturasIA.Tables["patentesAduanales"].Rows)
                    {
                        cove.patenteAduanal[pos] = item["patenteAduanal"].ToString();
                        pos = pos + 1;
                    }
                }
                else
                {
                    if (dsFacturasIA.Tables.Contains("patenteAduanal") && dsFacturasIA.Tables["patenteAduanal"].Rows.Count > 0)
                    {
                        cove.patenteAduanal = new string[dsFacturasIA.Tables["patenteAduanal"].Rows.Count];
                        int pos = 0;
                        foreach (DataRow item in dsFacturasIA.Tables["patenteAduanal"].Rows)
                        {
                            cove.patenteAduanal[pos] = item["patenteAduanal"].ToString();
                            pos = pos + 1;
                        }
                    }
                }
                #endregion

                #region RFC de consultas
                if (dsFacturasIA.Tables.Contains("rfcConsultas") && dsFacturasIA.Tables["rfcConsultas"].Rows.Count > 0)
                {
                    cove.rfcConsulta = new string[dsFacturasIA.Tables["rfcConsultas"].Rows.Count];
                    int pos = 0;
                    foreach (DataRow item in dsFacturasIA.Tables["rfcConsultas"].Rows)
                    {
                        cove.rfcConsulta[pos] = item["rfcConsulta"].ToString();
                        pos = pos + 1;
                    }
                }
                else
                {
                    if (dsFacturasIA.Tables.Contains("rfcConsulta") && dsFacturasIA.Tables["rfcConsulta"].Rows.Count > 0)
                    {
                        cove.rfcConsulta = new string[dsFacturasIA.Tables["rfcConsulta"].Rows.Count];
                        int pos = 0;
                        foreach (DataRow item in dsFacturasIA.Tables["rfcConsulta"].Rows)
                        {
                            cove.rfcConsulta[pos] = item[0].ToString();
                            pos = pos + 1;
                        }
                    }
                }
                #endregion

                #region Emisor
                DataRow[] emisores = dsFacturasIA.Tables["emisor"].Select();
                if (emisores.Length > 0)
                {
                    cove.emisor = new CoveServicio.Emisor();
                    cove.emisor.nombre = emisores[0]["nombre"].ToString();
                    cove.emisor.apellidoPaterno = emisores[0]["apellidoPaterno"].ToString();
                    cove.emisor.apellidoMaterno = emisores[0]["apellidoMaterno"].ToString();
                    cove.emisor.tipoIdentificador = Convert.ToInt16(emisores[0]["tipoIdentificador"]);
                    cove.emisor.identificacion = emisores[0]["identificacion"].ToString();

                    DataRow[] emisorDomicilio = dsFacturasIA.Tables["domicilio"].Select("emisor_Id=0");
                    if (emisorDomicilio.Length > 0)
                    {
                        cove.emisor.domicilio = new CoveServicio.Domicilio();
                        cove.emisor.domicilio.calle = emisorDomicilio[0]["calle"].ToString();
                        cove.emisor.domicilio.codigoPostal = emisorDomicilio[0]["codigoPostal"].ToString();
                        cove.emisor.domicilio.colonia = emisorDomicilio[0]["colonia"].ToString();
                        cove.emisor.domicilio.numeroExterior = emisorDomicilio[0]["numeroExterior"].ToString();
                        cove.emisor.domicilio.numeroInterior = emisorDomicilio[0]["numeroInterior"].ToString();
                        cove.emisor.domicilio.pais = emisorDomicilio[0]["pais"].ToString();
                        cove.emisor.domicilio.entidadFederativa = emisorDomicilio[0]["entidadFederativa"].ToString();
                        cove.emisor.domicilio.municipio = emisorDomicilio[0]["municipio"].ToString();
                        cove.emisor.domicilio.localidad = emisorDomicilio[0]["localidad"].ToString();
                    }
                }
                #endregion

                #region Datos del destinatario
                DataRow[] destinatarios = dsFacturasIA.Tables["destinatario"].Select();
                if (destinatarios.Length > 0)
                {
                    cove.destinatario = new CoveServicio.Destinatario();
                    cove.destinatario.nombre = destinatarios[0]["nombre"].ToString();
                    cove.destinatario.apellidoPaterno = destinatarios[0]["apellidoPaterno"].ToString();
                    cove.destinatario.apellidoMaterno = destinatarios[0]["apellidoMaterno"].ToString();
                    cove.destinatario.tipoIdentificador = Convert.ToInt16(destinatarios[0]["tipoIdentificador"]);
                    cove.destinatario.identificacion = destinatarios[0]["identificacion"].ToString();

                    DataRow[] destDomicilio = dsFacturasIA.Tables["domicilio"].Select("destinatario_Id=0");
                    if (destDomicilio.Length > 0)
                    {
                        cove.destinatario.domicilio = new CoveServicio.Domicilio();
                        cove.destinatario.domicilio.calle = destDomicilio[0]["calle"].ToString();
                        cove.destinatario.domicilio.codigoPostal = destDomicilio[0]["codigoPostal"].ToString();
                        cove.destinatario.domicilio.colonia = destDomicilio[0]["colonia"].ToString();
                        cove.destinatario.domicilio.numeroExterior = destDomicilio[0]["numeroExterior"].ToString();
                        cove.destinatario.domicilio.numeroInterior = destDomicilio[0]["numeroInterior"].ToString();
                        cove.destinatario.domicilio.pais = destDomicilio[0]["pais"].ToString();
                        cove.destinatario.domicilio.entidadFederativa = destDomicilio[0]["entidadFederativa"].ToString();
                        cove.destinatario.domicilio.municipio = destDomicilio[0]["municipio"].ToString();
                        cove.destinatario.domicilio.localidad = destDomicilio[0]["localidad"].ToString();
                    }
                }
                #endregion

                #region Facturas
                if (dsFacturasIA.Tables["facturas"].Rows.Count > 0)
                {
                    cove.facturas = new CoveServicio.FacturaIA[dsFacturasIA.Tables["facturas"].Rows.Count];
                    for (int i = 0; i < dsFacturasIA.Tables["facturas"].Rows.Count; i++)
                    {
                        CoveServicio.FacturaIA factura = new CoveServicio.FacturaIA();
                        factura.numeroFactura = dsFacturasIA.Tables["facturas"].Rows[i]["numeroFactura"].ToString();

                        if (!dsFacturasIA.Tables["facturas"].Rows[i].IsNull("certificadoOrigen") &&
                            !string.IsNullOrEmpty(dsFacturasIA.Tables["facturas"].Rows[i]["certificadoOrigen"].ToString()))
                            factura.certificadoOrigen = Convert.ToInt16(dsFacturasIA.Tables["facturas"].Rows[i]["certificadoOrigen"]);

                        if (!dsFacturasIA.Tables["facturas"].Rows[i].IsNull("numeroExportadorAutorizado") &&
                            !string.IsNullOrEmpty(dsFacturasIA.Tables["facturas"].Rows[i]["numeroExportadorAutorizado"].ToString()))
                            factura.numeroExportadorAutorizado = dsFacturasIA.Tables["facturas"].Rows[i]["numeroExportadorAutorizado"].ToString();

                        if (!dsFacturasIA.Tables["facturas"].Rows[i].IsNull("subdivision") &&
                            !string.IsNullOrEmpty(dsFacturasIA.Tables["facturas"].Rows[i]["subdivision"].ToString()))
                            factura.subdivision = Convert.ToInt16(dsFacturasIA.Tables["facturas"].Rows[i]["subdivision"]);

                        /* Mercancias */
                        DataRow[] mercancias = dsFacturasIA.Tables["mercancias"].Select(String.Format("facturas_id={0}", i));
                        if (mercancias.Length > 0)
                        {
                            factura.mercancias = new CoveServicio.Mercancia[mercancias.Length];
                            for (int j = 0; j < mercancias.Length; j++)
                            {
                                CoveServicio.Mercancia mercancia = new CoveServicio.Mercancia();

                                mercancia.descripcionGenerica = mercancias[j]["descripcionGenerica"].ToString();
                                mercancia.claveUnidadMedida = mercancias[j]["claveUnidadMedida"].ToString();
                                mercancia.cantidad = Convert.ToDecimal(mercancias[j]["cantidad"]);
                                mercancia.valorUnitario = Convert.ToDecimal(mercancias[j]["valorUnitario"]);
                                mercancia.valorTotal = Convert.ToDecimal(mercancias[j]["valorTotal"]);
                                mercancia.valorDolares = Convert.ToDecimal(mercancias[j]["valorDolares"]);
                                mercancia.tipoMoneda = mercancias[j]["tipoMoneda"].ToString();

                                if (dsFacturasIA.Tables.Contains("descripcionesEspecificas") && dsFacturasIA.Tables["descripcionesEspecificas"].Rows.Count > 0)
                                {
                                    string mercanciaId = mercancias[j]["mercancias_Id"].ToString();

                                    DataRow[] descMercancia = dsFacturasIA.Tables["descripcionesEspecificas"].Select(String.Format("mercancias_Id={0}", mercanciaId));
                                    if (descMercancia.Length > 0)
                                    {
                                        CoveServicio.DescripcionMercancia[] descripcionesMercancia = new CoveServicio.DescripcionMercancia[descMercancia.Length];
                                        for (int d = 0; d < descMercancia.Length; d++)
                                        {
                                            CoveServicio.DescripcionMercancia descripcionMercancia = new CoveServicio.DescripcionMercancia();
                                            descripcionMercancia.marca = descMercancia[d]["marca"].ToString();
                                            descripcionMercancia.modelo = descMercancia[d]["modelo"].ToString();
                                            descripcionMercancia.subModelo = descMercancia[d]["subModelo"].ToString();
                                            descripcionMercancia.numeroSerie = descMercancia[d]["numeroSerie"].ToString();
                                            descripcionesMercancia[d] = descripcionMercancia;
                                        }
                                        mercancia.descripcionesEspecificas = descripcionesMercancia;
                                    }
                                }
                                factura.mercancias[j] = mercancia;
                            }
                        }
                        cove.facturas[i] = factura;
                    }
                }
                #endregion
            }

            return cove;
            #endregion
        }

        [ComVisible(true)]
        public string[] ConsultarListaPedimentos()
        {
            #region ConsultarListaPedimentos
            string[] regreso = new string[2];

            regreso[0] = "Primer elemento";
            regreso[1] = "Segundo elemento";

            return regreso;
            #endregion
        }

        [ComVisible(true)]
        public pedimentoregresar ConsultarPedimento()
        {
            #region ConsultarPedimento
            pedimentoregresar pedimentoRegresar = new pedimentoregresar();
            pedimentoRegresar.Numero = "111111";
            pedimentoRegresar.Descripcion = "Pedimento";

            return pedimentoRegresar;
            #endregion
        }

        [ComVisible(true)]
        public string Timbrar(string layout, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userID, string userPass, string emisorRFC, string urlTimbrado, string PAC, string referencia)
        {
            #region Timbrar
            string cfdiReturn = string.Empty;
            CoveProxy.Timbrado.Timbrador timbrador = new Timbrado.Timbrador();

            if (File.Exists(layout))
            {
                try
                {
                    switch (PAC.ToUpper())
                    {
                        case "MODERNA":
                            cfdiReturn = timbrador.TimbrarPACModerna(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, emisorRFC, urlTimbrado);
                            break;
                        case "DIVERZA":
                            cfdiReturn = timbrador.TimbrarPACDiversa(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, emisorRFC, userPass, urlTimbrado, referencia, userID);
                            break;
                        case "FD":
                            cfdiReturn = timbrador.TimbrarPACFOLIODIGITAL(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, urlTimbrado, referencia);
                            break;
                        case "CARVAJAL":
                            cfdiReturn = timbrador.TimbrarPACCarvajal(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, emisorRFC, urlTimbrado);
                            break;
                        case "EKOMERCIO":
                            cfdiReturn = timbrador.TimbrarPACEkomercio(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, emisorRFC, urlTimbrado);
                            break;
                        case "PAXFACTURACION":
                            cfdiReturn = timbrador.TimbrarPACPaxFacturacion(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, urlTimbrado, referencia);
                            break;
                        case "EDICOM":
                            cfdiReturn = timbrador.TimbrarPACEDICOM(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID);
                            break;
                        case "BUZON-E":
                            cfdiReturn = timbrador.TimbrarPACBuzonE(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, urlTimbrado, referencia);
                            break;
                        case "SW":
                            cfdiReturn = timbrador.TimbrarPACSW(layout, archivoXslt, archivoCertificado, archivoKey, certificadoPass, userID, userPass, urlTimbrado, referencia);
                            break;
                        default:
                            cfdiReturn = $"ERROR PAC {PAC.ToUpper()} no soportado";
                            break;
                    }
                }
                catch (Exception e)
                {
                    cfdiReturn = String.Format("ERROR TRY {0}", e.Message);
                }
            }
            else
            {
                cfdiReturn = String.Format("ERROR No se encontro archivo {0}", layout);
            }
            return cfdiReturn;
            #endregion
        }

        public string CrearEkomercioPDF(string emisorRFC, string uuid, string rutaNombreArchivoPDF)
        {
            CoveProxy.Timbrado.Timbrador timbrador = new Timbrado.Timbrador();

            return timbrador.CrearEkomercioPDF(emisorRFC, uuid, rutaNombreArchivoPDF);
        }

        public string PDFSW(string xmlTimbrado, string uuid, string logo, string urlTimbrado, string urlPDF, string userId, string userPass)
        {
            CoveProxy.Timbrado.Timbrador timbrador = new Timbrado.Timbrador();

            return timbrador.PDFSW(xmlTimbrado, uuid, logo, urlTimbrado, urlPDF, userId, userPass);
        }

        public string ObtenerUsuario()
        {
            try
            {
                var usuario = System.Security.Principal.WindowsIdentity.GetCurrent();

                if (usuario.IsAuthenticated)
                {
                    return usuario.Name;
                }
                else
                {
                    return "ERROR: El usuario no esta autentificado";
                }
            }
            catch (Exception ex)
            {
                return string.Format("ERROR: {0}", ex.Message);
            }
        }

        private void WriteLogFile(string Error)
        {
            #region WriteLogFile
            string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string archivoLog = string.Format("{0}\\ErrorCoveEnvio.log", currentAssemblyDirectoryName);
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(archivoLog, false);
                sw.Write(Error);
            }
            catch (System.IO.DirectoryNotFoundException exfn)
            {
                Exception ne = new Exception(string.Format("No se encontro el archivo {0}, para grabar el error, error {1}", archivoLog, exfn.Message));
                throw ne;
            }
            catch (System.IO.IOException exIO)
            {
                Exception ne = new Exception(string.Format("Error para grabar en el archivo {0}, error {1}", archivoLog, exIO.Message));
                throw ne;
            }
            catch (System.Security.SecurityException exSe)
            {
                Exception ne = new Exception(string.Format("Error de seguridad en el archivo {0}, para grabar el error, error {1}", archivoLog, exSe.Message));
                throw ne;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
            #endregion
        }

        public static string EscapeXMLValue(string xmlString)
        {

            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            //return xmlString.Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("&", "&amp;");
            //á -> &aacute; é-> &eacute;  í-> &iacute;  ó-> &oacute; ú-> &uacute; ñ-> &ntilde;

            return xmlString
                //.Replace("&", "&amp;")
                .Replace("á", "&aacute;")
                .Replace("é", "&eacute;")
                .Replace("í", "&iacute;")
                .Replace("ó", "&oacute;")
                .Replace("ú", "&uacute;")
                .Replace("ñ", "&ntilde;")
                .Replace("Á", "&Aacute;")
                .Replace("É", "&Eacute;")
                .Replace("Í", "&Iacute;")
                .Replace("Ó", "&Oacute;")
                .Replace("Ú", "&Uacute;")
                .Replace("Ñ", "&Ntilde;")
                .Replace("€", "&euro;")
                .Replace("\"", "&quot;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
            ;
        }

        [ComVisible(true)]
        public string EnviarFacturaLaser(string archivoXML, string urlServicioWeb)
        {
            string result = string.Empty;
            var facXML = string.Empty;
            XmlDocument doc = new XmlDocument();

            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            if (File.Exists(archivoXML))
            {
                mx.com.laser.apps9.Facturaprod facWebServ = new mx.com.laser.apps9.Facturaprod();
                facWebServ.Url = urlServicioWeb;

                try
                {
                    doc.Load(archivoXML);
                    facXML = doc.InnerXml;

                    result = facWebServ.FacturasVenta(facXML);

                    return result;
                }
                catch (Exception ex)
                {
                    return string.Format("ERROR: {0}", ex.Message);
                }
            }
            else
            {
                return string.Format("ERROR no se encontro el archivo {0}", archivoXML);
            }
        }
        #endregion
    }

}
