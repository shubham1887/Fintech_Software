import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectPaymentReport as SELECT_SUBSCRIPTION_PAYMENT } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { ApprovalByAdmin } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { UPDATE_STATUS } from 'Components/Api/ServiceSubscriptionChargeAPI';

import { SelectOnlyParentUserForDropdown as PARENT_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectSubscriptionAllowedServices as SERVICE_SELECT } from 'Components/Api/ServiceApi';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    ADMIN_CODE,
    SUPPORT_CODE,
    API_USER_CODE,
    SUPERDIST_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { indianFormatNumber } from 'Components/utils';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const SubscriptionPaymentReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [Services, setServices] = useState([]);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [userList, setUserList] = useState([]);
    const [ParentUserList, setParentUserList] = useState([]);
    const [headerSummary, setHeaderSummary] = useState({});
    const [IsFetchAllList, setisFetchAllList] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [severity, setSeverity] = useState('success');
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [openFilter, setopenFilter] = useState(true);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isSuperDistRole, setIsSuperDistRole] = useState(false);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });
    const [parentDefault, setParentDefault] = useState({
        ParentID: 0,
        ParentFirmName: 'Select Parent User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date().addDays(-30),
        ServiceID: 0,
        User: userDefault,
        IsExcelDownload: false,
        SubscriptionType: 'ALL',
        DownlineUser: parentDefault
    });
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
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUserList(objWithSelectAll);
            return;
        }

        var jsonObj = {
            FirmName: text
        };

        if (isValidToRefreshList(userList) === false) {
            return;
        }

        setLoading(true);
        var request = JSON.stringify(jsonObj);
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
            Search: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.User.UserID,
            ServiceID: formRef.current.values.ServiceID,
            IsExcelDownload: formRef.current.values.IsExcelDownload,
            SubscriptionType: formRef.current.values.SubscriptionType,
            ParentID: formRef.current.values.DownlineUser.ParentID
        };
        if (SkipCount <= 0) {
            setHeaderSummary({});
        }

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);
        createColumns();
        var response = await SELECT_SUBSCRIPTION_PAYMENT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(
                data.ReportPath,
                'Service Subscription Payment Report'
            );
        }

        if (SkipCount <= 0) {
            setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }

        setTransactions(data.list);
    };
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);

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
            .typeError(PLEASE_SELECT_VALID + 'to date')
    });

    const ServiceItems = Services.map(({ ServiceID, ServiceName }, index) => (
        <MenuItem key={index} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));
    const selectServices = async () => {
        const response = await SERVICE_SELECT();
        if (!response.isAuthorized) LOGOUT();

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
            return;
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
            return;
        }

        if (ADMIN_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
            SelectParentUser();
            return;
        }
        if (SUPERDIST_CODE === parseInt(currentUsertypeid)) {
            setIsSuperDistRole(true);
            SelectParentUser();
            return;
        }

        if (SUPPORT_CODE === parseInt(currentUsertypeid)) {
            setIsAdminRole(true);
            SelectParentUser();
        }

        SelectParentUser();
    };

    const statusList = [
        {
            ID: 'APPROVE',
            Type: 'APPROVE'
        },
        {
            ID: 'REJECT',
            Type: 'REJECT'
        }
    ];

    useEffect(() => {
        CheckUserRole();
        selectServices();
    }, [isAdminRole]);

    const AdminApproval = async (item, value) => {
        var jsonObj = {
            UserServiceSubscriptionChargeID: item,
            IsAdminApproved: value
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoading(true);
        const response = await ApprovalByAdmin(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
    };

    const onUpdateClick = async (row) => {
        let requestObj = {
            ServiceAndUserwiseSubscriptionID: row[0],
            Status: row[11]
        };

        var jsonReq = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await UPDATE_STATUS(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceAndUserwiseSubscriptionID'
        });
        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'ServiceID',
            name: 'ServiceID',
            hide: true
        });
        columns.push({
            label: 'Payment Date',
            name: 'PaymentDate'
        });
        columns.push({
            label: 'EndDate',
            name: 'EndDate'
        });

        columns.push({
            label: 'User',
            name: 'UserName'
        });

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });

        columns.push({
            label: 'Type',
            name: 'SubscriptionType'
        });

        columns.push({
            label: 'Charge',
            name: 'Charge'
        });
        columns.push({
            label: 'LoginID',
            name: 'LoginID'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'Action',
            name: 'Status',
            dropdown: true,
            list: statusList
        });
        columns.push({
            label: 'Update',
            name: 'ServiceAndUserwiseSubscriptionID',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                onUpdateClick(row);
            }
        });
        setMainColumns(columns);
    };
    const callSearchParent = async (text) => {
        if (
            text === undefined ||
            text === null ||
            text === '' ||
            text.length <= 0 ||
            text === parentDefault.ParentFirmName
        )
            return;

        SelectParentUser(text);
    };

    const SelectParentUser = async (values) => {
        setLoading(true);
        var request = JSON.stringify(values);
        setLoading(false);
        const userResponse = await PARENT_SELECT_FOR_DROPDOWN(request);

        if (!userResponse.isAuthorized) LOGOUT();

        if (!userResponse.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
        var data = JSON.parse(userResponse.data);

        const transformedData = data.map((item) => ({
            ParentID: item.UserID,
            ParentFirmName: item.FirmName
        }));

        let objWithSelectAll = [parentDefault, ...transformedData];

        setParentUserList(objWithSelectAll);
    };

    const AmountAndCount = ({ name, amount, count, summayClass }) => {
        return (
            <h2 className={classes.recharge_h2}>
                {name} :
                <span className={summayClass}>
                    {indianFormatNumber(Number(amount || 0.0))}
                    {' ['}
                    {count || 0} {']'}
                </span>
            </h2>
        );
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
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={
                                            'Service Subscription Payment Report'
                                        }
                                        open={openFilter}
                                        setOpen={(value) =>
                                            setopenFilter(value)
                                        }
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
                                                    label="Service"
                                                    name="ServiceID"
                                                    title="Service"
                                                >
                                                    <MenuItem key="0" value="0">
                                                        All Service
                                                    </MenuItem>
                                                    {ServiceItems}
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
                                                    label="Subscription Type"
                                                    name="SubscriptionType"
                                                    title="Subscription Type"
                                                >
                                                    <MenuItem
                                                        key="ALL"
                                                        value="ALL"
                                                    >
                                                        ALL
                                                    </MenuItem>

                                                    <MenuItem
                                                        key="Monthly"
                                                        value="Monthly"
                                                    >
                                                        Monthly
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="Quarterly"
                                                        value="Quarterly"
                                                    >
                                                        Quarterly
                                                    </MenuItem>
                                                    <MenuItem
                                                        key="Yearly"
                                                        value="Yearly"
                                                    >
                                                        Yearly
                                                    </MenuItem>
                                                </MaterialSelect>
                                            </Grid>
                                            {!isRetailerRole && (
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
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={userList}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
                                                        textFieldProps={{
                                                            label: 'User'
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
                                            )}

                                            {(isAdminRole ||
                                                isSuperDistRole) && (
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
                                                        defaultValue={
                                                            parentDefault
                                                        }
                                                        FieldName="DownlineUser"
                                                        name="DownlineUser"
                                                        SubFieldName="ParentID"
                                                        options={ParentUserList}
                                                        getOptionLabel={(
                                                            option
                                                        ) =>
                                                            option.ParentFirmName
                                                        }
                                                        textFieldProps={{
                                                            label:
                                                                'Select Parent User'
                                                        }}
                                                        onSearchCallback={(
                                                            text
                                                        ) => {
                                                            callSearchParent(
                                                                text
                                                            );
                                                        }}
                                                        onCustomCallback={(
                                                            name,
                                                            value
                                                        ) => {
                                                            setParentDefault({
                                                                ParentID:
                                                                    value.ParentID,
                                                                ParentFirmName:
                                                                    value.ParentFirmName
                                                            });
                                                        }}
                                                    />
                                                </Grid>
                                            )}

                                            <Grid
                                                item
                                                xs={12}
                                                sm={12}
                                                md={1}
                                                lg={1}
                                            >
                                                <Box
                                                    style={{
                                                        textAlign: 'center'
                                                    }}
                                                >
                                                    <Icon
                                                        className={
                                                            classes.CustomImageButtonStyle
                                                        }
                                                        disabled={loading}
                                                        classes={{
                                                            root:
                                                                classes.iconRoot
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
                                                    onClick={(e) => {
                                                        setFieldValue(
                                                            'IsExcelDownload',
                                                            false
                                                        );
                                                        handleSubmit(e);
                                                    }}
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
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />
            {headerSummary != null && (
                <Box>
                    <Paper
                        style={{
                            paddingTop: 12,
                            marginBottom: 16,
                            paddingLeft: 10
                        }}
                    >
                        <Grid container spacing={1}>
                            <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                                <AmountAndCount
                                    summayClass={classes.Success_h3}
                                    name={'Total Charge'}
                                    amount={headerSummary.TotalCharge}
                                    count={headerSummary.TotalCount}
                                />
                            </Grid>
                        </Grid>
                    </Paper>
                </Box>
            )}
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
                    callSelectData(TakeCount, currentPage, SearchText, '', '');
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
        </CustomContainer>
    );
};

export default SubscriptionPaymentReport;
