using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Net;

namespace ServerCore.Utilities.Utils
{
    public class MailUtil
    {
        static bool mailSent = false;
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String toEmail = (string)e.UserState;

            if(e.Cancelled)
            {
                NLogManager.Info(string.Format("Send email canceled to {0}", toEmail));
            }

            if(e.Error != null)
            {
                NLogManager.Error(string.Format("Send email error to {0}, error = ", toEmail, e.Error.ToString()));
            }
            else
            {
                NLogManager.Info(string.Format("Send email success to {0}", toEmail));
            }
            mailSent = true;
        }

        //public static void SendEmail(string fromEmail, string toEmail, string subject, string body)
        //{
        //    // Command line argument must the the SMTP host.
        //    SmtpClient client = new SmtpClient();
        //    // Specify the e-mail sender.
        //    // Create a mailing address that includes a UTF8 character
        //    // in the display name.
        //    MailAddress from = new MailAddress("jane@contoso.com",
        //       "Jane " + (char)0xD8 + " Clayton",
        //    System.Text.Encoding.UTF8);
        //    // Set destinations for the e-mail message.
        //    MailAddress to = new MailAddress("ben@contoso.com");
        //    // Specify the message content.
        //    MailMessage message = new MailMessage(from, to);
        //    message.Body = "This is a test e-mail message sent by an application. ";
        //    // Include some non-ASCII characters in body and subject.
        //    string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
        //    message.Body += Environment.NewLine + someArrows;
        //    message.BodyEncoding = System.Text.Encoding.UTF8;
        //    message.Subject = "test message 1" + someArrows;
        //    message.SubjectEncoding = System.Text.Encoding.UTF8;
        //    // Set the method that is called back when the send operation ends.
        //    client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
        //    // The userState can be any object that allows your callback 
        //    // method to identify this send operation.
        //    // For this example, the userToken is a string constant.
        //    string userState = "test message1";
        //    client.SendAsync(message, userState);
        //    Console.WriteLine("Sending message... press c to cancel mail. Press any other key to exit.");
        //    string answer = Console.ReadLine();
        //    // If the user canceled the send, and mail hasn't been sent yet,
        //    // then cancel the pending operation.
        //    if(answer.StartsWith("c") && mailSent == false)
        //    {
        //        client.SendAsyncCancel();
        //    }
        //    // Clean up.
        //    message.Dispose();
        //    Console.WriteLine("Goodbye.");
        //}

        public static void SendEmail(string fromEmail, string passWordFromEmail, string fromEmailHost, int fromEmailPort, string fromEmailDisplayName, string toEmail, string subject, string body)
        {
            //string email = Convert.ToString(ConfigurationManager.AppSettings["Email"]);
            //string password = Convert.ToString(ConfigurationManager.AppSettings["PasswordEmail"]);
            //string emailHost = Convert.ToString(ConfigurationManager.AppSettings["EmailHost"]);
            //int emailPort = Convert.ToInt32(ConfigurationManager.AppSettings["EmailPort"]);
            //string displayName = Convert.ToString(ConfigurationManager.AppSettings["DisplayName"]);

            try
            {
                var loginInfo = new NetworkCredential(fromEmail, passWordFromEmail);
                var msg = new MailMessage();
                var smtpClient = new SmtpClient(fromEmailHost, fromEmailPort);

                msg.From = new MailAddress(fromEmail, fromEmailDisplayName, Encoding.UTF8);
                msg.To.Add(new MailAddress(toEmail));
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = loginInfo;
                smtpClient.Timeout = 30000;
                smtpClient.SendAsync(msg, toEmail);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }
    }
}
