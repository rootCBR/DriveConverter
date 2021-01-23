using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DriveConverter.Drive;

namespace DriveConverter.App
{
    public static class Config
    {
        public struct ArgumentInfo
        {
            public readonly string Name;
            public readonly string Value;

            public bool HasName
            {
                get { return !string.IsNullOrEmpty(Name); }
            }

            public bool HasValue
            {
                get { return !string.IsNullOrEmpty(Value); }
            }

            public bool IsEmpty
            {
                get { return !HasName && !HasValue; }
            }

            public bool IsSwitch
            {
                get { return HasName && !HasValue; }
            }

            public bool IsVariable
            {
                get { return HasName && HasValue; }
            }

            public bool IsValue
            {
                get { return HasValue && !HasName; }
            }

            public override string ToString()
            {
                if (this.HasName && this.HasValue)
                {
                    return $"{this.Name}={this.Value}";
                }

                if (this.HasName && !this.HasValue)
                {
                    return this.Name;
                }

                if (!this.HasName && this.HasValue)
                {
                    return this.Value;
                }

                return base.ToString();
            }

            public ArgumentInfo(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public ArgumentInfo(string argument)
            {
                if (argument == null)
                {
                    throw new ArgumentNullException("Can not create argument info from null string");
                }

                string arg = argument.TrimStart('-');

                if (arg != argument)
                {
                    if (arg.Length > 0)
                    {
                        var separator = arg.IndexOf("=");

                        if (separator != -1)
                        {
                            this.Name = arg.Substring(0, separator).ToLower();
                            this.Value = arg.Substring(separator + 1);
                        }
                        else
                        {
                            this.Name = arg.ToLower();
                            this.Value = string.Empty;
                        }
                    }
                    else
                    {
                        this.Name = string.Empty;
                        this.Value = string.Empty;
                    }
                }
                else
                {
                    this.Name = string.Empty;
                    this.Value = arg;
                }
            }
        }

        public static string Input;
        public static string HandlingType;

        public static IEnumerable<ArgumentInfo> Arguments
        {
            get { return _args; }
        }

        private static IEnumerable<ArgumentInfo> _args;

        static readonly string[] _types = { "dev", "rel" };

        public static readonly int BuildType =
#if (!DEBUG)
            1;
#else
            0;
#endif

        /*
            v1.00 23.01.2021 02:06
            v1.01 23.01.2021 22:47
            v1.02 23.01.2021 23:41
            v1.03 24.01.2021 00:39
        */
        public static readonly string BuildVersion = "1.03";

        public static string VersionString
        {
            get { return $"===== DriveConverter v{BuildVersion.ToString()}-{_types[BuildType]} ====="; }
        }

        static readonly string[] _usage = {
            "Usage:",
            "  DriveConverter.exe \"offroad_05.handling.bin\"",
            "  DriveConverter.exe \"civ_prestige_04.handling\""
        };

        public static string UsageString
        {
            get { return string.Join("\r\n", _usage); }
        }

        public static bool HasArg(string name)
        {
            foreach (var arg in _args)
            {
                if (arg.HasName && (arg.Name == name))
                    return true;
            }

            return false;
        }

        public static string GetArg(string name)
        {
            foreach (var arg in _args)
            {
                if (arg.HasName && (arg.Name == name))
                    return arg.Value;
            }

            return null;
        }

        public static bool GetArg(string name, ref int value)
        {
            int result = 0;

            foreach (var arg in _args)
            {
                if (arg.HasName && (arg.Name == name))
                {
                    if (int.TryParse(arg.Value, out result))
                    {
                        value = result;
                        return true;
                    }
                }
            }

            return false;
        }

        public static int ProcessArgs(string[] args)
        {
            //var optionCount = 0;
            var arguments = new List<ArgumentInfo>();

            for (int i = 0; i < args.Length; i++)
            {
                ArgumentInfo arg = new ArgumentInfo(args[i]);

                //Utility.Log.ToConsole("Argument: " + args[i]);

                if (arg.HasName)
                {
                    if (arg.IsSwitch)
                    {

                    }
                    else
                    {
                        switch (arg.Name)
                        {
                            case "type":
                                if (Parameters.Keys.ContainsKey(arg.Value.ToLower()))
                                {
                                    HandlingType = arg.Value;
                                }
                                else
                                {
                                    throw new ArgumentException("Invalid handling type specified");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (arg.IsValue)
                    {
                        Input = arg.Value;
                    }
                }

                arguments.Add(arg);
            }

            _args = arguments.AsEnumerable();

            return arguments.Count;
        }
    }
}
