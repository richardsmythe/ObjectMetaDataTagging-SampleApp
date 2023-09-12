using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder<TFilter>
        where TFilter : DefaultFilterCriteria
    {
        IQueryable<T> BuildDynamicQuery<T>(
            List<BaseTag> sourceObject,
            List<TFilter> filters,
            LogicalOperator logicalOperator = LogicalOperator.OR);
    }
}