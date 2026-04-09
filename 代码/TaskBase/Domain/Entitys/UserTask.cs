using System;
using Volo.Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskBaseModule.Domain.Shared;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;

namespace TaskBaseModule.Domain
{
    /// <summary>
    /// 通用的用户任务（用户交互），需要用户选择选项内的其中一项
    /// </summary>
    public class UserTask : TaskBaseAggregateRoot
    {

        protected UserTask() { }

        public UserTask(Guid Id, Guid parentTaskId, string parentTaskType):base(Id,parentTaskId,parentTaskType)
        {
            
        }
        /// <summary>
        /// 用户选项
        /// </summary>
        public List<Option> Options { get; private set; }
        /// <summary>
        /// 是否已经选择
        /// </summary>
        public bool Checked { get; private set; }
        /// <summary>
        /// 具体选中的选项
        /// </summary>
        public Guid? CheckedOption { get; private set; }
        /// <summary>
        /// 确定对应选项
        /// </summary>
        /// <param name="optionId"></param>
        /// <exception cref="BusinessException"></exception>
        public void Check(Guid optionId)
        {
            if (Options.FirstOrDefault(p => p.Id == optionId) != null)
            {
                CheckedOption = optionId;
                Checked = true;
            }
            else throw new BusinessException("InvalidOption");
        }
    }

    

    public class Option:Entity<Guid>
    {
        /// <summary>
        /// 所属客户任务Id
        /// </summary>
        public Guid UserTaskId { get; private set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Column(TypeName = "nvarchar(256)")]
        public string Describetion { get; private set; }
        /// <summary>
        /// 值
        /// </summary>
        [Column(TypeName = "nvarchar(64)")]
        public string Value { get; private set; }

    }
}

