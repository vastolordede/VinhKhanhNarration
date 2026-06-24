using VinhKhanhNarration.Api.DAO;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.BUS;

public class VendorCatalogBUS
{
    private readonly VendorModuleBUS _vendorModule;
    private readonly VendorModuleDAO _vendorDAO;
    private readonly PlaceBUS _placeBUS;
    private readonly PlaceDAO _placeDAO;
    private readonly DishDAO _dishDAO;
    private readonly DishBUS _dishBUS;
    private readonly PlaceDishDAO _placeDishDAO;

    public VendorCatalogBUS(
        VendorModuleBUS vendorModule,
        VendorModuleDAO vendorDAO,
        PlaceBUS placeBUS,
        PlaceDAO placeDAO,
        DishDAO dishDAO,
        DishBUS dishBUS,
        PlaceDishDAO placeDishDAO)
    {
        _vendorModule = vendorModule;
        _vendorDAO = vendorDAO;
        _placeBUS = placeBUS;
        _placeDAO = placeDAO;
        _dishDAO = dishDAO;
        _dishBUS = dishBUS;
        _placeDishDAO = placeDishDAO;
    }

    public VendorCatalogDTO GetCatalog(long vendorUserId)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        var place = _placeDAO.GetByOwnerVendorId(vendorUserId);
        return new VendorCatalogDTO
        {
            Place = place,
            Dishes = _dishDAO.GetByOwnerVendorId(vendorUserId),
            Menu = place == null
                ? new List<PlaceDishDTO>()
                : _placeDishDAO.GetByPlaceIdForVendor(place.PlaceId, vendorUserId)
        };
    }

    public PlaceDTO SavePlace(long vendorUserId, PlaceDTO request)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        var existing = _placeDAO.GetByOwnerVendorId(vendorUserId);
        request.OwnerVendorId = vendorUserId;
        request.IsActive = true;

        if (existing == null)
        {
            request.PlaceId = 0;
            var id = _placeBUS.Create(request);
            if (!_vendorDAO.AssignPlaceToVendor(vendorUserId, id))
                throw new InvalidOperationException("Không thể liên kết sạp với Vendor.");
            return _placeDAO.GetById(id)
                ?? throw new InvalidOperationException("Không thể tải lại thông tin sạp.");
        }

        request.PlaceId = existing.PlaceId;
        request.CreatedAt = existing.CreatedAt;
        if (!_placeBUS.Update(request))
            throw new InvalidOperationException("Không thể cập nhật thông tin sạp.");
        return _placeDAO.GetById(existing.PlaceId)
            ?? throw new InvalidOperationException("Không thể tải lại thông tin sạp.");
    }

    public long CreateDish(long vendorUserId, VendorDishRequestDTO request)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        var place = RequirePlace(vendorUserId);
        var dish = ToDish(request, vendorUserId);
        var dishId = _dishBUS.Create(dish);
        _placeDishDAO.UpsertMenuItem(new PlaceDishDTO
        {
            PlaceId = place.PlaceId,
            DishId = dishId,
            Price = request.MenuPrice ?? request.AveragePrice,
            IsRecommended = request.IsRecommended,
            Note = request.Note?.Trim()
        });
        return dishId;
    }

    public bool UpdateDish(long vendorUserId, long dishId, VendorDishRequestDTO request)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        var current = _dishDAO.GetOwnedById(dishId, vendorUserId)
            ?? throw new UnauthorizedAccessException("Món ăn không thuộc sạp của Vendor.");
        var dish = ToDish(request, vendorUserId);
        dish.DishId = dishId;
        dish.IsActive = current.IsActive;
        if (!_dishBUS.Update(dish)) return false;

        var place = RequirePlace(vendorUserId);
        return _placeDishDAO.UpsertMenuItem(new PlaceDishDTO
        {
            PlaceId = place.PlaceId,
            DishId = dishId,
            Price = request.MenuPrice ?? request.AveragePrice,
            IsRecommended = request.IsRecommended,
            Note = request.Note?.Trim()
        });
    }

    public bool DeactivateDish(long vendorUserId, long dishId)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        EnsureDishOwner(dishId, vendorUserId);
        return _dishBUS.Deactivate(dishId);
    }

    public bool RestoreDish(long vendorUserId, long dishId)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        EnsureDishOwner(dishId, vendorUserId);
        return _dishBUS.Restore(dishId);
    }

    public bool UpsertMenuItem(
        long vendorUserId,
        long dishId,
        VendorMenuItemRequestDTO request)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        var place = RequirePlace(vendorUserId);
        EnsureDishOwner(dishId, vendorUserId);
        if (request.Price.HasValue && request.Price.Value < 0)
            throw new ArgumentException("Giá món không được âm.");
        return _placeDishDAO.UpsertMenuItem(new PlaceDishDTO
        {
            PlaceId = place.PlaceId,
            DishId = dishId,
            Price = request.Price,
            IsRecommended = request.IsRecommended,
            Note = request.Note?.Trim()
        });
    }

    public bool RemoveMenuItem(long vendorUserId, long dishId)
    {
        _vendorModule.EnsureCanManageContent(vendorUserId);
        var place = RequirePlace(vendorUserId);
        EnsureDishOwner(dishId, vendorUserId);
        return _placeDishDAO.RemoveDishFromPlace(place.PlaceId, dishId);
    }

    private PlaceDTO RequirePlace(long vendorUserId) =>
        _placeDAO.GetByOwnerVendorId(vendorUserId)
        ?? throw new InvalidOperationException("Vendor cần khai báo thông tin sạp trước.");

    private void EnsureDishOwner(long dishId, long vendorUserId)
    {
        if (!_dishDAO.IsOwnedByVendor(dishId, vendorUserId))
            throw new UnauthorizedAccessException("Món ăn không thuộc sạp của Vendor.");
    }

    private static DishDTO ToDish(VendorDishRequestDTO request, long vendorUserId)
    {
        if (string.IsNullOrWhiteSpace(request.DishName))
            throw new ArgumentException("Tên món ăn là bắt buộc.");
        if (request.CategoryId <= 0)
            throw new ArgumentException("Danh mục món ăn là bắt buộc.");
        if ((request.AveragePrice.HasValue && request.AveragePrice.Value < 0) ||
            (request.MenuPrice.HasValue && request.MenuPrice.Value < 0))
            throw new ArgumentException("Giá món không được âm.");

        return new DishDTO
        {
            DishName = request.DishName.Trim(),
            CategoryId = request.CategoryId,
            OwnerVendorId = vendorUserId,
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            AveragePrice = request.AveragePrice,
            IsSignatureDish = request.IsSignatureDish,
            IsActive = true
        };
    }
}
