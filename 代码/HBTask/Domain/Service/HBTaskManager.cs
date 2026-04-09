using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using HBTaskModule.Domain.Shared;

namespace HBTaskModule.Domain
{
    public class HBTaskManager
    {
        /// <summary>
        /// 这里打算做一个多重任务的增删，应用的时候就不用分别根据不同的任务注入多个仓储，简化调用。
        /// </summary>
        /// <param name="basicTask"></param>
        /// <returns></returns>
        public T InsertTaskAsync<T>(T basicTask) where T : class, TaskBase
        {
            throw new ArgumentNullException();
        }

        public T GetTaskAsync<T>(Guid guid) where T : class, TaskBase
        {
            throw new ArgumentNullException();
        }

        public List<TaskBase> GetAllTaskAsync()
        {
            throw new ArgumentNullException();
        }

        public T UpdateTaskAsync<T>(T taskBase) where T: class, TaskBase
        {
            throw new Exception();
        }
    }   

}


