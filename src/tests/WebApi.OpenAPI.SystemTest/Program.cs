using WebApi.OpenAPI.SystemTest;
using WebApi.OpenAPI.SystemTest.Properties;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwagger();

var app = builder.Build();

app.AddEndpointsForOperationId();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
