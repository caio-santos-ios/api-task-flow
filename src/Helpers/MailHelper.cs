using MailKit.Net.Smtp;
using MimeKit;

namespace to_do_list.src.Helpers
{
    public class MailHelper(HttpClient http)
    {
        private readonly string EmailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "";
        private readonly string Password = Environment.GetEnvironmentVariable("PASSWORD_EMAIL") ?? "";
        private readonly string _apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
        private readonly string _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "";

        public async Task<bool> SendMailV1(string recipient, string subject, string body)
        {
            try
            {
                MimeMessage message = new();
                message.From.Add(MailboxAddress.Parse(EmailFrom));
                message.To.Add(MailboxAddress.Parse(recipient));
                message.Subject = subject;
                message.Body = new TextPart("html")
                {
                    Text = body  
                };

                using SmtpClient smtp = new();
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(EmailFrom, Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> SendMail(string recipient, string subject, string body)
        {
            try
            {
                var payload = new
                {
                    from = _fromEmail,
                    to = new[] { recipient },
                    subject,
                    html = body
                };

                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
                {
                    Content = JsonContent.Create(payload)
                };
                req.Headers.Authorization = new("Bearer", _apiKey);

                var res = await http.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                    return await res.Content.ReadAsStringAsync();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}