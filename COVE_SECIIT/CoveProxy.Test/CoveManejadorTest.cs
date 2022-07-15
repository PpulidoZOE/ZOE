using CoveProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CoveProxy.CoveServicio;
using CoveProxy.CoveRespuestaServicio;
using Utilerias;

namespace CoveProxy.Test
{


    /// <summary>
    ///This is a test class for CoveManejadorTest and is intended
    ///to contain all CoveManejadorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CoveManejadorTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Generar
        ///</summary>
        [TestMethod()]
        public void GenerarTest()
        {
            CoveManejador target = new CoveManejador(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            //actual = target.Generar();
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LeerCove
        ///</summary>
        [TestMethod()]
        public void LeerCoveTest()
        {
            CoveManejador target = new CoveManejador(); // TODO: Initialize to an appropriate value
            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\DF-0000001640.xml";
            //string archivoCove = @"C:\Temp\DI-015461.xml";
            //string archivoCove = @"C:\Temp\KM2013FE0005.xml";
            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\COVE_modificado_4.xml";
            //string archivoCove = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\XML Version 5\COVE3_Cove.xml";
            string archivoCove = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\XMLCaracteresEspeciales\18EMOL-BSLP-33.xml";

            ComprobanteValorElectronico cove = new ComprobanteValorElectronico();
            cove = target.LeerCove(archivoCove);
            target.CrearArchivoXMLFirmado(cove);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for LeerFacturasNOIA
        ///</summary>
        [TestMethod()]
        public void LeerFacturasNOIATest()
        {
            CoveManejador target = new CoveManejador(); // TODO: Initialize to an appropriate value
            //string archivoFacturasNOIA = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\AGRUPADA-AGRUPAMQ-NOIA.xml"; // TODO: Initialize to an appropriate value
            string archivoFacturasNOIA = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\FACT0002MQ-FACT0003MQ.xml";
            ComprobanteValorElectronicoNoIA expected = null; // TODO: Initialize to an appropriate value
            ComprobanteValorElectronicoNoIA actual;
            actual = target.LeerFacturasNOIA(archivoFacturasNOIA);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for TimbrarTest
        ///</summary>
        [TestMethod()]
        public void TimbrarDiverzaTest()
        {
            #region EnviarComprobanteTest
            CoveManejador target = new CoveManejador();
            //string urlServicio = "https://staging.diverza.com/stamp";
            //string urlServicio = "https://staging.diverza.com/issue";

            //string urlServicio = "https://staging.diverza.com/stamp/complete"; //Version CFDI 3.2
            //string urlServicio = "http://192.237.238.229:4006/api/v1/documents/issue";
            //string urlServicio = "https://serviciosdemo.diverza.com/api/v1/documents/issue"; //Version CFDI 3.3 api/v1/documents/stamp
            //string urlServicio = "https://serviciosdemo.diverza.com/api/v1/documents/stamp"; //Version CFDI 3.3 api/v1/documents/stamp
            //string urlServicio = "http://private-anon-9848fb1548-diverza.apiary-mock.com/api/v1/documents/issue"; //Version CFDI 3.3 api/v1/documents/stamp

            //string urlServicio = "https://servicios.diverza.com/api/v1/documents/stamp";
            string urlServicio = "https://servicios.diverza.com/extended_api/v1/documents/stamp";

            //string userName = "3935";
            //string rfc = "AAA010101AAA";
            //string passwordServicioWeb = "ABCD1234";

            //string userName = "63850";
            //string rfc = "DTI0511155E8";
            //string passwordServicioWeb = "4bLNxwAgPIrEMkuB0DJvfyVNrIv9QKpQ0lzJA96+UUw=";

            string userName = "63850";
            string rfc = "DTI0511155E8";
            string passwordServicioWeb = "4bLNxwAgPIrEMkuB0DJvfyVNrIv9QKpQ0lzJA96+UUw=";

            //string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\cadenaoriginal_3_2.xslt";
            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\aaa010101aaa.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\aaa010101aaa.key";
            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\30001000000300023708.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\30001000000300023708.key";
            //string passwordCertificado = "12345678a";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\00001000000505186740.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\Claveprivada_FIEL_DTI0511155E8_20200925_144658.key";
            //string passwordCertificado = "Dartdetijuana3";

            string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\Revisar\00001000000504314190.cer";
            string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\Revisar\CSD_DART_DE_TIJUANA_S_DE_RL_DE_CV_DTI0511155E8_20200626_121156";
            string passwordCertificado = "dartdetijuana20";

            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\Revisar\EXP128.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\CartaPorte\Ejemplo1_Insumo_AutoTransporte_Federal.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CFDI\Diverza\XML\ROGER\VEXP-1216TL-2.xml"; //VEXP-1216TL.XML
            string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CFDI\Diverza\XML\ROGER\00000105.xml";
            string actual = null;

            actual = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, userName, passwordServicioWeb, rfc, urlServicio, "DIVERZA", "1234567");
            //actual = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, userName, passwordServicioWeb, rfc, urlServicio, "DIVERZA", "20181001TEST");

            Assert.IsNotNull(actual);
            #endregion
        }

        [TestMethod()]
        public void TimbrarFacturacionModernaTest()
        {
            #region TimbrarFacturacionModernaTest
            CoveManejador target = new CoveManejador();
            requestTimbrarCFDI request = new requestTimbrarCFDI();

            request.urlTimbrado = "https://t1demo.facturacionmoderna.com/timbrado/soap";

            string resultados = string.Empty;
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\10003.xml";          
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI_ComercioExterior2.xml";
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturaComplementoComercioExterior.xml";
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\xml_factmoderna_sellado.xml";          
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\10004 - FactModerna.xml"; //Con este si funciona
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\FACTURA.xml"; //Con este si timbra - pero no tiene complemento de comercio exterior
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\FactModerna_ComercioExterior.xml";
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\prueba_cce.xml";  //Con este si funciona
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\FACTDEF099 - FactModerna.xml"; //Con este si funciona
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\1000 - FacturacionModerna.xml";
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FacturacionModerna\88877 - FactModerna.xml";
            string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Generados por ZOE\88877_v2.xml";

            string archivoXSLT = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XSLT\cadenaoriginal_3_2.xslt";
            //string archivoXSLT = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XSLT\SAT cadenaoriginal_3_2.xslt";

            //string archivoCertificado = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FacturacionModerna\TUMG620310R95_1210241209S.cer";
            //string archivoCertificadoKey = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FacturacionModerna\TUMG620310R95_1210241209S.key";
            string archivoCertificado = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FacturacionModerna\esi920427886_1210221507s.cer";
            string archivoCertificadoKey = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FacturacionModerna\esi920427886_1210221507s.key";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, "12345678a", request.UserID, request.UserPass, request.emisorRFC, request.urlTimbrado, "MODERNA", "Prueba1");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarFolioDigitalTest()
        {
            #region TimbrarFacturacionDigitalTest
            CoveManejador target = new CoveManejador();
            //string url = "https://www.foliosdigitalespac.com/WSTimbrado/WSTFD.svc"; //URL Servicio CFDI 3.2
            //string url = "https://www.foliosdigitalespac.com/WSTimbrado33Test/WSCFDI33.svc?WSDL"; //URL Servicio CFDI 3.3
            string url = "https://www.foliosdigitalespac.com/WSTimbrado33/WSCFDI33.svc?WSDL"; //URL Servicio PRODUCCION CFDI 3.3

            string resultados = string.Empty;
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\FD_XML_Ejemplo.xml";
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\FD_XML_Ejemplo Sin Sello.xml";
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Generados por ZOE\88877.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\FD_88877_v2.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\cce_1.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\CFDI 3.3\ORMEX\CFDI000.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\CFDI 3.3\ZEFEPRUEBA01.xml";
            string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\CFDI 3.3\CFDI000_TEST.xml";

            //string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\cadenaoriginal_3_2.xslt";
            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\aaa010101aaa.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\aaa010101aaa.key";
            //string passwordCertificado = "12345678a";

            string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FolioDigital\CSD_GAI020902FG4.cer";
            string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FolioDigital\CSD_GAI020902FG4.key";
            string passwordCertificado = "GRUPO020902";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FolioDigital\CertificadoFirmadoPM.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\FolioDigital\LlavePkcs8PM.key";
            //string passwordCertificado = "12345678a";

            string userID = "GAI020902FG4";
            string userPass = "8R@7TdO#";
            string emisor = "GAI020902FG4";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, userID, userPass, emisor, url, "FD", "Prueba1");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarFolioDigital_ORMEX_Test()
        {
            #region TimbrarFacturacionDigitalTest
            CoveManejador target = new CoveManejador();
            string url = "https://www.foliosdigitalespac.com/WSTimbrado33/WSCFDI33.svc?WSDL"; //URL Servicio PRODUCCION CFDI 3.3

            string resultados = string.Empty;
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\CFDI 3.3\ORMEX\A100220.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CFDI\Diverza\XML\ROGER\VEXP-1216TL-ORMEX_FD.xml";
            string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CFDI\Diverza\XML\ROGER\VEXP-1216TL-DART_FD.xml"; 

            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\CFDI 3.3\ORMEX\00001000000408492388.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\FolioDigital\CFDI 3.3\ORMEXSI\CSD_ORMEX_ORM810330NW5_20171208_162906.key";
            //string passwordCertificado = "ormex2020";
            //string userID = "ORM810330NW5";
            //string userPass = "OcyNeTe&";
            //string emisor = "ORM810330NW5";


            string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\Revisar\00001000000504314190.cer";
            string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Diverza\Dart\Revisar\CSD_DART_DE_TIJUANA_S_DE_RL_DE_CV_DTI0511155E8_20200626_121156";
            string passwordCertificado = "dartdetijuana20";
            string userID = "ORM810330NW5";
            string userPass = "OcyNeTe&";
            string emisor = "DTI0511155E8";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, userID, userPass, emisor, url, "FD", "Prueba_OMEX");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarFolioDigitalCFDI33Test()
        {
            #region TimbrarFolioDigitalCFDI33Test
            CoveManejador target = new CoveManejador();
            string url = "https://www.foliosdigitalespac.com/WSTimbrado33Test/WSCFDI33.svc";

            string resultados = string.Empty;
            string archivoCFDI = @"C:\Users\gluque\Source\Workspaces\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\FD_33.xml";

            string archivoXSLT = @"C:\Users\gluque\Source\Workspaces\COVE\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";

            string archivoCertificado = @"C:\Users\gluque\Source\Workspaces\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\aaa010101aaa.cer";
            string archivoCertificadoKey = @"C:\Users\gluque\Source\Workspaces\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\aaa010101aaa.key";
            string passwordCertificado = "12345678a";

            //resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, "DEMO000719HQ2", "oCZnEtEkK+", "DEMO000719HQ2", url, "FD33", "Prueba1");
            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, "GAI020902D33", "contRa$3na", "GAI020902D33", url, "FD", "0001");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarCarvajalTest()
        {
            #region TimbrarFacturacionDigitalTest
            CoveManejador target = new CoveManejador();
            //string url = "https://ww3.edcinvoice-cfdi.com/cfdiwebservice/CFDIService.asmx";
            string url = "http://ww3.edcinvoice-cfdi.com/cfdiwebservice/CFDIService.asmx";

            string resultados = string.Empty;
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Carvajal\88877_v2 - Carvajal.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Carvajal\Ejemplo XML CCE.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Carvajal\ejemplochedraui.XML";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\ejemplochedraui.XML";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\cfdv33-base.xml";
            string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\cce_1.xml";

            //string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\cadenaoriginal_3_2.xslt";
            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";

            string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Carvajal\20001000000100005870.cer";
            string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Carvajal\20001000000100005870.key";
            string passwordCertificado = "12345678a";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, "901481", "#QvX723@", "FMF970224TA4", url, "CARVAJAL", "Prueba1");

            Assert.IsFalse(resultados.ToUpper().Contains("ERROR"));
            #endregion
        }

        [TestMethod()]
        public void TimbrarEkomercio()
        {
            #region TimbrarEkomercio
            CoveManejador target = new CoveManejador();
            //string url = "https://200.34.168.212/WSCFDIBuilderPlusXML_NoATTest/WSCFDBuilderPlus.asmx";
            string url = "http://148.244.218.50/WSCFDIBuilderPlus/WSCFDBuilderPlus.asmx";

            string resultados = string.Empty;
            //string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Carvajal\Ejemplo XML CCE.xml";
            string archivoCFDI = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\Ekomercio\FACTDEF099 - Ekomercio.xml";

            string archivoXSLT = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\XSLT\cadenaoriginal_3_2.xslt";
            string archivoCertificado = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Ekomercio\CSD01_AAA010101AAA.cer";
            string archivoCertificadoKey = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\Ekomercio\CSD01_AAA010101AAA.key";
            string passwordCertificado = "12345678a";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, "ekomercio", "aserri", "ESI920427886", url, "EKOMERCIO", "Prueba1");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarPaxFacturacionTest()
        {
            #region TimbrarPaxFacturacionTest
            CoveManejador target = new CoveManejador();
            string resultados = string.Empty;
            //string url = "https://test.paxfacturacion.com.mx:453/webservices/wcfRecepcionASMX.asmx";
            string url = "https://www.paxfacturacion.com.mx:453/webservices/wcfRecepcionasmx.asmx?wsdl";

            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";

            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\Diverza\2018-1-001TEST.xml";
            //string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\PaxFacturacion\PAX_TEST.xml";
            string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\PaxFacturacion\CFDIPAX001.xml";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\PaxFacturacion\CSD_Pruebas_CFDI_MAG041126GT8.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\PaxFacturacion\CSD_Pruebas_CFDI_MAG041126GT8.key";
            //string passwordCertificado = "12345678a";

            //string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\PaxFacturacion\00001000000409476637.cer";
            //string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\PaxFacturacion\Claveprivada_FIEL_BMM080124RT5_20180212_141825.key";
            //string passwordCertificado = "PLANCHI2";

            string archivoCertificado = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\PaxFacturacion\00001000000401656956.cer";
            string archivoCertificadoKey = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\CertificadoEjemplos\PaxFacturacion\CSD_Planta_BMM080124RT5_20160307_115627.key";
            string passwordCertificado = "PLANCHI2";


            //string userID = "WSDL_PAX";
            //string userPass = "wqfCr8O3xLfEhMOHw4nEjMSrxJnvv7bvvr4cVcKuKkBEM++/ke+/gCPvv4nvvrfvvaDvvb/vvqTvvoA=";
            //string emisor = "MAG041126GT8";
            string userID = "BMMFacturacion";
            string userPass = "s5dS08N6Hme36VR2rd3Fu7A2rgkBNmfrm4HrYdulMNOujcsQdE13ZMNCKUmhN9HrmXNMsTyLiSDq/obMdkfG9g==";
            string emisor = "BMM080124RT5";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, userID, userPass, emisor, url, "PAXFACTURACION", "Prueba1");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarEDICOMTest()
        {
            #region TimbrarEDICOMTest
            CoveManejador target = new CoveManejador();
            string resultados = string.Empty;

            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";
            string archivoCFDI = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XMLEjemplos\CFDI 3.3\PaxFacturacion\CFDIPAX001.xml";
            string milisegEspera = "2000";
            string reintentos = "2";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, @"C:\Temp\TimbradoEDICOMEntrada", @"C:\Temp\TimbradoEDICOMSalida", milisegEspera, reintentos, null, null, null, "EDICOM", null);

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void TimbrarBuzonETest()
        {
            #region TimbrarBuzonETest
            CoveManejador target = new CoveManejador();
            string resultados = string.Empty;
            string url = "https://prepws.buzone.com.mx:8448/BuzonEIWTimBrado/services/TimbradoCFD?wsdl";

            string archivoXSLT = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\Timbrado\XSLT\CFDI 3.3\cadenaoriginal_3_3.xslt";
            string archivoCFDI = @"C:\Users\gluque\Documents\Gera\Zoe\CFDI_Buzon-E\AmbientePruebas\ESCUELA_KEMPER.xml";

            string archivoCertificado = @"C:\Users\gluque\Documents\Gera\Zoe\CFDI_Buzon-E\AmbientePruebas\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753.cer";
            string archivoCertificadoKey = @"C:\Users\gluque\Documents\Gera\Zoe\CFDI_Buzon-E\AmbientePruebas\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753.key";
            string passwordCertificado = "12345678a";

            string userID = "usrws_032806";
            string userPass = "4ni2MMK#y?2?";
            string emisor = "EKU9003173C9";

            resultados = target.Timbrar(archivoCFDI, archivoXSLT, archivoCertificado, archivoCertificadoKey, passwordCertificado, userID, userPass, emisor, url, "BUZON-E", "Prueba1");

            Assert.IsNotNull(resultados);
            #endregion
        }

        [TestMethod()]
        public void ObtenerEkomercioPDF()
        {
            CoveManejador target = new CoveManejador();
            string resultados = string.Empty;
            string emisorRFC = "ESI920427886";
            string uuid = "4ADB37C8-D46B-4B91-91B6-36CFA47C3BD6";

            resultados = target.CrearEkomercioPDF(emisorRFC, uuid, string.Format("c:\\temp\\{0}.pdf", uuid));

            Assert.IsNotNull(resultados);
        }


        /// <summary>
        ///A test for EnviarComprobante
        ///</summary>
        [TestMethod()]
        public void EnviarComprobanteTest()
        {
            #region EnviarComprobanteTest
            CoveManejador target = new CoveManejador();
            /* Produccion */
            //string urlServicio = "CONTINGENCIA";

            string urlServicio = "https://www.ventanillaunica.gob.mx/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www.ventanillaunica.gob.mx/ventanilla/ConsultarRespuestaCoveService";

            /*
            string userName = "ENS851002FB0";
            string passwordServicioWeb = "MWtuz7TQmYmcM89UaII9TsCHb/A4rAPDNBDle3OIa77JEFxEcYDJ3uOdcgJJ74Mh";
            string rutaCertificadoCert = @"C:\Proyectos\COVE\CoveProxy.Test\00001000000101739962.cer";
            string rutaCertificadoP12 = @"C:\Proyectos\COVE\CoveProxy.Test\00001000000101739962.p12";
            string passwordCertificado = "1050ENSA4";             
            */

            /* Datos de Calpacifico 
            string userName = "PMM1106013Z4";
            string passwordServicioWeb = "PVjBZ/y7UXa3U8OP32Y5WQKH7lT+XYipG+dKZFwh81H81h/Mis3lM7c/T6OLOgVU";
            string rutaCertificadoCert = @"C:\Proyectos\COVE\CoveProxy.Test\pmm1106013z4.cer";
            string rutaCertificadoP12 = @"C:\Proyectos\COVE\CoveProxy.Test\pmm1106013z4.p12";
            string passwordCertificado = "2913HAE21";
             */

            /* Datos de SMTC 
            string userName = "GADS590724TB9";
            string passwordServicioWeb = "cW2dusBiV2N7Tu+9y0LgbNDuMaWIYvPJ4sPSHegkWoBukVCyexIQ+Rme9crAqq4J";
            string rutaCertificadoCert = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\GADS590724tb9\gads590724tb9.cer";
            string rutaCertificadoP12 = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\GADS590724tb9\gads590724tb9.p12";
            string passwordCertificado = "GADS590724TB9KEY";
            */

            /* Datos de Medtronic */
            string userName = "NPB960219TM5";
            string passwordServicioWeb = "MxY4XqRhmlFRFzSHa//HMfTWOpsgO8L8V0X2ycmdNMPiLAmlS8vZUz967+B55n4f";
            string rutaCertificadoCert = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\NPB960219TM5\npb960219tm5.cer";
            string rutaCertificadoP12 = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\NPB960219TM5\npb960219tm5.p12";
            string passwordCertificado = "j2s5sybr2nd1";

            string actual = null;

            /* Pruebas 
            string urlServicio = "https://www2.ventanillaunica.gob.mx:443/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www2.ventanillaunica.gob.mx:443/ventanilla/ConsultarRespuestaCoveService";
            string userName = "TOAR0011165W7";
            string passwordServicioWeb = "TCY2SCOki4mo0EwgiA7X7tfmCMHXqIP+dvAdHFFO/utZgui9hvfs7Zg5t2QVmFBw";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\toar0011165w7.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\toar0011165w7.p12";
            string passwordCertificado = "12345678a";
            string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\Ejemplos Prueba\Ejemplo_GWT921026L97-IA.xml";
            */

            /* Pruebas 
            string urlServicio = "https://www2.ventanillaunica.gob.mx:443/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www2.ventanillaunica.gob.mx:443/ventanilla/ConsultarRespuestaCoveService";

            string userName = "TOAR0011165W7";
            string passwordServicioWeb = "TCY2SCOki4mo0EwgiA7X7tfmCMHXqIP+dvAdHFFO/utZgui9hvfs7Zg5t2QVmFBw";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\toar0011165w7.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\toar0011165w7.p12";
            string passwordCertificado = "12345678a";
            */

            /*
            string urlServicio = "https://www2.ventanillaunica.gob.mx:443/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www2.ventanillaunica.gob.mx:443/ventanilla/ConsultarRespuestaCoveService";
            string userName = "GWT921026L97";
            string passwordServicioWeb = "w1Tgcn1adDiTwz8c/3LapRWOxFKLUuUFRwE9C9zKy6Hjk5nNRC3u2Tv+mB8r25EA";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gwt921026l97.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gwt921026l97.p12";
            string passwordCertificado = "12345678a"; // TODO: Initialize to an appropriate value
            string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\Ejemplos Prueba\Ejemplo_GWT921026L97-IA.xml";
            */


            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\AGRUPAMP.xml";

            //Verificacion de XML con problemas de caracteres
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\FACT0004MQ - Con acento.xml"; //Marco error
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\FACT0004MQ - Sin acento sin espacio.xml"; //No marco error
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\FACT0004MQ - Con espacio.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\FACT0004MQ - Con acento y con espacio.xml";
            //


            //XML con nueva estructura
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\COVE_modificado_4.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\COVE_AcusePDF.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\VUCEMEnvio.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\ejemplo_cove.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\PUNTO90.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\COVE2.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Version 5\COVE3_Cove.xml";

            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\XML Version 5\COVE3_Cove.xml";
            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\COVE_modificado_4.xml";   //Marca: Cadena original : La cadena original es inválida
            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\TEST_COVE.xml";  //Marca: firma invalida
            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\COVEIMPTEST001.xml";  //Con este XML fue EXITOSO si regreso EDOCUMENT
            //string archivoCove = @"C:\ZOE Proyectos\COVE\CoveProxy.Test\COVEIMPTEST001CampoObsModificado.xml";

            //string archivoCove = @"D:\Documentos Personales\Proyectos\COVE\COVE\CoveProxy.Test\TEST_COVE.xml"; //Certificado gads590724tb9.p12
            //string archivoCove = @"C:\Proyectos\COVE\TESTCOVE002.xml"; //Certificado  pmm1106013z4.p12

            //string archivoCove = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\XMLCaracteresEspeciales\18EMOL-BSLP-33.xml"; 
            string archivoCove = @"C:\BTS Proyectos\Temp\COVE\CoveProxy.Test\XMLCaracteresEspeciales\18EPT-0273.xml";

            actual = target.EnviarComprobante(archivoCove, userName, passwordServicioWeb, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicio, urlServicioRespuesta);
            Assert.IsNotNull(actual);
            #endregion                
        }

        /// <summary>
        ///A test for EnviarFacturasNOIA
        ///</summary>
        [TestMethod()]
        public void EnviarFacturasNOIATest()
        {
            #region EnviarFacturasNOIATest
            CoveManejador target = new CoveManejador();
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\FACT0002MQ-FACT0003MQ_NOIA.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\AGRUPADA-AGRUPAMQ-NOIA.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\FACT0002MQ-FACT0003MQ.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\COVE2-PUNTO90_NOIA.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Version 5\COVE3-COVE4_NOIA.xml";
            string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Version 5\DOSPARTES-TEST00004_NOIA.xml";

            string urlServicio = "https://www.ventanillaunica.gob.mx/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www.ventanillaunica.gob.mx/ventanilla/ConsultarRespuestaCoveService";
            string userName = "ENS851002FB0";
            string passwordServicioWeb = "MWtuz7TQmYmcM89UaII9TsCHb/A4rAPDNBDle3OIa77JEFxEcYDJ3uOdcgJJ74Mh";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.p12";
            string passwordCertificado = "1050ENSA4";
            string actual;

            actual = target.EnviarFacturasNOIA(archivoCove, userName, passwordServicioWeb, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicio, urlServicioRespuesta);
            Assert.IsNotNull(actual);
            #endregion
        }

        /// <summary>
        ///A test for EnviarFacturasIA
        ///</summary>
        [TestMethod()]
        public void EnviarFacturasIATest()
        {
            CoveManejador target = new CoveManejador(); // TODO: Initialize to an appropriate value
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\FACT0002MQ-FACT0003MQ_IA.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\AGRUPADA-AGRUPAMQ-IA.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\FACT0003MQ-FACT0004MQ.xml";
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\COVE2-PUNTO90_IA.xml";

            /*
            string urlServicio = "https://www.ventanillaunica.gob.mx/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www.ventanillaunica.gob.mx/ventanilla/ConsultarRespuestaCoveService";
            string userName = "ENS851002FB0";
            string passwordServicioWeb = "MWtuz7TQmYmcM89UaII9TsCHb/A4rAPDNBDle3OIa77JEFxEcYDJ3uOdcgJJ74Mh";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.p12";
            string passwordCertificado = "1050ENSA4";
            */

            /* Pruebas */
            string urlServicio = "https://www2.ventanillaunica.gob.mx:443/ventanilla/RecibirCoveService";
            string urlServicioRespuesta = "https://www2.ventanillaunica.gob.mx:443/ventanilla/ConsultarRespuestaCoveService";

            string userName = "GWT921026L97";
            string passwordServicioWeb = "2eSBtD+Go01Kaka4YoLqrsvDWrP1AZn1nFZTQmT/3S7EFZVD5Pvju09ybA5nwMCK";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gwt921026l97.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gwt921026l97.p12";
            string passwordCertificado = "12345678a"; // TODO: Initialize to an appropriate value

            string actual;

            string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\Ejemplos Prueba\Ejemplo_GWT921026L97-IA.xml";

            actual = target.EnviarFacturasIA(archivoCove, userName, passwordServicioWeb, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicio, urlServicioRespuesta);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for ObtenerRespuesta
        ///</summary>
        [TestMethod()]
        public void ObtenerRespuestaTest()
        {
            CoveManejador target = new CoveManejador();

            string userName = "ENS851002FB0";
            string password = "MWtuz7TQmYmcM89UaII9TsCHb/A4rAPDNBDle3OIa77JEFxEcYDJ3uOdcgJJ74Mh";
            string numeroOperacion = "40485";
            string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.cer";
            string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.p12";
            string passwordCertificado = "1050ENSA4";
            string urlServicioRespuesta = "https://www.ventanillaunica.gob.mx/ventanilla/ConsultarRespuestaCoveService";
            string actual;
            actual = target.ObtenerRespuesta(userName, password, numeroOperacion, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicioRespuesta);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for EnviarComprobanteVersionNueva
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CoveProxy.dll")]
        public void EnviarComprobanteVersionNuevaTest()
        {
            CoveManejador_Accessor target = new CoveManejador_Accessor(); // TODO: Initialize to an appropriate value
            //string archivoCove = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\COVE_modificado.xml";
            string archivoCove = @"C:\aaa\TESTCOVE001.xml";

            //string userName = "GWT921026L97";
            //string passwordServicioWeb = "2eSBtD+Go01KaKa4YoLqrsvDWrP1AZn1nFZTQmT/3S7EFZVD5Pvju09ybA5nwMCK";
            //string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gwt921026l97.cer";
            //string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gwt921026l97.p12";
            //string passwordCertificado = "12345678a"; 

            //string userName = "ENS851002FB0";
            //string passwordServicioWeb = "MWtuz7TQmYmcM89UaII9TsCHb/A4rAPDNBDle3OIa77JEFxEcYDJ3uOdcgJJ74Mh";
            //string rutaCertificadoCert = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.cer";
            //string rutaCertificadoP12 = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\00001000000101739962.p12";
            //string passwordCertificado = "1050ENSA4";

            string userName = "GADS590724TB9";
            string passwordServicioWeb = "cW2dusBiV2N7Tu+9y0LgbNDuMaWIYvPJ4sPSHegkWoBukVCyexIQ+Rme9crAqq4J";
            string rutaCertificadoCert = @"C:\Users\gerardo\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gads590724tb9.cer";
            string rutaCertificadoP12 = @"C:\Users\gerardo\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gads590724tb9.p12";
            string passwordCertificado = "GADS590724TB9KEY";

            //string urlServicio = "https://www2.ventanillaunica.gob.mx:443/ventanilla/RecibirCoveService";
            //string urlServicioRespuesta = "https://www2.ventanillaunica.gob.mx:443/ventanilla/ConsultarRespuestaCoveService";

            string urlServicio = "https://www.ventanillaunica.gob.mx/ventanilla-8/RecibirCoveService";
            string urlServicioRespuesta = "https://www.ventanillaunica.gob.mx/ventanilla/ConsultarRespuestaCoveService";


            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            //actual = target.EnviarComprobanteVersionNueva(archivoCove, userName, passwordServicioWeb, rutaCertificadoCert, rutaCertificadoP12, passwordCertificado, urlServicio, urlServicioRespuesta);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LeerFacturasIA
        ///</summary>
        [TestMethod()]
        public void LeerFacturasIATest()
        {
            CoveManejador target = new CoveManejador();
            string archivoFacturasIA = @"C:\Users\gluque\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\XML Nueva estructura\FACT0003MQ-FACT0004MQ.xml";
            ComprobanteValorElectronicoIA actual;
            actual = target.LeerFacturasIA(archivoFacturasIA);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for ConsultarListaPedimentos
        ///</summary>
        [TestMethod()]
        public void ConsultarListaPedimentosTest()
        {
            CoveManejador target = new CoveManejador(); // TODO: Initialize to an appropriate value
            string[] expected = null; // TODO: Initialize to an appropriate value
            string[] actual;
            actual = target.ConsultarListaPedimentos();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod()]
        public void ObtenerUsuarioTest()
        {
            var target = new CoveManejador();
            string expected = @"BTS\gluque";
            var actual = target.ObtenerUsuario();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void EnviarFacturaLaserTest()
        {
            var target = new CoveManejador();
            var archivoXML = @"C:\Temp\960001.xml";
            var urlServicio = "https://apps9.laser.com.mx:443/WebServicesPro/Pedimento5.php/FacturasVenta";

            var response = target.EnviarFacturaLaser(archivoXML, urlServicio);

            Assert.IsNotNull(response);
        }

        [TestMethod()]
        public void EscribirArchivoLogTest()
        {
            ManejoArchivos.BorrarArchivoLog("Test");

            ManejoArchivos.EscribirArchivoLog("Test", "Primera linea");
            ManejoArchivos.EscribirArchivoLog("Test", "Segunda linea");
            ManejoArchivos.EscribirArchivoLog("Test", "Tercera linea");
        }
    }
}
