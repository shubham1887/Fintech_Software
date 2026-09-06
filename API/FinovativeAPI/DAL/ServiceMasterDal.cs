using Project.Model;
using Project.Processing;
using Project.Requests.Service;
using Project.Requests.User;
using Project.Response.Service;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class ServiceMasterDal
    {
        internal void RemoveCache()
        {
            var cache = new MemoryCacheProcessing<List<ServiceMaster>>();
            cache.Remove(CacheKeys.ServiceMaster);
        }

        public int Insert(ServiceMaster ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceMaster.Add(ObjModel);
                context.SaveChanges();
                return ObjModel.ServiceID;
            }
        }

        internal int Update(ServiceMaster ObjModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceMaster.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.ServiceID;
            }
        }

        internal void  UpdateRange(List<ServiceMaster> objModel)
        {
            RemoveCache();
            using (var context = new DatabaseContext())
            {
                context.ServiceMaster.UpdateRange(objModel);
                context.SaveChanges();
                return;
            }
          
        }
        public List<ServiceMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<ServiceMaster>>();
            List<ServiceMaster> list = _list.GetOrCreate(CacheKeys.ServiceMaster, () => SelectWithoutCache());

            return list;
        }

        private List<ServiceMaster> SelectWithoutCache()
        {
            int[] ids = new int[] { (int)ServiceCode.FundTransfer,
                                     (int)ServiceCode.PrepaidActivation};
            List<ServiceMaster> list;
            using (var context = new DatabaseContext())
            {
                list = (from service in context.ServiceMaster
                        where !ids.Contains(service.ServiceID)
                        orderby (service.ServiceID == (int)ServiceCode.Mobile ? 1 :
                                 service.ServiceID == (int)ServiceCode.Dth ? 2 :
                                 service.ServiceID == (int)ServiceCode.PostPaid ? 3 :
                                 service.ServiceID == (int)ServiceCode.Electricity ? 4 :
                                 service.ServiceID == (int)ServiceCode.Fasttag ? 5 :
                                 service.ServiceID == (int)ServiceCode.PipedGas ? 6 :
                                 service.ServiceID == (int)ServiceCode.Waterbill ? 7 :
                                 service.ServiceID == (int)ServiceCode.LoanRePayment ? 8 :
                                 service.ServiceID == (int)ServiceCode.MoneyTransfer ? 9 :
                                 service.ServiceID == (int)ServiceCode.ExpressMoneyTransfer ? 10 :
                                 service.ServiceID == (int)ServiceCode.AEPS ? 11 :
                                 service.ServiceID == (int)ServiceCode.MATM ? 12 :
                                 service.ServiceID == (int)ServiceCode.LapuLoading ? 13 :
                                 service.ServiceID == (int)ServiceCode.Game ? 14 :
                                 service.ServiceID == (int)ServiceCode.Landline ? 15 :
                                 service.ServiceID == (int)ServiceCode.GeneralInsurance ? 16 :
                                 service.ServiceID == (int)ServiceCode.LifeInsurance ? 17 :
                                 service.ServiceID == (int)ServiceCode.CableTV ? 18 :
                                 service.ServiceID == (int)ServiceCode.Broadband ? 19 :
                                 service.ServiceID == (int)ServiceCode.GasCylinderBooking ? 20 : 21

                                 )
                        select service).ToList();
            }
            return list;
        }

        internal List<ViewModel> Select()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list
                       where service.IsDelete == false
                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           IsDisplay = service.IsDisplay,
                           IsDefault = service.IsDefault,
                           IsOutside = service.IsOutside,
                           IsSubscriptionAllow = service.IsSubscriptionAllow,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance,
                           ServiceCategory = service.ServiceCategory,
                       }).ToList();

            return retList;
        }

        internal List<ViewModel> SelectBBPSServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list
                       where service.IsDelete == false
                       && service.IsActive == true
                       && service.IsBBPS == true
                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsDisplay = service.IsDisplay,
                       }).ToList();

            return retList;
        }

        internal List<ViewModel> SelectByRechargeServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list

                       where service.ServiceID != (int)ServiceCode.MoneyTransfer
                            && service.ServiceID != (int)ServiceCode.ExpressMoneyTransfer
                            && service.ServiceID != (int)ServiceCode.AEPS
                            && service.ServiceID != (int)ServiceCode.Payout
                            && service.ServiceID != (int)ServiceCode.PaymentGateway
                             && service.ServiceID != (int)ServiceCode.DTHBooking
                             && service.ServiceID != (int)ServiceCode.PhoneBooking
                             && service.ServiceID != (int)ServiceCode.MATM
                             && service.ServiceID != (int)ServiceCode.AccountOpening
                             && service.ServiceID != (int)ServiceCode.POS
                             && service.ServiceID != (int)ServiceCode.UPITransfer
                             && service.ServiceID != (int)ServiceCode.AEPSOnboarding
                             && service.ServiceID != (int)ServiceCode.PanCard
                             && service.ServiceID != (int)ServiceCode.MATMOnboarding

                            && service.IsActive == true
                            && service.IsDelete == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        internal List<ViewModel> SelectByRechargeDTH()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list
                       where (service.ServiceID == (int)ServiceCode.Dth
                              || service.ServiceID == (int)ServiceCode.Mobile)
                             && service.IsActive == true
                             && service.IsDelete == false
                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }


        internal List<ViewModel> SelectByOfflineServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list
                       where (service.ServiceID == (int)ServiceCode.DTHBooking
                             || service.ServiceID == (int)ServiceCode.PhoneBooking)
                             && service.IsActive == true
                             && service.IsDelete == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        internal List<UserServiceMappingDetailList> SelectDefaultServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<UserServiceMappingDetailList> retList = null;

            retList = (from service in list
                       where service.IsDefault == true
                            && service.IsActive == true
                            && service.IsDelete == false
                       select new UserServiceMappingDetailList
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                       }).ToList();

            return retList;
        }

        internal List<UserServiceMappingDetailList> SelectOutsideServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<UserServiceMappingDetailList> retList = null;

            retList = (from service in list
                       where service.IsOutside == true
                            && service.IsDelete == false
                       select new UserServiceMappingDetailList
                       {
                           ServiceID = service.ServiceID, 
                           ServiceName = service.Name,
                       }).ToList();

            return retList;
        }

        internal List<ViewModel> SelectChargeServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list

                       where service.ServiceID == (int)ServiceCode.AEPS

                            && service.IsActive == true
                            && service.IsDelete == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        internal ServiceMaster SelectByID(int serviceID)
        {
            List<ServiceMaster> list = SelectAll();

            return list.FirstOrDefault(S => S.ServiceID == serviceID);
        }

        internal ServiceMaster SelectByServiceName(string ServiceName)
        {
            List<ServiceMaster> list = SelectAll();
            return (from service in list
                    where service.Name == ServiceName
                    select service).FirstOrDefault();
        }

        internal decimal SelectChargeByServiceID(int serviceID)
        {
            ServiceMaster sm = SelectByID(serviceID);
            if (sm == null)
            {
                return 0;
            }
            return sm.Charge;
        }

        internal List<ViewModel> SelectByServicesType(ServiceTypeRequest mainRequest)
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            if (mainRequest.ServiceType == ServiceType.Recharge)
            {
                retList = (from service in list

                           where service.ServiceID != (int)ServiceCode.MoneyTransfer
                            && service.ServiceID != (int)ServiceCode.ExpressMoneyTransfer
                            && service.ServiceID != (int)ServiceCode.AEPS
                            && service.ServiceID != (int)ServiceCode.Payout
                            && service.ServiceID != (int)ServiceCode.VendorPay
                            && service.ServiceID != (int)ServiceCode.PaymentGateway
                             && service.ServiceID != (int)ServiceCode.DTHBooking
                             && service.ServiceID != (int)ServiceCode.PhoneBooking
                             && service.ServiceID != (int)ServiceCode.FRC
                             && service.ServiceID != (int)ServiceCode.Electricity
                              && service.ServiceID != (int)ServiceCode.Mobile
                              && service.ServiceID != (int)ServiceCode.MATM
                              && service.ServiceID != (int)ServiceCode.UPITransfer
                              && service.ServiceID != (int)ServiceCode.AccountOpening
                              && service.ServiceID != (int)ServiceCode.AEPSOnboarding
                              && service.ServiceID != (int)ServiceCode.MATMOnboarding
                              && service.ServiceID != (int)ServiceCode.POS
                              && service.ServiceID != (int)ServiceCode.CMS
                              && service.ServiceID != (int)ServiceCode.Game
                             && service.IsActive == true
                             && service.IsDelete == false
                           select new ViewModel
                           {
                               ServiceID = service.ServiceID,
                               ServiceName = service.Name,
                               IsActive = service.IsActive,
                               Charge = service.Charge,
                               MinBalance = service.MinBalance
                           }).ToList();
            }
            if (mainRequest.ServiceType == ServiceType.DMT)
            {
                retList = (from service in list
                           where service.IsActive == true
                            && service.IsDelete == false
                           orderby (service.ServiceID == (int)ServiceCode.Mobile ? 1 :
                                                      service.ServiceID == (int)ServiceCode.MoneyTransfer ? 2 :
                                                       service.ServiceID == (int)ServiceCode.ExpressMoneyTransfer ? 3 :
                                                      service.ServiceID == (int)ServiceCode.Electricity ? 4 :
                                                      service.ServiceID == (int)ServiceCode.AEPS ? 5 :
                                                      service.ServiceID == (int)ServiceCode.Payout ? 6 :
                                                      service.ServiceID == (int)ServiceCode.PaymentGateway ? 7 :
                                                      service.ServiceID == (int)ServiceCode.MATM ? 8 :
                                                      service.ServiceID == (int)ServiceCode.UPITransfer ? 9 :
                                                        service.ServiceID == (int)ServiceCode.VendorPay ? 10 :
                                                      service.ServiceID == (int)ServiceCode.AEPSOnboarding ? 11 :
                                                      service.ServiceID == (int)ServiceCode.MATMOnboarding ? 12 : 13
                                                      )
                           select new ViewModel
                           {
                               ServiceID = service.ServiceID,
                               ServiceName = service.Name,
                               IsActive = service.IsActive,
                               Charge = service.Charge,
                               MinBalance = service.MinBalance
                           }).ToList();
            }

            return retList;
        }

        internal List<ViewModel> SelectPrePaidDTHFRCService()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list

                       where service.ServiceID == (int)ServiceCode.Mobile
                        || service.ServiceID == (int)ServiceCode.Dth
                        || service.ServiceID == (int)ServiceCode.FRC
                         && service.IsActive == true
                         && service.IsDelete == false
                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        internal decimal SelectMinBalanceByServiceID(int serviceID)
        {
            ServiceMaster sm = SelectByID(serviceID);
            if (sm == null)
            {
                return 0;
            }
            return sm.MinBalance;
        }

        internal List<ViewModel> SelectByPrimeServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list

                       where (service.ServiceID == (int)ServiceCode.Mobile
                            || service.ServiceID == (int)ServiceCode.Dth
                            || service.ServiceID == (int)ServiceCode.PostPaid
                            || service.ServiceID == (int)ServiceCode.Electricity
                            || service.ServiceID == (int)ServiceCode.LifeInsurance
                            || service.ServiceID == (int)ServiceCode.GasCylinderBooking
                            || service.ServiceID == (int)ServiceCode.PipedGas
                            || service.ServiceID == (int)ServiceCode.Fasttag
                            || service.ServiceID == (int)ServiceCode.Waterbill
                            || service.ServiceID == (int)ServiceCode.LoanRePayment)
                            && service.IsActive == true
                            && service.IsDelete == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        internal List<ViewModel> SelectForCallBackUrl()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list

                       where service.IsActive == true
                            && service.IsDelete == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        public string SelectServiceNameByServiceID(int ServiceID)
        {
            List<ServiceMaster> list = SelectAll();
            ServiceMaster service = (from item in list
                                     where item.ServiceID == ServiceID
                                     select item).FirstOrDefault();
            if (service == null)
                return "";

            return service.Name;
        }

        internal List<ViewModel> SelectSubscriptionAllowedServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list
                       where
                               service.IsActive == true
                             && service.IsDelete == false
                             && service.IsSubscriptionAllow == true

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

        internal List<ViewModel> SelectRangeWiseServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list

                       where (service.ServiceID == (int)ServiceCode.MoneyTransfer
                            || service.ServiceID == (int)ServiceCode.VendorPay
                            )
                            && service.IsActive == true
                            && service.IsDelete == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }
        internal List<ViewModel> SelectServiceMoneyTransferAndUPITransfer()
        {
            List<ServiceMaster> list = SelectAll();
            return (from item in list
                    where item.ServiceID == (int)ServiceCode.MoneyTransfer || item.ServiceID == (int)ServiceCode.UPITransfer &&
                     item.IsDelete == false
                    select new ViewModel
                    {
                        ServiceID = item.ServiceID,
                        ServiceName = item.Name,
                    }).ToList();
        }

        internal List<ViewModel> SelectSubscriptionServices()
        {
            List<ServiceMaster> list = SelectAll();
            List<ViewModel> retList = null;

            retList = (from service in list
                       where
                               service.IsActive == true
                             && service.IsDelete == false
                             && service.IsSubscriptionAllow == true
                             && service.IsBBPS == false

                       select new ViewModel
                       {
                           ServiceID = service.ServiceID,
                           ServiceName = service.Name,
                           IsActive = service.IsActive,
                           Charge = service.Charge,
                           MinBalance = service.MinBalance
                       }).ToList();

            return retList;
        }

    }
}