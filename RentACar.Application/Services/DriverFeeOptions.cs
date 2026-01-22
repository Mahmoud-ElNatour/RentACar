namespace RentACar.Application.Services;

public class DriverFeeOptions
{
    public const string SectionName = "DriverFee";

    public string Mode { get; set; } = "PerDay";

    public decimal Rate { get; set; } = 50m;
}
