namespace AciModule.Domain.Shared
{
    public enum FunctionCode
    {
        Error = 0,           //错误
        Normal = 1,          //正常
        NotUsed = 2,         //没有使用
        Reserved = 3,        //预留
        HeartBeatPoll = 4,   //心跳
        HeartBeatACK = 5,    //心跳确认
    }

    public enum MessageType
    {
        None = 0x00,
        OrderInitiate = 0x71,           //Message 'q' //作为订单开始时的响应
        OrderAcknowledge = 0x62,        //Message 'b' //订单确认 (有两种模式，取决取所使用的q(开始的响应))
        OrderRequest = 0x6A,            //Message 'j' //订单请求
        OrderEvent = 0x73,              //Message 's' //订单事件
        OrderStatus = 0x6F,             //Message 'o' //订单状态
        OrderDelete = 0x6E,             //Message 'n' //作为删除订单的确认
        LocalParamCommand = 0x6D,       //Message 'm' //作为参数更新的确认 (本地参数命令)
        LocalParamContents = 0x77,      //Message 'w' //(本地参数内容)
        LocalParamRequest = 0x72,       //Message 'r' //(本李参数请求)
        GlobalParamCommand = 0x67,      //Message 'g' //(全局参数命令)
        GlobalParamStatus = 0x70,       //Message 'p' //(全局参数状态)
        AgvOmPlcToHost = 0x3C,          //Message '<'
        HostToAgvOmPlc = 0x3E,          //Message '>'
    }
    /// <summary>
    /// OM-PLC命令功能码
    /// </summary>
    public enum OmPlcCommandCode
    {
        None = 0,
        ReadMultiByte = 5,                 //读多个功能码
        WriteMultiByte = 6,                //写多个功能码
        NakReadMultiByte = 7,               //多字读取(plc回复)
        NakWriteMultiByte = 8,               //写入多字(plc回复)
    }
    /// <summary>
    /// 本地参数命令代码
    /// </summary>
    public enum LocalParamCommandCode
    {
        InsertSpontaneous = 0,          //自发插入
        InsertRequested = 1,            //请求插入
        Delete = 2,                     //删除
        Read = 3,                       //读取
        ChangePriority = 4,             //变更优先级
        ConnectVehicle = 5,             //连接车辆
    }
    /// <summary>
    /// 全局命令参数代码
    /// </summary>
    enum GlobalParamCommandCode
    {
        None = 0,                      //无
        Read = 1,                      //读
        Write = 2,                     //写
    }
    /// <summary>
    /// 全局参数状态码
    /// </summary>
    enum GlobalParamStatusCode
    {
        None = 0,                     //无
        ReadAck = 1,                  //读全局参数Ack
        WriteAck = 2,                 //写全局参数Ack
        ReadNak = 3,                  //读全局参数Nak
        WriteNak = 4,                 //写全局参数Nak
    }
    /// <summary>
    /// 确认状态码
    /// </summary>
    enum AckStatusCode
    {
        None = -1,                              //无                
        NotImplemented1 = 5,                    //没有执行
        NotImplemented2 = 6,                    //没有执行
        NotImplemented3 = 12,                   //没有执行
        NotImplemented4 = 13,                   //没有执行

        //Response Order Initial ('q')          //订单开始响应
        OrderRejected = 0,                      //订单已拒绝
        OrderAccepted = 1,                      //接收订单
        ParamNumberInvalid = 8,                 //参数数目无效
        PriorityError = 9,                      //优先级错误
        StructureError = 10,                    //无效结构
        FullBuffer = 11,                        //命令缓冲区满了
        ParamNumberHigh = 15,                   //参数数值太高
        UpdateNotAllow = 16,                    //不允许更新
        MissingParam = 26,                      //参少参数
        DuplicatedKey = 27,                     //重复的键
        FormatCodeInvalid = 28,                 //格式代码无效

        //Response Delete Order ('n')           //订单删除响应
        CancelAcknowledge = 25,                 //取消确认
        OrderCancelled = 4,                     //订单已取消
        IndexNotActivated = 14,                 //索引未激活
        CarrierError = 22,                      //载体错误
        IndexInvalid = 24,                      //无效索引号

        //Response Local Parameter Update ('m') //本地参数更新响应
        ParamAcknowledge = 7,                   //参数确认
        ParamDeleted = 18,                      //参数删除

        //Order Finished                        //订单完成响应
        OrderFinished = 3,                      //订单完成

        //Specified Event                       //指定事件响应
        ParamAccepted = 19,                     //接受参数值
        ParamRejected = 20,                     //参数不接受
        InputReleased = 23,                     //输入释放

        //Fatal Error                           //致命错误响应
        FatalError = 17,                        //致命错误，命令执行停止

        //Response Change Order Priority ('m')  //订单优先级变更响应
        PriorityAcknowledge = 35,               //更改顺序实例优先级确认

        //Carrier / Order Connection Event      //订单连接事件
        ConnectionSucceed = 39,                 //连接成功
        ConnectionFail = 40,                    //连接失败
        AllocationCarrier = 2,                  //分配载体
        LostCarrier = 21,                       //订单已经失去载体
        ConnectionCarrier = 37,                 //连接载体
    }

    /// <summary>
    /// 确认组代码
    /// </summary>
    enum AckGroupCode
    {
        None = 0x0000,                          //0
        GroupI = 0x0001,                        //1
        GroupII = 0x0002,                       //2
        GroupIII = 0x0004,                      //4
        GroupIV = 0x0008,                       //8
        GroupV = 0x0010,                        //16
        GroupVI = 0x0020,                       //32
        GroupVII = 0x0040,                      //64
        GroupVIII = 0x0080,                     //128
        GroupIX = 0x0100,                       //256
    }
    /// <summary>
    /// 订单选择代码
    /// </summary>
    enum OrderItemCode
    {
        None = -1,                  //无
        NumericalInterval = 0,      //数值间隔
        ExternalTrigger = 1,        //外部触发
        InternalTrigger = 2,        //内部触发
        UsedIndex = 3,              //使用索引
        CarrierNumber = 4,          //载体号码
        PriorityOrder = 5,          //优先顺序
        OrderState = 6,             //订单状态
    }
    /// <summary>
    /// 订单错误代码
    /// </summary>
    enum OrderErrorCode
    {
        None = 0x00,                //无
        EndOfStream = 0x7F,         //信息流结束
        GeneralError = 0x80,        //一般错误
        InvalidArgument = 0x81,     //非法参数
        RequestPending = 0x82,      //请求待处理
        FullBuffer = 0x83,          //缓冲区已满
        Unknow = 0x84,              //未知
    }
    /// <summary>
    /// 订单单位类型
    /// </summary>
    enum OrderUnitType
    {
        Requester = 0,              //请求者
        DEBUG = 2,                  //调试
        ACI = 3,                    //ACI
        CWay = 4,                   //方式
    }
    /// <summary>
    /// 订单触发模块
    /// </summary>
    enum OrderTriggerModule
    {
        SystemControl = 0x02,  //SystemGo                   //系统控制
        CarrierManager = 0x33, //CarWash and CarCharge      //载体管理
        InputTrigger = 0x2C,   //Input started order        //输入触发
        OrderManager = 0x2F,   //Sfork started order        //订单管理
    }

    /// <summary>
    /// 事件状态码
    /// </summary>
    enum EventStatusCode
    {
        NotUsed = 1,                //没有使用
        Pending = 2,                //等待
        Transitory1 = 3,            //暂时1
        Transitory2 = 4,            //暂时2
        WaitingVehicle = 5,         //等候(信号)车辆
        Transitory3 = 6,            //暂时3
        MovingVehicle = 7,          //移动车辆
    }
    /// <summary>
    /// 订单列表代码
    /// </summary>
    enum OrderListCode
    {
        None = 0,                   //无
        FreeInstance = 1,           //自由实例
        PendingList = 2,            //等待列表
        ActiveList = 3,             //活动列表
        CmvRequestList = 4,         //Cmv请求列表
        CarRequestList = 5,         //汽车请求列表
        CarReleaseList = 6,         //汽车放行列表
        CmvList = 7,                //Cmv列表
        CallocRequestList = 8,      //呼叫请求列表
        CmCommandsList = 9,         //Cm命令列表
        OmDebugList = 10,           //Om调试列表
    }
    /// <summary>
    /// 订单状态码
    /// </summary>
    enum OrderStateCode
    {
        None = -1,                  //无
        EmptyInstance = 0,          //空实例
        FuncEvaluated = 1,          //功能评估
        FuncNotEvaluated = 2,       //功能未评估
        NotUsed = 3,                //未使用
        PendingParamRequest = 4,    //待处理参数请求
        Dalayed = 5,                //延迟
        BreakEvaluated = 6,         //中断评估
        TerminateOrder = 7,         //终止订单
        Cancelled = 8,              //取消
        CancelTermination = 9,      //取消终止
        Retry = 10,                 //重试
        InputPollRequest = 11,      //输入投票请求
        SystemFuncRequest = 12,     //系统功能请求
        ExplicitParamRequest = 13,  //显示参数请求
        PlcRequest = 14,            //plc请求
        BarCodeRequest = 15,        //条形码请求
        EvaluatedByDebugger = 16,   //调试器评估
        WaitingForQueue = 17,       //等待队列
        WaitingForChild = 18,       //等待小孩
    }

    /// <summary>
    /// 订单状态代码
    /// </summary>
    enum OrderStatusCode
    {
        None = -1,                  //无
        TRUE = 0,                   //真
        FALSE = 1,                  //假
        ERROR = 2,                  //错误
    }
    /// <summary>
    /// 订单触发状态码
    /// </summary>
    enum OrderTriggerCode
    {
        None = -1,                  //无
        Internal = 0,               //内部
        Debug = 2,                  //调试
        ACI = 3,                    //ACI
        Cway = 4,                   //Cway
        Multidrop = 11,             //多个rop
    }

    /// <summary>
    /// 载体主要状态代码
    /// </summary>
    enum CarrierMainStatusCode
    {
        None = -1,                  //无
        Cancelled = 0,              //取消
        CancelConnnection = 1,      //取消连接
        Free = 2,                   //自由(免费)
        Allocated = 3,              //已分配
        Active = 4,                 //激活
        Connected = 5,              //连接
        Unknow = 6,                 //未知
    }
    /// <summary>
    /// 载体移动状态代码
    /// </summary>
    enum CarrierMoveStateCode
    {
        None = -1,                  //无
        Unknow = 0,                 //未知
        StandOnPoint = 1,           //在站点上
        MovingToEntry = 2,          //移动到入口
        MovingToPoint = 3,          //移到到点
        MovingToExit = 4,           //移动到出口
        MovingToRequiredExit = 5,   //移动到所需的出口
        MovingToEscape = 6,         //移动避开
        WaitingForCommand = 7,      //等待命令
    }
}
