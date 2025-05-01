using AutoMapper;
using Backend.DTO;
using Backend.Parameters;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class PharmacyService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public PharmacyService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        internal async Task<List<PharmacyFilteredByMaskDTO>> FilterByMaskCondition(FilterPharmaciesByMaskConditionParameter parameter)
        {
            var result = _dataContext.Masks
                        .Where(mask => mask.Price >= parameter.MinPrice && mask.Price <= parameter.MaxPrice)
                        .Include(mask => mask.Pharmacy)
                        .GroupBy(mask => mask.Pharmacy)
                        .Select(group => new PharmacyFilteredByMaskDTO
                        {
                            Id = group.Key.Id,
                            Name = group.Key.Name,
                            NumberOfMasks = group.Count()
                        });

            if (parameter.IsMoreThan)
                result = result.Where(p => p.NumberOfMasks >= parameter.MaskCount);
            else
                result = result.Where(p => p.NumberOfMasks < parameter.MaskCount);

            return await result.ToListAsync();

        }

        internal async Task<List<OpenedPharmacyDTO>> GetOpened(GetOpenPharmaciesParameter parameter)
        {
            ushort DayOfWeek = (ushort)parameter.DayOfWeek;
            TimeOnly Time = parameter.Time;

            var result = await _dataContext.OpeningHours
                .Include(oh => oh.Pharmacy)
                .Where(oh => oh.Week == DayOfWeek && oh.CloseTime >= Time && oh.OpenTime <= Time)
                .Select(oh => oh)
                .ToListAsync();

            return _mapper.Map<List<OpenedPharmacyDTO>>(result);
        }

        internal async Task<List<MaskInfoDTO>> GetPharmacyMasks(Guid id, GetMasksOfPharmacyParameter parameter)
        {
            var search = _dataContext.Masks
                            .Where(mask => mask.PharmacyId == id)
                            .Include(mask => mask.MaskType)
                            .Select(mask => mask);

            switch (parameter.SortBy)
            {
                case MaskSortBy.Name:
                    search = search.OrderBy(mask => mask.MaskType.Name);
                    break;
                case MaskSortBy.Price:
                    search = search.OrderBy(mask => mask.Price);
                    break;
                default:
                    break;
            }

            if (parameter.Order == Order.DESC)
                search = search.OrderDescending();

            var result = await search.ToListAsync();

            return _mapper.Map<List<MaskInfoDTO>>(result);
        }
    }

}