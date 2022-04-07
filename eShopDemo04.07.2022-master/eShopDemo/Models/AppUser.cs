using Microsoft.AspNetCore.Identity;

namespace eShopDemo.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsActivated { get; set; }

    }
}
