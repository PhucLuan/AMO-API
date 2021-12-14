using System;

namespace Rookie.AMO.DataAccessor.Entities
{
    public class BaseDto
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public Guid? CreatorId { get; set; }

        public bool Pubished { get; set; }
    }
}
