using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.Swagger;
using VinhKhanhNarration.Api.Utils;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVinhKhanhSwagger();

// CORS phải đặt TRƯỚC builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Common services
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<SessionGenerator>();
builder.Services.AddSingleton<GeoDistanceCalculator>();

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
builder.Services.AddScoped<QRCodeDAO>();
builder.Services.AddScoped<GuestSessionDAO>();
builder.Services.AddScoped<GuestPoiStateDAO>();
builder.Services.AddScoped<GeofenceEventDAO>();
builder.Services.AddScoped<ListeningHistoryDAO>();
builder.Services.AddScoped<FeedbackDAO>();

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
builder.Services.AddScoped<QRCodeBUS>();
builder.Services.AddScoped<GuestSessionBUS>();
builder.Services.AddScoped<GuestPoiStateBUS>();
builder.Services.AddScoped<GeofenceBUS>();
builder.Services.AddScoped<ListeningHistoryBUS>();
builder.Services.AddScoped<FeedbackBUS>();

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

// CORS phải đặt trước Authorization và MapControllers
app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }