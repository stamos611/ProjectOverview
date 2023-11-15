using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectOverview.Interface;
using ProjectOverview.Models;
using StackExchange.Redis;

namespace ProjectOverview.Implementantion
{
    public class MyProject:IMyProject
    {
        private readonly HttpClient _httpClient;
        private readonly ProjectOverviewContext _dbContext;
        private readonly IConnectionMultiplexer _redisCache;
        private IDatabase _redisDB;
        public MyProject(HttpClient httpClient, ProjectOverviewContext dbContext, IConnectionMultiplexer redisCache)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _redisCache = redisCache;
            _redisDB=_redisCache.GetDatabase();
        }
        public int? GetSecondLargest(RequestObj request)
        {
            //Check if request is not null
            if (request == null)
            {
                return null;
            }

            //Remove duplicates and order the array in descending order
            var orderedArray = request.RequestArrayObj!.Distinct().OrderByDescending(x => x).ToList();

            //Check if there is at least a second element
            if (orderedArray.Count < 2)
            {
                return null;
            }

            return orderedArray[1];
        }

        public async Task<ActionResult<IEnumerable<CountryInfo>>> GetAllCountries()
        {
            try
            {
                List<CountryInfo> countries = new List<CountryInfo>();

                string hashKey=RedisKeys.GetCountriesKey();

                bool keyExistToRedisDB = Redis.KeyExistToRedisDB(hashKey, ref _redisDB);
                if (keyExistToRedisDB)
                {
                    countries = _redisDB.HashGetAll(hashKey).ConvertFromRedis<List<CountryInfo>>();
                }
                else
                {
                    var dbRecs= await _ReadCountriesFromDb();
                    if (dbRecs != null)
                    {
                        countries=dbRecs;
                        //Save countries to Redis
                        _SaveToCache(countries);
                    }
                    else
                    {
                        var response = await _httpClient.GetStringAsync("https://restcountries.com/v3.1/all");

                        if (response != null)
                        {
                            var countriesResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(response);

                            // Transform dynamic list to CountryInfo Type
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
                                    CommonName = item.name.common != null ? item.name.common.ToString() : string.Empty,
                                    Capital = capital != null ? capital.ToString() : string.Empty,
                                    Borders = item.borders != null ? item.borders.ToObject<List<string>>() : new List<string>()
                                };
                                countries.Add(newcountry);
                            }

                            // Save countries to the database
                            foreach (var country in countries)
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

                            // Save countries to Redis
                            _SaveToCache(countries);
                            
                        }
                    }
                }
                
                return countries;
            }
            catch (Exception)
            {
                return new List<CountryInfo>();
            }
        }
        private async Task<List<CountryInfo>> _ReadCountriesFromDb()
        {
            List<CountryInfo> dbRecs = await (from c in _dbContext.Countries
                                select new CountryInfo
                                {
                                    CommonName = c.CommonName,
                                    Capital = c.Capital,
                                    Borders = c.Borders!.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                                }).ToListAsync();

            return dbRecs;
        }
        private async void _SaveToCache(List<CountryInfo> countries)
        {            
            //Get EndPoints
            var endpoints = _redisCache.GetEndPoints();

            //Get Server
            var server = _redisCache.GetServer(endpoints[0]);

            //Delete Keys
            var keys = server.Keys(0, pattern: "*" + RedisKeysConst.COUNTRY + RedisKeysConst.REDIS_DELIM + "*").ToArray();
            _redisDB.KeyDelete(keys);

            // Loop and insert to Redis
            foreach (var item in countries)
            {
                string hashCountriesKey = RedisKeys.GetCountriesKey();
                _redisDB.HashSet(hashCountriesKey, item.ToHashEntries());
            }
        }
    }    
}
