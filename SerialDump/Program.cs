using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialDump
{
    class Program
    {
        static string _portName;
        static int _baudRate = 115200;
        static Parity _parity = Parity.None;
        static int _dataBits = 8;
        static StopBits _stopBits = StopBits.One;

        static void ShowLogo()
        {
            System.Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("SerialDump v{0} - Serial Port Hex Dump Utility", v);
            Console.WriteLine("Copyright (C) 2020 Topten Software. All Rights Reserved.");

            Console.WriteLine("");
        }

        static void ShowHelp()
        {
            Console.WriteLine("usage: SerialDump <portname> [options]");
            Console.WriteLine();

            Console.WriteLine("Options:");
            Console.WriteLine("  --baud:<value>         Set baud rate (default 115200)");
            Console.WriteLine("  --databits:<value>     Number of data bits");
            Console.WriteLine("  --parity:<value>       None (default) | Odd | Even | Mark | Space");
            Console.WriteLine("  --stopbits:<value>     None | One (default) | Two | OnePointFive");
            Console.WriteLine();
        }

        public static bool ProcessArg(string arg)
        {
            if (arg == null)
                return true;

            // Args are in format [/-]<switchname>[:<value>];
            if (arg.StartsWith("--"))
            {
                string SwitchName = arg.Substring(2);
                string Value = null;

                int colonpos = SwitchName.IndexOf(':');
                if (colonpos >= 0)
                {
                    // Split it
                    Value = SwitchName.Substring(colonpos + 1);
                    SwitchName = SwitchName.Substring(0, colonpos);
                }

                switch (SwitchName)
                {
                    case "help":
                    case "h":
                    case "?":
                        ShowLogo();
                        ShowHelp();
                        return false;

                    case "v":
                    case "version":
                        ShowLogo();
                        return false;

                    case "baud":
                        _baudRate = int.Parse(Value);
                        break;

                    case "parity":
                        _parity = (Parity)Enum.Parse(typeof(Parity), Value);
                        break;

                    case "stopbits":
                        _stopBits = (StopBits)Enum.Parse(typeof(StopBits), Value);
                        break;

                    case "databits":
                        _dataBits = int.Parse(Value);
                        break;

                    default:
                        throw new InvalidOperationException(string.Format("Unknown switch '{0}'", arg));
                }
            }
            else
            {
                if (_portName == null)
                    _portName = arg;
                else
                    throw new InvalidOperationException(string.Format("Too many command line arguments, don't know what to do with '{0}'", arg));
            }

            return true;
        }

        static int Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!ProcessArg(args[i]))
                    return 0;
            }

            if (string.IsNullOrEmpty(_portName))
            {
                ShowLogo();
                ShowHelp();
                Console.WriteLine("No port name specified, quitting.");
                Console.WriteLine();
                return 7;
            }

            try
            {
                var sp = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
                sp.Open();

                int totalReceived = 0;
                var buf = new byte[16];
                while (true)
                {
                    var received = sp.Read(buf, 0, buf.Length);
                    for (int i = 0; i < received; i++)
                    {
                        if (((totalReceived + i) % 16) == 0)
                        {
                            if (totalReceived + i > 0)
                                Console.WriteLine();
                            Console.Write("{0:X4}: ", totalReceived + i);
                        }

                        Console.Write("{0:X2} ", buf[i]);
                    }
                    totalReceived += received;
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
                return 7;
            }
        }
    }
}
