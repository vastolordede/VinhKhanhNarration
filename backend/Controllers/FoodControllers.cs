using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VinhKhanhNarration.Api.BUS;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.Controllers;

[Authorize(Roles = "Admin,ContentManager")]
[Route("api/places")]
public class PlacesController : CrudControllerBase<PlaceDTO>
{
    private readonly PlaceBUS _bus;
    public PlacesController(PlaceBUS bus) : base(bus) => _bus = bus;

    [AllowAnonymous]
    [HttpGet("active")]
    public override IActionResult GetActive() => base.GetActive();

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public override IActionResult GetById(long id)
    {
        if (User.IsInRole("Admin") || User.IsInRole("ContentManager")) return base.GetById(id);
        var item = _bus.GetActive().FirstOrDefault(x => x.PlaceId == id);
        return item == null ? NotFoundMessage() : OkData(item);
    }

    [AllowAnonymous]
    [HttpGet("poi-enabled")] public IActionResult GetPoiEnabled() => OkData(_bus.GetPoiEnabledPlaces());
    [AllowAnonymous]
    [HttpGet("search")] public IActionResult Search([FromQuery] string keyword) => OkData(_bus.SearchByName(keyword));
    [AllowAnonymous]
    [HttpGet("nearby")] public IActionResult Nearby([FromQuery] decimal lat, [FromQuery] decimal lng, [FromQuery] double radius) => OkData(_bus.GetNearbyPlaces(lat, lng, radius));
}

[Authorize(Roles = "Admin,ContentManager")]
[Route("api/dish-categories")]
public class DishCategoriesController : CrudControllerBase<DishCategoryDTO>
{
    public DishCategoriesController(DishCategoryBUS bus) : base(bus) { }

    [AllowAnonymous]
    [HttpGet("active")]
    public override IActionResult GetActive() => base.GetActive();

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public override IActionResult GetById(long id) => base.GetById(id);
}

[Authorize(Roles = "Admin,ContentManager")]
[Route("api/dishes")]
public class DishesController : CrudControllerBase<DishDTO>
{
    private readonly DishBUS _bus;
    public DishesController(DishBUS bus) : base(bus) => _bus = bus;

    [AllowAnonymous]
    [HttpGet("active")]
    public override IActionResult GetActive() => base.GetActive();

    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public override IActionResult GetById(long id)
    {
        if (User.IsInRole("Admin") || User.IsInRole("ContentManager")) return base.GetById(id);
        var item = _bus.GetActive().FirstOrDefault(x => x.DishId == id);
        return item == null ? NotFoundMessage() : OkData(item);
    }

    [AllowAnonymous]
    [HttpGet("signature")] public IActionResult GetSignature() => OkData(_bus.GetSignatureDishes());
    [AllowAnonymous]
    [HttpGet("search")] public IActionResult Search([FromQuery] string keyword) => OkData(_bus.SearchByName(keyword));
    [AllowAnonymous]
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

[Authorize(Roles = "Admin,ContentManager")]
[Route("api/place-dishes")]
public class PlaceDishesController : CrudControllerBase<PlaceDishDTO>
{
    private readonly PlaceDishBUS _bus;
    public PlaceDishesController(PlaceDishBUS bus) : base(bus) => _bus = bus;

    [AllowAnonymous]
    [HttpGet("place/{placeId:long}")] public IActionResult GetByPlace(long placeId) => OkData(_bus.GetMenuByPlaceId(placeId));
    [AllowAnonymous]
    [HttpGet("dish/{dishId:long}")] public IActionResult GetByDish(long dishId) => OkData(_bus.GetPlacesByDishId(dishId));

    [HttpPost("assign")]
    public IActionResult Assign([FromBody] AssignPlaceDishRequestDTO request)
    {
        try { return OkData(_bus.AssignDishToPlace(request.PlaceId, request.DishId, request.Price, request.IsRecommended, request.Note)); }
        catch (Exception ex) { return BadRequestException(ex); }
    }

    [HttpDelete("place/{placeId:long}/dish/{dishId:long}")]
    public IActionResult Remove(long placeId, long dishId) => OkData(_bus.RemoveDishFromPlace(placeId, dishId));
}
