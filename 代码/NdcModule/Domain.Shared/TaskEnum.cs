using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciModule.Domain.Shared
{
    public enum TaskTypeEnum
    {
        /// <summary>
        /// 入库
        /// </summary>
        In = 1,
        /// <summary>
        /// 出库
        /// </summary>
        Out = 2,
        /// <summary>
        /// 移库
        /// </summary>
        Move = 3,
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
        CanceledWashing =31,
        /// <summary>
        /// 被人为触发的洗车任务AGV已执行完成
        /// </summary>
        CanceledWashFinish=32,
        /// <summary>
        /// 到达取货口时，agv发现取货路线异常，取货失败--agv主动发起取消，当前还没有取到货，任务直接取消
        /// </summary>
        RedirectRequest =33,
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
        OrderAgv=52,
        /// <summary>
        /// 卸货口路线异常任务执行结束
        /// </summary>
        OrderAgvFinish = 53
    }

    public enum ReplyTaskState
    {
        /// <summary>
        /// 同步 ndc 任务开始
        /// </summary>
        TaskStart = 1,
        /// <summary>
        /// 同步 ndc 参数确认
        /// </summary>
        Confirm = 2,
        /// <summary>
        /// 同步 ndc,确认执行agv
        /// </summary>
        ConfirmCar = 3,
        /// <summary>
        /// 同步 ndc,取货中
        /// </summary>
        PickingUp = 4,
        /// <summary>
        /// 同步 ndc,取货完成
        /// </summary>
        PickDown = 6,
        /// <summary>
        /// 同步 ndc,卸货中
        /// </summary>
        Unloading = 8,
        /// <summary>
        /// 同步 ndc ,卸货完成
        /// </summary>
        UnloadDown = 10,
        /// <summary>
        /// 同步 ndc,可以正常任务结束
        /// </summary>
        TaskFinish = 11,
        /// <summary>
        /// 回复确认取消任务
        /// </summary>
        ConfirmCancellation = 143,
        /// <summary>
        /// 确认agv洗车请求，并且把洗车的卸货点 ack1 发送给agv
        /// </summary>
        ConfirmWashing = 254,
        /// <summary>
        /// 确认agv 重定向请求
        /// </summary>
        ConfirmRedirection = 142,
        /// <summary>
        /// 未知货物在agv叉臂上，但当前agv也去做任务了，到达取料口时才发现自己叉臂上有料，会触发当前异常。此时agv调度自动转发任务给别的agv执行，当前agv执行洗车，ack1为洗车站点
        /// </summary>
        ConfirmUnknown = 142,
        /// <summary>
        /// 同步NDC 可以结束
        /// </summary>
        End=153
    }

    public enum TaskState
    {
        /// <summary>
        /// 等待分配索引任务
        /// </summary>
        Wait=0,
        /// <summary>
        /// 已回收任务
        /// </summary>
        Recycled = -1
    }

    public enum PriorityEnum
    {
        /// <summary>
        /// 默认
        /// </summary>
        None = 0,
        /// <summary>
        /// 优先处理
        /// </summary>
        ExecuteNow = 1
    }


}
