using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Models.QueryModels
{
    public class Query
    {
        public List<FilterCriteria> Filters { get; set; }
        public List<SortCriteria> Sorters { get; set; }
        public LogicalOperator Operator { get; set; }

    }
}
