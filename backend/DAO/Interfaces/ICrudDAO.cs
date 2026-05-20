namespace VinhKhanhNarration.Api.DAO.Interfaces;

public interface ICrudDAO<TDto, TKey>
{
    long Insert(TDto dto);
    bool Update(TDto dto);
    bool SoftDelete(TKey id);
    bool Restore(TKey id);
    TDto? GetById(TKey id);
    List<TDto> GetAll();
    List<TDto> GetActive();
}
