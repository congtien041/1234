using GameAPI.Data;
using GameAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    // 1. TÍNH NĂNG ĐĂNG KÝ
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRequest req) {
        if (await _db.Players.AnyAsync(x => x.Username == req.Username))
            return BadRequest("Tên tài khoản đã tồn tại!");

        var newPlayer = new Player {
            Username = req.Username,
            Password = req.Password // (Demo: Lưu thẳng pass)
        };
        
        _db.Players.Add(newPlayer);
        
        // Tặng sẵn nhân vật mặc định khi tạo acc
        _db.PlayerCharacters.Add(new PlayerCharacter { 
            PlayerId = newPlayer.Id, CharacterId = "Char_Default" 
        });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Đăng ký thành công!" });
    }

    // 2. TÍNH NĂNG ĐĂNG NHẬP
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest req) {
        var p = await _db.Players.FirstOrDefaultAsync(x => x.Username == req.Username && x.Password == req.Password);
        
        if (p == null) return Unauthorized("Sai tài khoản hoặc mật khẩu!");
        if (p.IsBanned) return Forbid("Tài khoản đã bị khóa!");

        // Trả về toàn bộ thông tin để Unity hiển thị lên Sảnh Chờ
        return Ok(new { 
            p.Id, p.Username, p.Role, 
            p.Level, p.Exp, p.Gold, p.Diamonds, p.EquippedCharacter 
        });
    }
}