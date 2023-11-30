using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder<TProperty1, TProperty2>
    {
        IQueryable<T> BuildDynamicQuery<T>(
            List<BaseTag> sourceObject,
            Func<BaseTag, bool>? property1Filter = null,
            Func<BaseTag, bool>? property2Filter = null,
            LogicalOperator logicalOperator = LogicalOperator.OR);
    }
}
