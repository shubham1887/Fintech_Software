using ClosedXML.Excel;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AgentDetails;
using FintechApp.Responses.AgentDetails;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class AgentDetailsService : IAgentDetailsService
    {
        private readonly ILocationPinCodeRepository _pinCodeRepo;
        private readonly IUserRepository _userRepo;
        private readonly IProviderUserMappingRepository _providerUserMappingRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly Logger _logger;

        public AgentDetailsService(
            ILocationPinCodeRepository pinCodeRepo,
            IUserRepository userRepo,
            IProviderUserMappingRepository providerUserMappingRepo,
            ISettingMasterRepository settingRepo)
        {
            _pinCodeRepo = pinCodeRepo;
            _userRepo = userRepo;
            _providerUserMappingRepo = providerUserMappingRepo;
            _settingRepo = settingRepo;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, int successfulUpdates, int totalRecords)> Upload(AgentUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ExcelDocument?.Data))
                return ("Invalid file data.", 0, 0);

            string filename = "AgentDetails_" + Guid.NewGuid().ToString("N")[..5] + ".xlsx";
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Media", "AgentDetails");
            string filepath = Path.Combine(path, filename);

            try
            {
                Directory.CreateDirectory(path);
                byte[] fileBytes = Convert.FromBase64String(request.ExcelDocument.Data);
                File.WriteAllBytes(filepath, fileBytes);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("File could not be saved.", 0, 0);
            }

            int successCount = 0;
            int totalCount = 0;

            try
            {
                using var workbook = new XLWorkbook(filepath);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1).ToList();
                totalCount = rows.Count;

                if (request.ServiceType.Equals("BBPS", StringComparison.OrdinalIgnoreCase))
                    (successCount) = await ProcessBBPS(rows);
                else if (request.ServiceType.Equals("MicroATM", StringComparison.OrdinalIgnoreCase))
                    (successCount) = await ProcessMicroATM(rows);
                else if (request.ServiceType.Equals("PinMaster", StringComparison.OrdinalIgnoreCase))
                    (successCount) = await ProcessPinMaster(rows);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("Failed to read the Excel file.", successCount, totalCount);
            }

            return (StatusType.Ok, successCount, totalCount);
        }

        public async Task<(string failureMessage, AgentUserWrapper? data)> SelectByPinCode(SelectPincodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PinCode))
                return (Messages.Common.Message.InvalidRequest, null);

            try
            {
                var pinCode = await _pinCodeRepo.GetByPinCode(request.PinCode);

                if (pinCode == null)
                    return ("No record found for this Pincode.", null);

                var response = new PinCodeMasterResponse
                {
                    SR_NO = pinCode.SR_NO,
                    PINCODE = pinCode.PINCODE,
                    HPY_DISTRICT = pinCode.HPY_DISTRICT,
                    HYP_STATE = pinCode.HYP_STATE,
                    HPY_TALUKA_CITY = pinCode.HPY_TALUKA_CITY,
                    REGIONNAME = pinCode.REGIONNAME,
                    HPY_REGION = pinCode.HPY_REGION,
                    HPY_ZONE = pinCode.HPY_ZONE,
                    HPY_AREA = pinCode.HPY_AREA,
                    HPY_NSM = pinCode.HPY_NSM,
                    HPY_RH = pinCode.HPY_RH,
                    HPY_ZSM = pinCode.HPY_ZSM,
                    HPY_SH = pinCode.HPY_SH,
                    HPY_CM = pinCode.HPY_CM,
                    HPY_ASM = pinCode.HPY_ASM,
                    HPY_FLS = pinCode.HPY_FLS,
                    ACTIVATED = pinCode.ACTIVATED,
                    ACTIVATION_DATE = pinCode.ACTIVATION_DATE,
                    LOCATION = pinCode.LOCATION,
                    WITHIN_CITY_LIMITS = pinCode.WITHIN_CITY_LIMITS,
                    DISTANCE_FROM_CBD = pinCode.DISTANCE_FROM_CBD,
                    INDIAPOST_OFFICE_NAME = pinCode.INDIAPOST_OFFICE_NAME,
                    DIVISIONNAME = pinCode.DIVISIONNAME,
                    LONGITUDE = pinCode.LONGITUDE,
                    LATITUDE = pinCode.LATITUDE,
                };

                var wrapper = new AgentUserWrapper
                {
                    List = new List<PinCodeMasterResponse> { response },
                    Count = 1
                };

                return (StatusType.Ok, wrapper);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        private async Task<int> ProcessBBPS(List<IXLRow> rows)
        {
            int success = 0;
            var allUsers = await _userRepo.GetAllUsers();

            foreach (var row in rows)
            {
                try
                {
                    string agentRefId = row.Cell("A").GetString();
                    string npciAgentId = row.Cell("B").GetString();

                    if (string.IsNullOrWhiteSpace(agentRefId) || string.IsNullOrWhiteSpace(npciAgentId))
                        continue;

                    var user = allUsers.FirstOrDefault(u => u.AliasName != null &&
                        u.AliasName.Equals(agentRefId, StringComparison.OrdinalIgnoreCase));

                    if (user == null) continue;

                    user.NPCIAgentID = npciAgentId;
                    await _userRepo.UpdateUser(user);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Write(ex);
                }
            }

            return success;
        }

        private async Task<int> ProcessMicroATM(List<IXLRow> rows)
        {
            int success = 0;
            var allUsers = await _userRepo.GetAllUsers();
            int yesBankProviderID = 0;
            try
            {
                var setting = await _settingRepo.GetByKey("YesBankProviderID");
                if (setting != null && int.TryParse(setting.Value, out int pid))
                    yesBankProviderID = pid;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

            foreach (var row in rows)
            {
                try
                {
                    string apiKey = row.Cell("A").GetString();
                    string agentCode = row.Cell("B").GetString();
                    string yesBankTID = row.Cell("C").GetString();
                    string merchantID = row.Cell("D").GetString();

                    if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(agentCode))
                        continue;

                    var user = allUsers.FirstOrDefault(u => u.AliasName != null &&
                        u.AliasName.Equals(agentCode, StringComparison.OrdinalIgnoreCase));

                    if (user == null) continue;

                    if (yesBankProviderID > 0)
                    {
                        await _providerUserMappingRepo.Insert(new ProviderUserMapping
                        {
                            UserID = user.UserMasterID,
                            ProviderID = yesBankProviderID,
                            Key = "API Key",
                            Value = apiKey
                        });
                        await _providerUserMappingRepo.Insert(new ProviderUserMapping
                        {
                            UserID = user.UserMasterID,
                            ProviderID = yesBankProviderID,
                            Key = "Yes Bank TID",
                            Value = yesBankTID
                        });
                    }

                    user.PaySwiffMerchantId = merchantID;
                    await _userRepo.UpdateUser(user);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Write(ex);
                }
            }

            return success;
        }

        private async Task<int> ProcessPinMaster(List<IXLRow> rows)
        {
            int success = 0;

            foreach (var row in rows)
            {
                try
                {
                    string pinCode = row.Cell("A").GetString();
                    bool isDuplicate = await _pinCodeRepo.CheckPinCode(pinCode);

                    if (!isDuplicate) continue;

                    var entity = new LocationPinCodeMaster
                    {
                        PINCODE = pinCode,
                        HPY_DISTRICT = row.Cell("B").GetString(),
                        HYP_STATE = row.Cell("C").GetString(),
                        HPY_TALUKA_CITY = row.Cell("D").GetString(),
                        REGIONNAME = row.Cell("E").GetString(),
                        HPY_REGION = row.Cell("F").GetString(),
                        HPY_ZONE = row.Cell("G").GetString(),
                        HPY_AREA = row.Cell("H").GetString(),
                        HPY_NSM = row.Cell("I").GetString(),
                        HPY_RH = row.Cell("J").GetString(),
                        HPY_ZSM = row.Cell("K").GetString(),
                        HPY_SH = row.Cell("L").GetString(),
                        HPY_CM = row.Cell("M").GetString(),
                        HPY_ASM = row.Cell("N").GetString(),
                        HPY_FLS = row.Cell("O").GetString(),
                        ACTIVATED = row.Cell("P").GetString(),
                        ACTIVATION_DATE = row.Cell("Q").GetString(),
                        LOCATION = row.Cell("R").GetString(),
                        WITHIN_CITY_LIMITS = row.Cell("S").GetString(),
                        DISTANCE_FROM_CBD = row.Cell("T").GetString(),
                        INDIAPOST_OFFICE_NAME = row.Cell("U").GetString(),
                        DIVISIONNAME = row.Cell("V").GetString(),
                        LONGITUDE = row.Cell("W").GetString(),
                        LATITUDE = row.Cell("X").GetString(),
                        IsActive = true,
                        IsDelete = false
                    };

                    await _pinCodeRepo.Insert(entity);
                    success++;
                }
                catch (Exception ex)
                {
                    _logger.Write(ex);
                }
            }

            return success;
        }
    }
}
