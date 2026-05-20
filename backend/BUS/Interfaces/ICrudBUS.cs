namespace VinhKhanhNarration.Api.BUS.Interfaces;

public interface ICrudBUS<TDto, TKey>
{
    long Create(TDto dto);
    bool Update(TDto dto);
    bool Deactivate(TKey id);
    bool Restore(TKey id);
    TDto? GetById(TKey id);
    List<TDto> GetAll();
    List<TDto> GetActive();
}
