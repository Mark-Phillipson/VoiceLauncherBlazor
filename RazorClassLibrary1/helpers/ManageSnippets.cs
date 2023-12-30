using DataAccessLibrary.DTO;
using System.Diagnostics;

namespace RazorClassLibrary.helpers
{

	public static class ManageSnippets
	{
		// Create a method that takes an XML snippet and edits parts if and returns a string of the XML snippet
		public static  string  CreateSnippet(CustomIntelliSenseDTO customIntelliSense)
		{
			string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CodeSnippets xmlns=\"http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet\">\r\n    <CodeSnippet Format=\"1.0.0\">\r\n        <Header>" +
				"      <SnippetTypes>\r\n        <SnippetType>Expansion</SnippetType>\r\n      </SnippetTypes>\r\n" +
				"\r\n            <Title>Title</Title>\r\n            <Author>Mark Phillipson</Author>\r\n            <Description>Description</Description>\r\n            <Shortcut>shortcut</Shortcut>\r\n        </Header>\r\n        <Snippet>\r\n            <Code Language=\"CSharp\">\r\n\t\t\t\t<![CDATA[codeValue]]>\r\n\t\t\t</Code>\r\n            <Declarations>\r\n                <Literal>\r\n                    <ID>Variable1</ID>\r\n                    <Default>Default1</Default>\r\n                </Literal>\r\n                <Literal>\r\n                    <ID>Variable2</ID>\r\n                    <Default>Default2</Default>\r\n                </Literal>\r\n                <Literal>\r\n                    <ID>Variable3</ID>\r\n                    <Default>Default3</Default>\r\n                </Literal>\r\n            </Declarations>\r\n        </Snippet>\r\n    </CodeSnippet>\r\n</CodeSnippets>";

			template = template.Replace("<Title>Title</Title>", $"<Title>{customIntelliSense.DisplayValue}</Title>");
			template = template.Replace("<Author>Mark Phillipson</Author>", $"<Author>{"Mark Phillipson"}</Author>");
			template = template.Replace("<Description>Description</Description>", $"<Description>{customIntelliSense.Remarks}</Description>");
			string shortcut = customIntelliSense.DisplayValue.Replace(" ", "");
			template = template.Replace("<Shortcut>shortcut</Shortcut>", $"<Shortcut>{shortcut}</Shortcut>");
			template = template.Replace("<Default>Default1</Default>", $"<Default>{customIntelliSense.Variable1}</Default>");
			template = template.Replace("<Default>Default2</Default>", $"<Default>{customIntelliSense.Variable2}</Default>");
			template = template.Replace("<Default>Default3</Default>", $"<Default>{customIntelliSense.Variable3}</Default>");
			string codeValue = customIntelliSense.SendKeysValue;
			codeValue = codeValue.Replace("`Variable1`", "$Variable1$");
			codeValue = codeValue.Replace("`Variable2`", "$Variable2$");
			codeValue = codeValue.Replace("`Variable3`", "$Variable3$");
			template = template.Replace("<![CDATA[codeValue]]>", $"<![CDATA[{codeValue.Trim()}]]>");

			string filename = @$"C:\Users\MPhil\OneDrive\Documents\CustomSnippets\CSharp\{customIntelliSense.DisplayValue}.snippet".Replace(" ","");
			try
			{
				File.WriteAllText(filename, template);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return $"An error has occurred unexpectedly: {ex.Message}";
			}
			Process.Start("explorer.exe",filename);
			return  $"Success snippet created at: {filename}";

		}
	}
}
