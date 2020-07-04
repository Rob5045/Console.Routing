﻿using ConsoleRouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ConsoleRouting.Tests
{
    [TestClass]
    public class TestCommands
    {
        Router router = new RouteBuilder().AddAssemblyOf<TestCommands>().Build();

        [TestMethod]
        public void PlainCommandAttribute()
        {
            var arguments = Arguments.Parse("tool");
            var result = router.Bind(arguments);

            Assert.AreEqual(1, result.Routes.Count());
            Assert.AreEqual("Tool", result.Route.Method.Name);
            Assert.AreEqual(0, result.Bind.Arguments.Length);
        }

        [TestMethod]
        public void DefaultCommand()
        {
            var arguments = Arguments.Parse("");
            var result = router.Bind(arguments);

            Assert.AreEqual("Info", result.Route.Method.Name);
        }


        [TestMethod]
        public void Binding()
        {
            var arguments = Arguments.Parse("action William will --foo --bar fubar");
            var result = router.Bind(arguments);

            Assert.AreEqual(true, result.Ok);
            Assert.AreEqual(result.BindCount, 1);
            Assert.AreEqual(4, result.Bind.Arguments.Count());
            Assert.IsTrue(result.Candidates.Count > 1);
            
            var bind = result.Bind;
            Assert.AreEqual(bind.Route.Method.Name, "Action");

            var paramlist = bind.Route.Method.GetRoutingParameters();
            Assert.AreEqual(paramlist.Count(), 4);

            Assert.AreEqual(bind.Arguments[0], "William");
            Assert.AreEqual(bind.Arguments[1], "will");
            Assert.AreEqual(((Flag)bind.Arguments[2]).Set, true); // -foo

            Assert.AreEqual("fubar", (FlagValue)bind.Arguments[3]); // -bar fubar
        }

        [TestMethod]
        public void Nesting()
        {
            var arguments = Arguments.Parse("main action hello");
            var result = router.Bind(arguments);
            Assert.AreEqual("Action", result.Route.Method.Name);
            Assert.AreEqual("main", result.Route.Nodes.First().Names.First());
            Assert.AreEqual("Action", result.Route.Nodes.Skip(1).First().Names.First());
            Assert.AreEqual(1, result.BindCount);

            arguments = Arguments.Parse("main sub detail hello");
            result = router.Bind(arguments);
            Assert.AreEqual("main", result.Route.Nodes.First().Names.First());
            Assert.AreEqual("Detail", result.Route.Method.Name);
            Assert.AreEqual("sub", result.Route.Nodes.Skip(1).First().Names.First());
            Assert.AreEqual("Detail", result.Route.Nodes.Skip(2).First().Names.First());
            Assert.AreEqual(1, result.BindCount);
        }

        [TestMethod]
        public void ForgetSubCommand()
        {
            var arguments = Arguments.Parse("mainfirst");
            var result = router.Bind(arguments);
            var count = result.Candidates.Count(RouteMatch.Partial);
            Assert.AreEqual(2, count);
        }
    }
}
