using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SliderMaster;
using FintechApp.Responses.SliderMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SliderMasterService : ISliderMasterService
    {
        private readonly ISliderMasterRepository _repository;

        public SliderMasterService(ISliderMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SliderMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<SliderMasterResponse?> GetByID(int sliderMasterId)
        {
            var entity = await _repository.GetByID(sliderMasterId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateSliderMasterRequest request)
        {
            var entity = new SliderMaster
            {
                SliderPath = request.SliderPath,
                Description = request.Description,
                Type = request.Type,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateSliderMasterRequest request)
        {
            var entity = await _repository.GetByID(request.SliderMasterID);
            if (entity == null) return Messages.SliderMasterMessages.NotFound;

            entity.SliderPath = request.SliderPath;
            entity.Description = request.Description;
            entity.Type = request.Type;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int sliderMasterId)
        {
            var entity = await _repository.GetByID(sliderMasterId);
            if (entity == null) return Messages.SliderMasterMessages.NotFound;
            int result = await _repository.Delete(sliderMasterId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static SliderMasterResponse MapToResponse(SliderMaster e) => new()
        {
            SliderMasterID = e.SliderMasterID,
            SliderPath = e.SliderPath,
            Description = e.Description,
            Type = e.Type,
            DateTime = e.DateTime
        };
    }
}
