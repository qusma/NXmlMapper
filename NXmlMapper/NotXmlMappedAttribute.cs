// -----------------------------------------------------------------------
// <copyright file="NotXmlMappedAttribute.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace NXmlMapper
{
    /// <summary>
    /// Prevent this attribute from being automatically mapped.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotXmlMappedAttribute : Attribute
    {
    }
}