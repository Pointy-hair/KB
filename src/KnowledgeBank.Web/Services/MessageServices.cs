using System;
using System.Threading.Tasks;
using KnowledgeBank.Web.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace KnowledgeBank.Web.Services
{
	// This class is used by the application to send Email and SMS
	// when you turn on two-factor authentication in ASP.NET Identity.
	// For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
	public class AuthMessageSender : IEmailSender, ISmsSender
	{
		private readonly MailSettings _settings;
		private readonly ILogger<AuthMessageSender> _logger;
		public AuthMessageSender(IOptions<MailSettings> settings, ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<AuthMessageSender>();
			_settings = settings.Value;
		}

		private MimeMessage BuildMessage(string name, string email, string subject, TextPart body)
		{
			var message = new MimeMessage();
			var from = new MailboxAddress(_settings.From.Name, _settings.From.Email);
			var to = new MailboxAddress(name, email);
			message.From.Add(from);
			message.To.Add(to);
			message.Subject = subject;
			message.Body = body;
			return message;
		}

		private async Task SendEmailAsync(MimeMessage message)
		{
			Random rand = new Random();
			for (int i = 0; i < _settings.RetryCount; i++)
			{
				try
				{
					using (var client = new SmtpClient())
					{
						await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl);
						await client.AuthenticateAsync(_settings.User, _settings.Password);
						await client.SendAsync(message);
						await client.DisconnectAsync(true);
						return;
					}
				}
				catch (Exception exception)
				{
					_logger.LogError(1000, exception, "Mail Send failed");
					await Task.Delay(rand.Next(3000));
				}
			}
		}

		public async Task SendEmailAsync(string name, string email, string subject, string text)
		{
			var message = BuildMessage(name, email, subject, new TextPart("html") { Text = text });
			await SendEmailAsync(message);
		}

		public Task SendSmsAsync(string number, string message)
		{
			// Plug in your SMS service here to send a text message.
			_logger.LogWarning("Sms sending is not yet implemented");
			return Task.FromResult(0);
		}
	}
}
