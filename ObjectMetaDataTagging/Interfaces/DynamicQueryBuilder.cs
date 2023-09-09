using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
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

        public IQueryable<T> BuildDynamicQuery<T>(
            List<T> sourceObject,
            List<FilterCriteria> filters,
            LogicalOperator logicalOperator = LogicalOperator.AND)
        {
            if (filters == null || filters.Count == 0) return sourceObject.AsQueryable();

            var parameter = Expression.Parameter(typeof(BaseTag), "tag");
            Expression predicateBody = null;

            foreach (var filter in filters)
            {
                // Create expressions to filter based on Name and Value properties
                var nameProperty = Expression.Property(parameter, "Name");
                var valueProperty = Expression.Property(parameter, "Value");
                var constantName = Expression.Constant(filter.Name);
                var constantValue = Expression.Constant(filter.Value, typeof(object));

                // Build filter expressions for Name and Value properties
                var nameFilterExpression = Expression.Equal(nameProperty, constantName);
                var valueFilterExpression = Expression.Equal(valueProperty, constantValue);

                // Combine filter expressions using logicalOperator
                Expression filterExpression = logicalOperator == LogicalOperator.AND
                    ? Expression.AndAlso(nameFilterExpression, valueFilterExpression)
                    : Expression.OrElse(nameFilterExpression, valueFilterExpression);

                if (predicateBody == null)
                {
                    predicateBody = filterExpression;
                }
                else
                {
                    predicateBody = logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, filterExpression)
                        : Expression.OrElse(predicateBody, filterExpression);
                }
            }

            var lambda = Expression.Lambda<Func<T, bool>>(predicateBody, parameter);

            return sourceObject.AsQueryable().Where(lambda); 
        }
    }
}
