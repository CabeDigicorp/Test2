using ModelData.Utilities;
using JoinApi.Models;
using JoinApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Diagnostics;
using MongoDB.Bson;
using ModelData.Dto;
using System.ComponentModel.DataAnnotations;
using JoinApi.Utilities;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;

/*
 * 
 * 
 * 
 * 
 * 
 *  QUESTO CONTROLLER FUNZIONA SOLO IN AMBIENTE DI SVILUPPO
 * 
 * 
 * 
 * 
 * 
 * 
 */

namespace JoinApi.Controllers
{
//#if DEBUG
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    public partial class DeveloperController : ControllerBase
    {
        private readonly UserManager<UtenteDoc> _userManager;
        private readonly RoleManager<RuoloDoc> _roleManager;
        private readonly MongoDbService _mongoDbService;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;

        public DeveloperController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration, 
            UserManager<UtenteDoc> userManager, RoleManager<RuoloDoc> roleManager, MongoDbService mongoDbService, Serilog.ILogger logger)
        {
            if (!webHostEnvironment.IsDevelopment() && 
                !webHostEnvironment.IsEnvironment ("Staging") && 
                !webHostEnvironment.IsEnvironment("Development-IISExpress"))
            {
                throw new Exception("QUESTO CONTROLLER FUNZIONA SOLO IN AMBIENTE DI SVILUPPO");
            }

            _userManager = userManager;
            _roleManager = roleManager;
            _mongoDbService = mongoDbService;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize]
        [EnableCors("corsPolicy")]
        [HttpGet("users-only")]
        public async Task<IActionResult> UsersOnly()
        {
            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("create-role")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> CreateRole(string name)
        {
            var role = new RuoloDoc { Name = name };
            await _roleManager.CreateAsync(role);

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("create-user")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> CreateUser(string email, string? nome = "nnn", string? cognome = "ccc", string? role = RuoliAuth0.REGISTERED)
        {
            var user = new UtenteDoc() { UserName = email, Email = email, Nome = nome, Cognome= cognome, EmailConfirmed = true };

            var result = await _userManager.CreateAsync(user, "Qw.123");

            var roleResult = await _userManager.AddToRoleAsync(user, role);

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("assign-role")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> AssignRole(string username, string roleName)
        {
            var user = await _userManager.FindByNameAsync(username);
            var role = await _roleManager.FindByNameAsync(roleName);

            await _userManager.AddToRoleAsync(user, role.Name);

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("get-roles")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> GetRoles(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var roles = await _userManager.GetRolesAsync(user);

            await Task.CompletedTask;
            return Ok(roles);
        }

        [HttpGet("check-role")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> CheckRoles(string username, string roleName)
        {
            var user = await _userManager.FindByNameAsync(username);
            var isInRole = await _userManager.IsInRoleAsync(user, roleName);

            await Task.CompletedTask;
            return Ok(isInRole);
        }


        [HttpGet("utenti-ruoli")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> Utenti()
        {
            var utenti = await _mongoDbService.UtentiCollection.Find(Builders<UtenteDoc>.Filter.Empty).ToListAsync();
            var ruoli = await _mongoDbService.RuoliCollection.Find(Builders<RuoloDoc>.Filter.Empty).ToListAsync();

            await Task.CompletedTask;
            return Ok(new {utenti, ruoli});
        }

        [HttpGet("get-license-token")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> GetLicenseToken(string licenseNumber)
        {
            var claims = new List<Claim>
            {
                new Claim("LicenseNumber", licenseNumber)
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:ValidIssuer"],
                audience: _configuration["JwtSettings:ValidAudience"],
                expires: DateTime.Now.AddDays(7),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var expiration = jwtSecurityToken.ValidTo;

            await Task.CompletedTask;

            return Ok(new { token, expiration });
        }



        [HttpGet("send-email")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> SendEmail(string recipientEmail, string subject)
        {
            //await _smtpService.SendAsync(recipientEmail, recipientEmail, subject, "This is the email body");

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("validate-password")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> ValidatePassword(string password)
        {

            foreach (var pv in _userManager.PasswordValidators)
            {
                var validateResult = await pv.ValidateAsync(_userManager, new UtenteDoc(), password);

                if (!validateResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,validateResult.Errors);
                }
            }


            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("create-user-password")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> CreateUserPassword(string password)
        {
            var user = new UtenteDoc() { UserName = "user-" + Guid.NewGuid().ToString().Substring(0,4) };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            await Task.CompletedTask;
            return Ok();
        }

        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> PostAsync(object data)
        {
            await Task.CompletedTask;
            return Ok(data);
        }


        [HttpGet("opera-add-tag-id")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> OperaAddTagId(Guid tagId)
        {
            if(tagId == Guid.Empty)
            {
                tagId = Guid.NewGuid();
            }

            var update = Builders<OperaDoc>.Update.Combine(Builders<OperaDoc>.Update.Push(o => o.TagIds, tagId));

            var result = _mongoDbService.OpereCollection.UpdateOne(_ => true, Builders<OperaDoc>.Update.Push(o => o.TagIds, tagId));

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("insert-opera")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> InsertOpera(string nome)
        {
            _mongoDbService.OpereCollection.InsertOne(new OperaDoc { Nome = nome });

            await Task.CompletedTask;
            return Ok();
        }

        [HttpGet("get-data")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> GetObject()
        {
            await Task.CompletedTask;
            return Ok(new { FirstName = "Mario", LastName = "Rossi", YearOfBirth = 1980 });
        }

        [HttpGet("download-stream")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> DownloadStream()
        {
            await Task.CompletedTask;
            var stream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));
            return File(stream, "text/plain", fileDownloadName: "hello.txt");
        }

        
        [HttpGet("test")]//test Ale
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> GetTest(string fileName)
        {
            ////https://localhost:5100/api/developer/test?filename=aaa

            ////////////////////////////////////////////////////////
            ////test EntityDtoBuilder
            EntityHelper_test.MongoDbService = _mongoDbService;
            EntityHelper_test.Test();

            await Task.CompletedTask;
            return Ok();
            //fine test

            ///////////////////////////////////////////////////////
            //test download allegati
            //string fileFullPath = string.Format("D:\\Temp\\Allegati\\{0}", fileName);
            //FileInfo file = new FileInfo(fileFullPath);
            //if (file.Exists)
            //{
            //    FileStream stream = System.IO.File.Open(fileFullPath, FileMode.Open, FileAccess.Read);
            //    return File(stream, "application/octet-stream", fileDownloadName: fileName);
            //}

            ////////////////////////////////////////////////////////
            //Test GetModel3dFilesName
            //string temp = "4b4a5f27-2b21-4b53-9e2d-a09ed293bc92";
            //Guid projectId = new Guid(temp);
            //var filesName = (new ProjectHelper(_mongoDbService, projectId)).GetModel3dFilesName();

            return Ok();


        }

        /// <summary>
        /// https://localhost:5100/api/developer/initdb?email=alessandro.uliana@digicorp.it
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("initdb")]//test Ale
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> InitDb(string email)
        {
            ///////////////////////////////////////////////////////////
            ///Init Db

            string adminEmail = email;
            string licenseCode = "thGCI6ngjoxOr9kB3VYNEenO2gEDACYAVXNlckNvZGU9ODU5I1VzZXJOYW1lPURpZ2kgQ29ycCBzLnIubC4BAwUAAAApq6eXKQA6HelelENuv7MpvCWKl4K7raqogiL5QPHsx7L9GjsJia4paQjJXvjblAQ=";

            var licenseInfoDto = JoinLicense.DecodeLicense(licenseCode);

            _mongoDbService.InitCliente(licenseInfoDto.CodiceCliente, "Digi Corp s.r.l.", adminEmail, licenseCode);



            //////////////////////////////////////////////



            return Ok();


        }


        [HttpPost("files-upload")]
        [EnableCors("corsPolicy")]
        [Authorize(Roles = RuoliAuth0.REGISTERED)]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> FilesUpload()
        {
            foreach (var file in Request.Form.Files)
            {
                _mongoDbService.UploadFile(file.OpenReadStream(), file.FileName, file.ContentType, new Dictionary<string,object> { { "codiceCliente", "859" } });
            }
            await Task.CompletedTask;
            return Ok();
        }
        
        private string? GetClaimCodiceCliente()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ApplicationClaimTypes.CodiceCliente);
            if (claim == null)
            {
                return null;
            }
            else
            {
                return claim.Value;
            }
        }

        [HttpPost("files-upload-2")]
        [EnableCors("corsPolicy")]
        //[Authorize(Roles = RuoliUtente.UTENTE)]
        public async Task<IActionResult> FilesUpload2([FromForm] DeveloperFilesUploadDto filesUploadDto)
        {
            foreach (var file in filesUploadDto.Files!)
            {
                _mongoDbService.UploadFile(file.OpenReadStream(), file.FileName, file.ContentType, new Dictionary<string, object> { { "codiceCliente", GetClaimCodiceCliente()! } });
            }
            await Task.CompletedTask;
            return Ok();
        }

        

        [HttpGet("file-download")]
        [EnableCors("corsPolicy")]
        public IActionResult FileDownload(string codiceCliente, string fileName)
        {
            var result = _mongoDbService.DownloadFile(i => i.Filename == fileName && i.Metadata["codiceCliente"] == codiceCliente);
            if(result.Found == true)
            {
                return File(result.ContentStream!, result.ContentType!);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("file-delete")]
        [EnableCors("corsPolicy")]
        public IActionResult FileDelete(string id)
        {
            var objectId = new ObjectId(id);
            _mongoDbService.DeleteFiles(i => i.Id == objectId);
            return Ok();
        }

        [HttpGet("nuovo-cliente")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> NuovoCliente()
        {
            _mongoDbService.ClientiCollection.InsertOne(new ClienteDoc { Nome = "Enel", CodiceCliente = "100", ChiaveLicenza = "aaaaaaaaa" });

            await Task.CompletedTask;
            return Ok();
        }

		[HttpGet("add-found-user")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> AddFoundUser(string userEmail, string foundUserEmail)
		{
            var utente= _mongoDbService.UtentiCollection.Find(u => u.Email == userEmail).FirstOrDefault();
            var foundUser= _mongoDbService.UtentiCollection.Find(u => u.Email == foundUserEmail).FirstOrDefault();

            if(!utente.FoundUsersIds.Contains(foundUser.Id))
            {
				var update = Builders<UtenteDoc>.Update.Push(u => u.FoundUsersIds, foundUser.Id);
				var result = _mongoDbService.UtentiCollection.UpdateOne(u => u.Id == utente.Id, update);
                if(result.ModifiedCount != 1)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
			}

			await Task.CompletedTask;
			return Ok();
		}




		[HttpGet("create-gruppo-utenti")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> CreateGruppiUtenti(string nome, Guid operaId)
		{
            _mongoDbService.GruppiUtentiCollection.InsertOne(new GruppoUtentiDoc { Nome = nome, OperaId = operaId });

			await Task.CompletedTask;
			return Ok();
		}

		[HttpGet("template")]
        [EnableCors("corsPolicy")]
        [NonAction]
        public async Task<IActionResult> Template()
        {


            await Task.CompletedTask;
            return Ok();
        }


    }
//#endif
}
