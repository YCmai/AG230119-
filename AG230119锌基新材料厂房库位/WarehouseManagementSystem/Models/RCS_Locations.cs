using System.ComponentModel.DataAnnotations;
using System.Reflection;

public class RCS_Locations
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name cannot be empty")]
    public string Name { get; set; } //库位名字，唯一

    [Required(ErrorMessage = "NodeRemark cannot be empty")]
    public string NodeRemark { get; set; }

    public string? MaterialCode { get; set; } //物料编码，允许为空

    public string PalletID { get; set; } = "0"; //托盘ID

    public string Weight { get; set; } = "0"; //重量，允许为空

    public string Quanitity { get; set; } //数量，允许为空

    public string? EntryDate { get; set; } //入库时间

    [Required(ErrorMessage = "Group cannot be empty")]
    public string Group { get; set; } //分组

    public int LiftingHeight { get; set; } = 0;//举升高度

    [Required(ErrorMessage = "Lock cannot be empty")]
    public bool Lock { get; set; }//是否锁定

    /// <summary>
    /// 等待点
    /// </summary>
    public string WattingNode {  get; set; }

}




