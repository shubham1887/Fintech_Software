using AutoMapper;
using Project.Model;
using Project.StaticModel;
using System;

namespace Project.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Map from DBModel Object to ViewModel Object
            CreateMap<DMTSetting, Response.DMTSettings.ViewModel>();
            CreateMap<DematSetting, Response.DematSetting.ViewModel>();

            CreateMap<PaymentGatewaySetting, Response.PaymentGateway.ViewModel>();

            CreateMap<AEPSSetting, Response.AEPSSetting.ViewModel>();

            CreateMap<CyberPlateSetting, Response.CyberPlate.ViewModel>();

            CreateMap<MenuMaster, Response.MenuMaster.ViewModel>();

            CreateMap<SliderMaster, Response.SliderMaster.ViewModel>()
                .ForMember(x => x.ImagePath, s => s.MapFrom(s => String.Concat(Company.WebAPIURL, s.SliderPath)));

            CreateMap<InvoiceSetting, Response.InvoiceSetting.ViewModel>().ForMember(x => x.LogoURL, s => s.MapFrom(s => String.Concat(Company.WebAPIURL, s.LogoURL)));

            CreateMap<ThirdPartyServiceProvider, Response.ThirdPartyServiceProvider.ViewModel>();

            CreateMap<UISettingMaster, Response.UISetting.ViewModel>();

            CreateMap<CompanyMaster, Response.Company.ViewModel>();

            CreateMap<PlanSetting, Response.PlanSetting.ViewModel>();

            CreateMap<EcollectionSetting, Response.EcollectionSetting.ViewModel>();

            CreateMap<PayoutSetting, Response.PayoutSetting.ViewModel>();

            CreateMap<CreditCardSetting, Response.CreditCardSetting.ViewModel>();

            CreateMap<CashDepositSetting, Response.CashDepositSetting.ViewModel>();

            CreateMap<BulkMessage, Response.BulkMessage.ViewModel>();

            CreateMap<UPITransferSetting, Response.UPITransferSetting.ViewModel>();

            CreateMap<eKYCSetting, Response.eKYCSetting.ViewModel>();

            CreateMap<AccountOpenSetting, Response.AccountOpenSetting.ViewModel>();

            CreateMap<PanCardSetting, Response.PanCardSetting.ViewModel>();

            CreateMap<LeadGenerationSetting, Response.LeadGenerationSetting.ViewModel>();

            CreateMap<CMSSetting, Response.CMSSetting.ViewModel>();

            CreateMap<BulkPaySetting, Response.BulkPaySetting.ViewModel>();

            CreateMap<OTTSetting, Response.OTTSetting.ViewModel>();
        }
    }
}