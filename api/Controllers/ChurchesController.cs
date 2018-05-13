using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using parishdirectoryapi.Models;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Services;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ValidateModel]
    public class ChurchesController : BaseController
    {
        public ChurchesController(IDataRepository dataRepository):base(dataRepository)
        {
        }

        [HttpGet("{churchId}")]
        public async Task<IActionResult> Get(string churchId)
        {
            var church = await DataRepository.GetChurch(churchId);
            return church == null ? NotFound() : (IActionResult)Ok(church);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Church church)
        {
            var res = await DataRepository.AddChurch(church);
            return res ? Created($"/api/churches/{church.ChurchId}", "") : (IActionResult)BadRequest();
        }

        [HttpPut("{churchId}")]
        public async Task<IActionResult> Put(string churchId, [FromBody]Church church)
        {
            var res = await DataRepository.UpdateChurch(church);
            return res ? Ok() : (IActionResult)BadRequest();
        }
    }
}