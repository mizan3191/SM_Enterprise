
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




