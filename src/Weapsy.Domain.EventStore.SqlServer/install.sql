USE [weapsy.dev]
GO
/****** Object:  Table [dbo].[DomainAggregate]    Script Date: 07/06/2016 19:33:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DomainAggregate](
	[Id] [uniqueidentifier] NOT NULL,
	[Type] [varchar](1000) NOT NULL,
 CONSTRAINT [PK_Aggregates] PRIMARY KEY NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DomainEvent]    Script Date: 07/06/2016 19:33:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DomainEvent](
	[AggregateId] [uniqueidentifier] NOT NULL,
	[SequenceNumber] [int] NOT NULL,
	[Type] [varchar](1000) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[UserId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY NONCLUSTERED 
(
	[AggregateId] ASC,
	[SequenceNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
