using Microsoft.AspNetCore.Mvc;

namespace ProjectOverview.Interface
{
    public interface IMyProject
    {
        int? GetSecondLargest(RequestObj request);

        Task<ActionResult<IEnumerable<CountryInfo>>> GetAllCountries();
        
    }
}
