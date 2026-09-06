
$base = "C:\Users\Shubham\OneDrive\Desktop\new Fintech application\Fino_API"

$entities = @(
    @{ N="UserWisePlanLimit"; PK="UserWisePlanLimitID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="UserTypeID";t="int"},@{n="CircleHitCount";t="int"},@{n="PlanHitCount";t="int"},@{n="BillFetchHitCount";t="int"},@{n="IsActive";t="bool"},@{n="IsDelete";t="bool"}) },
    @{ N="UserWisePlanView"; PK="UserWisePlanViewID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="ServiceID";t="int"},@{n="OperatorID";t="int"},@{n="ConsumerNumber";t="string"},@{n="ProviderID";t="int"}) },
    @{ N="Sender"; PK="SenderID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="Name";t="string"},@{n="Mobile";t="string"},@{n="GUID";t="string"},@{n="Address";t="string"},@{n="UserID";t="int"},@{n="IsActive";t="bool"},@{n="IsOTPVerified";t="bool"},@{n="IsKYC";t="bool"},@{n="KYCDocumentURL";t="string"},@{n="IPAddress";t="string"},@{n="Pincode";t="string"}) },
    @{ N="Beneficiary"; PK="BeneficiaryID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="SenderID";nav="Sender";t="Sender"},@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="Name";t="string"},@{n="BankName";t="string"},@{n="AccountNo";t="string"},@{n="IFSC";t="string"},@{n="GUID";t="string"},@{n="SenderMobile";t="string"},@{n="IPAddress";t="string"},@{n="SenderID";t="int"},@{n="UserID";t="int"},@{n="IsActive";t="bool"},@{n="IsOTPVerified";t="bool"},@{n="IsNameVerified";t="bool"}) },
    @{ N="SenderOTP"; PK="SenderOTPID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="SenderID";nav="Sender";t="Sender"},@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="Mobile";t="string"},@{n="OTP";t="string"},@{n="SenderID";t="int"},@{n="UserID";t="int"},@{n="IsOTPVerified";t="bool"}) },
    @{ N="BeneficiaryOTP"; PK="BeneficiaryOTPID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="SenderID";nav="Sender";t="Sender"},@{f="BeneficiaryID";nav="Beneficiary";t="Beneficiary"},@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="SenderMobile";t="string"},@{n="OTP";t="string"},@{n="SenderID";t="int"},@{n="BeneficiaryID";t="int"},@{n="UserID";t="int"},@{n="IsOTPVerified";t="bool"}) },
    @{ N="TransactionDetails"; PK="TransactionDetailID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="ReferenceID";t="int"},@{n="AssignedUserID";t="int"},@{n="OperatorID";t="int"},@{n="ServiceID";t="int"},@{n="WalletID";t="int"},@{n="DOCType";t="string"},@{n="OpeningBalance";t="decimal"},@{n="ClosingBalance";t="decimal"},@{n="TransactionAmount";t="decimal"},@{n="DiscountAmount";t="decimal"},@{n="GST";t="decimal"},@{n="TDS";t="decimal"},@{n="AmountCreditDebit";t="string"},@{n="CommissionCreditDebit";t="string"},@{n="TransactionDescription";t="string"},@{n="ConsumerNumber";t="string"},@{n="PayMode";t="string"}) },
    @{ N="TransactionProviderHistory"; PK="RechargeProviderHistoryID"; HasBase=$true; Filter=""; FKs=@(@{f="TransactionMasterID";nav="TransactionMaster";t="TransactionMaster"}); Fields=@(@{n="ProviderID";t="int"},@{n="OperatorID";t="int"},@{n="TransactionMasterID";t="int"},@{n="ProviderRequestID";t="string"}) },
    @{ N="PaymentGateway"; PK="PaymentGatewayID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"}); Fields=@(@{n="UserID";t="int"},@{n="OrderID";t="string"},@{n="Amount";t="decimal"},@{n="GatewayCharge";t="decimal"},@{n="Commission";t="decimal"},@{n="WalletID";t="int"},@{n="Status";t="string"},@{n="TransactionID";t="string"},@{n="PayMode";t="string"},@{n="Response";t="string"},@{n="PaymentProviderID";t="int"},@{n="IsUpdate";t="int"},@{n="OpeningBalance";t="decimal"},@{n="ClosingBalance";t="decimal"},@{n="Remark";t="string"},@{n="DistributerUserID";t="int"},@{n="SuperDistributerUserID";t="int"},@{n="DistributerCommission";t="decimal"},@{n="SuperDistributerCommission";t="decimal"},@{n="RetailerTDS";t="decimal"},@{n="DistributerTDS";t="decimal"},@{n="SuperDistributerTDS";t="decimal"},@{n="AdminGST";t="decimal"},@{n="ReferenceID";t="string"},@{n="ServiceID";t="int"},@{n="PaymentStatus";t="string"}) },
    @{ N="TransactionManualUpdate"; PK="TransactionManualUpdateID"; HasBase=$true; Filter=""; FKs=@(@{f="TransactionMasterID";nav="TransactionMaster";t="TransactionMaster"},@{f="ManualUpdatedBy";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="TransactionMasterID";t="int"},@{n="ManualUpdatedBy";t="int"},@{n="ProviderrefNo";t="string"},@{n="OldStatus";t="string"},@{n="NewStatus";t="string"}) },
    @{ N="TransactionStatusChange"; PK="TransactionStatusChangeID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="TransactionMasterID";nav="TransactionMaster";t="TransactionMaster"}); Fields=@(@{n="UserID";t="int"},@{n="TransactionMasterID";t="int"},@{n="TransactionDOCInt";t="int"},@{n="OldStatus";t="string"},@{n="NewStatus";t="string"},@{n="Amount";t="decimal"},@{n="Remark";t="string"}) },
    @{ N="ThreewayReconCompletion"; PK="ThreewayReconCompletionID"; HasBase=$true; Filter=""; FKs=@(@{f="TransactionMasterID";nav="TransactionMaster";t="TransactionMaster"},@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"},@{f="OperatorID";nav="OperatorMaster";t="OperatorMaster"}); Fields=@(@{n="TransactionMasterID";t="int"},@{n="ServiceID";t="int"},@{n="OperatorID";t="int"}) },
    @{ N="UserActionMaster"; PK="UserActionMasterID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="Type";t="string"},@{n="Status";t="string"},@{n="Description";t="string"},@{n="Platform";t="string"},@{n="FingurePrint";t="string"},@{n="IPAddress";t="string"}) },
    @{ N="AuthorizationMaster"; PK="AuthorizationMasterID"; HasBase=$true; Filter=""; FKs=@(@{f="UserTypeID";nav="UserTypeMaster";t="UserTypeMaster"}); Fields=@(@{n="Controller";t="string"},@{n="Action";t="string"},@{n="UserTypeID";t="int"},@{n="IsActive";t="bool"}) },
    @{ N="UserPaymentCollectMapping"; PK="UserPaymentCollectMappingID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="ProviderName";t="string"},@{n="UserID";t="int"},@{n="Key";t="string"},@{n="Value";t="string"}) },
    @{ N="UserPlanUtilization"; PK="UserPlanUtilizationID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="CircleFetchCount";t="int"},@{n="PlanFetchCount";t="int"},@{n="BillFetchCount";t="int"},@{n="TotalPlanCount";t="int"},@{n="TotalCircleCount";t="int"},@{n="TotalBillFetchCount";t="int"}) },
    @{ N="MPinHistory"; PK="MPinHistoryID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="Mpin";t="int"},@{n="DeviceID";t="string"},@{n="Device";t="string"}) },
    @{ N="WrongPasswordHistory"; PK="WrongPasswordHistoryID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="MobileNumber";t="string"},@{n="IPAddress";t="string"}) },
    @{ N="FailedSignUpData"; PK="FailedSignUpDataID"; HasBase=$true; Filter=""; FKs=@(); Fields=@(@{n="MobileNumber";t="string"},@{n="PANNo";t="string"},@{n="ReferralCode";t="string"},@{n="EmployeeID";t="string"},@{n="OrderID";t="string"},@{n="RedirectURL";t="string"},@{n="Status";t="string"},@{n="PinCode";t="string"},@{n="Description";t="string"}) },
    @{ N="ServiceSubscriptionCreatePlanMaster"; PK="ServiceSubscriptionCreatePlanMasterID"; HasBase=$true; Filter=""; FKs=@(); Fields=@(@{n="PlanName";t="string"},@{n="Amount";t="decimal"},@{n="ImageURL";t="string"},@{n="IsDefault";t="bool"}) },
    @{ N="ServiceSubscriptionPlanDetails"; PK="ServiceSubscriptionPlanDetailsID"; HasBase=$true; Filter=""; FKs=@(@{f="ServiceSubscriptionCreatePlanMasterID";nav="ServiceSubscriptionCreatePlanMaster";t="ServiceSubscriptionCreatePlanMaster"}); Fields=@(@{n="ServiceSubscriptionCreatePlanMasterID";t="int"},@{n="ServiceID";t="int"}) },
    @{ N="UserServiceSubscriptionDetails"; PK="UserServiceSubscriptionDetailsID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"}); Fields=@(@{n="UserID";t="int"},@{n="ServiceID";t="int"},@{n="ServiceAndUserwiseSubscriptionID";t="int"},@{n="Charge";t="decimal"}) },
    @{ N="UserServiceSubscriptionCharge"; PK="UserServiceSubscriptionChargeID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="ServiceID";t="int"},@{n="ServiceAndUserwiseSubscriptionID";t="int"},@{n="Charge";t="decimal"},@{n="Sequence";t="int"},@{n="IsAdminApproved";t="bool"}) },
    @{ N="PrepaidActivation"; PK="PrepaidActivationID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="OperatorID";t="int"},@{n="UserID";t="int"},@{n="ActivationType";t="string"},@{n="AlternateNumber";t="string"},@{n="Name";t="string"},@{n="Address";t="string"},@{n="Pincode";t="string"},@{n="Status";t="string"}) },
    @{ N="PureDMTTransferBeneDetail"; PK="PureDMTTransferBeneDetailID"; HasBase=$true; Filter=""; FKs=@(); Fields=@(@{n="SenderNumber";t="string"},@{n="ProviderID";t="int"},@{n="BeneID";t="string"},@{n="AccountNumber";t="string"},@{n="VerifyBeneName";t="string"},@{n="IsVerify";t="bool"}) },
    @{ N="SubCategory"; PK="SubCategoryID"; HasBase=$true; Filter=""; FKs=@(@{f="CategoryID";nav="Category";t="Category"}); Fields=@(@{n="CategoryID";t="int"},@{n="SubCategoryName";t="string"},@{n="ShortDescription";t="string"},@{n="LongDescription";t="string"}) },
    @{ N="CategoryChannelMapping"; PK="CategoryChannelMappingID"; HasBase=$true; Filter=""; FKs=@(@{f="ChannelCategoryID";nav="ChannelCategory";t="ChannelCategory"}); Fields=@(@{n="ChannelCategoryID";t="int"},@{n="ChannelName";t="string"},@{n="ChannelImagePath";t="string"},@{n="IsDelete";t="bool"}) }
)

function Get-Default($t) {
    if ($t -eq "string") { ' = string.Empty;' }
    elseif ($t -eq "DateTime") { ' = DateTime.MinValue;' }
    else { '' }
}

$dbSetEntries = @()
$ckEntries = @()
$diEntries = @()
$fkConfigs = @()

foreach ($e in $entities) {
    $n = $e.N; $pk = $e.PK; $hasBase = $e.HasBase; $filter = $e.Filter; $fks = $e.FKs; $fields = $e.Fields
    $dbSetName = "${n}s"; $ckName = "All${n}s"

    # ENTITY
    $baseClass = if ($hasBase) { " : BaseEntity" } else { "" }
    $navPropLines = $fks | ForEach-Object { "        public $($_.t) $($_.nav) { get; set; } = null!;" }
    $fieldPropLines = $fields | ForEach-Object { "        public $($_.t) $($_.n) { get; set; }$(Get-Default $_.t)" }
    $allProps = ($fieldPropLines + $navPropLines) -join [System.Environment]::NewLine
    $entityContent = "namespace FintechApp.Entities`n{`n    public class $n$baseClass`n    {`n        public int $pk { get; set; }`n$allProps`n    }`n}"
    Set-Content "$base\Entities\${n}.cs" -Value $entityContent -Encoding utf8

    # IREPO
    $filterLine = if ($filter) { "        Task<List<$n>> GetBy$filter(int $($filter.ToLower()));" } else { "" }
    $irepo = "using FintechApp.Entities;`nnamespace FintechApp.Interfaces.Repositories`n{`n    public interface I${n}Repository`n    {`n        Task<List<$n>> GetAll();`n        Task<${n}?> GetByID(int id);`n$filterLine`n        Task<int> Insert($n entity);`n        Task<int> Update($n entity);`n        Task<int> Delete(int id);`n    }`n}"
    Set-Content "$base\Interfaces\Repositories\I${n}Repository.cs" -Value $irepo -Encoding utf8

    # ISERVICE
    $filterSvcLine = if ($filter) { "        Task<List<${n}Response>> GetBy$filter(int $($filter.ToLower()));" } else { "" }
    $isvc = "using FintechApp.Requests.$n;`nusing FintechApp.Responses.$n;`nnamespace FintechApp.Interfaces.Services`n{`n    public interface I${n}Service`n    {`n        Task<List<${n}Response>> GetAll();`n        Task<${n}Response?> GetByID(int id);`n$filterSvcLine`n        Task<string> Create(Create${n}Request request);`n        Task<string> Update(Update${n}Request request);`n        Task<string> Delete(int id);`n    }`n}"
    Set-Content "$base\Interfaces\Services\I${n}Service.cs" -Value $isvc -Encoding utf8

    # MESSAGES
    $msg = "namespace FintechApp.Messages`n{`n    public static class ${n}Messages`n    {`n        public const string NotFound = `"$n not found.`";`n        public const string CreatedSuccessfully = `"$n created successfully.`";`n        public const string UpdatedSuccessfully = `"$n updated successfully.`";`n        public const string DeletedSuccessfully = `"$n deleted successfully.`";`n    }`n}"
    Set-Content "$base\Messages\${n}Messages.cs" -Value $msg -Encoding utf8

    # REQUESTS + RESPONSES
    New-Item -ItemType Directory -Force -Path "$base\Requests\$n" | Out-Null
    New-Item -ItemType Directory -Force -Path "$base\Responses\$n" | Out-Null

    $filterGetProp = if ($filter) { " public int $filter { get; set; }" } else { "" }
    $getReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Get${n}Request { public int $pk { get; set; }$filterGetProp } public class Get${n}RequestValidator : AbstractValidator<Get${n}Request> { public Get${n}RequestValidator() { RuleFor(x => x.$pk).GreaterThan(0); } } }"
    Set-Content "$base\Requests\$n\Get${n}Request.cs" -Value $getReq -Encoding utf8

    $createProps = ($fields | ForEach-Object { "public $($_.t) $($_.n) { get; set; }$(Get-Default $_.t)" }) -join " "
    $filterRule = if ($filter) { "RuleFor(x => x.$filter).GreaterThan(0);" } else { "" }
    $createReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Create${n}Request { $createProps } public class Create${n}RequestValidator : AbstractValidator<Create${n}Request> { public Create${n}RequestValidator() { $filterRule } } }"
    Set-Content "$base\Requests\$n\Create${n}Request.cs" -Value $createReq -Encoding utf8

    $updateProps = "public int $pk { get; set; } $createProps"
    $updateReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Update${n}Request { $updateProps } public class Update${n}RequestValidator : AbstractValidator<Update${n}Request> { public Update${n}RequestValidator() { RuleFor(x => x.$pk).GreaterThan(0); } } }"
    Set-Content "$base\Requests\$n\Update${n}Request.cs" -Value $updateReq -Encoding utf8

    $deleteReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Delete${n}Request { public int $pk { get; set; } } public class Delete${n}RequestValidator : AbstractValidator<Delete${n}Request> { public Delete${n}RequestValidator() { RuleFor(x => x.$pk).GreaterThan(0); } } }"
    Set-Content "$base\Requests\$n\Delete${n}Request.cs" -Value $deleteReq -Encoding utf8

    # RESPONSE
    $respLines = @("        public int $pk { get; set; }")
    foreach ($f in $fields) { $respLines += "        public $($f.t) $($f.n) { get; set; }$(Get-Default $f.t)" }
    $respLines += "        public DateTime DateTime { get; set; }"; $respLines += "        public DateTime UpdatedateTime { get; set; }"
    $respBody = $respLines -join [System.Environment]::NewLine
    Set-Content "$base\Responses\$n\${n}Response.cs" -Value "namespace FintechApp.Responses.$n {`n    public class ${n}Response {`n$respBody`n    }`n}" -Encoding utf8

    # REPOSITORY
    $getAllFilter = ".AsNoTracking().Where(x => !x.IsDeleted).ToList()"
    $softDelete = "entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result;"
    $filterRepoLine = if ($filter) { "        public async Task<List<$n>> GetBy$filter(int $($filter.ToLower())) { var list = await GetAll(); return list.Where(x => x.$filter == $($filter.ToLower())).ToList(); }" } else { "" }

    $repoContent = @"
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ${n}Repository : I${n}Repository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<$n>> _listCache;
        public ${n}Repository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<$n>>(); }
        public async Task<List<$n>> GetAll() => _listCache.GetOrCreate(CacheKeys.$ckName, () => _context.$dbSetName$getAllFilter) ?? new List<$n>();
        public async Task<${n}?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.$pk == id); }
$filterRepoLine
        public async Task<int> Insert($n entity) { await _context.$dbSetName.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result; }
        public async Task<int> Update($n entity) { _context.$dbSetName.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.$dbSetName.FindAsync(id); if (entity == null) return 0; $softDelete }
    }
}
"@
    Set-Content "$base\Repositories\${n}Repository.cs" -Value $repoContent -Encoding utf8

    # SERVICE
    $mapBody = ($fields | ForEach-Object { "$($_.n) = e.$($_.n)," }) -join " "
    $createBody = ($fields | ForEach-Object { "$($_.n) = request.$($_.n)," }) -join " "
    $updateBody = ($fields | ForEach-Object { "entity.$($_.n) = request.$($_.n);" }) -join " "
    $filterSvcLine2 = if ($filter) { "        public async Task<List<${n}Response>> GetBy$filter(int $($filter.ToLower())) { var list = await _repository.GetBy$filter($($filter.ToLower())); return list.Select(MapToResponse).ToList(); }" } else { "" }

    $svcContent = @"
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.$n;
using FintechApp.Responses.$n;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ${n}Service : I${n}Service
    {
        private readonly I${n}Repository _repository;
        public ${n}Service(I${n}Repository repository) { _repository = repository; }
        private static ${n}Response MapToResponse($n e) => new() { $pk = e.$pk, $mapBody DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<${n}Response>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<${n}Response?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
$filterSvcLine2
        public async Task<string> Create(Create${n}Request request) { var entity = new $n { $createBody }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(Update${n}Request request) { var entity = await _repository.GetByID(request.$pk); if (entity == null) return Messages.${n}Messages.NotFound; $updateBody await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.${n}Messages.NotFound; }
    }
}
"@
    Set-Content "$base\Services\${n}Service.cs" -Value $svcContent -Encoding utf8

    # CONTROLLER
    $filterCtrlBlock = ""
    if ($filter) {
        $filterCtrlBlock = @"

        [HttpPost] public async Task<APIResponse> GetBy$filter([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; Get${n}Request? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Get${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { var data = await _service.GetBy$filter(request.$filter); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Messages.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); }
        }
"@
    }

    $ctrlContent = @"
using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.$n;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ${n}Controller : ControllerBase
    {
        private readonly I${n}Service _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        public ${n}Controller(I${n}Service service, IConfiguration configuration) { _service = service; _logger = new Logger(); _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty); }
        [HttpPost] public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetAll(); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Messages.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
        [HttpPost] public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Get${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Get${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetByID(request.$pk); if (data == null) return ResponseProcess.GetFailResponse(Messages.${n}Messages.NotFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
$filterCtrlBlock
        [HttpPost] public async Task<APIResponse> Create([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Create${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Create${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { fail = await _service.Create(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.${n}Messages.CreatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
        [HttpPost] public async Task<APIResponse> Update([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Update${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Update${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { fail = await _service.Update(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.${n}Messages.UpdatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
        [HttpPost] public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Delete${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Delete${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { fail = await _service.Delete(request.$pk); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.${n}Messages.DeletedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
    }
}
"@
    Set-Content "$base\Controllers\${n}Controller.cs" -Value $ctrlContent -Encoding utf8

    $dbSetEntries += "        public DbSet<$n> $dbSetName { get; set; }"
    $ckEntries += "        public const string $ckName = `"Cache_${ckName}`";"
    $diEntries += "builder.Services.AddScoped<I${n}Repository, ${n}Repository>();"
    $diEntries += "builder.Services.AddScoped<I${n}Service, ${n}Service>();"
    foreach ($fk in $fks) {
        $fkConfigs += "            modelBuilder.Entity<$n>().HasOne(x => x.$($fk.nav)).WithMany().HasForeignKey(x => x.$($fk.f)).OnDelete(DeleteBehavior.Restrict);"
    }
}

Write-Output "Generated $($entities.Count) Phase 8 entities"
$dbSetEntries | Set-Content "$env:TEMP\phase8_dbsets.txt" -Encoding utf8
$ckEntries | Set-Content "$env:TEMP\phase8_cachekeys.txt" -Encoding utf8
$diEntries | Set-Content "$env:TEMP\phase8_di.txt" -Encoding utf8
$fkConfigs | Set-Content "$env:TEMP\phase8_fkconfigs.txt" -Encoding utf8
Write-Output "DbSets: $($dbSetEntries.Count), CacheKeys: $($ckEntries.Count), DI: $($diEntries.Count), FKs: $($fkConfigs.Count)"
