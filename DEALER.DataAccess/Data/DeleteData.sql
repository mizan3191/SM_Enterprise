
DELETE FROM PreOrderDetails;
DBCC CHECKIDENT ('PreOrderDetails', RESEED, 0);

DELETE FROM PreOrderPayments;
DBCC CHECKIDENT ('PreOrderPayments', RESEED, 0);

DELETE FROM PreOrderDeliveries;
DBCC CHECKIDENT ('PreOrderDeliveries', RESEED, 0);

DELETE FROM PreOrderDeliveryDetails;
DBCC CHECKIDENT ('PreOrderDeliveryDetails', RESEED, 0);

DELETE FROM PreOrders;
DBCC CHECKIDENT ('PreOrders', RESEED, 0);

DELETE FROM PriceHistories;
DBCC CHECKIDENT ('PriceHistories', RESEED, 0);

DELETE FROM ProductPrices;
DBCC CHECKIDENT ('ProductPrices', RESEED, 0);

DELETE FROM ProductStocks;
DBCC CHECKIDENT ('ProductStocks', RESEED, 0);

DELETE FROM TransactionHistories;
DBCC CHECKIDENT ('TransactionHistories', RESEED, 0);

DELETE FROM SupplierPaymentHistories;
DBCC CHECKIDENT ('SupplierPaymentHistories', RESEED, 0);

DELETE FROM CustomerPaymentHistories;
DBCC CHECKIDENT ('CustomerPaymentHistories', RESEED, 0);

DELETE FROM PurchaseDetails;
DBCC CHECKIDENT ('PurchaseDetails', RESEED, 0);

DELETE FROM Purchases;
DBCC CHECKIDENT ('Purchases', RESEED, 0);

DELETE FROM CylinderExchanges;
DBCC CHECKIDENT ('CylinderExchanges', RESEED, 0);

DELETE FROM CylinderExchangeDetails;
DBCC CHECKIDENT ('CylinderExchangeDetails', RESEED, 0);

DELETE FROM ExchangePaymentHistories;
DBCC CHECKIDENT ('ExchangePaymentHistories', RESEED, 0);

DELETE FROM ProductConsumptions;
DBCC CHECKIDENT ('ProductConsumptions', RESEED, 0);

DELETE FROM Expenses;
DBCC CHECKIDENT ('Expenses', RESEED, 0);

DELETE FROM YearlyExpenses;
DBCC CHECKIDENT ('YearlyExpenses', RESEED, 0);

DELETE FROM DailyExpenses;
DBCC CHECKIDENT ('DailyExpenses', RESEED, 0);

DELETE FROM DSRShopDues;
DBCC CHECKIDENT ('DSRShopDues', RESEED, 0);

DELETE FROM ShopEmptyCylinderProducts;
DBCC CHECKIDENT ('ShopEmptyCylinderProducts', RESEED, 0);

DELETE FROM CustomerProductReturns;
DBCC CHECKIDENT ('CustomerProductReturns', RESEED, 0);

DELETE FROM CustomerProductReturnDetails;
DBCC CHECKIDENT ('CustomerProductReturnDetails', RESEED, 0);

DELETE FROM EmptyCylinderPaymentHistories;
DBCC CHECKIDENT ('EmptyCylinderPaymentHistories', RESEED, 0);

DELETE FROM DSRShopPaymentHistories;
DBCC CHECKIDENT ('DSRShopPaymentHistories', RESEED, 0);

DELETE FROM OrderDetails;
DBCC CHECKIDENT ('OrderDetails', RESEED, 0);

DELETE FROM OrderPaymentHistories;
DBCC CHECKIDENT ('OrderPaymentHistories', RESEED, 0);

DELETE FROM DailyExpenses;
DBCC CHECKIDENT ('DailyExpenses', RESEED, 0);

DELETE FROM Orders;
DBCC CHECKIDENT ('Orders', RESEED, 0);

DELETE FROM Purchases;
DBCC CHECKIDENT ('Purchases', RESEED, 0);

DELETE FROM StaffSalaries;
DBCC CHECKIDENT ('StaffSalaries', RESEED, 0);



--Dont execute those

----DELETE FROM Customers;
----DBCC CHECKIDENT ('Customers', RESEED, 0);

----DELETE FROM Employees;
----DBCC CHECKIDENT ('Employees', RESEED, 0);

----DELETE FROM DealerPoints;
----DBCC CHECKIDENT ('DealerPoints', RESEED, 0);

----DELETE FROM Suppliers;
----DBCC CHECKIDENT ('Suppliers', RESEED, 0);

--Dont execute those

--Dont execute those

--------DELETE FROM UnitOfMeasurement;
--------DELETE FROM ReasonofAdjustment;
--------DELETE FROM ProductsSize;
--------DELETE FROM Road;
--------DELETE FROM PaymentType;
--------DELETE FROM ExpenseType;
--------DELETE FROM DailyExpenseType;
--------DELETE FROM YearlyExpenseType;
--------DELETE FROM CustomerType;
--------DELETE FROM EmployeeType;
--------DELETE FROM PaymentMethod;
--------DELETE FROM ShippingMethod;
--------DELETE FROM Shop;

--Dont execute those




