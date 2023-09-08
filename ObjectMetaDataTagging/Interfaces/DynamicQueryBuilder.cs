using ObjectMetaDataTagging.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{
    /// <summary>
    /// Build dynamic queries against a collection of objects, based on the given filter.
    /// The filter is based on the tagName and tagValue properties.
    /// </summary>
    public class DynamicQueryBuilder : IDynamicQueryBuilder
    {
        public IQueryable<T> BuildDynamicQuery<T>(IQueryable<T> sourceObject,
            List<FilterCriteria> filters, LogicalOperator logicalOperator = LogicalOperator.AND)
        {
            if (filters == null || filters.Count == 0) return sourceObject;

            ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
            Expression predicateBody = null;

            foreach(var filter in filters)
            {
                Expression property = Expression.Property(parameter, filter.TagName);
                Expression constant = Expression.Constant(filter.TagValue, typeof(T));

                Expression filterExpression = Expression.Equal(property, constant);

                if(predicateBody == null)
                {
                    predicateBody = filterExpression;
                }
                else
                {
                    if(logicalOperator == LogicalOperator.AND)
                    {
                        predicateBody = Expression.AndAlso(predicateBody, filterExpression);
                    }
                    else if (logicalOperator == LogicalOperator.OR)
                    {
                        predicateBody = Expression.OrElse(predicateBody, filterExpression);
                    }
                }
            }

            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(predicateBody, parameter);

            return sourceObject.Where(lambda);
        }
    }
}
