using GameAPI.Data;
using GameAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MainController : ControllerBase {
    private readonly AppDbContext _db;
    public MainController(AppDbContext db) => _db = db;

    // 1. API ĐĂNG KÝ (Tạo tài khoản mới)
    [HttpGet("register/{username}/{password}")]
    public async Task<IActionResult> Register(string username, string password) {
        bool isExist = await _db.Players.AnyAsync(x => x.Username == username);
        if (isExist) return BadRequest("Tên tài khoản đã có người sử dụng. Vui lòng chọn tên khác!");

        var newPlayer = new Player { Username = username, Password = password };
        _db.Players.Add(newPlayer);
        _db.PlayerCharacters.Add(new PlayerCharacter { PlayerId = newPlayer.Id, CharacterId = "Char_Default" });
        
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đăng ký thành công! Hãy quay lại trang Đăng nhập." });
    }

    // 2. API ĐĂNG NHẬP
    [HttpGet("login/{username}/{password}")]
    public async Task<IActionResult> Login(string username, string password) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        
        if (p == null) return NotFound("Tài khoản không tồn tại! Bạn cần đăng ký trước.");
        if (p.Password != password) return BadRequest("Sai mật khẩu!");
        if (p.IsBanned) return BadRequest("Tài khoản đã bị khóa bởi Admin!");

        return Ok(new { p.Id, p.Username, p.IsBanned, p.Gold, p.Diamonds, p.Level, p.Exp, p.EquippedCharacter });
    }

    // 3. DÀNH CHO WEB ADMIN: Khóa tài khoản
    [HttpPost("admin/ban/{id}")]
    public async Task<IActionResult> BanUser(Guid id) {
        var p = await _db.Players.FindAsync(id);
        if (p != null) { p.IsBanned = true; await _db.SaveChangesAsync(); }
        return Ok("Đã khóa!");
    }

    // 4. DÀNH CHO ADMIN: Bơm Vàng và Kim Cương trực tiếp
    [HttpGet("admin/add-currency")]
    public async Task<IActionResult> AddCurrency(string username, long addGold, long addDiamonds) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        if (p == null) return NotFound("Không tìm thấy người chơi này!");
        
        p.Gold += addGold;
        p.Diamonds += addDiamonds;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Đã bơm thành công cho {username}!", newGold = p.Gold, newDiamonds = p.Diamonds });
    }

    // ==========================================
    // HỆ THỐNG HÒM THƯ (INBOXES) MỚI
    // ==========================================

    // 5. ADMIN GỬI QUÀ (Đã đổi sang nhập Username cho dễ dùng trên Swagger)
    [HttpPost("admin/gift")]
    public async Task<IActionResult> SendGift(string username, string title, string itemId) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        if (p == null) return NotFound("Không tìm thấy người chơi!");

        // ItemId quy ước viết hoa. VD: "GOLD_500", "DIAMOND_10", "CHAR_NINJA"
        _db.Inboxes.Add(new Inbox { 
            PlayerId = p.Id, 
            Title = title, 
            ItemId = itemId.ToUpper(), 
            IsClaimed = false 
        });
        await _db.SaveChangesAsync();
        return Ok($"Đã gửi quà '{title}' (Mã: {itemId}) cho {username}!");
    }

    // 6. UNITY GỌI: Lấy danh sách quà chưa nhận
    [HttpGet("inbox/{username}")]
    public async Task<IActionResult> GetUnclaimedInbox(string username) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == username);
        if (p == null) return NotFound();

        var gifts = await _db.Inboxes
            .Where(x => x.PlayerId == p.Id && !x.IsClaimed)
            .ToListAsync();
            
        return Ok(gifts);
    }

    // 7. UNITY GỌI: Bấm nhận 1 món quà
    [HttpPost("inbox/claim/{inboxId}")]
    public async Task<IActionResult> ClaimGift(int inboxId) {
        var gift = await _db.Inboxes.FindAsync(inboxId);
        if (gift == null || gift.IsClaimed) return BadRequest("Quà không tồn tại hoặc đã nhận rồi!");

        var p = await _db.Players.FindAsync(gift.PlayerId);
        if (p == null) return NotFound("Lỗi dữ liệu người chơi!");

        // Xử lý tặng đồ tự động bằng cách đọc chuỗi ItemId
        if (gift.ItemId.StartsWith("GOLD_")) {
            long amount = long.Parse(gift.ItemId.Split('_')[1]);
            p.Gold += amount;
        } 
        else if (gift.ItemId.StartsWith("DIAMOND_")) {
            long amount = long.Parse(gift.ItemId.Split('_')[1]);
            p.Diamonds += amount;
        }
        else if (gift.ItemId.StartsWith("CHAR_")) {
            _db.PlayerCharacters.Add(new PlayerCharacter { PlayerId = p.Id, CharacterId = gift.ItemId });
        }

        // Chuyển trạng thái hòm thư thành đã nhận
        gift.IsClaimed = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Nhận quà thành công!", newGold = p.Gold, newDiamonds = p.Diamonds });
    }
}