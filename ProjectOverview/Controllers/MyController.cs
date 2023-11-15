using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectOverview.Interface;
using ProjectOverview.Models;
using System.Diagnostics.Metrics;

namespace ProjectOverview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ProjectOverviewContext _dbContext;
        private readonly IMyProject _myProject;
        public MyController(IHttpClientFactory httpClientFactory, ProjectOverviewContext dbContext,IMyProject myProject)
        {
            _httpClient = httpClientFactory.CreateClient();
            _dbContext = dbContext;
            _myProject = myProject;
        }

        #region Find Second Largest Number in an array
        [HttpPost("secondlargest")]
        public ActionResult<int> GetSecondLargest(RequestObj request)
        {
            //Check if Request is valid
            if (request == null || request.RequestArrayObj == null)
            {
                return  BadRequest("Invalid input");
            }

            var secondLargest= _myProject.GetSecondLargest(request);

            return  Ok(secondLargest);
        }
        #endregion


        #region Call 3rd Party API
        [HttpGet("AllCountries")]
        public async Task<ActionResult<IEnumerable<CountryInfo>>> GetAllCountries()
        {
            var Countries = await _myProject.GetAllCountries();
                      
            return Ok(Countries);
        }
        #endregion
    }
}
