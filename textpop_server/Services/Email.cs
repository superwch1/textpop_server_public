using MailKit.Net.Smtp;
using MimeKit;

namespace textpop_server.Services
{
    public class Email
    {
        private readonly string _email;
        private readonly string _password;
        private readonly string _port;
        private readonly string _host;
        public Email(IConfiguration configuration)
        {
            _email = configuration["GoDaddySMTP:email"]!;
            _password = configuration["GoDaddySMTP:password"]!;
            _port = configuration["GoDaddySMTP:port"]!;
            _host = configuration["GoDaddySMTP:host"]!;
        }


        public async Task ReportMessage(string senderId, string messageId, string userId)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("noreply", _email));
            message.To.Add(new MailboxAddress($"John Wong", "wchjohn2@gmail.com"));
            message.Subject = "Report for abusive message";

            message.Body = new TextPart("html")
            {
                Text =
                $@"
                <!DOCTYPE html> 
                <html>
                    <body style=""font-size: 16px;"">
                        <p style=""color: #000000"">Sender Id: {senderId}</p>
                        <p style=""color: #000000"">Message Id: {messageId}</p>
                        <p style=""color: #000000"">User Id: {userId}</p>
                    </body>
                </html>
                "
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_host, Convert.ToInt32(_port), true);
                await client.AuthenticateAsync(_email, _password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
