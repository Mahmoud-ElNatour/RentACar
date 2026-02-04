using System.Threading.Tasks;

namespace RentACar.Application.Services
{
    public interface ISignalRBroadcaster
    {
        Task BroadcastSupportMessageAsync(int conversationId, object messageDto);
    }
}
