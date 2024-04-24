using WebApi.OpenAPI.SystemTest;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwagger();

var app = builder.Build();

// Test with minimal API
app.MapGet("/", () => "Hello World!");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();