// -----------------------------------------------------------------------
// <copyright file="MapperExtensions.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq.Expressions;

namespace NXmlMapper
{
    public static class MapperExtensions
    {
        /// <summary>
        /// Set a custom mapping from an attribute value to a property.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="from">Name of the XML attribute.</param>
        /// <param name="path">The class property.</param>
        /// <param name="parseOptions">Optional parse options.</param>
        public static void SetAttributeMap<T, TProperty>(this Mapper<T> mapper, string from, Expression<Func<T, TProperty>> path, string parseOptions = null) 
            where T : new()
        {
            var member = (MemberExpression)path.Body;
            mapper.SetAttributeMap(from, member.Member.Name, parseOptions);
        }

        /// <summary>
        /// Set a custom mapping from a sub-element value to a property.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="from">Name of the XML element.</param>
        /// <param name="path">The class property.</param>
        /// <param name="parseOptions">Optional parse options.</param>
        public static void SetElementMap<T, TProperty>(this Mapper<T> mapper, string from, Expression<Func<T, TProperty>> path, string parseOptions = null)
    where T : new()
        {
            var member = (MemberExpression)path.Body;
            mapper.SetElementMap(from, member.Member.Name, parseOptions);
        }
    }
}
