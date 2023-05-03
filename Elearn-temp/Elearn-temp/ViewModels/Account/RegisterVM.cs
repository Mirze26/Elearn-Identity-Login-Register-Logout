using System.ComponentModel.DataAnnotations;

namespace Elearn_temp.ViewModels.Account
{
    public class RegisterVM
    {

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not vaild")] 
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password), Compare(nameof(Password))]  
        public string ConfirmPassword { get; set; }

    }
}
