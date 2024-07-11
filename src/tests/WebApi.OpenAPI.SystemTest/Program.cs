using WebApi.OpenAPI.SystemTest;
using WebApi.OpenAPI.SystemTest.Extensions;
using WebApi.OpenAPI.SystemTest.ExtractTypeResult;
using WebApi.OpenAPI.SystemTest.OperationId;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureControllers();
builder.Services.AddSwagger();

var app = builder.Build();

app.AddEndpointsForOperationId();
app.AddEndpointsForTypedResult();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
