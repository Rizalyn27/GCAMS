using GCAMS.Data;
using GCAMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;

        public NotificationsController(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: /Notifications/GetUnread
        [HttpGet]
        public async Task<IActionResult> GetUnread()
        {
            await _notificationService.GenerateDueNotificationsAsync();

            var username = User.Identity?.Name;

            var notifications = await _context.Notifs
                .Where(n => n.RecipientUsername == username)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt,
                    n.RelatedEntityType,
                    n.RelatedEntityId
                })
                .ToListAsync();

            var unreadCount = await _context.Notifs
                .CountAsync(n => n.RecipientUsername == username && !n.IsRead);

            return Json(new { unreadCount, notifications });
        }

        // POST: /Notifications/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var username = User.Identity?.Name;
            var notif = await _context.Notifs
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.RecipientUsername == username);

            if (notif == null) return NotFound();

            notif.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: /Notifications/MarkAllRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var username = User.Identity?.Name;
            var unread = await _context.Notifs
                .Where(n => n.RecipientUsername == username && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}