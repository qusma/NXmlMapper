NXmlMapper
==========
NXmlMapper is a library that lets you easily parse XML files by mapping them to classes. It can parse any numeric type, strings, bools, and DateTimes. Both XML element and attribute values are parse-able.

You can install NXmlMapper through NuGet by running `Install-Package NXmlMapper` in the package manager console.

Usage
---
Given this XML input:

    <Workforce>
        <Person Name="Bob" Age="43" />
        <Person Name="Alice" Age="35" />
        <Person Name="John" Age="49" />
    </Workforce>

And this class:

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

You can get a `List<Person>` with all the values filled in by doing the following:

    var mapper = new Mapper<Person>(xml);
    List<Person> workforce = mapper.ParseAll();

If automatic mapping is not enough, you can map XML to object properties manually. Let's say you want to map the XML attribute "fullName" to the property "Name". This can be done either using an attribute on the property:

    [AttributeName("fullName")]
    public string Name { get; set; }

or through a fluent interface:

    mapper.SetAttributeMap("fullName", x => x.Name);

You can also prevent automatic mapping through the `[NotXmlMapped]` attribute.
