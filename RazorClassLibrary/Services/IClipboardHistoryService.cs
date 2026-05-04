using RazorClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorClassLibrary.Services
{
    public interface IClipboardHistoryService
    {
        Task<(IEnumerable<ClipboardEntry> Items, int TotalCount)> QueryAsync(int page, int pageSize, string? search, DateTime? from, DateTime? to);
        Task<ClipboardEntry?> GetAsync(long id);
        Task UpdateAsync(ClipboardEntry entry);
        Task DeleteAsync(long id);
        Task DeleteBulkAsync(IEnumerable<long> ids);
        Task<IEnumerable<ClipboardEntry>> ExportAsync(IEnumerable<long> ids);
    }
}
