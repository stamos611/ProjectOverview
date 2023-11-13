using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectOverview.Models;
using System.Diagnostics.Metrics;

namespace ProjectOverview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {
        private readonly ProjectOverviewContext _dbContext;
        
        #region Find Second Largest Number in an array
        [HttpPost("secondlargest")]
        public ActionResult<int> GetSecondLargest([FromBody] RequestObj request)
        {
            //Check if Request is valid
            if (request == null || request.RequestArrayObj == null || !request.RequestArrayObj.Any())
            {
                return BadRequest("Invalid input");
            }

            //Remove duplicates and order the array in descending order
            var orderedArray = request.RequestArrayObj.Distinct().OrderByDescending(x => x).ToList();

            //Check if there is at least a second element
            if (orderedArray.Count >= 2)
            {
                return Ok(orderedArray[1]);
            }

            //If there is no second element, return null or handle as needed
            return Ok((int?)null);
        }
        #endregion

        #region Call 3rd Party API
        private readonly HttpClient _httpClient;

        public MyController(IHttpClientFactory httpClientFactory,ProjectOverviewContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _dbContext = dbContext;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<CountryInfo>>> GetAllCountries()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://restcountries.com/v3.1/all");
                var countries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CountryInfo>>(response);

                // Save countries to the database
                foreach (var country in countries!)
                {
                    var countryEntity = new Country
                    {
                        CommonName = country.CommonName,
                        Capital = country.Capital,
                        Borders = string.Join(",", country.Borders!)
                    };

                    _dbContext.Countries.Add(countryEntity);
                }

                await _dbContext.SaveChangesAsync();

                return  Ok();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}
