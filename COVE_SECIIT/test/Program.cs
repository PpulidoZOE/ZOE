using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoveProxy;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var p = new ParametrosTimbrado();

            //p.layout = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\xml_timbrado.xml";
            //p.archivoXslt = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\cadenaoriginal_3_3.xslt";
            //p.urlTimbrado = "http://services.test.sw.com.mx/cfdi33/issue/v3";
            //p.userID = "omar_figueroa@jabil.com";
            //p.userPass = "Jabil+SW";
            //p.PAC = "SW";
            //p.archivoCertificado = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753s.cer";
            //p.archivoKey = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753.key";
            //p.certificadoPass = "12345678a";

            //var timbrado = new CoveManejador().Timbrar(
            //    p.layout , p.archivoXslt , p.archivoCertificado, p.archivoKey , p.certificadoPass , 
            //    p.userID, p.userPass, p.emisorRFC, p.urlTimbrado, p.PAC, p.referencia);

            var p = new ParametrosTimbrado();

            p.layout = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\b606748b-983a-4383-8e08-a719822077f8.xml";
            p.archivoXslt = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\cadenaoriginal_3_3.xslt";
            p.urlPDF = "http://api.test.sw.com.mx/pdf/v1/api/GeneratePdf";
            p.urlTimbrado = "http://services.test.sw.com.mx/cfdi33/issue/v3";
            p.userID = "omar_figueroa@jabil.com";
            p.userPass = "Jabil+SW";
            p.PAC = "SW";
            p.archivoCertificado = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753s.cer";
            p.archivoKey = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753.key";
            p.certificadoPass = "12345678a";
            p.uuid = "b606748b-983a-4383-8e08-a719822077f8";
            p.logo = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\zoe_logo.png";

             var timbrado = new CoveManejador().PDFSW(p.layout, p.uuid, p.logo, p.urlTimbrado, p.urlPDF, p.userID, p.userPass);

        }

        public class ParametrosTimbrado
        {   
            public string layout { get; set; }
            public string archivoXslt { get; set;}
            public string archivoCertificado { get; set; }
            public string archivoKey { get; set; }
            public string certificadoPass { get; set; }
            public string userID { get; set; }
            public string userPass { get; set; }
            public string emisorRFC { get; set; }
            public string urlTimbrado { get; set; }
            public string PAC { get; set; }
            public string referencia { get; set; }
            public string uuid { get; set; }
            public string urlPDF { get; set; }
            public string logo { get; set; }
        }
    }
}
