using System;
using System.Collections.Generic;
using System.Text;
using FluiDBase;
using FluiDBase.Gather;
using NUnit.Framework;

namespace FluiDBaseTests
{
    [TestFixture]
    public class SqlGathererTests
    {
        SqlGatherer g => new SqlGatherer(new Filter(null, true));
        FileDescriptor fd = new FileDescriptor("/path/file.sql", new FileReader());


        [Test] public void Test_DoesMatch_NotType() => Assert.IsFalse(g.DoesMatch("no", "asfd"));
        [Test] public void Test_DoesMatch_NotContent() => Assert.IsFalse(g.DoesMatch(".sql", "asfd"));


        [Test] public void Test_DoesMatch() => Assert.IsTrue(g.DoesMatch(".sql", "--fluidbase"));
        [Test] public void Test_DoesMatch_Newline() => Assert.IsTrue(g.DoesMatch(".sql", Environment.NewLine + " -- fluidbase asdf"));


        [Test]
        public void Test_no_changeset()
        {
            var changesets = new List<ChangeSet>();

            g.GatherFromFile(@"-- fluidbase
script body without changeset header
",
                null, fd, changesets, new string[] { }, null);

            Assert.AreEqual(changesets.Count, 0);
        }


        [Test]
        public void Test_Header_Empty() => Assert.Throws<ProcessException>(() =>
            g.GatherFromFile(
            @"-- fluidbase

script body
-- changeset
",
            null, fd, new List<ChangeSet>(), new string[] { }, null));


        [Test]
        public void Test_One_Empty()
        {
            var changesets = new List<ChangeSet>();
            string fileContent = @"-- fluidbase
        --changeset author : myid
        ";
            
            g.GatherFromFile(fileContent, null, fd, changesets, new string[] { }, null);

            Assert.AreEqual(changesets.Count, 1);
            ChangeSet c = changesets[0];
            Assert.AreEqual(c.Author, "author");
            Assert.AreEqual(c.Id, "myid");
            Assert.AreEqual(c.RunAlways, false);
            Assert.AreEqual(c.RunOnChange, false);
            Assert.AreEqual(c.FileRelPath, fd.PathFromBase);
            Assert.AreEqual(c.Body, "");
        }


        [Test]
        public void Test_One_Header()
        {
            var changesets = new List<ChangeSet>();
            string fileContent = @"-- fluidbase
        --changeset author : myid runAlways:true runOnChange:true
script body!
        ";


            g.GatherFromFile(fileContent, null, fd, changesets, new string[] { }, null);

            Assert.AreEqual(changesets.Count, 1);
            ChangeSet c = changesets[0];
            Assert.AreEqual(c.Author, "author");
            Assert.AreEqual(c.Id, "myid");
            Assert.AreEqual(c.RunAlways, true);
            Assert.AreEqual(c.RunOnChange, true);
            Assert.AreEqual(c.FileRelPath, fd.PathFromBase);
            Assert.AreEqual(c.Body, "script body!");
        }




        [Test]
        public void Test_Two_Header()
        {
            var changesets = new List<ChangeSet>();
            string fileContent = @"-- fluidbase
--changeset konstantin : myid runAlways:true runOnChange:true

update t set a='a'
-- comment

--changeset konstantin : myid2 runAlways:true runOnChange:true
update t set a='b'
        ";


            g.GatherFromFile(fileContent, null, fd, changesets, new string[] { }, null);

            Assert.AreEqual(changesets.Count, 2);
            ChangeSet c = changesets[0];
            Assert.AreEqual(c.Author, "konstantin");
            Assert.AreEqual(c.Id, "myid");
            Assert.AreEqual(c.RunAlways, true);
            Assert.AreEqual(c.RunOnChange, true);
            Assert.AreEqual(c.FileRelPath, fd.PathFromBase);
            Assert.AreEqual(c.Body, @"update t set a='a'
-- comment");

            c = changesets[1];
            Assert.AreEqual(c.Author, "konstantin");
            Assert.AreEqual(c.Id, "myid2");
            Assert.AreEqual(c.RunAlways, true);
            Assert.AreEqual(c.RunOnChange, true);
            Assert.AreEqual(c.FileRelPath, fd.PathFromBase);
            Assert.AreEqual(c.Body, "update t set a='b'");
        }


        // errors - unique id
        // use props

        // comments is script
    }
}
