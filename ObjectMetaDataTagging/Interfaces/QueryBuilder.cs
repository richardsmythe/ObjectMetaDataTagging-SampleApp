using ObjectMetaDataTagging.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    public class QueryBuilder : IQueryBuilder
    {
        public Expression<Func<T, bool>> BuildQuery<T>(Query query)
        {
            throw new NotImplementedException();
        }
    }
}
