using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.Swagger;
using VinhKhanhNarration.Api.Utils;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.Services;
using VinhKhanhNarration.Api.Services.Interfaces;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVinhKhanhSwagger();

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SecretKey must be configured and at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// CORS phải đặt TRƯỚC builder.Build()
var corsAllowedOriginsRaw =
    Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
    ?? Environment.GetEnvironmentVariable("FRONTEND_URL")
    ?? "http://localhost:5173";

var corsAllowedOrigins = corsAllowedOriginsRaw
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy
            .WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Common services
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<SessionGenerator>();
builder.Services.AddSingleton<GeoDistanceCalculator>();
builder.Services.AddSingleton<JwtTokenGenerator>();

// DAO
builder.Services.AddScoped<AdminUserDAO>();
builder.Services.AddScoped<LanguageDAO>();
builder.Services.AddScoped<PlaceTypeDAO>();
builder.Services.AddScoped<ContentTypeDAO>();
builder.Services.AddScoped<TargetTypeDAO>();
builder.Services.AddScoped<TranslationSourceDAO>();
builder.Services.AddScoped<TriggerModeDAO>();
builder.Services.AddScoped<GeofenceEventTypeDAO>();
builder.Services.AddScoped<GeofenceEventStatusDAO>();
builder.Services.AddScoped<PlaceDAO>();
builder.Services.AddScoped<DishCategoryDAO>();
builder.Services.AddScoped<DishDAO>();
builder.Services.AddScoped<PlaceDishDAO>();
builder.Services.AddScoped<NarrationContentDAO>();
builder.Services.AddScoped<NarrationTranslationDAO>();
builder.Services.AddScoped<AudioFileDAO>();
builder.Services.AddScoped<GuestSessionDAO>();
builder.Services.AddScoped<GuestPoiStateDAO>();
builder.Services.AddScoped<GeofenceEventDAO>();
builder.Services.AddScoped<ListeningHistoryDAO>();
builder.Services.AddScoped<FeedbackDAO>();
builder.Services.AddScoped<AdminRefreshTokenDAO>();
builder.Services.AddScoped<VendorModuleDAO>();

// BUS
builder.Services.AddScoped<AdminUserBUS>();
builder.Services.AddScoped<LanguageBUS>();
builder.Services.AddScoped<PlaceTypeBUS>();
builder.Services.AddScoped<ContentTypeBUS>();
builder.Services.AddScoped<TargetTypeBUS>();
builder.Services.AddScoped<TranslationSourceBUS>();
builder.Services.AddScoped<TriggerModeBUS>();
builder.Services.AddScoped<GeofenceEventTypeBUS>();
builder.Services.AddScoped<GeofenceEventStatusBUS>();
builder.Services.AddScoped<PlaceBUS>();
builder.Services.AddScoped<DishCategoryBUS>();
builder.Services.AddScoped<DishBUS>();
builder.Services.AddScoped<PlaceDishBUS>();
builder.Services.AddScoped<NarrationContentBUS>();
builder.Services.AddScoped<NarrationTranslationBUS>();
builder.Services.AddScoped<AudioFileBUS>();
builder.Services.AddScoped<PublicNarrationBUS>();
builder.Services.AddScoped<GuestSessionBUS>();
builder.Services.AddScoped<GuestPoiStateBUS>();
builder.Services.AddScoped<GeofenceBUS>();
builder.Services.AddScoped<ListeningHistoryBUS>();
builder.Services.AddScoped<FeedbackBUS>();
builder.Services.AddScoped<VendorModuleBUS>();
builder.Services.AddHttpClient<GeocodingBUS>();
builder.Services.AddHttpClient<ITranslationService, AzureTranslatorService>();
builder.Services.AddHttpClient<ITextToSpeechService, AzureSpeechTtsService>();
builder.Services.AddScoped<IAudioStorage, LocalAudioStorage>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vinh Khanh Narration API v1");
        c.RoutePrefix = "swagger";
    });
}

// Serve generated MP3 files before mapping controllers.
app.UseStaticFiles();

// CORS phải đặt trước Authorization và MapControllers
app.UseCors("AppCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }