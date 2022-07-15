using System;
using System.Runtime.InteropServices;

namespace CoveProxy
{
    [ComVisible(true)]
    [Guid("B4C7BE8F-3A6B-4CFC-A9A7-F1D9BB1B140E")]
    public interface ICoveManejador
    {
        [ComVisible(true)]
        [DispId(0X01000001)]
        string EnviarComprobante(string archivoCove, string userName, string passwordServicioWeb, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio, string urlServicioRespuesta);

        [ComVisible(true)]
        [DispId(0X01000002)]
        string EnviarFacturasNOIA(string archivoFacturasNOIA, string userName, string passwordServicioWeb, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio, string urlServicioRespuesta);

        [ComVisible(true)]
        [DispId(0X01000003)]
        string EnviarFacturasIA(string archivoFacturasIA, string userName, string passwordServicioWeb, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio, string urlServicioRespuesta);

        [ComVisible(true)]
        [DispId(0X01000004)]
        string ObtenerRespuesta(string userName, string password, string numeroOperacion, string rutaCertificadoCert, string rutaCertificadoP12, string passwordCertificado, string urlServicio);

        [ComVisible(true)]
        [DispId(0X01000005)]
        string[] ConsultarListaPedimentos();

        [ComVisible(true)]
        [DispId(0X01000007)]
        pedimentoregresar ConsultarPedimento();

        [ComVisible(true)]
        [DispId(0X01000008)]
        string Timbrar(string layout, string archivoXslt, string archivoCertificado, string archivoKey, string certificadoPass, string userID, string userPass, string emisorRFC, string urlTimbrado, string PAC, string referencia);

        [ComVisible(true)]
        [DispId(0X01000009)]
        string CrearEkomercioPDF(string emisorRFC, string uuid, string rutaNombreArchivoPDF);

        [ComVisible(true)]
        [DispId(0X01000010)]
        string ObtenerUsuario();

        [ComVisible(true)]
        [DispId(0X01000011)]
        string EnviarFacturaLaser(string archivoXML, string urlServicioWeb);
    }

    [ComVisible(true)]
    public class pedimentoregresar
    {
        public string Numero;
        public string Descripcion;
    }
}
