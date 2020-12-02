using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models
{
    public class FpUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        [NotMapped]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [Display(Name = "Avatar")]
        public string ImageName { get; set; }
        public byte[] ImageData { get; set; }


        [NotMapped]
        [DataType(DataType.Upload)]       
        public IFormFile Avatar { get; set; }

        [NotMapped]
        public string Extension { get; set; }

        
    }
}
