using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using WMS.StorageModule.Domain.Shared;

namespace WMS.StorageModule.Domain
{

    //因为对Storage的增、删、改需要对其余数据项进行操作，需要引入IRepository<StorageOu>，所以创建了一个Manager（领域服务）来实现。

    /// <summary>
    ///
    /// </summary>
    public class StorageManager : DomainService, IStorageManager
    {
        private readonly IRepository<Storage, string> storagesReps;
        private readonly IGuidGenerator _guidGenerator;

        public StorageManager(IRepository<Storage,string> Storages,
            IGuidGenerator  guidGenerator)
        {
            this.storagesReps = Storages;
            _guidGenerator = guidGenerator;
        }

        //增
        /// <summary>
        /// 
        /// </summary>
        /// <param name="StorageCode">储位编号</param>
        /// <param name="parent">父储位，如果是空，表示没有父储位</param>
        /// <param name="mapNodeName">对应的地图节点，可以是空字符串</param>
        /// <param name="creatorId">创建者Id</param>
        /// <param name="storageCategory">储位分类</param>
        /// <param name="storageName">储位名称</param>
        /// <param name="sizeMes">储位大小信息，可以为空字符串</param>
        /// <returns></returns>
        public async Task<Storage> CreatAsync(string StorageId, Storage parent, string mapNodeName, Guid creatorId, string storageCategory, string storageName, string sizeMes, 
            string appData1, string appData2, int startHeight,WareHouseArea wareHouseArea)
        {
            var OUCode = await GetNextChildCodeAsync(parent);

            return await storagesReps.InsertAsync(new Storage(StorageId, parent?.Id, OUCode, mapNodeName, creatorId, storageCategory, storageName, sizeMes, appData1, appData2, startHeight, wareHouseArea.WareHouseId, wareHouseArea.Code));
        }

        //删
        /// <summary>
        /// 删除本储位，包含所有的子储位(todo,删除容易导致多线程问题，例如添加储位时同时删除子储位，ParentId需要建立数据库关联外键才可以，否则会出现问题）。如果设立外键，那么就不再支持非关系数据库。
        /// </summary>
        /// <param name="Storage"></param>
        /// <returns></returns>
        public async Task DeleteIncludeChildrenAsync(Storage Storage)
        {
            if (Storage == null)
            {
                await storagesReps.DeleteAsync(p => true);
            }
            else
            {
                await storagesReps.DeleteAsync(p => p.OUCode.StartsWith(Storage.OUCode));
            }
        
        }

        public async Task DeleteChildrenAsync(Storage parentStorage)
        {
            await storagesReps.DeleteAsync(p => p.Id != parentStorage.Id && p.OUCode.StartsWith(parentStorage.OUCode));
        }


        //改
        /// <summary>
        /// 这回修改目标储位和目标储位的下面的的所有储位。 对于子孙对象很多的，注意不能频繁调用。
        /// </summary>
        /// <param name="storageOUToMove"></param>
        /// <param name="NewFather"></param>
        /// <returns></returns>
        public async Task MoveAsync(Storage storageOUToMove, Storage NewParent)
        {
            if (storageOUToMove.ParentId == NewParent.Id)  //本身就是父子关系的，不用处理。
            {
                return;
            }

            //获取原来的子对象，需要递归的。
            var children = await FindChildrenAsync(storageOUToMove, true);

            //先保存更改前的OUCode
            var oldCode = storageOUToMove.OUCode;

            //更新需要移动的对象
            storageOUToMove.OUCode = await GetNextChildCodeAsync(NewParent);
            storageOUToMove.ParentId = NewParent.Id;
            await storagesReps.UpdateAsync(storageOUToMove);

            //更改所有的子孙对象
            foreach (var child in children)
            {
                child.OUCode = AppendCode(storageOUToMove.OUCode, GetRelativeCode(child.OUCode, oldCode));   //子孙对象直接删本对象的旧oldCode，替换成新的OUCode
                await storagesReps.UpdateAsync(child);
            }
        }


        //查

        /// <summary>
        /// 找出对应的子类，可以制定是否递归
        /// </summary>
        /// <param name="parent">父对象Id，可以是空</param>
        /// <param name="recursive">是否递归</param>
        /// <returns></returns>
        public async Task<List<Storage>> FindChildrenAsync(Storage parent, bool recursive = false)
        {
            if (parent == null)  //父对象是空，表示顶层对象
            {
                if (!recursive)  //最顶层对象的的子对象
                {
                    return (await storagesReps.GetQueryableAsync()).Where(a => a.ParentId ==null).ToList();
                }
                else  //最顶层对象的递归对象，那就是全部对象。
                {
                    return await storagesReps.GetListAsync();
                }
            }
            else
            {
                if (!recursive)   //指定对象的一级子对象
                {
                    return (await storagesReps.GetQueryableAsync()).Where(a => a.ParentId.Equals(parent.Id)).ToList();
                }
                else //指定对象的所有递归对象
                {
                    return (await storagesReps.GetQueryableAsync()).Where(a => a.Id != parent.Id && a.OUCode.StartsWith(parent.OUCode)).ToList();
                }
            }
        }

        /// <summary>
        /// 找出对应的储位及其子类
        /// </summary>
        /// <param name="parent">父对象Id，可以是空</param>
        /// <param name="recursive">是否递归</param>
        /// <returns></returns>
        public async Task<IQueryable<Storage>> FindStorageAndChildrenQueryableAsync(Storage parent, bool recursive = false,bool includeParent=false)
        {
            if (parent == null)  //父对象是空，表示顶层对象
            {
                if (!recursive)  //最顶层对象的的子对象
                {
                    return (await storagesReps.GetDbSetAsync()).AsQueryable().Where(p => p.ParentId == null);
                }
                else  //最顶层对象的递归对象，那就是全部对象。
                {
                    return (await storagesReps.GetDbSetAsync()/*.ConfigureAwait(false)*/).AsQueryable();
                }
            }
            else
            {
                if (!recursive)   //指定对象的一级子对象
                {
                    if (includeParent)
                    {
                        return (await storagesReps.GetDbSetAsync()).AsQueryable().Where(a => a.ParentId.Equals(parent.Id) || a.Id.Equals(parent.Id));
                    }
                    else
                    {
                        return (await storagesReps.GetDbSetAsync()).AsQueryable().Where(a => a.ParentId.Equals(parent.Id));
                    }
                }
                else //指定对象的所有递归对象
                {
                    if (includeParent)
                    {
                        return (await storagesReps.GetQueryableAsync()).Where(a => a.OUCode.StartsWith(parent.OUCode));
                    }
                    else
                    {
                        return (await storagesReps.GetQueryableAsync()).Where(a => a.Id != parent.Id && a.OUCode.StartsWith(parent.OUCode));
                    }
                }
            }
        }


        #region OU辅助

        /// <summary>
        /// 获取父储位的下一个子储位OuCode
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private  async Task<string> GetNextChildCodeAsync(Storage parent)
        {
            var lastChild = await GetLastChildOrNullAsync(parent);
            if (lastChild != null)
            {
                return CalculateNextCode(lastChild.OUCode);
            }

            var parentCode = (parent == null ? "" : parent.OUCode);

            return AppendCode(parentCode, CreateCode(1));
        }



        /// <summary>
        /// 获取子储位的最后一个。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private async Task<Storage> GetLastChildOrNullAsync(Storage parent)
        {
            var children = await FindChildrenAsync(parent);

            return children.OrderBy(c => c.OUCode).LastOrDefault();
        }

        /// <summary>
        /// Creates code for given numbers.
        /// Example: if numbers are 4,2 then returns "000004.000002";
        /// </summary>
        /// <param name="numbers">Numbers</param>
        private string CreateCode(params int[] numbers)
        {
            if (numbers.IsNullOrEmpty())
            {
                return null;
            }

            return numbers.Select(number => number.ToString(new string('0', StorageOUConsts.CodeUnitLength))).JoinAsString(".");
        }

        /// <summary>
        /// Appends a child code to a parent code.
        /// Example: if parentCode = "000001", childCode = "000042" then returns "000001.000042".
        /// </summary>
        /// <param name="parentCode">Parent code. Can be null or empty if parent is a root.</param>
        /// <param name="childCode">Child code.</param>
        private string AppendCode(string parentCode, string childCode)
        {
            if (childCode.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(childCode), "childCode can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return childCode;
            }

            return parentCode + "." + childCode;
        }

        /// <summary>
        /// Gets relative code to the parent.
        /// Example: if code = "000019.000055.000001" and parentCode = "000019" then returns "000055.000001".
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="parentCode">The parent code.</param>
        private string GetRelativeCode(string code, string parentCode)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return code;
            }

            if (code.Length == parentCode.Length)
            {
                return null;
            }

            return code.Substring(parentCode.Length + 1);
        }

        /// <summary>
        /// Calculates next code for given code.
        /// Example: if code = "000019.000055.000001" returns "000019.000055.000002".
        /// </summary>
        /// <param name="code">The code.</param>
        private string CalculateNextCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var parentCode = GetParentCode(code);
            var lastUnitCode = GetLastUnitCode(code);

            return AppendCode(parentCode, CreateCode(Convert.ToInt32(lastUnitCode) + 1));
        }

        /// <summary>
        /// Gets the last unit code.
        /// Example: if code = "000019.000055.000001" returns "000001".
        /// </summary>
        /// <param name="code">The code.</param>
        private string GetLastUnitCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            return splittedCode[splittedCode.Length - 1];
        }

        /// <summary>
        /// Gets parent code.
        /// Example: if code = "000019.000055.000001" returns "000019.000055".
        /// </summary>
        /// <param name="code">The code.</param>
        private string GetParentCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            if (splittedCode.Length == 1)
            {
                return null;
            }

            return splittedCode.Take(splittedCode.Length - 1).JoinAsString(".");
        }

        #endregion

    }
}
