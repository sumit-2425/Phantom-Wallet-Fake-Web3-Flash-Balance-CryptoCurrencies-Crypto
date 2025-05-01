using AutoMapper;
using Backend.DTO;
using Backend.Interface;
using Backend.Models;
using Backend.Parameters;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class SearchService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public SearchService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        internal async Task<SearchResultDTO> Search(SearchByNameParameter parameter)
        {
            List<object> results = new();
            int totalCount = 0;
            var keywordLower = parameter.Keyword.ToLower();
            try
            {
                var searchKeyword = parameter.Keyword.Trim().ToLower();
                switch (parameter.Type)
                {
                    case SearchType.Pharmacy:
                        var rankedPharmacies = await _dataContext.Pharmacies
                                                .Where(p => p.Name.Contains(searchKeyword))
                                                .Select(p => p)
                                                .OrderByDescending(p => p.Name.Equals(searchKeyword) ? 100 :
                                                                (p.Name.StartsWith(searchKeyword) ? 80 :
                                                                (p.Name.Contains(searchKeyword) ? 60 : 40)))
                                               .ToListAsync();
                        totalCount = rankedPharmacies.Count;
                        rankedPharmacies = rankedPharmacies.Skip(parameter.Offset).Take(parameter.Limit).Select(rp => rp).ToList();
                        results.AddRange(_mapper.Map<List<PharmacyBaseDTO>>(rankedPharmacies));
                        break;
                    case SearchType.Mask:
                        var rankedMasks = await _dataContext.Masks
                                                .Include(x => x.MaskType)
                                                .Where(m => m.MaskType.Name.Contains(searchKeyword))
                                                .Select(m => m)
                                                .OrderByDescending(m => m.MaskType.Name.Equals(searchKeyword) ? 100 :
                                                                (m.MaskType.Name.StartsWith(searchKeyword) ? 80 :
                                                                (m.MaskType.Name.Contains(searchKeyword) ? 60 : 40)))
                                               .ToListAsync();
                        totalCount = rankedMasks.Count;
                        rankedMasks = rankedMasks.Skip(parameter.Offset).Take(parameter.Limit).Select(rm => rm).ToList();
                        results.AddRange(_mapper.Map<List<MaskInfoDTO>>(rankedMasks));
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
                throw;
            }

            return new SearchResultDTO
            {
                Results = results,
                Metadata = new PaginationMetaData
                {
                    Total = totalCount,
                    Limit = parameter.Limit,
                    Offset = parameter.Offset,
                },
            };

        }
    }
}