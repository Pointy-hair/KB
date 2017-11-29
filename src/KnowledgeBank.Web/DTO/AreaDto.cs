using KnowledgeBank.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeBank.Web.DTO
{
    public class AreaDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public static partial class Dto
    {
        public static AreaDto FromModel(Area model)
        {
            return new AreaDto
            {
                Id = model.Id,
                Name = model.Name
            };
        }
    }
}
