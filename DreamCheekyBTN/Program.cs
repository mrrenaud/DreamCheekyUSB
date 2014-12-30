using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using DreamCheeky.Devices;

namespace DreamCheekyBTN
{
    class Program
    {
        static string _strCmd = "";
        static string _strCmdarGs = "";
        static string _strMacro = "";
        static int _count = 0;
        static void Main(string[] args)
        {
            int actions = 0;
            BigRedButton btn = null;
            try
            {
                if (args.ContainsInsensitive("debug"))
                {
                    System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
                }

                string devicearg = args.StartsWith("device=").FirstOrDefault();
                if (string.IsNullOrEmpty(devicearg))
                {
                    Console.WriteLine("\r\nConnecting to DreamCheekyBTN using default values...");
                    btn = new BigRedButton();
                }
                else
                {
                    Console.WriteLine("\r\nConnecting to DreamCheekyBTN using specified device...");
                    string[] deviceSplit = devicearg.Substring(7).Split(',');
                    if (deviceSplit.Length == 1)
                    {
                        btn = new BigRedButton(deviceSplit[0]); //One argument = device path
                    }
                    else
                    {
                        //Two or Three arguments = VID,PID,Count=0
                        int devicecount = 0;
                        if (deviceSplit.Length > 2)
                        {
                            devicecount = int.Parse(deviceSplit[2]);
                        }

                        int vid = int.Parse(deviceSplit[0].Substring(2), System.Globalization.NumberStyles.HexNumber);
                        int pid = int.Parse(deviceSplit[1].Substring(2), System.Globalization.NumberStyles.HexNumber);

                        btn = new BigRedButton(vid, pid, devicecount);
                    }
                }

                string cmdarg = args.StartsWith("CMD=").FirstOrDefault();
                if (!string.IsNullOrEmpty(cmdarg))
                {
                    actions++;
                    _strCmd = cmdarg.Substring(4);
                    Console.WriteLine("Setting command to: " + _strCmd);
                }

                string argarg = args.StartsWith("ARG=").FirstOrDefault();
                if (!string.IsNullOrEmpty(argarg))
                {
                    _strCmdarGs = argarg.Substring(4);
                    Console.WriteLine("Setting command arguments to: " + _strCmdarGs);
                }

                string macroarg = args.StartsWith("MACRO=").FirstOrDefault();
                if (!string.IsNullOrEmpty(macroarg))
                {
                    actions++;
                    string[] macosplit = macroarg.Split('=');
                    _strMacro = macosplit[1];
                    Console.WriteLine("Setting Macro to: " + _strMacro);
                }

                //if (actions > 0)
                //{
                btn.RegisterCallback(DoAction);
                Console.WriteLine("Listening for button press events. Press any key to escape...");
                Console.ReadKey(true);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n\r\nError: " + ex.Message + "\r\n\r\n");
            }
            finally
            {
                if (btn != null)
                {
                    btn = null;
                }
            }

            //Pause on exit or display usage syntax
            if (actions > 0)
            {
                Console.WriteLine("Finished\r\n");
            }
            else //No actions specified, show help
            {
                Console.WriteLine("  DreamCheekyBTN.exe [device=...] [options]");

                Console.WriteLine("\r\nExamples:");

                Console.WriteLine("  DreamCheekyBTN.exe debug MACRO=ASDF~  (ASDF then Enter)");
                Console.WriteLine("  DreamCheekyBTN.exe MACRO=%+{F1}       (ALT+SHIFT+F1)");
                Console.WriteLine("  DreamCheekyBTN.exe CMD=c:\\temp\\test.bat");
                Console.WriteLine(@"  DreamCheekyBTN.exe CMD=powershell ARG=""-noexit -executionpolicy unrestricted -File c:\test.ps1""");

                Console.WriteLine("\r\n\r\nDevice Path:");
                Console.WriteLine("  Optional, Defaults to first USB device with VID=0x1D34 and PID=0x000d");
                Console.WriteLine("  Example (VID,PID,Index): device=\"0x1D34,0x000d,0\"");
                Console.WriteLine("  Example (Path): device=" + @"""\\?\hid#vid_1d34&pid_000d#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}""");

                Console.WriteLine("\r\nOptions:");
                Console.WriteLine("  debug = Print trace statements to Console.Out");

                Console.WriteLine("\r\nCMD: will run specified command when button is pressed");
                Console.WriteLine("ARG: can be used to specified command arguments");
                Console.WriteLine("  Example (open calculator): CMD=calc");
                Console.WriteLine("  Example (run Powershell commands): ");
                Console.WriteLine("     CMD=\"%SystemRoot%\\system32\\WindowsPowerShell\\v1.0\\powershell.exe\"");
                Console.WriteLine(@"     ARG=""-Command \""& {write-host 'BEEP!'; [console]::beep(440,1000);}\""""");
                Console.WriteLine("  NOTE: use ^& instead of & if running from command prompt as & is special character");

                Console.WriteLine("\r\nMACRO: will send specified key sequense to active window via C# Sendkeys");
                Console.WriteLine("NOTE: +=Shift, ^=CTRL, %=ALT, ~=Return, use () to group characters.");
                Console.WriteLine("  Example: MACRO=\"%^g\"        (ALT + CTRL + g)");
                Console.WriteLine("  Example: MACRO=\"%(asdf)\"    (ALT + asdf)");
                Console.WriteLine();

            }

            if (args.ContainsInsensitive("pause"))
            {
                Console.WriteLine("\r\nPress enter to exit...");
                Console.ReadLine();
            }
        }

        static void DoAction()
        {
            _count++;
            Console.WriteLine("\r\n{0}: Detected button press event. Count={1}", DateTime.Now.ToLongTimeString(), _count);
            if (!string.IsNullOrEmpty(_strCmd))
            {
                try
                {
                    Process.Start(_strCmd, _strCmdarGs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            if (!string.IsNullOrEmpty(_strMacro))
            {
                try
                {
                    Console.WriteLine("Sending keys: " + _strMacro);
                    System.Windows.Forms.SendKeys.SendWait(_strMacro);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

        }
    }

    /// <summary>
    /// Extenstions for working with string arrays
    /// </summary>
    public static class StringArrayExtenstions
    {
        public static bool ContainsInsensitive(this string[] args, string name)
        {
            return args.Contains(name, StringComparer.CurrentCultureIgnoreCase);
        }

        public static IEnumerable<string> StartsWith(this string[] args, string value, StringComparison options = StringComparison.CurrentCultureIgnoreCase)
        {
            var q = from a in args
                    where a.StartsWith(value, options)
                    select a;
            return q;
        }
    }
}
