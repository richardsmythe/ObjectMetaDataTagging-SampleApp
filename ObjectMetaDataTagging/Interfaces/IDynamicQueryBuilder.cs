﻿using ObjectMetaDataTagging.Models.QueryModels;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder
    {
        IQueryable<T> BuildDynamicQuery<T>(
            IQueryable<T> sourceObject,
            List<FilterCriteria> filters,
            LogicalOperator logicalOperator = LogicalOperator.AND);
    }
}