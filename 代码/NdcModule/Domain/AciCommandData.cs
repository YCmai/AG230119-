
namespace AciModule.Domain
{
    public class AciCommandData
    {
        private GeneralAciData _RequestData;
        private List<GeneralAciData> _AcknowledgeDatas;
        private bool _Acknowledged = false;
        private AciCommandErrorCode _ErrorCode = AciCommandErrorCode.None;
        private AciCommandCallBack _AckCallBack = null;

        private AciCommandCallBack AckCallBack { get { return _AckCallBack; } }

        public GeneralAciData RequestData { get { return _RequestData; } }
        public GeneralAciData AcknowledgeData { get { return AcknowledgeDatas.Count > 0 ? AcknowledgeDatas[0] : null; } }
        public List<GeneralAciData> AcknowledgeDatas { get { return _AcknowledgeDatas; } }
        public bool Acknowledged { get { return _Acknowledged; } }
        public AciCommandErrorCode ErrorCode { get { return _ErrorCode; } }

        public AciCommandData(GeneralAciData request, AciCommandCallBack callback)
        {
            _RequestData = request;
            _AckCallBack = callback;
            _AcknowledgeDatas = new List<GeneralAciData>();
        }

        public void AddAcknowledge(GeneralAciData ack)
        {
            AcknowledgeDatas.Add(ack);
        }

        public void SetAcknowledged()
        {
            _Acknowledged = true;
        }

        public void Cancel(AciCommandErrorCode ecode)
        {
            _ErrorCode = ecode;
            _Acknowledged = true;
        }

        public void CallBack()
        {
            if (AckCallBack != null)
            {
                AckCallBack(this);
            }
        }

        public bool Wait()
        {
            return Wait(0x7FFFFFFF);
        }

        public bool Wait(int milliseconds)
        {
            DateTime start = DateTime.Now;
            while (!Acknowledged)
            {
                TimeSpan span = DateTime.Now - start;
                if (span.Milliseconds >= milliseconds) break;

                Thread.Sleep(10);
            }

            return Acknowledged;
        }
    }

    public enum AciCommandErrorCode
    {
        None,
        TimeOut,
        DataError,
    }
}
