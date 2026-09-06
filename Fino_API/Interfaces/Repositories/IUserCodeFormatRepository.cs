using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserCodeFormatRepository
    {
        Task<List<UserCodeFormat>> GetAll();
        Task<UserCodeFormat?> GetByUserTypeID(int userTypeId);
        Task<int> Insert(UserCodeFormat entity);
        Task<int> Update(UserCodeFormat entity);
        Task<int> Delete(int userCodeFormatId);

        /// <summary>
        /// Hands out the next running number for a user type in one atomic UPDATE, so two admins
        /// adding a user at the same moment can never receive the same number. Returns null when
        /// the type has no (active) format row. A <paramref name="period"/> different from the one
        /// stored restarts the count at StartNumber (yearly / monthly reset).
        /// </summary>
        Task<int?> AllocateNextSequence(int userTypeId, string period);

        /// <summary>True when the code is already carried by a user — deleted users included, so a
        /// removed account never hands its code to somebody else.</summary>
        Task<bool> UserCodeExists(string code);

        /// <summary>
        /// Highest running number already in use for a code prefix/pattern, read straight off the
        /// existing users. Used to seed a freshly-configured type so it continues the series that
        /// data migration or manual entry already started, instead of colliding with it.
        /// </summary>
        Task<int> MaxSequenceInUse(int userTypeId, string prefix, string suffix, int sequenceLength);
    }
}
