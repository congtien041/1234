using GameAPI.Data;
using Microsoft.EntityFrameworkCore;
// THÊM DÒNG NÀY ĐỂ FIX LỖI CS1061
using Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);

// Kết nối PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Publish API lên https://admin.monsterasp.net/
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              // Cho phép mọi nguồn (Unity, Web, Mobile) kết nối tới
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

// --- KÍCH HOẠT DỊCH VỤ SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ----------------------------------------------------

var app = builder.Build();

// --- HIỂN THỊ GIAO DIỆN WEB SWAGGER ---
// Lưu ý: Bỏ check IsDevelopment nếu bạn muốn chạy ở bất cứ đâu để test cho nhanh
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// Xóa hoặc comment dòng if (app.Environment.IsDevelopment())
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Game API V1");
    c.RoutePrefix = string.Empty; // Dòng này giúp bạn vào thẳng Swagger khi gõ gameserver.runasp.net
});

app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();