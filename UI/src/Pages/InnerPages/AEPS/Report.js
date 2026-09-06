import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import { SelectAEPSForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { GetAEPSTransactionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import { AEPSPrint } from 'Components/Print/AEPSPrint';
import {
    Container,
    Grid,
    MenuItem,
    Icon,
    Box,
    Accordion,
    CircularProgress
} from '@material-ui/core';
import { HeaderSummary } from 'Components/SummaryCards/HeaderEcollection';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { RETAILER, ADMIN } from 'Components/Helper/UsertypeCode';
import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Select as OPERATOR_SELECT_BY_SERVICEID } from 'Components/Api/OperatorsApi';
import { Select as SEARCH } from 'Components/Api/AEPSReportAPI';
import {
    PLEASE_SELECT_VALID,
    ADMIN_CODE,
    DIST_CODE,
    SUPERDIST_CODE,
    SUPPORT_CODE,
    AEPS_CODE
} from 'Components/Helper/Messages';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import HOCLoader from 'Components/Loader/HOCLoader';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { StatusItems, ExportTypeItems } from 'Components/ReportHelper';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { FETCH_LIST } from 'Config';
import { SelectRoleCommission as SELECT_ROLE_COMMISSION } from 'Components/Api/TransactionReportAPI';
import { HeaderCommission } from 'Components/SummaryCards/HeaderCommission';
import { CustomModal } from 'Components/Modal/CustomModal';
import usePosition from 'Components/Posistion';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const Report = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [openFilter, setopenFilter] = useState(true);
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [transactions, setTransactions] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [downline, setDownline] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [currentUserTypeID, setCurrentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [headerSummary, setHeaderSummary] = useState({});
    const [SearchText, setSearchText] = useState('');
    const [operators, setOperators] = useState([]);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [printData, setPrintData] = useState(null);
    const [providerList, setProviderList] = useState([]);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select Entity Name'
    });
    const [headerCommission, setheaderCommission] = useState({
        UserName: '',
        Operator: '',
        Number: '',
        Amount: ''
    });
    const [commiColumns, setcommiColumns] = useState([]);
    const { latitude, longitude, error } = usePosition();
    const [itemList, setItemList] = useState([]);
    const [CommissionModalOpen, setCommissionModalOpen] = useState(false);
    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: 0,
        OperatorID: 0,
        Status: 'ALL',
        ReportBy: 'Inline',
        ProviderID: 0,
        ExportType: 'Select Download',
        IsRechargeReport: false,
        Criteria: 0,
        CriteriaText: '',
        User: userDefault,
        Latitude: latitude,
        Longitude: longitude
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
    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };
    const showUserList = () => {
        if (isDistRole === true) return true;
        if (isAdminRole === true) return true;
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

        SelectUser(text);
    };
    const StatusCheck = async (ID) => {
        var inputObj = {
            ID: ID
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);
        setResponseMessage('');

        var response = await TRANSACTION_STATUS(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);
        if (!data.Status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        setSeverity('success');
        setIsError(true);

        if (formRef.current) formRef.current.handleSubmit();
    };
    const selectProvider = async () => {
        setLoadingOpen(true);
        const response = await PROVIDER_DROP_DOWN();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setProviderList(data);
    };
    const isValidToRefreshList = (list) => {
        if (IsFetchAllList === false) {
            return true;
        }

        if (list === undefined || list === null) {
            return true;
        }

        if (list.length <= 1) {
            return true;
        }

        return false;
    };

    const SelectUser = async (text) => {
        if (RETAILER === parseInt(currentUserTypeID)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }
        var jsonObj = {
            FirmName: text
        };

        if (isValidToRefreshList(downline) === false) {
            return;
        }

        setLoadingOpen(true);
        var request = JSON.stringify(jsonObj);
        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);

        setLoadingOpen(false);
        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userDefault, ...data];

        setDownline(objWithSelectAll);
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
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Status: formRef.current.values.Status,
            UserID: formRef.current.values.User.UserID,
            ProviderID: formRef.current.values.ProviderID,
            ReportBy: formRef.current.values.ReportBy,
            OperatorID: formRef.current.values.OperatorID,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            Latitude: latitude,
            Longitude: longitude,
            ExportType: formRef.current.values.ExportType
        };

        if (SkipCount <= 0) {
            setHeaderSummary({});
        }
        setTransactions([]);

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);

        setLoadingOpen(true);

        var response = await SEARCH(requestJSON);

        setLoadingOpen(false);

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
            downloadFile(data.ReportPath, 'AEPS Report');
            return;
        }
        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
    };
    const selectOperator = async (ServiceID) => {
        if (
            ServiceID === null ||
            ServiceID === undefined ||
            ServiceID === 0 ||
            ServiceID === '0'
        )
            return;

        var requestObj = {
            ServiceID: ServiceID
        };
        let request = JSON.stringify(requestObj);

        const {
            isAuthorized,
            status,
            data
        } = await OPERATOR_SELECT_BY_SERVICEID(request);

        if (!isAuthorized) LOGOUT();

        if (!status) setIsError(true);

        if (data !== 'Requested operators not found') {
            setOperators(JSON.parse(data));
        }
    };
    const operatorsList = operators.map(({ OperatorID, OperatorName }) => (
        <MenuItem key={OperatorName} value={OperatorID}>
            {OperatorName}
        </MenuItem>
    ));
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (DIST_CODE === parseInt(currentUserTypeID)) setIsDistRole(true);
        if (SUPERDIST_CODE === parseInt(currentUserTypeID)) setIsDistRole(true);
    };

    useEffect(() => {
        CheckUserRole();
        fetchUserList();
        selectProvider();
        selectOperator(AEPS_CODE);
        createColumns();
        refreshForm();
    }, []);

    const Print = (value) => {
        SelectPrintData(value);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);

        setLoadingOpen(true);
        var response = await TRANSACTION_RECEIPT(request);

        if (!response.isAuthorized) {
            LOGOUT();
        }
        setLoadingOpen(false);

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);

            setIsError(true);
            return;
        }

        var d = JSON.parse(response.data);

        setPrintData(d);

        setIsAlertOpen(true);
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
            label: 'Date & Time',
            name: 'DOC'
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'Entity Name',
                name: 'FirmName'
            });
        }

        columns.push({
            label: 'Mobile Number',
            name: 'SenderNumber'
        });
        columns.push({
            label: 'Transaction Type',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Aadhaar Number',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Op Bal(₹)',
            name: 'OpeningBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Amount(₹)',
            name: 'Amount',
            numeric: true,
            bold: true
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'CommissionDetails(₹)',
                name: 'RetailerComm',
                textwrapwidth: 60,
                hiddenbutton: true,
                callback: (row) => {
                    selectCommission(row);
                }
            });
        }
        columns.push({
            label: 'Commission(₹)',
            name: 'RetailerComm',
            numeric: true
        });


        columns.push({
            label: 'TDS',
            name: 'TDS'
        });

        columns.push({
            label: 'Net Commission',
            name: 'NetCommission',
            numeric: true
        });

        columns.push({
            label: 'Total Amount (₹)',
            name: 'DebitAmount',

        });

        columns.push({
            label: 'Cl Bal(₹)',
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
            label: 'Operator Ref.ID',
            name: 'ReferenceNo'
        });

        columns.push({
            label: 'Bank Name',
            name: 'BankName'
        });

        columns.push({
            label: 'Transaction Ref.ID',
            name: 'RequestID'
        });

        columns.push({
            label: 'Device Type',
            name: 'DeviceType'
        });

        if (isAdminRole) {
            columns.push({
                label: 'GST',
                name: 'GST'
            });
            columns.push({
                label: 'DistGST',
                name: 'DistributerGST'
            });
            columns.push({
                label: 'DistTDS',
                name: 'DistributerTDS'
            });
            columns.push({
                label: 'SuperGST',
                name: 'SuperDistributerGST'
            });
            columns.push({
                label: 'SuperTDS',
                name: 'SuperDistributerTDS'
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
            },
            {
                Title: 'Check',
                disabledOn: ['FAILURE', 'SUCCESS'],
                statusIndex: statusIndex,
                Callback: (values) => {
                    StatusCheck(values[0]);
                }
            }
        ];

        columns.splice(0, 0, {
            label: 'Action',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });
        setMainColumns(columns);
    };
    const selectCommission = async (row) => {
        setItemList([]);
        setheaderCommission({
            UserName: row[5],
            Operator: row[6],
            Number: row[8],
            Amount: row[10]
        });

        let jsonObj = {
            TransactionMasterID: row[2],
            UserID: row[3]
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(false);

        createCommissionColumns();
        const response = await SELECT_ROLE_COMMISSION(request);
        setLoadingOpen(false);

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

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + ' date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        // setTransactions([]);
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

    const AEPSReport = (
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
                    SearchText,
                    column,
                    direction
                );
            }}
        />
    );
    const OnChangeCriteria = (value) => {
        if (value === undefined || value <= 0) {
            setIsDisplayCriteria(false);
            setCriteriaTextLabel('');
            return;
        }
        setCriteriaTextLabel(value);
        setIsDisplayCriteria(true);
    };
    const loadingProgress = loadingOpen && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const AEPSForm = ({ setFieldValue, handleSubmit }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'AEPS Report'}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Status"
                                    name="Status"
                                    title="Status"
                                >
                                    {StatusItems}
                                </MaterialSelect>
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="FromDate"
                                    label="From Date"
                                    maxDate={new Date()}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialDatePickerField
                                    name="ToDate"
                                    label="To Date"
                                    maxDate={new Date()}
                                />
                            </Grid>

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Transaction Type"
                                    name="OperatorID"
                                    title="Operator"
                                >
                                    <MenuItem key="0" value={0}>
                                        Transaction Type
                                    </MenuItem>
                                    {operatorsList}
                                </MaterialSelect>
                            </Grid>

                            {showUserList() ? (
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <Field
                                        component={AsyncAutoComplete}
                                        defaultValue={userDefault}
                                        FieldName="User"
                                        name="User"
                                        SubFieldName="UserID"
                                        options={downline}
                                        getOptionLabel={(option) =>
                                            option.FirmName
                                        }
                                        textFieldProps={{
                                            label: 'Select Entity Name'
                                        }}
                                        onSearchCallback={(text) => {
                                            callSearch(text);
                                        }}
                                        onCustomCallback={(name, value) => {
                                            setUserDefault({
                                                UserID: value.UserID,
                                                FirmName: value.FirmName
                                            });
                                        }}
                                    />
                                </Grid>
                            ) : null}



                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="Selection Criteria"
                                    name="Criteria"
                                    title="criteria"
                                    onCustomCallback={(name, value) => {
                                        OnChangeCriteria(value);
                                    }}
                                >
                                    <MenuItem key={0} value={0}>
                                        Selection Criteria
                                    </MenuItem>
                                    <MenuItem key={1} value={'Transaction Ref ID'}>
                                        Transaction Ref ID
                                    </MenuItem>

                                </MaterialSelect>
                            </Grid>

                            {!IsDisplayCriteria ? null : (
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialTextField
                                        label={CriteriaTextLabel}
                                        name="CriteriaText"
                                        type="text"
                                        placeholder={
                                            'Enter ' + CriteriaTextLabel
                                        }
                                        length={50}
                                    />
                                </Grid>
                            )}

                            <Grid item xs={12} sm={12} md={2} lg={2}>
                                <MaterialSelect
                                    label="ExportType"
                                    name="ExportType"
                                    title="ExportType"
                                >
                                    {ExportTypeItems}
                                </MaterialSelect>
                            </Grid>
                            <Grid item xs={12} sm={12} md={1} lg={1}>
                                <PrimaryButton
                                    type="submit"
                                    title="Search"
                                    className={classes.CustomPrimaryButtonStyle}
                                    disabled={loadingOpen}
                                >
                                    Search
                                </PrimaryButton>
                                {loadingProgress}
                            </Grid>
                        </Grid>
                    </CustomAccordion>
                </Grid>
            </Form>
        );
    };
    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={customContainerCallback}
        >
            <Formik
                initialValues={ModelObj}
                innerRef={formRef}
                validationSchema={validationSchema}
                onSubmit={handleSubmit}
                resetForm={ModelObj}
            >
                {AEPSForm}
            </Formik>
            <HeaderSummary headerSummary={headerSummary} />

            {AEPSReport}

            {isAlertOpen ? (
                <AEPSPrint
                    isOpen={isAlertOpen}
                    title="AEPS receipt"
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
                <CustomModal
                    isOpen={CommissionModalOpen}
                    title="Upline Commission"
                    width="75%"
                    onDismissCallback={(value) => {
                        setCommissionModalOpen(value);
                    }}
                >
                    <Grid item xs={12}>
                        <HeaderCommission headerCommission={headerCommission} />
                        <ReactTable
                            data={itemList}
                            columns={commiColumns}
                            headerToolbar={false}
                            footerToolbar={false}
                        />
                    </Grid>
                </CustomModal>
            ) : null}
        </CustomContainer>
    );
};
