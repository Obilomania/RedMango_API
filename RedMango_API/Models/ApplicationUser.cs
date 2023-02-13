using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RedMango_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
    }
}
