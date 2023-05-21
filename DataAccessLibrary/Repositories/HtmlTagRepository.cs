
using AutoMapper;
using DataAccessLibrary.DTO;
using Microsoft.EntityFrameworkCore;
using Ardalis.GuardClauses;
using DataAccessLibrary.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VoiceLauncher.Repositories
{
    public class HtmlTagRepository : IHtmlTagRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IMapper _mapper;

        public HtmlTagRepository(IDbContextFactory<ApplicationDbContext> contextFactory,IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }
		        public async Task<IEnumerable<HtmlTagDTO>> GetAllHtmlTagsAsync(int maxRows= 400)
        {
            using var context = _contextFactory.CreateDbContext();
            var HtmlTags= await context.HtmlTags
                //.Where(v => v.?==?)
                //.OrderBy(v => v.?)
                .Take(maxRows)
                .ToListAsync();
            IEnumerable<HtmlTagDTO> HtmlTagsDTO = _mapper.Map<List<HtmlTag>, IEnumerable<HtmlTagDTO>>(HtmlTags);
            return HtmlTagsDTO;
        }
        public async Task<IEnumerable<HtmlTagDTO>> SearchHtmlTagsAsync(string serverSearchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            var HtmlTags= await context.HtmlTags
                //.Where(v => v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //||v.Property!= null  && v.Property.ToLower().Contains(serverSearchTerm.ToLower())
                //)
                //.OrderBy(v => v.?)
                .Take(1000)
                .ToListAsync();
            IEnumerable<HtmlTagDTO> HtmlTagsDTO = _mapper.Map<List<HtmlTag>, IEnumerable<HtmlTagDTO>>(HtmlTags);
            return HtmlTagsDTO;
        }

        public async Task<HtmlTagDTO> GetHtmlTagByIdAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var result =await context.HtmlTags.AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == Id);
            if (result == null) return null;
            HtmlTagDTO htmlTagDTO =_mapper.Map<HtmlTag, HtmlTagDTO>(result);
            return htmlTagDTO;
        }

        public async Task<HtmlTagDTO> AddHtmlTagAsync(HtmlTagDTO htmlTagDTO)
        {
            using var context = _contextFactory.CreateDbContext();
            HtmlTag htmlTag = _mapper.Map<HtmlTagDTO, HtmlTag>(htmlTagDTO);
            var addedEntity = context.HtmlTags.Add(htmlTag);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            HtmlTagDTO resultDTO =_mapper.Map<HtmlTag, HtmlTagDTO>(htmlTag);
            return resultDTO;
        }

        public async Task<HtmlTagDTO> UpdateHtmlTagAsync(HtmlTagDTO htmlTagDTO)
        {
            HtmlTag htmlTag=_mapper.Map<HtmlTagDTO, HtmlTag>(htmlTagDTO);
            using (var context = _contextFactory.CreateDbContext())
            {
                var foundHtmlTag = await context.HtmlTags.AsNoTracking().FirstOrDefaultAsync(e => e.Id == htmlTag.Id);

                if (foundHtmlTag != null)
                {
                    var mappedHtmlTag = _mapper.Map<HtmlTag>(htmlTag);
                    context.HtmlTags.Update(mappedHtmlTag);
                    await context.SaveChangesAsync();
                    HtmlTagDTO resultDTO = _mapper.Map<HtmlTag, HtmlTagDTO>(mappedHtmlTag);
                    return resultDTO;
                }
            }
            return null;
        }
        public async Task DeleteHtmlTagAsync(int Id)
        {
            using var context = _contextFactory.CreateDbContext();
            var foundHtmlTag = context.HtmlTags.FirstOrDefault(e => e.Id == Id);
            if (foundHtmlTag == null)
            {
                return;
            }
            context.HtmlTags.Remove(foundHtmlTag);
            await context.SaveChangesAsync();
        }
    }
}