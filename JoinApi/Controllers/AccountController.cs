using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using AutoMapper;
using JoinApi.Models;
using JoinApi.Service;
using JoinApi.Settings;
using JoinApi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelData.Dto;
using ModelData.Utilities;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBaseWithUserInfo
    {
        private readonly UserManager<UtenteDoc> _userManager;
        private readonly SignInManager<UtenteDoc> _signInManager;
        private readonly RoleManager<RuoloDoc> _roleManager;
        private readonly JwtSettings _jwtSettings;
		private readonly JoinWebUISettings _joinWebUISettings;

        //private readonly Auth0.AuthenticationApi.AuthenticationApiClient _auth0;


        public AccountController(UserManager<UtenteDoc> userManager,
                                RoleManager<RuoloDoc> roleManager,
                                SignInManager<UtenteDoc> signInManager,
                                IOptions<JwtSettings> jwtSettingsOptions,
                                MongoDbService mongoDbService,
                                IOptions<JoinWebUISettings> joinWebUISettingsOptions,
                                IMapper mapper,
                                HttpClient httpClient,
                                AuthSupportService authSupport)
            : base(mapper, mongoDbService, httpClient, authSupport)                                
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettingsOptions.Value;
            _joinWebUISettings = joinWebUISettingsOptions.Value;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UtenteInfoDto model)
        {
            UtenteDoc utente = await _userManager.FindByEmailAsync(model.Email);
            if (utente != null)
            {
                return Conflict(ErrorDtoBuilder.New.Add("Reason","UserAlreadyExists").Build());
            }

            utente = new UtenteDoc
            {
                Email = model.Email!.ToLower(),
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email!.ToLower(),
                Nome = model.Nome,
                Cognome = model.Cognome
            };

            var createResult = await _userManager.CreateAsync(utente); //, model.Password);
            if (!createResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var roles = new List<string>() { RuoliAuth0.REGISTERED };
            roles.Add(RuoliAuth0.REGISTERED);

            var addToRoleResult = await _userManager.AddToRolesAsync(utente, roles); 
            if (!addToRoleResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            


            return Ok();
        }


        //[HttpPost]
        //[Route("register-other")]
        //[Authorize(Roles = RuoliAuth0.REGISTERED)]
        //public async Task<IActionResult> RegisterOtherAsync([FromBody] RegisterOtherDto model)
        //{
        //    var utenteRichiedente = await _userManager.FindByNameAsync(User.Identity!.Name);

        //    if (!await _signInManager.CanSignInAsync(utenteRichiedente))
        //    {
        //        return Unauthorized();
        //    }

        //    UtenteDoc utente = await _userManager.FindByEmailAsync(model.Email);
        //    if (utente != null)
        //    {
        //        return Conflict(new { message = "Username not available" });
        //    }

        //    utente = new UtenteDoc
        //    {
        //        Email = model.Email!.ToLower(),
        //        UserName = model.Email!.ToLower(),
        //        //ClientiIds = new List<Guid>(utenteRichiedente.ClientiIds),
        //        Nome = model.Nome,
        //        Cognome = model.Cognome
        //    };

        //    foreach (var passwordValidator in _userManager.PasswordValidators)
        //    {
        //        var passwordValidateResult = await passwordValidator.ValidateAsync(_userManager, utente, model.Password);
        //        if (!passwordValidateResult.Succeeded)
        //        {
        //            return BadRequest(passwordValidateResult.Errors.Select(e => e.Description));
        //        }
        //    }

        //    var result = await _userManager.CreateAsync(utente, model.Password);
        //    if (!result.Succeeded)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }

        //    var addToRoleResult = await _userManager.AddToRolesAsync(utente, new string[] { RuoliAuth0.REGISTERED });
        //    if (!addToRoleResult.Succeeded)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }

        //    //await SendAccountCreationEmailAsync(utente);

        //    return Ok();
        //}

        //[HttpPost]
        //[Route("register-begin")]
        //public async Task<IActionResult> RegisterBegin([FromBody] RegisterBeginDto dto)
        //{
        //    if (dto.ChiaveLicenza == null && dto.RegisterAsGuest == false)
        //    {
        //        return BadRequest();
        //    }

        //    var claims = new List<Claim>();

        //    claims.Add(new Claim(ClaimTypes.Role, RuoliAuth0.REGISTERING));
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        //    string? codiceCliente = null;

        //    if (dto.RegisterAsGuest == false)
        //    {
        //        string submittedLicenseKey = dto.ChiaveLicenza!.Trim();

        //        JoinLicense joinLicense = new JoinLicense();
        //        if (!joinLicense.ValidateLicense(dto.ChiaveLicenza!))
        //        {
        //            return BadRequest(ErrorDtoBuilder.New.Add("Reason", "ChiaveLicenzaNotValid").Build());
        //        }

        //        codiceCliente = joinLicense.GetCodiceCliente();
        //        var nomeCliente = joinLicense.GetNomeCliente();
        //        joinLicense.DeactivateLicense();


        //        try
        //        {
        //            var cliente = await _mongoDbService.ClientiCollection.Find(p => p.CodiceCliente == codiceCliente).FirstOrDefaultAsync();

        //            if (cliente != null)//cliente già presente
        //            {
        //                if (cliente.ChiaveLicenza != submittedLicenseKey)
        //                {
        //                    // non è possibile associare ad un cliente più di una chiave di licenza Web
        //                    return BadRequest(ErrorDtoBuilder.New.Add("Reason", "ChiaveLicenzaAlreadyDefined").Build());
        //                }
        //            }
        //            else//cliente non ancora presente
        //            {
        //                var clienteNew = new ClienteDoc() { Id = Guid.NewGuid(), CodiceCliente = codiceCliente, ChiaveLicenza = submittedLicenseKey, Nome = nomeCliente };
        //                await _mongoDbService.ClientiCollection.InsertOneAsync(clienteNew);
        //            }
        //        }
        //        catch (MongoWriteException e)
        //        {
        //            if (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
        //            {
        //                return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
        //            }
        //            throw;
        //        }

        //        claims.Add(new Claim(ApplicationClaimTypes.CodiceCliente, codiceCliente));

        //    }

        //    JwtSecurityToken jwtSecurityToken = GetToken(claims, TimeSpan.FromMinutes(30));
        //    string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        //    var expiration = EpochTime.GetIntDate(jwtSecurityToken.ValidTo);

        //    return Ok(new RegisterBeginResponseDto { Success = true, CodiceCliente = codiceCliente, Token = token, Expiration = expiration });
        //}


        private JwtSecurityToken GetToken(List<Claim> authClaims, TimeSpan expiresIn)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                expires: DateTime.Now.Add(expiresIn),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }



        [HttpGet]
        [Route("get-utente-by-email/{email}")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult<UtenteInfoDto>> GetUtenteByEmail(string email)
        {
            email = email.ToLower();
            var foundUser = await _mongoDbService.UtentiCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (foundUser == null)
            {
                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == CurrentUser.JoinInfo.Id).FirstOrDefaultAsync();
            var update = Builders<UtenteDoc>.Update.Push(u => u.FoundUsersIds, foundUser.Id);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == utente.Id, update);

            var utenteInfoDto = _mapper.Map<UtenteInfoDto>(foundUser);
            return Ok(utenteInfoDto);
        }

        [HttpGet]
        [Route("get-utente-attuale")]
        [Authorize]
        public async Task<ActionResult<UtenteInfoDto>> GetUtenteLoggedIn()
        {
            string? email = CurrentUserEmail;
            
            if (!string.IsNullOrWhiteSpace(email))
            {
                var utente = await _mongoDbService.UtentiCollection.Find(u => u.Email == email).FirstAsync();
                if (utente != null)
                {
                    var dto = _mapper.Map<UtenteInfoDto>(utente);
                    foreach (Guid rid in utente.Roles)
                    {
                        var ruolo = await _mongoDbService.RuoliCollection.Find(r => r.Id == rid).FirstAsync();
                        dto.RolesList.Add(new RuoloInfoDto() { Id = ruolo.Id, Name = ruolo.Name });
                    }

                    return Ok(dto);
                }
                else
                {
                    return Ok(null);
                }
            }
            else
            {
                //TODO
                return Problem("JOIN Login error: user has no associated email");
            }
        }


        [HttpGet]
		[Route("get-utente")]
		[Authorize(Roles = RuoliAuth0.REGISTERED)]
		public async Task<ActionResult<UtenteInfoDto>> GetUtente()
		{
			var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == CurrentUser.JoinInfo.Id).FirstAsync();
            var utenteInfoDto = _mapper.Map<UtenteInfoDto>(utente);

			return Ok(utenteInfoDto);
		}

		[HttpGet]
		[Route("get-found-users")]
		[Authorize(Roles = RuoliAuth0.REGISTERED)]
		public async Task<ActionResult> GetFoundUsers()
		{
			var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == CurrentUser.JoinInfo.Id).FirstAsync();
            var foundUsers = await _mongoDbService.UtentiCollection.Find(u => utente.FoundUsersIds.Contains(u.Id)).ToListAsync();
            
            return Ok(_mapper.Map<IEnumerable<UtenteInfoDto>>(foundUsers));
		}

        [HttpPost]
		[Route("assign-utente-gruppo")]
		[Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> AssignUtenteGruppi(AssignUtenteGruppoDto dto)
        {
            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (utente.GruppiIds.Contains(dto.GruppoId))
            {
                //l'utente fa già parte del gruppo
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id.Equals(dto.GruppoId)).FirstOrDefaultAsync();
            if(gruppo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Gruppo non trovato");
            }

            var clienteId = (
                         from o in _mongoDbService.OpereCollection.AsQueryable(null)
                         join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                         where o.Id == gruppo.OperaId
                         select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera, settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.GruppiIds.Add(gruppo.Id);
            var update = Builders<UtenteDoc>.Update.Set(u => u.GruppiIds, utente.GruppiIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if(result.MatchedCount != 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore inserimento utente in gruppo");
            }

            return Ok();
		}

        [HttpPost]
        [Route("remove-utente-gruppo")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveUtenteGruppi(AssignUtenteGruppoDto dto)
        {
            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            if (!utente.GruppiIds.Contains(dto.GruppoId))
            {
                //l'utente non fa parte del gruppo
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id.Equals(dto.GruppoId)).FirstOrDefaultAsync();
            if (gruppo == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Gruppo non trovato");
            }

            var clienteId = (
                         from o in _mongoDbService.OpereCollection.AsQueryable(null)
                         join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                         where o.Id == gruppo.OperaId
                         select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera, settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.GruppiIds.Remove(gruppo.Id);
            var update = Builders<UtenteDoc>.Update.Set(u => u.GruppiIds, utente.GruppiIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione utente da gruppo");
            }

            return Ok();
        }

        [HttpPost]
        [Route("remove-utente-opera")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        public async Task<ActionResult> RemoveUtenteOpera(RemoveUtenteOperaDto dto)
        {
            var utente = await _mongoDbService.UtentiCollection.Find(u => u.Id == dto.UtenteId).FirstOrDefaultAsync();
            if (utente == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Utente non trovato");
            }

            var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == dto.OperaId).FirstOrDefaultAsync();
            if (opera == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Opera non trovata");
            }

            var gruppi = _mongoDbService.GruppiUtentiCollection.Find(g => g.OperaId.Equals(dto.OperaId) && utente.GruppiIds.Contains(g.Id)).ToList();
            if (gruppi == null || gruppi.Count() == 0)
            {
                //l'utente non fa parte di alcun gruppo dell'opera
                return StatusCode(StatusCodes.Status208AlreadyReported);
            }

            var clienteId = (from s in _mongoDbService.SettoriCollection.AsQueryable(null)
                             where s.Id == opera.SettoreId
                             select s.ClienteId).FirstOrDefault();
            if (clienteId == Guid.Empty)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - settore o cliente non trovato");
            }

            //TODO Inserire valutazione permessi
            if (!CurrentUser.JoinInfo.Clienti.Select(c => c.Id).Contains(clienteId))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore - Permessi mancanti");
            }

            utente.GruppiIds.RemoveAll(id => gruppi.Select(g => g.Id).Contains(id));
            var update = Builders<UtenteDoc>.Update.Set(u => u.GruppiIds, utente.GruppiIds);
            var result = await _mongoDbService.UtentiCollection.UpdateOneAsync(u => u.Id == dto.UtenteId, update);
            if (result.MatchedCount != 1)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore rimozione utente da gruppi");
            }

            return Ok();
        }


        [HttpGet]
		[Route("get-gruppo-opera-utenti-ids")]
		[Authorize(Roles = RuoliAuth0.REGISTERED)]
		public async Task<ActionResult> GetGruppoUtentiIds(Guid operaId, Guid gruppoId)
		{
            //TODO verifica che opera sia effettivamente del cliente che effettua questa richiesta
            //var gruppo = await _mongoDbService.GruppiUtentiCollection.Find(g => g.Id == gruppoId && g.OperaId == operaId).FirstAsync();
            var utenti = await _mongoDbService.UtentiCollection.Find(u => u.GruppiIds.Contains(gruppoId)).ToListAsync();
            return Ok(utenti.Select(u => u.Id));

        }

        [HttpGet]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            CurrentUserLogout();
            return Ok();
        }
    }
}
