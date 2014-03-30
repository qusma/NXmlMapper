// -----------------------------------------------------------------------
// <copyright file="ElementNameAttribute.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace NXmlMapper
{
    /// <summary>
    /// Map an attribute's value to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AttributeNameAttribute : Attribute
    {
        public string AttributeName;
        public string ParseOptions;

        public AttributeNameAttribute(string name, string parseOptions = null)
        {
            AttributeName = name;
            ParseOptions = parseOptions;
        }
    }
}