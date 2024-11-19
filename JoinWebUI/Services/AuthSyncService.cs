using AutoMapper;
using JoinWebUI.Models;
using ModelData.Dto;

namespace JoinWebUI.Services
{

    public interface IAuthSyncService
    {
        public UtenteModel UtenteData { get; set; }
        public Task<UtenteModel> GetJoinUserInfoAsync();
        public Task<bool> SaveCurrentUserAsync(UtenteModel user, bool updateExisting);

        public event EventHandler SyncedUserUpdated;
        public Task LogoutAsync();
        public void Dispose();

    }
    public class AuthSyncService : IAuthSyncService
    {
        public event EventHandler SyncedUserUpdated;

        private readonly JoinWebApiClient _apiClient;
        private readonly AutoMapper.IMapper _mapper;
        private bool _running;

        public UtenteModel UtenteData { get; set; }

        public AuthSyncService(JoinWebApiClient apiClient, IMapper mapper)
        {
            _apiClient = apiClient;
            _mapper = mapper;
        }



        public void Start()
        {
            if (!_running)
            {
                _running = true;
            }

        }

        public void Dispose()
        {
            if (_running)
            {

            }
        }

        public async Task<UtenteModel> GetJoinUserInfoAsync()
        {
            var foundUsersResult = await _apiClient.JsonGetAsync<UtenteDto>("utenti/get-utente-attuale"); //, currentUser.Claims.Where(c => c.Type == ClaimTypes.Email).First().Value);
            
            UtenteModel user = null;
            var roles = new List<string>();

            if (foundUsersResult.Success)
            {
                if (foundUsersResult.ResponseContentData != null)
                {
                    try
                    {
                        UtenteData = user = _mapper.Map<UtenteModel>(foundUsersResult.ResponseContentData);


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return user;
        }

        public async Task<bool> SaveCurrentUserAsync(UtenteModel user, bool updateExisting)
        {
            UtenteDto dto = _mapper.Map<UtenteDto>(user);

            JoinWebApiClient.RequestResult<UtenteDto> result;
            if (updateExisting) result = await _apiClient.JsonPutAsync<UtenteDto>("utenti", user.IdString, dto);
            else result = await _apiClient.JsonPostAsync<UtenteDto>("utenti", dto);

            if (result.Success)
            {
                SyncedUserUpdated?.Invoke(this, EventArgs.Empty);
            }
            return result.Success;

        }


        public async Task LogoutAsync()
        {
            await _apiClient.JsonGetAsync<string>("utenti/logout");
            _apiClient.ClearBearerToken();
        }

    }

}

