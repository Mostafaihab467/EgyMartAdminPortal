namespace EgyMartAdminPortal.Models.Newsletter
{
    public class NewsletterStats
    {
        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int UnsubscribedCount { get; set; }
        public int TotalBroadcasts { get; set; }
        public int TotalClients { get; set; }
        public int TotalVendors { get; set; }
    }

    public class NewsletterSubscriber
    {
        public long SubscribeID { get; set; }
        public string EMailAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string UserType { get; set; } = "Customer";
        public long? UserId { get; set; }
        public DateTime AddDate { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public int TotalCount { get; set; }
    }

    public class NewsletterBroadcast
    {
        public long NewsletterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = "All";
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
        public string? AttachmentOriginalName { get; set; }
        public long? AttachmentSize { get; set; }
        public int SentCount { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalCount { get; set; }
    }

    public class SendBroadcastModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = "All";
    }

    public class NotificationPayload
    {
        public long NewsletterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = "All";
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
        public string? AttachmentOriginalName { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class SubscribersResponse
    {
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<NewsletterSubscriber> Data { get; set; } = new();
    }

    public class BroadcastsResponse
    {
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<NewsletterBroadcast> Data { get; set; } = new();
    }

    public class StatsResponse
    {
        public bool Success { get; set; }
        public NewsletterStats? Data { get; set; }
    }
}
