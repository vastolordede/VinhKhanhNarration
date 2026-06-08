using VinhKhanhNarration.Api.BUS.Interfaces;
using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;
using VinhKhanhNarration.Api.DTO.Common;

namespace VinhKhanhNarration.Api.BUS;

public class PlaceBUS : ICrudBUS<PlaceDTO, long>
{
   private readonly PlaceDAO _dao;
private readonly GeocodingBUS _geocodingBUS;

public PlaceBUS(PlaceDAO dao, GeocodingBUS geocodingBUS)
{
    _dao = dao;
    _geocodingBUS = geocodingBUS;
}

public long Create(PlaceDTO dto)
{
    TryFillCoordinatesFromAddress(dto);
    ValidatePlace(dto);
    return _dao.Insert(dto);
}

public bool Update(PlaceDTO dto)
{
    TryFillCoordinatesFromAddress(dto);
    ValidatePlace(dto);
    return _dao.Update(dto);
}

private void TryFillCoordinatesFromAddress(PlaceDTO dto)
{
    var missingCoordinates =
        dto.Latitude == null ||
        dto.Longitude == null ||
        dto.Latitude == 0 ||
        dto.Longitude == 0;

    if (!missingCoordinates || string.IsNullOrWhiteSpace(dto.Address))
    {
        return;
    }

    var geo = _geocodingBUS
        .ResolveAddressAsync(dto.Address)
        .GetAwaiter()
        .GetResult();

    if (geo == null)
    {
        return;
    }

    dto.Latitude = geo.Latitude;
    dto.Longitude = geo.Longitude;
}
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public PlaceDTO? GetById(long id) => _dao.GetById(id);
    public List<PlaceDTO> GetAll() => _dao.GetAll();
    public List<PlaceDTO> GetActive() => _dao.GetActive();
    public List<PlaceDTO> SearchByName(string keyword) => _dao.SearchByName(keyword);
    public List<PlaceDTO> GetPoiEnabledPlaces() => _dao.GetPoiEnabledPlaces();
    public List<PlaceDTO> GetNearbyPlaces(decimal latitude, decimal longitude, double radiusMeters) => _dao.GetNearbyPlaces(latitude, longitude, radiusMeters);

    private static void ValidatePlace(PlaceDTO dto)
{
    var errors = new Dictionary<string, string>();

    if (string.IsNullOrWhiteSpace(dto.PlaceName))
        errors["placeName"] = "Place Name is required.";

    if (dto.PlaceTypeId <= 0)
        errors["placeTypeId"] = "Place Type is required.";

    if (dto.TriggerModeId <= 0)
        errors["triggerModeId"] = "Trigger Mode is required.";

    if ((dto.IsPoi || dto.IsGeofenceEnabled) && (dto.Latitude == null || dto.Longitude == null))
    {
        errors["latitude"] = "Latitude is required for POI/geofence places.";
        errors["longitude"] = "Longitude is required for POI/geofence places.";
    }

    if (dto.TriggerRadiusMeters <= 0)
        errors["triggerRadiusMeters"] = "Radius Meters must be greater than 0.";

    if (dto.Priority < 0)
        errors["priority"] = "Priority must be greater than or equal to 0.";

    if (dto.DebounceSeconds < 0)
        errors["debounceSeconds"] = "Debounce Seconds must be greater than or equal to 0.";

    if (dto.CooldownSeconds < 0)
        errors["cooldownSeconds"] = "Cooldown Seconds must be greater than or equal to 0.";

    if (errors.Count > 0)
        throw new ApiValidationException(errors);
}

    private static void ValidateGeofenceConfig(PlaceDTO dto)
    {
        if ((dto.IsPoi || dto.IsGeofenceEnabled) && (dto.Latitude == null || dto.Longitude == null))
            throw new ArgumentException("Latitude and Longitude are required for POI/geofence places.");
        if (dto.TriggerRadiusMeters <= 0) throw new ArgumentException("TriggerRadiusMeters must be greater than 0.");
        if (dto.Priority < 0) throw new ArgumentException("Priority must be greater than or equal to 0.");
        if (dto.DebounceSeconds < 0) throw new ArgumentException("DebounceSeconds must be greater than or equal to 0.");
        if (dto.CooldownSeconds < 0) throw new ArgumentException("CooldownSeconds must be greater than or equal to 0.");
    }
}

public class DishCategoryBUS : ICrudBUS<DishCategoryDTO, long>
{
    private readonly DishCategoryDAO _dao;
    public DishCategoryBUS(DishCategoryDAO dao) => _dao = dao;
    public long Create(DishCategoryDTO dto) { ValidateCategory(dto); if (_dao.IsCategoryNameExists(dto.CategoryName)) throw new InvalidOperationException("Category name already exists."); return _dao.Insert(dto); }
    public bool Update(DishCategoryDTO dto) { ValidateCategory(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public DishCategoryDTO? GetById(long id) => _dao.GetById(id);
    public List<DishCategoryDTO> GetAll() => _dao.GetAll();
    public List<DishCategoryDTO> GetActive() => _dao.GetActive();
private static void ValidateCategory(DishCategoryDTO dto)
{
    var errors = new Dictionary<string, string>();

    if (string.IsNullOrWhiteSpace(dto.CategoryName))
        errors["categoryName"] = "Category Name is required.";

    if (errors.Count > 0)
        throw new ApiValidationException(errors);
}}

public class DishBUS : ICrudBUS<DishDTO, long>
{
    private readonly DishDAO _dao;
    public DishBUS(DishDAO dao) => _dao = dao;
    public long Create(DishDTO dto) { ValidateDish(dto); return _dao.Insert(dto); }
    public bool Update(DishDTO dto) { ValidateDish(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public DishDTO? GetById(long id) => _dao.GetById(id);
    public List<DishDTO> GetAll() => _dao.GetAll();
    public List<DishDTO> GetActive() => _dao.GetActive();
    public List<DishDTO> GetByCategoryId(long categoryId) => _dao.GetByCategoryId(categoryId);
    public List<DishDTO> SearchByName(string keyword) => _dao.SearchByName(keyword);
    public List<DishDTO> GetSignatureDishes() => _dao.GetSignatureDishes();
private static void ValidateDish(DishDTO dto)
{
    var errors = new Dictionary<string, string>();

    if (string.IsNullOrWhiteSpace(dto.DishName))
        errors["dishName"] = "Dish Name is required.";

    if (dto.CategoryId <= 0)
        errors["categoryId"] = "Category is required.";

    if (dto.AveragePrice.HasValue && dto.AveragePrice.Value < 0)
        errors["averagePrice"] = "Average Price must be greater than or equal to 0.";

    if (errors.Count > 0)
        throw new ApiValidationException(errors);
}}

public class PlaceDishBUS : ICrudBUS<PlaceDishDTO, long>
{
    private readonly PlaceDishDAO _dao;
    public PlaceDishBUS(PlaceDishDAO dao) => _dao = dao;
    public long Create(PlaceDishDTO dto) { ValidatePlaceDish(dto); if (_dao.IsPlaceDishExists(dto.PlaceId, dto.DishId)) throw new InvalidOperationException("Dish already assigned to this place."); return _dao.Insert(dto); }
    public bool Update(PlaceDishDTO dto) { ValidatePlaceDish(dto); return _dao.Update(dto); }
    public bool Deactivate(long id) => _dao.SoftDelete(id);
    public bool Restore(long id) => _dao.Restore(id);
    public PlaceDishDTO? GetById(long id) => _dao.GetById(id);
    public List<PlaceDishDTO> GetAll() => _dao.GetAll();
    public List<PlaceDishDTO> GetActive() => _dao.GetActive();
    public List<PlaceDishDTO> GetMenuByPlaceId(long placeId) => _dao.GetByPlaceId(placeId);
    public List<PlaceDishDTO> GetPlacesByDishId(long dishId) => _dao.GetByDishId(dishId);
    public bool AssignDishToPlace(long placeId, long dishId, decimal? price, bool isRecommended, string? note) => Create(new PlaceDishDTO { PlaceId = placeId, DishId = dishId, Price = price, IsRecommended = isRecommended, Note = note }) > 0;
    public bool RemoveDishFromPlace(long placeId, long dishId) => _dao.RemoveDishFromPlace(placeId, dishId);
private static void ValidatePlaceDish(PlaceDishDTO dto)
{
    var errors = new Dictionary<string, string>();

    if (dto.PlaceId <= 0)
        errors["placeId"] = "Place is required.";

    if (dto.DishId <= 0)
        errors["dishId"] = "Dish is required.";

    if (dto.Price.HasValue && dto.Price.Value < 0)
        errors["price"] = "Price must be greater than or equal to 0.";

    if (errors.Count > 0)
        throw new ApiValidationException(errors);
}}
