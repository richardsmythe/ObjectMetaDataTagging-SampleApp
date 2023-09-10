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

            Console.WriteLine("Filter Criteria:");
            foreach (var filter in filters)
            {
                Console.WriteLine($"- Name: {filter.Name}, Value: {filter.Value}");
            }

            var parameter = Expression.Parameter(typeof(BaseTag), "tag");
            Expression predicateBody = null;

            // Print the values of "Name" and "Value" properties for objects in sourceObject
            foreach (var tag in sourceObject)
            {
                Console.WriteLine($"tag name: { tag.Name} tag value: {tag.Value }");
            }

            foreach (var filter in filters)
            {
                // Create expressions to filter based on Name and Value properties
                var nameProperty = Expression.Property(parameter, "Name");
                var valueProperty = Expression.Property(parameter, "Value");
                var constantName = Expression.Constant(filter.Name);
                var constantValue = Expression.Constant(filter.Value, typeof(object));

                Console.WriteLine($"Filter Criteria: - Name: {filter.Name}, Value: {filter.Value}");

                // Build filter expressions for Name and Value properties
                var nameFilterExpression = Expression.Equal(nameProperty, constantName);
                var valueFilterExpression = Expression.Equal(valueProperty, constantValue);

                Console.WriteLine($"Accessed Name property: {nameProperty.Member.Name}");
                Console.WriteLine($"Accessed Value property: {valueProperty.Member.Name}");

                Console.WriteLine($"Applying Filter: {filter.Name} == {filter.Value}");

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

            // Creates a lambda expression in that represents a delegate of type Func<T, bool>.
            // This lambda expression encapsulates the filtering logic generated based on the filter criteria.
            var lambda = Expression.Lambda<Func<BaseTag, bool>>(predicateBody, parameter);

            var result = sourceObject.AsQueryable().Where(lambda);

            Console.WriteLine($"Filtered result: {result.Count()} items");

            return (IQueryable<T>)result;
        }
    }
}
