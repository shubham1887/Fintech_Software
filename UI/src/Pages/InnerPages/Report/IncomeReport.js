import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';

import { SelectIncome as SELECT_INCOME } from 'Components/Api/IncomeAPI';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectRoleWithoutSupport as USER_TYPE_SELECT_WITHOUT_SUPPORT } from 'Components/Api/UserTypeApi';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { ADMIN } from 'Components/Helper/UsertypeCode';

import {
    PLEASE_SELECT_VALID,
    API_USER_CODE,
    RETAILER_CODE,
    DIST_CODE,
    SUPERDIST_CODE,
    ADMIN_CODE,
    CLIENT_CODE
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { ExportTypeItems } from 'Components/ReportHelper';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const IncomeReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [downLine, setDownLine] = useState([]);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isDistributorRole, setIsDistributorRole] = useState(false);
    const [isSuperDistributorRole, setIsSuperDistributorRole] = useState(false);
    const [userTypeList, setUserTypeList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [openFilter, setopenFilter] = useState(true);
    const [currentUserTypeID, setCurrentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        UserTypeID: 0,
        ExportType: 'Select Download',
        User: userDefault
    });

    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUserTypeID))
            setIsRetailerRole(true);

        if (API_USER_CODE === parseInt(currentUserTypeID))
            setIsRetailerRole(true);

        if (CLIENT_CODE === parseInt(currentUserTypeID))
            setIsRetailerRole(true);
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (DIST_CODE === parseInt(currentUserTypeID))
            setIsDistributorRole(true);
        if (SUPERDIST_CODE === parseInt(currentUserTypeID))
            setIsSuperDistributorRole(true);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            UserID: values.User.UserID,
            ExportType: values.ExportType
        };
        setTransactions([]);
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_INCOME(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        setSubmitting(false);
        var data = JSON.parse(response.data);
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Income_Report');
            return;
        }
        setTransactions(data.list);
    };

    const selectUserTypes = async () => {
        if (RETAILER_CODE === parseInt(currentUserTypeID)) {
            return;
        }
        if (API_USER_CODE === parseInt(currentUserTypeID)) {
            return;
        }
        if (CLIENT_CODE === parseInt(currentUserTypeID)) {
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

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        CheckUserRole();
        selectUserTypes();
        createColumns();
        refreshForm();
    }, [isAdminRole,isDistributorRole,isSuperDistributorRole]);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });
        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'SuccessAmt',
            name: 'SuccessAmt',
            numeric: true
        });
        columns.push({
            label: 'SuccessCount',
            name: 'SuccessCount',
            numeric: true
        });

        columns.push({
            label: 'RCommission',
            name: 'RCommission',
            numeric: true
        });

        if (isAdminRole === true || isDistributorRole === true || isSuperDistributorRole === true) {
            columns.push({
                label: 'DCommission',
                name: 'DCommission',
                numeric: true
            });
        }
        if (isAdminRole === true || isSuperDistributorRole === true) {

            columns.push({
                label: 'SCommission',
                name: 'SCommission',
                numeric: true
            });
        }
        if (isAdminRole === true ) {

            columns.push({
                label: 'AdminCommission',
                name: 'AdminCommission',
                numeric: true
            });
            columns.push({
                label: 'NetProfit',
                name: 'NetProfit',
                numeric: true
            });
        }
        setMainColumns(columns);
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
                                        title={'Income report'}
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
                                            {!isRetailerRole ? (
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
                                                        <MenuItem
                                                            key="0"
                                                            value="0"
                                                        >
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
                                                        defaultValue={
                                                            userDefault
                                                        }
                                                        FieldName="User"
                                                        name="User"
                                                        SubFieldName="UserID"
                                                        options={downLine}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
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
                                    </CustomAccordion>
                                </Paper>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <ReactTable
                data={transactions}
                columns={mainColumns}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                }}
                count={totalCount}
                rowsPerPage={10}
            />
        </CustomContainer>
    );
};
