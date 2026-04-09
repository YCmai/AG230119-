using System.ComponentModel.DataAnnotations;
using System.Reflection;

public class RCS_UserTasks
{
    public int ID { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskStatuEnum taskStatus { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? executedTime { get; set; }


    /// <summary>
    /// 设备管理器任务ID
    /// </summary>
    public string? runTaskId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? startTime { get; set; }

    /// <summary>
    /// 是否要执行
    /// </summary>
    public bool executed { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? endTime { get; set; }


    /// <summary>
    /// 请求编号
    /// </summary>
    public string requestCode { get; set; }


    /// <summary>
    /// 任务类型
    /// </summary>
    public TaskType taskType { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public int priority { get; set; }


    /// <summary>
    /// 执行任务的agv
    /// </summary>
    public string? robotCode { get; set; } = "0";

    /// <summary>
    /// 起点
    /// </summary>
    public string? sourcePosition { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    public string? targetPosition { get; set; }

    public bool IsCancelled { get; set; }

    public string TaskStatusDisplayName
    {
        get
        {
            var fieldInfo = taskStatus.GetType().GetField(taskStatus.ToString());
            var attribute = (DisplayAttribute)fieldInfo.GetCustomAttribute(typeof(DisplayAttribute));
            return attribute != null ? attribute.Name : taskStatus.ToString();
        }
    }


    public string TaskTypeDisplayName
    {
        get
        {
            var fieldInfo = taskType.GetType().GetField(taskType.ToString());
            var attribute = (DisplayAttribute)fieldInfo.GetCustomAttribute(typeof(DisplayAttribute));
            return attribute != null ? attribute.Name : taskType.ToString();
        }
    }

    /// <summary>
    /// 任务类型枚举
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// 上料架到锅炉起升架
        /// </summary>
        [Display(Name = "上料架到锅炉起升架")]
        上料 = 1,

        /// <summary>
        /// 下料线到空储位
        /// </summary>
        [Display(Name = "下料线到空储位")]
        下料 = 2,

        /// <summary>
        /// 空储位到上料架
        /// </summary>
        [Display(Name = "空储位到上料架")]
        AssemblyToPlatingEmpty = 3
    }
   
}

public enum TaskStatuEnum
{
    /// <summary>
    /// 未执行
    /// </summary>
    None = -1,
    /// <summary>
    /// 不存在的任务号发生的洗车,当前可能是人工叉货并且把当前agv接入系统的情况下发生
    /// </summary>
    CarWash = 0,
    /// <summary>
    /// 任务开始
    /// </summary>
    TaskStart = 1,
    /// <summary>
    /// 进行参数反馈确认
    /// </summary>
    Confirm = 2,
    /// <summary>
    /// 确认执行agv
    /// </summary>
    ConfirmCar = 3,
    /// <summary>
    /// 取货中
    /// </summary>
    PickingUp = 4,
    /// <summary>
    /// 取货完成
    /// </summary>
    PickDown = 6,
    /// <summary>
    /// 卸货中
    /// </summary>
    Unloading = 8,
    /// <summary>
    /// 卸货完成
    /// </summary>
    UnloadDown = 10,
    /// <summary>
    /// 正常任务结束
    /// </summary>
    TaskFinish = 11,
    /// <summary>
    /// 任务被人为主动取消,当前车还没有取到货，任务直接取消
    /// </summary>
    Canceled = 30,
    /// <summary>
    /// 任务被人为主动取消，但当前agv已载货，触发一个洗车任务
    /// </summary>
    CanceledWashing = 31,
    /// <summary>
    /// 被人为触发的洗车任务AGV已执行完成
    /// </summary>
    CanceledWashFinish = 32,
    /// <summary>
    /// 到达取货口时，agv发现取货路线异常，取货失败--agv主动发起取消，当前还没有取到货，任务直接取消
    /// </summary>
    RedirectRequest = 33,
    /// <summary>
    /// 任务中有无效取货点 --系统主动发起取消当前任务，还没派车
    /// </summary>
    InvalidUp = 49,
    /// <summary>
    /// 任务中有无效卸货点无效  --系统主动发起取消当前任务，还没派车
    /// </summary>
    InvalidDown = 50,
    /// <summary>
    /// 到达卸货口时，agv发现卸货路线异常，卸货失败，主动请求洗车，转卸到别的点 --avg主动发起取消
    /// </summary>
    OrderAgv = 52,
    /// <summary>
    /// 卸货口路线异常任务执行结束
    /// </summary>
    OrderAgvFinish = 53
}



