using System.Threading.Tasks;

namespace KnowledgeBank.Web.Services
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string name, string email, string subject, string message);
	}
}
