using WebApi.OpenAPI.SystemTest;
using WebApi.OpenAPI.SystemTest.ExtractTypeResult;
using WebApi.OpenAPI.SystemTest.OperationId;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwagger();

var app = builder.Build();

app.AddEndpointsForOperationId();
app.AddEndpointsForTypedResult();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

// For integration testing purposes only in order to use WebApplicationFactory<TProgram>
public partial class Program;