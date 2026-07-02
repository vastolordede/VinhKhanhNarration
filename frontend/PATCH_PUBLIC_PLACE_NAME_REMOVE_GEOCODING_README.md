# Public place name and Admin geocoding UI patch

## Changes

1. Public map bottom sheet always displays `selectedPlace.placeName` as the heading.
   The translated narration title remains narration metadata and is no longer used as the stall/place heading.
2. Removes the "Lấy tọa độ từ địa chỉ" action from the Admin Places screen.
3. Latitude and longitude remain editable manually.
4. Backend geocoding API is not removed; this patch only removes the unsupported frontend action.

## Files

- `frontend/src/features/public/MapExploreScreen.tsx`
- `frontend/src/features/admin/PlacesManagementScreen.tsx`

No database migration or environment-variable change is required.
