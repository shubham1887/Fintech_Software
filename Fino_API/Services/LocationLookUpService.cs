using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LocationLookUp;
using FintechApp.Responses.LocationLookUp;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class LocationLookUpService : ILocationLookUpService
    {
        private readonly ILocationLookUpRepository _repository;
        public LocationLookUpService(ILocationLookUpRepository repository) { _repository = repository; }

        private static LocationLookUpResponse MapToResponse(LocationLookUp e) => new()
        {
            LocationLookUpID = e.LocationLookUpID,
            TableID = e.TableID,
            SimCardDetails = e.SimCardDetails,
            Type = e.Type,
            Location = e.Location,
            IPAddress = e.IPAddress,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
        };

        public async Task<List<LocationLookUpResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<LocationLookUpResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateLocationLookUpRequest request)
        {
            var entity = new LocationLookUp
            {
                TableID = request.TableID,
                SimCardDetails = request.SimCardDetails,
                Type = request.Type,
                Location = request.Location,
                IPAddress = request.IPAddress,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateLocationLookUpRequest request)
        {
            var entity = await _repository.GetByID(request.LocationLookUpID);
            if (entity == null) return Messages.LocationLookUpMessages.NotFound;
            entity.TableID = request.TableID;
            entity.SimCardDetails = request.SimCardDetails;
            entity.Type = request.Type;
            entity.Location = request.Location;
            entity.IPAddress = request.IPAddress;
            entity.Latitude = request.Latitude;
            entity.Longitude = request.Longitude;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.LocationLookUpMessages.NotFound;
        }
    }
}
