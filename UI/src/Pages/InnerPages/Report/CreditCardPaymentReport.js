import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { TransactionPrint } from 'Components/Print/RechargePrint';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectForCreditCardFilter as TRANSACTION_FILTER } from 'Components/Api/TransactionReportAPI';
import { GetCreditCardTransactionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import { SelectCreditCardForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { CustomModal } from 'Components/Modal/CustomModal';
import { AddComplaint } from 'Pages/InnerPages/Complaints/Add';
import {
    Container,
    Grid,
    MenuItem,
    CircularProgress,
    Box,
    Icon,
    Accordion
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    API_USER_CODE,
    ADMIN_CODE,
    CREDITCARDPAYMENT_CODE,
    DIST_CODE,
    SUPERDIST_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import { OTPModal } from 'Components/Modal/OTPModal';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import excel from 'Assets/Images/excel.svg';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';

import { VerifyOTPRefundForCreditCard as VERIFY_OTP_REFUND } from 'Components/Api/TransactionAPI';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { HeaderSummary } from 'Components/SummaryCards/HeaderSummary';

import { StatusItems, ExportTypeItems } from 'Components/ReportHelper';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { FETCH_LIST } from 'Config';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const CreditCardPaymentReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [openFilter, setopenFilter] = useState(true);

    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [userList, setUserList] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [masterID, setMasterID] = useState(0);

    const [isAdminRole, setIsAdminRole] = useState(false);
    const [TransactionMasterID, setTransactionMasterID] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [modalErrorMessage, setModalErrorMessage] = useState('');

    const [isLogModalOpen, setIsLogModalOpen] = useState(false);
    const [headerSummary, setHeaderSummary] = useState({});
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState('');
    const [providerList, setProviderList] = useState([]);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [currentPage, setCurrentPage] = useState(0);

    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [CommissionModalOpen, setCommissionModalOpen] = useState(false);
    const [commiColumns, setcommiColumns] = useState([]);
    const [isAPIRole, setIsAPIRole] = useState(false);
    const [openOTP, setOpenOTP] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);
    const [printData, setPrintData] = useState(null);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [ConsumerNumber, setConsumerNumber] = useState('');
    const [Amount, setAmount] = useState('');

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: CREDITCARDPAYMENT_CODE,
        OperatorID: 0,
        Status: 'ALL',
        ExportType: 'Select Download',
        User: userDefault,
        ProviderID: 0,
        Criteria: 0,
        CriteriaText: ''
    });
    const fetchUserList = async () => {
        let listValue = await FETCH_LIST();

        if (listValue === undefined || listValue === null || listValue === '') {
            return;
        }

        if (listValue !== undefined && listValue === 'All') {
            setisFetchAllList(true);
            SelectUser('');
            return;
        }
    };
    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ServiceID: formRef.current.values.ServiceID,
            OperatorID: formRef.current.values.OperatorID,
            ProviderID: formRef.current.values.ProviderID,
            Criteria: formRef.current.values.Criteria,
            //CriteriaNumber: formRef.current.values.CriteriaNumber,
            CriteriaText: formRef.current.values.CriteriaText,
            ExportType: formRef.current.values.ExportType
        };
        if (SkipCount <= 0) {
            setHeaderSummary({});
        }
        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_FILTER(requestJSON);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setSeverity('success');
        var data = JSON.parse(response.data);

        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Credit Card Report');
            return;
        }
        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }

        setTransactions(data.list);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoading(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();

        // if (!status) return;

        var d = JSON.parse(data);
        setPrintData(d);

        setIsAlertOpen(true);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const selectProvider = async () => {
        if (ADMIN_CODE !== parseInt(currentUsertypeid)) return;

        setLoading(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };

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

    useEffect(() => {
        fetchUserList();
        selectProvider();
        CheckUserRole();
        refreshForm();
    }, []);
    const showUserList = () => {
        if (isDistRole === true) return true;
        if (isAdminRole === true) return true;
    };
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);

        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (API_USER_CODE === parseInt(currentUsertypeid)) setIsAPIRole(true);
        if (DIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
        if (SUPERDIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
    };

    const StatusCheck = async (ID) => {
        setResponseMessage('');
        var inputObj = {
            ID: ID
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_STATUS(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        let data = JSON.parse(response.data);
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        if (data.IsOTP === true) {
            setOpenOTP(true);
            setTransactionMasterID(ID);
            return;
        }
    };

    const Insert = async (values) => {
        setResponseMessage('');
        var jsonObj = {
            OTP: values.OTP,
            TransactionMasterID: TransactionMasterID
        };
        var requestObj = JSON.stringify(jsonObj);

        setIsError(false);
        setLoading(true);

        var response = await VERIFY_OTP_REFUND(requestObj);

        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);

        if (data.IsOTP === true) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data.Message);
            setOpenOTP(true);
            setTransactionMasterID(TransactionMasterID);
            return;
        }
        setSeverity('success');
        setResponseMessage(data.Message);
        setOpenOTP(false);
    };
    const Print = (value) => {
        SelectPrintData(value);
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
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        if (isRetailerRole === false) {
            columns.push({
                label: 'User',
                name: 'FirmName'
            });
            columns.push({
                label: 'User Mobile',
                name: 'UserMobile'
            });

            statusIndex = 1;
        }

        columns.push({
            label: 'Mobile Number',
            name: 'SenderNo'
        });

        columns.push({
            label: 'Card Holder Name',
            name: 'SenderName'
        });

        columns.push({
            label: 'Card Number',
            name: 'ConsumerNumber',
            bold: true
        });

        columns.push({
            label: 'Reference',
            name: 'ReferenceNo'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Op Bal',
            name: 'OpeningBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Amt',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Comm.Info',
            name: 'CommissionInfo'
        });
        columns.push({
            label: 'TDS',
            name: 'TDS'
        });

        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Type',
            name: 'PaymentType'
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Provider',
                name: 'ProviderName'
            });
        }

        columns.push({
            label: 'IP',
            name: 'IPAddress'
        });
        if (isAdminRole) {
            columns.push({
                label: 'GST',
                name: 'GST'
            });
            columns.push({
                label: 'RequestID',
                name: 'ProviderRequestID'
            });
        }

        var options = [
            {
                Title: 'Print',
                disabledOn: ['FAILURE'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    Print(values[2]);
                }
            },
            {
                Title: 'Check',
                disabledOn: ['FAILURE'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    StatusCheck(values[2]);
                }
            }
        ];

        if (isAdminRole === true) {
            options.push({
                Title: 'Log',
                Callback: (values) => {
                    logClick({
                        ID: values[2],
                        Number: values[9],
                        amt: values[15]
                    });
                }
            });
        }

        columns.splice(0, 0, {
            label: ' ',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });

        setMainColumns(columns);
    };

    const OnChangeCriteria = (setFieldValue, value) => {
        setFieldValue('CriteriaText', '');

        if (value === undefined || value <= 0) {
            setIsDisplayCriteria(false);
            setCriteriaTextLabel('');
            return;
        }
        setCriteriaTextLabel(value);
        setIsDisplayCriteria(true);
        //setCriteriaNumberLabel(true);
    };
    const logClick = async ({ ID, Number, amt }) => {
        setMasterID(ID);
        setConsumerNumber(Number);
        setAmount(amt);
        setIsLogModalOpen(true);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
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
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={ModelObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'Credit Card Report'}
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

                                        {showUserList() ? (
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
                                                    label="Select Provider"
                                                    name="ProviderID"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Provider
                                                    </MenuItem>
                                                    {providerList.map(
                                                        (
                                                            {
                                                                ProviderID,
                                                                ProviderName
                                                            },
                                                            index
                                                        ) => (
                                                            <MenuItem
                                                                key={index}
                                                                value={
                                                                    ProviderID
                                                                }
                                                            >
                                                                {ProviderName}
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
                                                label="Selection Criteria"
                                                name="Criteria"
                                                title="criteria"
                                                onCustomCallback={(
                                                    name,
                                                    value
                                                ) => {
                                                    OnChangeCriteria(
                                                        setFieldValue,
                                                        value
                                                    );
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria
                                                </MenuItem>
                                                <MenuItem
                                                    key={1}
                                                    value={'TransactionID'}
                                                >
                                                    TransactionID
                                                </MenuItem>
                                                <MenuItem
                                                    key={2}
                                                    value={'AccountNumber'}
                                                >
                                                    Account Number
                                                </MenuItem>
                                                <MenuItem
                                                    key={2}
                                                    value={'SenderNo'}
                                                >
                                                    Sender Number
                                                </MenuItem>
                                                <MenuItem
                                                    key={3}
                                                    value={'UserNumber'}
                                                >
                                                    User Number
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>

                                        {!IsDisplayCriteria ? null : (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={3}
                                                lg={3}
                                            >
                                                <MaterialTextField
                                                    label={CriteriaTextLabel}
                                                    name="CriteriaText"
                                                    type="text"
                                                    placeholder={
                                                        'Enter ' +
                                                        CriteriaTextLabel
                                                    }
                                                    length={50}
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
                serverSide={true}
                rowsPerPage={TakeCount}
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
                getSearchCallback={(SearchText) => {
                    callSelectData(currentPage, TakeCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />

            {isAlertOpen ? (
                <TransactionPrint
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

            {isLogModalOpen ? (
                <CustomModal
                    isOpen={isLogModalOpen}
                    title="API Initial Log"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsLogModalOpen(value);
                    }}
                >
                    <SelectByIDLog
                        TransactionMasterID={masterID}
                        ConsumerNumber={ConsumerNumber}
                        Amount={Amount}
                        setLoading={(value) => setLoading(value)}
                    />
                </CustomModal>
            ) : null}

            {isOpen ? (
                <CustomModal
                    isOpen={isOpen}
                    title="Complaint"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsOpen(value);
                    }}
                >
                    <AddComplaint
                        IsEdit={false}
                        ComplaintMasterID={0}
                        TransactionMasterID={TransactionMasterID}
                        onDismissCallback={(value) => {
                            setIsOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}

            {CommissionModalOpen ? (
                <CustomModal
                    isOpen={CommissionModalOpen}
                    title="Upline Commission"
                    width="75%"
                    onDismissCallback={(value) => {
                        setCommissionModalOpen(value);
                    }}
                >
                    <Grid item xs={12}>
                        <ReactTable
                            data={itemList}
                            columns={commiColumns}
                            headerToolbar={false}
                            footerToolbar={false}
                        />
                    </Grid>
                </CustomModal>
            ) : null}

            {openOTP ? (
                <OTPModal
                    isOpen={openOTP}
                    title="OTP"
                    error={modalErrorMessage}
                    submitCallback={(value) => {
                        Insert(value);
                    }}
                    resendCallback={(value) => {}}
                    onDismissCallback={(value) => {
                        setOpenOTP(value);
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};

export default CreditCardPaymentReport;
