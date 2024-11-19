using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JoinApi.Models;
using AutoMapper;
using ModelData.Dto;
using JoinApi.Service;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authorization;
using ModelData.Utilities;
using ModelData.Model;
using ModelData.Dto.Error;
using JoinApi.Utilities;
using MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using Serilog;

namespace JoinApi.Controllers
{ 
    [Route("api/[controller]")]
    [EnableCors("corsPolicy")]
    [ApiController]
    [Authorize]
    public class ProgettiController : ControllerBaseWithUserInfo
    {
        private readonly Serilog.ILogger _logger;

        //private IEnumerable<Guid> _opereIds;
        private IEnumerable<Guid> OpereUtenteIds
        {
            get
            {
                //if(_opereIds == null)
                //{
                //_opereIds = _mongoDbService.OpereCollection.Find(o => CurrentUserClientiIds.Contains(o.ClienteId)).ToList().Select(o => o.Id);
                return (from o in _mongoDbService.OpereCollection.AsQueryable()
                        join s in _mongoDbService.SettoriCollection.AsQueryable() on o.SettoreId equals s.Id
                        where CurrentUserClientiIds.Contains(s.ClienteId)
                        select o.Id).ToList();
                //}
                //return _opereIds;
            }
        }



        public ProgettiController(IMapper mapper, MongoDbService mongoDbService, HttpClient httpClient, AuthSupportService authSupport, Serilog.ILogger logger) : base(mapper, mongoDbService, httpClient, authSupport, logger)
        {
            _logger = logger;
        }

        // GET: api/Progetti?operaId=5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
        [HttpGet]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<IEnumerable<ProgettoDto>>> GetProgetti(Guid? operaId, Guid? clienteId)
        {
            _logger.ForContext("Guid", operaId).ForContext("Cliente", clienteId).
                   Information("Accesso controller ProgettiController, richiesta GetProgetti.");

            List<Guid> opereIds = new List<Guid>();
            FilterDefinition<ProgettoDoc> filter;
            if(operaId != null)
            {
                //var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == operaId && CurrentUserClientiIds.Contains(o.ClienteId)).FirstOrDefaultAsync();
                var oId = (from o in _mongoDbService.OpereCollection.AsQueryable()
                             join s in _mongoDbService.SettoriCollection.AsQueryable() on o.SettoreId equals s.Id
                             where CurrentUserClientiIds.Contains(s.ClienteId) && o.Id.Equals(operaId)
                             select o.Id).FirstOrDefault();
                if (oId == null || oId == Guid.Empty)
                {
                    _logger.ForContext("Guid", operaId).ForContext("Cliente", clienteId).
                        Warning("Uscita controller ProgettiController, richiesta GetProgetti. Not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }

                opereIds.Add(oId);
                
            }
            else if (clienteId != null)
            {
                var oIds = (from o in _mongoDbService.OpereCollection.AsQueryable(null)
                            join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
                            where s.ClienteId == clienteId
                            select o.Id).Distinct();
                if (oIds == null || !oIds.Any())
                {
                    _logger.ForContext("Guid", operaId).ForContext("Cliente", clienteId).
                        Warning("Uscita controller ProgettiController, richiesta GetProgetti. Record not found.");

                    return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
                }

                opereIds.AddRange(oIds);
            }
            else
            {
                opereIds.AddRange(OpereUtenteIds);
                //filter = Builders<ProgettoDoc>.Filter.Where(p => OpereUtenteIds.Contains(p.OperaId));
            }

            var all = await _mongoDbService.ProgettiCollection.Find(p => true).ToListAsync();
            var progetti = await _mongoDbService.ProgettiCollection.Find(p => opereIds.Contains(p.OperaId)).ToListAsync();

            _logger.ForContext("Guid", operaId).ForContext("Cliente", clienteId)./*ForContext("Progetti", JsonSerializer.Serialize(progetti)).*/
                    Information("Uscita controller ProgettiController, richiesta GetProgetti. Completata correttamente.");

            return Ok(_mapper.Map<IEnumerable<ProgettoDto>>(progetti));
        }


        // GET: api/Progetti/5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
        [HttpGet("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<ProgettoDto>> GetProgetto(Guid id)
        {
            _logger.ForContext("Guid", id).
                Information("Accesso controller ProgettiController, richiesta GetProgetto.");

            var progetto = await _mongoDbService.ProgettiCollection.Find(p => p.Id == id && OpereUtenteIds.Contains(p.OperaId)).FirstOrDefaultAsync();

            if (progetto == null)
            {
                _logger.ForContext("Guid", id).
                        Warning("Uscita controller ProgettiController, richiesta GetProgetto. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            _logger.ForContext("Guid", id)/*.ForContext("Progetti", JsonSerializer.Serialize(progetto))*/.
                   Information("Uscita controller ProgettiController, richiesta GetProgetto. Completata correttamente.");

            return _mapper.Map<ProgettoDto>(progetto);
        }

        [HttpGet]
        [Route("get-by-allegato")]
        [EnableCors("corsPolicy")]
        public async Task<ActionResult<IEnumerable<ProgettoDto>>> GetProgettiByAllegato(Guid operaId, string fileName)
        {
            _logger.ForContext("Guid", operaId).ForContext("Allegato", fileName).
                   Information("Accesso controller ProgettiController, richiesta GetProgettiByAllegato.");

            List<Guid> opereIds = new List<Guid>();
            FilterDefinition<ProgettoDoc> filter;

            //var opera = await _mongoDbService.OpereCollection.Find(o => o.Id == operaId && CurrentUserClientiIds.Contains(o.ClienteId)).FirstOrDefaultAsync();
            var progetti = (from p in _mongoDbService.ProgettiCollection.AsQueryable(null)
                            join a in _mongoDbService.Model3dFileCollection.AsQueryable(null) on p.Id equals a.ProgettoId
                            from i in a.Items
                            where p.OperaId.Equals(operaId) && i.FileName == fileName
                            select p).Distinct().ToList();

            _logger.ForContext("Guid", operaId).ForContext("Allegato", fileName).
                    Information("Uscita controller ProgettiController, richiesta GetProgetti. Completata correttamente.");

            return Ok(_mapper.Map<IEnumerable<ProgettoDto>>(progetti));
        }

        //// PUT: api/Progetti/5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutProgetto(Guid id, ProgettoUpdateDto progettoUpdateDto)
        //{
        //    if (id != progettoUpdateDto.Id)
        //    {
        //        return BadRequest();
        //    }

        //    var progetto = await _mongoDbService.ProgettiCollection.Find(p => p.Id == id && OpereUtenteIds.Contains(p.OperaId)).FirstOrDefaultAsync();

        //    if (progetto == null)
        //    {
        //        return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
        //    }

        //    long matchedCount = 0;
        //    if (progettoUpdateDto.ReplaceProjectOnly)
        //    {

        //        ProjectDocuments? projectDocs = null;
        //        if (!string.IsNullOrEmpty(progettoUpdateDto.ProjectAsString))
        //        {
        //            //deserializzo project
        //            ModelData.Model.Project prj = ModelData.ModelSerializer.Deserialize<ModelData.Model.Project>(progettoUpdateDto.ProjectAsString);

        //            //creo i documenti da inserire nel db
        //            projectDocs = new ProjectDocuments(_mapper);
        //            projectDocs.FromProject(prj);
        //        }

        //        try
        //        {
        //            await _mongoDbService.AddProgetto(progetto, projectDocs);
        //        }
        //        catch (Exception exc)
        //        {
        //            if ((exc as MongoWriteException)?.WriteError.Category == ServerErrorCategory.DuplicateKey)
        //            {
        //                return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
        //            }

        //            return StatusCode(StatusCodes.Status500InternalServerError, exc.Message);
        //        }
        //    }
        //    else
        //    {

        //        var update = Builders<ProgettoDoc>.Update
        //            .Set(p => p.Descrizione, progettoUpdateDto.Descrizione)
        //            .Set(p => p.Nome, progettoUpdateDto.Nome);

        //        var result = await _mongoDbService.ProgettiCollection.UpdateOneAsync(p => p.Id == id && OpereUtenteIds.Contains(p.OperaId), update);
        //        matchedCount = result.MatchedCount;
        //    }

        //    if(matchedCount == 0)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError);
        //    }

        //    return NoContent();
        //}

        // POST: api/Progetti
        [HttpPost]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> PostProgetto(ProgettoContentDto progettoContentDto)
        {
            _logger/*.ForContext("ProgettoContentDto", JsonSerializer.Serialize(progettoContentDto))*/.
                Information("Accesso controller ProgettiController, richiesta PostProgetto.");

            ProjectDocuments project = null;
            var progetto = _mapper.Map<ProgettoDoc>(progettoContentDto);

            try
            {
                if (!string.IsNullOrEmpty(progettoContentDto.Content))
                {
                    //deserializzo project
                    Project prj = ModelData.ModelSerializer.Deserialize<Project>(progettoContentDto.Content);

                    //creo i documenti da inserire nel db
                    project = new ProjectDocuments(_mapper);
                    project.FromProject(prj);

                    progetto.ContentSize = System.Text.Encoding.Unicode.GetByteCount(progettoContentDto.Content);

                    await _mongoDbService.AddProgetto(progetto, project);
                }
            }
            catch (Exception exc)
            {
                if (exc is MongoWriteException && (exc as MongoWriteException)?.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    _logger/*.ForContext("Progetti", JsonSerializer.Serialize(progettoContentDto))*/.
                        Warning("Uscita controller ProgettiController, richiesta PostProgetto. Chiave duplicata.");

                    return Conflict(ErrorDtoBuilder.New.Add("Reason", "DuplicateKey").Build());
                }

                _logger/*.ForContext("Progetti", JsonSerializer.Serialize(progettoContentDto))*/.
                        Error($"Uscita controller ProgettiController, richiesta PostProgetto. Dettaglio eccezione {exc}.");

                return StatusCode(StatusCodes.Status500InternalServerError, exc.Message);
                //return BadRequest(exc.Message);
            }

            _logger/*.ForContext("Progetti", JsonSerializer.Serialize(progettoContentDto))*/.
                   Information("Uscita controller ProgettiController, richiesta PostProgetto. Completata correttamente.");

            return CreatedAtAction(nameof(GetProgetto), new { id = progetto.Id }, _mapper.Map<ProgettoDto>(progetto));
        }

        // DELETE: api/Progetti/5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
        [HttpDelete("{id}")]
        [EnableCors("corsPolicy")]
        public async Task<IActionResult> DeleteProgetto(Guid id)
        {
            _logger.ForContext("Guid", id).
                Information("Accesso controller ProgettiController, richiesta DeleteProgetto.");

            var progetto = await _mongoDbService.ProgettiCollection.Find(p => p.Id == id && OpereUtenteIds.Contains(p.OperaId)).FirstOrDefaultAsync();

            if (progetto == null)
            {
                _logger.ForContext("Guid", id).
                    Warning("Uscita controller ProgettiController, richiesta DeleteProgetto. Not found.");

                return NotFound();
            }
                     
            var result = await _mongoDbService.DeleteProgetti(new List<Guid>() { id });
            //var deletedCount = await _mongoDbService.DeleteProgetti(new Guid[] { id });

            if (result == 0)
            //if (deletedCount == 0)
            {
                _logger.ForContext("Guid", id).
                   Error("Uscita controller ProgettiController, richiesta DeleteProgetto. Internal server error.");

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.ForContext("Guid", id).
                   Error("Uscita controller ProgettiController, richiesta DeleteProgetto. No content.");

            return NoContent();
        }

        // GET: api/Progetti/progetto-content?progettoId=5de7a53b-eb4d-4337-8bf8-934eb03b1ed5
        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("progetto-content")]
        public async Task<ActionResult<ProgettoContentDto>> GetProgettoContent(Guid progettoId)
        {
            _logger.ForContext("Guid", progettoId).
                Information("Accesso controller ProgettiController, richiesta GetProgettoContent.");

            var progetto = await _mongoDbService.ProgettiCollection.Find(p => p.Id == progettoId && OpereUtenteIds.Contains(p.OperaId)).FirstOrDefaultAsync();

            if (progetto == null)
            {
                _logger.ForContext("Guid", progettoId).
                    Warning("Uscita controller ProgettiController, richiesta GetProgettoContent. Record not found.");

                return NotFound(ErrorDtoBuilder.New.Add("Reason", "RecordNotFound").Build());
            }

            var progettoContent = _mapper.Map<ProgettoContentDto>(progetto);  
            
            ProjectDocuments prjDocs = new ProjectDocuments(_mapper);
            await _mongoDbService.LoadProjectDocuments(progettoId, prjDocs);
            Project prj = prjDocs.ToProject();
            string prjAsString = ModelData.ModelSerializer.Serialize(prj);
            

            progettoContent.Content = prjAsString;
            _logger.ForContext("Guid", progettoId)./*.ForContext("ProgettoContent", JsonSerializer.Serialize(progettoContent)).*/
                    Information("Uscita controller ProgettiController, richiesta GetProgettoContent. Completata correttamente.");

            return progettoContent;
        }

        //[HttpGet]
        //[Route("get-by-cliente")]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<ProgettoInfoDto>>> GetProgettiCliente(Guid clienteId)
        //{
        //    var progetti = (from p in _mongoDbService.ProgettiCollection.AsQueryable(null)
        //                    join o in _mongoDbService.OpereCollection.AsQueryable(null) on p.OperaId equals o.Id
        //                    join s in _mongoDbService.SettoriCollection.AsQueryable(null) on o.SettoreId equals s.Id
        //                    where s.ClienteId == clienteId
        //                    select p).Distinct();

        //    return Ok(_mapper.Map<List<ProgettoInfoDto>>(progetti.ToList()));
        //}

        //[HttpGet]
        //[Route("get-by-opera")]
        //[Authorize]
        //public async Task<ActionResult<IEnumerable<ProgettoInfoDto>>> GetProgettiOpera(Guid operaId)
        //{
        //    var progetti = (from p in _mongoDbService.ProgettiCollection.AsQueryable(null)
        //                    where p.OperaId == operaId
        //                    select p).Distinct();

        //

        #region Heartbeat

        private DateTime _lastHeartbeat = DateTime.UtcNow;
        private bool _heartbeatLost = false;

        [HttpGet]
        [EnableCors("corsPolicy")]
        [Route("heartbeat")]
        public IActionResult GetHeartbeat()
        {
            _logger.
                Information("Entrata controller ProgettiController, richiesta GetHeartbeat.");

            _lastHeartbeat = DateTime.UtcNow;
            _heartbeatLost = false;
            return Ok("Heartbeat received.");
        }

        public void CheckHeartbeat()
        {
            _logger.
                Information("Entrata controller ProgettiController, richiesta CheckHeartbeat.");

            if (DateTime.UtcNow - _lastHeartbeat > TimeSpan.FromMinutes(1))
            {
                _heartbeatLost = true;
            }
        }

        #endregion

    }
}
