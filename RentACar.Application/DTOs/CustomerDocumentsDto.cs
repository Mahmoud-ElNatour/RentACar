using System.ComponentModel.DataAnnotations;

namespace RentACar.Application.DTOs
{
    public class CustomerDocumentsDto
    {
        public byte[]? DrivingLicenseFront { get; set; }
        public byte[]? DrivingLicenseBack { get; set; }
        public byte[]? NationalIdfront { get; set; }
        public byte[]? NationalIdback { get; set; }
    }
}
