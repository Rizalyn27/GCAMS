using GCAMS.Data;
using GCAMS.Models.Notifs;
using GCAMS.Services;
using GCAMS.ViewModels;
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
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Today's items, plus anything still unread from earlier so nothing
            // important silently disappears before the person has seen it.
            var notifications = await _context.Notifs
                .Where(n => n.RecipientUsername == username
                         && ((n.CreatedAt >= today && n.CreatedAt < tomorrow) || !n.IsRead))
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
                    n.RelatedEntityId,
                    Type = n.Type.ToString(),
                    IsToday = n.CreatedAt >= today && n.CreatedAt < tomorrow
                })
                .ToListAsync();

            var unreadCount = await _context.Notifs
                .CountAsync(n => n.RecipientUsername == username && !n.IsRead
                              && n.CreatedAt >= today && n.CreatedAt < tomorrow);
            return Json(new { unreadCount, notifications });
        }

       

        // POST: /Notifications/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id, string? returnUrl = null)
        {
            var username = User.Identity?.Name;
            var notif = await _context.Notifs
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.RecipientUsername == username);

            if (notif == null) return NotFound();

            notif.IsRead = true;
            await _context.SaveChangesAsync();

            // The bell's fetch() call doesn't pass returnUrl, so this falls back to Ok() for it;
            // the dashboard's plain <form> POST does pass it, so it redirects back there instead.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Ok();
        }

        // POST: /Notifications/MarkAllRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead(string? returnUrl = null)
        {
            var username = User.Identity?.Name;
            var unread = await _context.Notifs
                .Where(n => n.RecipientUsername == username && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return Ok();
        }

        // GET: /Notifications
        [Authorize]
        public async Task<IActionResult> Index(string filter = "all", int page = 1)
        {
            var username = User.Identity?.Name;

            // Base query — everything addressed to this user. Counts are taken
            // from here so the tab numbers describe the whole history, not the
            // page currently on screen.
            var all = _context.Notifs
                .Where(n => n.RecipientUsername == username);

            var vm = new NotificationsIndexViewModel
            {
                Filter = filter,
                Page = page < 1 ? 1 : page,

                UnreadCount = await all.CountAsync(n => !n.IsRead),
                AnnouncementCount = await all.CountAsync(n => n.Type == NotificationType.Announcement)
            };

            var filtered = filter switch
            {
                "unread" => all.Where(n => !n.IsRead),
                "announcement" => all.Where(n => n.Type == NotificationType.Announcement),
                _ => all
            };

            vm.TotalCount = await filtered.CountAsync();

            // Page past the end (say, after marking everything read on page 3)
            // and you'd get a blank screen — walk back to the last real page.
            if (vm.Page > vm.TotalPages && vm.TotalPages > 0)
                vm.Page = vm.TotalPages;

            vm.Items = await filtered
                .OrderByDescending(n => n.CreatedAt)
                .Skip((vm.Page - 1) * vm.PageSize)
                .Take(vm.PageSize)
                .ToListAsync();

            return View(vm);
        }

        // POST: /Notifications/MarkOneRead/5
        //
        // The existing MarkRead is called by the bell's JavaScript and returns
        // Ok(). This one is for the full page, where a normal form post needs
        // a redirect back rather than a bare 200.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkOneRead(int id, string filter = "all", int page = 1)
        {
            var username = User.Identity?.Name;

            var notif = await _context.Notifs
                .FirstOrDefaultAsync(n => n.NotificationId == id
                                       && n.RecipientUsername == username);

            if (notif != null && !notif.IsRead)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { filter, page });
        }

    }

}