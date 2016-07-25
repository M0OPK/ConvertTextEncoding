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
            
            // Set mode to default (we check later if it changed)
            Encoding mode = Encoding.Default;
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
                        case "OUTPUT":
                            // Process specified output encoding, set mode accordingly
                            switch (arg.ToUpper().Trim())
                            {
                                case "UTF8":
                                    mode = Encoding.UTF8;
                                    break;
                                case "UNICODE":
                                    mode = Encoding.Unicode;
                                    break;
                                case "UNICODE_BE":
                                    mode = Encoding.BigEndianUnicode;
                                    break;
                                case "ANSI":
                                    mode = Encoding.ASCII;
                                    break;
                                default:
                                    invalidArg = true;
                                    break;
                            }
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
            if (mode == Encoding.Default || spareArgs.Count != 2)
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
            if (!convFile.ConvertTextFile(inputFilename, outputFilename, replaceMode, mode))
            {
                OutputMessage(string.Format("Error converting file {0}", inputFilename));
            }
        }

        private static void DoUsage(bool helpMode = false)
        {
            // Create messagebox with usage instructions
            string text = "Usage:\r\n";
            text += "ConvertText -output (Unicode/Unicode_BE/Ansi/Utf8) [-replace] <InputFile> <OutputFile>\r\n";
            if (helpMode)
            {
                text += " - Converts the input file to the specified output encoding.\r\n";
                text += " - Input detection is automatic.\r\n";
                text += " - Optionally replace original file with converted file\r\n";
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
