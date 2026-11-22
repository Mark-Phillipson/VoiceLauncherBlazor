using DataAccessLibrary.DTO;

namespace RazorClassLibrary.Pages
{
   // Tooltip helpers for the copy button on the /customintellisenses page.
   public partial class CustomIntelliSenseTable
   {
      private const int TooltipPreviewMaximumLength = 220;
      private const string MaskedValue = "********";

      private static bool IsSensitiveSnippet(CustomIntelliSenseDTO snippet, bool categoryIsSensitive)
      {
         return categoryIsSensitive
             || snippet.Sensitive
             || snippet.CategoryName?.Contains("password", StringComparison.OrdinalIgnoreCase) == true
             || snippet.DisplayValue?.Contains("password", StringComparison.OrdinalIgnoreCase) == true;
      }

      public static string GetSnippetPreview(CustomIntelliSenseDTO snippet, bool categoryIsSensitive = false, int maximumLength = TooltipPreviewMaximumLength)
      {
         if (snippet == null) return string.Empty;

         if (IsSensitiveSnippet(snippet, categoryIsSensitive)) return MaskedValue;

         var value = snippet.SendKeysValue?.Trim();
         if (string.IsNullOrEmpty(value)) return "(No snippet content)";

         return value.Length > maximumLength
             ? value[..maximumLength].TrimEnd() + "…"
             : value;
      }

      public static string GetSnippetTooltip(CustomIntelliSenseDTO snippet, bool categoryIsSensitive = false)
      {
         if (snippet == null) return string.Empty;

         return string.Join("\n",
             $"Language: {(string.IsNullOrWhiteSpace(snippet.LanguageName) ? "Not specified" : snippet.LanguageName)}",
             $"Category: {(string.IsNullOrWhiteSpace(snippet.CategoryName) ? "Not specified" : snippet.CategoryName)}",
             $"Display Value: {(string.IsNullOrWhiteSpace(snippet.DisplayValue) ? "Not specified" : snippet.DisplayValue)}",
             $"Snippet: {GetSnippetPreview(snippet, categoryIsSensitive)}");
      }
   }
}