namespace RentACar.Web.ViewModels
{
    public class EmailHubViewModel
    {
        public int SentToday { get; set; }
        public double DeliveryRate { get; set; }
        public int ActiveReminders { get; set; }
        public int PendingErrors { get; set; }
    }
}
