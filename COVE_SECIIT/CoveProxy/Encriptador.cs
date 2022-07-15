using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography;

namespace CoveProxy
{
    public class Encriptador
    {
        private X509Certificate2 ObtenerCertificado(string rutaArchivoCert, string password)
        {
            X509Certificate2 x509 = new X509Certificate2();

            byte[] rawData = Utilerias.ManejoArchivos.FileBytes(rutaArchivoCert);

            //x509.Import(rawData, password, X509KeyStorageFlags.DefaultKeySet);           
            x509.Import(rawData, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);           
            
            return x509;
        }

        private X509Certificate2 ObtenerCertificadoDeStore(string rutaArchivoCert, string password)
        {
            X509Certificate2 publicCert = new X509Certificate2(rutaArchivoCert, password);

            X509Certificate2 privateCert = null;
            
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (cert.GetCertHashString() == publicCert.GetCertHashString())
                {
                    privateCert = cert;
                    break;
                }
            }

            if (privateCert == null)
            {
                X509Store storeMachine = new X509Store(StoreLocation.LocalMachine);
                storeMachine.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 cert in storeMachine.Certificates)
                {
                    if (cert.GetCertHashString() == publicCert.GetCertHashString())
                    {
                        privateCert = cert;
                        break;
                    }
                }
            }

            return privateCert;
        }
        

        //public byte[] RSASignToByteArray(string data, string rutaCertificado, string password)
        //{
        //    X509Certificate2 certificate = ObtenerCertificado(rutaCertificado, password);
        //    RSACryptoServiceProvider rsaCsp = (RSACryptoServiceProvider)certificate.PrivateKey;

        //    byte[] dataBytes = Encoding.ASCII.GetBytes(data);

        //    byte[] signatureBytes = rsaCsp.SignData(dataBytes, "SHA1");

        //    return signatureBytes;
        //}

        public byte[] RSASignToByteArray(string data, string rutaCertificado, string password)
        {
            //X509Certificate2 certificate = ObtenerCertificadoDeStore(rutaCertificado, password);
            X509Certificate2 certificate = ObtenerCertificado(rutaCertificado, password);

            byte[] signatureBytes = Firmar(data, certificate);

            return signatureBytes;
        }

        private byte[] Firmar(string data, X509Certificate2 certificate)
        {
            RSACryptoServiceProvider key = null;

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            string hashAlgoritmo = string.Empty;

            switch (certificate.SignatureAlgorithm.FriendlyName)
            {
                case "sha256RSA":
                    key = new RSACryptoServiceProvider();
                    key.FromXmlString(certificate.PrivateKey.ToXmlString(true));
                    hashAlgoritmo = CryptoConfig.MapNameToOID("SHA256");
                    break;
                case "sha1RSA":
                    key = (RSACryptoServiceProvider)certificate.PrivateKey;
                    hashAlgoritmo = "SHA1";
                    break;
                default:
                    key = (RSACryptoServiceProvider)certificate.PrivateKey;
                    hashAlgoritmo = "SHA1";
                    break;
            }
           
            byte[] signatureBytes = key.SignData(dataBytes, hashAlgoritmo);

            return signatureBytes;
        }
    }
}
