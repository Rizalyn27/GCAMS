using GCAMS.Models.Notifs;

namespace GCAMS.ViewModels
{
    public class NotificationsIndexViewModel
    {
        public List<Notifs> Items { get; set; } = new();

        /// <summary>"all", "unread" or "announcement" — drives the tab strip.</summary>
        public string Filter { get; set; } = "all";

        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int AnnouncementCount { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
