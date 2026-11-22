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
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (6, '18KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (7, '20KG', 'Commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (10, '30KG', 'Large commercial cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (12, '35KG', 'Medium bulk cylinder', 0);
INSERT INTO ProductsSize (Id, Name, Description, IsDeleted) VALUES (13, '45KG', 'Large bulk cylinder', 0);

INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (1, N'Empty Cylinder Return', N'Empty Cylinder Return',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (2, N'Daily Expense', N'Daily Expense',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (3, N'Shop Due', N'Shop Due',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (4, N'Payment in Order', N'Payment in Order',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (5, N'Product Return', N'Product Return',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (6, N'bKash', N'bKash',0)

INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (10, N'Cash', N'Cash',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (11, N'bKash', N'bKash',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (12, N'Nagad', N'Nagad',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (13, N'Rocket', N'Rocket',0)
INSERT [dbo].[PaymentMethod] ([Id], [Name], [Description], IsDeleted) VALUES (14, N'Bank', N'Bank',0)


INSERT INTO ShippingMethod (Id, Name, Description, IsDeleted) VALUES (1, 'Self Pickup', 'Customer picks up the product from the store',0);
INSERT INTO ShippingMethod (Id, Name, Description, IsDeleted) VALUES (2, 'Auto Rickshaw', 'Local delivery using a Auto Rickshaw',0);
INSERT INTO ShippingMethod (Id, Name, Description, IsDeleted) VALUES (3, 'Truck', 'Truck',0);

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
INSERT INTO EmployeeType (Id, Name, Description, IsDeleted) VALUES (4, 'SR', 'SR',0);

INSERT INTO DailyExpenseType (Id, Name, Description, IsDeleted) VALUES (1, 'Personal Due', 'Personal Due',0);
INSERT INTO DailyExpenseType (Id, Name, Description, IsDeleted) VALUES (2, 'Bazar Khoroc', 'Bazar Khoroc',0);
INSERT INTO DailyExpenseType (Id, Name, Description, IsDeleted) VALUES (3, 'Guest Entertainment', 'Expenses for guest hospitality',0);


INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (1, 'Electricity Bill', 'Monthly electricity expenses',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (2, 'Water Bill', 'Monthly water usage charges',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (3, 'Internet Bill', 'Monthly internet charges',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (4, 'Staff Salary', 'Salaries for staff members',0);
INSERT INTO ExpenseType (Id, Name, Description, IsDeleted) VALUES (7, 'Home Rent', 'Monthly house rent',0);

INSERT INTO YearlyExpenseType (Id, Name, Description, IsDeleted)
VALUES
(1, 'Asset Purchase', 'Car, motorcycle, land, house and other asset purchases', 0),
(2, 'License & Registration', 'Shop license, trade license, vehicle registration and renewal fees', 0),
(3, 'Business Investment', 'Shop setup, business equipment and other business investments', 0),
(4, 'Property & Construction', 'Land development, house construction, renovation and property-related expenses', 0),
(5, 'Other Yearly Expense', 'Other major yearly expenses not covered by existing categories', 0);


INSERT [dbo].PaymentType ([Id], [Name], [Description], IsDeleted) VALUES (1, N'Advance Salary', N'Advance Salary',0)
INSERT [dbo].PaymentType ([Id], [Name], [Description], IsDeleted) VALUES (2, N'Inceptive', N'Inceptive',0)
INSERT [dbo].PaymentType ([Id], [Name], [Description], IsDeleted) VALUES (3, N'Partial Salary', N'Partial Salary',0)
INSERT [dbo].PaymentType ([Id], [Name], [Description], IsDeleted) VALUES (4, N'Salary', N'Salary',0)
INSERT [dbo].PaymentType ([Id], [Name], [Description], IsDeleted) VALUES (5, N'Other', N'Other',0)

