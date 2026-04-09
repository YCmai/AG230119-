using Volo.Abp.Application.Dtos;

namespace WMS.MaterialModule.Application
{
    public class GetAllMaterialModifyRecordInput : PagedResultRequestDto
    {
        /// <summary>
        /// SKU
        /// </summary>
        public string sku { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string barCode { get; set; }
    }
}
