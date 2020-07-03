﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ConsoleRouting.Tests
{
    [TestClass]
    public class TestParameters
    {
        Router router = new RouteBuilder().AddAssemblyOf<TestParameters>().Build();

        [TestMethod]
        public void SingleLiteral()
        {
            // ToolCommands.Single(string name) // 1 matching bind

            var arguments = Arguments.Parse("action Foo");
            var result = router.Bind(arguments);

            Assert.AreEqual(result.Count, 1);

            var bind = result.Bind;
            Assert.AreEqual(bind.Route.Method.Name, "Action");

            var parameters = bind.Route.Method.GetRoutingParameters();
            Assert.AreEqual(parameters.Count(), 1);

            Assert.AreEqual(1, bind.Arguments.Length);
            Assert.AreEqual(bind.Arguments[0], "Foo");
        }


        [TestMethod]
        public void FlagValue()
        {
            var arguments = Arguments.Parse("-a -b --test abc");
            var parameter = Parameters.Create<Flag>("test");

            arguments.TryGet(parameter, out Flag flag);
            Assert.AreEqual("test", flag.Name);
            arguments.TryGetFlagValue(parameter, out var value);
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        public void FlagValueBinding()
        {
            // since we match on used parameter count, flag vaues are a special case
            // one FlagValue consumes 2 command line arguments
            var arguments = Arguments.Parse("save --all --pattern '{id}-{id}'");
            var result = router.Bind(arguments);
            Assert.AreEqual(1, result.Count);

        }


        [TestMethod]
        public void AlternateParamNames()
        {
            var arguments = Arguments.Parse("-?"); // should route to ToolModule.Info
            var result = router.Bind(arguments);
            string s = result.ToString();
            Assert.AreEqual(1, result.Count);

        }
    }
}