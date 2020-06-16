USE [master]
GO
/****** Object:  Database [Traducir]    Script Date: 16.06.2020 11:27:01 ******/
CREATE DATABASE [Traducir]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Traducir', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\Traducir.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'Traducir_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\Traducir_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Traducir] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Traducir].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Traducir] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Traducir] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Traducir] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Traducir] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Traducir] SET ARITHABORT OFF 
GO
ALTER DATABASE [Traducir] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Traducir] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [Traducir] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Traducir] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Traducir] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Traducir] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Traducir] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Traducir] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Traducir] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Traducir] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Traducir] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Traducir] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Traducir] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Traducir] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Traducir] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Traducir] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Traducir] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Traducir] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Traducir] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Traducir] SET  MULTI_USER 
GO
ALTER DATABASE [Traducir] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Traducir] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Traducir] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Traducir] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
USE [Traducir]
GO
/****** Object:  UserDefinedFunction [dbo].[fnColumnExists]    Script Date: 16.06.2020 11:27:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	-- create fnColumnExists(table, column)
	CREATE FUNCTION [dbo].[fnColumnExists](
		@table_name nvarchar(max),
		@column_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found bit
		SET @found = 0
		IF	EXISTS (
				SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = @table_name AND COLUMN_NAME = @column_name )
		BEGIN
			SET @found = 1
		END


		RETURN @found
	END
GO
/****** Object:  UserDefinedFunction [dbo].[fnConstraintExists]    Script Date: 16.06.2020 11:27:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	--create fnConstraintExists(table, constraint)
	CREATE FUNCTION [dbo].[fnConstraintExists](
		@table_name nvarchar(max),
		@constraint_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found  bit
		SET @found = 0
		IF EXISTS (
			SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE
				TABLE_NAME = @table_name AND
				CONSTRAINT_NAME = @constraint_name)
		BEGIN
			SET @found = 1
		END

		RETURN @found
	END;
GO
/****** Object:  UserDefinedFunction [dbo].[fnIndexExists]    Script Date: 16.06.2020 11:27:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	-- create fnIndexExists(table, index)
	CREATE FUNCTION [dbo].[fnIndexExists](
		@table_name nvarchar(max),
		@index_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found bit
		SET @found = 0
		IF	EXISTS (
				SELECT 1 FROM sys.indexes
				WHERE object_id = OBJECT_ID(@table_name) AND name = @index_name )
		BEGIN
			SET @found = 1
		END


		RETURN @found
	END
GO
/****** Object:  UserDefinedFunction [dbo].[fnTableExists]    Script Date: 16.06.2020 11:27:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	-- create fnTableExists(table)
	-- see: http://stackoverflow.com/questions/167576/sql-server-check-if-table-exists/167680#167680
	CREATE FUNCTION [dbo].[fnTableExists](
		@table_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found bit
		SET @found = 0
		IF EXISTS (
			SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE
				TABLE_TYPE = 'BASE TABLE' AND
				TABLE_NAME = @table_name)
		BEGIN
			SET @found = 1
		END

		RETURN @found
	END
GO
/****** Object:  Table [dbo].[StringHistory]    Script Date: 16.06.2020 11:27:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StringHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StringId] [int] NOT NULL,
	[HistoryTypeId] [tinyint] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_StringHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StringHistoryTypes]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StringHistoryTypes](
	[Id] [tinyint] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](255) NOT NULL,
 CONSTRAINT [PK_StringHistoryTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Strings]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Strings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [varchar](255) NOT NULL,
	[NormalizedKey] [varchar](255) NOT NULL,
	[OriginalString] [nvarchar](max) NOT NULL,
	[Translation] [nvarchar](max) NULL,
	[NeedsPush] [bit] NOT NULL DEFAULT ((0)),
	[Variant] [varchar](255) NULL,
	[CreationDate] [datetime] NOT NULL,
	[DeletionDate] [datetime] NULL,
	[IsUrgent] [bit] NOT NULL DEFAULT ((0)),
	[FamilyKey] [char](32) NULL,
	[IsIgnored] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_Strings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Strings_Key] UNIQUE NONCLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Strings_NormalizedKey] UNIQUE NONCLUSTERED 
(
	[NormalizedKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StringSuggestionHistory]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StringSuggestionHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StringSuggestionId] [int] NOT NULL,
	[HistoryTypeId] [tinyint] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[UserId] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_StringSuggestionHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StringSuggestionHistoryTypes]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StringSuggestionHistoryTypes](
	[Id] [tinyint] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](255) NOT NULL,
 CONSTRAINT [PK_StringSuggestionHistoryTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[StringSuggestions]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StringSuggestions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StringId] [int] NOT NULL,
	[Suggestion] [nvarchar](max) NULL,
	[StateId] [tinyint] NOT NULL,
	[CreatedById] [int] NOT NULL,
	[LastStateUpdatedById] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastStateUpdatedDate] [datetime] NULL,
 CONSTRAINT [PK_StringSuggestions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StringSuggestionStates]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[StringSuggestionStates](
	[Id] [tinyint] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](255) NOT NULL,
 CONSTRAINT [PK_StringSuggestionStates] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserHistory]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[HistoryTypeId] [tinyint] NOT NULL,
	[Comment] [nvarchar](100) NULL,
	[UpdatedById] [int] NULL,
	[CreationDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UserHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UserHistoryTypes]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UserHistoryTypes](
	[Id] [tinyint] NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Description] [varchar](255) NOT NULL,
 CONSTRAINT [PK_UserHistoryTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Users]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] NOT NULL,
	[DisplayName] [nvarchar](150) NOT NULL,
	[IsModerator] [bit] NOT NULL,
	[IsTrusted] [bit] NOT NULL,
	[IsReviewer] [bit] NOT NULL,
	[IsBanned] [bit] NOT NULL,
	[CreationDate] [datetime] NOT NULL,
	[LastSeenDate] [datetime] NULL,
	[NextNotificationUrgentStrings] [datetime] NULL,
	[NextNotificationSuggestionsAwaitingApproval] [datetime] NULL,
	[NextNotificationSuggestionsAwaitingReview] [datetime] NULL,
	[NextNotificationStringsPushedToTransifex] [datetime] NULL,
	[NextNotificationSuggestionsApproved] [datetime] NULL,
	[NextNotificationSuggestionsRejected] [datetime] NULL,
	[NextNotificationSuggestionsReviewed] [datetime] NULL,
	[NextNotificationSuggestionsOverriden] [datetime] NULL,
	[NotificationDetails] [varchar](max) NULL,
	[NotificationsIntervalId] [smallint] NOT NULL DEFAULT ((1440)),
	[NotificationsIntervalValue] [smallint] NOT NULL DEFAULT ((7)),
	[NotificationsIntervalMinutes] [int] NOT NULL DEFAULT ((10080)),
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[VersionInfo]    Script Date: 16.06.2020 11:27:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VersionInfo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Version] [bigint] NOT NULL,
	[AppliedOn] [datetime] NOT NULL,
	[Description] [nvarchar](256) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Strings_FamilyKey]    Script Date: 16.06.2020 11:27:02 ******/
CREATE NONCLUSTERED INDEX [IX_Strings_FamilyKey] ON [dbo].[Strings]
(
	[FamilyKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Strings_NormalizedKey_Filtered]    Script Date: 16.06.2020 11:27:02 ******/
CREATE NONCLUSTERED INDEX [IX_Strings_NormalizedKey_Filtered] ON [dbo].[Strings]
(
	[NormalizedKey] ASC
)
INCLUDE ( 	[DeletionDate]) 
WHERE ([DeletionDate] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[StringHistory]  WITH CHECK ADD  CONSTRAINT [FK_StringHistory_String] FOREIGN KEY([StringId])
REFERENCES [dbo].[Strings] ([Id])
GO
ALTER TABLE [dbo].[StringHistory] CHECK CONSTRAINT [FK_StringHistory_String]
GO
ALTER TABLE [dbo].[StringHistory]  WITH CHECK ADD  CONSTRAINT [FK_StringHistory_StringHistoryTypes] FOREIGN KEY([HistoryTypeId])
REFERENCES [dbo].[StringHistoryTypes] ([Id])
GO
ALTER TABLE [dbo].[StringHistory] CHECK CONSTRAINT [FK_StringHistory_StringHistoryTypes]
GO
ALTER TABLE [dbo].[StringHistory]  WITH CHECK ADD  CONSTRAINT [FK_StringHistory_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[StringHistory] CHECK CONSTRAINT [FK_StringHistory_User]
GO
ALTER TABLE [dbo].[StringSuggestionHistory]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestionHistory_StringSuggestion] FOREIGN KEY([StringSuggestionId])
REFERENCES [dbo].[StringSuggestions] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestionHistory] CHECK CONSTRAINT [FK_StringSuggestionHistory_StringSuggestion]
GO
ALTER TABLE [dbo].[StringSuggestionHistory]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestionHistory_StringSuggestionHistoryTypes] FOREIGN KEY([HistoryTypeId])
REFERENCES [dbo].[StringSuggestionHistoryTypes] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestionHistory] CHECK CONSTRAINT [FK_StringSuggestionHistory_StringSuggestionHistoryTypes]
GO
ALTER TABLE [dbo].[StringSuggestionHistory]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestionHistory_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestionHistory] CHECK CONSTRAINT [FK_StringSuggestionHistory_User]
GO
ALTER TABLE [dbo].[StringSuggestions]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestions_CreatedBy] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestions] CHECK CONSTRAINT [FK_StringSuggestions_CreatedBy]
GO
ALTER TABLE [dbo].[StringSuggestions]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestions_LastStateUpdatedById] FOREIGN KEY([LastStateUpdatedById])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestions] CHECK CONSTRAINT [FK_StringSuggestions_LastStateUpdatedById]
GO
ALTER TABLE [dbo].[StringSuggestions]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestions_String] FOREIGN KEY([StringId])
REFERENCES [dbo].[Strings] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestions] CHECK CONSTRAINT [FK_StringSuggestions_String]
GO
ALTER TABLE [dbo].[StringSuggestions]  WITH CHECK ADD  CONSTRAINT [FK_StringSuggestions_StringSuggestionStates] FOREIGN KEY([StateId])
REFERENCES [dbo].[StringSuggestionStates] ([Id])
GO
ALTER TABLE [dbo].[StringSuggestions] CHECK CONSTRAINT [FK_StringSuggestions_StringSuggestionStates]
GO
ALTER TABLE [dbo].[UserHistory]  WITH CHECK ADD  CONSTRAINT [FK_UserHistory_UpdatedById] FOREIGN KEY([UpdatedById])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserHistory] CHECK CONSTRAINT [FK_UserHistory_UpdatedById]
GO
ALTER TABLE [dbo].[UserHistory]  WITH CHECK ADD  CONSTRAINT [FK_UserHistory_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserHistory] CHECK CONSTRAINT [FK_UserHistory_User]
GO
ALTER TABLE [dbo].[UserHistory]  WITH CHECK ADD  CONSTRAINT [FK_UserHistory_UserHistoryType] FOREIGN KEY([HistoryTypeId])
REFERENCES [dbo].[UserHistoryTypes] ([Id])
GO
ALTER TABLE [dbo].[UserHistory] CHECK CONSTRAINT [FK_UserHistory_UserHistoryType]
GO
USE [master]
GO
ALTER DATABASE [Traducir] SET  READ_WRITE 
GO
