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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using DataAccessLibrary;
using DataAccessLibrary.Repositories;

namespace VoiceLauncher;

public class CursorlessCheatsheetItemJsonRepository : ICursorlessCheatsheetItemJsonRepository
{
   private readonly string _jsonFilePath;
   private readonly IMapper _mapper;
   private readonly ICursorlessCheatsheetItemRepository cursorlessCheatsheetItemRepository;
   private readonly IWebHostEnvironment _webHostEnvironment;

   public CursorlessCheatsheetItemJsonRepository(IOptions<JsonRepositoryOptions> options, IWebHostEnvironment webHostEnvironment, IMapper mapper, ICursorlessCheatsheetItemRepository cursorlessCheatsheetItemRepository)
   {
      _jsonFilePath = options.Value.JsonFilePath;
      _webHostEnvironment = webHostEnvironment;
      _mapper = mapper;
      this.cursorlessCheatsheetItemRepository = cursorlessCheatsheetItemRepository;
   }

   public IEnumerable<CursorlessCheatsheetItemDTO> GetAllCursorlessCheatsheetItemsAsync(int maxRows = 400)
   {
      string path = Path.Combine(_webHostEnvironment.WebRootPath, "CursorlessCheatsheetData.json");
      string jsonData = File.ReadAllText(path);
      List<CursorlessCheatsheetItem>? items;
      try
      {
         items = JsonSerializer.Deserialize<List<CursorlessCheatsheetItem>>(jsonData);
      }
      catch (System.Exception exception)
      {
         System.Console.WriteLine(exception);
         return new List<CursorlessCheatsheetItemDTO>();
      }
      var limitedItems = items?.Take(maxRows) ?? Enumerable.Empty<CursorlessCheatsheetItem>();
      return _mapper.Map<IEnumerable<CursorlessCheatsheetItemDTO>>(limitedItems);
   }

   public async Task<IEnumerable<CursorlessCheatsheetItemDTO>> SearchCursorlessCheatsheetItemsAsync(string serverSearchTerm)
   {
      var jsonData = await File.ReadAllTextAsync(_jsonFilePath);
      var items = JsonSerializer.Deserialize<List<CursorlessCheatsheetItem>>(jsonData);
      var filteredItems = items?.Where(item => item.SpokenForm.Contains(serverSearchTerm, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<CursorlessCheatsheetItem>();
      return _mapper.Map<IEnumerable<CursorlessCheatsheetItemDTO>>(filteredItems);
   }
   public async Task<bool> ExportToJsonAsync()
   {
      var cursorlessCheatsheetItems = await cursorlessCheatsheetItemRepository.GetAllCursorlessCheatsheetItemsAsync(400);
      string json = JsonSerializer.Serialize(cursorlessCheatsheetItems);
      string path = Path.Combine(_webHostEnvironment.WebRootPath, "CursorlessCheatsheetData.json");
      try
      {
         await File.WriteAllTextAsync(path, json);
      }
      catch (System.Exception exception)
      {
         System.Console.WriteLine(exception);
         return false;
      }
      return true;
   }
}
