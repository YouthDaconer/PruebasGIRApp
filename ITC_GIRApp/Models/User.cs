using System.ComponentModel.DataAnnotations;

namespace ITC_GIRApp.Models
{
    public class User
    {
        [Required(AllowEmptyStrings = true)]
        public string SAMAccountName { get; set; } = "";

        [Required(AllowEmptyStrings = true)]
        public string Password { get; set; } = "";

        [Required(AllowEmptyStrings = true)]
        [StringLength(3)]
        public string BusinessName { get; set; }

        [Required]
        public string DocumentNumber { get; set; } = "0";

        [Required(AllowEmptyStrings = false)]
        [StringLength(3)]
        public string ContactName { get; set; }

        [EmailAddress]
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; } = "example@domain.co";

        [EmailAddress]
        public string Email2 { get; set; } = null;

        public string Phone { get; set; } = null;

        [Required]
        public string Mobile { get; set; } = null;

        [Required(AllowEmptyStrings = false)]
        public string Region { get; set; }

        public string Address { get; set; } = null;

        [Required(AllowEmptyStrings = false)]
        [Range(typeof(bool), "false", "true")]
        public bool IsFirstTime { get; set; } = false;

        public string Captcha { get; set; } = "";

        [Required(AllowEmptyStrings = true)]
        public string CodeVerification { get; set; } = "";

        [Required(AllowEmptyStrings = true)]
        public string Role { get; set; } = "";
    }
}