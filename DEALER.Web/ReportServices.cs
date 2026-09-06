using System.Data;

namespace DEALER.Web
{
    public class ReportServices : IReportServices
    {
        private readonly IOrder m_Order;
        private readonly ICompany m_Company;
        private readonly ICustomer m_Customer;

        public ReportServices(IOrder order, ICompany company, ICustomer customer)
        {
            m_Order = order;
            m_Company = company;
            m_Customer = customer;
        }

        public DataTable OrderInfoById(int id)
        {
            var order = m_Order.OrderInfoById(id);

            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            dt.Columns.Add("Email");
            dt.Columns.Add("PhoneNumber");
            dt.Columns.Add("FullAddress");
            dt.Columns.Add("OrderDateFormated");
            dt.Columns.Add("OrderId");
            dt.Columns.Add("PaymentMethod");
            dt.Columns.Add("DeliveryLocation");
            dt.Columns.Add("ShippingMethod");

            DataRow dataRow = dt.NewRow();

            dataRow["Id"] = order.Id;
            dataRow["Name"] = order.Name;
            dataRow["Email"] = order.Email;
            dataRow["PhoneNumber"] = order.PhoneNumber;
            dataRow["FullAddress"] = order.FullAddress;
            dataRow["OrderDateFormated"] = order.OrderDateFormated;
            dataRow["OrderId"] = order.OrderId;
            dataRow["PaymentMethod"] = order.PaymentMethod;
            dataRow["DeliveryLocation"] = order.DeliveryLocation;
            dataRow["ShippingMethod"] = order.ShippingMethod;

            dt.Rows.Add(dataRow);

            return dt;
        }

        public DataTable PrintInvoiceById(int id)
        {
            var order = m_Order.GetOrdersById(id);

            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Name");
            dt.Columns.Add("Email");
            dt.Columns.Add("PhoneNumber");
            dt.Columns.Add("FullAddress");

            DataRow dataRow = dt.NewRow();

            dataRow["Id"] = order.Id;
            dataRow["Name"] = order.Name;
            dataRow["Email"] = order.Email;
            dataRow["PhoneNumber"] = order.PhoneNumber;
            dataRow["FullAddress"] = order.FullAddress;

            dt.Rows.Add(dataRow);

            return dt;
        }

        public DataTable GetOrderDataById(int id)
        {
            var orderlist = m_Order.GetOrderDetailsByOrderId(id);
            var getPayment = m_Order.GetPayment(id);

            DataTable dtorderlist = new DataTable();
            dtorderlist.Columns.Add("ProductName");
            dtorderlist.Columns.Add("Quantity");
            dtorderlist.Columns.Add("Discount");
            dtorderlist.Columns.Add("ProductPrice");
            dtorderlist.Columns.Add("TotalPrice");

            foreach (var order in orderlist)
            {
                DataRow dataRowdtorderlist = dtorderlist.NewRow();

                dataRowdtorderlist["ProductName"] = order.ProductName;
                dataRowdtorderlist["Quantity"] = order.Quantity;
                dataRowdtorderlist["Discount"] = order.Discount;
                dataRowdtorderlist["ProductPrice"] = order.ProductPrice;
                dataRowdtorderlist["TotalPrice"] = order.TotalPrice;

                dtorderlist.Rows.Add(dataRowdtorderlist);
            }

            foreach (var order in getPayment)
            {
                DataRow dataRowdtorderlist = dtorderlist.NewRow();

                dataRowdtorderlist["ProductName"] = order.name;
                dataRowdtorderlist["TotalPrice"] = order.value;

                dtorderlist.Rows.Add(dataRowdtorderlist);
            }

            return dtorderlist;
        }

        public DataTable GetCompanyInfo()
        {
            var company = m_Company.GetCompanyInfo();

            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Phone");
            dt.Columns.Add("WhatsApp");
            dt.Columns.Add("WebSiteLink");
            dt.Columns.Add("Email");
            dt.Columns.Add("Email2");
            dt.Columns.Add("DhakaOfficeAddress");
            dt.Columns.Add("HeadOfficeAddress");
            dt.Columns.Add("SpecialNotice");
            dt.Columns.Add("FacebookPage");
            dt.Columns.Add("FacebookPage2");
            dt.Columns.Add("FacebookPage3");
            dt.Columns.Add("Bkash");
            dt.Columns.Add("Nagad");
            dt.Columns.Add("Rocket");
            dt.Columns.Add("Image");

            DataRow dataRow = dt.NewRow();

            dataRow["Name"] = company.Name;
            dataRow["Phone"] = company.Phone + ", " + company.WhatsApp;
            dataRow["WhatsApp"] = company.WhatsApp;
            dataRow["WebSiteLink"] = company.WebSiteLink;
            dataRow["Email"] = company.Email;
            dataRow["Email2"] = company.Email2;
            dataRow["DhakaOfficeAddress"] = company.DhakaOfficeAddress;
            dataRow["HeadOfficeAddress"] = company.HeadOfficeAddress;
            dataRow["SpecialNotice"] = company.SpecialNotice;
            dataRow["FacebookPage"] = company.FacebookPage;
            dataRow["FacebookPage2"] = company.FacebookPage2;
            dataRow["FacebookPage3"] = company.FacebookPage3;
            dataRow["Bkash"] = company.Bkash;
            dataRow["Nagad"] = company.Nagad;
            dataRow["Rocket"] = company.Rocket;
            dataRow["Image"] = company.Image;

            dt.Rows.Add(dataRow);

            return dt;
        }
        
        public DataTable DuePayment(int id)
        {
            var duePaymnet = m_Order.DuePayment(id); 

            DataTable dt = new DataTable();
            dt.Columns.Add("TotalAmountThisOrder");
            dt.Columns.Add("AmountPaid");
            dt.Columns.Add("TotalDueBeforePayment");
            dt.Columns.Add("TotalDueAfterPayment");
            dt.Columns.Add("PaymentDate");
            dt.Columns.Add("PaymentMethod");
            dt.Columns.Add("TransactionID");
            dt.Columns.Add("Number");

            DataRow dataRow = dt.NewRow();

            dataRow["TotalAmountThisOrder"] = duePaymnet.TotalAmountThisOrder;
            dataRow["AmountPaid"] = duePaymnet.AmountPaid;
            dataRow["TotalDueBeforePayment"] = duePaymnet.TotalDueBeforePayment;
            dataRow["TotalDueAfterPayment"] = duePaymnet.TotalDueAfterPayment;
            dataRow["PaymentDate"] = duePaymnet.PaymentDate;
            dataRow["PaymentMethod"] = duePaymnet.PaymentMethod;
            dataRow["TransactionID"] = duePaymnet.TransactionID;
            dataRow["Number"] = duePaymnet.Number;

            dt.Rows.Add(dataRow);

            return dt;
        }
        
        public DataTable GetCustomerInfo(int id)
        {
            var customer = m_Customer.GetCustomer(id); 

            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Email");
            dt.Columns.Add("Phone");
            dt.Columns.Add("FullAddress");

            DataRow dataRow = dt.NewRow();

            dataRow["Name"] = customer.Name;
            dataRow["Email"] = customer.Email;
            dataRow["Phone"] = customer.Phone;
            dataRow["FullAddress"] = customer.Address;

            dt.Rows.Add(dataRow);

            return dt;
        }

    }
}