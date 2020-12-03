using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.Core
{
    public class Category
    {
        public int Id { get; set; }
        public int HouseHoldId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public HouseHold HouseHold { get; set; }

        public ICollection<CategoryItem> CategoryItems { get; set; } = new HashSet<CategoryItem>();
    }
}
