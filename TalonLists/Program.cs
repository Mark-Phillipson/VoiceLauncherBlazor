using System;// See https://aka.ms/new-console-template for more information
using System.IO;
// Note this will not work for Captures!


string directoryPath = @"C:\Users\MPhil\AppData\Roaming\talon\user\community"; // Replace with the actual path to your Talon user directory
string outputFilePath = @"C:\Users\MPhil\AppData\Roaming\talon\user\mystuff\talon_my_stuff\TalonLists.txt"; // Replace with the desired output file path
WriteTalonListsToFile(directoryPath, outputFilePath);


static void WriteTalonListsToFile(string directory, string outputFile)
{
    using (StreamWriter writer = new StreamWriter(outputFile))
    {
        writer.WriteLine($"All Talon Lists In all directories below {directory} created: {DateTime.Now}");
        foreach (string filePath in Directory.GetFiles(directory, "*.talon-list", SearchOption.AllDirectories))
        {
            string[] lines = File.ReadAllLines(filePath);
            string listName = lines[0].Split(new[] { ": " }, StringSplitOptions.None)[1];
            writer.WriteLine($"List: {listName}");
            for (int i = 2; i < lines.Length; i++)
            {
                writer.WriteLine($"  - {lines[i]}");
            }
            writer.WriteLine();
        }
        writer.WriteLine("End of Talon Lists");
    }
}