using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace fhir_integration.Handlers
{
    class EmailHandler
    {
        public string recipient { get; set; }

        public EmailHandler(string email)
        {
            this.recipient = email;
        }

        public void Send(string subject, string body)
        {
            string to = recipient;
            string from = "fhir.integrator@gmail.com";
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("fhir.integrator@gmail.com", "integrujuohen"),
                EnableSsl = true
            };

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to send email", ex.Message);
            }
        }
    }
}
