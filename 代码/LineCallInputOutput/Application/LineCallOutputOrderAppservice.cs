using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using WMS.LineCallInputModule.Domain;
using Volo.Abp.Application.Services;
using WMS.MaterialModule.Domain;
using WMS.StorageModule.Domain;
using Volo.Abp.EventBus.Local;
using HBTaskModule.Application;
using Volo.Abp.Guids;
using Volo.Abp.Application.Dtos;
using System.Linq;

namespace WMS.LineCallInputModule.Application
{
    [Route("api/WMS/LineCallOutputOrder")]
    public class LineCallOutputOrderAppservice : ApplicationService
    {
        private readonly LineCallInputOrderManager _lineCallInputOrderManager;
        private readonly IRepository<LineCallOutputOrder, Guid> _lineCallOutputOrderRepos;
        private readonly IRepository<MaterialItem, Guid> _materialItemRepos;
        private readonly IRepository<Storage, string> _storageRepos;
        private readonly MaterialManager _materialManager;
        private readonly IRepository<WareHouse, string> _wareHouseRepos;
        private readonly IGuidGenerator _guidGenerator;

        public LineCallOutputOrderAppservice(
            LineCallInputOrderManager lineCallInputOrderManager,
            IRepository<LineCallOutputOrder, Guid>  lineCallOutputOrderRepos,
            IRepository<MaterialItem, Guid> materialItemRepos,
            IRepository<Storage, string> storageRepos,
            MaterialManager materialManager,
            IRepository<WareHouse,string> wareHouseRepos,
            IGuidGenerator guidGenerator
            )
        {
            _lineCallInputOrderManager = lineCallInputOrderManager;
            _lineCallOutputOrderRepos = lineCallOutputOrderRepos;
            _materialItemRepos = materialItemRepos;
            _storageRepos = storageRepos;
            _materialManager = materialManager;
            _wareHouseRepos = wareHouseRepos;
            _guidGenerator = guidGenerator;
        }



        [HttpGet("GetList")]  
        public async Task<PagedResultDto<LineCallOutputOrderDto>> GetList(GetAllLineCallOutputOrderPageInput input)
        {
            var query = (await this._lineCallOutputOrderRepos.GetQueryableAsync())
                .WhereIf(!input.SKU.IsNullOrWhiteSpace(), p => p.SKU == input.SKU)
                .WhereIf(!input.OutputStorageId.IsNullOrWhiteSpace(), p => p.OutputStorageId == input.OutputStorageId)
                .WhereIf(input.State.HasValue, p => p.State == input.State)
                .WhereIf(!input.WarehouseCode.IsNullOrWhiteSpace(), p => p.WarehouseCode == input.WarehouseCode)
                .OrderBy(p => p.CreationTime);

            var total = query.Count();
            var entities = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<LineCallOutputOrderDto>(total, ObjectMapper.Map<List<LineCallOutputOrder>, List<LineCallOutputOrderDto>>(entities));
        }


        /// <summary>
        /// 上架一个物料，并马上呼叫创建入库单进行移库
        /// </summary>
        /// <param name="lineCallInputOrderCreateInput"></param>
        /// <returns></returns>
        [HttpPost("CreateOutputOrderAndStart")]
        public async Task<LineCallOutputOrderDto> CreateInputOrderAndStartAsync([FromBody] LineCallOutputOrderCreateInput lineCallOutputOrderCreateInput)
        {
            var Warehouse = await _wareHouseRepos.GetAsync(lineCallOutputOrderCreateInput.WarehouseId);
            var Storage = await _storageRepos.GetAsync(p=> p.Id == lineCallOutputOrderCreateInput.OutputStorageId && p.WareHouseId == Warehouse.Id );



            //添加呼叫单
            var entity = new LineCallOutputOrder(
                Id: lineCallOutputOrderCreateInput.Id,
                reMark: lineCallOutputOrderCreateInput.ReMark,
                creatorId: lineCallOutputOrderCreateInput.CreatorId,
                sku: lineCallOutputOrderCreateInput.SKU,
                outputStorageId: lineCallOutputOrderCreateInput.OutputStorageId,
                warehouseCode: lineCallOutputOrderCreateInput.WarehouseId);

            //马上启动呼叫任务
            await _lineCallInputOrderManager.StartOutputAsync(entity);
            entity = await this._lineCallOutputOrderRepos.InsertAsync(entity);

            return ObjectMapper.Map<LineCallOutputOrder, LineCallOutputOrderDto>(entity);
        }



        /// <summary>
        /// 删除线边入库单
        /// </summary>
        /// <param name="storageDeleteInput"></param>
        /// <returns></returns>
        [HttpPost("Delete")]
        public async Task DeleteAsync(Guid guid)
        {
            await _lineCallOutputOrderRepos.DeleteAsync(guid);
        }

        /// <summary>
        /// 删除线边出库单
        /// </summary>
        /// <param name="storageDeleteInput"></param>
        /// <returns></returns>
        [HttpPost("DeleteAll")]
        public async Task DeleteAllAsync()
        {
            var all = await _lineCallOutputOrderRepos.GetListAsync();
            await _lineCallOutputOrderRepos.DeleteManyAsync(all);
            await Task.CompletedTask;
        }

    }
}
