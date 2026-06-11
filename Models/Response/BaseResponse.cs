using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CherukarasThejas.Models.Response
{
    public class BaseResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}