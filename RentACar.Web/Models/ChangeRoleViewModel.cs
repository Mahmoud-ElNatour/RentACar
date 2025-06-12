using System.ComponentModel.DataAnnotations;

namespace RentACar.Web.Models
{
    public class ChangeRoleViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}
