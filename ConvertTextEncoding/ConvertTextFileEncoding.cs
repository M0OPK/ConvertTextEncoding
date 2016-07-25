using System.Text;
using System.IO;

namespace ConvertTextEncoding
{
    class ConvertTextFileEncoding
    {
        // Convert text file, from text encoding to text encoding
        public bool ConvertTextFile(string inputFilename, string outputFilename, bool replaceMode = false, 
                                    Encoding outputMode = null, Encoding inputMode = null)
        {
            FileStream inputFile;
            FileStream outputFile;
            try
            {
                // Open files and create streams
                inputFile = File.Open(inputFilename, FileMode.Open);
                outputFile = File.Open(outputFilename, FileMode.Create);
            }
            catch
            {
                return false;
            }

            StreamReader inputStream;
            StreamWriter outputStream;
            try
            {
                // Create streamreaders from file streams
                if (inputMode != null)
                    inputStream = new StreamReader(inputFile, inputMode);
                else
                    inputStream = new StreamReader(inputFile);

                if (outputMode != null)
                    outputStream = new StreamWriter(outputFile, outputMode);
                else
                    outputStream = new StreamWriter(outputFile);
            }
            catch
            {
                return false;
            }

            string inputLine = string.Empty;

            try
            {
                // Read all lines and write them to destination file. Since we set encoding, it will automatically be converted
                while ((inputLine = inputStream.ReadLine()) != null)
                {
                    outputStream.WriteLine(inputLine);
                }
            }
            catch
            {
                return false;
            }

            try
            {
                // Close streams and files
                inputStream.Close();
                outputStream.Close();
                inputFile.Close();
                outputFile.Close();
            }
            catch
            {
                return false;
            }

            // If we're replacing, delete original, and move new file to original filename
            try
            {
                if (replaceMode)
                {
                    File.Delete(inputFilename);
                    File.Move(outputFilename, inputFilename);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
