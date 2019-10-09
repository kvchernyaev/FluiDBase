using System;
using System.Collections.Generic;
using System.Linq;
using FluiDBase;
using NUnit.Framework;

namespace FluiDBaseTests
{
    [TestFixture]
    public class ArgsParserTests
    {

        [Test]
        public void TestOk() => Test(
            @"konstantinche:myid runOnChange:true runAlways:false endDelimiter:\ngo runInTransaction:false  stripComments:false",
                new KeyValuePair<string, string>("konstantinche", "myid"),
                new KeyValuePair<string, string>("runOnChange", "true"),
                new KeyValuePair<string, string>("runAlways", "false"),
                new KeyValuePair<string, string>("endDelimiter", @"\ngo"),
                new KeyValuePair<string, string>("runInTransaction", "false"),
                new KeyValuePair<string, string>("stripComments", "false")
        );


        [Test]
        public void TestSimplest() => Test(
            @"sdf:qwe",
            new KeyValuePair<string, string>("sdf", "qwe")
        );


        [Test]
        public void TestSimplestSpaces() => Test(
            @"sdf : qwe",
            new KeyValuePair<string, string>("sdf", "qwe")
        );


        [Test]
        public void TestSimplest1() => Test(
            @"sdf",
            new KeyValuePair<string, string>("sdf", null)
        );


        [Test]
        public void TestSimplest2() => Test(
            @"sdf:",
            new KeyValuePair<string, string>("sdf", null)
        );


        [Test]
        public void TestSimplest3() => Test(
            @"sdf: ",
            new KeyValuePair<string, string>("sdf", null)
        );


        [Test]
        public void TestSimplest4() => Assert.Catch<ArgumentException>(() => Test(@": ")); 


        [Test]
        public void TestSimplest5() => Assert.Catch<ArgumentException>(() => Test(@": va ")); 


        [Test]
        public void TestPair() => Test(
            @"sdf : qwe er",
            new KeyValuePair<string, string>("sdf", "qwe"),
            new KeyValuePair<string, string>("er", null)
        );


        [Test]
        public void TestQuote() => Test(
            @"sdf : ""qwe """,
            new KeyValuePair<string, string>("sdf", "qwe ")
        );


        [Test]
        public void TestBothQuote() => Test(
            @""" my key "" : "" val:\""i\"" """,
            new KeyValuePair<string, string>(" my key ", " val:\"i\" ")
        );


        [Test]
        public void TestQuoteNotClosed() => Assert.Catch<ArgumentException>(() => Test(@""" my key "));


        [Test]
        public void TestQuoteNotClosed2() => Assert.Catch<ArgumentException>(() => Test(@"k:"" quote "));



        void Test(string s, params KeyValuePair<string, string>[] expected)
        {
            var p = new ArgsParser(':');
            List<KeyValuePair<string, string>> real = p.Parse(s);

            DoAssert(expected.ToList(), real, s);
        }


        void DoAssert(List<KeyValuePair<string, string>> expected, List<KeyValuePair<string, string>> real, string wholeString)
        {
            if (expected == real)
                return;

            for (int i = 0; i < expected.Count; i++)
            {
                if (i >= real.Count)
                    Assert.Fail($"Lacks for arguments: {string.Join(", ", expected.Skip(real.Count).Select(Format))}. Whole string is [{wholeString}]");

                Assert.AreEqual(expected[i].Key, real[i].Key, $"expected parameter key [{expected[i].Key}], but provided [{real[i].Key}]. Whole string is [{wholeString}]");
                Assert.AreEqual(expected[i].Value, real[i].Value, $"expected parameter [{expected[i].Key}] value [{expected[i].Value}], but provided [{real[i].Value}]. Whole string is [{wholeString}]");
            }

            if (expected.Count < real.Count)
                Assert.Fail($"Excess arguments provided: {string.Join(", ", real.Skip(expected.Count).Select(Format))}. Whole string is [{wholeString}]");
        }


        string Format(KeyValuePair<string, string> kvp) => $"[{kvp.Key}]=[{kvp.Value}]";
    }
}
