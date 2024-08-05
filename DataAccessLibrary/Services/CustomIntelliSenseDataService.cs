using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using DataAccessLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;


namespace VoiceLauncher.Services
{
    public class CustomIntelliSenseDataService : ICustomIntelliSenseDataService
    {
        private readonly ICustomIntelliSenseRepository _customIntelliSenseRepository;

        public CustomIntelliSenseDataService(ICustomIntelliSenseRepository customIntelliSenseRepository)
        {
            _customIntelliSenseRepository = customIntelliSenseRepository;
        }
        public async Task<List<CustomIntelliSenseDTO>> GetAllCustomIntelliSensesAsync(int LanguageId, int CategoryId, int pageNumber, int pageSize)
        {
            var CustomIntelliSenses = await _customIntelliSenseRepository.GetAllCustomIntelliSensesAsync(LanguageId, CategoryId, pageNumber, pageSize);
            return CustomIntelliSenses.ToList();
        }
        public async Task<List<CustomIntelliSenseDTO>> SearchCustomIntelliSensesAsync(string serverSearchTerm)
        {
            var CustomIntelliSenses = await _customIntelliSenseRepository.SearchCustomIntelliSensesAsync(serverSearchTerm);
            return CustomIntelliSenses.ToList();
        }

        public async Task<CustomIntelliSenseDTO> GetCustomIntelliSenseById(int Id)
        {
            var customIntelliSense = await _customIntelliSenseRepository.GetCustomIntelliSenseByIdAsync(Id);
            return customIntelliSense;
        }
        public async Task<CustomIntelliSenseDTO> AddCustomIntelliSense(CustomIntelliSenseDTO customIntelliSenseDTO)
        {
            Guard.Against.Null(customIntelliSenseDTO);
            var result = await _customIntelliSenseRepository.AddCustomIntelliSenseAsync(customIntelliSenseDTO);
            if (result == null)
            {
                throw new Exception($"Add of customIntelliSense failed ID: {customIntelliSenseDTO.Id}");
            }
            return result;
        }
        public async Task<CustomIntelliSenseDTO> UpdateCustomIntelliSense(CustomIntelliSenseDTO customIntelliSenseDTO, string username)
        {
            Guard.Against.Null(customIntelliSenseDTO);
            Guard.Against.Null(username);
            var result = await _customIntelliSenseRepository.UpdateCustomIntelliSenseAsync(customIntelliSenseDTO);
            if (result == null)
            {
                throw new Exception($"Update of customIntelliSense failed ID: {customIntelliSenseDTO.Id}");
            }
            return result;
        }

        public async Task DeleteCustomIntelliSense(int Id)
        {
            await _customIntelliSenseRepository.DeleteCustomIntelliSenseAsync(Id);
        }
        public void SendSnippet(string itemToCopyAndPaste, CustomIntelliSenseDTO CustomIntelliSenseDTO)
        {
            InputSimulator simulator = new InputSimulator();
            simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.TAB);
            simulator.Keyboard.Sleep(100);
            simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            simulator.Keyboard.Sleep(300);
            simulator.Keyboard.TextEntry(itemToCopyAndPaste);
            if (CustomIntelliSenseDTO.SelectWordFromRight > 1)
            {
                simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                for (int i = 0; i < CustomIntelliSenseDTO.SelectWordFromRight; i++)
                {
                    simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                }
                simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            }
            if (CustomIntelliSenseDTO.SelectWordFromRight > 0)
            {
                simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                simulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                simulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
                simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            }
            if (CustomIntelliSenseDTO.MoveCharactersLeft > 0)
            {
                for (int i = 0; i < CustomIntelliSenseDTO.MoveCharactersLeft; i++)
                {
                    simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                }
            }
            if (CustomIntelliSenseDTO.SelectCharactersLeft > 0)
            {
                simulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
                for (int i = 0; i < CustomIntelliSenseDTO.SelectCharactersLeft; i++)
                {
                    simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                }
                simulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            }
        }
    }
}