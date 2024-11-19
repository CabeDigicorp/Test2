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


    public class ModelliController : ApiController
    {

        ModelliInfo ModelliInfo { get; set; } = new ModelliInfo();


        //GET: api/Modelli
        [HttpGet]
        public IEnumerable<ModelloInfo> Get()
        {
            IEnumerable<ModelloInfo> items = ModelliInfo.GetItems();
            return items.ToArray();
        }

        //GET: api/Modelli/filename
        [HttpGet]
        public HttpResponseMessage Download(string id)
        {

            ModelloInfo modInfo = ModelliInfo.GetModelloInfo(id);

            FileStream fileStream = ModelliInfo.Open(id);

            if (fileStream == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var response = new HttpResponseMessage();
            response.Content = new StreamContent(fileStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = ModelliInfo.AddFileExtension(modInfo.FileName);

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = ModelliInfo.GetLength(id);
            return response;
        }



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
