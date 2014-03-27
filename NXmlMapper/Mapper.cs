using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace NXmlMapper
{
    public class Mapper<T> where T: new()
    {
        private Dictionary<string, string> _elementPropertyMap;
        private Dictionary<string, string> _attributePropertyMap;

        private string _elementName;

        private XDocument _xml;

        public Mapper(string elementName = null)
        {
            _elementPropertyMap = new Dictionary<string, string>();
            _attributePropertyMap = new Dictionary<string, string>();

            _elementName = elementName ?? typeof(T).Name;

            GetMapsFromAttributes();
            GetMapsFromProperties();
        }

        public Mapper(string xml, string elementName = null) 
            : this(elementName)
        {
            SetXml(xml);
        }

        public Mapper(XDocument xml, string elementName = null)
            : this(elementName)
        {
            SetXml(xml);
        }

        public Mapper(IEnumerable<XElement> xml, string elementName = null)
            : this(elementName)
        {
            SetXml(xml);
        }

        private void GetMapsFromAttributes()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                ElementNameAttribute elementAttr = p.GetCustomAttribute<ElementNameAttribute>();
                if (elementAttr != null) //TODO is this right?
                {
                    _elementPropertyMap.Add(elementAttr.ElementName, p.Name);
                }

                AttributeNameAttribute attributeAttr = p.GetCustomAttribute<AttributeNameAttribute>();
                if (attributeAttr != null) //TODO is this right?
                {
                    _attributePropertyMap.Add(attributeAttr.AttributeName, p.Name);
                }
            }
        }

        private void GetMapsFromProperties()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                if (p.GetCustomAttribute<ElementNameAttribute>() == null && p.GetCustomAttribute<AttributeNameAttribute>() == null)
                {
                    _attributePropertyMap.Add(p.Name, p.Name);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">Name of the XML field.</param>
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

        public void SetXml(string xml)
        {
            _xml = XDocument.Load(new StringReader(xml));
        }

        public void SetXml(XDocument xml)
        {
            _xml = xml;
        }

        public void SetXml(IEnumerable<XElement> xml)
        {
            _xml = new XDocument(xml);
        }

        public T ParseOne()
        {
            if (_xml == null) throw new Exception("No XML data set");

            return ParseElement(_xml.Element(_elementName)); //TODO handle null
        }

        public List<T> ParseAll(bool recursive = false)
        {
            if (_xml == null) throw new Exception("No XML data set");

            var list = _xml.Elements(_elementName).Select(ParseElement).ToList();

            return list;
        }

        private T ParseElement(XElement e)
        {
            var result = new T();

            foreach (XAttribute attr in e.Attributes().Where(attr => _attributePropertyMap.ContainsKey(attr.Name.LocalName)))
            {
                ParseAttribute(attr, ref result, _attributePropertyMap[attr.Name.LocalName]);
            }

            return result;
        }

        private void ParseAttribute(XAttribute a, ref T target, string propertyName)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
            Type propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(double))
            {
                double value;
                if (double.TryParse(a.Value, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(int))
            {
                int value;
                if (int.TryParse(a.Value, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(decimal))
            {
                decimal value;
                if (decimal.TryParse(a.Value, out value))
                {
                    propertyInfo.SetValue(target, value);
                }
            }
            else if (propertyType == typeof(string))
            {
                propertyInfo.SetValue(target, a.Value);
            }
        }
    }
}
