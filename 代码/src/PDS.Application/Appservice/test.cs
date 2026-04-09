
    using Microsoft.Extensions.DependencyInjection;
    using PDS.Domain.Entitys;
    using Quartz;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Volo.Abp.BackgroundWorkers;
    using Volo.Abp.BackgroundWorkers.Quartz;
    using Volo.Abp.DependencyInjection;
    using Volo.Abp.Domain.Entities;
    using Volo.Abp.Domain.Entities.Events;
    using Volo.Abp.EventBus;
    using Volo.Abp.Threading;
    using Volo.Abp.Uow;

namespace PDS.ProcessControl
{

    public class ProcessControl
    {
        //默认开启下，
    }


    /// <summary>
    /// 流程定义
    /// </summary>
    public class ProcessDefine : Entity<string>  //id是流程名称
    {
        //包含的流程
        List<ProcessNode> processStepInfos { get; set; }

        public void AddStep<TStep>(string StepDescribe)
        {
            processStepInfos.Add(new ProcessNode(StepDescribe, typeof(TStep)));
        }
    }

    /// <summary>
    /// 流程实例
    /// </summary>
    public class Process : Entity<Guid>
    {
        /// <summary>
        /// 依照的流程定义
        /// </summary>
        public ProcessDefine processDefine { get; protected set; }
        /// <summary>
        /// 流程的数据上下文
        /// </summary>
        public string ProcessDataContextId { get; protected set; }
        /// <summary>
        /// 流程控制器
        /// </summary>
        public string ProcessControllerId { get; protected set; }
        /// <summary>
        /// 默认状态
        /// </summary>
        public ProcessState BaseState { get; protected set; }
    }



    /// <summary>
    /// 执行各节点之间的流转业务
    /// </summary>
    public class ProcessFlowControl
    {
        //todo:实现默认节点间的流转。（有默认，可定制）
        //1.需要监控每一个Node的状态更改。
        //2.获取当前节点的流转条件（难点，判断条件的持久化，lamda作为一个类，每个条件一个类？不过这样写去来需要写很多类，而且以后用UI也无法写lamda），
        //3.判断是否符合条件，符合条件，且换到指定节点。
    }


    public class Service
    {
        public void test()
        {
            ProcessDefine processDefine = new ProcessDefine();
            processDefine.AddStep<ProcessNode_UpdateMes>("更新Mes");
        }
    }



    public enum PrcessNodeState
    {
        NotStart,
        Excuting,
        Finished,
        Canecel,
    }

    public enum ProcessState
    {
        NotStart,
        Excuting,
        Finished,
        Canecel,
    }


    /// <summary>
    /// 用来给数据库记录用到的步骤。通过反射，可以得到具体类，调用其Excute函数。
    /// </summary>
    public class ProcessNode : Entity<string>
    {

        public PrcessNodeState NodeState { get; set; }

        public string DetaiState { get; set; }

        public string ProcessDataContextId { get; set; }

        /// <summary>
        /// 当前节点类型
        /// </summary>
        public string ExcuterType { get; set; }   //TODO:这里假设每一个Node运行都是无状态的，所以只记录其类型名称就能反序列化。否则需要序列化整个实体对象。

        protected ProcessNode() { }
        public ProcessNode(string Name, Type type)
        {
            this.Id = Name;
            ExcuterType = type.FullName;
        }

        /// <summary>
        /// 下一个节点。节点数量比判断添加多1个；最后一个表示默认。
        /// </summary>
        public List<string> NextNodeTypeList { get; set; }     //TODO:这里假设每一个Node运行都是无状态的，所以只记录其类型名称就能反序列化。否则需要序列化整个实体对象。

        /// <summary>
        /// 判断条件
        /// </summary>
        public List<string> NextNodeTypeCondition { get; set; }

    }



    #region 具体的Node业务，所有的状态更改，都抛出状态更新事件。

    /// <summary>
    /// ProcessStep必须是无状态的，构造函数。
    /// </summary>
    public abstract class ProcessNodeExcuter
    {
        public virtual void Excute(ProcessNode processNodeInfo)
        {

        }
        //TODO,exucte的执行时机是什么？
        //方案1：最简单的方法是定时执行（例如0.5s），只要是已激活且没有完成的，都定时执行以下。但效率有点低。

        //方案2：定时执行（时间5s或者10s），同时通过暴露API或者捕获事件来触发。最好是可以通过简单地设置特性，实现这个功能。
        //执行的实例是写逻辑的，才知道是否需要定时执行，是否需要监控特定事件。 但因为数据是保存在ProcessNodeInfo对象上的，而且同一个事件，要ProcessNode匹配才行，所以应该还是不能特性实现API和时间捕获。
        //所以，如果用特性，还需要考虑匹配问题，和匹配性能。
        //所以，可能只能是为每个配套的写一个事件处理类货AppService，统一处理所有的同Node实例。
    }

    public class ProcessNode_UpdateMes : ProcessNodeExcuter
    {
        public override void Excute(ProcessNode processNodeInfo)
        {
            throw new NotImplementedException();
        }
    }

    #endregion





}


