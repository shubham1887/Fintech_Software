using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IEmailSettingRepository
    {
        Task<List<EmailSettingMaster>> GetAll();
        Task<EmailSettingMaster?> GetByID(int emailSettingId);
        Task<int> Insert(EmailSettingMaster entity);
        Task<int> Update(EmailSettingMaster entity);
        Task<int> Delete(int emailSettingId);
    }
}
