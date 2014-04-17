using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

// This class parses XML files and creates objects, filling property values
// by parsing element and attribute values in the XML.

namespace NXmlMapper
{
    public class Mapper<T> where T: new()
    {
        private readonly Dictionary<string, string> _elementPropertyMap;
        private readonly Dictionary<string, string> _elementParseOptions;
        private readonly Dictionary<string, string> _attributePropertyMap;
        private readonly Dictionary<string, string> _attributeParseOptions;

        private readonly string _elementName;

        private IEnumerable<XElement> _xml;
        private IEnumerator<XElement> _xmlEnumerator;

        /// <summary>
        /// Create mapper to parse xml files to objects.
        /// </summary>
        /// <param name="elementName">If the element name differs from the class name, provide the target element name here.</param>
        private Mapper(string elementName = null)
        {
            _elementPropertyMap = new Dictionary<string, string>();
            _elementParseOptions = new Dictionary<string, string>();
            _attributePropertyMap = new Dictionary<string, string>();
            _attributeParseOptions = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(elementName))
            {
                //Check if the class has an attribute specifying the element name. If not use, the class name.
                var elementNameAttr = (ElementNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ElementNameAttribute));
                _elementName = elementNameAttr != null ? elementNameAttr.ElementName : typeof(T).Name;
            }
            else
            {
                _elementName = elementName;
            }

            GetMapsFromAttributes();
            GetMapsFromProperties();
        }

        /// <summary>
        /// Create mapper to parse xml files to objects.
        /// </summary>
        /// /// <param name="xml">The XML to be parsed.</param>
        /// <param name="elementName">If the element name differs from the class name, provide the target element name here.</param>
        public Mapper(string xml, string elementName = null) 
            : this(elementName)
        {
            SetXml(xml);
        }

        /// <summary>
        /// Create mapper to parse xml files to objects.
        /// </summary>
        /// <param name="xml">The XML to be parsed.</param>
        /// <param name="elementName">If the element name differs from the class name, provide the target element name here.</param>
        public Mapper(XDocument xml, string elementName = null)
            : this(elementName)
        {
            SetXml(xml);
        }

        /// <summary>
        /// Create mapper to parse xml files to objects.
        /// </summary>
        /// /// <param name="xml">The XML to be parsed.</param>
        /// <param name="elementName">If the element name differs from the class name, provide the target element name here.</param>
        public Mapper(IEnumerable<XElement> xml, string elementName = null)
            : this(elementName)
        {
            SetXml(xml);
        }

        /// <summary>
        /// Go through the properties and look for custom mapping attributes, 
        /// then add them to the dictionaries. In case of duplicates, the first instance holds.
        /// </summary>
        private void GetMapsFromAttributes()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                //Element -> Property mappings
                ElementNameAttribute elementAttr = p.GetCustomAttribute<ElementNameAttribute>();
                if (elementAttr != null)
                {
                    SetElementMap(elementAttr.ElementName, p.Name, elementAttr.ParseOptions);
                }

                //Attribute -> Property mappings
                AttributeNameAttribute attributeAttr = p.GetCustomAttribute<AttributeNameAttribute>();
                if (attributeAttr != null)
                {
                    SetAttributeMap(attributeAttr.AttributeName, p.Name, attributeAttr.ParseOptions);
                }
            }
        }

        /// <summary>
        /// Go through the properties, any that don't have custom mapping or the ignore attribute
        /// are added as attribute->property mappings. 
        /// </summary>
        private void GetMapsFromProperties()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                if (p.GetCustomAttribute<ElementNameAttribute>() == null && 
                    p.GetCustomAttribute<AttributeNameAttribute>() == null &&
                    p.GetCustomAttribute<NotXmlMappedAttribute>() == null)
                {
                    _attributePropertyMap.Add(p.Name, p.Name);
                }
            }
        }

        /// <summary>
        /// Set a custom mapping from a sub-element value to a property.
        /// </summary>
        /// <param name="from">Name of the XML element.</param>
        /// <param name="to">Name of the class property.</param>
        /// <param name="parseOptions">Optional parse options.</param>
        public void SetElementMap(string from, string to, string parseOptions = null)
        {
            //if there's an attribute mapping, remove it
            RemoveMappingsToProperty(to);

            //add the element mapping
            _elementPropertyMap.Add(from, to);

            //Parse options
            if (_elementParseOptions.ContainsKey(from))
            {
                _elementParseOptions[from] = parseOptions;
            }
            else if (!string.IsNullOrEmpty(parseOptions))
            {
                _elementParseOptions.Add(from, parseOptions);
            }
        }

        /// <summary>
        /// Set a custom mapping from an attribute value to a property.
        /// </summary>
        /// <param name="from">Name of the XML element.</param>
        /// <param name="to">Name of the class property.</param>
        /// <param name="parseOptions">Optional parse options.</param>
        public void SetAttributeMap(string from, string to, string parseOptions = null)
        {
            //If there's a corresponding element mapping for this property, remove it
            RemoveMappingsToProperty(to);

            //add the attribute mapping
            _attributePropertyMap.Add(from, to);

            //Parse options
            if (_attributeParseOptions.ContainsKey(from))
            {
                _attributeParseOptions[from] = parseOptions;
            }
            else if (!string.IsNullOrEmpty(parseOptions))
            {
                _attributeParseOptions.Add(from, parseOptions);
            }
        }

        /// <summary>
        /// Remove any mappings to a property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        private void RemoveMappingsToProperty(string propertyName)
        {
            foreach (var key in _attributePropertyMap.Where(x => x.Value == propertyName).Select(x => x.Key).ToList())
            {
                _attributePropertyMap.Remove(key);
            }

            foreach (var key in _elementPropertyMap.Where(x => x.Value == propertyName).Select(x => x.Key).ToList())
            {
                _elementPropertyMap.Remove(key);
            }
        }

        /// <summary>
        /// Set the xml input to be parsed.
        /// </summary>
        public void SetXml(string xml)
        {
            SetXml(XDocument.Load(new StringReader(xml)).Descendants(_elementName));
        }

        /// <summary>
        /// Set the xml input to be parsed.
        /// </summary>
        public void SetXml(XDocument xml)
        {
            SetXml(xml.Descendants(_elementName));
        }

        /// <summary>
        /// Set the xml input to be parsed.
        /// </summary>
        public void SetXml(IEnumerable<XElement> xml)
        {
            _xml = xml;
            _xmlEnumerator = _xml.GetEnumerator();
        }

        /// <summary>
        /// Parse the next element. Returns null if the collection has been exhausted.
        /// </summary>
        public T ParseNext()
        {
            if (_xml == null) throw new Exception("No XML data set");

            _xmlEnumerator.MoveNext();
            XElement element = _xmlEnumerator.Current;

            if (element == null) return default(T);

            return ParseElement(element);
        }

        /// <summary>
        /// Parse all elements.
        /// </summary>
        public List<T> ParseAll()
        {
            if (_xml == null) throw new Exception("No XML data set");

            var list = _xml.Select(ParseElement).ToList();

            return list;
        }

        /// <summary>
        /// Parses a single XML element's attribute values and sub-element values 
        /// into an object's properties based on the mappings.
        /// </summary>
        private T ParseElement(XElement e)
        {
            var result = new T();

            //Parse the attributes
            foreach (XAttribute attr in e.Attributes().Where(attr => _attributePropertyMap.ContainsKey(attr.Name.LocalName)))
            {
                string parseOptions = _attributeParseOptions.ContainsKey(attr.Name.LocalName) 
                    ? _attributeParseOptions[attr.Name.LocalName] 
                    : null;
                ParseString(attr.Value, ref result, _attributePropertyMap[attr.Name.LocalName], parseOptions);
            }

            //Parse any subelements
            foreach (string elementName in _elementPropertyMap.Keys)
            {
                XElement subElement = e.Element(elementName);
                if (subElement == null) continue;

                string parseOptions = _elementParseOptions.ContainsKey(elementName)
                    ? _elementParseOptions[elementName]
                    : null;

                ParseString(subElement.Value, ref result, _elementPropertyMap[elementName], parseOptions);
            }

            return result;
        }

        /// <summary>
        /// Parses a string into an object's property based on the property type.
        /// </summary>
        /// <param name="input">String to be parsed.</param>
        /// <param name="target">Target object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="parseOptions">Optional: options to be passed to the parser. Example: for exact datetime parsing.</param>
        private void ParseString(string input, ref T target, string propertyName, string parseOptions = null)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
            Type propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(double) || propertyType == typeof(double?))
            {
                double value;
                if (double.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(float) || propertyType == typeof(float?))
            {
                float value;
                if (float.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(int) || propertyType == typeof(int?))
            {
                int value;
                if (int.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(uint) || propertyType == typeof(uint?))
            {
                uint value;
                if (uint.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
            {
                decimal value;
                if (decimal.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(long) || propertyType == typeof(long?))
            {
                long value;
                if (long.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(ulong) || propertyType == typeof(ulong?))
            {
                ulong value;
                if (ulong.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(short) || propertyType == typeof(short?))
            {
                short value;
                if (short.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(ushort) || propertyType == typeof(ushort?))
            {
                ushort value;
                if (ushort.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(string))
            {
                propertyInfo.SetValue(target, input);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                DateTime dt;
                bool success = 
                    string.IsNullOrEmpty(parseOptions)
                        ? DateTime.TryParse(input, out dt)
                        : DateTime.TryParseExact(input, parseOptions, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

                if (success)
                {
                    propertyInfo.SetValue(target, dt);
                }
            }
            else if (propertyType == typeof(bool))
            {
                if (input == "1" || input.ToLower() == "true")
                    propertyInfo.SetValue(target, true);
                else
                    propertyInfo.SetValue(target, false);
            }
        }
    }
}
