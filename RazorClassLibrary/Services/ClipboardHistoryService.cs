using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazorClassLibrary.Models;
using DataAccessLibrary;
using Microsoft.Extensions.Configuration;

namespace RazorClassLibrary.Services
{
    public class ClipboardHistoryService : IClipboardHistoryService
    {
        private readonly ISqlDataAccess _db;
        private readonly IConfiguration _config;
        private readonly string TableName = "ClipboardHistory";
        private bool _schemaInitialized = false;
        private HashSet<string> _columnsFound = new(StringComparer.OrdinalIgnoreCase);
        private string _idCol = "Id";
        private string _contentCol = "Content";
        private string _dateCol = "CreatedAt";
        private string _typeCol = "Type";

        public ClipboardHistoryService(ISqlDataAccess db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        private async Task EnsureSchemaAsync()
        {
            if (_schemaInitialized) return;
            try
            {
                // Read column names via PRAGMA
                var rows = await _db.LoadData<dynamic, object>($"PRAGMA table_info('{TableName}');", new { });
                foreach (var r in rows)
                {
                    try
                    {
                        // Dapper dynamic: access 'name'
                        string? name = null;
                        if (r is IDictionary<string, object> dict && dict.TryGetValue("name", out var n1)) name = n1?.ToString();
                        else
                        {
                            // try property access
                            try { name = r.name; } catch { }
                        }
                        if (!string.IsNullOrWhiteSpace(name)) _columnsFound.Add(name!);
                    }
                    catch
                    {
                        // ignore per-row errors
                    }
                }

                string Choose(params string[] candidates)
                {
                    foreach (var c in candidates)
                    {
                        if (_columnsFound.Contains(c)) return c;
                    }
                    return string.Empty;
                }

                var cId = Choose("Id", "id", "rowid");
                if (!string.IsNullOrEmpty(cId)) _idCol = cId;

                var cContent = Choose("Content", "content", "Text", "text", "Value", "value", "Data", "data");
                if (!string.IsNullOrEmpty(cContent)) _contentCol = cContent;

                var cDate = Choose("CreatedAt", "created_at", "createdat", "created", "timestamp", "time", "ts", "date", "createdOn", "created_on", "added_at", "added");
                if (!string.IsNullOrEmpty(cDate)) _dateCol = cDate;

                var cType = Choose("Type", "type", "Kind", "kind", "Format", "format");
                if (!string.IsNullOrEmpty(cType)) _typeCol = cType;
            }
            catch
            {
                // ignore errors, keep defaults
            }
            finally
            {
                _schemaInitialized = true;
            }
        }

        public async Task<(IEnumerable<ClipboardEntry> Items, int TotalCount)> QueryAsync(int page, int pageSize, string? search, DateTime? from, DateTime? to)
        {
            await EnsureSchemaAsync();

            var whereClauses = new List<string>();
            var parameters = new Dapper.DynamicParameters();
            if (!string.IsNullOrWhiteSpace(search) && !string.IsNullOrEmpty(_contentCol))
            {
                whereClauses.Add($"\"{_contentCol}\" LIKE @search");
                parameters.Add("search", $"%{search}%");
            }
            if (from.HasValue && !string.IsNullOrEmpty(_dateCol) && _columnsFound.Contains(_dateCol))
            {
                whereClauses.Add($"\"{_dateCol}\" >= @from");
                parameters.Add("from", from.Value);
            }
            if (to.HasValue && !string.IsNullOrEmpty(_dateCol) && _columnsFound.Contains(_dateCol))
            {
                whereClauses.Add($"\"{_dateCol}\" <= @to");
                parameters.Add("to", to.Value);
            }

            // Exclude known browser-extension generated clipboard items (JSON action/request payloads)
            // These typically contain keys like "version", "type":"request", or specific action names like "directClickElement".
            if (!string.IsNullOrEmpty(_contentCol) && _columnsFound.Contains(_contentCol))
            {
                whereClauses.Add($"NOT (\"{_contentCol}\" LIKE @excludeJsonA OR \"{_contentCol}\" LIKE @excludeJsonB OR \"{_contentCol}\" LIKE @excludeJsonC)");
                parameters.Add("excludeJsonA", @"%{""version"":%");
                parameters.Add("excludeJsonB", @"%""type"":""request""%");
                parameters.Add("excludeJsonC", @"%""directClickElement""%");
            }

            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            var countSql = $"SELECT COUNT(1) FROM \"{TableName}\" {where}";
            var counts = await _db.LoadData<int, object>(countSql, parameters);
            var total = counts.FirstOrDefault();

            var offset = Math.Max(0, (page - 1) * pageSize);

            var selectList = new List<string>();
            selectList.Add(!string.IsNullOrEmpty(_idCol) ? $"\"{_idCol}\" AS Id" : "Id");
            selectList.Add(!string.IsNullOrEmpty(_contentCol) ? $"\"{_contentCol}\" AS Content" : "Content");
            if (!string.IsNullOrEmpty(_dateCol) && _columnsFound.Contains(_dateCol))
                selectList.Add($"\"{_dateCol}\" AS CreatedAt");
            else
                selectList.Add("NULL AS CreatedAt");
            selectList.Add(!string.IsNullOrEmpty(_typeCol) ? $"\"{_typeCol}\" AS Type" : "NULL AS Type");

            var orderBy = (!string.IsNullOrEmpty(_dateCol) && _columnsFound.Contains(_dateCol)) ? $"\"{_dateCol}\" DESC" : $"\"{_idCol}\" DESC";

            var sql = $"SELECT {string.Join(", ", selectList)} FROM \"{TableName}\" {where} ORDER BY {orderBy} LIMIT @pageSize OFFSET @offset";
            parameters.Add("pageSize", pageSize);
            parameters.Add("offset", offset);

            var items = await _db.LoadData<ClipboardEntry, object>(sql, parameters);

            return (items, total);
        }

        public async Task<ClipboardEntry?> GetAsync(long id)
        {
            try
            {
                await EnsureSchemaAsync();
                var sql = $"SELECT \"{_idCol}\" AS Id, \"{_contentCol}\" AS Content, \"{_dateCol}\" AS CreatedAt, \"{_typeCol}\" AS Type FROM \"{TableName}\" WHERE \"{_idCol}\" = @id";
                var item = await _db.LoadSingleData<ClipboardEntry, object>(sql, new { id });
                return item;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateAsync(ClipboardEntry entry)
        {
            await EnsureSchemaAsync();
            var sql = $"UPDATE \"{TableName}\" SET \"{_contentCol}\" = @Content WHERE \"{_idCol}\" = @Id";
            await _db.SaveData(sql, new { entry.Content, entry.Id });
        }

        public async Task DeleteAsync(long id)
        {
            await EnsureSchemaAsync();
            var sql = $"DELETE FROM \"{TableName}\" WHERE \"{_idCol}\" = @id";
            await _db.SaveData(sql, new { id });
        }

        public async Task DeleteBulkAsync(IEnumerable<long> ids)
        {
            var arr = ids.ToArray();
            await EnsureSchemaAsync();
            var sql = $"DELETE FROM \"{TableName}\" WHERE \"{_idCol}\" IN @ids";
            await _db.SaveData(sql, new { ids = arr });
        }

        public async Task<IEnumerable<ClipboardEntry>> ExportAsync(IEnumerable<long> ids)
        {
            var arr = ids.ToArray();
            await EnsureSchemaAsync();
            var sql = $"SELECT \"{_idCol}\" AS Id, \"{_contentCol}\" AS Content, \"{_dateCol}\" AS CreatedAt, \"{_typeCol}\" AS Type FROM \"{TableName}\" WHERE \"{_idCol}\" IN @ids ORDER BY \"{_dateCol}\" DESC";
            var items = await _db.LoadData<ClipboardEntry, object>(sql, new { ids = arr });
            return items;
        }
    }
}
