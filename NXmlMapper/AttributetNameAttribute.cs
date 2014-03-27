// -----------------------------------------------------------------------
// <copyright file="ElementNameAttribute.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace NXmlMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AttributeNameAttribute : Attribute
    {
        public string AttributeName;

        public AttributeNameAttribute(string name)
        {
            AttributeName = name;
        }
    }
}