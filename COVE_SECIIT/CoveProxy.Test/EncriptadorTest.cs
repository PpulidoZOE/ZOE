using CoveProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Utilerias;

namespace CoveProxy.Test
{
    
    
    /// <summary>
    ///This is a test class for EncriptadorTest and is intended
    ///to contain all EncriptadorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EncriptadorTest
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
        ///A test for Encriptar
        ///</summary>
        [TestMethod()]
        public void EncriptarTest()
        {
            Encriptador target = new Encriptador(); // TODO: Initialize to an appropriate value
            //string infoEncriptar = "|0|TOAR0011165W7|TOAR0011165W7001|0|0|2011-11-11|0|EXP20111115|1|TOAR221116SWSXXXX1|TOAR0011165W7|PRUEBA DEL USUARIO TOAR0011165W7|1|GUZN5008099V4|PATERNO TOAR0011165W7|MATERNO TOAR0011165W7|NOMBRE GUZN5008099V4|DOMICILIO TOAR0011165W7|111|0|COL TOAR0011165W7|MEXICO|NINGUNO|MIGUEL HIDALGO|DF|MEX|111111|2|TOAR221116SWSXXXX1|DESTINATARIO|DESTINATARIO|DESTINATARIO|CALLE|EXT|INT|COLONIA|LOC|REF|MUN|EF|MEX|111111|TOAR0011165W7|2|20.000|20.00|20.00|20.0000|TOAR0011165W7|TOAR0011165W7|TOAR0011165W7|TOAR0011165W7|"; 
            //string infoEncriptar = "|1|ENS851002FB0|FACT0004MQ|0|0|2012-01-22|0|4|ENS851002FB0|1|ENS851002FB0|ENSATEC S.A. DE C.V.|CALLE é COLINAS|11730|COLINAS|TIJUANA|TITJUANA|BAJA CALIFORNIA|MEX|22680|0|irs|cliente para submaquila|calle int|no ext|no int|col|loc|muni|CA|USA|111|maq 11|6|240.000|27.00|324.00|24.0000|ruda|2001|3212312312312312|";
            //string infoEncriptar = "|TOCE.IMP|ITUTUET66545|0|2011-11-15|5|Esta es una prueba del webservice de Cove|APH0609268C2|AFC000526BJ2|0023|0097|0|1|9984882|1|APH0609268C2|MARTINEZ|ORTIZ|MARIA|leo|23|23|BOSQUES DE LAS LOMAS|MEXICO|ECATEPEC|MEXICO|MEX|55567|1|AFC000526BJ2|BETANCOURT|MARTINEZ|FELIPE|CAPRICORNIO|78|78|PRADOS DE ECATEPEC|ECATEPEC|TULTITLAN|MEXICO|MEX|66532|CUADERNOS|2|USD|200.000|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|";
            //string infoEncriptar = "|TOCE.EXP|KM2013FE0005|0|2013-01-08|4|Numero Econ&amp;oacute;mico: no aplica  Tipo de Contendor:   Candados Oficiales:   Numero de Pedimento: 130709733000195  E-Documents: IA-35/11 04361200MEU82|0973|0|0|1|GAI020902FG4|GRUPO AMERICAN INDUSTRIES, S.A. DE C.V.|AVE. WASHINGTON|3701|EDIFICIO 18|PANAMERICANA|CHIHUAHUA|CHIHUAHUA|CHIHUAHUA|MEX|31210|0|06-0858386|KAMAN AEROSPACE CORPORATION|GUN CLUB ROAD|227|JACKSONVILLE|JACKSONVILLE|FI|USA|32218|PLACA A TIERRA|EA|5.000|USD|14.120000|70.600000|70.6000|CANAL|EA|3.000|USD|34.000000|102.000000|102.0000|";
            string infoEncriptar = "|TOCE.EXP|TESTCOVE001|0|2015-07-08|1|GATR730803K93|3435|0|0|1|GADS590724TB9|DEMO DE RL DE CV|CALLE 09|9999|CIUDAD INDUSTRIAL|YUCATAN|YUCATAN|YUCATAN|MEX|97777|0|86597211|DEMO, INC. S. A. de C. V. de R. L. de S. V. S. etc etc etc etc|calle int|no ext|no int|col|loc|muni|CA|USA|111|1019 ENSAMBLE DE CONECTOR DE METAL PARA LA INDUSTRIA AUTOMATRIZ|C62_1|1.000|USD|120.460000|120.460000|120.4600|";

            //string rutaCertificado = @"C:\Proyectos\COVE\CoveProxy.Test\toar0011165w7.p12";
            //string password = "12345678a"; 
                        
            string rutaCertificado = @"C:\Proyectos\COVE\CoveProxy.Test\GADS590724tb9\gads590724tb9.p12";
            string password = "GADS590724TB9KEY";

            //string rutaCertificado = @"C:\Proyectos\COVE\CoveProxy.Test\pmm1106013z4.p12";
            //string password = "2913HAE21";

            string firma = string.Empty;
            
            //actual = target.Encriptar(infoEncriptar, rutaCertificado, password);
            //firma = Utilerias.ManejoArchivos.ByteArrayToStringHex(actual);

            //firma = target.RSASign(infoEncriptar, rutaCertificado, password);

            byte[] signed = new byte[] { };
            
            //signed = target.RSASignToByteArray(infoEncriptar, rutaCertificado, password);

            signed = target.RSASignToByteArray(infoEncriptar, rutaCertificado, password);

            firma = Utilerias.ManejoArchivos.ByteArrayToStringHex(signed);

            Assert.IsFalse(firma == string.Empty);
        }

        /// <summary>
        ///A test for RSASignToByteArray
        ///</summary>
        [TestMethod()]
        public void RSASignToByteArrayTest()
        {
            Encriptador target = new Encriptador(); 
            //string data = "|TOCE.IMP|ITUTUET66545|0|2011-11-15|5|Esta es una prueba del webservice de Cove|APH0609268C2|AFC000526BJ2|0023|0097|0|1|9984882|1|APH0609268C2|MARTINEZ|ORTIZ|MARIA|leo|23|23|BOSQUES DE LAS LOMAS|MEXICO|ECATEPEC|MEXICO|MEX|55567|1|AFC000526BJ2|BETANCOURT|MARTINEZ|FELIPE|CAPRICORNIO|78|78|PRADOS DE ECATEPEC|ECATEPEC|TULTITLAN|MEXICO|MEX|66532|CUADERNOS|2|USD|200.000|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|";
            //string data = "|TOCE.IMP|ITUTUET66545|0|2011-11-15|5|Esta es una prueba del webservice de Cove|APH0609268C2|AFC000526BJ2|3491|0|1|9984882|1|APH0609268C2|MARTINEZ|ORTIZ|MARIA|leo|23|23|BOSQUES DE LAS LOMAS|MEXICO|ECATEPEC|MEXICO|MEX|55567|1|AFC000526BJ2|BETANCOURT|MARTINEZ|FELIPE|CAPRICORNIO|78|78|PRADOS DE ECATEPEC|ECATEPEC|TULTITLAN|MEXICO|MEX|66532|CUADERNOS|2|200.000|USD|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|";
            //string data = "|TOCE.IMP|ITUTUET66545|0|2011-11-15|5|Esta es una prueba del webservice de Cove|APH0609268C2|AFC000526BJ2|3491|0|1|9984882|1|APH0609268C2|MARTINEZ|ORTIZ|MARIA|leo|23|23|BOSQUES DE LAS LOMAS|MEXICO|ECATEPEC|MEXICO|MEX|55567|1|AFC000526BJ2|BETANCOURT|MARTINEZ|FELIPE|CAPRICORNIO|78|78|PRADOS DE ECATEPEC|ECATEPEC|TULTITLAN|MEXICO|MEX|66532|CUADERNOS|2|200.000|USD|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|CUADERNOS|2|200.000|USD|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|";
            //string data = "|TOCE.IMP|TEST001V3|0|2012-03-14|5|TEST TET|GATR730803K93|3491|0|0|3|SIN TAX ID|EMPRESA DE PRUEBA|CALLE MONARCAS|95|USA|1|ENS851002FB0|ENSATEC|COLINAS|11730|PARQUE IND FLORIDO|TIJUANA|BAJA CALIFORNIA|MEX|22244|MERCANCIA DE PRUEBA|6|25.000|USD|1500.000000|37500.0000|MARCA DE PRUEBA|";
            //string data = "|TOCE.IMP|ITUTUET66545|0|2011-11-15|5|Esta es una prueba del webservice de Cove|APH0609268C2|AFC000526BJ2|3491|0|1|9984882|1|APH0609268C2|MARTINEZ|ORTIZ|MARIA|leo|23|23|BOSQUES DE LAS LOMAS|MEXICO|ECATEPEC|MEXICO|MEX|55567|1|AFC000526BJ2|BETANCOURT|MARTINEZ|FELIPE|CAPRICORNIO|78|78|PRADOS DE ECATEPEC|ECATEPEC|TULTITLAN|MEXICO|MEX|66532|CUADERNOS|2|200.000|USD|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|CUADERNOS|2|200.000|USD|10.000000|2000.000000|200000.0000|SCRIBE|DE987|C-4567|LA-02012011-WE|";
            string data = "|TOCE.EXP|TESTCOVE001|0|2015-07-08|1|GATR730803K93|3435|0|0|1|GADS590724TB9|DEMO DE RL DE CV|CALLE 09|9999|CIUDAD INDUSTRIAL|YUCATAN|YUCATAN|YUCATAN|MEX|97777|0|86597211|DEMO, INC. S. A. de C. V. de R. L. de S. V. S. etc etc etc etc|calle int|no ext|no int|col|loc|muni|CA|USA|111|1019 ENSAMBLE DE CONECTOR DE METAL PARA LA INDUSTRIA AUTOMATRIZ|C62_1|1.000|USD|120.460000|120.460000|120.4600|";


            string rutaCertificado = @"C:\Users\gerardo\Documents\Visual Studio 2010\Projects\COVE\CoveProxy.Test\gads590724tb9.p12";
            string password = "GADS590724TB9KEY"; // TODO: Initialize to an appropriate value
            byte[] actual;
            string firma = string.Empty;
            actual = target.RSASignToByteArray(data, rutaCertificado, password);
            firma = Convert.ToBase64String(actual);
            Assert.IsTrue(firma.Length > 0);
            
            //KigkhdNavhiRJuxEkUoQjRQlxOMg7tONL/nR4absDcxY67VUX9G27zXgTq2MvxPhRGwAHKSdjIQM16Zlic757tZiSKDsfV3eYcKlX+vFKNyzGlgwI08xp250bZmwWJDa1Va7YiAN/sVtuK3sfbBu/wIsuM5cyMDzX786Ryyxk9Q=
            //u9278QlugbtUDQ5gr2djPSldqU/rBhJVWBUVg7+go8KcCzUTMbrlL/Awk4QBKPJXjeRIoW6+F8vBP/OHrt+ul/QxK0VdJsu+WaW4+aH5lvZuDxY1LlXL6mQ8RzcedQ2Zy7TY02vOBWgy54XNoZtqTIX6Ug4jIt5akPPQ+ZpmXX8=

            //XAAH2FL7TLEpdjg49crj96z5aCqEdCnoJXi9fL//tBukkFOJfzhDjOaRAUWtBnF21+IpzWz2WVHxTJ1EA9EyGdcqwHgDdFdQVgbWuJlKv5llxL9gDScMAgou6BcRqa1IuGa14nSQf6Quei2gMIXLNdC4DgqKOGmUO7l63x03OQQ=

            //Firma del ejemplo
            //rFKpcP28jbZD+v00DdFIogjGjS2znlIwlVgMTt87fcLUkcN2ip/5ann+a3RJcCAjLimvOD0wpx38xOTDvMpr9TRt4BaKknfMnc3q0/WI+tXY1CeWRIX6slHRk31IceOv2SB0PFxctiyupvO/ZL+HLylAYf58WllS0XZgtvobedc=
            //rFKpcP28jbZD+v00DdFIogjGjS2znlIwlVgMTt87fcLUkcN2ip/5ann+a3RJcCAjLimvOD0wpx38xOTDvMpr9TRt4BaKknfMnc3q0/WI+tXY1CeWRIX6slHRk31IceOv2SB0PFxctiyupvO/ZL+HLylAYf58WllS0XZgtvobedc=
        }
    }
}
