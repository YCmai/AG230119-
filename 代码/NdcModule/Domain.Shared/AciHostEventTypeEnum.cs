namespace AciModule.Domain.Shared
{
    public enum AciHostEventTypeEnum
    {
        None = 0,
        WarmStart = 6001,
        ColdStart = 6002,
        ConnectionCarrier = 6003,
        CentralControlRestart = 6004,
        HostSync = 30,
        OrderReceived = 31,
        /// <summary>
        ///
        ///洗车
        ///
        ///
        ///洗车的情况是，当AGV在执行任务过程中，并且AGV有货物，主机主动发起请求取消指令
        ///指令发送成功后，主机收会NDC洗车通知20
        ///这时主机需要回收142指令和洗车站点 
        ///
        ///
        /// 这里异常处理点库位逻辑 8E就是142
        ///
        ///
        /// 
        /// </summary>
        CarrierConnected = 32,
        /// <summary>
        /// 取货失败流程处理，包含刚开始下发的任务中存在无效的取货站点
        /// </summary>
        OrderTransform = 49,
        /// <summary>
        /// 请求重定向卸货站点
        /// </summary>
        OrderCancel = 34,
        /// <summary>
        /// agv卸货时发现路线异常，主动请求重定向站点
        /// </summary>
        OrderAgv = 52,
        OrderComplete = 35,


        ValidKey = 45,
        FetchError = 46,
        DeliverError = 47,
        /// <summary>
        /// 未知货物洗车
        /// </summary>
        CarWashRequest = 60,
        CarWashFailed = 61,
        CarWashComplete = 62,
        CarrierContinue = 63,
        CarChargeStart = 70,
        CarChargeEnd = 71,
        CarChargeError = 75,
        Location = 252,

        /// <summary>
        /// 订单开始  
        /// </summary>
        OrderStart = 1,
        /// <summary>
        /// 参数确认(取货站点和卸货站点) 
        /// </summary>
        ParameterCheck = 2,
        /// <summary>
        /// AGV移动到取货站点入口,这步Ndc已经叫车到取货入口了，NDC在这步才会返回AGVid,不需要主机回复同步
        /// 回复格式:SendHostAcknowledge(null, ev.Index, 3, 0, 0);
        /// </summary>
        MoveToLoad = 3,
        /// <summary>
        /// Host同步 AGV已到达取货站点入口前(需要发送取货高度和取货深度)
        /// </summary>

        LoadHostSyncronisation = 4,

        /// <summary>
        /// AGV取货完成
        /// </summary>
        Loading = 5,

        /// <summary>
        /// Host(agv已取到货，更新库位信息或处理其他)
        /// </summary>
        LoadingHostSyncronisation = 6,

        /// <summary>
        /// AGV移动到卸货站点入口 (此时可以更改卸货站点)
        /// </summary>
        MovingToUnload = 7,

        /// <summary>
        /// Host同步 (此时发送卸货高度和卸货深度id)
        /// </summary>
        UnloadHostSyncronisation = 8,

        /// <summary>
        /// AGV卸货完成
        /// </summary>
        Unloading = 9,

        /// <summary>
        /// Host同步(更新库位信息)
        /// 卸货完成，需要回复Ndc
        /// </summary>
        UnloadingHostSyncronisation = 10,

        /// <summary>
        /// 订单完成
        /// </summary>
        OrderFinish = 11,

        /// <summary>
        /// 取货失败，agv无法到达取货点取货
        /// </summary>
        RedirectRequestFetch = 33,

        /// <summary>
        /// 请求重定向卸货站点
        /// </summary>
        RedirectRequestDeliver = 34,
        
        /// <summary>
        /// 请求取消
        /// </summary>
        CancelRequest = 48,
        /// <summary>
        /// 无效的卸货站点
        /// </summary>

        InvalidDeliverStation = 50,
        /// <summary>
        /// 订单重定向确认
        ///
        /// 一种是当发现需要洗车的AGV
        /// Ndc收到洗车站点后，发送254过来，确认修改站点
        /// 主机收到254后，回复ndc确认
        /// ndc收到254后，再走正常流程。
        ///
        /// 另外一种是主动重定向任务站点，AGV会自己判断需不需要洗车，如果需要要的话，就走上面流程
        /// 
        /// </summary>
        Redirect = 254,


        ///订单取消
        Cancel = 255,

        /// <summary>
        /// 结束
        /// </summary>
        End = 153,

        /// <summary>
        /// 系统重启
        /// </summary>
        ResetStart = 6003,

        /// <summary>
        /// 这里也是重启
        /// </summary>
        ResetStart2 = 6002,


        /// <summary>
        /// 卸货点重定向
        /// </summary>
        RedirectOrNot = 141,
    }
}
