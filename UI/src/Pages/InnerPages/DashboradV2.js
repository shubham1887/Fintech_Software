import { Grid, Container, Box, Button } from '@material-ui/core';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Formik, Form } from 'formik';

import { LOGOUT } from 'ConstComponents';
import DashboardQuickLinks from 'Pages/InnerPages/DashBoard/DashQuicklinks/DashboardQuicklinks';
import ServiceSummary from 'Pages/InnerPages/DashBoard/DashServiceSummary/ServiceSummary';
import TodaysSummary from 'Pages/InnerPages/DashBoard/TodaysSummary';
import React, { useEffect, useState } from 'react';
import { UserLowBalance } from 'Pages/InnerPages/UserwiseLowBalance/LowBalance';
import CircularProgress from '@material-ui/core/CircularProgress';
import { CheckAllProviderBalance as PROVIDER_CHECK_ALL_BALANCE } from 'Components/Api/ProviderApi';
import { Select as SELECT_DASHBOARD_DATA } from 'Components/Api/DashboardApi';
import { selectUserSummary as SELECT_DASHBOARD_USERSUMMARY } from 'Components/Api/UsersApi';
import { SelectSale as SELECT_SERVICE_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import { Select as SELECT_DASHBOARD_TODAYSUMMARY } from 'Components/Api/TodaySummaryApi';
import { SelectProviderSummary as SELECT_PROVIDER_SUMMARY } from 'Components/Api/ProviderPurchaseAPI';
import DashBoardUserSummary from 'Pages/InnerPages/DashBoard/DashBoardUserSummary';
import DashBoardAdminTransactionDetails from 'Pages/InnerPages/DashBoard/DashBoardAdminTransactionDetails';
import TodaysTransactions from 'Pages/InnerPages/DashBoard/TodaysTransactions';
import ProvidersSummary from 'Pages/InnerPages/DashBoard/ProvidersSummary';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import MonthlySummaryGraph from 'Pages/InnerPages/DashBoard/MonthlySummaryGraph';

import { useHistory } from 'react-router';

import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { TODAYSUMMARYONTOP, PROVIDERSSUMMARYONTOP } from 'Config';
import { SelectLowBalance as SELECT_LOW_BALANCE } from 'Components/Api/UserwiseLowBalanceAPI';
import HourlyLineGraph from './DashBoard/HourlyLineGraph';
const CustomContainer = HOCLoader(Container);

export const AdminDashboardV2 = () => {
                                          const classes = commonStyle();
                                          const history = useHistory();
                                          const [
                                              ProviderSummraylist,
                                              setProviderSummraylist
                                          ] = useState([]);
                                          const [
                                              isError,
                                              setIsError
                                          ] = useState(false);
                                          const [
                                              severity,
                                              setSeverity
                                          ] = useState('success');
                                          const [
                                              responseMessage,
                                              setResponseMessage
                                          ] = useState('');
                                          const [
                                              loadingOpen,
                                              setLoadingOpen
                                          ] = useState(false);
                                          const [
                                              UserSummraylist,
                                              setUserSummraylist
                                          ] = useState([]);

                                          const [
                                              lowBalance,
                                              setLowBalance
                                          ] = useState(false);
                                          const [
                                              LowBalanceMessage,
                                              setLowBalanceMessage
                                          ] = useState('');
                                          const [
                                              TodaySummarylist,
                                              setTodaySummarylist
                                          ] = useState([]);
                                          const [
                                              ServicewiseSale,
                                              setServicewiseSale
                                          ] = useState([]);
                                          const [
                                              modalOpen,
                                              setModalOpen
                                          ] = useState(false);
                                          const [
                                              isTodaySummaryOnTop,
                                              setTodaySummaryOnTop
                                          ] = useState(false);
                                          const [
                                              isTodaySummaryOnBottom,
                                              setTodaySummaryOnBottom
                                          ] = useState(true);
                                          const [
                                              isProvidersSummaryOnTop,
                                              setProvidersSummaryOnTop
                                          ] = useState(false);
                                          const [
                                              isProvidersSummaryOnBottom,
                                              setProvidersSummaryOnBottom
                                          ] = useState(true);
                                          const [
                                              isMonthlyGraphOnTop,
                                              setMonthlyGraphOnTop
                                          ] = useState(true);
                                          const [
                                              isHourlyGraphOnTop,
                                              setHourlyGraphOnTop
                                          ] = useState(true);
                                          const [
                                              isMonthlyGraphOnBottom,
                                              setMonthlyGraphOnBottom
                                          ] = useState(false);

                                          var IsProviderPerformanceGraph =
                                              localStorage.getItem(
                                                  'IsProviderPerformanceGraph'
                                              ) === 'true';
                                          const fetchTodaySummaryOnTop = async () => {
                                              let isTop = await TODAYSUMMARYONTOP();
                                              if (isTop === 'YES') {
                                                  setTodaySummaryOnTop(true);
                                                  setTodaySummaryOnBottom(
                                                      false
                                                  );
                                                  setMonthlyGraphOnTop(false);
                                                  setHourlyGraphOnTop(false);
                                                  setMonthlyGraphOnBottom(true);
                                                  return;
                                              }
                                          };

                                          const fetchProvidersSummaryOnTop = async () => {
                                              let isTop = await PROVIDERSSUMMARYONTOP();
                                              if (isTop === 'YES') {
                                                  setProvidersSummaryOnTop(
                                                      true
                                                  );
                                                  setProvidersSummaryOnBottom(
                                                      false
                                                  );
                                                  return;
                                              }
                                          };

                                          const [
                                              formObject,
                                              setFormObject
                                          ] = useState({
                                              TotalSuccessAmount: 0,
                                              TotalFailedAmount: 0,
                                              TransferAmount: 0,
                                              TransferCount: 0,
                                              ComplaintAmount: 0,
                                              ComplaintCount: 0,
                                              KycCount: 0,
                                              TotalTarget: 0,
                                              RemainsTarget: 0,
                                              TotalAdminCommission: 0,
                                              TotalAdminSurcharge: 0,
                                              AdminCommissionProfit: 0,
                                              AdminSurchargeProfit: 0,

                                              PrePaidSuccess: 0,
                                              PrePaidFail: 0,
                                              DthSuccess: 0,
                                              DthFail: 0,
                                              PostPaidSuccess: 0,
                                              PostPaidFail: 0,
                                              DMTSuccess: 0,
                                              DMTFail: 0,
                                              AEPSSuccess: 0,
                                              AEPSFail: 0,
                                              RofferComm: 0,
                                              RofferTotal: 0,
                                              RofferPer: 0
                                          });
                                          const [ModelObj] = useState({
                                              Date: new Date()
                                          });
                                          const selectLowBalance = async () => {
                                              setLowBalance(false);
                                              setLowBalanceMessage('');

                                              const response = await SELECT_LOW_BALANCE();
                                              setLoadingOpen(false);

                                              if (!response.isAuthorized)
                                                  LOGOUT();

                                              if (!response.status) {
                                                  // setIsError(true);
                                                  // setResponseMessage(response.data);
                                                  // setSeverity('error');
                                                  return;
                                              }

                                              let data = JSON.parse(
                                                  response.data
                                              );

                                              if (data.Status === 'Ok') {
                                                  setLowBalance(false);
                                                  setLowBalanceMessage('');
                                                  return;
                                              }
                                              setLowBalance(true);
                                              setLowBalanceMessage(
                                                  data.Message
                                              );
                                          };
                                          const selectTodaySummary = async () => {
                                              var inputObj = {
                                                  Date: new Date()
                                              };
                                              var requestObj = JSON.stringify(
                                                  inputObj
                                              );
                                              const response = await SELECT_DASHBOARD_TODAYSUMMARY(
                                                  requestObj
                                              );
                                              setLoadingOpen(false);

                                              if (!response.isAuthorized)
                                                  LOGOUT();

                                              if (!response.status) {
                                                  setIsError(true);
                                                  setSeverity('error');
                                                  setResponseMessage(
                                                      response.data
                                                  );

                                                  return;
                                              }

                                              var data = JSON.parse(
                                                  response.data
                                              );
                                              if (
                                                  data !== null ||
                                                  data !== undefined
                                              )
                                                  setTodaySummarylist(data);
                                              setLoadingOpen(false);
                                          };

                                          const selectServicewiseSale = async () => {
                                              let inputObj = {
                                                  FromDate: new Date(),
                                                  ToDate: new Date(),
                                                  UserID: 0
                                              };
                                              setServicewiseSale([]);

                                              let requestObj = JSON.stringify(
                                                  inputObj
                                              );
                                              const response = await SELECT_SERVICE_WISE_SALE(
                                                  requestObj
                                              );

                                              setLoadingOpen(false);

                                              if (!response.isAuthorized)
                                                  LOGOUT();
                                              if (!response.status) {
                                                  return;
                                              }

                                              var data = JSON.parse(
                                                  response.data
                                              );

                                              if (
                                                  data.list !== null ||
                                                  data.list !== undefined
                                              )
                                                  setServicewiseSale(data.list);
                                              setLoadingOpen(false);
                                          };

                                          const selectUserSummary = async () => {
                                              var inputObj = {
                                                  Date: new Date()
                                              };
                                              var requestObj = JSON.stringify(
                                                  inputObj
                                              );
                                              setUserSummraylist([]);
                                              const response = await SELECT_DASHBOARD_USERSUMMARY(
                                                  requestObj
                                              );

                                              if (!response.isAuthorized)
                                                  LOGOUT();

                                              if (!response.status) {
                                                  setIsError(true);
                                                  setResponseMessage(
                                                      response.data
                                                  );
                                                  setSeverity('error');
                                                  return;
                                              }
                                              var data = JSON.parse(
                                                  response.data
                                              );
                                              if (
                                                  data !== null ||
                                                  data !== undefined
                                              )
                                                  setUserSummraylist(data);
                                          };
                                          const submithandle = async (
                                              values,
                                              { setSubmitting, resetForm }
                                          ) => {
                                              if (values.Date == null) return;

                                              var inputObj = {
                                                  Date: values.Date,
                                                  FromDate: values.Date,
                                                  ToDate: values.Date,
                                                  UserID: 0
                                              };

                                              var requestObj = JSON.stringify(
                                                  inputObj
                                              );
                                              setTodaySummarylist([]);
                                              setServicewiseSale([]);
                                              setUserSummraylist([]);
                                              setProviderSummraylist([]);

                                              setLoadingOpen(true);

                                              const [
                                                  response,
                                                  responseService,
                                                  responseUser,
                                                  responseProvider,
                                                  responseCard
                                              ] = await Promise.all([
                                                  SELECT_DASHBOARD_TODAYSUMMARY(
                                                      requestObj
                                                  ),
                                                  SELECT_SERVICE_WISE_SALE(
                                                      requestObj
                                                  ),
                                                  SELECT_DASHBOARD_USERSUMMARY(
                                                      requestObj
                                                  ),
                                                  SELECT_PROVIDER_SUMMARY(
                                                      requestObj
                                                  ),
                                                  SELECT_DASHBOARD_DATA(
                                                      requestObj
                                                  )
                                              ]);

                                              //const response = await SELECT_DASHBOARD_TODAYSUMMARY(requestObj);
                                              var data = JSON.parse(
                                                  response.data
                                              );
                                              if (
                                                  data !== null ||
                                                  data !== undefined
                                              )
                                                  setTodaySummarylist(data);

                                              //const responseService = await SELECT_SERVICE_WISE_SALE(requestObj);
                                              var dataService = JSON.parse(
                                                  responseService.data
                                              );
                                              if (
                                                  dataService.list !== null ||
                                                  dataService.list !== undefined
                                              )
                                                  setServicewiseSale(
                                                      dataService.list
                                                  );

                                              // const responseUser = await SELECT_DASHBOARD_USERSUMMARY(requestObj);
                                              var dataUser = JSON.parse(
                                                  responseUser.data
                                              );
                                              if (
                                                  dataUser !== null ||
                                                  dataUser !== undefined
                                              )
                                                  setUserSummraylist(dataUser);

                                              //const responseProvider = await SELECT_PROVIDER_SUMMARY(requestObj);
                                              var dataProvider = JSON.parse(
                                                  responseProvider.data
                                              );
                                              if (
                                                  dataProvider !== null ||
                                                  dataProvider !== undefined
                                              )
                                                  setProviderSummraylist(
                                                      dataProvider
                                                  );

                                              //const responseCard = await SELECT_DASHBOARD_DATA(requestObj);

                                              var dataCard = JSON.parse(
                                                  responseCard.data
                                              );
                                              setFormObject({
                                                  TotalSuccessAmount:
                                                      dataCard.TotalSuccessAmount,
                                                  TotalFailedAmount:
                                                      dataCard.TotalFailedAmount,
                                                  TransferAmount:
                                                      dataCard.TransferAmount,
                                                  TransferCount:
                                                      dataCard.TransferCount,
                                                  ComplaintAmount:
                                                      dataCard.ComplaintAmount,
                                                  ComplaintCount:
                                                      dataCard.ComplaintCount,
                                                  KycCount: dataCard.KycCount,
                                                  TotalTarget:
                                                      dataCard.TotalTarget,
                                                  RemainsTarget:
                                                      dataCard.RemainsTarget,
                                                  TotalAdminCommission:
                                                      dataCard.TotalAdminCommission,
                                                  TotalAdminSurcharge:
                                                      dataCard.TotalAdminSurcharge,
                                                  AdminCommissionProfit:
                                                      dataCard.AdminCommissionProfit,
                                                  AdminSurchargeProfit:
                                                      dataCard.AdminSurchargeProfit,
                                                  RofferComm:
                                                      dataCard.RofferComm,
                                                  RofferTotal:
                                                      dataCard.RofferTotal,
                                                  RofferPer: dataCard.RofferPer
                                              });
                                              setLoadingOpen(false);
                                              setSubmitting(false);
                                          };
                                          const CheckAllBalance = async () => {
                                              setLoadingOpen(true);
                                              setIsError(false);
                                              const response = await PROVIDER_CHECK_ALL_BALANCE();
                                              setLoadingOpen(false);
                                              if (!response.isAuthorized)
                                                  LOGOUT();
                                              setSeverity('success');
                                              setResponseMessage(response.data);

                                              if (!response.status)
                                                  setSeverity('error');
                                              setIsError(true);
                                              selectProviderSummary();
                                          };

                                          const selectProviderSummary = async () => {
                                              var inputObj = {
                                                  Date: new Date()
                                              };
                                              var requestObj = JSON.stringify(
                                                  inputObj
                                              );
                                              setProviderSummraylist([]);
                                              const response = await SELECT_PROVIDER_SUMMARY(
                                                  requestObj
                                              );

                                              if (!response.isAuthorized)
                                                  LOGOUT();

                                              if (!response.status) {
                                                  setIsError(true);
                                                  setResponseMessage(
                                                      response.data
                                                  );
                                                  setSeverity('error');
                                                  return;
                                              }
                                              var data = JSON.parse(
                                                  response.data
                                              );
                                              if (
                                                  data !== null ||
                                                  data !== undefined
                                              )
                                                  setProviderSummraylist(data);
                                          };

                                          const select = async () => {
                                              var inputObj = {
                                                  Date: new Date()
                                              };
                                              var requestObj = JSON.stringify(
                                                  inputObj
                                              );
                                              const response = await SELECT_DASHBOARD_DATA(
                                                  requestObj
                                              );
                                              setLoadingOpen(false);

                                              if (!response.isAuthorized)
                                                  LOGOUT();

                                              if (!response.status) {
                                                  setIsError(true);
                                                  setSeverity('error');
                                                  setResponseMessage(
                                                      response.data
                                                  );

                                                  return;
                                              }

                                              var data = JSON.parse(
                                                  response.data
                                              );
                                              setFormObject({
                                                  TotalSuccessAmount:
                                                      data.TotalSuccessAmount,
                                                  TotalFailedAmount:
                                                      data.TotalFailedAmount,
                                                  TransferAmount:
                                                      data.TransferAmount,
                                                  TransferCount:
                                                      data.TransferCount,
                                                  ComplaintAmount:
                                                      data.ComplaintAmount,
                                                  ComplaintCount:
                                                      data.ComplaintCount,
                                                  KycCount: data.KycCount,
                                                  TotalTarget: data.TotalTarget,
                                                  RemainsTarget:
                                                      data.RemainsTarget,
                                                  TotalAdminCommission:
                                                      data.TotalAdminCommission,
                                                  TotalAdminSurcharge:
                                                      data.TotalAdminSurcharge,

                                                  PrePaidSuccess:
                                                      data.PrePaidSuccess,
                                                  PrePaidFail: data.PrePaidFail,
                                                  DthSuccess: data.DthSuccess,
                                                  DthFail: data.DthFail,
                                                  PostPaidSuccess:
                                                      data.PostPaidSuccess,
                                                  PostPaidFail:
                                                      data.PostPaidFail,
                                                  DMTSuccess: data.DMTSuccess,
                                                  DMTFail: data.DMTFail,
                                                  AEPSSuccess: data.AEPSSuccess,
                                                  AEPSFail: data.AEPSFail,
                                                  AdminCommissionProfit:
                                                      data.AdminCommissionProfit,
                                                  AdminSurchargeProfit:
                                                      data.AdminSurchargeProfit,
                                                  RofferComm: data.RofferComm,
                                                  RofferTotal: data.RofferTotal,
                                                  RofferPer: data.RofferPer
                                              });
                                          };

                                          const handleDismissCallback = () => {
                                              setModalOpen(false);
                                          };

                                          const currentDate = new Date();
                                          const minDate = new Date();
                                          minDate.setMonth(
                                              currentDate.getMonth() - 3
                                          );

                                          useEffect(() => {
                                              setLoadingOpen(true);
                                              selectLowBalance();
                                              fetchTodaySummaryOnTop();
                                              fetchProvidersSummaryOnTop();
                                              selectTodaySummary();
                                              selectServicewiseSale();
                                              selectUserSummary();
                                              select();

                                              selectProviderSummary();
                                              setLoadingOpen(false);
                                          }, []);

                                          const handleCustomContainerCloseCallback = (
                                              alertState
                                          ) => {
                                              setIsError(alertState);
                                          };

                                          const TodaysSummaryHideShow = () => {
                                              return (
                                                  <Grid
                                                      container
                                                      className="mb-5"
                                                  >
                                                      <Grid
                                                          item
                                                          xs={12}
                                                          sm={12}
                                                          md={4}
                                                          lg={4}
                                                          xl={4}
                                                          className="mb-5"
                                                      >
                                                          <TodaysSummary
                                                              data={
                                                                  TodaySummarylist
                                                              }
                                                          />
                                                      </Grid>
                                                      <Grid
                                                          item
                                                          xs={12}
                                                          sm={12}
                                                          md={8}
                                                          lg={8}
                                                          xl={8}
                                                          className="mb-5"
                                                      >
                                                          <DashboardQuickLinks />
                                                      </Grid>
                                                  </Grid>
                                              );
                                          };

                                          const HourlyLineGraphShow = () => {
                                              return (
                                                  <Grid
                                                      container
                                                      className="mb-5"
                                                  >
                                                      <Grid item xs={12}>
                                                          <HourlyLineGraph />
                                                      </Grid>
                                                  </Grid>
                                              );
                                          };

                                          const MonthlyGraphHideShow = () => {
                                              return (
                                                  <Grid
                                                      container
                                                      className="mb-5"
                                                  >
                                                      <Grid item xs={12}>
                                                          <MonthlySummaryGraph />
                                                      </Grid>
                                                  </Grid>
                                              );
                                          };

                                          return (
                                              <CustomContainer
                                                  isLoading={loadingOpen}
                                                  isError={isError}
                                                  alertMessage={responseMessage}
                                                  severity={severity}
                                                  onCloseCallback={
                                                      handleCustomContainerCloseCallback
                                                  }
                                              >
                                                  <Formik
                                                      initialValues={ModelObj}
                                                      onSubmit={submithandle}
                                                      resetForm={ModelObj}
                                                      render={({
                                                          values,
                                                          setFieldValue,
                                                          handleChange,
                                                          handleSubmit
                                                      }) => {
                                                          return (
                                                              <Form>
                                                                  <Grid
                                                                      item
                                                                      xs={12}
                                                                  >
                                                                      <Grid
                                                                          container
                                                                          spacing={
                                                                              2
                                                                          }
                                                                      >
                                                                          <Grid
                                                                              item
                                                                              xs={
                                                                                  12
                                                                              }
                                                                              sm={
                                                                                  12
                                                                              }
                                                                              md={
                                                                                  2
                                                                              }
                                                                              lg={
                                                                                  2
                                                                              }
                                                                          >
                                                                              <MaterialDatePickerField
                                                                                  name="Date"
                                                                                  label="Date"
                                                                                  minDate={
                                                                                      minDate
                                                                                  }
                                                                                  maxDate={
                                                                                      currentDate
                                                                                  }
                                                                              />
                                                                          </Grid>
                                                                          <Grid
                                                                              item
                                                                              xs={
                                                                                  12
                                                                              }
                                                                              sm={
                                                                                  12
                                                                              }
                                                                              md={
                                                                                  2
                                                                              }
                                                                              lg={
                                                                                  2
                                                                              }
                                                                          >
                                                                              <PrimaryButton
                                                                                  type="submit"
                                                                                  title="Search"
                                                                                  className={
                                                                                      classes.CustomPrimaryButtonStyle
                                                                                  }
                                                                                  style={{
                                                                                      marginTop:
                                                                                          '11px'
                                                                                  }}
                                                                                  fullWidth
                                                                                  disabled={
                                                                                      loadingOpen
                                                                                  }
                                                                              >
                                                                                  Search
                                                                              </PrimaryButton>

                                                                              {loadingOpen && (
                                                                                  <CircularProgress
                                                                                      size={
                                                                                          24
                                                                                      }
                                                                                      className={
                                                                                          classes.buttonProgress
                                                                                      }
                                                                                  />
                                                                              )}
                                                                          </Grid>
                                                                          <Grid
                                                                              item
                                                                              xs={
                                                                                  12
                                                                              }
                                                                              sm={
                                                                                  12
                                                                              }
                                                                              md={
                                                                                  6
                                                                              }
                                                                              lg={
                                                                                  6
                                                                              }
                                                                          ></Grid>
                                                                          <Grid
                                                                              item
                                                                              xs={
                                                                                  12
                                                                              }
                                                                              sm={
                                                                                  12
                                                                              }
                                                                              md={
                                                                                  2
                                                                              }
                                                                              lg={
                                                                                  2
                                                                              }
                                                                          >
                                                                              <Box
                                                                                  className={
                                                                                      classes.flexReverse
                                                                                  }
                                                                              >
                                                                                  <Button
                                                                                      variant="contained"
                                                                                      color="primary"
                                                                                      style={{
                                                                                          textTransform:
                                                                                              'none'
                                                                                      }}
                                                                                      className={
                                                                                          classes.CustomPrimaryButtonStyle
                                                                                      }
                                                                                      onClick={() => {
                                                                                          CheckAllBalance();
                                                                                      }}
                                                                                  >
                                                                                      Refresh
                                                                                      Bal
                                                                                  </Button>
                                                                              </Box>
                                                                          </Grid>
                                                                      </Grid>
                                                                  </Grid>
                                                              </Form>
                                                          );
                                                      }}
                                                  />
                                                  <Grid
                                                      container
                                                      className="mb-5"
                                                  >
                                                      <Grid
                                                          item
                                                          xs={12}
                                                          sm={12}
                                                          md={9}
                                                          lg={9}
                                                          xl={9}
                                                      >
                                                          {isTodaySummaryOnTop
                                                              ? TodaysSummaryHideShow()
                                                              : null}
                                                          {isMonthlyGraphOnTop
                                                              ? MonthlyGraphHideShow()
                                                              : null}
                                                          {IsProviderPerformanceGraph &&
                                                              HourlyLineGraphShow()}

                                                          <Grid
                                                              container
                                                              className="mb-5"
                                                          >
                                                              <Grid
                                                                  item
                                                                  xs={12}
                                                                  sm={12}
                                                                  md={4}
                                                                  lg={4}
                                                                  xl={4}
                                                                  className="mb-5"
                                                              >
                                                                  <TodaysTransactions
                                                                      data={
                                                                          ServicewiseSale
                                                                      }
                                                                  />
                                                              </Grid>
                                                              <Grid
                                                                  item
                                                                  xs={12}
                                                                  sm={12}
                                                                  md={8}
                                                                  lg={8}
                                                                  xl={8}
                                                                  className="mb-5"
                                                              >
                                                                  {/* <ProviderFailGraph /> */}
                                                                  <ServiceSummary
                                                                      data={
                                                                          ServicewiseSale
                                                                      }
                                                                  />
                                                              </Grid>
                                                          </Grid>
                                                          {isMonthlyGraphOnBottom
                                                              ? MonthlyGraphHideShow()
                                                              : null}
                                                          {isTodaySummaryOnBottom
                                                              ? TodaysSummaryHideShow()
                                                              : null}
                                                      </Grid>

                                                      <Grid
                                                          item
                                                          xs={12}
                                                          sm={12}
                                                          md={3}
                                                          lg={3}
                                                          xl={3}
                                                      >
                                                          <div
                                                              className={
                                                                  classes.dashboard_right_panel
                                                              }
                                                          >
                                                              {isProvidersSummaryOnTop ? (
                                                                  <ProvidersSummary
                                                                      data={
                                                                          ProviderSummraylist
                                                                      }
                                                                  />
                                                              ) : null}
                                                              <DashBoardAdminTransactionDetails
                                                                  transactionDetails={
                                                                      formObject
                                                                  }
                                                              />
                                                              <DashBoardUserSummary
                                                                  UserSummary={
                                                                      UserSummraylist
                                                                  }
                                                              />
                                                              {isProvidersSummaryOnBottom ? (
                                                                  <ProvidersSummary
                                                                      data={
                                                                          ProviderSummraylist
                                                                      }
                                                                  />
                                                              ) : null}
                                                          </div>
                                                      </Grid>
                                                  </Grid>

                                                  {lowBalance ? (
                                                      <UserLowBalance
                                                          isOpen={lowBalance}
                                                          title="Low Balance"
                                                          message={
                                                              LowBalanceMessage
                                                          }
                                                          closeButtonClick={() => {
                                                              setLowBalance(
                                                                  false
                                                              );
                                                          }}
                                                          VirtualBalanceButtonClick={() => {
                                                              setLowBalance(
                                                                  false
                                                              );
                                                              history.push(
                                                                  '/VirtualBalance'
                                                              );
                                                              return;
                                                          }}
                                                      />
                                                  ) : null}
                                              </CustomContainer>
                                          );
                                      };
