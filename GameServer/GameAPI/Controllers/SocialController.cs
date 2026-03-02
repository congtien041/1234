using GameAPI.Data;
using GameAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController : ControllerBase {
    private readonly AppDbContext _db;
    public SocialController(AppDbContext db) => _db = db;

    // ==========================================
    // 1. TÍNH NĂNG LAST ONLINE
    // ==========================================
    [HttpPost("ping-online/{username}")]
    public async Task<IActionResult> PingOnline(string username) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        if (p == null) return NotFound();

        p.LastOnline = DateTime.UtcNow; // Cập nhật giờ hiện tại
        await _db.SaveChangesAsync();
        return Ok("Đã cập nhật trạng thái Online");
    }

    // ==========================================
    // 2. TÍNH NĂNG KẾT BẠN
    // ==========================================
    [HttpPost("add-friend")]
    public async Task<IActionResult> SendFriendRequest(string senderName, string receiverName) {
        var sender = await _db.Players.FirstOrDefaultAsync(x => x.Username == senderName);
        var receiver = await _db.Players.FirstOrDefaultAsync(x => x.Username == receiverName);
        if (sender == null || receiver == null) return NotFound("Không tìm thấy người chơi!");

        // Kiểm tra xem đã gửi lời mời hoặc là bạn bè chưa
        bool exists = await _db.Friendships.AnyAsync(f => 
            (f.PlayerId1 == sender.Id && f.PlayerId2 == receiver.Id) ||
            (f.PlayerId1 == receiver.Id && f.PlayerId2 == sender.Id));
            
        if (exists) return BadRequest("Đã gửi lời mời hoặc đã là bạn bè!");

        _db.Friendships.Add(new Friendship { PlayerId1 = sender.Id, PlayerId2 = receiver.Id });
        await _db.SaveChangesAsync();
        return Ok($"Đã gửi lời mời kết bạn tới {receiverName}");
    }

    [HttpGet("my-friends/{username}")]
    public async Task<IActionResult> GetFriendsList(string username) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        if (p == null) return NotFound();

        // Lấy danh sách bạn bè (Đã Accepted)
        var friendLinks = await _db.Friendships
            .Where(f => (f.PlayerId1 == p.Id || f.PlayerId2 == p.Id) && f.Status == "Accepted")
            .ToListAsync();

        var friendList = new List<object>();
        foreach (var link in friendLinks) {
            var friendId = link.PlayerId1 == p.Id ? link.PlayerId2 : link.PlayerId1;
            var friend = await _db.Players.FindAsync(friendId);
            if (friend != null) {
                // Nếu LastOnline cách đây dưới 2 phút -> Đang Online
                bool isOnline = (DateTime.UtcNow - friend.LastOnline).TotalMinutes < 2;
                friendList.Add(new { 
                    friend.Username, friend.Level, isOnline, friend.LastOnline 
                });
            }
        }
        return Ok(friendList);
    }

    // ==========================================
    // 3. TÍNH NĂNG MỜI VÀO PHÒNG (TEAM 2 / TEAM 4)
    // ==========================================
    [HttpPost("invite-party")]
    public async Task<IActionResult> SendPartyInvite(string senderName, string receiverName, string roomName, string gameMode) {
        var sender = await _db.Players.FirstOrDefaultAsync(x => x.Username == senderName);
        var receiver = await _db.Players.FirstOrDefaultAsync(x => x.Username == receiverName);
        if (sender == null || receiver == null) return NotFound();

        _db.PartyInvites.Add(new PartyInvite {
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            RoomName = roomName,
            GameMode = gameMode
        });
        await _db.SaveChangesAsync();
        return Ok("Đã gửi lời mời vào phòng!");
    }

    // Unity sẽ gọi API này mỗi 3 giây để check xem có ai mời mình không
    [HttpGet("check-invites/{username}")]
    public async Task<IActionResult> CheckInvites(string username) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        if (p == null) return NotFound();

        // Lấy các lời mời chưa đọc và gửi trong vòng 1 phút đổ lại (để tránh lời mời rác)
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
        var invites = await _db.PartyInvites
            .Where(x => x.ReceiverId == p.Id && x.Status == "Pending" && x.CreatedAt >= oneMinuteAgo)
            .Select(x => new {
                x.Id,
                SenderName = _db.Players.FirstOrDefault(u => u.Id == x.SenderId).Username,
                x.RoomName,
                x.GameMode
            })
            .ToListAsync();

        return Ok(invites);
    }
}