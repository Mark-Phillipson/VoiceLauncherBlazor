using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Options;
using DataAccessLibrary;
using DataAccessLibrary.Repositories;

namespace DataAccessLibrary.Repositories;

public class CursorlessCheatsheetItemJsonRepository : ICursorlessCheatsheetItemJsonRepository
{
   private readonly string _jsonFilePath;
   private readonly IMapper _mapper;
   private readonly ICursorlessCheatsheetItemRepository cursorlessCheatsheetItemRepository;
   private readonly string _contentRootPath;

   public CursorlessCheatsheetItemJsonRepository(IOptions<JsonRepositoryOptions> options, IMapper mapper, ICursorlessCheatsheetItemRepository cursorlessCheatsheetItemRepository)
   {
      _jsonFilePath = options.Value.JsonFilePath;
      _contentRootPath = AppContext.BaseDirectory;
      _mapper = mapper;
      this.cursorlessCheatsheetItemRepository = cursorlessCheatsheetItemRepository;
   }

   public IEnumerable<CursorlessCheatsheetItemDTO> GetAllCursorlessCheatsheetItemsAsync(int maxRows = 400)
   {
      string path = Path.Combine(_contentRootPath, "wwwroot", "CursorlessCheatsheetData.json");
      if (!File.Exists(path))
      {
         return Enumerable.Empty<CursorlessCheatsheetItemDTO>();
      }
      string jsonData = File.ReadAllText(path);
      List<CursorlessCheatsheetItem>? items;
      try
      {
         items = JsonSerializer.Deserialize<List<CursorlessCheatsheetItem>>(jsonData);
      }
      catch (Exception exception)
      {
         Console.WriteLine(exception);
         return Enumerable.Empty<CursorlessCheatsheetItemDTO>();
      }
      var limitedItems = items?.Take(maxRows) ?? Enumerable.Empty<CursorlessCheatsheetItem>();
      return _mapper.Map<IEnumerable<CursorlessCheatsheetItemDTO>>(limitedItems);
   }

   public async Task<IEnumerable<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm)
   {
      if (!File.Exists(_jsonFilePath))
      {
         return Enumerable.Empty<CursorlessCheatsheetItemDTO>();
      }
      var jsonData = await File.ReadAllTextAsync(_jsonFilePath);
      var items = JsonSerializer.Deserialize<List<CursorlessCheatsheetItem>>(jsonData);
      var filteredItems = items?.Where(item => item.SpokenForm.Contains(serverSearchTerm, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<CursorlessCheatsheetItem>();
      return _mapper.Map<IEnumerable<CursorlessCheatsheetItemDTO>>(filteredItems);
   }

   public async Task<bool> ExportToJsonAsync()
   {
      var cursorlessCheatsheetItems = await cursorlessCheatsheetItemRepository.GetAllCursorlessCheatsheetItemsAsync(400);
      string json = JsonSerializer.Serialize(cursorlessCheatsheetItems);
      string path = Path.Combine(_contentRootPath, "wwwroot", "CursorlessCheatsheetData.json");
      try
      {
         await File.WriteAllTextAsync(path, json);
      }
      catch (Exception exception)
      {
         Console.WriteLine(exception);
         return false;
      }
      return true;
   }
}
