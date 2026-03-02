using GameAPI.Data;
using Microsoft.EntityFrameworkCore;
// THÊM DÒNG NÀY ĐỂ FIX LỖI CS1061
using Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);

// Kết nối PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// --- KÍCH HOẠT DỊCH VỤ SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ----------------------------------------------------

var app = builder.Build();

// --- HIỂN THỊ GIAO DIỆN WEB SWAGGER ---
// Lưu ý: Bỏ check IsDevelopment nếu bạn muốn chạy ở bất cứ đâu để test cho nhanh
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();