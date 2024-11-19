using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ModelData.Dto;
using JoinApi.Service;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using JoinApi.Utilities;
using Microsoft.AspNetCore.Cors;

namespace JoinApi.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class FoglioDiCalcoloController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        public FoglioDiCalcoloController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        [Route("get-foglio-di-calcolo-progetto/{ProgettoId}")]
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<FoglioDiCalcoloDto>> GetFoglioDiCalcolo(Guid ProgettoId)
        {

            try
            {
                _logger.ForContext("Guid", ProgettoId).
                    Information("Accesso controller FoglioDiCalcoloController, richiesta File.");

                var data = _mongoDbService.FogliDiCalcoloDataCollection.Find(u => u.ProgettoId == ProgettoId).FirstOrDefault();

                if (
                    (data != null && !data.SerializedData.Any())
                    )
                {
                    _logger.ForContext("Guid", ProgettoId).
                        Warning("Uscita controller FoglioDiCalcoloController, richiesta foglio DiCalcolo di progetto. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }
                else
                {
                    FoglioDiCalcoloDto foglioDiCalcoloDto = new();
                    foglioDiCalcoloDto.ProgettoId = ProgettoId;
                    foglioDiCalcoloDto.FoglioDiCalcoloBase = data?.SerializedData;

                    return Ok(foglioDiCalcoloDto);
                }
            }
            catch (Exception ex)
            {
                _logger.
                    Error($"Uscita controller FoglioDiCalcoloController, richiesta foglio DiCalcolo di progetto. Dettaglio eccezione: {ex}");
                return BadRequest(ex);
                throw;
            }
            finally
            {
            }
        }
    }
}