using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectOverview.Models;
using System.Diagnostics.Metrics;

namespace ProjectOverview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {


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
        private readonly ProjectOverviewContext _dbContext;
        public MyController(IHttpClientFactory httpClientFactory, ProjectOverviewContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _dbContext = dbContext;
        }

        [HttpGet("AllCountries")]
        public async Task<ActionResult<IEnumerable<CountryInfo>>> GetAllCountries()
        {
            try
            {
                List<CountryInfo> countries = new List<CountryInfo>();

                var response = await _httpClient.GetStringAsync("https://restcountries.com/v3.1/all");

                if (response != null)
                {
                    var countriesResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(response);

                    foreach (var item in countriesResponse!)
                    {
                        string capital = string.Empty;
                        // Check if 'capital' property is an array
                        if (item.capital is JArray capitalArray && capitalArray.Count > 0)
                        {
                            // Take the first element of the array as the capital string
                            capital = capitalArray[0].ToString();
                        }
                        else if (item.capital != null)
                        {
                            // If 'capital' is not an array but exists, convert it to string
                            capital = item.capital.ToString();
                        }

                        // Extract necessary information and create CountryInfo objects
                        var newcountry = new CountryInfo
                        {
                            CommonName = item.name.common !=null?item.name.common.ToString():string.Empty,
                            Capital = capital != null? capital.ToString():string.Empty,
                            Borders = item.borders != null ? item.borders.ToObject<List<string>>() : new List<string>()
                        };
                        countries.Add(newcountry);
                    }
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
                }
                return Ok(countries);                
            }
            catch (JsonException ex)
            {
                // Handle JSON deserialization errors
                return StatusCode(500, $"Error in deserialization: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                // Handle database-related exceptions
                return StatusCode(500, $"Error saving data to the database: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        #endregion
    }
}
