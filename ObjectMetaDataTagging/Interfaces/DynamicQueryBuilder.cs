﻿using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Interfaces
{
    public class DynamicQueryBuilder<TProperty1, TProperty2> : IDynamicQueryBuilder<TProperty1, TProperty2>
    {
        public IQueryable<T> BuildDynamicQuery<T>(
         List<BaseTag> sourceObject,
         Func<BaseTag, bool> property1Filter = null,
         Func<BaseTag, bool> property2Filter = null,
         LogicalOperator logicalOperator = LogicalOperator.OR)
        {
            if (property1Filter == null && property2Filter == null)
            {
                Console.WriteLine("BuildDynamicQuery: No filter conditions found.");
                return sourceObject.AsQueryable().Cast<T>();
            }

            var parameter = Expression.Parameter(typeof(BaseTag), "tag");
            Console.WriteLine($"BuildDynamicQuery: Parameter name: {parameter.Name}");
            Expression predicateBody = null;

            if (property1Filter != null)
            {
                Console.WriteLine("BuildDynamicQuery: Adding property1Filter");
                predicateBody = predicateBody == null
                    ? Expression.Invoke(Expression.Constant(property1Filter), parameter)
                    : logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, Expression.Invoke(Expression.Constant(property1Filter), parameter))
                        : Expression.OrElse(predicateBody, Expression.Invoke(Expression.Constant(property1Filter), parameter));
                Console.WriteLine("BuildDynamicQuery: property1Filter: " + property1Filter.ToString());
            }

            if (property2Filter != null)
            {
                Console.WriteLine("BuildDynamicQuery: Adding property2Filter");
                predicateBody = predicateBody == null
                    ? Expression.Invoke(Expression.Constant(property2Filter), parameter)
                    : logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, Expression.Invoke(Expression.Constant(property2Filter), parameter))
                        : Expression.OrElse(predicateBody, Expression.Invoke(Expression.Constant(property2Filter), parameter));
                Console.WriteLine("BuildDynamicQuery: property2Filter: " + property2Filter.ToString());
            }

            if (predicateBody == null)
            {
                // No valid filter predicates provided, return all sourceObject items.
                Console.WriteLine("BuildDynamicQuery: No valid filter predicates, returning all items.");
                return sourceObject.AsQueryable().Cast<T>();
            }

            var lambda = Expression.Lambda<Func<BaseTag, bool>>(predicateBody, parameter);
            Console.WriteLine("BuildDynamicQuery: Filter expression: " + lambda.ToString());

            var result = sourceObject.AsQueryable().Where(lambda);
            Console.WriteLine("BuildDynamicQuery: Filtered items count: " + result.Count());

            return result.Cast<T>();
        }
    }
}
