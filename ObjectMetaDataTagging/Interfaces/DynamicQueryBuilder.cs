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
     List<BaseTag> sourceObject,
     List<FilterCriteria> filters,
     LogicalOperator logicalOperator = LogicalOperator.AND)
        {
            if (filters == null || filters.Count == 0)
            {
                Console.WriteLine("No filters provided.");
                return (IQueryable<T>)sourceObject.AsQueryable();
            }

            var parameter = Expression.Parameter(typeof(BaseTag), "Tag");
            Expression predicateBody = null;

            foreach (var tag in sourceObject)
            {
                Console.WriteLine($"tag.Value Type: {tag.Value.GetType().FullName}");
            }

            foreach (var filter in filters)
            {
                // Note - currently works fine with Name and Type using both OR and AND, however the property Value didn't work in the AND operator.
                var nameProperty = Expression.Property(parameter, "Name");
                var valueProperty = Expression.Property(parameter, "Type");
                var constantName = Expression.Constant(filter.Name);
                var constantValue = Expression.Constant(filter.Type);

                var nameFilterExpression = Expression.Equal(nameProperty, constantName);
                var valueFilterExpression = Expression.Equal(valueProperty, constantValue);

                var filterExpression = logicalOperator == LogicalOperator.AND
                    ? Expression.And(nameFilterExpression, valueFilterExpression)
                    : Expression.OrElse(nameFilterExpression, valueFilterExpression);


                Console.WriteLine($"filter.Value Type: {filter.Type.GetType().FullName}");

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

            var lambda = Expression.Lambda<Func<BaseTag, bool>>(predicateBody, parameter);
            Console.WriteLine("Generated Expression:");
            Console.WriteLine(lambda.ToString());
            var result = sourceObject.AsQueryable().Where(lambda);    
            Console.WriteLine($"Filtered result: {result.Count()} items");

            return (IQueryable<T>)result;
        }
    }
}
