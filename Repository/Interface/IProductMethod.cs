using CherukarasThejas.Areas.BillSoftware.Data;
using CherukarasThejas.Models.Response;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BillManagementSoftware.Repository.Interface
{
    public interface IProductMethod
    {
        Task<BaseResponse<Empty>> SaveProduct(ProductPost data); 
        Task<BaseResponse<Empty>> DeleteProduct(int pId);
        Task<List<SelectListItem>> GetProductDrpList();
        Task<string> GetProductRate(int pId);
    }
}
