using System.ComponentModel.DataAnnotations;

namespace WebAPI.Test.DataModels
{
    internal class InvalidLoginModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Pass { get; set; }

    }
}
