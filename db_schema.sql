CREATE TABLE [Accounts] (
    [AccountId] bigint NOT NULL IDENTITY,
    [Username] varchar(50) NOT NULL,
    [Email] varchar(100) NOT NULL,
    [PasswordHash] varchar(255) NOT NULL,
    [Role] int NOT NULL,
    [Status] nvarchar(max) NULL DEFAULT N'1',
    [LastLoginAt] datetime NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Accounts__349DA5A63D3BB39A] PRIMARY KEY ([AccountId])
);
GO


CREATE TABLE [MembershipPlans] (
    [MembershipPlanId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [DurationDays] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NULL,
    [CreatedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_MembershipPlans] PRIMARY KEY ([MembershipPlanId])
);
GO


CREATE TABLE [Products] (
    [ProductId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(10,2) NOT NULL,
    [StockQuantity] int NULL DEFAULT 0,
    [Category] int NULL,
    [Brand] varchar(100) NULL,
    [Weight] decimal(5,2) NULL,
    [ThumbnailUrl] varchar(255) NULL,
    [IsActive] bit NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Products__B40CC6CDA3CD9911] PRIMARY KEY ([ProductId])
);
GO


CREATE TABLE [Services] (
    [ServiceId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(10,2) NOT NULL,
    [DurationMinutes] int NULL,
    [Category] int NULL,
    [ThumbnailUrl] varchar(255) NULL,
    [IsActive] bit NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Services__C51BB00ACFC86446] PRIMARY KEY ([ServiceId])
);
GO


CREATE TABLE [RefreshTokens] (
    [RefreshTokenId] bigint NOT NULL IDENTITY,
    [AccountId] bigint NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [ExpiredAt] datetime NOT NULL,
    [IsRevoked] bit NULL DEFAULT CAST(0 AS bit),
    [DeviceInfo] varchar(255) NULL,
    [IpAddress] varchar(100) NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__RefreshT__F5845E39079A7C9E] PRIMARY KEY ([RefreshTokenId]),
    CONSTRAINT [FK_RefreshTokens_Accounts] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([AccountId])
);
GO


CREATE TABLE [Users] (
    [UserId] bigint NOT NULL IDENTITY,
    [AccountId] bigint NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Phone] varchar(20) NULL,
    [Address] nvarchar(255) NULL,
    [AvatarUrl] varchar(255) NULL,
    [DateOfBirth] date NULL,
    [Gender] int NULL,
    [IsProMember] bit NULL DEFAULT CAST(0 AS bit),
    [ProExpiredAt] datetime NULL,
    [EmergencyContact] varchar(20) NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Users__1788CC4C7445433F] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Accounts] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([AccountId])
);
GO


CREATE TABLE [Pets] (
    [PetId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Species] varchar(50) NULL,
    [Breed] nvarchar(100) NULL,
    [Gender] nvarchar(max) NULL,
    [BirthDate] date NULL,
    [Color] nvarchar(50) NULL,
    [CurrentWeight] decimal(5,2) NULL,
    [PreviousWeight] decimal(5,2) NULL,
    [WeightUpdatedAt] datetime NULL,
    [HealthStatus] nvarchar(255) NULL,
    [AllergyInfo] nvarchar(255) NULL,
    [VaccinationInfo] nvarchar(255) NULL,
    [MedicalHistory] nvarchar(max) NULL,
    [AvatarUrl] varchar(255) NULL,
    [IsNeutered] bit NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Pets__48E53862EA894294] PRIMARY KEY ([PetId]),
    CONSTRAINT [FK_Pets_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Bookings] (
    [BookingId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [PetId] bigint NOT NULL,
    [BookingCode] varchar(20) NULL,
    [BookingDate] date NOT NULL,
    [StartTime] datetime NOT NULL,
    [EndTime] datetime NULL,
    [Status] nvarchar(max) NULL DEFAULT N'1',
    [Note] nvarchar(max) NULL,
    [TotalPrice] decimal(10,2) NULL DEFAULT 0.0,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Bookings__73951AED16224A83] PRIMARY KEY ([BookingId]),
    CONSTRAINT [FK_Bookings_Pets] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]),
    CONSTRAINT [FK_Bookings_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Conversations] (
    [ConversationId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [DoctorId] bigint NOT NULL,
    [PetId] bigint NOT NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [StartedAt] datetime NOT NULL DEFAULT ((getdate())),
    [EndedAt] datetime NULL,
    CONSTRAINT [PK__Conversa__C050D877B046C5C3] PRIMARY KEY ([ConversationId]),
    CONSTRAINT [FK_Conversations_Customers] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_Conversations_Doctors] FOREIGN KEY ([DoctorId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_Conversations_Pets] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId])
);
GO


CREATE TABLE [PetWeightHistories] (
    [WeightHistoryId] bigint NOT NULL IDENTITY,
    [PetId] bigint NOT NULL,
    [Weight] decimal(5,2) NOT NULL,
    [RecordedAt] datetime NULL DEFAULT ((getdate())),
    [Note] nvarchar(255) NULL,
    CONSTRAINT [PK__PetWeigh__D97AE35DA20C4F7A] PRIMARY KEY ([WeightHistoryId]),
    CONSTRAINT [FK_WeightHistory_Pets] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId])
);
GO


CREATE TABLE [BookingDetails] (
    [BookingDetailId] bigint NOT NULL IDENTITY,
    [BookingId] bigint NOT NULL,
    [ServiceId] bigint NOT NULL,
    [Quantity] int NULL DEFAULT 1,
    [UnitPrice] decimal(10,2) NOT NULL,
    [SubTotal] decimal(10,2) NOT NULL,
    [Note] nvarchar(255) NULL,
    CONSTRAINT [PK__BookingD__8136D45A044AB6D9] PRIMARY KEY ([BookingDetailId]),
    CONSTRAINT [FK_BookingDetails_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]),
    CONSTRAINT [FK_BookingDetails_Services] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId])
);
GO


CREATE TABLE [Feedbacks] (
    [FeedbackId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [BookingId] bigint NULL,
    [ProductId] bigint NULL,
    [Rating] int NULL,
    [Comment] nvarchar(max) NULL,
    [Reply] nvarchar(max) NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Feedback__6A4BEDD62ACAB83E] PRIMARY KEY ([FeedbackId]),
    CONSTRAINT [FK_Feedbacks_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]),
    CONSTRAINT [FK_Feedbacks_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_Feedbacks_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Orders] (
    [OrderId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [BookingId] bigint NULL,
    [OrderCode] varchar(20) NULL,
    [TotalAmount] decimal(10,2) NOT NULL,
    [DiscountAmount] decimal(10,2) NULL DEFAULT 0.0,
    [FinalAmount] decimal(10,2) NOT NULL,
    [OrderType] int NULL,
    [Status] int NULL,
    [PaymentStatus] int NULL,
    [ShippingAddress] nvarchar(255) NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    [UpdatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Orders__C3905BCFFCA790FB] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_Orders_Bookings] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]),
    CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [LogAI] (
    [LogAIId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [PetId] bigint NULL,
    [ConversationId] bigint NULL,
    [Prompt] nvarchar(max) NULL,
    [Response] nvarchar(max) NULL,
    [ModelName] varchar(100) NULL,
    [TokensUsed] int NULL,
    [ResponseTimeMs] int NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__LogAI__7BEA36FC7EB0CF87] PRIMARY KEY ([LogAIId]),
    CONSTRAINT [FK_LogAI_Conversations] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([ConversationId]),
    CONSTRAINT [FK_LogAI_Pets] FOREIGN KEY ([PetId]) REFERENCES [Pets] ([PetId]),
    CONSTRAINT [FK_LogAI_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO


CREATE TABLE [Messages] (
    [MessageId] bigint NOT NULL IDENTITY,
    [ConversationId] bigint NOT NULL,
    [SenderId] bigint NOT NULL,
    [SenderType] int NULL,
    [MessageType] int NULL,
    [Content] nvarchar(max) NULL,
    [AttachmentUrl] varchar(255) NULL,
    [SentAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Messages__C87C0C9C976FA83F] PRIMARY KEY ([MessageId]),
    CONSTRAINT [FK_Messages_Conversations] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([ConversationId])
);
GO


CREATE TABLE [OrderItems] (
    [OrderItemId] bigint NOT NULL IDENTITY,
    [OrderId] bigint NOT NULL,
    [ProductId] bigint NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(10,2) NOT NULL,
    [SubTotal] decimal(10,2) NOT NULL,
    CONSTRAINT [PK__OrderIte__57ED068162365192] PRIMARY KEY ([OrderItemId]),
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]),
    CONSTRAINT [FK_OrderItems_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
);
GO


CREATE TABLE [Payments] (
    [PaymentId] bigint NOT NULL IDENTITY,
    [OrderId] bigint NOT NULL,
    [PaymentMethod] varchar(30) NULL,
    [TransactionCode] varchar(100) NULL,
    [Amount] decimal(10,2) NOT NULL,
    [PaymentStatus] int NULL,
    [PaidAt] datetime NULL,
    [CreatedAt] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Payments__9B556A3812E3924C] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId])
);
GO


CREATE TABLE [UserMemberships] (
    [UserMembershipId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [MembershipPlanId] bigint NOT NULL,
    [PaymentId] bigint NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [PricePaid] decimal(18,2) NOT NULL,
    [Status] int NULL,
    [CreatedAt] datetime2 NULL,
    CONSTRAINT [PK_UserMemberships] PRIMARY KEY ([UserMembershipId]),
    CONSTRAINT [FK_UserMemberships_MembershipPlans_MembershipPlanId] FOREIGN KEY ([MembershipPlanId]) REFERENCES [MembershipPlans] ([MembershipPlanId]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserMemberships_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([PaymentId]),
    CONSTRAINT [FK_UserMemberships_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);
GO


CREATE UNIQUE INDEX [UQ__Accounts__536C85E49E13586D] ON [Accounts] ([Username]);
GO


CREATE UNIQUE INDEX [UQ__Accounts__A9D105346E8BF242] ON [Accounts] ([Email]);
GO


CREATE INDEX [IX_BookingDetails_BookingId] ON [BookingDetails] ([BookingId]);
GO


CREATE INDEX [IX_BookingDetails_ServiceId] ON [BookingDetails] ([ServiceId]);
GO


CREATE INDEX [IX_Bookings_PetId] ON [Bookings] ([PetId]);
GO


CREATE INDEX [IX_Bookings_UserId] ON [Bookings] ([UserId]);
GO


CREATE UNIQUE INDEX [UQ__Bookings__C6E56BD52D4FED17] ON [Bookings] ([BookingCode]) WHERE [BookingCode] IS NOT NULL;
GO


CREATE INDEX [IX_Conversations_DoctorId] ON [Conversations] ([DoctorId]);
GO


CREATE INDEX [IX_Conversations_PetId] ON [Conversations] ([PetId]);
GO


CREATE INDEX [IX_Conversations_UserId] ON [Conversations] ([UserId]);
GO


CREATE INDEX [IX_Feedbacks_BookingId] ON [Feedbacks] ([BookingId]);
GO


CREATE INDEX [IX_Feedbacks_ProductId] ON [Feedbacks] ([ProductId]);
GO


CREATE INDEX [IX_Feedbacks_UserId] ON [Feedbacks] ([UserId]);
GO


CREATE INDEX [IX_LogAI_ConversationId] ON [LogAI] ([ConversationId]);
GO


CREATE INDEX [IX_LogAI_PetId] ON [LogAI] ([PetId]);
GO


CREATE INDEX [IX_LogAI_UserId] ON [LogAI] ([UserId]);
GO


CREATE INDEX [IX_Messages_ConversationId] ON [Messages] ([ConversationId]);
GO


CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO


CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
GO


CREATE INDEX [IX_Orders_BookingId] ON [Orders] ([BookingId]);
GO


CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
GO


CREATE UNIQUE INDEX [UQ__Orders__999B52293D39C3A4] ON [Orders] ([OrderCode]) WHERE [OrderCode] IS NOT NULL;
GO


CREATE UNIQUE INDEX [UQ__Payments__C3905BCE50D03B90] ON [Payments] ([OrderId]);
GO


CREATE INDEX [IX_Pets_UserId] ON [Pets] ([UserId]);
GO


CREATE INDEX [IX_PetWeightHistories_PetId] ON [PetWeightHistories] ([PetId]);
GO


CREATE INDEX [IX_RefreshTokens_AccountId] ON [RefreshTokens] ([AccountId]);
GO


CREATE INDEX [IX_UserMemberships_MembershipPlanId] ON [UserMemberships] ([MembershipPlanId]);
GO


CREATE INDEX [IX_UserMemberships_PaymentId] ON [UserMemberships] ([PaymentId]);
GO


CREATE INDEX [IX_UserMemberships_UserId] ON [UserMemberships] ([UserId]);
GO


CREATE UNIQUE INDEX [UQ__Users__349DA5A70CB62B8B] ON [Users] ([AccountId]);
GO


