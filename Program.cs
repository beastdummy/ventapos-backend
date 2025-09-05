
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ventapos.Api.Auth;
using Ventapos.Api.Data;
using Ventapos.Api.Features;
using Microsoft.OpenApi.Models;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

// Config
var connStr = builder.Configuration.GetConnectionString("db") 
              ?? "Server=localhost;Port=3306;Database=ventapos;User Id=root;Password=secret;Allow User Variables=True;TreatTinyAsBoolean=false;";

// Services
builder.Services.AddSingleton(new Db(connStr));
builder.Services.AddSingleton<JwtService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ventapos.Api", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Introduce **solo** el token JWT (sin 'Bearer ').",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    // Requisito global: todas las operaciones aceptan este esquema
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

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

app.MapGet("/health/db", async (Db db) =>
{
    try
    {
        using var conn = await db.OpenAsync();
        var version = await conn.ExecuteScalarAsync<string>("SELECT VERSION()");
        return Results.Ok(new { ok = true, version });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithTags("Health")
.AllowAnonymous();


app.Run();
