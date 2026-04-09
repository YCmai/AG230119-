using AciModule.Domain;
using AciModule.Domain.Entitys;
//using AciModule.Domain.Queue;
using AciModule.Domain.Service;
using AciModule.Domain.Shared;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;

using WMS.StorageModule.Domain;

namespace AciModule.Applicaion.EventHandle
{
    /// <summary>
    /// ACI数据事件处理器
    /// </summary>
    /// <remarks>
    /// 负责处理所有ACI相关的事件，包括订单开始、参数确认、装卸货、任务完成等状态的处理
    /// 实现 ILocalEventHandler<AciDataEventArgs> 接口以处理本地事件
    /// </remarks>
    public class AciDataEventHandle : ILocalEventHandler<AciDataEventArgs>, ITransientDependency
    {
        private readonly AciAppManager _aciAppManager;
        private readonly IRepository<NdcTask_Moves, Guid> _ndcTask;
        private readonly ILogger<AciDataEventHandle> _logger;
        private readonly IRepository<RCS_IOAGV_Tasks> _rcs_IOAGV_Tasks;
        private readonly IRepository<RCS_UserTasks> _rcs_RCS_UserTasks;
        private readonly IRepository<RCS_IODevices> _rcs_IODevices;
        private readonly IRepository<RCS_IOSignals> _rcs_IOSignals;
        private readonly IRepository<RCS_Locations> _rcs_Locations;
        static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public AciDataEventHandle(AciAppManager aciAppManager, IRepository<RCS_UserTasks> rcs_RCS_UserTasks, IRepository<NdcTask_Moves, Guid> NdcTask, ILogger<AciDataEventHandle> logger, IRepository<RCS_IOAGV_Tasks> rcs_IOAGV_Tasks, IRepository<RCS_IODevices> rcs_IODevices, IRepository<RCS_IOSignals> rcs_IOSignals, IRepository<RCS_Locations> rcs_Locations)
        {
            _aciAppManager = aciAppManager;
            _ndcTask = NdcTask;
            _logger = logger;
            _rcs_IOAGV_Tasks=rcs_IOAGV_Tasks;
            _rcs_IODevices = rcs_IODevices;
            _rcs_IOSignals= rcs_IOSignals;
            _rcs_Locations=rcs_Locations;
            _rcs_RCS_UserTasks=rcs_RCS_UserTasks;

        }

        /// <summary>
        /// 处理ACI事件的主要方法
        /// </summary>
        /// <param name="e">ACI数据事件参数</param>
        /// <remarks>
        /// 使用信号量确保事件处理的线程安全
        /// 根据事件类型分发到不同的处理方法
        /// </remarks>
        [UnitOfWork]
        public async Task HandleEventAsync(AciDataEventArgs e)
        {
            await _semaphore.WaitAsync();

            try
            {
                switch (e.AciData.DataType)
                {
                    case MessageType.OrderEvent:
                        Task.WaitAll(HandleOrderEvent(e));
                        break;
                }
            }
            finally
            {
                _semaphore.Release();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 处理订单事件
        /// </summary>
        /// <param name="e">ACI数据事件参数</param>
        /// <remarks>
        /// 解析订单事件数据并根据不同的事件类型进行相应处理
        /// 维护事件历史记录
        /// </remarks>
        /// <summary>
        /// 处理订单事件
        /// </summary>
        /// <param name="e">ACI数据事件参数</param>
        /// <remarks>
        /// 解析订单事件数据并根据不同的事件类型进行相应处理
        /// 维护事件历史记录
        /// </remarks>
        private async Task HandleOrderEvent(AciDataEventArgs e)
        {
            try
            {
                OrderEventAciData? data = e.AciData as OrderEventAciData;
                AciEvent ev = new AciEvent()
                {
                    Type = (AciHostEventTypeEnum)data.MagicCode1,
                    Parameter1 = data.MagicCode2,
                    Parameter2 = data.MagicCode3,
                    Index = data.OrderIndex
                };
                var ndcTask = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
                switch (ev.Type)
                {
                    case AciHostEventTypeEnum.OrderStart:
                        
                        try
                        {
                            _logger.LogCritical($"OrderStart");
                            await HandleOrderStartEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理订单开始事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.ParameterCheck:
                        try
                        {
                            _logger.LogCritical($"ParameterCheck0");
                          
                            _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.Confirm, 0, 0);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理参数检查事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.MoveToLoad:
                        try
                        {
                            _logger.LogCritical($"MoveToLoad");
                            await HandleMoveToLoadEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理移动到装货点事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.LoadHostSyncronisation:
                        try
                        {
                            _logger.LogCritical($"LoadHostSyncronisation");
                            var vehicleAtLoad0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
                            if (vehicleAtLoad0 != null)
                            {
                                var userTaskModel = await _rcs_RCS_UserTasks.FirstOrDefaultAsync(x => x.requestCode == vehicleAtLoad0.SchedulTaskNo);
                                if (userTaskModel != null)
                                {
                                    _logger.LogCritical($"ParameterCheck1");

                                    // 处理所有上料库位的信号逻辑
                                    string sourcePosition = userTaskModel.sourcePosition ?? "";
                                    try
                                    {

                                        if (sourcePosition.Contains("产品下线取货1号"))
                                        {
                                            _logger.LogCritical($"ParameterCheck2");

                                            var responseSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                                             x.DeviceId == 29 &&
                                             x.Address == "DI1");
                                            if (responseSignal != null && responseSignal.Value != 1)
                                            {
                                                _logger.LogCritical($"ParameterCheck3");

                                                if (userTaskModel.IsCancelled)
                                                {
                                                    _logger.LogCritical($"ParameterCheck4");
                                                    return;
                                                }
                                                else
                                                {
                                                    _logger.LogCritical($"ParameterCheck5");
                                                    userTaskModel.IsCancelled = true;
                                                    await _rcs_RCS_UserTasks.UpdateAsync(userTaskModel);
                                                    return;
                                                }
                                            }

                                        }

                                        if (sourcePosition.Contains("产品下线取货2号"))
                                        {
                                            var responseSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                                            x.DeviceId == 30 &&
                                            x.Address == "DI1");
                                            if (responseSignal != null && responseSignal.Value != 1)
                                            {
                                                if (userTaskModel.IsCancelled)
                                                {
                                                    return;
                                                }
                                                else
                                                {
                                                    userTaskModel.IsCancelled = true;
                                                    await _rcs_RCS_UserTasks.UpdateAsync(userTaskModel);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        _logger.LogError($"任务 {ev.Parameter2} 在二次检测异常{ex.Message}");

                                    }
                                }
                            }
                            await HandleLoadHostSyncronisationEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理装货同步事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.LoadingHostSyncronisation:
                        try
                        {
                            await HandleLoadingHostSyncronisationEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理装货完成同步事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.UnloadHostSyncronisation:
                        try
                        {
                            await HandleUnloadHostSyncronisationEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理卸货同步事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.UnloadingHostSyncronisation:
                        try
                        {
                            await HandleUnloadingHostSyncronisationEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理卸货完成同步事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.OrderFinish:
                        try
                        {
                            await HandleOrderFinishEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理订单完成事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.End:
                        try
                        {
                            await HandleEndEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理结束事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.CancelRequest:
                        try
                        {
                            await HandleCancelRequestEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理取消请求事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.Cancel:
                        try
                        {
                            if (ev.Parameter2 == (int)TaskStatuEnum.CarWash)
                            {
                                _aciAppManager.SendHostAcknowledge(null, ev.Index, 255, 0, 0);
                                break;
                            }
                            _aciAppManager.SendHostAcknowledge(null, ev.Index, 255, 0, 0);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理取消事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.CarrierConnected:
                        try
                        {
                            // 当前没有实现的方法
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理载具连接事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.Redirect:
                        try
                        {
                            _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmWashing, 400, 0);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理重定向事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.OrderTransform:
                        try
                        {
                            await HandleOrderTransformEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理订单转换事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.InvalidDeliverStation:
                        try
                        {
                            await HandleInvalidDeliverStationEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理无效卸货站点事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.OrderCancel:
                        try
                        {
                            await HandleOrderCancelEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理订单取消事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.OrderAgv:
                        try
                        {
                            await HandleOrderAgvEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理订单AGV事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.RedirectRequestFetch:
                        try
                        {
                            await HandleRedirectRequestFetchEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理重定向请求取货事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.RedirectOrNot:
                        try
                        {
                            await HandleRedirectOrNotEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理是否重定向事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.CarWashRequest:
                        try
                        {
                            _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmUnknown, 1160, 0);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理洗车请求事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.ResetStart:
                        try
                        {
                            _logger.LogCritical($"ResetStart任务编号{ev.Index}系统重启。");
                            await HandleResetStartEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理重启开始事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                    case AciHostEventTypeEnum.ResetStart2:
                        try
                        {
                            _logger.LogCritical($"ResetStart2任务编号{ev.Index}系统重启。");
                            await HandleResetStartEvent(ev);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"处理重启开始2事件时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                        }
                        break;
                }

                if (ev.Type != AciHostEventTypeEnum.HostSync)
                {
                    try
                    {
                        _aciAppManager.AciEventAdd(ev);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"添加ACI事件历史记录时发生错误: {ex.Message}, 堆栈: {ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"HandleOrderEvent整体处理出现异常: {ex.Message}, 堆栈: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 处理订单开始事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为开始状态
        /// 设置订单索引
        /// </remarks>
        private async Task HandleOrderStartEvent(AciEvent ev)
        {
            var vehicleAtLoad0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
            if (vehicleAtLoad0 != null) {
                var userTaskModel = await _rcs_RCS_UserTasks.FirstOrDefaultAsync(x => x.requestCode == vehicleAtLoad0.SchedulTaskNo);
                if (userTaskModel!=null)
                {
                    // 处理所有上料库位的信号逻辑
                    string sourcePosition = userTaskModel.sourcePosition ?? "";
                    int deviceId = 0;
                    string ip = "";
                    string signalAddress = "";

                    try
                    {
                        // 解析库位号码和设置相应的设备ID、IP和信号地址
                        if (sourcePosition.Contains("号上料库位"))
                        {
                            // 提取库位号码
                            string positionNumberStr = sourcePosition.Replace("号上料库位", "");
                            if (int.TryParse(positionNumberStr, out int positionNumber))
                            {
                                // 根据库位号设置对应参数
                                if (positionNumber >= 1 && positionNumber <= 6)
                                {
                                    deviceId = 26;
                                    ip = "192.168.200.121";
                                    signalAddress = $"DO{positionNumber}";
                                }
                                else if (positionNumber >= 7 && positionNumber <= 12)
                                {
                                    deviceId = 27;
                                    ip = "192.168.200.122";
                                    signalAddress = $"DO{positionNumber - 6}"; // 7号库位用DO1，8号库位用DO2，以此类推
                                }

                                // 如果成功解析了参数，执行信号处理
                                if (deviceId > 0 && !string.IsNullOrEmpty(signalAddress))
                                {
                                    // 检查DO信号状态
                                    var doSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                                        x.DeviceId == deviceId &&
                                        x.Address == signalAddress);

                                    if (doSignal != null && doSignal.Value != 1)
                                    {
                                        // 检查是否存在未执行的DO任务
                                        var existingDOTask = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                                            x.SignalAddress == signalAddress &&
                                            x.TaskId == vehicleAtLoad0.SchedulTaskNo &&
                                            x.DeviceIP == ip &&
                                            x.Value == true);

                                        if (existingDOTask == null)
                                        {
                                            // 创建DO持续信号任务
                                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                                            {
                                                TaskType = "SetSignal",
                                                CompletedTime = null,
                                                CreatedTime = DateTime.Now,
                                                TaskId = vehicleAtLoad0.SchedulTaskNo,
                                                Status = "Pending",
                                                DeviceIP = ip,
                                                LastUpdatedTime = DateTime.Now,
                                                SignalAddress = signalAddress,
                                                Value = true
                                            });

                                            _logger.LogCritical($"任务 {ev.Parameter2} 在{sourcePosition}创建{signalAddress}持续信号任务");
                                        }
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                       
                        _logger.LogError($"任务 {ev.Parameter2} 在二次检测异常{ex.Message}");

                    }
                }
            }

            var ndcTasks = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.None);
            if (ndcTasks != null)
            {
                ndcTasks.SetStatus(TaskStatuEnum.TaskStart);
                ndcTasks.SetOrderIndex(ev.Index);
                await _ndcTask.UpdateAsync(ndcTasks);
                return;
            }
            var ndcTasksInProgress = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.TaskStart);
            if (ndcTasksInProgress != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.TaskStart, ndcTasksInProgress.PickupHeight, ndcTasksInProgress.UnloadHeight);
                return;
            }
        }

        /// <summary>
        /// 处理移动到装货点事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为确认车辆状态
        /// 设置订单索引
        /// </remarks>
        private async Task HandleMoveToLoadEvent(AciEvent ev)
        {

            //如果是1号铸锭下线取货任务，需要二次采集DI1信号，如果是就放行。

            var vehicleAtLoad0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
            if (vehicleAtLoad0 != null)
            {
                if (vehicleAtLoad0.TaskType ==2)
                {
                    // 根据装货点判断设备ID
                    int deviceId = 28; // 默认设备ID

                    if (vehicleAtLoad0.PickupSite == 15)
                    {
                        deviceId = 29;
                    }
                    else if (vehicleAtLoad0.PickupSite == 14)
                    {
                        deviceId = 30;
                    }

                    // 检测DI1信号
                    var responseSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                        x.DeviceId == deviceId &&
                        x.Address == "DI1");

                    if (responseSignal != null && responseSignal.Value != 1)
                    {
                        _logger.LogError($"任务 {ev.Parameter2} 在装货点 {vehicleAtLoad0.PickupSite} 检测到不允许信号");
                        return;

                    }
                }
            }

            var move = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.TaskStart);
            if (move != null)
            {
                move.SetStatus(TaskStatuEnum.ConfirmCar, ev.Parameter1);
                move.SetOrderIndex(ev.Index);
                await _ndcTask.UpdateAsync(move);
            }
        }

        /// <summary>
        /// 处理装货同步事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为正在装货
        /// 发送装货高度和深度参数
        /// </remarks>
        private async Task HandleLoadHostSyncronisationEvent(AciEvent ev)
        {
            //如果是1号铸锭下线任务，DO1（持续）进行产线锁定
            // 获取当前任务信息
            try
            {
                var ndcTaskModel = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
                if (ndcTaskModel == null) return;

                if (ndcTaskModel.TaskType == 2)
                {

                    // 根据装货点判断设备ID
                    int deviceId = 28; // 默认设备ID
                    var ip = "192.168.200.124";
                    if (ndcTaskModel.PickupSite == 15)
                    {
                        deviceId = 29;
                        ip = "192.168.200.124";
                    }
                    else if (ndcTaskModel.PickupSite == 14)
                    {
                        deviceId = 30;
                        ip = "192.168.200.125";
                    }

                    // 检查DO1信号状态
                    var do1Signal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                        x.DeviceId == deviceId &&
                        x.Address == "DO1");

                    if (do1Signal != null && do1Signal.Value != 1)
                    {
                        // 检查是否存在未执行的DO1任务
                        var existingDO1Task = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                            x.SignalAddress == "DO1" &&
                            x.TaskId == ndcTaskModel.SchedulTaskNo && x.DeviceIP == ip && x.Value == true);



                        if (existingDO1Task == null)
                        {
                            // 创建DO1持续信号任务
                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                            {
                                TaskType = "SetSignal",
                                CompletedTime = null,
                                CreatedTime = DateTime.Now,
                                TaskId = ndcTaskModel.SchedulTaskNo,
                                Status = "Pending",
                                DeviceIP = ip,
                                LastUpdatedTime = DateTime.Now,
                                SignalAddress = "DO1",
                                Value = true
                            });

                            _logger.LogCritical($"任务 {ev.Parameter2} 创建DO1持续信号任务");
                        }

                        return;
                    }
                }


                var load0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.ConfirmCar);
                if (load0 != null)
                {
                    load0.SetStatus(TaskStatuEnum.PickingUp);
                    await _ndcTask.UpdateAsync(load0);
                    return;
                }

                var load1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.PickingUp);
                if (load1 != null)
                {
                    var upHeight = load1.PickupHeight == 0 ? 0 : load1.PickupHeight;
                    var upDepth = 0;

                    _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.PickingUp, upHeight, upDepth);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError($"HandleLoadHostSyncronisationEvent任务报错异常-{ex.Message}");
            }
        }

        /// <summary>
        /// 处理装货完成同步事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为装货完成
        /// 处理安全交互信号
        /// </remarks>
        private async Task HandleLoadingHostSyncronisationEvent(AciEvent ev)
        {
            //如果是1号铸锭下线任务，触发DO2（3S脉冲），释放DO1
            var ndcTaskModel = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
            if (ndcTaskModel == null) return;

            if (ndcTaskModel.TaskType == 2)
            {
                try
                {
                    // 根据装货点判断设备ID
                    int deviceId = 28; // 默认设备ID
                    var ip = "192.168.200.124";
                    if (ndcTaskModel.PickupSite == 15)
                    {
                        deviceId = 29;
                        ip = "192.168.200.124";
                    }
                    else if (ndcTaskModel.PickupSite == 14)
                    {
                        deviceId = 30;
                        ip = "192.168.200.125";
                    }

                    // 2. 检查DO1信号状态
                    var do1Signal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                        x.DeviceId == deviceId &&
                        x.Address == "DO1");

                    if (do1Signal != null && do1Signal.Value == 1)
                    {
                        // 检查是否存在未执行的DO1释放任务
                        var existingDO1Task = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                            x.SignalAddress == "DO1" &&
                            x.TaskId == ndcTaskModel.SchedulTaskNo && x.DeviceIP == ip && x.Value == false);

                        if (existingDO1Task == null)
                        {
                            // 创建释放DO1信号任务
                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                            {
                                TaskType = "SetSignal",
                                CompletedTime = null,
                                CreatedTime = DateTime.Now,
                                TaskId = ndcTaskModel.SchedulTaskNo,
                                Status = "Pending",
                                DeviceIP = ip,
                                LastUpdatedTime = DateTime.Now,
                                SignalAddress = "DO1",
                                Value = false
                            });

                            _logger.LogCritical($"任务 {ev.Parameter2} 创建释放DO1信号任务");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"处理装货完成同步事件时发生错误: {ex.Message}");
                }
            }

            var vehicleAtLoad0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
            if (vehicleAtLoad0 != null)
            {
                var userTaskModel = await _rcs_RCS_UserTasks.FirstOrDefaultAsync(x => x.requestCode == vehicleAtLoad0.SchedulTaskNo);
                if (userTaskModel != null)
                {
                    // 处理所有上料库位的信号逻辑
                    string sourcePosition = userTaskModel.sourcePosition ?? "";
                    int deviceId = 0;
                    string ip = "";
                    string signalAddress = "";

                    // 解析库位号码和设置相应的设备ID、IP和信号地址
                    if (sourcePosition.Contains("号上料库位"))
                    {
                        // 提取库位号码
                        string positionNumberStr = sourcePosition.Replace("号上料库位", "");
                        if (int.TryParse(positionNumberStr, out int positionNumber))
                        {
                            // 根据库位号设置对应参数
                            if (positionNumber >= 1 && positionNumber <= 6)
                            {
                                deviceId = 26;
                                ip = "192.168.200.121";
                                signalAddress = $"DO{positionNumber}";
                            }
                            else if (positionNumber >= 7 && positionNumber <= 12)
                            {
                                deviceId = 27;
                                ip = "192.168.200.122";
                                signalAddress = $"DO{positionNumber - 6}"; // 7号库位用DO1，8号库位用DO2，以此类推
                            }

                            // 如果成功解析了参数，执行信号处理
                            if (deviceId > 0 && !string.IsNullOrEmpty(signalAddress))
                            {
                                // 检查DO信号状态
                                var doSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                                    x.DeviceId == deviceId &&
                                    x.Address == signalAddress);

                                if (doSignal != null && doSignal.Value == 1)
                                {
                                    // 检查是否存在未执行的DO任务
                                    var existingDOTask = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                                        x.SignalAddress == signalAddress &&
                                        x.TaskId == vehicleAtLoad0.SchedulTaskNo &&
                                        x.DeviceIP == ip &&
                                        x.Value == false);

                                    if (existingDOTask == null)
                                    {
                                        // 创建DO持续信号任务
                                        await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                                        {
                                            TaskType = "SetSignal",
                                            CompletedTime = null,
                                            CreatedTime = DateTime.Now,
                                            TaskId = vehicleAtLoad0.SchedulTaskNo,
                                            Status = "Pending",
                                            DeviceIP = ip,
                                            LastUpdatedTime = DateTime.Now,
                                            SignalAddress = signalAddress,
                                            Value = false
                                        });

                                        _logger.LogCritical($"任务 {ev.Parameter2} 在{sourcePosition}创建{signalAddress}持续信号任务");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (ev.Parameter2 == (int)TaskStatuEnum.CarWash)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.PickDown, 0, 0);
                return;
            }

            var loadDone0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.PickingUp);
            if (loadDone0 != null)
            {
                loadDone0.SetStatus(TaskStatuEnum.PickDown);

                await _ndcTask.UpdateAsync(loadDone0);
                return;
            }

            var loadDone1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.PickDown);
            if (loadDone1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.PickDown, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理卸货同步事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为正在卸货
        /// 发送卸货高度和深度参数
        /// 处理站台交互逻辑：
        /// 1. 创建DO1请求进入站台的IO任务（持续信号）
        /// 2. 检测DI1信号（起升架允许进入反馈）
        /// 3. 确认DI1信号后允许AGV进入
        /// </remarks>
        private async Task HandleUnloadHostSyncronisationEvent(AciEvent ev)
        {
            // 获取当前任务信息
            var ndcTaskModel = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
            if (ndcTaskModel == null) return;

            // 如果是起升架卸货任务，需要进行站台交互
            if (ndcTaskModel.TaskType == 1) // 假设1代表起升架卸货任务类型
            {
                try
                {
                    // 1. 检查DO1信号状态
                    var requestSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x => 
                        x.DeviceId == 28 && x.Address == "DO1");
                    
                    if (requestSignal != null && requestSignal.Value != 1)
                    {
                        // 检查是否存在未执行的相同IO任务
                        var existingTask = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                            x.SignalAddress == "DO1" &&  x.TaskId == ndcTaskModel.SchedulTaskNo && x.DeviceIP== "192.168.200.123" && x.Value ==true);

                        if (existingTask == null)
                        {
                            // 创建新的IO任务来设置DO1信号
                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                            {
                                TaskType = "SetSignal",
                                CompletedTime = null,
                                CreatedTime = DateTime.Now,
                                TaskId = ndcTaskModel.SchedulTaskNo,
                                Status = "Pending",
                                DeviceIP = "192.168.200.123",
                                LastUpdatedTime = DateTime.Now,
                                SignalAddress = "DO1",
                                Value = true
                            });

                             _logger.LogCritical($"任务 {ev.Parameter2} 创建DO1信号请求任务");
                        }
                        
                       
                    }

                    // 2. 等待并检测DI1信号（起升架允许进入反馈）
                    var responseSignal = await _rcs_IOSignals.FirstOrDefaultAsync(x =>
                        x.DeviceId == 28 && x.Address == "DI1");

                    if (responseSignal != null && responseSignal.Value == 1)
                    {
                        // 3. 检测到允许信号，继续执行卸货流程
                        _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.Unloading,
                            ndcTaskModel.UnloadHeight, 0);

                        // 更新任务状态
                        if (ndcTaskModel.TaskStatus == TaskStatuEnum.PickDown)
                        {
                            ndcTaskModel.SetStatus(TaskStatuEnum.Unloading);
                            await _ndcTask.UpdateAsync(ndcTaskModel);
                        }
                    }
                    else
                    {
                        _logger.LogError($"任务 {ev.Parameter2} 等待起升架允许进入信号超时");
                    }


                }
                catch (Exception ex)
                {
                     _logger.LogError($"处理站台交互信号时发生错误: {ex.Message}");
                }
            }
            else
            {
                // 处理普通卸货任务的原有逻辑
                if (ev.Parameter2 == (int)TaskStatuEnum.CarWash)
                {
                    _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.Unloading, 0, 0);
                    return;
                }

                var unoad0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.PickDown);
                if (unoad0 != null)
                {
                    unoad0.SetStatus(TaskStatuEnum.Unloading);
                    await _ndcTask.UpdateAsync(unoad0);
                }

                var unoad1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.Unloading);
                if (unoad1 != null)
                {
                    var doneHeight = unoad1.UnloadHeight == 0 ? 0 : unoad1.UnloadHeight;
                    var upDepth = 0;
                    _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.Unloading, doneHeight, upDepth);
                }

                var unoad2 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.OrderAgv);
                if (unoad2 != null)
                {
                    var doneHeight = unoad2.UnloadHeight == 0 ? 0 : unoad2.UnloadHeight;
                    var depth = unoad2.UnloadDepth == 0 ? 0 : unoad2.UnloadDepth;
                    _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.Unloading, doneHeight, depth);
                    return;
                }

                var unoad3 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.CanceledWashing);
                if (unoad3 != null)
                {
                    var doneHeight = unoad3.UnloadHeight == 0 ? 0 : unoad3.UnloadHeight;
                    var depth = unoad3.UnloadDepth == 0 ? 0 : unoad3.UnloadDepth;
                    _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.Unloading, doneHeight, depth);
                    return;
                }
            }
        }

        /// <summary>
        /// 处理卸货完成同步事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为卸货完成
        /// 处理站台交互信号：
        /// 1. 发送DO2脉冲信号（3秒）
        /// 2. 释放DO1持续信号
        /// </remarks>
        private async Task HandleUnloadingHostSyncronisationEvent(AciEvent ev)
        {
            // 获取当前任务信息
            var ndcTaskModel = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2);
            if (ndcTaskModel == null) return;

            // 如果是起升架卸货任务，需要进行站台交互
            if (ndcTaskModel.TaskType == 1) // 假设1代表起升架卸货任务类型
            {
                try
                {
                    // 1. 检查DO2信号状态
                    var do2Signal = await _rcs_IOSignals.FirstOrDefaultAsync(x => 
                        x.DeviceId == 28 && x.Address == "DO2");
                    
                    if (do2Signal != null && do2Signal.Value != 1)
                    {
                        // 检查是否存在未执行的DO2任务
                        var existingDO2Task = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                            x.SignalAddress == "DO2" &&
                            x.DeviceIP == "192.168.200.123" && x.Value==true && x.TaskId == ndcTaskModel.SchedulTaskNo);

                        if (existingDO2Task == null)
                        {
                            // 创建DO2脉冲信号任务
                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                            {
                                TaskType = "SetSignal",
                                CompletedTime = null,
                                CreatedTime = DateTime.Now,
                                TaskId = ndcTaskModel.SchedulTaskNo,
                                Status = "Pending",
                                DeviceIP = "192.168.200.123",
                                LastUpdatedTime = DateTime.Now,
                                SignalAddress = "DO2",
                                Value = true
                            });

                          
                            _logger.LogCritical($"任务 {ev.Parameter2} 创建DO2脉冲信号任务");
                        }
                    }

                    // 2. 检查DO1信号状态
                    var do1Signal = await _rcs_IOSignals.FirstOrDefaultAsync(x => 
                        x.DeviceId == 28 && x.Address == "DO1");
                    
                    if (do1Signal != null && do1Signal.Value == 1)
                    {
                        // 检查是否存在未执行的DO1释放任务
                        var existingDO1Task = await _rcs_IOAGV_Tasks.FirstOrDefaultAsync(x =>
                            x.SignalAddress == "DO1" &&
                            x.DeviceIP == "192.168.200.123" &&
                            x.TaskId == ndcTaskModel.SchedulTaskNo && x.Value==false);

                        if (existingDO1Task == null)
                        {
                            // 创建释放DO1信号任务
                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                            {
                                TaskType = "SetSignal",
                                CompletedTime = null,
                                CreatedTime = DateTime.Now,
                                TaskId = ndcTaskModel.SchedulTaskNo,
                                Status = "Pending",
                                DeviceIP = "192.168.200.123",
                                LastUpdatedTime = DateTime.Now,
                                SignalAddress = "DO1",
                                Value = false
                            });

                             _logger.LogCritical($"任务 {ev.Parameter2} 创建释放DO1信号任务");


                            await _rcs_IOAGV_Tasks.InsertAsync(new RCS_IOAGV_Tasks
                            {
                                TaskType = "SetSignal",
                                CompletedTime = null,
                                CreatedTime = DateTime.Now,
                                TaskId = ndcTaskModel.SchedulTaskNo,
                                Status = "Pending",
                                DeviceIP = "192.168.200.123",
                                LastUpdatedTime = DateTime.Now,
                                SignalAddress = "DO2",
                                Value = false
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                     _logger.LogError($"处理站台交互信号时发生错误: {ex.Message}");
                }
            }

            // 处理原有的卸货完成逻辑
            if (ev.Parameter2 == (int)TaskStatuEnum.CarWash)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.UnloadDown, 0, 0);
                return;
            }

            var unloadDone0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.Unloading);
            if (unloadDone0 != null)
            {
                unloadDone0.SetStatus(TaskStatuEnum.UnloadDown);
                await _ndcTask.UpdateAsync(unloadDone0);
                return;
            }

            var unloadDone1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.UnloadDown);
            if (unloadDone1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.UnloadDown, 0, 0);
                return;
            }

            var unloadDone2 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.OrderAgv);
            if (unloadDone2 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.UnloadDown, 0, 0);
                return;
            }

            var unloadDone3 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.CanceledWashing);
            if (unloadDone3 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.UnloadDown, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理订单完成事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为完成状态
        /// 处理不同类型任务的完成逻辑
        /// </remarks>
        private async Task HandleOrderFinishEvent(AciEvent ev)
        {
            if (ev.Parameter2 == (int)TaskStatuEnum.CarWash)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.TaskFinish, 0, 0);
                return;
            }
            var finish0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.UnloadDown);
            if (finish0 != null)
            {
                finish0.SetStatus(TaskStatuEnum.TaskFinish);
                _ndcTask.UpdateAsync(finish0, true).Wait();
            }

            var finish1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.OrderAgv);

            if (finish1 != null)
            {
                finish1.SetStatus(TaskStatuEnum.OrderAgvFinish);
                _ndcTask.UpdateAsync(finish1, true).Wait();
            }

            var finish2 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.CanceledWashing);

            if (finish2 != null)
            {
                finish2.SetStatus(TaskStatuEnum.CanceledWashFinish);
                _ndcTask.UpdateAsync(finish2, true).Wait();
            }

            _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.TaskFinish, 0, 0);
        }

        /// <summary>
        /// 处理无效卸货站点事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为无效卸货点
        /// 发送确认取消指令
        /// </remarks>
        private async Task HandleInvalidDeliverStationEvent(AciEvent ev)
        {
            var invalidDown0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus != TaskStatuEnum.InvalidDown);
            if (invalidDown0 != null)
            {
                invalidDown0.SetStatus(TaskStatuEnum.InvalidDown, ev.Parameter1);
                await _ndcTask.UpdateAsync(invalidDown0);
                return;
            }

            var invalidDown1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.InvalidDown);
            if (invalidDown1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmCancellation, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理订单取消事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为已取消
        /// 处理洗车重定向确认
        /// </remarks>
        private async Task HandleOrderCancelEvent(AciEvent ev)
        {
            var orderCancl0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus != TaskStatuEnum.CanceledWashing);

            if (orderCancl0 != null)
            {
                orderCancl0.SetStatus(TaskStatuEnum.CanceledWashing);
                await _ndcTask.UpdateAsync(orderCancl0);
                return;
            }
            var orderCancl1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.CanceledWashing);

            if (orderCancl1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmRedirection, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理系统重启事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 处理所有已取消的任务
        /// 更新任务状态
        /// </remarks>
        private async Task HandleResetStartEvent(AciEvent ev)
        {
            var cancelTasks = await _ndcTask.GetListAsync(i => i.TaskStatus > TaskStatuEnum.CarWash && i.TaskStatus < TaskStatuEnum.TaskFinish);

            foreach (var cancelTask in cancelTasks)
            {
                cancelTask.SetStatus(TaskStatuEnum.Canceled);
                await _ndcTask.UpdateAsync(cancelTask);
                _logger.LogCritical($"重启系统修改{cancelTask.SchedulTaskNo}任务状态");
            }
        }

        /// <summary>
        /// 处理取货请求重定向事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 处理取货失败的情况
        /// 更新任务状态为重定向请求
        /// </remarks>
        private async Task HandleRedirectRequestFetchEvent(AciEvent ev)
        {
            var fetch0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus != TaskStatuEnum.RedirectRequest);
            if (fetch0 != null)
            {
                fetch0.SetStatus(TaskStatuEnum.RedirectRequest);
                await _ndcTask.UpdateAsync(fetch0);
                return;
            }

            var fetch1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.RedirectRequest);
            if (fetch1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmCancellation, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理AGV订单事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 处理AGV相关的任务状态更新
        /// 发送重定向确认
        /// </remarks>
        private async Task HandleOrderAgvEvent(AciEvent ev)
        {
            if (ev.Parameter2 == (int)TaskStatuEnum.CarWash)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmRedirection, 0, 0);
                return;
            }

            var orderAgv0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus != TaskStatuEnum.OrderAgv);
            if (orderAgv0 != null)
            {
                orderAgv0.SetStatus(TaskStatuEnum.OrderAgv);
                _ndcTask.UpdateAsync(orderAgv0, true).Wait();
                return;
            }

            var orderAgv1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.OrderAgv);
            if (orderAgv1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmRedirection, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理订单转换事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 处理取货流程失败的情况
        /// 更新任务状态为无效取货点
        /// </remarks>
        private async Task HandleOrderTransformEvent(AciEvent ev)
        {
            var invalidUp0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus != TaskStatuEnum.InvalidUp);
            if (invalidUp0 != null)
            {
                invalidUp0.SetStatus(TaskStatuEnum.InvalidUp, ev.Parameter1);
                _ndcTask.UpdateAsync(invalidUp0, true).Wait();
                return;
            }

            var invalidUp1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.InvalidUp);
            if (invalidUp1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmCancellation, 0, 0);
                return;
            }
        }

        /// <summary>
        /// 处理结束事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 回收已完成任务的ID
        /// 发送结束确认
        /// </remarks>
        private async Task HandleEndEvent(AciEvent ev)
        {
            int taskOut = 0, taskIn = 0;

            var recovery = await _ndcTask.GetListAsync(x => x.NdcTaskId == ev.Parameter1);
            foreach (var item in recovery)
            {
                if (item.TaskStatus == TaskStatuEnum.TaskFinish) item.RecoveryId();
                if (item.TaskStatus == TaskStatuEnum.Canceled) item.RecoveryId();
                if (item.TaskStatus == TaskStatuEnum.InvalidUp) item.RecoveryId();
                if (item.TaskStatus == TaskStatuEnum.InvalidDown) item.RecoveryId();
                if (item.TaskStatus == TaskStatuEnum.CanceledWashFinish) item.RecoveryId();
                if (item.TaskStatus == TaskStatuEnum.RedirectRequest) item.RecoveryId();
                if (item.TaskStatus == TaskStatuEnum.OrderAgvFinish) item.RecoveryId();
            }
            _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.End, 0, 0);
        }

        /// <summary>
        /// 处理取消请求事件
        /// </summary>
        /// <param name="ev">ACI事件对象</param>
        /// <remarks>
        /// 更新任务状态为已取消
        /// 发送取消确认
        /// </remarks>
        private async Task HandleCancelRequestEvent(AciEvent ev)
        {
            var cancel0 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus != TaskStatuEnum.Canceled);

            if (cancel0 != null)
            {
                cancel0.SetStatus(TaskStatuEnum.Canceled);
                await _ndcTask.UpdateAsync(cancel0);

                return;
            }

            var cancel1 = await _ndcTask.FirstOrDefaultAsync(x => x.NdcTaskId == ev.Parameter2 && x.TaskStatus == TaskStatuEnum.Canceled);

            if (cancel1 != null)
            {
                _aciAppManager.SendHostAcknowledge(null, ev.Index, (int)ReplyTaskState.ConfirmCancellation, 0, 0);
                return;
            }
        }

        private async Task HandleRedirectOrNotEvent(AciEvent ev)
        {




        }
    }
}
