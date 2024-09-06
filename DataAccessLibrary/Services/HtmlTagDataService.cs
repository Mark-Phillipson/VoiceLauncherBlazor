using Ardalis.GuardClauses;
using AutoMapper;
using DataAccessLibrary.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceLauncher.Repositories;


namespace VoiceLauncher.Services
{
    public class HtmlTagDataService : IHtmlTagDataService
    {
        private readonly IHtmlTagRepository _htmlTagRepository;

        public HtmlTagDataService(IHtmlTagRepository htmlTagRepository)
        {
            this._htmlTagRepository = htmlTagRepository;
        }
        public async Task<List<HtmlTagDTO>> GetAllHtmlTagsAsync()
        {
            var HtmlTags = await _htmlTagRepository.GetAllHtmlTagsAsync(300);
            return HtmlTags.ToList();
        }
        public async Task<List<HtmlTagDTO>> SearchHtmlTagsAsync(string serverSearchTerm)
        {
            var HtmlTags = await _htmlTagRepository.SearchHtmlTagsAsync(serverSearchTerm);
            return HtmlTags.ToList();
        }

        public async Task<HtmlTagDTO?> GetHtmlTagById(int Id)
        {
            var htmlTag = await _htmlTagRepository.GetHtmlTagByIdAsync(Id);
            return htmlTag;
        }
        public async Task<HtmlTagDTO?> AddHtmlTag(HtmlTagDTO htmlTagDTO)
        {
            Guard.Against.Null(htmlTagDTO);
            var result = await _htmlTagRepository.AddHtmlTagAsync(htmlTagDTO);
            if (result == null)
            {
                throw new Exception($"Add of htmlTag failed ID: {htmlTagDTO.Id}");
            }
            return result;
        }
        public async Task<HtmlTagDTO?> UpdateHtmlTag(HtmlTagDTO htmlTagDTO, string username)
        {
            Guard.Against.Null(htmlTagDTO);
            Guard.Against.Null(username);
            var result = await _htmlTagRepository.UpdateHtmlTagAsync(htmlTagDTO);
            if (result == null)
            {
                throw new Exception($"Update of htmlTag failed ID: {htmlTagDTO.Id}");
            }
            return result;
        }

        public async Task DeleteHtmlTag(int Id)
        {
            await _htmlTagRepository.DeleteHtmlTagAsync(Id);
        }
    }
}