using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Contracts.Dtos
{
    public class AssetDto
    {
        public Guid Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public string Specification { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime InstalledDate { get; set; }

    }

}
