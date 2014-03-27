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
            _input = "<SampleXml DoubleProp=\"5.0\">" +
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

        }

        [Test]
        public void IntPropertyParsedCorrectly()
        {

        }

        [Test]
        public void StringPropertyParsedCorrectly()
        {

        }

        [Test]
        public void NamedAttributePropertyParsedCorrectly()
        {

        }

        [Test]
        public void NamedElementPropertyParsedCorrectly()
        {

        }

    }
}
