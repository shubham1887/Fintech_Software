import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { HeaderSummary } from 'Components/SummaryCards/HeaderEcollection';
import { SelectUserForDropDownForCashCollection as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Select as TRANSACTION_FILTER } from 'Components/Api/PaymentGatewayAPI';
import { TransactionStatus as TRANSACTION_STATUS } from 'Components/Api/PaymentGatewayAPI';
import { SelectRoleCommission as SELECT_ROLE_COMMISSION } from 'Components/Api/PaymentGatewayAPI';
import { HeaderPGCommission } from 'Components/SummaryCards/HeaderPGCommission';
import { Container, Grid, Box, MenuItem, Accordion } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { StatusPGEdit } from 'Pages/InnerPages/Report/ManualPGStatusEdit';
import { PGTransactionPrint } from 'Components/Print/PGPrint';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    SUPPORT_CODE,
    API_USER_CODE,
    ADMIN_CODE,
    PAYMENT_GATEWAY_CODE,
    CREDITCARD_TO_BANK
} from 'Components/Helper/Messages';
import { SelectRechargeAndDMT as WALLET_SELECT } from 'Components/Api/WalletApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectByOrderID as SELECT_BY_ORDERID } from 'Components/Api/PaymentGatewayAPI';
import { FETCH_LIST } from 'Config';
import {
    StatusItems,
    PGModeItems,
    ExportTypeItems
} from 'Components/ReportHelper';

import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { SelectByIDLog } from 'Pages/InnerPages/Report/GetwayLogByID';
import { Select as GATEWAY_DROP_DOWN } from 'Components/Api/PaymentGatewaySettingAPI';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const PaymentGatewayReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [openFilter, setopenFilter] = useState(true);
    const [CommissionModalOpen, setCommissionModalOpen] = useState(false);
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [commiColumns, setcommiColumns] = useState([]);
    const [paymentGatewayID, setPaymentGatewayID] = useState(0);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [userList, setUserList] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [isLogModalOpen, setIsLogModalOpen] = useState(false);
    const [orderID, setOrderID] = useState(0);
    const [UserName, setUserName] = useState('');
    const [Amount, setAmount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [wallets, setWallets] = useState([]);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [headerSummary, setHeaderSummary] = useState({});
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const [providerList, setProviderList] = useState([]);
    const [printData, setPrintData] = useState(null);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
      const IsPGSettlementHold =
          localStorage.getItem('IsPGSettlementHold') === 'true';
      

      const [ModelObj] = useState({
          ToDate: new Date(),
          FromDate: new Date(),
          ProviderID: 0,
          WalletID: 0,
          ServiceID: PAYMENT_GATEWAY_CODE,
          Criteria: 0,
          CriteriaText: '',
          Status: 'ALL',
          PayMode: 'ALL',
          User: userDefault,
          ExportType: 'Select Download'
      });
      const selectGateway = async () => {
          if (!isAdminRole) return;

          setLoading(true);
          const response = await GATEWAY_DROP_DOWN();
          setLoading(false);
          if (!response.isAuthorized) LOGOUT();

          if (!response.status) return;

          var data = JSON.parse(response.data);
          setProviderList(data);
      };

      const OnChangeCriteria = (value) => {
          if (value === undefined || value <= 0) {
              setIsDisplayCriteria(false);
              setCriteriaTextLabel('');
              return;
          }
          setCriteriaTextLabel(value);
          setIsDisplayCriteria(true);
      };

      const [headerCommission, setheaderCommission] = useState({
          UserName: '',
          PayMode: '',
          Amount: ''
      });
      const fetchUserList = async () => {
          let listValue = await FETCH_LIST();

          if (
              listValue === undefined ||
              listValue === null ||
              listValue === ''
          ) {
              return;
          }

          if (listValue !== undefined && listValue === 'All') {
              SelectUser('');
              return;
          }
      };
      const refreshForm = () => {
          if (!formRef.current) return;

          formRef.current.handleSubmit();
      };
      const CheckUserRole = () => {
          if (
              ADMIN_CODE === parseInt(currentUsertypeid) ||
              SUPPORT_CODE === parseInt(currentUsertypeid)
          )
              setIsAdminRole(true);

          if (RETAILER_CODE === parseInt(currentUsertypeid)) {
              setIsRetailerRole(true);
          }
          if (API_USER_CODE === parseInt(currentUsertypeid)) {
              setIsRetailerRole(true);
          }
      };
      const handleSubmit = async (values, { setSubmitting, resetForm }) => {
          setSubmitting(true);
          //setTransactions([]);
          createColumns();
          await callSelectData(TakeCount, SkipCount);
          setSubmitting(false);
      };

      const callSelectData = async (TakeCount, SkipCount) => {
          let inputObj = {
              TakeCount: TakeCount,
              SkipCount: SkipCount,
              FromDate: formRef.current.values.FromDate,
              ToDate: formRef.current.values.ToDate,
              Status: formRef.current.values.Status,
              UserID: formRef.current.values.User.UserID,
              ProviderID: formRef.current.values.ProviderID,
              ServiceID: formRef.current.values.ServiceID,
              WalletID: formRef.current.values.WalletID,
              PayMode: formRef.current.values.PayMode,
              Criteria: formRef.current.values.Criteria,
              CriteriaText: formRef.current.values.CriteriaText,
              ExportType: formRef.current.values.ExportType
          };

          var requestObj = JSON.stringify(inputObj);

          if (SkipCount <= 0) {
              setHeaderSummary({});
          }
          setIsError(false);
          setLoading(true);
          setResponseMessage('');

          var response = await TRANSACTION_FILTER(requestObj);
          setLoading(false);

          if (!response.isAuthorized) LOGOUT();

          setSeverity('success');

          if (!response.status) {
              setIsError(true);
              setSeverity('error');
              setResponseMessage(response.data);

              return;
          }
          var data = JSON.parse(response.data);
          if (data.IsDownload) {
              downloadFile(data.ReportPath, 'PaymentGateway Report');
              return;
          }

          if (SkipCount <= 0) {
              setHeaderSummary(data.HeaderSummary);
              setTotalCount(data.Count);
          }

          setTransactions(data.list);
      };
      function downloadFile(URL, Title) {
          let a = document.createElement('a');
          a.href = URL;
          a.setAttribute('download', Title);
          a.setAttribute('target', '_blank');
          a.click();
      }

      const validationSchema = Yup.object({
          User: Yup.object()
              .nullable(PLEASE_SELECT_VALID + 'User')
              .required(PLEASE_SELECT_VALID + 'User')
              .shape({
                  UserID: Yup.number()
                      .nullable(PLEASE_SELECT_VALID + 'User')
                      .min(0, PLEASE_SELECT_VALID + 'User')
                      .required(PLEASE_SELECT_VALID + 'User')
              }),

          FromDate: Yup.date()
              .nullable(PLEASE_SELECT_VALID + 'from date')
              .required(PLEASE_SELECT_VALID + 'date')
              .max(new Date(), PLEASE_SELECT_VALID + 'from date')
              .typeError(PLEASE_SELECT_VALID + 'from date'),

          ToDate: Yup.date()
              .nullable(PLEASE_SELECT_VALID + 'date')
              .required(PLEASE_SELECT_VALID + 'date')
              .max(new Date(), PLEASE_SELECT_VALID + 'to date')
              .typeError(PLEASE_SELECT_VALID + 'to date')
      });
      const selectWallets = async () => {
          var request = {
              ID: 0
          };
          var json = JSON.stringify(request);
          const response = await WALLET_SELECT(json);

          if (!response.isAuthorized) LOGOUT();

          if (!response.status) {
              setResponseMessage(response.data);
              setIsError(true);
              return;
          }

          var data = JSON.parse(response.data);

          setWallets(data);
      };

      const callSearch = async (text) => {
          if (
              text === undefined ||
              text === null ||
              text === '' ||
              text.length <= 0 ||
              text === userDefault.FirmName
          )
              return;

          var jsonObj = {
              FirmName: text
          };
          SelectUser(jsonObj);
      };

      const SelectUser = async (values) => {
          var currentUsertypeid = localStorage.getItem('UserTypeID');

          if (RETAILER_CODE === parseInt(currentUsertypeid)) {
              let objWithSelectAll = [userDefault];
              setUserList(objWithSelectAll);
              return;
          }

          setLoading(true);
          var request = JSON.stringify(values);
          setLoading(false);
          const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

          if (!userResponse.isAuthorized) LOGOUT();

          if (!userResponse.status) {
              setIsError(true);
              setSeverity('error');
              return;
          }
          var data = JSON.parse(userResponse.data);

          let objWithSelectAll = [userDefault, ...data];

          setUserList(objWithSelectAll);
      };

      const StatusCheck = async (OrderID) => {
          var inputObj = {
              OrderID: OrderID
          };

          var requestObj = JSON.stringify(inputObj);

          setIsError(false);
          setLoading(true);
          setResponseMessage('');

          var response = await TRANSACTION_STATUS(requestObj);
          setLoading(false);

          if (!response.isAuthorized) LOGOUT();
          if (!response.status) {
              setIsError(true);
              setResponseMessage(response.data);
              setSeverity('error');
              return;
          }
          setIsError(true);
          setSeverity('success');
          if (response.data === 'Ok') setResponseMessage('Transaction success');
          else setResponseMessage(response.data);
      };

      const selectCommission = async (row) => {
          setItemList([]);
          setheaderCommission({
              UserName: row[4],
              PayMode: row[13],
              Amount: row[8]
          });

          let jsonObj = {
              PaymentGatewayID: row[2],
              UserID: row[14]
          };

          var request = JSON.stringify(jsonObj);

          setIsError(false);
          setLoading(true);
          createCommissionColumns();
          const response = await SELECT_ROLE_COMMISSION(request);
          setLoading(false);

          if (!response.isAuthorized) LOGOUT();

          if (!response.status) {
              setIsError(true);
              setSeverity('error');
              setResponseMessage(response.data);
          }
          var data = JSON.parse(response.data);
          setCommissionModalOpen(true);

          var columns = [];

          columns.push({
              Role: 'Distributor',
              FirmName: data[0].DistributorName,
              Commission: data[0].DistributorCommission
          });

          columns.push({
              Role: 'SuperDistributor',
              FirmName: data[0].SuperDistributorName,
              Commission: data[0].SuperDistributorCommission
          });

          setItemList(columns);
      };
      const createCommissionColumns = () => {
          var columns = [];

          columns.push({
              label: '#',
              name: 'id',
              srno: true
          });

          columns.push({
              label: 'Role',
              name: 'Role'
          });
          columns.push({
              label: 'FirmName',
              name: 'FirmName'
          });
          columns.push({
              label: 'Commission',
              name: 'Commission'
          });

          setcommiColumns(columns);
      };
      const Print = async (OrderID) => {
          var inputObj = {
              OrderID: OrderID
          };
          var requestObj = JSON.stringify(inputObj);

          const response = await SELECT_BY_ORDERID(requestObj);
          setLoading(false);

          if (!response.isAuthorized) LOGOUT();

          if (!response.status) {
              return;
          }
          var data = JSON.parse(response.data);

          setPrintData(data);

          setIsAlertOpen(true);
      };
      const WalletList = wallets.map((item) => (
          <MenuItem key={item.WalletID} value={item.WalletID}>
              {item.Name}
          </MenuItem>
      ));

      useEffect(() => {
          fetchUserList();
          selectGateway();
          selectWallets();
          CheckUserRole();
          refreshForm();
      }, [isAdminRole]);

      const logClick = async (request, UserName, Amount) => {
          setIsLogModalOpen(true);
          setOrderID(request);
          setUserName(UserName);
          setAmount(Amount);
      };
      const createColumns = () => {
          var columns = [];
          var statusIndex = 1;

          columns.push({
              label: 'Status',
              name: 'Status',
              hide: true
          });

          columns.push({
              label: '#',
              name: 'PaymentGatewayID',
              srno: true
          });

          columns.push({
              label: 'Date',
              name: 'DisplayDOC'
          });
          columns.push({
              label: 'Name',
              name: 'UserName'
          });
          columns.push({
              label: 'OrderID',
              name: 'OrderID'
          });
          columns.push({
              label: 'Wallet',
              name: 'WalletName'
          });
          columns.push({
              label: 'Op Bal',
              name: 'OpeningBalance',
              textwrapwidth: 60,
              numeric: true
          });

          columns.push({
              label: 'Amount',
              name: 'Amount',
              textwrapwidth: 60,
              numeric: true
          });
          if (isAdminRole === true) {
              columns.push({
                  label: 'Comm',
                  name: 'Commission',
                  textwrapwidth: 60,
                  hiddenbutton: true,
                  callback: (row) => {
                      selectCommission(row);
                  }
              });
          }
          if (!isAdminRole) {
              columns.push({
                  label: 'Comm',
                  name: 'Commission',
                  textwrapwidth: 60,
                  numeric: true
              });
          }
          columns.push({
              label: 'Cl Bal',
              name: 'ClosingBalance',
              textwrapwidth: 60,
              numeric: true
          });
          columns.push({
              label: 'Status',
              name: 'Status',
              customStatus: true
          });
          if (IsPGSettlementHold) {
              columns.push({
                  label: 'PaymentStatus',
                  name: 'PaymentStatus',
                  customStatus: true
              });
          }

          columns.push({
              label: 'Txn ID',
              name: 'TransactionID'
          });
          columns.push({
              label: 'PayMode',
              name: 'PayMode'
          });
          columns.push({
              label: 'UserID',
              name: 'UserID',
              hide: true
          });

          if (isAdminRole === true) {
              columns.push({
                  label: 'PG Name',
                  name: 'ProviderName'
              });
          }
          columns.push({
              label: 'Remark',
              name: 'Remark'
          });
          columns.push({
              label: 'Sender',
              name: 'Sender'
          });
          columns.push({
              label: 'Parent',
              name: 'ParentName'
          });

          if (isAdminRole === true) {
              columns.push({
                  label: 'Update',
                  name: 'PaymentGatewayID',
                  icon: 'EditIcon',
                  disabledOn: ['FAILURE', 'SUCCESS'],
                  statusIndex: statusIndex,
                  iconButton: true,
                  callback: (value) => {
                      Edit(value);
                  }
              });
          }
          var options = [
              {
                  Title: 'Print',
                  disabledOn: ['FAILURE'],
                  statusIndex: statusIndex,
                  Callback: (values) => {
                      Print(values[0]);
                  }
              }
          ];
          options.push({
              Title: 'Check',
              disabledOn: ['FAILURE', 'SUCCESS'],
              statusIndex: statusIndex,
              Callback: (values) => {
                  StatusCheck(values[0]);
                  refreshForm();
              }
          });
          if (isAdminRole === true) {
              options.push({
                  Title: 'Log',
                  Callback: (values) => {
                      logClick(values[5], values[4], values[8]);
                  }
              });
          }
          columns.splice(0, 0, {
              label: ' ',
              name: 'OrderID',
              MoreMenu: true,
              options: options
          });
          setMainColumns(columns);
      };

    const Edit = async (request) => {
        setIsModalOpen(true);

        setPaymentGatewayID(request);
    };
    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Payment Gateway Report'}
                                    open={openFilter}
                                    setOpen={(value) => setopenFilter(value)}
                                >
                                    <Grid container spacing={2}>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Status"
                                                name="Status"
                                                title="Status"
                                            >
                                                {StatusItems}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="FromDate"
                                                label="From Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialDatePickerField
                                                name="ToDate"
                                                label="To Date"
                                                maxDate={new Date()}
                                            />
                                        </Grid>
                                        {!isRetailerRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <Field
                                                    component={
                                                        AsyncAutoComplete
                                                    }
                                                    defaultValue={userDefault}
                                                    FieldName="User"
                                                    name="User"
                                                    SubFieldName="UserID"
                                                    options={userList}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select User'
                                                    }}
                                                    onSearchCallback={(
                                                        text
                                                    ) => {
                                                        callSearch(text);
                                                    }}
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        setUserDefault({
                                                            UserID:
                                                                value.UserID,
                                                            FirmName:
                                                                value.FirmName
                                                        });
                                                    }}
                                                />
                                            </Grid>
                                        ) : null}
                                        {isAdminRole ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialSelect
                                                    label="Select Gateway"
                                                    name="ProviderID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Gateway
                                                    </MenuItem>
                                                    {providerList.map(
                                                        (
                                                            {
                                                                PaymentGatewaySettingID,
                                                                Name
                                                            },
                                                            index
                                                        ) => (
                                                            <MenuItem
                                                                key={index}
                                                                value={
                                                                    PaymentGatewaySettingID
                                                                }
                                                            >
                                                                {Name}
                                                            </MenuItem>
                                                        )
                                                    )}
                                                </MaterialSelect>
                                            </Grid>
                                        ) : null}
                                        <Grid
                                        

                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Wallet"
                                                name="WalletID"
                                                title="Select Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    Select Wallet
                                                </MenuItem>
                                                {WalletList}
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="PayMode"
                                                name="PayMode"
                                                title="PayMode"
                                            >
                                                {PGModeItems}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Selection criteria"
                                                name="Criteria"
                                                title="criteria"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    // setCriteriaTextLabel(value);
                                                    // setIsDisplayCriteria(true);

                                                    OnChangeCriteria(value);
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria
                                                </MenuItem>
                                                <MenuItem
                                                    key={1}
                                                    value={'OrderID'}
                                                >
                                                    OrderID
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                        {!IsDisplayCriteria ? null : (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialTextField
                                                    label={CriteriaTextLabel}
                                                    name="CriteriaText"
                                                    type="text"
                                                    placeholder={
                                                        'Enter ' +
                                                        CriteriaTextLabel
                                                    }
                                                    length={100}
                                                />
                                            </Grid>
                                        )}
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="Service"
                                                name="ServiceID"
                                                title="Service"
                                            >
                                                <MenuItem
                                                    key={PAYMENT_GATEWAY_CODE}
                                                    value={PAYMENT_GATEWAY_CODE}
                                                >
                                                    Payment Gateway
                                                </MenuItem>

                                                <MenuItem
                                                    key={CREDITCARD_TO_BANK}
                                                    value={CREDITCARD_TO_BANK}
                                                >
                                                    CreditCard To Bank
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <MaterialSelect
                                                label="ExportType"
                                                name="ExportType"
                                                title="ExportType"
                                            >
                                                {ExportTypeItems}
                                            </MaterialSelect>
                                        </Grid>
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <PrimaryButton
                                                type="submit"
                                                title="Search"
                                                className={
                                                    classes.CustomPrimaryButtonStyle
                                                }
                                                disabled={loading}
                                            >
                                                Search
                                            </PrimaryButton>
                                            {loading && (
                                                <CircularProgress
                                                    size={24}
                                                    className={
                                                        classes.buttonProgress
                                                    }
                                                />
                                            )}
                                        </Grid>
                                    </Grid>
                                </CustomAccordion>
                            </Grid>
                        </Form>
                    );
                }}
            </Formik>
            <HeaderSummary headerSummary={headerSummary} />
            <ReactTable
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    callSelectData(currentPage, TakeCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        // SearchText,
                        column,
                        direction
                    );
                }}
            />

            {isModalOpen ? (
                <CustomModalWithScroll
                    isOpen={isModalOpen}
                    title="Manual Status update"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsModalOpen(value);
                        refreshForm();
                    }}
                >
                    <StatusPGEdit
                        PaymentGatewayID={paymentGatewayID}
                        onDismissCallback={(value) => {
                            setIsModalOpen(value);
                            refreshForm();
                        }}
                    />
                </CustomModalWithScroll>
            ) : null}
            {isAlertOpen ? (
                <PGTransactionPrint
                    isOpen={isAlertOpen}
                    title="Transaction Print"
                    request={printData}
                    positiveButtonClick={(printData) => {
                        setIsAlertOpen(false);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            {CommissionModalOpen ? (
                <CustomModalWithScroll
                    isOpen={CommissionModalOpen}
                    title="Upline Commission"
                    width="75%"
                    onDismissCallback={(value) => {
                        setCommissionModalOpen(value);
                    }}
                >
                    <Grid item xs={12}>
                        <HeaderPGCommission
                            headerCommission={headerCommission}
                        />
                        <ReactTable
                            data={itemList}
                            columns={commiColumns}
                            headerToolbar={false}
                            footerToolbar={false}
                        />
                    </Grid>
                </CustomModalWithScroll>
            ) : null}
            {isLogModalOpen ? (
                <CustomModalWithScroll
                    isOpen={isLogModalOpen}
                    title="API Log"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsLogModalOpen(value);
                    }}
                >
                    <SelectByIDLog
                        OrderID={orderID}
                        UserName={UserName}
                        Amount={Amount}
                        setLoading={(value) => setLoading(value)}
                    />
                </CustomModalWithScroll>
            ) : null}
        </CustomContainer>
    );
};
