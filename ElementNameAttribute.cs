// -----------------------------------------------------------------------
// <copyright file="ElementNameAttribute.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace NXmlMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ElementNameAttribute : Attribute
    {
        public string ElementName;

        public ElementNameAttribute(string name)
        {
            ElementName = name;
        }
    }
}