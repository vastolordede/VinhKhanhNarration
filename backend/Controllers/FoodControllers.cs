using Microsoft.AspNetCore.Mvc;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Route("api/places")]
public class PlacesController : CrudControllerBase<PlaceDTO>
{
    private readonly PlaceBUS _bus;
    public PlacesController(PlaceBUS bus) : base(bus) => _bus = bus;

    [HttpGet("poi-enabled")] public IActionResult GetPoiEnabled() => OkData(_bus.GetPoiEnabledPlaces());
    [HttpGet("search")] public IActionResult Search([FromQuery] string keyword) => OkData(_bus.SearchByName(keyword));
    [HttpGet("nearby")] public IActionResult Nearby([FromQuery] decimal lat, [FromQuery] decimal lng, [FromQuery] double radius) => OkData(_bus.GetNearbyPlaces(lat, lng, radius));
}

[Route("api/dish-categories")]
public class DishCategoriesController : CrudControllerBase<DishCategoryDTO>
{
    public DishCategoriesController(DishCategoryBUS bus) : base(bus) { }
}

[Route("api/dishes")]
public class DishesController : CrudControllerBase<DishDTO>
{
    private readonly DishBUS _bus;
    public DishesController(DishBUS bus) : base(bus) => _bus = bus;

    [HttpGet("signature")] public IActionResult GetSignature() => OkData(_bus.GetSignatureDishes());
    [HttpGet("search")] public IActionResult Search([FromQuery] string keyword) => OkData(_bus.SearchByName(keyword));
    [HttpGet("category/{categoryId:long}")] public IActionResult GetByCategory(long categoryId) => OkData(_bus.GetByCategoryId(categoryId));
}

public class AssignPlaceDishRequestDTO
{
    public long PlaceId { get; set; }
    public long DishId { get; set; }
    public decimal? Price { get; set; }
    public bool IsRecommended { get; set; }
    public string? Note { get; set; }
}

[Route("api/place-dishes")]
public class PlaceDishesController : CrudControllerBase<PlaceDishDTO>
{
    private readonly PlaceDishBUS _bus;
    public PlaceDishesController(PlaceDishBUS bus) : base(bus) => _bus = bus;

    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetMenuByPlaceId(placeId));
    [HttpGet("dish/{dishId:long}")] public IActionResult GetByDish(long dishId) => OkData(_bus.GetPlacesByDishId(dishId));

    [HttpPost("assign")]
    public IActionResult Assign([FromBody] AssignPlaceDishRequestDTO request)
    {
        try { return OkData(_bus.AssignDishToPlace(request.PlaceId, request.DishId, request.Price, request.IsRecommended, request.Note)); }
        catch (Exception ex) { return BadRequestMessage(ex.Message); }
    }

    [HttpDelete("place/{placeId:long}/dish/{dishId:long}")]
    public IActionResult Remove(long placeId, long dishId) => OkData(_bus.RemoveDishFromPlace(placeId, dishId));
}
