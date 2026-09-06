using AspNetCore.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection;

namespace DEALER.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IReportServices _services;

        public ReportsController(IReportServices services, IWebHostEnvironment webHostEnvironment)
        {
            _services = services;

            this._webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }


        [HttpGet("OrderDetail/{orderId}")]
        public IActionResult OrderDetail(int orderId)
        {
            var data = new OrderDetail
            {
                Id = 1,
                ProductId = 10,
                OrderId = 12,
                Quantity = 15,
                Price = 150,
            };

            return Ok(data);
        }

        [HttpGet("OrderDetailsReport/{orderId}")]
        public IActionResult OrderReport(int orderId)
        {
            var dt = new DataTable();
            var allOrderlist = new DataTable();
            var company = new DataTable();

            dt = _services.PrintInvoiceById(orderId);
            allOrderlist = _services.GetOrderDataById(orderId);
            company = _services.GetCompanyInfo();

            string mimetype = "";
            int extension = 1;

            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\OrderDetails.rdlc";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ReportParameterOrderDetails", "Order Details");

            // Create the LocalReport instance
            LocalReport localReport = new LocalReport(path);

            // Enable external images using reflection
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = localReport.GetType().GetField("localReport", bindFlags);
            object rptObj = field.GetValue(localReport);
            Type type = rptObj.GetType();
            PropertyInfo pi = type.GetProperty("EnableExternalImages", bindFlags);
            pi.SetValue(rptObj, true, null);

            // Add the data sources
            localReport.AddDataSource("DataSetOrderDetails", dt);
            localReport.AddDataSource("DataSetGetOrderDataById", allOrderlist);
            localReport.AddDataSource("DataSetCompanyInfo", company);

            var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }


        [HttpGet("OrderDetailsA4Report/{orderId}")]
        public IActionResult OrderA4Report(int orderId)
        {
            var dt = new DataTable();
            var allOrderlist = new DataTable();
            var company = new DataTable();

            dt = _services.OrderInfoById(orderId);
            allOrderlist = _services.GetOrderDataById(orderId);
            company = _services.GetCompanyInfo();

            string mimetype = "";
            int extension = 1;

            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\OrderDetailsA4.rdlc";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ReportParameterOrderA4Details", "Order Details");

            // Create the LocalReport instance
            LocalReport localReport = new LocalReport(path);

            // Enable external images using reflection
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = localReport.GetType().GetField("localReport", bindFlags);
            object rptObj = field.GetValue(localReport);
            Type type = rptObj.GetType();
            PropertyInfo pi = type.GetProperty("EnableExternalImages", bindFlags);
            pi.SetValue(rptObj, true, null);

            // Add the data sources
            localReport.AddDataSource("DataSetOrderInfoById", dt);
            localReport.AddDataSource("DataSetGetOrderDataById", allOrderlist);
            localReport.AddDataSource("DataSetCompanyInfoForA4", company);

            var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }



        [HttpGet("DuePaymentReport/{paymentId}")]
        public IActionResult DuePaymentReport(int customerId, int paymentId)
        {
            var dt = new DataTable();
            dt = _services.DuePayment(paymentId);

            var company = new DataTable();
            company = _services.GetCompanyInfo();

            var customerInfo = new DataTable();
            customerInfo = _services.GetCustomerInfo(customerId);

            string mimetype = "";
            int extension = 1;

            var path = $"{this._webHostEnvironment.WebRootPath}\\Reports\\DuePaymentReport.rdlc";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("ReportParameterDuePayment", "Customer Due Payment");

            // Create the LocalReport instance
            LocalReport localReport = new LocalReport(path);

            // Enable external images using reflection
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = localReport.GetType().GetField("localReport", bindFlags);
            object rptObj = field.GetValue(localReport);
            Type type = rptObj.GetType();
            PropertyInfo pi = type.GetProperty("EnableExternalImages", bindFlags);
            pi.SetValue(rptObj, true, null);

            // Add the data sources
            localReport.AddDataSource("DataSetDuePayment", dt);
            localReport.AddDataSource("DataSetCompanyInfo", company);
            localReport.AddDataSource("DataSetCustomerInfo", customerInfo);

            var result = localReport.Execute(RenderType.Pdf, extension, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }
    }
}
