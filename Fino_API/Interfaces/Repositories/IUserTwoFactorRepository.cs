using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserTwoFactorRepository
    {
        /// <summary>
        /// This user's enrolment, confirmed or not. Read straight from the database, never from a
        /// cached list: a secret is a credential, and a stale cache here would keep accepting
        /// codes from a phone whose enrolment was already revoked.
        /// </summary>
        Task<UserTwoFactor?> GetByUserID(int userId);

        /// <summary>Start (or restart) enrolment: replaces any existing row for the user.</summary>
        Task<UserTwoFactor> StartEnrolment(int userId, string secretBase32);

        Task<int> Update(UserTwoFactor entity);

        /// <summary>Remove the enrolment entirely — the secret must not outlive it.</summary>
        Task<int> DeleteForUser(int userId);
    }
}
