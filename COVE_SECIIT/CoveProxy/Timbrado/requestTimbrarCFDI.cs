﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoveProxy
{
    [Serializable]    
    public class requestTimbrarCFDI
    {
        public string text2CFDI;
        public string UserID;
        public string UserPass;
        public string emisorRFC;
        public bool generarCBB;
        public bool generarTXT;
        public bool generarPDF;
        public string urlTimbrado;
        public string proxy_user;
        public string proxy_pass;
        public string proxy_url;
        public int proxy_port;

        public requestTimbrarCFDI()
        {
            // Configuración inicial para la conexion con el servios SOAP de Timbrado
            this.UserID = "UsuarioPruebasWS";
            this.UserPass = "b9ec2afa3361a59af4b4d102d3f704eabdf097d4";
            this.emisorRFC = "ESI920427886";
            this.generarCBB = true;
            this.generarPDF = true;
            this.generarTXT = false;
            this.urlTimbrado = "https://t1demo.facturacionmoderna.com/timbrado/soap";
            this.proxy_url = "";
            this.proxy_pass = "";
            this.proxy_port = 80;
            this.proxy_user = "";
        }
    }
}
