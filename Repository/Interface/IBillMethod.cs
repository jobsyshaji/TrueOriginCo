using CherukarasThejas.Models.Response;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillManagementSoftware.Repository.Interface
{
    public interface IBillMethod
    {
        Task<BaseResponse<Empty>> SaveBillInfo();
        Task<string> GetCustomerName(string mobileNo);
    }
}
