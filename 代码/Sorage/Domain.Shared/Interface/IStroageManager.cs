using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.StorageModule.Domain.Shared
{
    public interface IStorageManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="StorageCode"></param>
        /// <param name="parent">父储位，如果是空，表示没有父储位</param>
        /// <param name="mapNodeName"></param>
        /// <param name="creatorId"></param>
        /// <param name="storageCategory">储位类别</param>
        /// <param name="storageName">储位名称</param>
        /// <param name="sizeMes"></param>
        /// <param name="isContainer"></param>
        /// <returns></returns>
        Task<Storage> CreatAsync(string StorageCode, Storage parent, string mapNodeName, Guid creatorId, string storageCategory, string storageName, string sizeMes,
            string appData1, string appData2, int startHeight,WareHouseArea wareHouseArea);

        Task<IQueryable<Storage>> FindStorageAndChildrenQueryableAsync(Storage parent, bool recursive = false, bool includeParent = false);
        /// <summary>
        /// 删除本储位，包含所有的子储位(todo,删除容易导致多线程问题，例如添加储位时同时删除子储位，ParentId需要建立数据库关联外键才可以，否则会出现问题）。如果设立外键，那么就不再支持非关系数据库。
        /// </summary>
        /// <param name="Storage"></param>
        /// <returns></returns>
        Task DeleteIncludeChildrenAsync(Storage Storage);
        /// <summary>
        /// 删除制定储位的子储位。
        /// </summary>
        /// <param name="parentStorage"></param>
        /// <returns></returns>
        Task DeleteChildrenAsync(Storage parentStorage);
        /// <summary>
        /// 这回修改目标储位和目标储位的下面的的所有储位。 对于子孙对象很多的，注意不能频繁调用。
        /// </summary>
        /// <param name="storageOUToMove"></param>
        /// <param name="NewFather"></param>
        /// <returns></returns>
        Task MoveAsync(Storage storageOUToMove, Storage NewParent);
        /// <summary>
        /// 找出对应的子类，可以制定是否递归
        /// </summary>
        /// <param name="parent">父对象Id，可以是空</param>
        /// <param name="recursive">是否递归</param>
        /// <returns></returns>
        Task<List<Storage>> FindChildrenAsync(Storage parent, bool recursive = false);
    }
}
