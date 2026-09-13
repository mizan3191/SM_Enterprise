namespace DEALER.DataAccess
{
    public interface ILookup
    {
        #region Lookup List
        DateTime GetOrderDate(int orderId);
        (string CompanyName,string Address ) GetCompanyInfo();
        Task<IList<SalesReturnDTO>> GetAllSalesReturn(DateTime? startDate, DateTime? endDate);
        Task<IList<SalesHistoryDTO>> GetAllProductSalesHistory(DateTime? startDate, DateTime? endDate);
        Task<IList<DSRSalesReturnDTO>> GetDSRAllSalesReturn(DateTime? startDate, DateTime? endDate);
        Task<IList<TransactionHistory>> GetAllTransactionHistory(DateTime? startDate, DateTime? endDate);
        Task<IList<Lov>> GetAllProductList();
        Task<IList<Lov>> GetAllSupplierList();
       // Task<IList<Lov>> GetAllUnitOfMeasurementList();
        //Task<IList<Lov>> GetAllPackagingList();
        Task<IList<Lov>> GetAllSizeList();
        Task<IList<Lov>> GetAllRoadList();
        Task<IList<Lov>> GetAllCustomerTypeList();
        Task<IList<Lovd>> GetAllEmployeeList();
        Task<IList<Lovd>> GetAllCustomerList();
        Task<IList<Lov>> GetAllEmployeeTypeList();
        Task<IList<Lov>> GetAllExpenseTypeList();
        Task<IList<Lov>> GetAllDailyExpenseTypeList();
        Task<IList<Lov>> GetAllReasonofAdjustmentList();
        Task<IList<Lov>> GetAllPaymentMethodList();
        Task<IList<Lov>> GetAllShippingMethodList();
        Task<IList<Lov>> GetAllShopList();
        Task<IList<Lov>> GetAllPaymentTypeList();
        Task<IList<OrderExportToPdfDTO>> OrderExportToPdfList(int orderId);

        #endregion Lookup List

      



        #region Size
        bool UpdateSize(ProductsSize size);
        bool DeleteSize(int id);
        int CreateSize(ProductsSize size);
        ProductsSize GetSize(int id);
        Task<IList<ProductsSize>> GetAllSize();
        #endregion Size

        #region ReasonofAdjustment
        bool UpdateReasonofAdjustment(ReasonofAdjustment reasonofAdjustment);
        bool DeleteReasonofAdjustment(int id);
        int CreateReasonofAdjustment(ReasonofAdjustment reasonofAdjustment);
        ReasonofAdjustment GetReasonofAdjustment(int id);
        Task<IList<ReasonofAdjustment>> GetAllReasonofAdjustment();
        #endregion ReasonofAdjustment

        #region Road
        bool UpdateRoad(Road road);
        bool DeleteRoad(int id);
        int CreateRoad(Road road);
        Road GetRoad(int id);
        Task<IList<Road>> GetAllRoad();
        #endregion Road

        #region TransactionHistory
        bool UpdateTransactionHistory(TransactionHistory TransactionHistory);
        bool DeleteTransactionHistory(int id);
        int CreateTransactionHistory(TransactionHistory TransactionHistory);
        double GetLatestBalance();

        #endregion TransactionHistory

        #region Shop
        bool UpdateShop(Shop Shop);
        bool DeleteShop(int id);
        int CreateShop(Shop Shop);
        Shop GetShop(int id);
        Task<IList<Shop>> GetAllShop();
        #endregion Shop


        #region PaymentType
        bool UpdatePaymentType(PaymentType PaymentType);
        bool DeletePaymentType(int id);
        int CreatePaymentType(PaymentType PaymentType);
        PaymentType GetPaymentType(int id);
        Task<IList<PaymentType>> GetAllPaymentType();
        #endregion PaymentType

        #region CustomerType
        bool UpdateCustomerType(CustomerType customerType);
        bool DeleteCustomerType(int id);
        int CreateCustomerType(CustomerType customerType);
        CustomerType GetCustomerType(int id);
        Task<IList<CustomerType>> GetAllCustomerType();
        #endregion CustomerType



        #region EmployeeType
        bool UpdateEmployeeType(EmployeeType EmployeeType);
        bool DeleteEmployeeType(int id);
        int CreateEmployeeType(EmployeeType EmployeeType);
        EmployeeType GetEmployeeType(int id);
        Task<IList<EmployeeType>> GetAllEmployeeType();
        #endregion EmployeeType

        #region ExpenseType
        bool UpdateExpenseType(ExpenseType expenseType);
        bool DeleteExpenseType(int id);
        int CreateExpenseType(ExpenseType expenseType);
        ExpenseType GetExpenseType(int id);
        Task<IList<ExpenseType>> GetAllExpenseType();
        #endregion ExpenseType

        #region DailyExpenseType
        bool UpdateDailyExpenseType(DailyExpenseType DailyExpenseType);
        bool DeleteDailyExpenseType(int id);
        int CreateDailyExpenseType(DailyExpenseType DailyExpenseType);
        DailyExpenseType GetDailyExpenseType(int id);
        Task<IList<DailyExpenseType>> GetAllDailyExpenseType();
        #endregion DailyExpenseType

        #region PaymentMethod
        bool UpdatePaymentMethod(PaymentMethod PaymentMethod);
        bool DeletePaymentMethod(int id);
        int CreatePaymentMethod(PaymentMethod PaymentMethod);
        PaymentMethod GetPaymentMethod(int id);
        Task<IList<PaymentMethod>> GetAllPaymentMethod();
        #endregion PaymentMethod

        #region ShippingMethod
        bool UpdateShippingMethod(ShippingMethod shippingMethod);
        bool DeleteShippingMethod(int id);
        int CreateShippingMethod(ShippingMethod shippingMethod);
        ShippingMethod GetShippingMethod(int id);
        Task<IList<ShippingMethod>> GetAllShippingMethod();
        #endregion ShippingMethod
    }
}