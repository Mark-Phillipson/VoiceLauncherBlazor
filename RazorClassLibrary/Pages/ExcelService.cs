using DataAccessLibrary.DTOs;
using System.Text;
using System.IO;
namespace RazorClassLibrary.Pages
{

    public class ExcelService
    {
        private string Message = "";

        public ExcelService()
        {
        }

        public string ExportTransactionsToExcel(List<TransactionDTO> data, string fileName)
        {
            // Export out as a CSV file
            var builder = new StringBuilder();
            builder.AppendLine("Date,Type,Description,MoneyIn,MoneyOut,MyTransactionType");
            foreach (var transaction in data)
            {
                builder.AppendLine($"{transaction.Date},{transaction.Type},{transaction.Description},{transaction.MoneyIn},{transaction.MoneyOut},{transaction.MyTransactionType}");
            }

            var fileContent = builder.ToString();
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);

            try
            {
                File.WriteAllText(downloadsPath, fileContent);
            }
            catch (System.Exception exception)
            {
                System.Console.WriteLine(exception.Message);
                Message = $"Error saving file to {downloadsPath} folder {exception.Message}";
                return Message;
            }
            Message = $"{downloadsPath} File has been saved to Downloads folder";
            return Message;
        }
    }
}