using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.Models
{
    public class Login
    {
        [Required]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [Required]
        public string Password { get; set; }

        public string? Error { get; set; } // Error
    }
}
