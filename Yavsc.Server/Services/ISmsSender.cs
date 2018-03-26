
using System.Threading.Tasks;

namespace Yavsc.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(TwilioSettings settings, string number, string message);
    }
}
