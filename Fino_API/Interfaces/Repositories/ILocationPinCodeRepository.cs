using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ILocationPinCodeRepository
    {
        Task<LocationPinCodeMaster?> GetByPinCode(string pinCode);
        Task<bool> CheckPinCode(string pinCode);
        Task<int> Insert(LocationPinCodeMaster entity);
    }
}
