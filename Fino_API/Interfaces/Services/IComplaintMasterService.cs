using FintechApp.Requests.ComplaintMaster;
using FintechApp.Responses.ComplaintMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IComplaintMasterService
    {
        Task<List<ComplaintMasterResponse>> GetAll();

        /// <summary>The queue scoped to what this caller may see (admin = all, upline = network, else own).</summary>
        Task<List<ComplaintMasterResponse>> GetAllForCaller(int callerUserID, int callerUserTypeID);

        /// <summary>Complaints raised against one transaction, scoped the same way.</summary>
        Task<List<ComplaintMasterResponse>> GetByTransaction(int transactionMasterID, int callerUserID, int callerUserTypeID);

        Task<List<ComplaintMasterResponse>> GetByUserID(int userID);
        Task<ComplaintMasterResponse?> GetByID(int id);
        Task<string> Create(CreateComplaintMasterRequest request);
        Task<string> Update(UpdateComplaintMasterRequest request);

        /// <summary>Accept (refunds the transaction) or reject. Admin / Support only.</summary>
        Task<string> Resolve(ResolveComplaintMasterRequest request);

        /// <summary>Withdraw a still-open complaint. The raiser's own action; moves no money.</summary>
        Task<string> Cancel(CancelComplaintMasterRequest request, int callerUserID, int callerUserTypeID);

        Task<string> Delete(int id);
    }
}
