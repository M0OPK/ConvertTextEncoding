// Define or undefine console application (default console app, also change built target)
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ConvertTextEncoding;

namespace ConvertFile
{
    class ConvertTextEncoding
    {
        static void Main(string[] args)
        {

            // Set modes to default (we check later if it changed)
            Encoding inputMode = Encoding.Default;
            Encoding outputMode = Encoding.Default;
            List<string> spareArgs = new List<string>();

            // Parse arguments
            bool argMode = false;
            bool invalidArg = false;
            bool replaceMode = false;
            string argString = "";
            foreach (string arg in args)
            {
                // If we were waiting for an argument parameter, process it now
                if (argMode)
                {
                    // Switch on previous argument name
                    switch (argString.ToUpper().Trim())
                    {
                        case "INPUT":
                            // Process specified output encoding, set mode accordingly
                            switch (arg.ToUpper().Trim())
                            {
                                case "UTF8":
                                    inputMode = Encoding.UTF8;
                                    break;
                                case "UNICODE":
                                    inputMode = Encoding.Unicode;
                                    break;
                                case "UNICODE_BE":
                                    inputMode = Encoding.BigEndianUnicode;
                                    break;
                                case "ANSI":
                                    inputMode = Encoding.ASCII;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "OUTPUT":
                            // Process specified output encoding, set mode accordingly
                            switch (arg.ToUpper().Trim())
                            {
                                case "UTF8":
                                    outputMode = Encoding.UTF8;
                                    break;
                                case "UNICODE":
                                    outputMode = Encoding.Unicode;
                                    break;
                                case "UNICODE_BE":
                                    outputMode = Encoding.BigEndianUnicode;
                                    break;
                                case "ANSI":
                                    outputMode = Encoding.ASCII;
                                    break;
                                default:
                                    invalidArg = true;
                                    break;
                            }
                            break;
                        case "INPUTCP":
                            if (inputMode != Encoding.ASCII)
                            {
                                OutputMessage("Error: Input codepage specified, but input mode not ANSI");
                                invalidArg = true;
                                break;
                            }
                            int inputCodepage;
                            if (!int.TryParse(arg, out inputCodepage))
                            {
                                OutputMessage(string.Format("Invalid Input codepage {0}, non numeric. See https://msdn.microsoft.com/en-us/library/windows/desktop/dd317756(v=vs.85).aspx for valid values", arg));
                                invalidArg = true;
                                break;
                            }
                            try
                            {
                                inputMode = Encoding.GetEncoding(inputCodepage);
                            }
                            catch
                            {
                                OutputMessage(string.Format("Invalid Input codepage {0}. See https://msdn.microsoft.com/en-us/library/windows/desktop/dd317756(v=vs.85).aspx for valid values", arg));
                                invalidArg = true;
                                break;
                            }
                            break;
                        case "OUTPUTCP":
                            if (outputMode != Encoding.ASCII)
                            {
                                OutputMessage("Error: Output codepage specified, but output mode not ANSI");
                                invalidArg = true;
                                break;
                            }
                            int outputCodepage;
                            if (!int.TryParse(arg, out outputCodepage))
                            {
                                OutputMessage(string.Format("Invalid Output codepage {0}, non numeric. See https://msdn.microsoft.com/en-us/library/windows/desktop/dd317756(v=vs.85).aspx for valid values", arg));
                                invalidArg = true;
                                break;
                            }
                            try
                            {
                                outputMode = Encoding.GetEncoding(outputCodepage);
                            }
                            catch
                            {
                                OutputMessage(string.Format("Invalid Output codepage {0}. See https://msdn.microsoft.com/en-us/library/windows/desktop/dd317756(v=vs.85).aspx for valid values", arg));
                                invalidArg = true;
                                break;
                            }
                            break;
                        case "INPUTLAN":
                            if (inputMode != Encoding.ASCII)
                            {
                                OutputMessage("Error: Input language specified, but input mode not ANSI");
                                invalidArg = true;
                                break;
                            }
                            int inputLanCodepage = LanToCodePage(arg);
                            if (inputLanCodepage == 0)
                            {
                                OutputMessage(string.Format("Invalid Input language {0}", arg));
                                invalidArg = true;
                                break;
                            }
                            inputMode = Encoding.GetEncoding(inputLanCodepage);
                            break;
                        case "OUTPUTLAN":
                            if (outputMode != Encoding.ASCII)
                            {
                                OutputMessage("Error: Output language specified, but input mode not ANSI");
                                invalidArg = true;
                                break;
                            }
                            int outputLanCodepage = LanToCodePage(arg);
                            if (outputLanCodepage == 0)
                            {
                                OutputMessage(string.Format("Invalid Output language {0}", arg));
                                invalidArg = true;
                                break;
                            }
                            outputMode = Encoding.GetEncoding(outputLanCodepage);
                            break;
                        default:
                            break;
                    }
                    argMode = false;
                }
                else if (arg.Substring(0, 1) == "-")
                {
                    // If argument begins with "-" process it as an argument
                    argString = arg.Substring(1, arg.Length - 1);

                    // Get argument text, and test against list
                    switch (argString.ToUpper().Trim())
                    {
                        case "OUTPUT":
                        case "INPUT":
                        case "INPUTCP":
                        case "OUTPUTCP":
                        case "INPUTLAN":
                        case "OUTPUTLAN":
                            // We'll get the output mode from the next argument
                            argMode = true;
                            break;
                        case "REPLACE":
                            // Set replace mode to true (replace original file)
                            replaceMode = true;
                            argMode = false;
                            break;
                        case "HELP":
                            // Show help info
                            argMode = false;
                            DoUsage(true);
                            Environment.Exit(0);
                            break;
                        default:
                            invalidArg = true;
                            break;
                    }
                }
                else
                {
                    // Anything else should be a mandatory argument (input file, output file)
                    spareArgs.Add(arg);
                }
            }

            // If replace mode, we expect one argument, so create a work filename to work into
            if (replaceMode && spareArgs.Count == 1)
                spareArgs.Add(spareArgs[0] + ".convertwork");

            // If no encode mode was set, or we don't have both input and output file, arguments are invalid
            if (outputMode == Encoding.Default || spareArgs.Count != 2)
                invalidArg = true;

            // Handle invalid arguments, by showing window with usage
            if (invalidArg)
            {
                DoUsage(true);
                Environment.Exit(1);
            }

            // Get input/output files from passed arguments
            string inputFilename = spareArgs[0];
            string outputFilename = spareArgs[1];

            // Check input file exists
            if (!File.Exists(inputFilename))
            {
                OutputMessage(String.Format("Input file {0} not found", inputFilename));
                Environment.Exit(2);
            }

            // Perform conversion and report failure if not successful
            ConvertTextFileEncoding convFile = new ConvertTextFileEncoding();
            if (!convFile.ConvertTextFile(inputFilename, outputFilename, replaceMode, outputMode, inputMode))
            {
                OutputMessage(string.Format("Error converting file {0}", inputFilename));
            }
        }

        private static int LanToCodePage(string language)
        {
            int codePage;
            switch (language.ToUpper().Trim())
            {
                case "THA":
                    codePage = 874;
                    break;
                case "JPN":
                    codePage = 932;
                    break;
                case "CHA":
                    codePage = 936;
                    break;
                case "KOR":
                    codePage = 949;
                    break;
                case "CHT":
                    codePage = 950;
                    break;
                case "CRO":
                case "CZL":
                case "HUN":
                case "POL":
                case "ROM":
                case "SKL":
                case "SLO":
                    codePage = 1250;
                    break;
                case "BUL":
                case "KAZ":
                case "RUS":
                    codePage = 1251;
                    break;
                case "ANZ":
                case "BRS":
                case "DAN":
                case "NED":
                case "FIN":
                case "FRA":
                case "GER":
                case "ITA":
                case "NOR":
                case "POR":
                case "ESP":
                case "SWE":
                case "ENG":
                case "AME":
                    codePage = 1252;
                    break;
                case "GRE":
                    codePage = 1253;
                    break;
                case "TUR":
                    codePage = 1254;
                    break;
                case "ARB":
                    codePage = 1256;
                    break;
                case "EST":
                case "LAT":
                case "LIT":
                    codePage = 1257;
                    break;
                default:
                    codePage = 0;
                    break;
            }
            return codePage;
        }

        private static void DoUsage(bool helpMode = false)
        {
            // Create messagebox with usage instructions
            string text = "Usage:\r\n";
            text += "ConvertText -input (Unicode/Unicode_BE/Ansi/Utf8) -output (Unicode/Unicode_BE/Ansi/Utf8) -inputcp (codepage) -outputcp (codepage) -inputlan (lancode e.g. ENG) -outputlan (lancode) [-replace] <InputFile> <OutputFile>\r\n";
            if (helpMode)
            {
                text += " - Converts the input file to the specified output encoding.\r\n";
                text += " - Input detection is automatic, but can be specified.\r\n";
                text += " - Optionally replace original file with converted file\r\n";
                text += " - For ANSI input or output, codepage or language code can be optionally specified\r\n";
                text += "Example: ConvertFile -output Unicode testIn.txt testOut.txt\r\n";
            }
            OutputMessage(text);
        }

        // Output to console or messagebox depending on build mode
        private static void OutputMessage(string message)
        {
#if WINAPP
            MessageBox.Show(message, "ConvertTextEncoding");
#else
            Console.WriteLine(message);
#endif
        }
    }
}
