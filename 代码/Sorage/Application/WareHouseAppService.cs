using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using WMS.StorageModule.Domain;

namespace WMS.StorageModule.Application
{
    [Route("/api/wms/warehouse")]
    public class WareHouseAppService : ApplicationService
    {
        WareHouseManager _wareHouseManager;
        IRepository<WareHouse> _wareHouseRepository;
        public WareHouseAppService(WareHouseManager wareHouseManager, IRepository<WareHouse> wareHouseRepository)
        {
            _wareHouseManager = wareHouseManager;
            _wareHouseRepository = wareHouseRepository;
        }
        [HttpGet("all")]
        public async Task<List<WareHouseDto>> GetAllWareHouses()
        {
            var context = await _wareHouseRepository.WithDetailsAsync(m => m.Areas);
            var result = context.ToList();
            return ObjectMapper.Map<List<WareHouse>, List<WareHouseDto>>(result);
        }

        [HttpGet("page")]
        public async Task<PagedResultDto<WareHouseDto>> GetWareHouses(GetWareHousePageRequest input)
        {
            var context = (await _wareHouseRepository.WithDetailsAsync(m => m.Areas)).WhereIf(!string.IsNullOrEmpty(input.WareHouseId), m => m.Id.Contains(input.WareHouseId));
            var count = context.Count();
            var result = context.OrderBy(m => m.Id).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<WareHouseDto>(count, ObjectMapper.Map<List<WareHouse>, List<WareHouseDto>>(result));
        }
        [HttpPost("create")]
        public async Task<WareHouseDto> CreateAsync(CreateWareHouseInput input)
        {
            var wareHouse = await _wareHouseManager.CreateAsnyc(input.Id, input.Name, input.Description);
            return ObjectMapper.Map<WareHouse, WareHouseDto>(wareHouse);
        }
        [HttpPut("{id}")]
        public async Task<WareHouseDto> UpdateAsync(string id, UpdateWareHouseDto input)
        {
            var wareHouse = new WareHouse(id, input.Name, input.Description);
            wareHouse.ConcurrencyStamp = input.ConcurrencyStamp;
            await _wareHouseRepository.UpdateAsync(wareHouse);
            return ObjectMapper.Map<WareHouse, WareHouseDto>(wareHouse);
        }
        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            var warehouse = await _wareHouseRepository.GetAsync(m => m.Id == id);
            await _wareHouseRepository.DeleteAsync(warehouse);
        }
        [HttpGet("{id}")]
        public async Task<WareHouseDto> GetAsync(string id)
        {
            var wareHouse = await _wareHouseRepository.GetAsync(m => m.Id == id);
            return ObjectMapper.Map<WareHouse, WareHouseDto>(wareHouse);
        }
        [HttpPost("area/create")]
        public async Task<WareHouseAreaDto> CreateAreaAsnyc(AddAreaInput input)
        {
            var wareHouse = await _wareHouseRepository.GetAsync(m => m.Id == input.wareHouseId);
            var area = wareHouse.AddArea(input.Code, input.Name, input.Description, input.Category);
            return ObjectMapper.Map<WareHouseArea, WareHouseAreaDto>(area);
        }
        [HttpPut("area/{wareHouseId}/{areaCode}")]
        public async Task<WareHouseAreaDto> UpdateAreaAsync(string wareHouseId, string areaCode, UpdateAreaInput input)
        {
            var warehouse = (await _wareHouseRepository.WithDetailsAsync(m => m.Areas)).First(m => m.Id == wareHouseId);
            //先将原来的删除
            warehouse.Areas.Remove(warehouse.Areas.First(m => m.Code == areaCode));
            var area = warehouse.AddArea(areaCode, input.Name, input.Description, input.Category);
            return ObjectMapper.Map<WareHouseArea, WareHouseAreaDto>(area);
        }
        [HttpDelete("area/{wareHouseId}/{areaCode}")]
        public async Task DeleteAreaAsync(string wareHouseId, string areaCode)
        {
            var warehouse = (await _wareHouseRepository.WithDetailsAsync(m => m.Areas)).First(m => m.Id == wareHouseId);
            warehouse.Areas.Remove(warehouse.Areas.First(m => m.Code == areaCode));
        }
        [HttpGet("area/{wareHouseId}/{areaCode}")]
        public async Task<WareHouseAreaDto> GetAreaAsync(string wareHouseId, string areaCode)
        {
            var warehouse = (await _wareHouseRepository.WithDetailsAsync(m => m.Areas)).First(m => m.Id == wareHouseId);
            var area = warehouse.Areas.FirstOrDefault(m => m.Code == areaCode);
            return ObjectMapper.Map<WareHouseArea, WareHouseAreaDto>(area);
        }
    }
}
