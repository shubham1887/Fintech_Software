import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectDownlineForLedger as SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
 
import { Select as SELECT_LEDGER } from 'Components/Api/LedgerApi';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { Container, Grid, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import {
    API_USER_CODE,
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    CLIENT_CODE
} from 'Components/Helper/Messages';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { ExportTypeItems } from 'Components/ReportHelper';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const LedgerReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [headertransactions, setHeaderTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [HeadermainColumns, setHeaderMainColumns] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [userList, setUserList] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [SortDirection, setSortDirection] = useState('');
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [IsDisplayCriteria, setIsDisplayCriteria] = useState(false);
    const [CriteriaTextLabel, setCriteriaTextLabel] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailer, setIsRetailerRole] = useState(false);
    const [wallets, setWallets] = useState([]);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [openFilter, setopenFilter] = useState(true);
    const [downline, setDownline] = useState([]);
    const [isheaderForOwnUser, setIsHeaderForOwnUser] = useState(true);

    const [userDefault, setUserDefault] = useState({
        UserID: -2,
        FirmName: 'Select User'
    });

    const [userAllDefault] = useState({
        UserID: -1,
        FirmName: 'All'
    });

    const [userOwnDefault] = useState({
        UserID: 0,
        FirmName: 'Own'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ExportType: 'Select Download',
        IsPDFDownload: false,
        IsExcelDownload: false,
        TopRecords: TakeCount,
        WalletID: 0,
        UserTypeID: 0,
        Criteria: 0,
        CriteriaText: '',
        User: userDefault
    });

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        setSortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            WalletID: formRef.current.values.WalletID,
            UserID: formRef.current.values.User.UserID,
            UserTypeID: formRef.current.values.UserTypeID,
            ExportType: formRef.current.values.ExportType,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            IsPDFDownload: formRef.current.values.IsPDFDownload,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        if (formRef.current.values.IsExcelDownload === false) setTransactions();

        setIsHeaderForOwnUser(true);
        if (requestObj.UserID === -1) setIsHeaderForOwnUser(false);
        var requestJSON = JSON.stringify(requestObj);

        setIsError(false);
        setLoading(true);

        if (SkipCount <= 0) {
            setHeaderTransactions([]);
        }
        var response = await SELECT_LEDGER(requestJSON);
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
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'User Ledger');
            return;
        }

        if (SkipCount <= 0) {
            setHeaderTransactions(data.Headerlist);
            setTotalCount(data.Count);
        }
        setTransactions(data.list);
    };
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        createHeaderColumns();
        createColumns();
        await callSelectData(TakeCount, SkipCount, '', '', '');
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
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);
        if (API_USER_CODE === parseInt(currentUsertypeid))
            setIsRetailerRole(true);

        createColumns();
    };
    const validationSchema = Yup.object({
        // User: Yup.object()
        //     .nullable(PLEASE_SELECT_VALID + 'User')
        //     .required(PLEASE_SELECT_VALID + 'User')
        //     .shape({
        //         UserID: Yup.number()
        //             .nullable(PLEASE_SELECT_VALID + 'User')
        //             .min(-1, PLEASE_SELECT_VALID + 'User')
        //             .required(PLEASE_SELECT_VALID + 'User')
        //     }),

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

        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const callSearch = async (text, UserTypeID) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === userDefault.FirmName
        )
            return;

        var jsonObj = {
            FirmName: text,
            UserTypeID: UserTypeID
        };
        SelectUser(jsonObj);
    };

    const SelectUser = async (values) => {
        if (isRetailer === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);

        const userResponse = await SELECT_FOR_DROPDOWN(request);
        setLoading(false);
        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(userResponse.data);

        let objWithSelectAll = [userAllDefault, userOwnDefault, ...data];

        setDownline(objWithSelectAll);
    };

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    const TextDisplay = (setFieldValue) => {
        setFieldValue('CriteriaText', '');
        setIsDisplayCriteria(true);
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
    useEffect(() => {
        refreshForm();
        CheckUserRole();
        selectWallets();
        if (!isRetailer) {
            selectUserTypes();
            SelectUser();
        }
    }, []);

    const selectUserTypes = async () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            return;
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            return;
        }
        if (CLIENT_CODE === parseInt(currentUsertypeid)) {
            return;
        }
        const response = await USER_TYPE_SELECT_WITHOUT_SUPPORT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };

    const createHeaderColumns = () => {
        var columns = [];

        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBal',
            CreditAmount: true
        });

        columns.push({
            label: 'Purchase',
            name: 'ReceiverAmount',
            CreditAmount: true
        });

        columns.push({
            label: 'F.Deducted',
            name: 'ReverseAmountDr',
            CreditAmount: true
        });
        columns.push({
            label: 'Comm',
            name: 'CommissionAmount',
            CreditAmount: true
        });
        columns.push({
            label: 'TDS',
            name: 'TDS',
            CreditAmount: true
        });
        columns.push({
            label: 'Debited',
            name: 'Debited',
            DebitAmount: true
        });
        columns.push({
            label: 'Requested',
            name: 'Requested',
            DebitAmount: true
        });

        if (!isRetailer) {
            columns.push({
                label: 'F.Transfer',
                name: 'TransferAmount',
                DebitAmount: true
            });
        }
        columns.push({
            label: 'PG',
            name: 'PGAmount',
            DebitAmount: true
        });
        columns.push({
            label: 'EC',
            name: 'ECollectionAmount',
            DebitAmount: true
        });
        columns.push({
            label: 'AEPS',
            name: 'AEPSAmount',
            DebitAmount: true
        });
        columns.push({
            label: 'MATM',
            name: 'MATMAmount',
            DebitAmount: true
        });
        columns.push({
            label: 'Pu.Return',
            name: 'ReverseAmountCr',
            DebitAmount: true
        });

        columns.push({
            label: 'Surcharge',
            name: 'Surcharge',
            DebitAmount: true
        });

        columns.push({
            label: 'Compl.Amt',
            name: 'ComplaintAmount',
            DebitAmount: true
        });
        if (isRetailer === true) {
            columns.push({
                label: 'P.Success',
                name: 'PastSuccessAmount',
                DebitAmount: true
            });
        }
        if (isRetailer === true) {
            columns.push({
                label: 'P.Fail',
                name: 'PastFailAmount',
                DebitAmount: true
            });
        }

        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            DebitAmount: true
        });
        columns.push({
            label: 'Exp.Cl.Bal',
            name: 'ExpectedCloseBal',
            DebitAmount: true
        });
        columns.push({
            label: 'Diff',
            name: 'Difference',
            DebitAmount: true
        });

        setHeaderMainColumns(columns);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionDetailID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });

        columns.push({
            label: 'Description',
            name: 'Description',
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'Op.Bal',
            name: 'OpeningBalance',
            numeric: true
        });
        columns.push({
            label: 'ConsumerNumber',
            name: 'ConsumerNumber',
            numeric: true,
            bold: true
        });
        columns.push({
            label: 'Amt',
            name: 'customAmount',
            customAmount: true
        });
        columns.push({
            label: 'Comm',
            name: 'CommissionAmount',
            numeric: true
        });
        columns.push({
            label: 'TDS',
            name: 'TDS',
            numeric: true
        });
        columns.push({
            label: 'Cl.Bal',
            name: 'ClosingBalance',
            numeric: true
        });
        if (!isRetailer) {
            columns.push({
                label: 'Impact By',
                name: 'AssignedUserName'
            });
        }
        columns.push({
            label: 'ReferenceID ',
            name: 'ReferenceID'
        });
        columns.push({
            label: 'TransactionID',
            name: 'TransactionID'
        });
        columns.push({
            label: 'RechargeDate',
            name: 'RechargeDate'
        });

        setMainColumns(columns);
    };

    const OnChangeUserRole = (name, value, setFieldValue) => {
        setDownline([]);
        var jsonObj = {
            FirmName: '',
            UserTypeID: value
        };
        SelectUser(jsonObj);
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
                onSubmit={handleSubmit}
                resetForm={ModelObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <CustomAccordion
                                    title={'User Ledger'}
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
                                                label="Wallet"
                                                name="WalletID"
                                                title="Select Wallet"
                                            >
                                                <MenuItem key="0" value="0">
                                                    All Wallet
                                                </MenuItem>
                                                {wallets.map((item) => (
                                                    <MenuItem
                                                        key={item.WalletID}
                                                        value={item.WalletID}
                                                    >
                                                        {item.Name}
                                                    </MenuItem>
                                                ))}
                                            </MaterialSelect>
                                        </Grid>
                                        {!isRetailer ? (
                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={2}
                                                lg={2}
                                            >
                                                <MaterialSelect
                                                    label="Role"
                                                    name="UserTypeID"
                                                    title="Select Role"
                                                    onCustomCallback={(
                                                        name,
                                                        value
                                                    ) => {
                                                        OnChangeUserRole(
                                                            name,
                                                            value,
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    <MenuItem key="0" value="0">
                                                        All Role
                                                    </MenuItem>
                                                    {userTypeList.map(
                                                        (item) => (
                                                            <MenuItem
                                                                key={
                                                                    item.UserTypeID
                                                                }
                                                                value={
                                                                    item.UserTypeID
                                                                }
                                                            >
                                                                {item.UserType}
                                                            </MenuItem>
                                                        )
                                                    )}
                                                </MaterialSelect>
                                            </Grid>
                                        ) : null}

                                        {!isRetailer ? (
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
                                                    options={downline}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'User'
                                                    }}
                                                    onSearchCallback={(
                                                        text
                                                    ) => {
                                                        callSearch(
                                                            text,
                                                            values.UserTypeID
                                                        );
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
                                        {/* <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={1}
                                            lg={1}
                                        >
                                            <Box
                                                style={{ textAlign: 'center' }}
                                            >
                                                <Icon
                                                    className={
                                                        classes.CustomImageButtonStyle
                                                    }
                                                    disabled={loading}
                                                    classes={{
                                                        root: classes.iconRoot
                                                    }}
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            true
                                                        );
                                                        handleSubmit(e);
                                                    }}
                                                >
                                                    <img
                                                        alt=""
                                                        className={
                                                            classes.imageIcon
                                                        }
                                                        src={excel}
                                                    />
                                                </Icon>
                                            </Box>
                                        </Grid> */}

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
                                                    OnChangeCriteria(value);
                                                }}
                                            >
                                                <MenuItem key={0} value={0}>
                                                    Selection Criteria
                                                </MenuItem>

                                                <MenuItem
                                                    key={1}
                                                    value={'ConsumerNumber'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    ConsumerNumber
                                                </MenuItem>

                                                <MenuItem
                                                    key={2}
                                                    value={'TransactionID'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    TransactionID
                                                </MenuItem>

                                                <MenuItem
                                                    key={2}
                                                    value={'ReferenceID'}
                                                    onClick={() => {
                                                        TextDisplay(
                                                            setFieldValue
                                                        );
                                                    }}
                                                >
                                                    ReferenceID
                                                </MenuItem>
                                            </MaterialSelect>
                                        </Grid>
                                        {IsDisplayCriteria && (
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
            />

            {isheaderForOwnUser ? (
                <ReactTable
                    data={headertransactions}
                    columns={HeadermainColumns}
                    getCallback={({ currentPage, RowsPerPage }) => {
                        setTakeCount(RowsPerPage);
                        setSkipCount(RowsPerPage * currentPage);
                    }}
                    count={totalCount}
                    rowsPerPage={10}
                    headerToolbar={false}
                    footerToolbar={false}
                />
            ) : null}

            <br />

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
                        currentPage,
                        TakeCount,
                        SearchText,
                        column,
                        direction
                    );
                }}
            />
        </CustomContainer>
    );
};
