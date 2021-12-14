using Rookie.AMO.DataAccessor.Entities;
using System;

namespace Rookie.AMO.Contracts.Dtos
{
    public class CategoryDto: BaseDto
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Desc { get; set; }
    }
}
