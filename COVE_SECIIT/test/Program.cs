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
            var p = new ParametrosTimbrado();

            p.layout = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\xml_timbrado.xml";
            p.archivoXslt = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\cadenaoriginal_3_3.xslt";
            p.urlTimbrado = "http://services.test.sw.com.mx/cfdi33/issue/v3";
            p.userID = "omar_figueroa@jabil.com";
            p.userPass = "Jabil+SW";
            p.PAC = "SW";
            p.archivoCertificado = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753s.cer";
            p.archivoKey = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753.key";
            p.certificadoPass = "12345678a";
            p.referencia = "T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbXB3YVZxTHdOdHAwVXY2NTdJb1hkREtXTzE3dk9pMmdMdkFDR2xFWFVPUXpTUm9mTG1ySXdZbFNja3FRa0RlYURqbzdzdlI2UUx1WGJiKzViUWY2dnZGbFloUDJ6RjhFTGF4M1BySnJ4cHF0YjUvbmRyWWpjTkVLN3ppd3RxL0dJPQ.T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbFlVcU92YUJTZWlHU3pER1kySnlXRTF4alNUS0ZWcUlVS0NhelhqaXdnWTRncklVSWVvZlFZMWNyUjVxYUFxMWFxcStUL1IzdGpHRTJqdS9Zakw2UGRYTWRoZVR0T3hQWFYzUCszK0tQWTRHWHpVVE5sSVNUQTQ0QWw3clhXTFIwNlJET1RiVWl1Q1Y2YzdTelVDRzlNRlZJTGNoNjRFb3lIZFpFd1ZORzMwNjIzOGp0dW42UDdhRVJOOTN5UWwrYmN2bEJPSS9GTndabkp4Qmo3czZIVE1uMjcxQm5PaW1OU3VIOVB0UURJbkxCQWpMMk9xZEV1QjhYWmp6R3RtdGNESWtjVVZhWUQ5S3NBN08yL0szWTNiVzh0SEFJRUthK2t0WEEwUGdaYnN1UnVJRTZoNzh0TjB4TXRQbVJiU2ozUTBzQUtEd0VqeUViVDR2U3puVnZZaHFrVHdpSm9jeWd1TDRiejV0WWsyWTJqcUMzaUtIMkNjSlZuMHIxcmZYUEhPYW9VVmFOZ1hoeVVxVm12dlFtWExMT3JFeE5EYW1qdVV1dXE3OGozQ0JOM0REdHVTaTV6ZnVuQjA0MU9CSTdEd0NOUmFveUVYRkx4TWN5VVZTKzg.oXbkerv_PNB3HmlJrDAaao8wOTJWwizrl-y04G_BqrA";

            var timbrado = new CoveManejador().Timbrar(
                p.layout, p.archivoXslt, p.archivoCertificado, p.archivoKey, p.certificadoPass,
                p.userID, p.userPass, p.emisorRFC, p.urlTimbrado, p.PAC, p.referencia);

            //var p = new ParametrosTimbrado();
            //p.layout = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\f6439fd1-8614-4f6e-93ef-747e05205fd9.xml";
            //p.archivoXslt = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\cadenaoriginal_3_3.xslt";
            //p.urlPDF = "http://api.test.sw.com.mx/pdf/v1/api/GeneratePdf";
            //p.urlTimbrado = "http://services.test.sw.com.mx/cfdi33/issue/v3";
            //p.userID = "omar_figueroa@jabil.com";
            //p.userPass = "Jabil+SW";
            //p.userPass = "T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbXB3YVZxTHdOdHAwVXY2NTdJb1hkREtXTzE3dk9pMmdMdkFDR2xFWFVPUXpTUm9mTG1ySXdZbFNja3FRa0RlYURqbzdzdlI2UUx1WGJiKzViUWY2dnZGbFloUDJ6RjhFTGF4M1BySnJ4cHF0YjUvbmRyWWpjTkVLN3ppd3RxL0dJPQ.T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbFlVcU92YUJTZWlHU3pER1kySnlXRTF4alNUS0ZWcUlVS0NhelhqaXdnWTRncklVSWVvZlFZMWNyUjVxYUFxMWFxcStUL1IzdGpHRTJqdS9Zakw2UGRYTWRoZVR0T3hQWFYzUCszK0tQWTRHWHpVVE5sSVNUQTQ0QWw3clhXTFIwNlJET1RiVWl1Q1Y2YzdTelVDRzlNRlZJTGNoNjRFb3lIZFpFd1ZORzMwNjIzOGp0dW42UDdhRVJOOTN5UWwrYmN2bEJPSS9GTndabkp4Qmo3czZIVE1uMjcxQm5PaW1OU3VIOVB0UURJbkxCQWpMMk9xZEV1QjhYWmp6R3RtdGNESWtjVVZhWUQ5S3NBN08yL0szWTNiVzh0SEFJRUthK2t0WEEwUGdaYnN1UnVJRTZoNzh0TjB4TXRQbVJiU2ozUTBzQUtEd0VqeUViVDR2U3puVnZZaHFrVHdpSm9jeWd1TDRiejV0WWsyWTJqcUMzaUtIMkNjSlZuMHIxcmZYUEhPYW9VVmFOZ1hoeVVxVm12dlFtWExMT3JFeE5EYW1qdVV1dXE3OGozQ0JOM0REdHVTaTV6ZnVuQjA0MU9CSTdEd0NOUmFveUVYRkx4TWN5VVZTKzg.oXbkerv_PNB3HmlJrDAaao8wOTJWwizrl-y04G_BqrA";
            //p.PAC = "SW";
            //p.archivoCertificado = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753s.cer";
            //p.archivoKey = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\CSD_EKU9003173C9_20190617131829\CSD_Escuela_Kemper_Urgate_EKU9003173C9_20190617_131753.key";
            //p.certificadoPass = "12345678a";
            //p.uuid = "0e788ede-3666-4fe4-907c-638925d934b1";
            //p.logo = @"D:\ZOE_IT_DEV\_PRUEBAS_\COVE\zoe_logo.png";

            ////var timbrado = new CoveManejador().PDFSW(p.layout, p.uuid, p.logo, p.urlTimbrado, p.urlPDF, p.userID, p.userPass);

            //var timbrado = new CoveManejador().Timbrar(p.layout, p.uuid, p.logo, p.urlPDF, p.certificadoPass,"PDF", p.userPass, p.emisorRFC, p.urlTimbrado, p.PAC, p.userPass);
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
