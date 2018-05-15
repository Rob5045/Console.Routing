﻿using System;
using System.Linq;
using System.Reflection;

namespace Shell.Routing
{
    public class Route
    {
        public Module Section { get; }
        public Command Command { get; }
        public Type Type { get; }
        public MethodInfo Method { get; }

        public Route(Module section, Command command, Type type, MethodInfo method)
        {
            Section = section;
            Command = command;
            Type = type;
            Method = method;
        }

        public override string ToString()
        {
            var pars = this.ParametersDescription();
            return $"{Section.Name.ToLower()} {Method.Name.ToLower()} {pars}";
        }
    }

}