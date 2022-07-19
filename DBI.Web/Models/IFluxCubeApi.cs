using DBI_eDahab.Web.ViewModels;
using System.Threading.Tasks;

namespace DBI_eDahab.Web.Models
{
    public interface IFluxCubeApi
    {
        Task<AccountInfoRespone> GetCustomerInfo(AccountInfoRequest accountInfoRequest);
        Task SendSmsAsync(string title, string phone, string message);
    }
}