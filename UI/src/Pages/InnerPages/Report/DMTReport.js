import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { DMTTransactionPrint } from 'Components/Print/DMTPrint';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectForDMTFilter as TRANSACTION_FILTER } from 'Components/Api/TransactionReportAPI';

import { GetDMTTransactionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import { GetTransactionRefund } from 'Components/Api/TransactionAPI';

import { SelectDMTForReceiptForWeb as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { CustomModal } from 'Components/Modal/CustomModal';
import { AddComplaint } from 'Pages/InnerPages/Complaints/Add';
import {
    Container,
    Grid,
    MenuItem,
    CircularProgress,
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
    MONEY_TRANSFER_CODE,
    DIST_CODE,
    SUPERDIST_CODE,
    SUPPORT_CODE
} from 'Components/Helper/Messages';
import { OTPModal } from 'Components/Modal/OTPModal';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import { VerifyOTPRefund as VERIFY_OTP_REFUND } from 'Components/Api/TransactionAPI';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { HeaderSummary } from 'Components/SummaryCards/HeaderSummary';
import PrintConfirmation from 'Pages/InnerPages/Report/PrintConfirmation';
import { DMTStatusItems, ExportTypeItems } from 'Components/ReportHelper';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { FETCH_LIST } from 'Config';
import usePosition from 'Components/Posistion';
import { Refund as REFUND } from 'Components/Api/YesBankDMTAPI';
import { RefundConfirmAlert } from 'Components/Alert/RefundConfirmAlert';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const DMTReport = () => {
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
    const [isChargeOpen, setIsChargeOpen] = useState(false);

    const [isAdminRole, setIsAdminRole] = useState(false);
    const [TransactionMasterID, setTransactionMasterID] = useState(0);
    const [isLogModalOpen, setIsLogModalOpen] = useState(false);
    const [headerSummary, setHeaderSummary] = useState({});
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [IsDisplayAmount, setIsDisplayAmount] = useState(false);

    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [CriteriaTextLabelAmount, setCriteriaTextLabelAmount] = useState(
        false
    );
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [currentPage, setCurrentPage] = useState(0);
    const [printConfirmationRequest, setPrintConfirmationRequest] = useState(
        {}
    );
    //const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [itemList, setItemList] = useState([]);
    const [CommissionModalOpen, setCommissionModalOpen] = useState(false);
    const [commiColumns, setcommiColumns] = useState([]);
    const [openOTP, setOpenOTP] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);
    const [printData, setPrintData] = useState(null);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [ConsumerNumber, setConsumerNumber] = useState('');
    const [Amount, setAmount] = useState('');

    const [TransID, setTransID] = useState(0);
    const [openRefundOTP, setOpenRefundOTP] = useState(false);
    var IsServiceCharge = localStorage.getItem('IsServiceCharge');
    const { latitude, longitude, error } = usePosition();
    const [isConfirmAlertOpen, setIsConfirmAlertOpen] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: MONEY_TRANSFER_CODE,
        OperatorID: 0,
        Status: 'ALL',
        ReportBy: 'Inline',
        ExportType: 'Select Download',
        IsPDFDownload: false,
        IsExcelDownload: false,
        IsDMTReport: false,
        User: userDefault,
        ProviderID: 0,
        Criteria: 0,
        CriteriaText: '',
        CriteriaTextAmount: '',
        Latitude: latitude,
        Longitude: longitude
    });

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
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ServiceID: formRef.current.values.ServiceID,
            OperatorID: formRef.current.values.OperatorID,
            ProviderID: formRef.current.values.ProviderID,
            ReportBy: formRef.current.values.ReportBy,
            ExportType: formRef.current.values.ExportType,
            IsPDFDownload: formRef.current.values.IsPDFDownload,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            IsRechargeReport: formRef.current.values.IsExcelDownload,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            CriteriaTextAmount: formRef.current.values.CriteriaTextAmount,
            Latitude: latitude,
            Longitude: longitude
        };
        // if (formRef.current.values.IsExcelDownload === false) setTransactions();

        if (SkipCount <= 0) {
            setHeaderSummary({});
        }

        setResponseMessage('');

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_FILTER(requestJSON);

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

        if (data.IsExcelDownload || data.IsPDFDownload) {
            downloadFile(data.ReportPath, 'DMT Report');
            return;
        }

        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
    };

    const SelectPrintData = async (TransactionMasterID, Charge) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);

        const resp = await TRANSACTION_RECEIPT(request);

        if (!resp.isAuthorized) LOGOUT();

        if (!resp.status) return;

        var data = JSON.parse(resp.data);
        data.Charge =
            Charge === undefined || Charge === null ? data.Charge : Charge;
        setPrintData(data);

        setIsAlertOpen(true);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();

        // if (IsDisplayCriteria === true) {
        //     if (
        //         formRef.current.values.CriteriaText === undefined ||
        //         formRef.current.values.CriteriaText === null ||
        //         formRef.current.values.CriteriaText.length <= 0
        //     ) {
        //         setIsError(true);
        //         setSeverity('error');
        //         setResponseMessage('Please ,Enter valid ' + CriteriaTextLabel);
        //         return false;
        //     }

        // }

        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
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

        // CriteriaText: Yup.string()
        //     .nullable(PLEASE_SELECT_VALID + CriteriaTextLabel)
        //     .required(PLEASE_SELECT_VALID + CriteriaTextLabel),

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

    useEffect(() => {
        CheckUserRole();
        refreshForm();
    }, []);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);
        //if (API_USER_CODE === parseInt(currentUsertypeid)) setIsAPIRole(true);
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

        if (data.Status !== 'Ok') {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data.Message);
            return;
        }

        if (data.Description === 'OTP sent to customer mobile') {
            setOpenRefundOTP(true);
            setTransID(ID);
            return;
        }

        if (data.IsOTP === true) {
            setOpenOTP(true);
            setTransactionMasterID(ID);
            return;
        }
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const Refund = async (ID) => {
        setResponseMessage('');
        var inputObj = {
            ID: ID
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await REFUND(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        let data = JSON.parse(response.data);

        if (data.Status !== 'Ok') {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data.Message);
            return;
        }
        if (data.IsOTP === true) {
            setOpenOTP(true);
            setTransactionMasterID(ID);
            return;
        }
        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const CustomOtpModal = openRefundOTP ? (
        <OTPModal
            isOpen={openRefundOTP}
            title="OTP"
            error={setResponseMessage}
            submitCallback={(value) => {
                GetTransactionRefundByOTP(value);
            }}
            resendCallback={(value) => { }}
            onDismissCallback={(value) => { }}
        />
    ) : null;

    const GetTransactionRefundByOTP = async (otpObj) => {
        setLoading(true);
        setOpenOTP(false);

        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP,
            TransactionMasterID: TransID,
            ID: TransID
        };

        var response = await GetTransactionRefund(JSON.stringify(RequestObj));
        setLoading(false);

        setResponseMessage(response.data);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            setOpenOTP(true);
            return;
        }
        let data = JSON.parse(response.data);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
        setOpenRefundOTP(false);
        return;
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
        setSeverity('success');
        setResponseMessage(data.Message);
        if (data.IsOTP === true) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data.Message);
            setOpenOTP(true);
            setTransactionMasterID(TransactionMasterID);
            return;
        }
        setOpenOTP(false);
        setIsConfirmAlertOpen(true);
    };

    const updateCustomCharge = (flag) => {
        setIsChargeOpen(flag);
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
            label: 'Txn Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Sender Name',
            name: 'SenderName'
        });

        columns.push({
            label: 'Sender Mobile Number',
            name: 'SenderNo'
        });

        columns.push({
            label: 'Account Number',
            name: 'ConsumerNumber',
            bold: true
        });

        columns.push({
            label: 'Bank Name',
            name: 'BankName'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });

        columns.push({
            label: 'Beneficiary Name',
            name: 'BeneName',
            bold: true
        });

        columns.push({
            label: 'Bene Name by Bank',
            name: 'BeneficaryNameByBank',
            bold: true
        });

        columns.push({
            label: 'Payment Mode',
            name: 'PaymentType'
        });

        columns.push({
            label: 'Op Bal (₹)',
            name: 'OpeningBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Amount (₹)',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Charge',
            name: 'Charge',
            textwrapwidth: 60,
            numeric: true
        });

        if (isAdminRole) {
            columns.push({
                label: 'GST (₹)',
                name: 'GST'
            });
        }

        columns.push({
            label: 'Commission',
            name: 'RetailerCommission'
        });

        if (isAdminRole) {
            columns.push({
                label: 'TDS',
                name: 'TDS'
            });
        }

        columns.push({
            label: 'Net Commission',
            name: 'NetCommission'
        });

        columns.push({
            label: 'MDS/DS Commission',
            name: 'CommissionInfo'
        });


        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });

        columns.push({
            label: 'Txn Reference ID',
            name: 'ReferenceNo'
        });

        if (isAdminRole === true) {
            columns.push({
                label: 'Request ID',
                name: 'ProviderRequestID'
            });
        }

        var options = [
            {
                Title: 'Print',
                disabledOn: ['FAILURE'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    if (IsServiceCharge === 'true') {
                        displayCharge({
                            ID: values[2],
                            Amount: values[14]
                        });
                    } else {
                        Print({
                            ID: values[2]
                        });
                    }
                }
            },
            {
                Title: 'Check',
                disabledOn: ['FAILURE', 'SUCCESS'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    StatusCheck(values[2]);
                }
            },
            {
                Title: 'Refund',
                disabledOn: ['FAILURE', 'SUCCESS', 'PROCESS'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    Refund(values[2]);
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
                        amt: values[14]
                    });
                }
            });
        }

        if (
            RETAILER_CODE === parseInt(currentUsertypeid) ||
            API_USER_CODE === parseInt(currentUsertypeid)
        ) {
            options.push({
                Title: 'Complaint',
                disabledOn: ['FAILURE'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    RaiseComplaint(values[2]);
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

    const logClick = async ({ ID, Number, amt }) => {
        setMasterID(ID);
        setConsumerNumber(Number);
        setAmount(amt);
        setIsLogModalOpen(true);
    };

    const RaiseComplaint = (ID) => {
        setTransactionMasterID(ID);
        setIsOpen(true);
    };

    const displayCharge = (values) => {
        setPrintConfirmationRequest(values);
        updateCustomCharge(true);
    };

    const Print = ({ ID, Charge }) => {
        SelectPrintData(ID, Charge);
        //setCalculatedCharge(Charge);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
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
        setIsDisplayAmount(false);

        if (value === 'AmountRange') {
            setCriteriaTextLabel('FromAmount');
            setCriteriaTextLabelAmount('ToAmount');
            setIsDisplayAmount(true);
        }
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
                        <Form
                            style={{
                                marginBottom: '15px'
                            }}
                        >
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'DMT Report'}
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
                                                {DMTStatusItems}
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
                                                    key={2}
                                                    value={'SenderNo'}
                                                    onClick={setFieldValue}
                                                >
                                                    Sender Mobile Number
                                                </MenuItem>
                                                <MenuItem
                                                    key={1}
                                                    value={'TransactionID'}
                                                    onClick={setFieldValue}
                                                >
                                                    Transaction Ref ID
                                                </MenuItem>
                                                <MenuItem
                                                    key={2}
                                                    value={'AccountNumber'}
                                                    onClick={setFieldValue}
                                                >
                                                    Account Number
                                                </MenuItem>
                                                <MenuItem
                                                    key={3}
                                                    value={'UserNumber'}
                                                    onClick={setFieldValue}
                                                >
                                                    User Mobile Number
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                        {!IsDisplayCriteria ? null : (
                                            <>
                                                <Grid
                                                    item
                                                    xs={12}
                                                    sm={12}
                                                    md={2}
                                                    lg={2}
                                                >
                                                    <MaterialTextField
                                                        label={
                                                            CriteriaTextLabel
                                                        }
                                                        name="CriteriaText"
                                                        type="text"
                                                        placeholder={
                                                            'Enter ' +
                                                            CriteriaTextLabel
                                                        }
                                                    />
                                                </Grid>
                                                {!IsDisplayAmount ? null : (
                                                    <Grid
                                                        item
                                                        xs={12}
                                                        sm={12}
                                                        md={2}
                                                        lg={2}
                                                    >
                                                        <MaterialTextField
                                                            label={
                                                                CriteriaTextLabelAmount
                                                            }
                                                            name="CriteriaTextAmount"
                                                            type="text"
                                                            placeholder={
                                                                'Enter ' +
                                                                CriteriaTextLabelAmount
                                                            }
                                                        />
                                                    </Grid>
                                                )}
                                            </>
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
                                                // onClick={(e) => {
                                                //     setFieldValue(
                                                //         'IsExcelDownload',
                                                //         false
                                                //     );
                                                //     handleSubmit(e);
                                                // }}
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
                    callSelectData(TakeCount, SkipCount, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        '',
                        column,
                        direction
                    );
                }}
            />

            {isAlertOpen ? (
                <DMTTransactionPrint
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

            {isChargeOpen ? (
                <PrintConfirmation
                    isOpen={isChargeOpen}
                    title="Do you want to print ?"
                    message="Are you sure to get this print?"
                    request={printConfirmationRequest}
                    positiveButtonClick={(request) => {
                        setIsChargeOpen(false);

                        Print(request);
                    }}
                    negativeButtonClick={(value) => {
                        setIsChargeOpen(false);
                    }}
                />
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
                    error={''}
                    submitCallback={(value) => {
                        Insert(value);
                    }}
                    resendCallback={(value) => { }}
                    onDismissCallback={(value) => {
                        setOpenOTP(value);
                    }}
                />
            ) : null}

            {isConfirmAlertOpen && (
                <RefundConfirmAlert
                    isOpen={isConfirmAlertOpen}
                    title="Verification"
                    message="Transaction Refunded successfully"
                    positiveButtonClick={() => {
                        setIsConfirmAlertOpen(false);
                    }}
                />
            )}

            {CustomOtpModal}
        </CustomContainer>
    );
};
