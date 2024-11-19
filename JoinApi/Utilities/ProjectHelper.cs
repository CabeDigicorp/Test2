using JoinApi.Controllers;
using JoinApi.Service;
using Microsoft.AspNetCore.Http.Extensions;
using MongoDB.Driver;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Web;

namespace JoinApi.Utilities
{
    public class ProjectHelper
    {
        readonly MongoDbService _mongoDBService;
        Guid _progettoId = Guid.Empty;
        

        public ProjectHelper(MongoDbService mongoDBService, Guid progettoId)
        {
            _mongoDBService = mongoDBService;
            _progettoId = progettoId;
            
            

                
        }

        private static async Task<bool> PingAsync(Uri uri)
        {
            //var hostUrl = "www.code4it.dev";
            Ping ping = new Ping();

            PingReply result = await ping.SendPingAsync(uri.Host);
            return result.Status == IPStatus.Success;
        }

        public static async Task<bool> IsValidUrl(Uri uri)
        {
            bool ok = await PingAsync(uri);

            if (uri.AbsolutePath != @"/api/Allegati/download")
                ok = false;

            return ok;
        }


        public List<string> GetModel3dFilesName()
        {
            List<string> filesName = new List<string>();

            var model3dFilesInfo = _mongoDBService.Model3dFileCollection.Find(p => p.ProgettoId == _progettoId).FirstOrDefault();
            foreach (var model3dFileInfo in model3dFilesInfo.Items)
            {
                string url = model3dFileInfo.FilePath;

                string? fileName = string.Empty;
                string? operaId = string.Empty;

                Uri uri = new Uri(url);

                if (!IsValidUrl(uri).Result)
                    continue;


                fileName = HttpUtility.ParseQueryString(uri.Query).Get("fileName");
                operaId = HttpUtility.ParseQueryString(uri.Query).Get("operaId");

                if (!string.IsNullOrEmpty(fileName))
                {
                    filesName.Add(fileName);    
                }

            }

            return filesName;
        }





    }
}
