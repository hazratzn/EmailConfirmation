using System.Threading.Tasks;

namespace eShopDemo.Services.Interfaces
{
    public interface ISendEmail
    {
        Task SendEmail(string toEmail, string url);
    }
}
