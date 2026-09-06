using Project.BAL;
using Project.Response;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class ResponseProcess
    {
        public static APIResponse GetSucessReason(string Message, int UserID = 0, string Action = "", string Description = "", Object model = null)
        {
            if (!string.IsNullOrEmpty(Action) && model != null)
            {
                Task.Run(() =>
                {
                    Description = new UserActionHistoryProcessing().GetDescriptionByAction(Action, model);
                    new UserActionHistoryBal().Insert(UserID, Action, Description, StatusType.Success);
                });
            }
            return new APIResponse
            {
                Status = true,
                Body = new EncryptDecrypt().Encrypt(Message)
            };
        }

        public static APIResponse GetFailReason(string Message, int UserID = 0, string Action = "", string Description = "")
        {
            Task.Run(() =>
            {
                new UserActionHistoryBal().Insert(UserID, Action, Description, StatusType.Failure);
            }
            );

            return new APIResponse
            {
                Status = false,
                Body = new EncryptDecrypt().Encrypt(Message)
            };
        }

        public bool CheckFailure(string Status)
        {
            Status = Status.Trim();

            if (Status.ToString().EqualsIgnoreCase("FAILURE")
                || (Status.ToString().EqualsIgnoreCase("FAIL"))
                || (Status.ToString().EqualsIgnoreCase("FAILED"))
                || (Status.ToString().EqualsIgnoreCase("REFUNDED"))
                )

                return true;

            return false;
        }

        public bool CheckSuccess(string Status)
        {
            Status = Status.Trim();

            return Status.ToString().EqualsIgnoreCase("SUCCESS");
        }

        public bool checkProcess(string Status)
        {
            Status = Status.Trim();

            if (Status.ToString().EqualsIgnoreCase("PROCESS"))
            {
                return true;
            }

            if (Status.ToString().EqualsIgnoreCase("PENDING"))
            {
                return true;
            }

            return false;
        }

        public static APIResponse GetSuccessReasonForAPIUser(string Message)
        {
            return new APIResponse
            {
                Status = true,
                Body = Message
            };
        }

        public static APIResponse GetFailReasonForAPIUser(string Message)
        {
            return new APIResponse
            {
                Status = false,
                Body = Message
            };
        }
    }
}