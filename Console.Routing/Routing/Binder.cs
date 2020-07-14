﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoleRouting
{
    public static class Binder 
    {
        
        public static void Bind(IEnumerable<Type> types, Arguments arguments)
        {
            if (types is null) return;

            foreach(var type in types)
            {
                Bind(type, arguments);
            }
        }

        public static void Bind(Type type, Arguments arguments)
        {
            if (type is null) return;

            var globals = new List<IArgument>();

            foreach (var arg in arguments)
            {
                if (arg is Flag f && type.GetProperty(typeof(bool), f.Name) is PropertyInfo pb)
                {
                    pb.SetValue(null, true);
                    globals.Add(arg);
                }
                //else if (arg is Flag<string> && type.GetProperty(typeof(string), t.Value) is PropertyInfo ps)
                //{
                //    ps.SetValue(null, t.Value);
                //    globals.Add(arg);
                //}
                
            }

            foreach (var a in globals) arguments.Remove(a);
        }

        public static bool TryBind(Route route, Arguments arguments, out Bind bind)
        {
            if (TryBindParameters(route, arguments, out var values))
            {
                bind = new Bind(route, values);
                return true;
            }
            else
            {
                bind = null;
                return false;
            }
        }

        public static bool TryBindParameters(Route route, Arguments arguments, out object[] values)
        {
            var parameters = route.Method.GetRoutingParameters().ToArray();
            var offset = route.Nodes.Count(); // amount of parameters to skip, because they are commands.
            var argcount = arguments.Count - offset;
            var count = parameters.Length;

            values = new object[count];
            int ip = 0; // index of parameters
            int used = 0; // arguments used;

            foreach (var param in parameters)
            {
                int ia = offset + ip; // index of arguments
                if (param.Type == typeof(string))
                {
                    if (arguments.TryGetText(ia, out Text Text))
                    {
                        values[ip++] = Text.Value;
                        used++;
                    }

                    else if (param.Optional)
                    {
                        values[ip++] = null;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (param.Type.IsEnum)
                {
                    if (arguments.TryGetEnum(ia, param, out object value))
                    {
                        values[ip++] = value;
                        used++;
                    }
                    else
                    {
                        return false;
                    }

                }

                else if (param.Type == typeof(int))
                {
                    if (arguments.TryGetInt(ia, out int value))
                    {
                        values[ip++] = value;
                        used++;
                    }
                }

                else if (param.Type == typeof(Assignment))
                {
                    if (arguments.TryGetAssignment(param.Name, out Assignment assignment))
                    {
                        values[ip++] = assignment;
                        used++;
                    }
                    else
                    {
                        values[ip++] = Assignment.NotProvided();
                    }
                }
               
                else if (param.Type.IsGenericType && param.Type.GetGenericTypeDefinition() == typeof(Flag<>))
                {
                    Type genarg = param.Type.GetGenericArguments()[0];

                    if (arguments.TryGetOptionString(param, out string value))
                    {
                        if (genarg == typeof(string))
                        {
                            values[ip++] = new Flag<string>(null, value);
                            used += 2;
                        }
                        else if (genarg.IsEnum)
                        {
                            var enumvalue = Enum.Parse(param.Type.GetGenericArguments()[0], value, ignoreCase: true);
                            var t = typeof(Flag<>).MakeGenericType(genarg);
                            var flagt = Activator.CreateInstance(t, param.Name, enumvalue, true, true);
                            values[ip++] = flagt;
                            used += 2;
                        }

                    }
                    else
                    {
                        values[ip++] = Flag<string>.NotGiven;
                    }


                }

                else if (param.Type == typeof(Flag))
                {
                    if (arguments.TryGet(param, out Flag flag))
                    {
                        values[ip++] = flag;
                        used++;
                    }
                    else
                    {
                        values[ip++] = new Flag(param.Name, set: false);
                    }
                }

                else if (param.Type == typeof(bool))
                {
                    if (arguments.TryGet(param, out Flag flag))
                    {
                        values[ip++] = true;
                        used++;
                    }
                    else
                    {
                        values[ip++] = false;
                    }
                }

                else if (param.Type == typeof(Arguments))
                {
                    values[ip++] = arguments;
                    return true;
                }
                else
                {
                    // this method has a signature with an unknown type.
                    return false;
                }
            }
            return (argcount == used);

        }
    }
}


