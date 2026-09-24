using System.Net;
using System.Text;
using DHAFacilitationAPIs.Application.Common.Interfaces;
using DHAFacilitationAPIs.Application.ViewModels;
using DHAFacilitationAPIs.Infrastructure.Data;
using DHAFacilitationAPIs.Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using MobileAPI;
using MobileAPI.Infrastructure;

var options = new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot" //ensure this matches your actual web root folder
};

var builder = WebApplication.CreateBuilder(options);


// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMobileAPIServices(builder.Configuration);

builder.Services.AddSingleton<DapperConnectionFactory>();
builder.Services.AddHttpClient<ISmsService, SmsService>();
builder.Services.AddHttpClient<ISmartPayService, SmartPayService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SetPasswordPolicy", policy =>
    {
        policy.RequireClaim("purpose", "set_password");
    });
});





builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            context.Response.AddApplicationError(error.Error.Message);
            await context.Response.WriteAsync(error.Error.Message);
        }
    });
});
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers().RequireAuthorization();
app.UseMiddleware<CustomExceptionMiddleware>();
app.Run();
