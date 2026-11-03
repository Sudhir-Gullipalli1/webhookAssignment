USE [Assignment];
GO

CREATE TABLE [dbo].[webhook_events] (
    [EventId]       nvarchar(255) NOT NULL,
    [Type]          nvarchar(255) NOT NULL,
    [Livemode]      bit           NOT NULL,
    [CreatedAtUtc]  datetime2     NULL,
    [ReceivedAtUtc] datetime2     NOT NULL,
    [Payload]       nvarchar(max) NOT NULL,
    CONSTRAINT [PK_webhook_events] PRIMARY KEY ([EventId])
);
GO

CREATE INDEX [IX_webhook_events_Type] ON [dbo].[webhook_events] ([Type]);
GO
