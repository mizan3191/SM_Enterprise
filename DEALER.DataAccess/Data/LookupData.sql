INSERT INTO [Users] (
    [Name],
    [UserName],
    [Password],
    [Email],
    [UserRole],
    [IsDisable]
)
VALUES (
    'Super Admin',
    'SuperAdmin',
    'JxNGvi6if82fQBQEcrk+LpX5D5NpBRmBy2od3cp8SOU=', 
    'admin@example.com',
    '1',
    0
);

-- SuperAdmin
--Super@dmin


INSERT INTO [CompanyInfos] (
    [Name],
    [Phone],
    [WhatsApp],
    [WebSiteLink],
    [Email],
    [Email2],
    [DhakaOfficeAddress],
    [HeadOfficeAddress],
    [SpecialNotice],
    [FacebookPage],
    [FacebookPage2],
    [FacebookPage3],
    [Logo],
    [Bkash],
    [Nagad],
    [Rocket]
)
VALUES (
    'SM ENTERPRISE.',
    '01686257221',
    '01896092680',
    'https://www.boniyadi.com',
    'info@boniyadi.com',
    'support@boniyadi.com',
    '144/2 Golgonda, Taltola, Mymensingh',
    'Main Office Address',
    'SM ENTERPRISE!',
    'https://facebook.com/SMENTERPRISE',
    '',
    '',
    NULL,
    '017XXXXXXXX',
    '017XXXXXXXX',
    '017XXXXXXXX'
);



INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (1, '5.5KG', 'Small domestic cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (2, '12KG', 'Regular domestic cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (3, '12.5KG', 'Regular domestic cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (4, '15KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (5, '16KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (6, '18KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (7, '20KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (8, '22KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (9, '25KG', 'Large commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (10, '30KG', 'Large commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (11, '33KG', 'Large commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (12, '35KG', 'Medium bulk cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (13, '45KG', 'Large bulk cylinder', 0);


INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (1, 'Electricity Bill', 'Monthly electricity expenses',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (2, 'Water Bill', 'Monthly water usage charges',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (3, 'Internet Bill', 'Monthly internet charges',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (4, 'Staff Salary', 'Salaries for staff members',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (7, 'Home Rent', 'Monthly house rent',0);

INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (1, N'Cash', N'Cash',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (2, N'bKash', N'bKash',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (3, N'Nagad', N'Nagad',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (4, N'Rocket', N'Rocket',0)

INSERT INTO ShippingMethod (Id, Name, Description, IsDeleted) VALUES (1, 'Self Pickup', 'Customer picks up the product from the store',0);
INSERT INTO ShippingMethod (Id, Name, Description, IsDeleted) VALUES (2, 'Bike/biCycle Delivery', 'Local delivery using a bicycle or motorbike',0);

INSERT INTO CustomerType (Id, Name, Description, IsDeleted)
VALUES (1, 'Regular Customer', 'Customer who purchases regularly', 0);
INSERT INTO CustomerType (Id, Name, Description, IsDeleted)
VALUES (2, 'Corporate Customer', 'Corporate or business customer', 0);
INSERT INTO CustomerType (Id, Name, Description, IsDeleted)
VALUES (3, 'VIP Customer', 'Premium or VIP customer', 0);
INSERT INTO CustomerType (Id, Name, Description, IsDeleted)
VALUES (4, 'Member', 'Registered loyalty or membership customer', 0);
INSERT INTO CustomerType (Id, Name, Description, IsDeleted)
VALUES (5, 'Company Staff', 'Company staff or internal employee', 0); 
INSERT INTO CustomerType (Id, Name, Description, IsDeleted)
VALUES (6, 'Wholesale Customer', 'Customer who purchases products in bulk', 0);


INSERT INTO EmployeeType (Id, Name, Description, IsDeleted) VALUES (1, 'Shop Owner', 'Shop Owner',0);
INSERT INTO EmployeeType (Id, Name, Description, IsDeleted) VALUES (2, 'Staff', 'Company staff or internal employee',0);
INSERT INTO EmployeeType (Id, Name, Description, IsDeleted) VALUES (3, 'Driver', 'Driver',0);

INSERT INTO DailyExpenseType (Id, Name, Description, IsDeleted) VALUES (1, 'Personal Due', 'Personal Due',0);
INSERT INTO DailyExpenseType (Id, Name, Description, IsDeleted) VALUES (2, 'Bazar Khoroc', 'Bazar Khoroc',0);
INSERT INTO DailyExpenseType (Id, Name, Description, IsDeleted) VALUES (3, 'Guest Entertainment', 'Expenses for guest hospitality',0);





--DELETE FROM PriceHistories;
--DBCC CHECKIDENT ('PriceHistories', RESEED, 0);

--DELETE FROM ProductPrices;
--DBCC CHECKIDENT ('ProductPrices', RESEED, 0);

--DELETE FROM ProductStocks;
--DBCC CHECKIDENT ('ProductStocks', RESEED, 0);

--DELETE FROM TransactionHistories;
--DBCC CHECKIDENT ('TransactionHistories', RESEED, 0);

--DELETE FROM SupplierPaymentHistories;
--DBCC CHECKIDENT ('SupplierPaymentHistories', RESEED, 0);

--DELETE FROM CustomerPaymentHistories;
--DBCC CHECKIDENT ('CustomerPaymentHistories', RESEED, 0);

--DELETE FROM PurchaseDetails;
--DBCC CHECKIDENT ('PurchaseDetails', RESEED, 0);

--DELETE FROM Purchases;
--DBCC CHECKIDENT ('Purchases', RESEED, 0);

--DELETE FROM CylinderExchanges;
--DBCC CHECKIDENT ('CylinderExchanges', RESEED, 0);

--DELETE FROM CylinderExchangeDetails;
--DBCC CHECKIDENT ('CylinderExchangeDetails', RESEED, 0);

--DELETE FROM ExchangePaymentHistories;
--DBCC CHECKIDENT ('ExchangePaymentHistories', RESEED, 0);

--DELETE FROM ProductConsumptions;
--DBCC CHECKIDENT ('ProductConsumptions', RESEED, 0);

--DELETE FROM Expenses;
--DBCC CHECKIDENT ('Expenses', RESEED, 0);

--DELETE FROM DSRShopDues;
--DBCC CHECKIDENT ('DSRShopDues', RESEED, 0);

--DELETE FROM ShopEmptyCylinderProducts;
--DBCC CHECKIDENT ('ShopEmptyCylinderProducts', RESEED, 0);

--DELETE FROM CustomerProductReturns;
--DBCC CHECKIDENT ('CustomerProductReturns', RESEED, 0);

--DELETE FROM CustomerProductReturnDetails;
--DBCC CHECKIDENT ('CustomerProductReturnDetails', RESEED, 0);

--DELETE FROM EmptyCylinderPaymentHistories;
--DBCC CHECKIDENT ('EmptyCylinderPaymentHistories', RESEED, 0);

--DELETE FROM DSRShopPaymentHistories;
--DBCC CHECKIDENT ('DSRShopPaymentHistories', RESEED, 0);

--DELETE FROM OrderDetails;
--DBCC CHECKIDENT ('OrderDetails', RESEED, 0);

--DELETE FROM OrderPaymentHistories;
--DBCC CHECKIDENT ('OrderPaymentHistories', RESEED, 0);

--DELETE FROM DailyExpenses;
--DBCC CHECKIDENT ('DailyExpenses', RESEED, 0);

--DELETE FROM Orders;
--DBCC CHECKIDENT ('Orders', RESEED, 0);




