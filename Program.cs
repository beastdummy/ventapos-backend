
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ventapos.Api.Auth;
using Ventapos.Api.Data;
using Ventapos.Api.Features;


var builder = WebApplication.CreateBuilder(args);

// Config
var connStr = builder.Configuration.GetConnectionString("db") 
              ?? "Server=localhost;Port=3306;Database=ventapos;User Id=root;Password=secret;Allow User Variables=True;TreatTinyAsBoolean=false;";

// Services
builder.Services.AddSingleton(new Db(connStr));
builder.Services.AddSingleton<JwtService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (ajusta orígenes según frontend)
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Auth (JWT)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev_secret_change_me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ventapos";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ventapos-app";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuth();
app.MapCatalog();
app.MapBarcodes();
app.MapCashbox();
app.MapPeople();
app.MapMesas();
app.MapPurchases();
app.MapSales();
app.MapAudit();
app.MapBranches();
app.MapWarehouses();
app.MapInventory();
app.MapMovementTypes();
app.MapComprobantes();
app.MapOutbox();
app.MapPricing();
app.MapUsersAdmin();

app.Run();
