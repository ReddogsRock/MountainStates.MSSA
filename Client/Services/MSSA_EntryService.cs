using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using MountainStates.MSSA.Module.MSSA_Entries.Models;

namespace MountainStates.MSSA.Module.MSSA_Entries.Services
{
    public class MSSA_EntryService : ServiceBase, IMSSA_EntryService, IService
    {
        public MSSA_EntryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("MSSA_Entry");

        public async Task<List<MSSA_Entry>> GetEntriesAsync(int moduleId)
        {
            return await GetJsonAsync<List<MSSA_Entry>>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<List<EntryListItem>> GetTrialEntriesAsync(int trialId, int moduleId)
        {
            return await GetJsonAsync<List<EntryListItem>>(
                CreateAuthorizationPolicyUrl($"{ApiUrl}/trial/{trialId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Entry> GetEntryAsync(int entryId, int moduleId)
        {
            return await GetJsonAsync<MSSA_Entry>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{entryId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Entry> AddEntryAsync(MSSA_Entry entry, int moduleId)
        {
            return await PostJsonAsync<MSSA_Entry>(CreateAuthorizationPolicyUrl($"{ApiUrl}?moduleid={moduleId}", EntityNames.Module, moduleId), entry);
        }

        public async Task<MSSA_Entry> UpdateEntryAsync(MSSA_Entry entry, int moduleId)
        {
            return await PutJsonAsync<MSSA_Entry>(CreateAuthorizationPolicyUrl($"{ApiUrl}/{entry.EntryId}?moduleid={moduleId}", EntityNames.Module, moduleId), entry);
        }

        public async Task DeleteEntryAsync(int entryId, int moduleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{ApiUrl}/{entryId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task GenerateRunOrderAsync(int trialId, int classId, int moduleId)
        {
            await PostJsonAsync<object>(CreateAuthorizationPolicyUrl($"{ApiUrl}/generaterunorder?trialId={trialId}&classId={classId}&moduleid={moduleId}", EntityNames.Module, moduleId), null);
        }

        public async Task<List<MSSA_Class>> GetClassesAsync(int moduleId)
        {
            var url = CreateApiUrl("MSSA_Class");
            return await GetJsonAsync<List<MSSA_Class>>(CreateAuthorizationPolicyUrl($"{url}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }

        public async Task<MSSA_Class> GetClassAsync(int classId, int moduleId)
        {
            var url = CreateApiUrl("MSSA_Class");
            return await GetJsonAsync<MSSA_Class>(CreateAuthorizationPolicyUrl($"{url}/{classId}?moduleid={moduleId}", EntityNames.Module, moduleId));
        }
    }
}
