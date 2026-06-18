using Npgsql;
using VinhKhanhNarration.Api.Database;
using VinhKhanhNarration.Api.DTO;

namespace VinhKhanhNarration.Api.DAO;

public class PlaceDAO : GenericCrudDAO<PlaceDTO>
{
    public PlaceDAO(DbConnectionFactory factory) : base(factory) { }

    public override List<PlaceDTO> GetActive()
    {
        return QueryList(@"
            SELECT p.*
            FROM places p
            WHERE p.is_active = TRUE
              AND (
                    p.owner_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = p.owner_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
            ORDER BY p.place_id DESC;");
    }

    public List<PlaceDTO> GetPoiEnabledPlaces()
    {
        return QueryList(@"
            SELECT p.*
            FROM places p
            WHERE p.is_active = TRUE
              AND p.is_poi = TRUE
              AND p.is_geofence_enabled = TRUE
              AND (
                    p.owner_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = p.owner_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
            ORDER BY p.priority DESC;");
    }

    public List<PlaceDTO> SearchByName(string keyword)
    {
        return QueryList("SELECT * FROM places WHERE place_name ILIKE @keyword ORDER BY place_name;", cmd => cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%"));
    }

    public List<PlaceDTO> GetNearbyPlaces(decimal latitude, decimal longitude, double radiusMeters)
    {
        const string sql = @"
            SELECT * FROM places
            WHERE is_active = TRUE
              AND is_poi = TRUE
              AND is_geofence_enabled = TRUE
              AND (
                    owner_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = places.owner_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
              AND latitude IS NOT NULL
              AND longitude IS NOT NULL
              AND (
                  6371000 * 2 * ASIN(SQRT(
                      POWER(SIN(RADIANS((latitude - @lat) / 2)), 2) +
                      COS(RADIANS(@lat)) * COS(RADIANS(latitude)) *
                      POWER(SIN(RADIANS((longitude - @lng) / 2)), 2)
                  ))
              ) <= @radius
            ORDER BY priority DESC;";

        return QueryList(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@lat", latitude);
            cmd.Parameters.AddWithValue("@lng", longitude);
            cmd.Parameters.AddWithValue("@radius", radiusMeters);
        });
    }
}

public class DishCategoryDAO : GenericCrudDAO<DishCategoryDTO>
{
    public DishCategoryDAO(DbConnectionFactory factory) : base(factory) { }

    public bool IsCategoryNameExists(string categoryName)
    {
        return Exists("SELECT COUNT(1) FROM dish_categories WHERE category_name = @name;", cmd => cmd.Parameters.AddWithValue("@name", categoryName));
    }
}

public class DishDAO : GenericCrudDAO<DishDTO>
{
    public DishDAO(DbConnectionFactory factory) : base(factory) { }

    public List<DishDTO> GetByCategoryId(long categoryId)
    {
        return QueryList("SELECT * FROM dishes WHERE category_id = @id ORDER BY dish_name;", cmd => cmd.Parameters.AddWithValue("@id", categoryId));
    }

    public List<DishDTO> SearchByName(string keyword)
    {
        return QueryList("SELECT * FROM dishes WHERE dish_name ILIKE @keyword ORDER BY dish_name;", cmd => cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%"));
    }

    public List<DishDTO> GetSignatureDishes()
    {
        return QueryList("SELECT * FROM dishes WHERE is_active = TRUE AND is_signature_dish = TRUE ORDER BY dish_name;");
    }
}

public class PlaceDishDAO : GenericCrudDAO<PlaceDishDTO>
{
    public PlaceDishDAO(DbConnectionFactory factory) : base(factory) { }

    public override List<PlaceDishDTO> GetActive() => GetAll();
    public override bool Restore(long id) => false;

    public List<PlaceDishDTO> GetByPlaceId(long placeId)
    {
        return QueryList("SELECT * FROM place_dishes WHERE place_id = @id ORDER BY place_dish_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", placeId));
    }

    public List<PlaceDishDTO> GetByDishId(long dishId)
    {
        return QueryList("SELECT * FROM place_dishes WHERE dish_id = @id ORDER BY place_dish_id DESC;", cmd => cmd.Parameters.AddWithValue("@id", dishId));
    }

    public bool IsPlaceDishExists(long placeId, long dishId)
    {
        return Exists("SELECT COUNT(1) FROM place_dishes WHERE place_id = @placeId AND dish_id = @dishId;", cmd =>
        {
            cmd.Parameters.AddWithValue("@placeId", placeId);
            cmd.Parameters.AddWithValue("@dishId", dishId);
        });
    }

    public bool RemoveDishFromPlace(long placeId, long dishId)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM place_dishes WHERE place_id = @placeId AND dish_id = @dishId;", conn);
        cmd.Parameters.AddWithValue("@placeId", placeId);
        cmd.Parameters.AddWithValue("@dishId", dishId);
        return cmd.ExecuteNonQuery() > 0;
    }
}
