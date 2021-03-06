USE [TerminalPays]
GO
/****** Object:  Table [dbo].[Pays]    Script Date: 07.09.2017 0:18:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pays](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateCreate] [datetime2](7) NOT NULL,
	[DateConfirm] [datetime2](7) NULL CONSTRAINT [DF_Pays_DateConfirm]  DEFAULT (NULL),
	[uniqueNumber] [nvarchar](max) NOT NULL,
	[PaymentId] [int] NOT NULL,
	[Sum] [money] NOT NULL,
	[Comission] [money] NOT NULL,
	[PaymentNumber] [nvarchar](max) NULL CONSTRAINT [DF_Pays_PaymentNumber]  DEFAULT (NULL),
	[AdvertiserId] [int] NOT NULL,
	[StatusId] [int] NOT NULL,
 CONSTRAINT [PK_Pays] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StatusPay]    Script Date: 07.09.2017 0:18:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StatusPay](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_PayStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[Pays] ON 

INSERT [dbo].[Pays] ([Id], [DateCreate], [DateConfirm], [uniqueNumber], [PaymentId], [Sum], [Comission], [PaymentNumber], [AdvertiserId], [StatusId]) VALUES (1, CAST(N'2017-08-26 08:00:00.0010000' AS DateTime2), NULL, N'490409', 1111, 100.0000, 0.0000, NULL, 490409, 1)
SET IDENTITY_INSERT [dbo].[Pays] OFF
INSERT [dbo].[StatusPay] ([Id], [Name], [Description]) VALUES (1, N'waiting', N'Создан неподтвержденный платеж, ожидает оплаты')
INSERT [dbo].[StatusPay] ([Id], [Name], [Description]) VALUES (2, N'confirmed', N'Платеж подтвержден')
ALTER TABLE [dbo].[Pays]  WITH CHECK ADD  CONSTRAINT [FK_Pays_Advertiser] FOREIGN KEY([AdvertiserId])
REFERENCES [dbo].[Advertiser] ([Id])
GO
ALTER TABLE [dbo].[Pays] CHECK CONSTRAINT [FK_Pays_Advertiser]
GO
ALTER TABLE [dbo].[Pays]  WITH CHECK ADD  CONSTRAINT [FK_Pays_Status] FOREIGN KEY([StatusId])
REFERENCES [dbo].[StatusPay] ([Id])
GO
ALTER TABLE [dbo].[Pays] CHECK CONSTRAINT [FK_Pays_Status]
GO
