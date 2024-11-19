using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using JoinService.Models;


namespace JoinService.Controllers
{


    public class PrezzariController : ApiController
    {

        PrezzariInfo PrezzariInfo { get; set; } = new PrezzariInfo();


        //GET: api/Prezzari
        [HttpGet]
        public IEnumerable<PrezzarioInfo> Get()
        {
            IEnumerable<PrezzarioInfo> items = PrezzariInfo.GetItems();
            return items.ToArray();
        }

        //GET: api/Prezzari/filename
        [HttpGet]
        public HttpResponseMessage Download(string id)
        {

            PrezzarioInfo prezInfo = PrezzariInfo.GetPrezzarioInfo(id);

            FileStream fileStream = PrezzariInfo.Open(id);

            if (fileStream == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var response = new HttpResponseMessage();
            response.Content = new StreamContent(fileStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = PrezzariInfo.AddFileExtension(prezInfo.FileName);

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = PrezzariInfo.GetLength(id);
            return response;
        }


        // GET: api/Prezzari
        //[HttpGet]
        //public IEnumerable<int> Get()
        //{
        //    List<int> files = PrezzariInfo.GetPrezzariId();
        //    return files.ToArray();
        //    //return new string[] { "value1", "value2" };
        //}

        //// GET: api/Prezzari/5
        //[HttpGet]
        //public PrezzarioInfo Get(int id)
        //{
        //    PrezzarioInfo prezInfo = PrezzariInfo.GetPrezzarioInfo(id);

        //    return prezInfo;

        //    //string json = null;
        //    //JsonSerializer.JsonSerialize(prezInfo, out json);

        //    //return json;
        //}



        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
