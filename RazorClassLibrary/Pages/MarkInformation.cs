//using OpenAI.Chat;
using Microsoft.SemanticKernel;

public class MarkInformation
{
    [KernelFunction]
    public string GetInformationAboutMark(string dataCategory)
    {
        if (dataCategory == "age")
        {
            return "Mark Was Born on the a day in August 1964 so he is currently sixty years old";
        }
        else if (dataCategory == "location")
        {
            return "Mark's location is Maidstone Kent";
        }
        else if (dataCategory == "job")
        {
            return "Mark's is a .NET software developer freelancing on Upwork";
        }
        else if (dataCategory == "hobbies")
        {
            return "Mark's hobbies include riding the bicycle, reading and programming in C#";
        }
        else if (dataCategory == "surname")
        {
            return "Mark has a last name of Phillipson";
        }
        else
        {
            return "I'm sorry, I don't know that information about Mark.";
        }
    }

}