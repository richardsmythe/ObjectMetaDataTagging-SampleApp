using ObjectMetaDataTagging.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Interfaces
{

    public class DynamicQueryBuilder : IDynamicQueryBuilder
    {
            /// <summary>
    /// Constructs a dynamic query to filter a collection of objects based on specified filter criteria.
    /// </summary>
    /// <param name="source">The source collection to filter.</param>
    /// <param name="filterCriteria">A list of filter criteria specifying property names and values to filter by.</param>
    /// <returns>An IQueryable representing the filtered collection of objects.</returns>

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
