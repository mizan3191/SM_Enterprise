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

