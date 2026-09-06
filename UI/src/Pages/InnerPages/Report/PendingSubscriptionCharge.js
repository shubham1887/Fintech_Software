import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { Container, Grid, Box, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectPendingChargeReport as SELECT_PENDING_SUBSCRIPTION_CHARGE } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { SelectByRole as SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import excel from 'Assets/Images/excel.svg';
import Icon from '@material-ui/core/Icon';
import {
    API_USER_CODE,
    RETAILER_CODE,
    CLIENT_CODE,
    SUPERDIST_CODE,
    ADMIN_CODE,
    DIST_CODE
} from 'Components/Helper/Messages';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const PendingSubcriptionCharge = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);
    const [isSuperDistRole, setIsSuperDistRole] = useState(false);
    const [openFilter, setopenFilter] = useState(true);
    const [SearchText, setSearchText] = useState('');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [currentPage, setCurrentPage] = useState(0);
    const [userTypeList, setUserTypeList] = useState([]);
    const [UsersList, setUsers] = useState([]);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        User: userDefault,
        IsExcelDownload: false
    });

    const SubmitHandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setTransactions([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
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
            UserID: formRef.current.values.User.UserID,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_PENDING_SUBSCRIPTION_CHARGE(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);

        if (data.IsExcelDownload)
            downloadFile(data.ReportPath, 'PendingSubscription Charge Report');
        if (SkipCount <= 0) setTotalCount(data.Count);
        setTransactions(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.body.removeChild(a);
    }

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (SUPERDIST_CODE === parseInt(currentUsertypeid))
            setIsSuperDistRole(true);
        if (DIST_CODE === parseInt(currentUsertypeid)) setIsDistRole(true);
    };

    const UserTypeList = userTypeList.map((item) => (
        <MenuItem key={item.UserTypeID} value={item.UserTypeID}>
            {item.UserType}
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
        const { isAuthorized, status, data } = await SELECT_FOR_DROPDOWN(
            request
        );
        if (!isAuthorized) LOGOUT();
        if (!status) return;
        var d = JSON.parse(data);
        let objWithSelectAll = [userDefault, ...d];

        setUsers(objWithSelectAll);
    };

    const selectUserTypes = async () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            let objWithSelectAll = [userDefault];
            setUsers(objWithSelectAll);
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

    const refreshForm = () => {
        if (!formRef.current) return;

        formRef.current.handleSubmit();
    };

    useEffect(() => {
        createColumns();
        CheckUserRole();
        selectUserTypes();
        refreshForm();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ServiceAndUserwiseSubscriptionID',
            srno: true
        });

        columns.push({
            label: 'UserID',
            name: 'UserID',
            hide: true
        });

        columns.push({
            label: 'Role',
            name: 'UserType'
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

        if (isAdminRole === true) {
            columns.push({
                label: 'Original Charge',
                name: 'OriginalCharge',
                editable: true
            });

            columns.push({
                label: 'Discounted Charge',
                name: 'Charge',
                editable: true
            });
        }

        columns.push({
            label: 'Plan',
            name: 'PlanName'
        });

        columns.push({
            label: 'LoginID',
            name: 'LoginID'
        });

        columns.push({
            label: 'Status',
            name: 'Status'
        });

        setMainColumns(columns);
    };

    const loadingProgress = loading && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const PendingSubscriptionChargeForm = ({
        values,
        setFieldValue,
        handleSubmit
    }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'PendingSubscriptionCharge Report'}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        {(isAdminRole || isSuperDistRole || isDistRole) && (
                            <Grid container spacing={2}>
                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <MaterialSelect
                                        label="Select Role"
                                        name="UserTypeID"
                                        title="Select Role"
                                    >
                                        <MenuItem key="0" value="0">
                                            Select Role
                                        </MenuItem>
                                        {UserTypeList}
                                    </MaterialSelect>
                                </Grid>

                                <Grid item xs={12} sm={12} md={2} lg={2}>
                                    <Field
                                        component={AsyncAutoComplete}
                                        defaultValue={userDefault}
                                        FieldName="User"
                                        name="User"
                                        SubFieldName="UserID"
                                        options={UsersList}
                                        getOptionLabel={(option) =>
                                            option.FirmName
                                        }
                                        textFieldProps={{
                                            label: 'Select User'
                                        }}
                                        onSearchCallback={(text) => {
                                            callSearch(text, values.UserTypeID);
                                        }}
                                        onCustomCallback={(name, value) => {
                                            setUserDefault({
                                                UserID: value.UserID,
                                                FirmName: value.FirmName
                                            });
                                        }}
                                    />
                                </Grid>

                                <Grid item xs={12} sm={12} md={1} lg={1}>
                                    <Box style={{ textAlign: 'center' }}>
                                        <Icon
                                            className={
                                                classes.CustomImageButtonStyle
                                            }
                                            disabled={loading}
                                            classes={{ root: classes.iconRoot }}
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
                                                className={classes.imageIcon}
                                                src={excel}
                                            />
                                        </Icon>
                                    </Box>
                                </Grid>
                                <Grid item xs={12} sm={12} md={1} lg={1}>
                                    <PrimaryButton
                                        type="submit"
                                        title="SEARCH"
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
                                    {loadingProgress}
                                </Grid>
                            </Grid>
                        )}
                    </CustomAccordion>
                </Grid>
            </Form>
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
                initialValues={ModelObj}
                innerRef={formRef}
                onSubmit={SubmitHandle}
                resetForm={ModelObj}
            >
                {PendingSubscriptionChargeForm}
            </Formik>

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
        </CustomContainer>
    );
};
