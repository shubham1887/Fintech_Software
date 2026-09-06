using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IPendingRegistrationRepository
    {
        Task<List<PendingRegistration>> GetAll();
        Task<List<PendingRegistration>> GetByStatus(string status);
        Task<PendingRegistration?> GetByID(int id);
        Task<int> Insert(PendingRegistration entity);
        Task<int> Update(PendingRegistration entity);

        /// <summary>
        /// Returns the first field ("Mobile number" / "Email address" / "PAN" / "Aadhaar" / "GST")
        /// that already belongs to an existing user, or null when none match. Blank inputs are skipped.
        /// </summary>
        Task<string?> FindDuplicateInUsers(string mobile, string email, string pan, string aadhaar, string gst);

        /// <summary>
        /// Same check, but against other still-pending registrations so a duplicate can't queue twice.
        /// </summary>
        Task<string?> FindDuplicateInPending(string mobile, string email, string pan, string aadhaar, string gst);
    }
}
