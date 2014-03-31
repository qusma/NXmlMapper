using System;
using NXmlMapper;

namespace Tests
{
    public class TestClass
    {
        public double DoubleProp { get; set; }

        public decimal DecimalProp { get; set; }

        public int IntProp { get; set; }

        public string StringProp { get; set; }

        [NotXmlMapped]
        public int NotMapped { get; set; }

        [AttributeName("Foo")]
        public int PropWithSpecifiedAttributeName { get; set; }

        [ElementName("Bar")]
        public int PropWithSpecifiedEelementName { get; set; }

        public int FooBar { get; set; }

        public bool BoolProp { get; set; }

        [AttributeName("date", "yyyy;MM;dd")]
        public DateTime Date { get; set; }
    }
}
