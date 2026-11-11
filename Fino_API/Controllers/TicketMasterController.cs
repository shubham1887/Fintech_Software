using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TicketMaster;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TicketMasterController : ControllerBase
    {
        private readonly ITicketMasterService _ticketMasterService;
        private readonly IAppNotificationService _notifications;
        private readonly IUserRepository _userRepository;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public TicketMasterController(
            ITicketMasterService ticketMasterService,
            IAppNotificationService notifications,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _ticketMasterService = ticketMasterService;
            _notifications = notifications;
            _userRepository = userRepository;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        // Admin and support run the queue: they see every ticket and are the only ones who can
        // resolve one. Everybody else sees — and edits — only the tickets they raised themselves.
        // Before this gate, GetAll honoured whatever UserID the caller sent, so any signed-in
        // retailer could read every other merchant's ticket subject and body by sending 0.
        private static bool RunsTheQueue(KeyDataModel k) =>
            k.UserTypeID == StaticModels.UserTypeCode.Admin || k.UserTypeID == StaticModels.UserTypeCode.SupportUser;

        /// <summary>True when this caller is allowed to see/touch the given ticket.</summary>
        private static bool CanTouch(KeyDataModel k, Responses.TicketMaster.TicketMasterResponse t) =>
            RunsTheQueue(k) || t.UserID == k.UserID;

        /// <summary>
        /// Alert the admins and support agents that a new ticket is waiting. Best effort: a
        /// notification failure must never lose the ticket the user just raised.
        /// </summary>
        private async Task NotifyQueue(string subject, int raisedByUserId)
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                var queue = users
                    .Where(u => !u.IsDeleted && u.IsActive
                                && (u.UserTypeID == StaticModels.UserTypeCode.Admin
                                    || u.UserTypeID == StaticModels.UserTypeCode.SupportUser))
                    .Select(u => u.UserMasterID)
                    .Where(id => id != raisedByUserId)
                    .ToList();

                if (queue.Count == 0) return;

                await _notifications.NotifyUsers(
                    queue, "New support ticket", subject, StaticModels.NotificationKind.Ticket, "/s/TicketReport");
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetTicketMasterRequest? getRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                getRequest = JsonConvert.DeserializeObject<GetTicketMasterRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var ticket = await _ticketMasterService.GetTicketMasterById(getRequest.TicketMasterID);

                if (ticket == null)
                    return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.TicketNotFound);
                if (!CanTouch(keyDataModel, ticket))
                    return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.TicketNotFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(ticket));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetTicketMasterRequest? getRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                getRequest = JsonConvert.DeserializeObject<GetTicketMasterRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                // Scope first, then filter: a non-admin's request is pinned to their own UserID
                // whatever they asked for.
                if (!RunsTheQueue(keyDataModel)) getRequest.UserID = keyDataModel.UserID;

                var tickets = await _ticketMasterService.GetTicketMasters(getRequest);

                if (tickets == null || tickets.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(tickets));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            CreateTicketMasterRequest? createRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                createRequest = JsonConvert.DeserializeObject<CreateTicketMasterRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || createRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                // A ticket is raised BY the signed-in user — the client does not get to say whose.
                createRequest.UserID = keyDataModel.UserID;
                createRequest.UserTypeID = keyDataModel.UserTypeID;

                failureMessage = await _ticketMasterService.CreateTicketMaster(createRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                // Tell the people who work the queue. Without this a ticket sits unseen until
                // somebody happens to open the screen.
                await NotifyQueue(createRequest.Subject, keyDataModel.UserID);

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketCreatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateTicketMasterRequest? updateRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateTicketMasterRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || updateRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var existing = await _ticketMasterService.GetTicketMasterById(updateRequest.TicketMasterID);
                if (existing == null) return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.TicketNotFound);
                if (!CanTouch(keyDataModel, existing))
                    return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.NotYourTicket);
                // Status and resolution belong to the queue, not to the requester — they go
                // through Resolve, which stamps who closed the ticket and when.
                if (!RunsTheQueue(keyDataModel) &&
                    (updateRequest.FieldName.EqualsIgnoreCase("status")
                     || updateRequest.FieldName.EqualsIgnoreCase("resolution")
                     || updateRequest.FieldName.EqualsIgnoreCase("resolutionbyuserid")))
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

                failureMessage = await _ticketMasterService.UpdateTicketMaster(updateRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            DeleteTicketMasterRequest? deleteRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<DeleteTicketMasterRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || deleteRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var target = await _ticketMasterService.GetTicketMasterById(deleteRequest.TicketMasterID);
                if (target == null) return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.TicketNotFound);
                if (!CanTouch(keyDataModel, target))
                    return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.NotYourTicket);

                failureMessage = await _ticketMasterService.DeleteTicketMaster(deleteRequest.TicketMasterID);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketDeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        /// <summary>
        /// Work a ticket: set its status and write the resolution note in one step. The field-level
        /// Update endpoint could do neither together, so a ticket could be marked Resolved with an
        /// empty note, or carry a note while still sitting Open in the queue.
        /// </summary>
        [HttpPost("Resolve")]
        public async Task<APIResponse> Resolve([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            ResolveTicketMasterRequest? request;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<ResolveTicketMasterRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            if (!RunsTheQueue(keyDataModel))
                return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                // Read the ticket BEFORE resolving so we still know who raised it.
                var ticket = await _ticketMasterService.GetTicketMasterById(request.TicketMasterID);

                failureMessage = await _ticketMasterService.ResolveTicketMaster(request, keyDataModel.UserID);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                if (ticket != null && ticket.UserID > 0 && ticket.UserID != keyDataModel.UserID)
                {
                    await _notifications.NotifyUsers(
                        new[] { ticket.UserID },
                        $"Ticket #{request.TicketMasterID} is {request.Status}",
                        request.Resolution,
                        StaticModels.NotificationKind.Ticket,
                        "/s/TicketReport");
                }

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketResolvedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}