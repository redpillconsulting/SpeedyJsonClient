using System;
using System.Collections.Generic;

namespace JsonClient.Test.TestData {
    public class Person {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Person> Children { get; set; }

    }
}