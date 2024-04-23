using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using UnityEngine;

using static Modules.Utility.Utility;

namespace ViitorCloud.Utility {

    public class ApplicationLogger : MonoBehaviour {
        public string output = "";
        public string stack = "";
        public static string datePatt = @"MM/dd/yyyy HH:mm:ss";
        public LogType logType;

        public bool log, warning, error;

        public static ApplicationLogger applicationLogger {
            set; get;
        }

        public static string logpath;

        private void OnEnable() {
            if (PlayerPrefs.GetString("AppVersion", "1.0.0") != Application.version) {
                Log("Clear");
                PlayerPrefs.SetString("AppVersion", Application.version);
            }
            if (applicationLogger == null) {
                Application.logMessageReceivedThreaded += HandleLog;
                logpath = Path.Combine(Application.persistentDataPath, Application.productName + $"_{Application.platform}_Log.txt");
                applicationLogger = this;
                Log(
                    $"************************************************* This is New Run {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}*************************************************");
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        private void OnDisable() {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {
            //if (logType == LogType.Warning)
            //{
            //    return;
            //}

            output = logString;
            stack = stackTrace;
            logType = type;
            if (type != LogType.Warning) {
                if (logString != "ClearLog") {
                    WriteLog($"{logType}\n{logString}\n{stackTrace}", false);
                } else {
                    WriteLog("Log Cleared.", true);
                }
            }
        }

        /// <summary>
        /// Wtite the log
        /// </summary>
        /// <param name="content">string to write</param>
        public static void WriteLog(string content, bool clearlog) {
            //try
            {
                //The File is exist on given path ?
                if (File.Exists(logpath)) {
                    if (!clearlog) {
                        File.AppendAllText(logpath, string.Format("\n{0} - {1}", DateTime.Now.ToString(datePatt), content));
                    } else {
                        File.WriteAllText(logpath, content);
                    }
                    //WriteLog(content,false);
                } else {
                    LogError("LogFile Does not exist.");
                    //_ = File.Create(logpath, 1, FileOptions.WriteThrough);
                    FileStream fileStream;
                    fileStream = new FileStream(logpath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 4,
                        FileOptions.WriteThrough);

                    fileStream.Close();
                    WriteLog(content, false);
                }
            }
            //catch
            //{
            //}
        }

        public static void sendMailNow() {
            string sub = "Feedback Real Immersre";
            string tomail = /*@"vinayak.lakhani@gmail.com"; //*/
                @"vinayak.lakhani@viitor.cloud,mehta.ravi@viitor.cloud";
            string newline = Environment.NewLine;
            string body = $"Device Name - {SystemInfo.deviceName}{newline}" +
                       $"Device Model - {SystemInfo.deviceModel}{newline}" +
                       $"Device Type - {SystemInfo.deviceType}{newline}" +
                       $"Operating System - {SystemInfo.operatingSystem}{newline}" +
                       $"Operating System Family- {SystemInfo.operatingSystemFamily}{newline}" +
                       $"Processor Count- {SystemInfo.processorCount}{newline}" +
                       $"Processor Frequency- {SystemInfo.processorFrequency}{newline}" +
                       $"Battery Status - {SystemInfo.batteryStatus}{newline}" +
                       $"Battery Level - {SystemInfo.batteryLevel}{newline}" +
                       $"Processor Type - {SystemInfo.processorType}{newline}" +
                       $"Graphics DeviceName - {SystemInfo.graphicsDeviceName}{newline}" +
                       $"System Memory Size - {SystemInfo.systemMemorySize}{newline}" +
                       $"{newline}{newline}{newline}" +
                       $"Sent for {Application.productName}.";
            string fromMail = "office.testing778@gmail.com";
            string password = "Wayne Rooney";

            MailMessage mail = new MailMessage {
                From = new MailAddress(fromMail)
            };
            mail.To.Add(tomail);
            mail.Subject = sub;
            mail.Body = body;
            if (File.Exists(logpath)) {
                Attachment data = new Attachment(logpath, System.Net.Mime.MediaTypeNames.Application.Octet);
                mail.Attachments.Add(data);
            } else {
                body += newline + newline + "<b>NO LOG FILE</b>";
            }

            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com") {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, password),
                EnableSsl = true
            };
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                    return true;
                };
            smtpServer.Timeout = 500;
            try {
                smtpServer.Send(mail);
                Log("ClearLog");
            } catch (Exception e) {
                LogError(e.GetBaseException().Message);
            }
        }

        [ContextMenu("ClearLog")]
        private void cleartext() {
            WriteLog("Log Cleared.", true);
        }
    }
}