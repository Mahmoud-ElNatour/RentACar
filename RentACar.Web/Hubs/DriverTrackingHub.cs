using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RentACar.Web.Hubs;

[Authorize]
public class DriverTrackingHub : Hub
{
    public Task JoinBookingGroup(int bookingId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }

    public Task LeaveBookingGroup(int bookingId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }
}
