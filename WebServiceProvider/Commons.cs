using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceClient
{

    public class GenericResponse
    {
        public GenericResponse(bool success) { Success = success; Message = string.Empty; }


        public GenericResponse(bool success, string reason)
        {
            Set(success, reason);
        }

        public void Set(bool success, string msg) { Success = success; Message = msg; }

        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
    }

    public class AddResponse : GenericResponse
    {
        public AddResponse(bool success) : base(success) { }

        public Guid NewId { get; set; } = Guid.Empty;

    }



}
