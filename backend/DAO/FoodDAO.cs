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
        return QueryList(@"
            SELECT p.*
            FROM places p
            WHERE p.is_active = TRUE
              AND p.place_name ILIKE @keyword
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
            ORDER BY p.place_name;",
            cmd => cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%"));
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

    public PlaceDTO? GetByOwnerVendorId(long vendorUserId) =>
        QuerySingle(@"
            SELECT *
            FROM places
            WHERE owner_vendor_id = @vendorUserId
            ORDER BY place_id
            LIMIT 1;",
            cmd => cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId));

    public bool IsOwnedByVendor(long placeId, long vendorUserId) =>
        Exists(@"
            SELECT COUNT(1)
            FROM places
            WHERE place_id = @placeId
              AND owner_vendor_id = @vendorUserId;", cmd =>
            {
                cmd.Parameters.AddWithValue("@placeId", placeId);
                cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            });
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

    public override List<DishDTO> GetActive() => QueryPublicDishes();

    public List<DishDTO> GetByCategoryId(long categoryId) =>
        QueryPublicDishes("d.category_id = @id", cmd => cmd.Parameters.AddWithValue("@id", categoryId));

    public List<DishDTO> SearchByName(string keyword) =>
        QueryPublicDishes("d.dish_name ILIKE @keyword", cmd =>
            cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%"));

    public List<DishDTO> GetSignatureDishes() =>
        QueryPublicDishes("d.is_signature_dish = TRUE");

    private List<DishDTO> QueryPublicDishes(
        string? extraFilter = null,
        Action<NpgsqlCommand>? bind = null)
    {
        var extra = string.IsNullOrWhiteSpace(extraFilter)
            ? string.Empty
            : $" AND ({extraFilter})";
        return QueryList($@"
            SELECT d.*
            FROM dishes d
            WHERE d.is_active = TRUE
              AND (
                    d.owner_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users vu
                        JOIN vendor_subscriptions vs
                          ON vs.vendor_user_id = vu.vendor_user_id
                         AND vs.status = 'Active'
                         AND vs.expires_at > CURRENT_TIMESTAMP
                        WHERE vu.vendor_user_id = d.owner_vendor_id
                          AND vu.account_status IN ('Active','ExpiringSoon')
                          AND vu.is_active = TRUE
                    )
                  )
              {extra}
            ORDER BY d.dish_name;", bind);
    }

    public List<DishDTO> GetByOwnerVendorId(long vendorUserId) =>
        QueryList(@"
            SELECT *
            FROM dishes
            WHERE owner_vendor_id = @vendorUserId
            ORDER BY is_active DESC, dish_name;",
            cmd => cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId));

    public DishDTO? GetOwnedById(long dishId, long vendorUserId) =>
        QuerySingle(@"
            SELECT *
            FROM dishes
            WHERE dish_id = @dishId
              AND owner_vendor_id = @vendorUserId
            LIMIT 1;", cmd =>
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            });

    public bool IsOwnedByVendor(long dishId, long vendorUserId) =>
        Exists(@"
            SELECT COUNT(1)
            FROM dishes
            WHERE dish_id = @dishId
              AND owner_vendor_id = @vendorUserId;", cmd =>
            {
                cmd.Parameters.AddWithValue("@dishId", dishId);
                cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            });
}

public class PlaceDishDAO : GenericCrudDAO<PlaceDishDTO>
{
    public PlaceDishDAO(DbConnectionFactory factory) : base(factory) { }

    public override List<PlaceDishDTO> GetActive() => GetAll();
    public override bool Restore(long id) => false;

    public override List<PlaceDishDTO> GetAll() => QueryList(@"
        SELECT pd.*,
               d.dish_name,
               d.description AS dish_description,
               d.image_url AS dish_image_url,
               d.category_id,
               d.owner_vendor_id AS dish_owner_vendor_id,
               p.place_name
        FROM place_dishes pd
        JOIN dishes d ON d.dish_id = pd.dish_id
        JOIN places p ON p.place_id = pd.place_id
        ORDER BY pd.place_dish_id DESC;");

    public override PlaceDishDTO? GetById(long id) => QuerySingle(@"
        SELECT pd.*,
               d.dish_name,
               d.description AS dish_description,
               d.image_url AS dish_image_url,
               d.category_id,
               d.owner_vendor_id AS dish_owner_vendor_id,
               p.place_name
        FROM place_dishes pd
        JOIN dishes d ON d.dish_id = pd.dish_id
        JOIN places p ON p.place_id = pd.place_id
        WHERE pd.place_dish_id = @id
        LIMIT 1;", cmd => cmd.Parameters.AddWithValue("@id", id));

    public List<PlaceDishDTO> GetByPlaceId(long placeId)
    {
        return QueryList(@"
            SELECT pd.*,
                   d.dish_name,
                   d.description AS dish_description,
                   d.image_url AS dish_image_url,
                   d.category_id,
                   d.owner_vendor_id AS dish_owner_vendor_id,
                   p.place_name
            FROM place_dishes pd
            JOIN dishes d ON d.dish_id = pd.dish_id
            JOIN places p ON p.place_id = pd.place_id
            WHERE pd.place_id = @id
              AND d.is_active = TRUE
              AND p.is_active = TRUE
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
              AND (
                    d.owner_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users dvu
                        JOIN vendor_subscriptions dvs
                          ON dvs.vendor_user_id = dvu.vendor_user_id
                         AND dvs.status = 'Active'
                         AND dvs.expires_at > CURRENT_TIMESTAMP
                        WHERE dvu.vendor_user_id = d.owner_vendor_id
                          AND dvu.account_status IN ('Active','ExpiringSoon')
                          AND dvu.is_active = TRUE
                    )
                  )
            ORDER BY pd.is_recommended DESC, pd.place_dish_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@id", placeId));
    }

    public List<PlaceDishDTO> GetByPlaceIdForVendor(long placeId, long vendorUserId)
    {
        return QueryList(@"
            SELECT pd.*,
                   d.dish_name,
                   d.description AS dish_description,
                   d.image_url AS dish_image_url,
                   d.category_id,
                   d.owner_vendor_id AS dish_owner_vendor_id,
                   p.place_name
            FROM place_dishes pd
            JOIN dishes d ON d.dish_id = pd.dish_id
            JOIN places p ON p.place_id = pd.place_id
            WHERE pd.place_id = @placeId
              AND p.owner_vendor_id = @vendorUserId
              AND d.owner_vendor_id = @vendorUserId
            ORDER BY d.is_active DESC, pd.is_recommended DESC, pd.place_dish_id DESC;",
            cmd =>
            {
                cmd.Parameters.AddWithValue("@placeId", placeId);
                cmd.Parameters.AddWithValue("@vendorUserId", vendorUserId);
            });
    }

    public PlaceDishDTO? GetByPlaceAndDish(long placeId, long dishId) =>
        QuerySingle(@"
            SELECT pd.*,
                   d.dish_name,
                   d.description AS dish_description,
                   d.image_url AS dish_image_url,
                   d.category_id,
                   d.owner_vendor_id AS dish_owner_vendor_id,
                   p.place_name
            FROM place_dishes pd
            JOIN dishes d ON d.dish_id = pd.dish_id
            JOIN places p ON p.place_id = pd.place_id
            WHERE pd.place_id = @placeId AND pd.dish_id = @dishId
            LIMIT 1;", cmd =>
            {
                cmd.Parameters.AddWithValue("@placeId", placeId);
                cmd.Parameters.AddWithValue("@dishId", dishId);
            });

    public bool UpsertMenuItem(PlaceDishDTO dto)
    {
        using var conn = CreateConnection();
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO place_dishes
            (place_id, dish_id, price, is_recommended, note)
            VALUES (@placeId, @dishId, @price, @isRecommended, @note)
            ON CONFLICT (place_id, dish_id)
            DO UPDATE SET price = EXCLUDED.price,
                          is_recommended = EXCLUDED.is_recommended,
                          note = EXCLUDED.note,
                          updated_at = CURRENT_TIMESTAMP;", conn);
        cmd.Parameters.AddWithValue("@placeId", dto.PlaceId);
        cmd.Parameters.AddWithValue("@dishId", dto.DishId);
        cmd.Parameters.AddWithValue("@price", DbValue(dto.Price));
        cmd.Parameters.AddWithValue("@isRecommended", dto.IsRecommended);
        cmd.Parameters.AddWithValue("@note", DbValue(dto.Note));
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<PlaceDishDTO> GetByDishId(long dishId)
    {
        return QueryList(@"
            SELECT pd.*,
                   d.dish_name,
                   d.description AS dish_description,
                   d.image_url AS dish_image_url,
                   d.category_id,
                   d.owner_vendor_id AS dish_owner_vendor_id,
                   p.place_name
            FROM place_dishes pd
            JOIN dishes d ON d.dish_id = pd.dish_id
            JOIN places p ON p.place_id = pd.place_id
            WHERE pd.dish_id = @id
              AND d.is_active = TRUE
              AND p.is_active = TRUE
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
              AND (
                    d.owner_vendor_id IS NULL
                    OR EXISTS (
                        SELECT 1
                        FROM vendor_users dvu
                        JOIN vendor_subscriptions dvs
                          ON dvs.vendor_user_id = dvu.vendor_user_id
                         AND dvs.status = 'Active'
                         AND dvs.expires_at > CURRENT_TIMESTAMP
                        WHERE dvu.vendor_user_id = d.owner_vendor_id
                          AND dvu.account_status IN ('Active','ExpiringSoon')
                          AND dvu.is_active = TRUE
                    )
                  )
            ORDER BY pd.place_dish_id DESC;",
            cmd => cmd.Parameters.AddWithValue("@id", dishId));
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
