using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2.DocumentModel;
using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Services;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ValidateModel]
    public class ChurchesController : Controller
    {
        private IDataRepository _dataRepository;

        public ChurchesController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet("{churchId}")]
        public async Task<IActionResult> Get(string churchId)
        {
            var church = await _dataRepository.GetChurch(churchId);
            return church == null ? NotFound() : (IActionResult)Ok(church);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Church church)
        {
            var res = await _dataRepository.AddChurch(church);
            return res ? Created($"/api/churches/{church.ChurchId}", "") : (IActionResult)BadRequest();
        }

        [HttpPut("{churchId}")]
        public async Task<IActionResult> Put(string churchId, [FromBody]Church church)
        {
            var res = await _dataRepository.UpdateChurch(church);
            return res ? Ok() : (IActionResult)BadRequest();
        }
    }
}