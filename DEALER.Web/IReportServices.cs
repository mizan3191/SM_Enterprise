using System.Data;

namespace DEALER.Web
{
    public interface IReportServices
    {
        DataTable PrintInvoiceById(int id);
        DataTable OrderInfoById(int id);
        DataTable GetOrderDataById(int id);
        DataTable GetCompanyInfo();
        DataTable DuePayment(int id);
        DataTable GetCustomerInfo(int id);
    }
}