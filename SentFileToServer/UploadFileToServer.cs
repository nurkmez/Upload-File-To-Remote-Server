using NLog;
using Renci.SshNet;
using System;
using System.Configuration;
using System.IO;
using Topshelf.Logging;

namespace SentFileToServer
{

    public static class UploadFileToServer
    {

        private static readonly Logger logWriter = NLog.LogManager.GetCurrentClassLogger();

        private static string transfer_folder = ConfigurationManager.AppSettings["transfer_folder"];
        private static string ongoing_folder = ConfigurationManager.AppSettings["ongoing_folder"];
        private static string sent_folder = ConfigurationManager.AppSettings["sent_folder"];

        // Enter your host name or IP here
        private static string host = ConfigurationManager.AppSettings["sftp_server_ip"];
        // Enter your sftp username here
        private static string username = ConfigurationManager.AppSettings["sftp_server_username"];
        // Enter your sftp password here
        private static string password = ConfigurationManager.AppSettings["sftp_password"];
        // Enter your sftp port here
        private static int port = Convert.ToInt32( ConfigurationManager.AppSettings["sftp_port"]);
        // 
        private static string sftp_server_folder = ConfigurationManager.AppSettings["sftp_server_folder"];
        private static bool Send(string fileName, string outgoingFilePath)
        {

            var connectionInfo = new ConnectionInfo(host, port, username, new PasswordAuthenticationMethod(username, password));
            // Upload File
            try
            {
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();

                    if (!sftp.Exists(sftp_server_folder))
                    {
                        sftp.CreateDirectory(sftp_server_folder);
                        logWriter.Info("Remote folder created =>" + sftp_server_folder);

                    }
                    sftp.ChangeDirectory(sftp_server_folder);
                    logWriter.Info("Remote folder is changed to =>" + sftp_server_folder);

                    using (var uplfileStream = System.IO.File.OpenRead(outgoingFilePath))
                    {
                        sftp.UploadFile(uplfileStream, fileName, true);
                        logWriter.Info("File is uploaded =>" + fileName);
                    }
                    sftp.Disconnect();
                    logWriter.Info("sftp is disconnected=>");

                }
            }
            catch (Renci.SshNet.Common.SshConnectionException ex)
            {
                logWriter.Error("Cannot connect to the server. =>" + ex.Message);

                return false;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                logWriter.Error("Unable to establish the socket. =>" + ex.Message);
                return false;
            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
            {
                logWriter.Error("Authentication of SSH session failed. =>" + ex.Message);
                return false;
            }
            return true;
        }


        public static int SendToServer()
        {
            try
            {

                Directory.CreateDirectory(transfer_folder);
                Directory.CreateDirectory(ongoing_folder);
                Directory.CreateDirectory(sent_folder);
                logWriter.Info("Local directories has been created if those are not exists");

                string[] files;

                if (Directory.Exists(ongoing_folder))
                {
                    files = System.IO.Directory.GetFiles(ongoing_folder);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {


                        var fileName = Path.GetFileName(s);
                        var outgoingFilePath = Path.Combine(ongoing_folder, fileName);
                        var sentFilePath = Path.Combine(sent_folder, fileName);
                        
                        Send(fileName, outgoingFilePath);

                        File.Copy(outgoingFilePath, sentFilePath, true);
                        File.Delete(outgoingFilePath);
                        logWriter.Info("File has been moved from => " + outgoingFilePath + " to => " + sentFilePath);
                    }
                    logWriter.Info("Files have been sent to remote server");
                    return 1;
                }
                else
                {
                    logWriter.Info("No file found to send to remote server");
                    return 0;
                }
            }
            catch(Exception ex)
            {
                logWriter.Error("Error occured" + ex.Message);
                return -1;
            }

        }
    }
}

