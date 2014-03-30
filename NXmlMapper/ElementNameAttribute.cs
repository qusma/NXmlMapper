// -----------------------------------------------------------------------
// <copyright file="ElementNameAttribute.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace NXmlMapper
{
    /// <summary>
    /// Map a sub-element's value to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ElementNameAttribute : Attribute
    {
        public string ElementName;
        public string ParseOptions;

        public ElementNameAttribute(string name, string parseOptions = null)
        {
            ElementName = name;
            ParseOptions = parseOptions;
        }
    }
}