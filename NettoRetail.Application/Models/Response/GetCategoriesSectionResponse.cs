using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Response
{
    public class GetCategoriesSectionResponse
    {
        public int ShopId { get; set; }
        public bool Visibility { get; set; }
        public string Preview { get; set; }
        public string Name { get; set; }
        public int NumberOfCategories { get; set; }
    }
}
