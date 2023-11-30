using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Services
{
    public class DynamicQueryBuilder<TProperty1, TProperty2> : IDynamicQueryBuilder<TProperty1, TProperty2>
    {
        /// <summary>
        ///  Dynamically filters BaseTag objects based on the given fields, e.g. Name and Type. 
        ///  This is done via a custom filter that should extend BaseTag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject">A collection of type BaseTag that is used to filter against.</param>
        /// <param name="property1Filter">Delegate based filter for first property.</param>
        /// <param name="property2Filter">Delegate based filter for second property.</param>
        /// <param name="logicalOperator">Used to combine the filter expressions</param>
        /// <returns>An object of filtered results.</returns>
        public IQueryable<T> BuildDynamicQuery<T>(
         List<BaseTag> sourceObject,
         Func<BaseTag, bool> property1Filter = null,
         Func<BaseTag, bool> property2Filter = null,
         LogicalOperator logicalOperator = LogicalOperator.OR)
        {
            if (property1Filter == null && property2Filter == null)
            {
                Console.WriteLine("No filter conditions found.");
                return sourceObject.AsQueryable().Cast<T>();
            }

            var parameter = Expression.Parameter(typeof(BaseTag), "tag");
            Console.WriteLine($"Parameter name: {parameter.Name}");
            Expression? predicateBody = null;

            if (property1Filter != null)
            {
                predicateBody = predicateBody == null
                    ? Expression.Invoke(Expression.Constant(property1Filter), parameter)
                    : logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, Expression.Invoke(Expression.Constant(property1Filter), parameter))
                        : Expression.OrElse(predicateBody, Expression.Invoke(Expression.Constant(property1Filter), parameter));
                Console.WriteLine("property1Filter: " + property1Filter.ToString());
            }

            if (property2Filter != null)
            {
                predicateBody = predicateBody == null
                    ? Expression.Invoke(Expression.Constant(property2Filter), parameter)
                    : logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, Expression.Invoke(Expression.Constant(property2Filter), parameter))
                        : Expression.OrElse(predicateBody, Expression.Invoke(Expression.Constant(property2Filter), parameter));
                Console.WriteLine("property2Filter: " + property2Filter.ToString());
            }

            Console.WriteLine($"predicatebody: {predicateBody}");

            if (predicateBody == null)
            {
                Console.WriteLine("No valid filter predicates, returning all items.");
                return sourceObject.AsQueryable().Cast<T>();
            }

            var lambda = Expression.Lambda<Func<BaseTag, bool>>(predicateBody, parameter);
            Console.WriteLine("Filter expression: " + lambda.ToString());

            var result = sourceObject.AsQueryable().Where(lambda);
            Console.WriteLine("Filtered items count: " + result.Count());

            return result.Cast<T>();
        }
    }
}
