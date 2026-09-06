using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IRichTextMediaRepository
    {
        Task<List<RichTextMedia>> GetAll();
        Task<RichTextMedia?> GetByID(int id);
        Task<int> Insert(RichTextMedia entity);
        Task<int> Update(RichTextMedia entity);
        Task<int> Delete(int id);
    }
}
