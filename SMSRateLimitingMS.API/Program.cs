using SMSRateLimitingMS.Application.Interfaces;
using SMSRateLimitingMS.Application.Settings;
using SMSRateLimitingMS.Application.UseCases.CheckSMSRateLimit;
using SMSRateLimitingMS.Infrastructure.BackgroundServices;
using SMSRateLimitingMS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Configuration
SMSRateLimitSettings appSettings = new();
builder.Configuration.GetSection("SMSRateLimit").Bind(appSettings);

builder.Services.AddSingleton(appSettings);

// Services
builder.Services.AddSingleton<IRateLimitRepository, InMemoryRateLimitRepository>();
builder.Services.AddSingleton<IRateLimitHistoryRepository, InMemoryRateLimitHistoryRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CheckSMSRateLimitCommandHandler).Assembly);
});

// Background Job
builder.Services.AddHostedService<RateLimitCleanupService>();
builder.Services.AddHostedService<HistoryCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x => x
 .SetIsOriginAllowed(origin => true)
 .AllowAnyMethod()
 .AllowAnyHeader()
 .AllowCredentials());

app.UseAuthorization();

app.MapControllers();

app.Run();
