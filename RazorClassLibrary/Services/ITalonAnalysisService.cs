using RazorClassLibrary.Models;
using System.Threading.Tasks;

namespace RazorClassLibrary.Services
{
    public interface ITalonAnalysisService
    {
        Task<TalonAnalysisResult> AnalyzeCommandsAsync();
    }
}
