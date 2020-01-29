using System;
using System.Collections.Generic;
using System.Text;
using FluiDBase;
using NUnit.Framework;

namespace FluiDBaseTests
{
    [TestFixture]
    public class FilterTests
    {
        [Test] public void Test_Exclude_Empty_Allowed() => Assert.IsFalse(new Filter(null, true).Exclude(null, true));
        [Test] public void Test_Exclude_Empty_Allowed_() => Assert.IsFalse(new Filter(new[] { "cont" }, true).Exclude(null, true));
        [Test] public void Test_Exclude_Empty_Allowed__() => Assert.IsFalse(new Filter(new[] { "cont" }, true).Exclude(null, false));


        [Test] public void Test_Exclude_Empty_Excluded() => Assert.IsTrue(new Filter(null, false).Exclude(null, true));
        [Test] public void Test_Exclude_Empty_Excluded_() => Assert.IsTrue(new Filter(new[] { "cont" }, false).Exclude(null, true));


        [Test] public void Test_Exclude_Empty_Ignored() => Assert.IsFalse(new Filter(null, true).Exclude(null, false));
        [Test] public void Test_Exclude_Empty_Ignored_() => Assert.IsFalse(new Filter(null, false).Exclude(null, false));
        [Test] public void Test_Exclude_Empty_Ignored__() => Assert.IsFalse(new Filter(new[] { "cont" }, false).Exclude(null, false));


        [Test] public void Test_Exclude_Single_Excluded() => Assert.IsTrue(new Filter(null, true).Exclude("any_context", true));
        [Test] public void Test_Exclude_Single_Excluded_() => Assert.IsTrue(new Filter(null, true).Exclude("any_context", false));
        [Test] public void Test_Exclude_Single_Excluded__() => Assert.IsTrue(new Filter(null, false).Exclude("any_context", true));
        [Test] public void Test_Exclude_Single_Excluded___() => Assert.IsTrue(new Filter(null, false).Exclude("any_context", false));


        [Test] public void Test_Exclude_Context_Excluded() => Assert.IsTrue(new Filter(new[] { "cont" }, true).Exclude("any_context", true));
        [Test] public void Test_Exclude_Context_Excluded_() => Assert.IsTrue(new Filter(new[] { "cont" }, true).Exclude("any_context", false));
        [Test] public void Test_Exclude_Context_Excluded__() => Assert.IsTrue(new Filter(new[] { "cont" }, false).Exclude("any_context", true));
        [Test] public void Test_Exclude_Context_Excluded___() => Assert.IsTrue(new Filter(new[] { "cont" }, false).Exclude("any_context", false));
        [Test] public void Test_Exclude_Context_Excluded_Mult() => Assert.IsTrue(new Filter(new[] { "cont", "c2" }, true).Exclude("any_context", true));
        [Test] public void Test_Exclude_Context_Excluded_Mult_() => Assert.IsTrue(new Filter(new[] { "cont", "c2" }, true).Exclude("any_context , any2", true));
        [Test] public void Test_Exclude_Context_Excluded_Mult__() => Assert.IsTrue(new Filter(new[] { "cont" }, true).Exclude("any_context , any2", true));


        [Test] public void Test_Exclude_Context_Allowed() => Assert.IsFalse(new Filter(new[] { "cont" }, true).Exclude("cont", true));
        [Test] public void Test_Exclude_Context_Allowed_() => Assert.IsFalse(new Filter(new[] { "cont" }, true).Exclude("cont", false));
        [Test] public void Test_Exclude_Context_Allowed__() => Assert.IsFalse(new Filter(new[] { "cont" }, false).Exclude("cont", true));
        [Test] public void Test_Exclude_Context_Allowed___() => Assert.IsFalse(new Filter(new[] { "cont" }, false).Exclude("cont", false));


        [Test] public void Test_Exclude_MultContext_Allowed() => Assert.IsFalse(new Filter(new[] { "cont", "c2" }, true).Exclude("cont", true));
        [Test] public void Test_Exclude_MultContext_Allowed_() => Assert.IsFalse(new Filter(new[] { "cont", "c2" }, true).Exclude("cont,a", true));
        [Test] public void Test_Exclude_MultContext_Allowed__() => Assert.IsFalse(new Filter(new[] { "cont", "c2" }, true).Exclude("a,c2", true));
    }
}
