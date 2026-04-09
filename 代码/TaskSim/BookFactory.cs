using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSim
{
    /// <summary>
    /// 还书机对象
    /// </summary>
    class BookFatory
    {

        const string OnlineUrl = "http://localhost:5000/PDS/api/PDSSim/GetOrCreateForSim";
        const string TaskFinishUrl = "http://localhost:5000/PDS/api/PDSSim/PutBookTaskFinished";

        Random random = new Random();

        #region 还书机的基本属性
        public string Id { get; set; }
        public bool IsOnline { get; set; } = false;
        public string BookId { get; set; } = "";
        public DateTime StartPutTime { get; set; }
        public BookState State { get; set; }

        public bool AutoAddBook { get; set; } = false;
        public DateTime LastAutoAddBookTime { get; set; }

        public string BookSortDecribtion { get; set; }

        #endregion

        public BookFatory()
        {
            //启动一个线程，每1s调用一次Update
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        Update();
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            });
        }

        private  void Update()
        {
            if(AutoAddBook && (DateTime.Now - LastAutoAddBookTime).TotalSeconds>2)
            {
                this.AddBook(Guid.NewGuid().ToString(), "图书分类" + random.Next(1, 40));
                LastAutoAddBookTime = DateTime.Now;
            }

            //执行更新当前任务（还书机放书到agv）
            if (this.State == BookState.PuttingBook)
            {
                //如果大于2s，认为书本已经放完。更新状态并通知pds书本已经放完了。
                if ((DateTime.Now - this.StartPutTime).TotalMilliseconds >= 2000)
                {
                    this.State = BookState.NoBook;
                    this.BookId = "";
                    this.BookSortDecribtion = "";
                    var result = HttpClientHelper.Post<CommonResponseDto>(TaskFinishUrl, new OpStationOnLineInput() 
                    {
                        frameId = Guid.NewGuid().ToString(),
                        operationStationId = this.Id,
                        packageCode = this.BookId,
                        packageExtInfo = "",
                        state = this.State,
                        packageSortDecribtion = this.BookSortDecribtion
                    });  
                    
                }
            }

            //定时与Pds通讯，并执行pds回复的任务（放书到agv）
            if (this.IsOnline)
            {
                var result = HttpClientHelper.Post<GetOrCreateProcessResult>(OnlineUrl, new OpStationOnLineInput()
                {
                    frameId = Guid.NewGuid().ToString(),
                    operationStationId = this.Id,
                    packageCode = this.BookId,
                    packageExtInfo = "",
                    state = this.State,
                    packageSortDecribtion = this.BookSortDecribtion
                });
                if(this.State== BookState.HasUnhadleBook)
                {

                }

                //查看pds的回复，看看pds是否需要还书机执行放书任务。
                if (result != null)
                {
                    if (result.PutBook == true && this.State == BookState.HasUnhadleBook)
                    {
                        this.StartPutTime = DateTime.Now;
                        this.State = BookState.PuttingBook;
                        System.Diagnostics.Debug.WriteLine(this.Id + "收到放书任务：");

                    }
                    System.Diagnostics.Debug.WriteLine(this.Id + "访问成功：");

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(this.Id + "访问访问失败：");
                    Thread.Sleep(5000);
                }
            }
        }

        /// <summary>
        /// 添加书本
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="bookSort"></param>
        /// <returns></returns>
        public bool AddBook(string bookId, string bookSort)
        {
            if (!this.IsOnline) return false;  //上线才能还书。
            if (this.State != BookState.NoBook) return false;//原来有书就不能还书。

            this.BookId = bookId;
            this.BookSortDecribtion = bookSort;
            this.State = BookState.HasUnhadleBook;  //还书后，状态就变成有书未处理。
            return true;
        }



    }

    #region 接口传输对象

    public class OpStationOnLineInput
    {
        public string frameId { get; set; }
        public string operationStationId { get; set; }
        public string packageCode { get; set; }
        public string packageExtInfo { get; set; }
        public BookState state { get; set; }
        public string packageSortDecribtion { get; set; }
    }

    public enum BookState
    {
        /// <summary>
        /// 当前没有书本。（也可以能是书本已经投完）
        /// </summary>
        NoBook,
        /// <summary>
        /// 当前有未处理书本。此时PackageCode有效，表示还书机在等待PDS下发投书任务。
        /// </summary>
        HasUnhadleBook,
        /// <summary>
        /// 当前正在执行投书任务。
        /// </summary>
        PuttingBook
    }

    public class GetOrCreateProcessResult
    {
        public string FrameId { get; set; }
        /// <summary>
        /// 还书机是否需要把书投出去（给agv）
        /// </summary>
        public bool PutBook { get; set; }
    }

    [Serializable]
    public class CommonResponseDto 
    {
        const string Result_Success = "Success";
        const string Result_Fault = "Fault";

        protected CommonResponseDto() { }

        public static CommonResponseDto CreateSuccessResponse(string frameId)
        {
            return new CommonResponseDto() { FrameId = frameId, Result = Result_Success };
        }

        public static CommonResponseDto CreateFaultResponse(string frameId, string faultMessage)
        {
            return new CommonResponseDto() { FrameId = frameId, Result = Result_Fault, FaultMessage = faultMessage };
        }



        public string FrameId { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string Result { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string FaultMessage { get; protected set; } = "";
    }

    #endregion
}
