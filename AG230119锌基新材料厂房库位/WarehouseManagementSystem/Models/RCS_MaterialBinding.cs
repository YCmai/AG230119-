using System.ComponentModel.DataAnnotations;
using System.Reflection;

public class RCS_MaterialBinding
{
    public int Id { get; set; }

    public string ProductCode { get; set; } //库位名字，唯一

    public string MaterialCode { get; set; }

    public int RequestQty { get; set; } //物料编码，允许为空
}




