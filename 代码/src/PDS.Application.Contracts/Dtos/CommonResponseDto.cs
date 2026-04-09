using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;


namespace PDS.Application.Contracts.Dtos
{
    [Serializable]
    public class CommonResponseDto : EntityDto
    {
        const string Result_Success = "Success";
        const string Result_Fault = "Fault";

        protected CommonResponseDto() { }

        public static CommonResponseDto CreateSuccessResponse(string frameId)
        {
            return new CommonResponseDto() {FrameId = frameId, Result = Result_Success };
        }

        public static CommonResponseDto CreateFaultResponse(string frameId,string faultMessage)
        {
            return new CommonResponseDto() { FrameId = frameId, Result = Result_Fault, FaultMessage=  faultMessage };
        }



        public string FrameId { get;protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string Result { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string FaultMessage { get; protected set; } = "";
    }

    
}
