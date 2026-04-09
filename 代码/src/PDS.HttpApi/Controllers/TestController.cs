using Microsoft.AspNetCore.Mvc;
using PDS.Localization;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace PDS.Controllers
{
    //提示swagger报错是，https://localhost:【port】/swagger/V1/swagger.json

    [Route("api/[controller]/[action]")]  
    public class TestController : PDSController
    {
        [HttpPost("abc")]  //在class路由的后面加上"/abc"
        //[Route("abc")]  //在class路由的后面加上"/abc"

        public async Task<ActionResult>  AddDeliver([FromBody]testInput testInput)
        {
            //return Ok(new testdto() { Name = testInput.Name , Value = testInput.Value });  //可行，自动把首字母变为小写
            //return Json(new testdto() { Name = testInput.Name, Value = testInput.Value });  //可行，自动把首字母变为小写
            return Json(new { Name = testInput.Name, Value = testInput.Value });    //可行，自动把首字母变为小写
        }


    }


    public class testdto
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }



    public class testInput
    {
        public testInput() { }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}