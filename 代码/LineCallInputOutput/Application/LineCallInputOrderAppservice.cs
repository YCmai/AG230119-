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
    [Route("api/WMS/LineCallInputOrder")]
    public class LineCallInputOrderAppservice : ApplicationService
    {
        private readonly LineCallInputOrderManager _lineCallInputOrderManager;
        private readonly IRepository<LineCallInputOrder, Guid> _lineCallInputOrderRepos;
        private readonly IRepository<MaterialItem, Guid> _materialItemRepos;
        private readonly IRepository<Storage, string> _storageRepos;
        private readonly MaterialManager _materialManager;
        private readonly IRepository<WareHouse, string> _wareHouseRepos;
        private readonly IGuidGenerator _guidGenerator;

        public LineCallInputOrderAppservice(
            LineCallInputOrderManager lineCallInputOrderManager,
            IRepository<LineCallInputOrder, Guid>  lineCallInputOrderRepos,
            IRepository<MaterialItem, Guid> materialItemRepos,
            IRepository<Storage, string> storageRepos,
            MaterialManager materialManager,
            IRepository<WareHouse,string> wareHouseRepos,
            IGuidGenerator guidGenerator
            )
        {
            _lineCallInputOrderManager = lineCallInputOrderManager;
            _lineCallInputOrderRepos = lineCallInputOrderRepos;
            _materialItemRepos = materialItemRepos;
            _storageRepos = storageRepos;
            _materialManager = materialManager;
            _wareHouseRepos = wareHouseRepos;
            _guidGenerator = guidGenerator;
        }


        [HttpGet("GetList")] 
        public async Task<PagedResultDto<LineCallInputOrderDto>> GetList(GetAllLineCallInputOrderPageInput input)
        {
            var query = (await this._lineCallInputOrderRepos.GetQueryableAsync())
                .WhereIf(!input.SKU.IsNullOrWhiteSpace(), p => p.SKU == input.SKU)
                .WhereIf(!input.InputStorageId.IsNullOrWhiteSpace(), p => p.InputStorageId == input.InputStorageId)
                .WhereIf(input.State.HasValue, p => p.State == input.State)
                .WhereIf(!input.WarehouseCode.IsNullOrWhiteSpace(), p => p.WarehouseCode == input.WarehouseCode)
                .OrderBy(p => p.CreationTime);

            var total = query.Count();
            var entities = query.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
            return new PagedResultDto<LineCallInputOrderDto>(total, ObjectMapper.Map<List<LineCallInputOrder>, List<LineCallInputOrderDto>>(entities));
        }


        /// <summary>
        /// 上架一个物料，并马上呼叫创建入库单进行移库
        /// </summary>
        /// <param name="lineCallInputOrderCreateInput"></param>
        /// <returns></returns>
        [HttpPost("CreateInputOrderAndStartDeliver")]
        public async Task<LineCallInputOrderDto> CreateInputOrderAndStartDeliverAsync([FromBody] LineCallInputOrderCreateInput lineCallInputOrderCreateInput)
        {
            var Warehouse = await _wareHouseRepos.GetAsync(lineCallInputOrderCreateInput.WarehouseId);
            var Storage = await _storageRepos.GetAsync(p=> p.Id == lineCallInputOrderCreateInput.PutonStorageId && p.WareHouseId == Warehouse.Id );

            //物料存入储位
            await _materialManager.PutMaterialInStorage(
                materialGuid: _guidGenerator.Create(),
                materialSKU: lineCallInputOrderCreateInput.SKU,
                materialName:lineCallInputOrderCreateInput.Name,
                category:"",
                materialSizeMess:"",
                materialIsContainer: false,
                materialBarCode: lineCallInputOrderCreateInput.BarCode,
                materialBatch:"Default",
                materialDescription: "",
                MaterialQuatity: 1,
                storage: Storage);


            //添加呼叫单
            var entity = new LineCallInputOrder(
                warehouseCode:lineCallInputOrderCreateInput.WarehouseId,
                Id: lineCallInputOrderCreateInput.Id,
                reMark: "",
                pickUserId: lineCallInputOrderCreateInput.CreatorId,  //todo，没有对创建者进行确认是否存在。
                creatorId: lineCallInputOrderCreateInput.CreatorId,
                putonStorageId: lineCallInputOrderCreateInput.PutonStorageId,
                sku: lineCallInputOrderCreateInput.SKU,
                barCode: lineCallInputOrderCreateInput.BarCode
                );

            //马上启动呼叫任务
            await _lineCallInputOrderManager.StartDeliverAsync(entity);
            entity = await this._lineCallInputOrderRepos.InsertAsync(entity);


            return ObjectMapper.Map<LineCallInputOrder, LineCallInputOrderDto>(entity);
        }

        /// <summary>
        /// 开始移库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("StartDeliver")]
        public async Task<LineCallInputOrderDto> StartDeliverAsync(Guid id)
        {
            //添加呼叫
            var entity = await _lineCallInputOrderRepos.GetAsync(id);
            //启动
            await _lineCallInputOrderManager.StartDeliverAsync(entity);
            await _lineCallInputOrderRepos.UpdateAsync(entity);


            return ObjectMapper.Map<LineCallInputOrder, LineCallInputOrderDto>(entity);
        }


        /// <summary>
        /// 删除线边入库单
        /// </summary>
        /// <param name="storageDeleteInput"></param>
        /// <returns></returns>
        [HttpPost("Delete")]
        public async Task DeleteAsync(Guid guid)
        {
            await _lineCallInputOrderRepos.DeleteAsync(guid);
        }

        /// <summary>
        /// 删除线边出库单
        /// </summary>
        /// <param name="storageDeleteInput"></param>
        /// <returns></returns>
        [HttpPost("DeleteAll")]
        public async Task DeleteAllAsync()
        {
            var all = await _lineCallInputOrderRepos.GetListAsync();
            await _lineCallInputOrderRepos.DeleteManyAsync(all);
            await Task.CompletedTask;
        }

    }
}
