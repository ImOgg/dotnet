var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline. 課程說這邊想先移除欸
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// 不是https的話就不需要重定向了
// app.UseHttpsRedirection();

// 認證和授權的部分課程也說先移除
// app.UseAuthorization();

app.MapControllers();

app.Run();
