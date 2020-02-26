using System;
using JsonClient.Test.TestData;

namespace JsonClient.Test {
    public class TestFixture {
        public TestFixture () { }

        public Person CreatePerson (Guid? id = null) {
            return new Person () {
            Id = id ?? Guid.NewGuid (),
            Name = "My name",
            Age = 12,
            Children = new System.Collections.Generic.List<Person> () {
            new Person () {
            Id = Guid.NewGuid (),
            Name = "My name",
            Age = 12
            }
            }
            };
        }
    }
}