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
        private Dictionary<string, string> _elementPropertyMap;
        private Dictionary<string, string> _attributePropertyMap;

        private string _elementName;

        private XDocument _xml;

        /// <summary>
        /// Create mapper to parse xml files to objects.
        /// </summary>
        /// <param name="elementName">If the element name differs from the class name, provide the target element name here.</param>
        private Mapper(string elementName = null)
        {
            _elementPropertyMap = new Dictionary<string, string>();
            _attributePropertyMap = new Dictionary<string, string>();

            if (elementName != null)
            {
                _elementName = elementName;
            }
            else
            {
                //Check if the class has an attribute specifying the element name. If not use, the class name.
                var elementNameAttr = (ElementNameAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ElementNameAttribute));
                _elementName = elementNameAttr != null ? elementNameAttr.ElementName : typeof(T).Name;
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
                if (elementAttr != null) //TODO is this right?
                {
                    if(!_elementPropertyMap.ContainsKey(elementAttr.ElementName))
                        _elementPropertyMap.Add(elementAttr.ElementName, p.Name);
                }

                //Attribute -> Property mappings
                AttributeNameAttribute attributeAttr = p.GetCustomAttribute<AttributeNameAttribute>();
                if (attributeAttr != null) //TODO is this right?
                {
                    if (!_attributePropertyMap.ContainsKey(attributeAttr.AttributeName))
                        _attributePropertyMap.Add(attributeAttr.AttributeName, p.Name);
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
        public void SetElementMap(string from, string to)
        {
            if (_elementPropertyMap.ContainsKey(from))
            {
                _elementPropertyMap[from] = to;
            }
            else
            {
                _elementPropertyMap.Add(from, to);
            }

            if (_attributePropertyMap.ContainsKey(from))
            {
                _attributePropertyMap.Remove(from);
            }
        }

        /// <summary>
        /// Set a custom mapping from an attribute value to a property.
        /// </summary>
        /// <param name="from">Name of the XML element.</param>
        /// <param name="to">Name of the class property.</param>
        public void SetAttributeMap(string from, string to)
        {
            if (_attributePropertyMap.ContainsKey(from))
            {
                _attributePropertyMap[from] = to;
            }
            else
            {
                _attributePropertyMap.Add(from, to);
            }

            if (_elementPropertyMap.ContainsKey(from))
            {
                _elementPropertyMap.Remove(from);
            }
        }

        /// <summary>
        /// Set the xml input to be parsed.
        /// </summary>
        public void SetXml(string xml)
        {
            _xml = XDocument.Load(new StringReader(xml));
        }

        /// <summary>
        /// Set the xml input to be parsed.
        /// </summary>
        public void SetXml(XDocument xml)
        {
            _xml = xml;
        }

        /// <summary>
        /// Set the xml input to be parsed.
        /// </summary>
        public void SetXml(IEnumerable<XElement> xml)
        {
            _xml = new XDocument(xml);
        }

        /// <summary>
        /// Parse the first matching element.
        /// </summary>
        public T ParseOne()
        {
            if (_xml == null) throw new Exception("No XML data set");

            //TODO maybe this should keep parsing the next element every time it's called?

            XElement firstElement = _xml.Descendants(_elementName).FirstOrDefault();

            if (firstElement == null)
                throw new Exception("No matching elements found");

            return ParseElement(firstElement);
        }

        /// <summary>
        /// Parse all matching elements.
        /// </summary>
        public List<T> ParseAll()
        {
            if (_xml == null) throw new Exception("No XML data set");

            var list = _xml.Descendants(_elementName).Select(ParseElement).ToList(); //todo if empty?

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
                ParseString(attr.Value, ref result, _attributePropertyMap[attr.Name.LocalName]);
            }

            //Parse any subelements
            foreach (string elementName in _elementPropertyMap.Keys)
            {
                XElement subElement = e.Element(elementName);
                if (subElement != null)
                {
                    ParseString(subElement.Value, ref result, _elementPropertyMap[elementName]);
                }
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

            if (propertyType == typeof(double))
            {
                double value;
                if (double.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(int))
            {
                int value;
                if (int.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(decimal))
            {
                decimal value;
                if (decimal.TryParse(input, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(string))
            {
                propertyInfo.SetValue(target, input);
            }
            else if (propertyType == typeof(DateTime))
            {
                DateTime dt;
                bool success = false;
                
                if(!string.IsNullOrEmpty(parseOptions))
                {
                    success = DateTime.TryParse(input, out dt);
                }
                else
                {
                    success = DateTime.TryParseExact(input, parseOptions, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                }

                if (success)
                {
                    propertyInfo.SetValue(target, dt);
                }
            }
        }
    }
}
