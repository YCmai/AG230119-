using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using WMS.StorageModule.Domain.Shared;
using WMS.StorageModule.Domain;
using Volo.Abp.EventBus.Local;
using WMS.MaterialModule.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Guids;
using WMS.BarCodeModule.Domain;
using Volo.Abp;
using System.ComponentModel;

namespace WMS.MaterialModule.Domain
{

    //因为对Storage的增、删、改需要对其余数据项进行操作，需要引入IRepository<StorageOu>，所以创建了一个Manager（领域服务）来实现。

    /// <summary>
    ///
    /// </summary>
    public class MaterialManager : DomainService
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IRepository<Storage, string> _storagesRepos;
        private readonly IRepository<MaterialItem, Guid> _materialItemRepos;
        private readonly IRepository<MaterialStatistics, Guid> _materilStatisticsRepos;
        private readonly IRepository<MaterialInfo, string> _materialInfoRepos;
        private readonly IRepository<MaterialRecord, Guid> _materialRecordRepos;
        private readonly IRepository<MaterialModifyRecord, Guid> _materialModifyRecordRepos;
        private readonly IRepository<BarCodeRecord, Guid> _barCodeSplitRecordRepos;
        private readonly IStorageManager _storageManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IBarCodeGenerator _barCodeGenerator;

        public MaterialManager(
            ILocalEventBus localEventBus,
            IRepository<Storage, string> storageRepos,
            IRepository<MaterialItem, Guid> materialItemRepos,
            IRepository<MaterialStatistics, Guid> materilStatisticsRepos,
            IRepository<MaterialInfo, string> materialInfoRepos,
            IRepository<MaterialRecord, Guid> materialRecordRepos,
            IRepository<MaterialModifyRecord, Guid> materialModifyRecordRepos,
            IRepository<BarCodeRecord, Guid> barCodeSplitRecordRepos,
            IStorageManager storageManager,
            IGuidGenerator guidGenerator,
            IBarCodeGenerator barCodeGenerator

            )
        {
            _localEventBus = localEventBus;
            this._storagesRepos = storageRepos;
            _materialItemRepos = materialItemRepos;
            _materilStatisticsRepos = materilStatisticsRepos;
            _materialInfoRepos = materialInfoRepos;
            _materialRecordRepos = materialRecordRepos;
            _materialModifyRecordRepos = materialModifyRecordRepos;
            _barCodeSplitRecordRepos = barCodeSplitRecordRepos;
            _storageManager = storageManager;
            _guidGenerator = guidGenerator;
            _barCodeGenerator = barCodeGenerator;
        }

        /// <summary>
        /// 物料入库
        /// </summary>
        /// <param name="materialGuid"></param>
        /// <param name="materialSKU"></param>
        /// <param name="materialSizeMess"></param>
        /// <param name="materialIsContainer"></param>
        /// <param name="materialBarCode"></param>
        /// <param name="materialDescription"></param>
        /// <param name="MaterialQuatity"></param>
        /// <param name="materialAppData1"></param>
        /// <param name="materialAppData2"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public async Task<MaterialItem> PutMaterialInStorage(Guid materialGuid, string materialSKU, string materialName,
            string category, string materialSizeMess, bool materialIsContainer, string materialBarCode, string materialBatch, string materialDescription,
            int MaterialQuatity, Storage storage)
        {
            var materialInfo = await _materialInfoRepos.FirstOrDefaultAsync(p => p.Id == materialSKU);
            if (materialInfo == null)
            {
                materialInfo = await _materialInfoRepos.InsertAsync(new MaterialInfo(materialSKU, materialName, materialIsContainer, category, materialDescription, materialSizeMess));
            }

            //插入物料信息
            var NewMaterialItem = new MaterialItem(materialGuid,
                materialInfo.Id,
                materialBarCode,
                storage.Id,
                MaterialQuatity,
                materialBatch,
                DateTime.Now,
                storage.WareHouseId);

            await _materialItemRepos.InsertAsync(NewMaterialItem);

            //更改库存统计表
            var Statistics = await this._materilStatisticsRepos.FindAsync(p => p.WareHouseId == storage.WareHouseId && p.SKU == materialInfo.Id);
            if (Statistics == null)
            {
                await _materilStatisticsRepos.InsertAsync(new MaterialStatistics(_guidGenerator.Create(), materialSKU, NewMaterialItem.AvailableQuatity, NewMaterialItem.FreezeQuatity, NewMaterialItem.LockedQuatity, storage.WareHouseId));
            }
            else
            {
                Statistics.SetQuatity(Statistics.AvailableQuatity + NewMaterialItem.AvailableQuatity, Statistics.FreezeQuatity + NewMaterialItem.FreezeQuatity, Statistics.LockedQuatity + NewMaterialItem.LockedQuatity);
                await this._materilStatisticsRepos.UpdateAsync(Statistics);
            }


            //更改储位对物料数量的统计
            storage.SetMaterialCount(storage.CurrentNodeMaterialCount + MaterialQuatity);
            //await _storagesRepos.UpdateAsync(storage);

            return NewMaterialItem;
        }


        /// <summary>
        /// 物料出库(只有锁定的物料可以出库）
        /// </summary>
        /// <param name="materialItem"></param>
        public async Task TakeAwayFromLock(MaterialItem materialItem, int QuatityToTake)
        {
            if (QuatityToTake < 0) throw new Exception("取货数量不能小于0");
            if (materialItem.LockedQuatity < QuatityToTake) throw new Exception("没有足够的物料");

            //更新物料信息
            var OldAvailableQuatity = materialItem.AvailableQuatity;
            var OldFreezeQuatity = materialItem.FreezeQuatity;
            var OldLockedQuatity = materialItem.LockedQuatity;
            var OldSumQuatity = materialItem.SumQuatity;
            materialItem.SetQuatity(OldAvailableQuatity, OldFreezeQuatity, OldLockedQuatity - QuatityToTake);

            //添加物料（出库）记录，分为全部出库和部分出库
            string BarCode = materialItem.BarCode;
            if (materialItem.SumQuatity != 0) //如果出库后数量不是0，表示不是全部出库，需要进行条码拆分
            {
                BarCode = _barCodeGenerator.Create(materialItem.BarCode);  //创建新的条码
                await _barCodeSplitRecordRepos.InsertAsync(new BarCodeRecord(materialItem.BarCode, OldSumQuatity, BarCode, QuatityToTake)); //添加条码拆分记录
            }
            var NewRecord = new MaterialRecord(_guidGenerator.Create(), materialItem.MaterialInfoId, BarCode,
                materialItem.StorageId, QuatityToTake, materialItem.Batch, materialItem.StoreTime, DateTime.Now, materialItem.WareHouseId);
            await _materialRecordRepos.InsertAsync(NewRecord); //添加物料记录。"全部出库"和"部分出库"都需要添加，差别在于条码。部分出库会拆分条码，新条码认为出库，旧条码修改为剩余数量后留在库存里面。

            //更新库位统计
            var Statistics = await this._materilStatisticsRepos.GetAsync(p => p.WareHouseId == materialItem.WareHouseId && p.SKU == materialItem.MaterialInfoId);
            Statistics.SetQuatity(Statistics.AvailableQuatity, Statistics.FreezeQuatity, Statistics.LockedQuatity - QuatityToTake);
            await this._materilStatisticsRepos.UpdateAsync(Statistics);


            //更改储位对物料数量的统计
            var storage = await _storagesRepos.GetAsync(materialItem.StorageId);
            storage.SetMaterialCount(storage.CurrentNodeMaterialCount - QuatityToTake);
            await _storagesRepos.UpdateAsync(storage);

        }


        /// <summary>
        /// 把item拆分一部分出来。原item对应的数量会减小，返回值为新的item（调用insert）
        /// </summary>
        /// <param name="itemToSplit"></param>
        /// <param name="NewItemCount"></param>
        /// <param name="autoSave">insert的时候是否调用antosave</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<MaterialItem> SplitInTwoItem(MaterialItem itemToSplit,int NewItemCount,bool autoSave=false)
        {
            if (itemToSplit.AvailableQuatity <= NewItemCount) throw new BusinessException("可用数量不够，无法拆分");

            //添加拆分记录
			var NewBarCode = _barCodeGenerator.Create(itemToSplit.BarCode);  //创建新的条码
			await _barCodeSplitRecordRepos.InsertAsync(new BarCodeRecord(itemToSplit.BarCode, itemToSplit.SumQuatity, NewBarCode, NewItemCount)); //添加条码拆分记录

            //旧item数量减小
            itemToSplit.SetQuatity(itemToSplit.AvailableQuatity - NewItemCount, itemToSplit.FreezeQuatity, itemToSplit.LockedQuatity);

            //创建新item
            var NewItem = new MaterialItem(Guid.NewGuid(), itemToSplit.MaterialInfoId, NewBarCode, itemToSplit.StorageId, NewItemCount, itemToSplit.Batch, itemToSplit.StoreTime, itemToSplit.WareHouseId);

            await _materialItemRepos.InsertAsync(itemToSplit, autoSave);
            return itemToSplit;
		}


        /// <summary>
        /// 物料移储
        /// </summary>
        /// <param name="materialToMove"></param>
        /// <param name="NewStorage"></param>
        public async Task MoveMaterialToNewStrorage(MaterialItem materialToMove, Storage NewStorage)
        {

            //新储位物料增加
            NewStorage.SetMaterialCount(NewStorage.CurrentNodeMaterialCount + materialToMove.SumQuatity);

            //旧储位物料减小
            var oldStorage = await _storagesRepos.GetAsync(materialToMove.StorageId);
            oldStorage.SetMaterialCount(oldStorage.CurrentNodeMaterialCount - materialToMove.SumQuatity);
            await _storagesRepos.UpdateAsync(oldStorage);

            //修改区物料信息的储位记录
            materialToMove.ChangeStorageRecord(NewStorage.Id);
        }


        #region 储位内部“可用”，“冻结”，“锁定”互相转换

        public async Task LockTransformToAvaile(MaterialItem materialItem, int Quantity)
        {
            if (Quantity < 0) throw new Exception("取货数量不能小于0");
            if (materialItem.LockedQuatity < Quantity) throw new Exception("没有足够的物料");
            var OldAvailableQuatity = materialItem.AvailableQuatity;
            var OldFreezeQuatity = materialItem.FreezeQuatity;
            var OldLockedQuatity = materialItem.LockedQuatity;

            materialItem.SetQuatity(OldAvailableQuatity + Quantity, OldFreezeQuatity, OldLockedQuatity - Quantity);


            //更新库位统计
            var Statistics = await this._materilStatisticsRepos.GetAsync(p => p.WareHouseId == materialItem.WareHouseId && p.SKU == materialItem.MaterialInfoId);
            Statistics.SetQuatity(Statistics.AvailableQuatity + Quantity, Statistics.FreezeQuatity, Statistics.LockedQuatity - Quantity);
            await this._materilStatisticsRepos.UpdateAsync(Statistics);

        }

        public async Task AvaileTransformToLock(MaterialItem materialItem, int Quantity)
        {
            if (Quantity < 0) throw new Exception("取货数量不能小于0");
            if (materialItem.AvailableQuatity < Quantity) throw new Exception("没有足够的物料");
            var OldAvailableQuatity = materialItem.AvailableQuatity;
            var OldFreezeQuatity = materialItem.FreezeQuatity;
            var OldLockedQuatity = materialItem.LockedQuatity;
            materialItem.SetQuatity(OldAvailableQuatity - Quantity, OldFreezeQuatity, OldLockedQuatity + Quantity);

            //更新库位统计
            var Statistics = await this._materilStatisticsRepos.GetAsync(p => p.WareHouseId == materialItem.WareHouseId && p.SKU == materialItem.MaterialInfoId);
            Statistics.SetQuatity(Statistics.AvailableQuatity - Quantity, Statistics.FreezeQuatity, Statistics.LockedQuatity + Quantity);
            await this._materilStatisticsRepos.UpdateAsync(Statistics);


        }

        public async Task FreezeTransformToAvaile(MaterialItem materialItem, int Quantity)
        {
            if (Quantity < 0) throw new Exception("取货数量不能小于0");
            if (materialItem.FreezeQuatity < Quantity) throw new Exception("没有足够的物料");

            var OldAvailableQuatity = materialItem.AvailableQuatity;
            var OldFreezeQuatity = materialItem.FreezeQuatity;
            var OldLockedQuatity = materialItem.LockedQuatity;

            materialItem.SetQuatity(OldAvailableQuatity + Quantity, OldFreezeQuatity - Quantity, OldLockedQuatity);


            //更新库位统计
            var Statistics = await this._materilStatisticsRepos.GetAsync(p => p.WareHouseId == materialItem.WareHouseId && p.SKU == materialItem.MaterialInfoId);
            Statistics.SetQuatity(Statistics.AvailableQuatity + Quantity, Statistics.FreezeQuatity - Quantity, Statistics.LockedQuatity);
            await this._materilStatisticsRepos.UpdateAsync(Statistics);

        }

        public async Task AvaileTransformToFreeze(MaterialItem materialItem, int Quantity)
        {
            if (Quantity < 0) throw new Exception("取货数量不能小于0");
            if (materialItem.AvailableQuatity < Quantity) throw new Exception("没有足够的物料");
            var OldAvailableQuatity = materialItem.AvailableQuatity;
            var OldFreezeQuatity = materialItem.FreezeQuatity;
            var OldLockedQuatity = materialItem.LockedQuatity;
            materialItem.SetQuatity(OldAvailableQuatity - Quantity, OldFreezeQuatity + Quantity, OldLockedQuatity);


            //更新库位统计
            var Statistics = await this._materilStatisticsRepos.GetAsync(p => p.WareHouseId == materialItem.WareHouseId && p.SKU == materialItem.MaterialInfoId);
            Statistics.SetQuatity(Statistics.AvailableQuatity - Quantity, Statistics.FreezeQuatity + Quantity, Statistics.LockedQuatity);
            await this._materilStatisticsRepos.UpdateAsync(Statistics);
        }


        #endregion

        /// <summary>
        /// 获取储位所有物料
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="includeChildrenStorage"></param>
        /// <param name="includeDetetails"></param>
        /// <returns></returns>
        public async Task<List<MaterialItem>> GetMaterial(string storageId, bool includeChildrenStorage = false, bool includeContainerMaterial = false)
        {
            if (includeChildrenStorage)
            {
                IQueryable<MaterialItem> materialQuery;
                if (includeContainerMaterial)
                {
                    materialQuery = (await _materialItemRepos.GetQueryableAsync());
                }
                else
                {

                    materialQuery = from m in (await _materialItemRepos.GetQueryableAsync())
                                    join q in (await _materialInfoRepos.GetQueryableAsync()).Where(p => p.IsContainer == false)
                                    on m.MaterialInfoId equals q.Id
                                    select m;
                }

                var parentStorage = await _storagesRepos.GetAsync(storageId);

                //内联。
                var Query = from material in materialQuery
                            join stroage in (await _storageManager.FindStorageAndChildrenQueryableAsync(parentStorage, recursive: true, includeParent: true))
                            on material.StorageId equals stroage.Id
                            select material;
                return Query.ToList();
            }
            else  //只获取本储位直接物料。
            {
                if (includeContainerMaterial)
                {
                    return await _materialItemRepos.GetListAsync(p => p.AvailableQuatity > 0 && p.StorageId == storageId);
                }
                else
                {
                    var temp = from m in (await _materialItemRepos.GetQueryableAsync()).Where(p => p.AvailableQuatity > 0 && p.StorageId == storageId)
                               join q in (await _materialInfoRepos.GetQueryableAsync()).Where(p => p.IsContainer == false)
                                    on m.MaterialInfoId equals q.Id
                               select m;

                    return temp.ToList();
                }
            }
        }

        internal async Task<MaterialItem> ModifyAdd(Guid materialGuid, MaterialInfo materialInfo, string materialBarCode, string materialBatch,
            int Quatity, Storage storage, Guid? userId)
        {
            //插入物料信息
            var NewMaterialItem = new MaterialItem(materialGuid,
                materialInfo.Id,
                materialBarCode,
                storage.Id,
                Quatity,
                materialBatch,
                DateTime.Now,
                storage.WareHouseId);

            await _materialItemRepos.InsertAsync(NewMaterialItem);

            //更改库存统计表
            var Statistics = await this._materilStatisticsRepos.FindAsync(p => p.WareHouseId == storage.WareHouseId && p.SKU == materialInfo.Id);
            if (Statistics == null)
            {
                await _materilStatisticsRepos.InsertAsync(new MaterialStatistics(_guidGenerator.Create(), materialInfo.Id, NewMaterialItem.AvailableQuatity, NewMaterialItem.FreezeQuatity, NewMaterialItem.LockedQuatity, storage.WareHouseId));
            }
            else
            {
                Statistics.SetQuatity(Statistics.AvailableQuatity + NewMaterialItem.AvailableQuatity, Statistics.FreezeQuatity + NewMaterialItem.FreezeQuatity, Statistics.LockedQuatity + NewMaterialItem.LockedQuatity);
                await this._materilStatisticsRepos.UpdateAsync(Statistics);
            }


            //更改储位对物料数量的统计
            storage.SetMaterialCount(storage.CurrentNodeMaterialCount + Quatity);
            //await _storagesRepos.UpdateAsync(storage);

            await _materialModifyRecordRepos.InsertAsync(new MaterialModifyRecord(_guidGenerator.Create(), materialBarCode, materialInfo.Id, 0, Quatity, DateTime.Now, userId));
            //生成记录
            return NewMaterialItem;
        }

        public async Task ModifyUpdateAvailCount(MaterialItem materialItem, int NewAvailableCount, Guid? userId)
        {
            var OldValue = materialItem.AvailableQuatity;
            materialItem.SetQuatity(NewAvailableCount, materialItem.FreezeQuatity, materialItem.LockedQuatity);

            //更改库存统计表
            var Statistics = await this._materilStatisticsRepos.GetAsync(p => materialItem.WareHouseId == materialItem.WareHouseId && p.SKU == materialItem.MaterialInfoId);
            Statistics.SetQuatity(Statistics.AvailableQuatity + NewAvailableCount - OldValue, Statistics.FreezeQuatity, Statistics.LockedQuatity);
            await this._materilStatisticsRepos.UpdateAsync(Statistics);

            //更改储位对物料数量的统计
            var storage = await _storagesRepos.GetAsync(materialItem.StorageId);
            // storage.SetMaterialCount(storage.CurrentNodeMaterialCount + NewAvailableCount - OldValue);
            //项目大部分都是直接置为0，按照公式计算会出现负1的数据
            storage.SetMaterialCount(0);
            await _storagesRepos.UpdateAsync(storage);

            await _materialModifyRecordRepos.InsertAsync(new MaterialModifyRecord(_guidGenerator.Create(), materialItem.BarCode, materialItem.MaterialInfoId, OldValue, NewAvailableCount, DateTime.Now, userId));
            //生成记录
        }


    }
}
