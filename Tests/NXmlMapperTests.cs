using System;
using System.Collections.Generic;
using NUnit.Framework;
using NXmlMapper;

namespace Tests
{
    [TestFixture]
    public class NXmlMapperTests
    {
        string _input;

        [SetUp]
        public void SetUp()
        {
            _input = "<SampleXml " +
                         "DoubleProp=\"5.0\" " +
                         "DecimalProp=\"1.23456789\" " +
                         "IntProp=\"123\" " +
                         "StringProp=\"BobTheBuilder\" " +
                         "NotMapped=\"11\" " +
                         "Foo=\"99\" " +
                         "date=\"2001;11;16\" " +
                         "BarFoo=\"1\">" +

                        "<Bar>55</Bar>" + 
                     "</SampleXml>";
        }

        [Test]
        public void DoublePropertyParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(5.0, t.DoubleProp);
        }

        [Test]
        public void DecimalPropertyParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(1.23456789, t.DecimalProp);
        }

        [Test]
        public void IntPropertyParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(123, t.IntProp);
        }

        [Test]
        public void StringPropertyParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual("BobTheBuilder", t.StringProp);
        }

        [Test]
        public void NotXmlMappedAttributeStopsMapping()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(0, t.NotMapped);
        }

        [Test]
        public void NamedAttributePropertyParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(99, t.PropWithSpecifiedAttributeName);
        }

        [Test]
        public void PropertyUsedWithSetAttributeMapParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            mapper.SetAttributeMap("BarFoo", "FooBar");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(1, t.FooBar);
        }

        [Test]
        public void NamedElementPropertyParsedCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(55, t.PropWithSpecifiedEelementName);
        }

        [Test]
        public void ParseAllReturnsAllElementsCorrectly()
        {
            string input = "<Collection>" +
                                "<TestClass IntProp=\"0\" /> " +
                                "<TestClass IntProp=\"1\" /> " +
                                "<TestClass IntProp=\"2\" /> " +
                                "<TestClass IntProp=\"3\" /> " +
                                "<TestClass IntProp=\"4\" /> " +
                                "<TestClass IntProp=\"5\" /> " +
                            "</Collection>";

            var mapper = new Mapper<TestClass>(input);
            List<TestClass> result = mapper.ParseAll();
            Assert.AreEqual(6, result.Count);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(i, result[i].IntProp);
            }
        }

        [Test]
        public void DateTimeParsingWorksCorrectly()
        {
            var mapper = new Mapper<TestClass>(_input, "SampleXml");
            TestClass t = mapper.ParseOne();
            Assert.AreEqual(new DateTime(2001, 11, 16), t.Date);
        }

        [Test]
        public void BoolPropertyOneIsParsedAsTrue()
        {
            string input = "<TestClass BoolProp=\"1\" /> ";
            var mapper = new Mapper<TestClass>(input);
            TestClass result = mapper.ParseOne();
            Assert.IsTrue(result.BoolProp);
        }

        [Test]
        public void BoolPropertyTrueStringIsParsedAsTrue()
        {
            string input = "<TestClass BoolProp=\"true\" /> ";
            var mapper = new Mapper<TestClass>(input);
            TestClass result = mapper.ParseOne();
            Assert.IsTrue(result.BoolProp);
        }

        [Test]
        public void BoolPropertyZeroIsParsedAsFalse()
        {
            string input = "<TestClass BoolProp=\"0\" /> ";
            var mapper = new Mapper<TestClass>(input);
            TestClass result = mapper.ParseOne();
            Assert.IsFalse(result.BoolProp);
        }

        [Test]
        public void NullableDoubleParsingWorksWhenValueIsNumber()
        {
            string input = "<TestClass NullableDoubleProp=\"0.05\" /> ";
            var mapper = new Mapper<TestClass>(input);
            TestClass result = mapper.ParseOne();
            Assert.AreEqual(0.05, result.NullableDoubleProp);
        }

        [Test]
        public void NullableDoubleParsingReturnsNullWhenParsingDoesNotWork()
        {
            string input = "<TestClass NullableDoubleProp=\"asdf\" /> ";
            var mapper = new Mapper<TestClass>(input);
            TestClass result = mapper.ParseOne();
            Assert.AreEqual(null, result.NullableDoubleProp);
        }
    }
}
