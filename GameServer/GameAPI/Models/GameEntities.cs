using System.ComponentModel.DataAnnotations;

namespace GameAPI.Models;

public class Player {
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string Password { get; set; } = ""; // Ở dự án thực tế sẽ mã hóa MD5/BCrypt
    public string Role { get; set; } = "User"; 
    public bool IsBanned { get; set; } = false;
    public int Level { get; set; } = 1;
    public long Exp { get; set; } = 0;
    public long Gold { get; set; } = 0;
    public long Diamonds { get; set; } = 0;
    public string EquippedCharacter { get; set; } = "Char_Default";
    public DateTime LastOnline { get; set; } = DateTime.UtcNow;
}

public class PlayerCharacter {
    [Key] public int Id { get; set; }
    public Guid PlayerId { get; set; }
    public string CharacterId { get; set; } = "";
}

// BỔ SUNG LẠI CLASS INBOX CHO HỆ THỐNG TẶNG QUÀ
public class Inbox {
    [Key] public int Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Title { get; set; } = "";
    public string ItemId { get; set; } = ""; // Vd: "VIP_TICKET", "Gold_500"
    public bool IsClaimed { get; set; } = false;
}

public class Friendship {
    [Key] public int Id { get; set; }
    public Guid PlayerId1 { get; set; }
    public Guid PlayerId2 { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

    public class PartyInvite {
    [Key] public int Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string RoomName { get; set; } = "";
    public string GameMode { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Các class phụ trợ để nhận dữ liệu từ Unity
    public class AuthRequest {
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}