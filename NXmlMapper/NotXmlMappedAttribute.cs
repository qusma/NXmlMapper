// -----------------------------------------------------------------------
// <copyright file="NotXmlMappedAttribute.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace NXmlMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotXmlMappedAttribute : Attribute
    {
    }
}