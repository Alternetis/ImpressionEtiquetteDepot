using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace ImpressionEtiquetteDepot.Core
{
    public static class Log
    {
        public const string LogLineSeparator = "[-]";
        public enum LogIdentifier
        {
            ErreurImport
        };
        public enum LogStreamType
        {
            LogStream,
            LogChronoStream
        }
        private const string Key = "A9gJL3eMmYH4ruiW7t0b6bp";

        //<JG> 29/05/2012
        public static string LogDirectory = System.Windows.Forms.Application.StartupPath + "\\Logs";
        public static string ArchiveDirectory = System.Windows.Forms.Application.StartupPath + "\\Logs" + "\\Log_Archives";
        public static StreamWriter LogStream = null;
        public static StreamWriter LogChronoStream = null;
        public static string LogPath = string.Empty;

        private static void CreateLog(string LogIdentifier, LogStreamType SelectedLogStream)
        {
            LogIdentifier = CheckLogIdentifier(LogIdentifier);
            try
            {
                if (Directory.Exists(LogDirectory) == false)
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                if (File.Exists(LogDirectory + "\\test.txt"))
                {
                    File.Delete(LogDirectory + "\\test.txt");
                }

                // Créer un fichier puis supprimer celui-ci pour tester les accès au dossier Log
                using (FileStream fileStr = File.Create(LogDirectory + "\\test.txt"))
                {
                    // Ajouter du texte au fichier  
                    Byte[] text = new UTF8Encoding(true).GetBytes("Lorem Ipsum");
                    fileStr.Write(text, 0, text.Length);
                    fileStr.Close();
                    File.Delete(fileStr.Name);
                }
            }
            catch
            {
                // Erreur d'écriture ou de suppression => utilisation du dossier dans le compte utilisateur. 
                LogDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Logs";
                ArchiveDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "Logs\\Log_Archives";

                if (Directory.Exists(LogDirectory) == false)
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }

            #region Archivage
            try
            {
                string[] FileList = Directory.GetFiles(LogDirectory, "*_Log.txt");

                if (FileList.Length > 19)
                {
                    //Archivage des anciens logs
                    Directory.CreateDirectory(ArchiveDirectory);

                    string DayDirectory = ArchiveDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd");
                    Directory.CreateDirectory(DayDirectory);

                    foreach (string File in FileList)
                    {
                        try
                        {
                            FileInfo Name = new FileInfo(File);
                            Directory.Move(LogDirectory + "\\" + Name.Name, DayDirectory + "\\" + Name.Name);
                        }
                        catch { }
                    }
                }
                if (Directory.Exists(ArchiveDirectory))
                {
                    string[] DirList = Directory.GetDirectories(ArchiveDirectory);
                    if (DirList.Length > 20)
                    {
                        DeleteFolder(DirList.OrderBy(d => d).First());
                    }
                }
            }
#pragma warning disable CS0168 // La variable 'ex' est déclarée, mais jamais utilisée
            catch (Exception ex)
#pragma warning restore CS0168 // La variable 'ex' est déclarée, mais jamais utilisée
            {
                // erreur d'archivage des logs
                //Error.SendMailError(ex.ToString());
            }
            #endregion

            LogPath = LogDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ((!string.IsNullOrWhiteSpace(LogIdentifier)) ? "_" : "") + LogIdentifier + "_Log.txt";
            switch (SelectedLogStream)
            {
                case LogStreamType.LogStream:
                default:
                    LogStream = new StreamWriter(LogPath, true, Encoding.UTF8);
                    LogStream.AutoFlush = true;
                    break;
                case LogStreamType.LogChronoStream:
                    int counter = 1;
                    while (File.Exists(LogPath))
                    {
                        LogPath = LogDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ((!string.IsNullOrWhiteSpace(LogIdentifier)) ? "_" : "") + LogIdentifier + "_Log" + counter.ToString() + ".txt";
                        counter++;
                    }
                    LogChronoStream = new StreamWriter(LogPath, true, Encoding.UTF8) { AutoFlush = true };
                    break;
            }
        }

        public static void DeleteFolder(string directory)
        {
            try
            {
                string[] DirList = Directory.GetDirectories(directory);
                string[] FileList = Directory.GetFiles(directory);

                foreach (string dir in DirList)
                    DeleteFolder(dir);

                foreach (string file in FileList)
                    File.Delete(file);

                Directory.Delete(directory);
            }
            catch (Exception)
            {
                // Not implemented
            }
        }

        public static void CloseLog(LogStreamType SelectedLogStream)
        {
            switch (SelectedLogStream)
            {
                case LogStreamType.LogStream:
                default:

                    if (LogStream != null)
                    {
                        LogStream.WriteLine("");
                        LogStream.WriteLine("Fermeture du log courant");
                        LogStream.Flush();
                        LogStream.Close();
                        LogStream = null;
                    }
                    break;
                case LogStreamType.LogChronoStream:
                    if (LogChronoStream != null)
                    {
                        LogChronoStream.WriteLine("");
                        LogChronoStream.WriteLine("Fermeture du log courant");
                        LogChronoStream.Flush();
                        LogChronoStream.Close();
                        LogChronoStream = null;
                    }
                    break;
            }
        }

        public static void WriteLog(string Value, bool NewLogGroup = false)
        {
            if (LogStream == null)
            {
                CreateLog("", LogStreamType.LogStream); lock (LogStream)
                {
                    LogStream.WriteLine("");
                    LogStream.WriteLine("ImpressionEtiquette version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    LogStream.WriteLine("");
                    LogStream.WriteLine("Informations de connexion :");
                    LogStream.WriteLine("Serveur d'exécution : " + Environment.MachineName);
                    LogStream.WriteLine("Domaine réseau : " + Environment.UserDomainName);
                    LogStream.WriteLine("Session utilisateur de : " + Environment.UserName);
                    //LogStream.WriteLine("Connexion base de données Sage : " + Properties.Settings.Default.SageConnection);
                    //LogStream.WriteLine("Connexion objets métiers Sage : " + Properties.Settings.Default.a + " - " + Properties.Settings.Default.SAGEUSER.Replace("<", "&lt;").Replace(">", "&gt;"));
                    LogStream.WriteLine("");
                }
            }
            else if (NewLogGroup)
            {
                lock (LogStream)
                {
                    LogStream.WriteLine("--------------------------------------------------------");
                    LogStream.WriteLine("--------------------------------------------------------");
                }
            }
            lock (LogStream)
            {
                if (NewLogGroup)
                {
                    LogStream.WriteLine("");
                    LogStream.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH-mm"));
                    LogStream.WriteLine("");
                    LogStream.WriteLine("Utilisateur : " + Environment.UserName);
                    LogStream.WriteLine("");
                }
                LogStream.WriteLine(Value);
            }
        }

        //public static void OpenDirectory(string dir)
        //{
        //    //not open if in task
        //    if (Global.UILaunch)
        //    {
        //        if (!Directory.Exists(dir))
        //        {
        //            MessageBox.Show("Le dossier n'existe pas !", "", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //        else
        //        {
        //            System.Diagnostics.Process.Start("explorer.exe", dir);
        //        }
        //    }
        //}

        public static void WriteSpecificLog(List<string> log_in, LogIdentifier LogIdentifier)
        {
            try
            {
                if (log_in.Count > 0)
                {
                    if (LogStream != null)
                    {
                        CloseLog(LogStreamType.LogStream);
                    }
                    CreateLog(LogIdentifier.ToString(), LogStreamType.LogStream);
                    LogStream.WriteLine("");
                    //LogStream.WriteLine("PrestaConnect version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    LogStream.WriteLine("");
                    LogStream.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH-mm"));
                    LogStream.WriteLine("");
                    LogStream.WriteLine("Informations de connexion :");
                    LogStream.WriteLine("Serveur d'exécution : " + Environment.MachineName);
                    LogStream.WriteLine("Domaine réseau : " + Environment.UserDomainName);
                    LogStream.WriteLine("Session utilisateur de : " + Environment.UserName);
                  
                    //LogStream.WriteLine("Connexion base de données Sage : " + Properties.Settings.Default.SageConnection);
                   // LogStream.WriteLine("Connexion objets métiers Sage : " + Properties.Settings.Default.a + " - " + Properties.Settings.Default.SAGEUSER);
                    LogStream.WriteLine("");
                    LogStream.WriteLine("--------------------------------------------------------");
                    LogStream.WriteLine("");

                    foreach (string line in log_in)
                    {
                        if (line == LogLineSeparator)
                        {
                            LogStream.WriteLine("");
                            LogStream.WriteLine("--------------------------------------------------------");
                            LogStream.WriteLine("");
                        }
                        else
                            LogStream.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString(), true);
                WriteLog(LogIdentifier.ToString(), false);
                foreach (string line in log_in)
                    WriteLog(line, true);
            }
            CloseLog(LogStreamType.LogStream);
        }

        /*  public static void WriteChronoLog(List<string> log_in, LogIdentifier LogIdentifier)
         {
             try
             {
                 if (log_in.Count > 0)
                 {
                     if (LogChronoStream == null)
                     {
                         CreateLog(LogIdentifier.ToString(), LogStreamType.LogChronoStream);
                         LogChronoStream.WriteLine("");
                         LogChronoStream.WriteLine("PrestaConnect version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                         LogChronoStream.WriteLine("");
                         LogChronoStream.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH-mm"));
                         LogChronoStream.WriteLine("");
                         LogChronoStream.WriteLine("Informations de connexion :");
                         LogChronoStream.WriteLine("Serveur d'exécution : " + Environment.MachineName);
                         LogChronoStream.WriteLine("Domaine réseau : " + Environment.UserDomainName);
                         LogChronoStream.WriteLine("Session utilisateur de : " + Environment.UserName);

                         LogChronoStream.WriteLine("Connexion base de données Sage : " + Properties.Settings.Default.SAGEConnectionString);
                         LogChronoStream.WriteLine("Connexion objets métiers Sage : " + Properties.Settings.Default.GCMFILE + " - " + Properties.Settings.Default.SAGEUSER);
                         LogChronoStream.WriteLine("");
                         LogChronoStream.WriteLine("--------------------------------------------------------");
                         LogChronoStream.WriteLine("");
                     }

                     foreach (string line in log_in)
                     {
                         if (line == LogLineSeparator)
                         {
                             LogChronoStream.WriteLine("");
                             LogChronoStream.WriteLine("--------------------------------------------------------");
                             LogChronoStream.WriteLine("");
                         }
                         else
                             LogChronoStream.WriteLine(line);
                     }
                 }
                 //CloseLog(LogStreamType.LogChronoStream);
             }
             catch (Exception ex)
             {
                 WriteLog(ex.ToString(), true);
             }
         } */

        /*
        public static bool SendLogMail(List<string> log_in, string Destinataire, out string MessageNotSend)
        {
            bool send = false;
            MessageNotSend = "Compte mail expéditeur invalide";
            try
            {
                if (Properties.Settings.Default.SendMailErrorActive)
                {
                    //string User = Global.GetConfig().ConfigMailUser;
                    //string UserId = Global.GetConfig().ConfigMailUserId;
                    //string Password = Global.GetConfig().ConfigMailPassword;
                    //string SMTP = Global.GetConfig().ConfigMailSMTP;
                    //int Port = Global.GetConfig().ConfigMailPort;
                    //bool isSSL = Global.GetConfig().ConfigMailSSL;


                    //Add parameters
                    string User = Properties.Settings.Default.MailUser;
                    string UserId = Properties.Settings.Default.MailAdress;
                    string Password = Properties.Settings.Default.MailPassword;
                    string SMTP = Properties.Settings.Default.MailSMTP;
                    int Port = Properties.Settings.Default.MailPort;
                    bool isSSL = Properties.Settings.Default.MailSSL;


                    if (!string.IsNullOrWhiteSpace(UserId)
                        //&& !string.IsNullOrWhiteSpace(Password)
                        && !string.IsNullOrWhiteSpace(SMTP))
                    {
                        MailMessage ObjMessage = new MailMessage();
                        MailAddress ObjAdrExp = new MailAddress(string.IsNullOrEmpty(User) ? UserId : User);
                        MailAddress ObjAdrRec = new MailAddress(Destinataire);
                        SmtpClient ObjSmtpClient = new SmtpClient(SMTP, Port);

                        ObjMessage.From = ObjAdrExp;
                        ObjMessage.To.Add(ObjAdrRec);

                        string subject = "Log ExportBCA", title = string.Empty;

                        title = subject;
                        ObjMessage.Subject = subject;

                        StringBuilder Body = new StringBuilder();
                        Body.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
                        Body.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                        Body.Append("<head>");
                        Body.Append("<meta http-equiv=\"content-type\" content=\"text/html\"; charset=UTF-8\" />");
                        title += ". ExportBCA version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                        Body.Append("<title>" + title + "</title>");
                        Body.Append("</head>");
                        Body.Append("<body>");
                        Body.Append(title);
                        Body.Append("<br />");
                        Body.Append("Informations de connexion :<br />");
                        Body.Append("<ul>");
                        Body.Append("<li>" + "Serveur d'exécution : " + Environment.MachineName + "</li>");
                        Body.Append("<li>" + "Domaine réseau : " + Environment.UserDomainName + "</li>");
                        Body.Append("<li>" + "Session utilisateur de : " + Environment.UserName + "</li>");
                        Body.Append("</ul>");
                        Body.Append("<br />");
                        foreach (string str in log_in)
                        {
                            Body.Append("<br />" + str);
                        }
                        Body.Append("</body>");
                        Body.Append("</html>");

                        ObjMessage.Body = Body.ToString();
                        ObjMessage.IsBodyHtml = true;
                        ObjSmtpClient.EnableSsl = isSSL;
                        ObjSmtpClient.Credentials = new NetworkCredential(User, Password);
                        ObjSmtpClient.Send(ObjMessage);
                        send = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageNotSend = ex.Message;
                Error.SendMailError(ex.ToString());
            }
            return send;
        }
        */
        public static string CheckLogIdentifier(string LogIdentifier)
        {
            if (string.IsNullOrEmpty(LogIdentifier))
                return string.Empty;
            else
            {
                string r = LogIdentifier;
                foreach (char c in LogIdentifier)
                    if (!char.IsLetter(c))
                        r.Replace(c, '_');

                return r;
            }
        }

        /*
        public static string EncryptString(string ValueToSecure, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(ValueToSecure);

            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(Key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = Encoding.UTF8.GetBytes(Key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }*/
    }
}
