import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserwiseSale as SELECT_USER_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import { Select as WALLET_SELECT } from 'Components/Api/WalletApi';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';

import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectReportByUserID } from '../Report/SelectReportByUserID';
import {
    PLEASE_SELECT_VALID,
    API_USER_CODE,
    RETAILER_CODE,
    ADMIN_CODE,
    DIST_CODE
} from 'Components/Helper/Messages';
import { HeaderUserwiseSaleSummary } from 'Components/SummaryCards/HeaderUserwiseSaleSummary';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { CustomModalWithScroll } from 'Components/Modal/CustomModalWithScroll';
import { ExportTypeItems } from 'Components/ReportHelper';
const CustomContainer = HOCLoader(Container);

const UserwiseSale = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [downLine, setDownLine] = useState([]);
    const [isAdminRole, setIsAdminRole] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [wallets, setWallets] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [userTypeList, setUserTypeList] = useState([]);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [headerSummary, setHeaderSummary] = useState({});
    const [currentPage, setCurrentPage] = useState(0);

    const [SearchText, setSearchText] = useState('');
    const [isOpenReport, setIsOpenReport] = useState(false);
    const [ReportData, setReportData] = useState({});
    var currentUserTypeId = localStorage.getItem('UserTypeID');

    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUserTypeId))
            setIsRetailerRole(true);

        if (API_USER_CODE === parseInt(currentUserTypeId))
            setIsRetailerRole(true);

        if (ADMIN_CODE === parseInt(currentUserTypeId)) setIsAdminRole(true);
        if (DIST_CODE === parseInt(currentUserTypeId)) setIsAdminRole(true);
    };
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [formObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        User: userDefault,
        WalletID: 0,
        UserTypeID: 0,
        ExportType: 'Select Download'
    });

    const selectUserTypes = async () => {
        if (RETAILER_CODE === parseInt(currentUserTypeId)) {
            return;
        }
        if (API_USER_CODE === parseInt(currentUserTypeId)) {
            return;
        }
        const response = await USER_TYPE_SELECT_WITHOUT_SUPPORT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
        }

        var data = JSON.parse(response.data);
        setUserTypeList(data);
    };
    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
        </MenuItem>
    ));
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
        }

        var data = JSON.parse(response.data);
        setWallets(data);
    };
    const WalletList = wallets.map((item) => (
        <MenuItem key={item.WalletID} value={item.WalletID}>
            {item.Name}
        </MenuItem>
    ));

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
        selectUsers(jsonObj);
    };

    const selectUsers = async (values) => {
        var request = JSON.stringify(values);
        const {
            isAuthorized,
            status,
            data
        } = await RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN(request);
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        let objWithSelectAll = [userDefault, ...d];

        setDownLine(objWithSelectAll);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            WalletID: formRef.current.values.WalletID,
            UserTypeID: formRef.current.values.UserTypeID,
            UserID: formRef.current.values.User.UserID,
            ExportType: formRef.current.values.ExportType,
            IsPDFDownload: formRef.current.values.IsPDFDownload
        };

        if (SkipCount <= 0) {
            setHeaderSummary({});
        }
        //setItemList([]);
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_USER_WISE_SALE(requestObj);

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
            downloadFile(data.ReportPath, 'User Sale');
            return;
        }
        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
        }

        setItemList(data.list);
        setTotalCount(data.Count);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        //setItemList([]);
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
    }

    const validationSchema = Yup.object({
        FromDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'from date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'from date')
            .typeError(PLEASE_SELECT_VALID + 'from date'),

        ToDate: Yup.date()
            .nullable(PLEASE_SELECT_VALID + 'date')
            .required(PLEASE_SELECT_VALID + 'date')
            .max(new Date(), PLEASE_SELECT_VALID + 'to date')
            .typeError(PLEASE_SELECT_VALID + 'to date'),

        WalletID: Yup.string()
            .notOneOf(['0'], PLEASE_SELECT_VALID + 'wallet')
            .required(PLEASE_SELECT_VALID + 'wallet')
    });

    useEffect(() => {
        CheckUserRole();
        selectWallets();
        selectUserTypes();
        createColumns();
    }, []);

    const SelectReport = async (row) => {
        setReportData({ UserID: row[1], FirmName: row[2] });
        setIsOpenReport(true);
    };
    const OpenReportModalCallback = (value) => {
        setIsOpenReport(value);
    };

    const OpenReportModal = isOpenReport ? (
        <CustomModalWithScroll
            isOpen={isOpenReport}
            title={ReportData.FirmName}
            width="75%"
            onDismissCallback={OpenReportModalCallback}
        >
            <SelectReportByUserID
                ReportData={ReportData}
                onDismissCallback={OpenReportModalCallback}
            />
        </CustomModalWithScroll>
    ) : null;

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'UserMobile',
            name: 'UserMobile',
            hiddenbutton: true,
            callback: (row) => {
                SelectReport(row);
            }
        });

        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'Wallet',
            name: 'WalletName'
        });

        columns.push({
            label: 'OpeningBal',
            name: 'OpeningBal',
            numeric: true
        });
        columns.push({
            label: 'ClosingBal',
            name: 'ClosingBalance',
            numeric: true
        });
        columns.push({
            label: 'Sale',
            name: 'Sale',
            numeric: true
        });

        columns.push({
            label: 'R.Commi',
            name: 'RetailerCommission',
            numeric: true
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'D.Commi',
                name: 'DistributerCommission',
                numeric: true
            });
            columns.push({
                label: 'S.Commi',
                name: 'SuperDistributerCommission',
                numeric: true
            });
        }

        columns.push({
            label: 'Surcharge',
            name: 'Surcharge',
            numeric: true
        });

        columns.push({
            label: 'Purchase',
            name: 'Purchase',
            numeric: true
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'TransferAmount',
                name: 'TransferAmount',
                numeric: true
            });
        }
        columns.push({
            label: 'ReverseAmountDr',
            name: 'ReverseAmountDr',
            numeric: true
        });
        if (isAdminRole === true) {
            columns.push({
                label: 'ReverseAmountCr',
                name: 'ReverseAmountCr',
                numeric: true
            });
        }
        columns.push({
            label: 'S.count',
            name: 'SuccessCount',
            numeric: true
        });
        columns.push({
            label: 'F.count',
            name: 'FailCount',
            numeric: true
        });

        setMainColumns(columns);
    };
    const UserWiseSaleReport = (
        <ReactTable
            data={itemList}
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
                    SearchText,
                    column,
                    direction
                );
            }}
        />
    );
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
                initialValues={formObj}
                validationSchema={validationSchema}
                onSubmit={submithandle}
                resetForm={formObj}
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper className={classes.PaperStyle}>
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
                                                    Select Wallet
                                                </MenuItem>
                                                {WalletList}
                                            </MaterialSelect>
                                        </Grid>

                                        {isAdminRole ? (
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
                                                >
                                                    <MenuItem key="0" value="0">
                                                        Select Role
                                                    </MenuItem>
                                                    {UserTypeList}
                                                </MaterialSelect>
                                            </Grid>
                                        ) : null}

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
                                                    options={downLine}
                                                    getOptionLabel={(option) =>
                                                        option.FirmName
                                                    }
                                                    textFieldProps={{
                                                        label: 'Select User'
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
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            <HeaderUserwiseSaleSummary headerUserwiseSale={headerSummary} />
            <h3 className={classes.PaperHead_h2}>UserWiseSale</h3>
            {UserWiseSaleReport}
            {OpenReportModal}
        </CustomContainer>
    );
};

export default UserwiseSale;
