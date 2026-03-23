using Microsoft.EntityFrameworkCore;
using TicTacToe.Data;
using TicTacToe.Hubs;
using TicTacToe.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<GameSessionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://tictac-arena-client.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseCors("frontend");
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.Migrate();


app.Run();
