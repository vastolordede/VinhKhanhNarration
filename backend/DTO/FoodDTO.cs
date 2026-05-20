using VinhKhanhNarration.Api.DAO.Mapping;

namespace VinhKhanhNarration.Api.DTO;

[DbTable("places")]
public class PlaceDTO
{
    [DbColumn("place_id", IsKey = true, IsIdentity = true)] public long PlaceId { get; set; }
    [DbColumn("place_name")] public string PlaceName { get; set; } = string.Empty;
    [DbColumn("place_type_id")] public long PlaceTypeId { get; set; }
    [DbColumn("address")] public string? Address { get; set; }
    [DbColumn("description")] public string? Description { get; set; }
    [DbColumn("latitude")] public decimal? Latitude { get; set; }
    [DbColumn("longitude")] public decimal? Longitude { get; set; }
    [DbColumn("opening_hours")] public string? OpeningHours { get; set; }
    [DbColumn("image_url")] public string? ImageUrl { get; set; }
    [DbColumn("is_poi")] public bool IsPoi { get; set; } = true;
    [DbColumn("is_geofence_enabled")] public bool IsGeofenceEnabled { get; set; } = true;
    [DbColumn("trigger_radius_meters")] public int TriggerRadiusMeters { get; set; } = 50;
    [DbColumn("priority")] public int Priority { get; set; } = 1;
    [DbColumn("trigger_mode_id")] public long TriggerModeId { get; set; }
    [DbColumn("debounce_seconds")] public int DebounceSeconds { get; set; } = 10;
    [DbColumn("cooldown_seconds")] public int CooldownSeconds { get; set; } = 300;
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

[DbTable("dish_categories")]
public class DishCategoryDTO
{
    [DbColumn("category_id", IsKey = true, IsIdentity = true)] public long CategoryId { get; set; }
    [DbColumn("category_name")] public string CategoryName { get; set; } = string.Empty;
    [DbColumn("description")] public string? Description { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

[DbTable("dishes")]
public class DishDTO
{
    [DbColumn("dish_id", IsKey = true, IsIdentity = true)] public long DishId { get; set; }
    [DbColumn("dish_name")] public string DishName { get; set; } = string.Empty;
    [DbColumn("category_id")] public long CategoryId { get; set; }
    [DbColumn("description")] public string? Description { get; set; }
    [DbColumn("image_url")] public string? ImageUrl { get; set; }
    [DbColumn("average_price")] public decimal? AveragePrice { get; set; }
    [DbColumn("is_signature_dish")] public bool IsSignatureDish { get; set; }
    [DbColumn("is_active")] public bool IsActive { get; set; } = true;
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}

[DbTable("place_dishes")]
public class PlaceDishDTO
{
    [DbColumn("place_dish_id", IsKey = true, IsIdentity = true)] public long PlaceDishId { get; set; }
    [DbColumn("place_id")] public long PlaceId { get; set; }
    [DbColumn("dish_id")] public long DishId { get; set; }
    [DbColumn("price")] public decimal? Price { get; set; }
    [DbColumn("is_recommended")] public bool IsRecommended { get; set; }
    [DbColumn("note")] public string? Note { get; set; }
    [DbColumn("created_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime CreatedAt { get; set; }
    [DbColumn("updated_at", IgnoreOnInsert = true, IgnoreOnUpdate = true)] public DateTime UpdatedAt { get; set; }
}
