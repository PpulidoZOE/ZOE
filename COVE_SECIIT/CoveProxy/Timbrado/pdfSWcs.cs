using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoveProxy.Timbrado
{
    public class pdfSW
    {

        public string xmlContent;
        public string logo;
        //public string extras;
        public string templateId;
    }

    public class EmailSettings
    {
        private string FilePath;
        private string IniFilePath;
        private string IniFileName;
        private string AttachmentPath;
        private string SmtpServer;
        private string SmtpPuerto;
        private string SmtpUser;
        private string SmtpPassword;
        private string EmailSender;
        private string EmailSubject;
        private string EmailBody;
        private string EmailRecipients;
        private string[] FileAttachments;
        private string[] EmailRecipientsSep;

        public string FilePath1 { get => FilePath; set => FilePath = value; }
        public string SmtpServer1 { get => SmtpServer; set => SmtpServer = value; }
        public string SmtpPuerto1 { get => SmtpPuerto; set => SmtpPuerto = value; }
        public string SmtpUser1 { get => SmtpUser; set => SmtpUser = value; }
        public string SmtpPassword1 { get => SmtpPassword; set => SmtpPassword = value; }
        public string EmailSender1 { get => EmailSender; set => EmailSender = value; }
        public string EmailSubject1 { get => EmailSubject; set => EmailSubject = value; }
        public string EmailBody1 { get => EmailBody; set => EmailBody = value; }
        public string EmailRecipients1 { get => EmailRecipients; set => EmailRecipients = value; }


        private void getParentFolder (string FullPath)
        {

            AttachmentPath = Directory.GetParent(FullPath).FullName;
            IniFilePath = Directory.GetParent(AttachmentPath).FullName;

        }

        private void getFileAttachments(string AttachmentPath)
        {
            FileAttachments = Directory.GetFiles(AttachmentPath);
        }

        private void getRecipients(string recip)
        {
            EmailRecipientsSep = recip.Split(',');
        }
        public void getEmailData(string FilePath)
        {
            string line;
            string line2;

            string pattern = @"\[([^\[\]]+)\]";

            getParentFolder(FilePath);
            getFileAttachments(AttachmentPath);

            IniFileName = "EmailCFDI.ini";
            IniFilePath = Path.Combine(IniFilePath, IniFileName);

            string[][] stringSeparators = { new string[] { "SmtpServer" }, new string[] { "SmtpPuerto" }, new string[] { "SmtpUser" },
                                            new string[] { "SmtpPassword" }, new string[] { "EmailSender" }, new string[] { "EmailSubject" },
                                            new string[] { "EmailBody" }, new string[] { "EmailRecipients" } };
            try
            {
                using (StreamReader sr = new StreamReader(IniFilePath))
                {
                    line = sr.ReadToEnd();
                    foreach (Match m in Regex.Matches(line, pattern))
                    {
                        line2 = m.Groups[0].Value;

                        SmtpServer = line2.Split(stringSeparators[0], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        SmtpPuerto = line2.Split(stringSeparators[1], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        SmtpUser = line2.Split(stringSeparators[2], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        SmtpPassword = line2.Split(stringSeparators[3], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        EmailSender = line2.Split(stringSeparators[4], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        EmailSubject = line2.Split(stringSeparators[5], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        EmailBody = line2.Split(stringSeparators[6], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        EmailRecipients = line2.Split(stringSeparators[7], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                    }
                }

                getRecipients(EmailRecipients);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public bool sendEmail()
        {

            System.Net.Mail.Attachment attachment;

            try
            {

                SmtpClient smtpClient = new SmtpClient(SmtpServer, Int32.Parse(SmtpPuerto)); //EMAIL SERVIDOR Y PUERTO
                NetworkCredential networkCredential = new NetworkCredential(SmtpUser, SmtpPassword);

                smtpClient.Credentials = (ICredentialsByHost)networkCredential;

                MailMessage msj = new MailMessage();
                msj.From = new MailAddress(EmailSender);

                /*Recipients*/

                foreach (string reco in EmailRecipientsSep)
                    msj.To.Add(reco);


                msj.Subject = EmailSubject;

                msj.IsBodyHtml = true;
                string body = EmailBody;
                msj.Body = body;

                foreach (string att in FileAttachments)
                {
                    attachment = new System.Net.Mail.Attachment(att);
                    msj.Attachments.Add(attachment);
                }
                   
                smtpClient.Send(msj);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }


}

