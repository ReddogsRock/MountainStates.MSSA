using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Oqtane.Services;
using Oqtane.Shared;

namespace MountainStates.MSSA.Module.TopDogs.Services
{
    public class TopDogsService : ServiceBase, ITopDogsService
    {
        public TopDogsService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("TopDogs");

        public async Task<List<Models.TopDog>> GetTopDogsAsync(int year, int level, string species, int quantity)
        {
            return await GetJsonAsync<List<Models.TopDog>>($"{ApiUrl}/list?year={year}&level={level}&species={species}&quantity={quantity}");
        }
    }

    public interface ITopDogsService
    {
        Task<List<Models.TopDog>> GetTopDogsAsync(int year, int level, string species, int quantity);
    }
}