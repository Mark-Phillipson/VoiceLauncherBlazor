using RazorClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorClassLibrary.Services
{
    public interface IClipboardHistoryService
    {
        Task<(IEnumerable<ClipboardEntry> Items, int TotalCount)> QueryAsync(int page, int pageSize, string? search, DateTime? from, DateTime? to, ClipboardHistorySortOrder sortOrder = ClipboardHistorySortOrder.NewestFirst);
        Task<ClipboardEntry?> GetAsync(long id);
        Task UpdateAsync(ClipboardEntry entry);
        Task SetPinnedAsync(long id, bool isPinned);
        Task DeleteAsync(long id);
        Task DeleteBulkAsync(IEnumerable<long> ids);
        Task<IEnumerable<ClipboardEntry>> ExportAsync(IEnumerable<long> ids, ClipboardHistorySortOrder sortOrder = ClipboardHistorySortOrder.NewestFirst);
    }
}
