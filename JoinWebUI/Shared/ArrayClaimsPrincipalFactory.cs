using JoinWebUI.Models;
using JoinWebUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

using ModelData.Utilities;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.Json;
using System.Xml.Linq;

namespace JoinWebUI.Shared;

public class ArrayClaimsPrincipalFactory<TAccount> : AccountClaimsPrincipalFactory<TAccount> where TAccount : RemoteUserAccount
{
    private IAuthSyncService _authSync;
    private JoinWebApiClient _joinWebApiClient;

     public ArrayClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor, IAuthSyncService authSync, JoinWebApiClient joinWebApiClient)
    : base(accessor)
    {
        _authSync = authSync;
        _joinWebApiClient = joinWebApiClient;
    }


    // when a user belongs to multiple roles, Auth0 returns a single claim with a serialised array of values
    // this class improves the original factory by deserializing the claims in the correct way
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync (TAccount account, RemoteAuthenticationUserOptions options)
    {

        var user = await base.CreateUserAsync(account, options);

        var claimsIdentity = (ClaimsIdentity)user.Identity;

        if (account != null)
        {
            foreach (var kvp in account.AdditionalProperties)
            {
                var name = kvp.Key;
                var value = kvp.Value;
                if (value != null &&
                    value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    claimsIdentity.RemoveClaim(claimsIdentity.FindFirst(kvp.Key));

                    var claims = element.EnumerateArray()
                        .Select(x => new Claim(kvp.Key, x.ToString()));

                    claimsIdentity.AddClaims(claims);
                }
            }

            AccessToken token;
            (await TokenProvider.RequestAccessToken()).TryGetToken(out token);
            _joinWebApiClient.SetBearerToken(token.Value);

            //UtenteModel dbUser = await _authSync.GetJoinUserInfo();
            //if (dbUser == null)
            //{
            //    claimsIdentity.AddClaim(new Claim(ROLEKEY, RuoliUtente.REGISTERING));
            //}
            //else
            //{
            //    foreach (string role in dbUser.RolesList.Select(r => r.Name))
            //    {
            //        if (claimsIdentity.Claims.Where(c => c.Type == ROLEKEY && c.Value == role).Count() == 0)
            //        {
            //             claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, "Join DB via JoinApi"));
            //        }
            //    }
            //}

        }

        var debug = user.Claims.ToList();

        return user;
    }

}

