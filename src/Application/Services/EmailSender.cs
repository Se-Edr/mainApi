using Application.ConfigCalsses;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Numerics;
using System.Text;

namespace Application.Services
{
    public class EmailSender(IOptions<EmailSenderSettings> options,IOptions<ApiSettings> apiSet)
    {
        
        private async Task<string> SendEmail(string email,string messageToSend,string myUrl=null,bool isUrl = false)
        {
            EmailSenderSettings settings = options.Value;
            
            MimeMessage message= new MimeMessage();
            message.From.Add(new MailboxAddress("Avantime Garage",settings.sender));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "From best Garage in the world";

            if (isUrl)
            {
                string wholeMessage = $"{messageToSend} <a href='{myUrl}'>link</a>";
                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = wholeMessage
                };
            }
            else
            {
                message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = "TEST message for date registration (future feature)"
                };
            }

            using (var client=new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync(settings.sender, settings.appPas);
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
            return message.Body.ToString();
        }
        //return sended url
        public async Task<string> SendTokenUrl(string email,string tokenToConfirm,string method,bool forReset)
        {
            ApiSettings settings=apiSet.Value;
            

            StringBuilder message= new StringBuilder();
            
            message.Append(
            forReset?
                "If You requested to change your password, click please on this  "
                :
                "Congratulations, You successfully registered, to verificate your profile click please this "); 


            string url = $"{settings.BaseAddress}/api/Auth/{method}?email={email}&token={tokenToConfirm}";
            string mess = await SendEmail(email,message.ToString(),url,true); 

            return mess;
        }

    }
}
