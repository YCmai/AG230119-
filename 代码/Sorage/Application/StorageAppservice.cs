using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using WMS.StorageModule.Domain.Shared;
using WMS.StorageModule.Domain;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using System.Linq;
using Volo.Abp.Guids;
using Volo.Abp.Application.Dtos;
using Volo.Abp;

namespace WMS.StorageModule.Application
{
    [Route("api/WMS/Storage")]
    public class StorageAppService : ApplicationService
    {
        private readonly IStorageManager _storageManager;
        private readonly IRepository<Storage, string> _storageReps;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IRepository<WareHouse, string> _wareHouseRepos;

        public StorageAppService(
            IStorageManager StroageManager,
            IRepository<Storage, string> StorageReadOnlyBasicReps,
            IGuidGenerator guidGenerator,
            IRepository<WareHouse, string> WareHouseRepos
            )
        {
            this._storageManager = StroageManager;
            this._storageReps = StorageReadOnlyBasicReps;
            _guidGenerator = guidGenerator;
            _wareHouseRepos = WareHouseRepos;
        }
        [HttpGet("Query")]
        public async Task<List<StorageDto>> QueryAllAsync(string id, int maxResultCount = 20)
        {
            var entities = (await _storageReps.GetQueryableAsync())
                .WhereIf(!string.IsNullOrEmpty(id), m => m.Id.Contains(id))
                .ToList()
                .OrderBy(m => m.Id)
                .Take(maxResultCount)
                .ToList();
            return ObjectMapper.Map<List<Storage>, List<StorageDto>>(entities);
        }

        /// <summary>
        /// 获取指定储位
        /// </summary>
        /// <param name="getStorageInput"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<StorageDto> GetAsync(string id)
        {
            var entity = await _storageReps.GetAsync(id);
            return ObjectMapper.Map<Storage, StorageDto>(entity);
        }

        /// <summary>
        /// 获取所有储位(按创建时间排序）
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public async Task<PagedResultDto<StorageDto>> GetAllAsync(GetAllStoragePageInput input)
        {
            var query = (await this._storageReps.GetQueryableAsync())
                .WhereIf(!input.WareHouseCode.IsNullOrWhiteSpace(),p=>p.WareHouseId==input.WareHouseCode)
                .WhereIf(!input.AreaCode.IsNullOrWhiteSpace(),p=>p.WareHouseIdAreaCode==input.AreaCode);
            var total = query.Count();
            var entities = query.OrderBy(p => p.CreationTime)
                                .Skip(input.SkipCount)
                                .Take(input.MaxResultCount)
                                .ToList();
            return new PagedResultDto<StorageDto>(total, ObjectMapper.Map<List<Storage>, List<StorageDto>>(entities));
        }

        /// <summary>
        /// 获取根储位（也就是储位父级是空的储位）(按创建时间排序）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("GetRoot")]
        public async Task<PagedResultDto<StorageDto>> GetRootAsync(GetRootStoragePageInput input)
        {
            var query = (await this._storageManager.FindStorageAndChildrenQueryableAsync(null, recursive: false, includeParent: false))
                .WhereIf(!input.WareHouseCode.IsNullOrWhiteSpace(), p => p.WareHouseId == input.WareHouseCode)
                .WhereIf(!input.AreaCode.IsNullOrWhiteSpace(), p => p.WareHouseIdAreaCode == input.AreaCode);
            var total = query.Count();
            var entities = query
                .OrderBy(p=>p.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();
            return new PagedResultDto<StorageDto>(total, ObjectMapper.Map<List<Storage>, List<StorageDto>>(entities));
        }

        /// <summary>
        /// 获取指定储位的子储位（按创建时间排序）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("GetChildren")]
        public async Task<PagedResultDto<StorageDto>> GetChildren(GetChildrenStoragePageInput input)
        {
            var parentStorage = await _storageReps.GetAsync(input.ParentStorageId);            
            var query = (await this._storageManager.FindStorageAndChildrenQueryableAsync(parentStorage, input.recursive, includeParent: false))
                .WhereIf(!input.WareHouseCode.IsNullOrWhiteSpace(), p => p.WareHouseId == input.WareHouseCode)
                .WhereIf(!input.AreaCode.IsNullOrWhiteSpace(), p => p.WareHouseIdAreaCode == input.AreaCode); 
            var total = query.Count();
            var entities = query
                .OrderBy(p => p.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();
            return new PagedResultDto<StorageDto>(total, ObjectMapper.Map<List<Storage>, List<StorageDto>>(entities));
        }



        [HttpPost("Create")]
        public async Task<StorageDto> CreateAsync(StorageCreateInput storageCreateInput)
        {
            Storage parent = null;
            if (storageCreateInput.ParentId.IsNullOrWhiteSpace())
            {
                parent = null;
            }
            else
            {
                parent = await this._storageReps.GetAsync(storageCreateInput.ParentId);
                if (parent == null) throw new BusinessException("ParenStorageNotFound");
            }

            var wareHouse =await _wareHouseRepos.GetAsync(storageCreateInput.WareHouseId,includeDetails:true);
            foreach (var item in wareHouse.Areas)
            {
                if (item.Code == storageCreateInput.WareHouseAreaCode)
                {
                    var entity = await this._storageManager.CreatAsync(
                         storageCreateInput.Id,
                         parent,
                         storageCreateInput.MapNodeName,
                         storageCreateInput.CreatorId,
                         storageCreateInput.StorageType,
                         storageCreateInput.Name,
                         storageCreateInput.SizeMes,
                         storageCreateInput.AppData1,
                         storageCreateInput.AppData2,
                         storageCreateInput.StartHeight,
                         item);

                    return ObjectMapper.Map<Storage, StorageDto>(entity);
                }
            }

            throw new BusinessException("WarehouseRegionNotFound");
        }



        [HttpDelete]
        public async Task DeleteAsync(string Id)
        {
            var entity = await this._storageReps.FindAsync(Id);
            if (entity == null) throw new BusinessException("储位不存在");
            await this._storageManager.DeleteIncludeChildrenAsync(entity);
        }

        [HttpPost("Update")]
        public async Task<StorageDto> UpdteAsyc(UpdateStorageInput input)
        {
            var storage = await this._storageReps.GetAsync(input.Id);
            storage.MapNodeName = input.MapNodeName;
            storage.Name = input.Name;
            storage.SizeMes = input.SizeMes;
            storage.StartHeight = input.StartHeight;
            storage.Category = input.Category;
            storage.AppData1 = input.AppData1;
            storage.AppData2 = input.AppData2;
            var re = await this._storageReps.UpdateAsync(storage);
            return ObjectMapper.Map<Storage, StorageDto>(re);
        }

        [HttpPost("CallLineSeed")]
        public async Task CallLineSeed()
        {
            //构建线边仓库
            var NewWarehouse = new WareHouse("DefaultCallLineWareHouse", "线边仓", "系统默认线边呼叫仓库");
            //仓库包含了3区域
            var DefaultInputArea = NewWarehouse.AddArea("DefaultInputArea", "线边入库区", "", DefaultAreaCategory.LineCallInputArea);
            var DefaultOutputArea = NewWarehouse.AddArea("DefaultOutputArea", "线边出库区", "", DefaultAreaCategory.LineCallOutputArea);
            var DefaultStorageArea = NewWarehouse.AddArea("DefaultStorageArea", "线边存储区", "", DefaultAreaCategory.LineCallStorageArea);
            await _wareHouseRepos.InsertAsync(NewWarehouse);

            //入库区有一个储位
            await this._storageManager.CreatAsync(
                StorageCode: "Input_storage01",
                parent: null,
                mapNodeName: "",
                creatorId: _guidGenerator.Create(),
                storageCategory: StorageDefaultCategory.GeneralStorage,
                storageName: "入库储位01",
                sizeMes: "",
                appData1: "",
                appData2: "",
                startHeight: 0,
                wareHouseArea: DefaultInputArea);

            //出库区有一个储位
            await this._storageManager.CreatAsync(
               StorageCode: "Output_storage01",
               parent: null,
               mapNodeName: "",
               creatorId: Guid.NewGuid(),
               storageCategory: StorageDefaultCategory.GeneralStorage,
               storageName: "出库储位01",
               sizeMes: "",
               appData1: "",
               appData2: "",
               startHeight: 0,
               wareHouseArea: DefaultOutputArea);

            //存储区有2个储位
            await this._storageManager.CreatAsync(
                StorageCode: "Store_storage01",
                parent: null,
                mapNodeName: "",
                creatorId: Guid.NewGuid(),
                storageCategory: StorageDefaultCategory.GeneralStorage,
                storageName: "存储储位01",
                sizeMes: "",
                appData1: "",
                appData2: "",
                startHeight: 0,
                wareHouseArea: DefaultStorageArea);

            await this._storageManager.CreatAsync(
                StorageCode: "Store_storage02",
                parent: null,
                mapNodeName: "",
                creatorId: Guid.NewGuid(),
                storageCategory: StorageDefaultCategory.GeneralStorage,
                storageName: "存储储位02",
                sizeMes: "",
                appData1: "",
                appData2: "",
                startHeight: 0,
                wareHouseArea: DefaultStorageArea);
        }
    }

}
