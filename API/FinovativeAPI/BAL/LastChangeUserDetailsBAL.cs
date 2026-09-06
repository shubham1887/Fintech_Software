using Project.DAL;
using System.Collections.Generic;

namespace Project.BAL
{
    public class LastChangeUserDetailsBAL
    {

        internal (List<Project.Response.Onboarding.LastChangeUserDetailsViewModel>, string) SelectByID(Project.Requests.Onboarding.SelectByIDRequest request)
        {
            string failureMessage = string.Empty;

            List<Project.Response.Onboarding.LastChangeUserDetailsViewModel> response = new List<Project.Response.Onboarding.LastChangeUserDetailsViewModel>();

            response = new LastChangeUserDetailsDal().SelectByID(request.OnboardingUserID);
            if (response.Count < 0)
            {
                failureMessage = Message.OnboardingMessages.DataNotFound;
                return (response, failureMessage);
            }
            failureMessage = StaticModel.StatusType.Ok;
            return (response, failureMessage);
        }
    }
}
