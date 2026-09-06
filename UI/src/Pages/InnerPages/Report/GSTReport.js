import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';

import { SelectGSTDetailReportForAdmin as SELECT_GST } from 'Components/Api/TaxApi';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, Paper, MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { ADMIN } from 'Components/Helper/UsertypeCode';
import {
    GET_Month_FOR_GST,
    GET_YEAR_FOR_GST
} from 'Components/Helper/Messages';
import {
    PLEASE_SELECT_VALID,
    API_USER_CODE,
    RETAILER_CODE,
    ADMIN_CODE
} from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

const GSTReport = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [downLine, setDownLine] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isAdmin, setIsAdminRole] = useState(false);
    const [openFilter, setopenFilter] = useState(true);
    const [currentUserTypeID, setCurrentUserTypeID] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'Select User'
    });
    const [Months] = useState([
        { Key: 1, Value: 'January' },
        { Key: 2, Value: 'February' },
        { Key: 3, Value: 'March' },
        { Key: 4, Value: 'April' },
        { Key: 5, Value: 'May' },
        { Key: 6, Value: 'June' },
        { Key: 7, Value: 'July' },
        { Key: 8, Value: 'August' },
        { Key: 9, Value: 'September' },
        { Key: 10, Value: 'October' },
        { Key: 11, Value: 'November' },
        { Key: 12, Value: 'December' }
    ]);
    const [Years, setYears] = useState([]);

    const GetYears = () => {
        var currentYear = new Date().getFullYear();
        var yearList = [];
        for (var year = currentYear; year >= 2000; year--) {
            yearList.push({ Key: year });
        }
        setYears(yearList);
    };

    const [ModelObj] = useState({
        Month: GET_Month_FOR_GST(),
        Year: GET_YEAR_FOR_GST(),
        IsExcelDownload: false,
        User: userDefault
    });
    const MonthItems = Months.map(({ Key, Value }, index) => (
        <MenuItem key={index} value={Key}>
            {Value}
        </MenuItem>
    ));
    const YearItems = Years.map(({ Key }, index) => (
        <MenuItem key={index} value={Key}>
            {Key}
        </MenuItem>
    ));
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUserTypeID))
            setIsRetailerRole(true);

        if (API_USER_CODE === parseInt(currentUserTypeID))
            setIsRetailerRole(true);
        if (ADMIN_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            Month: values.Month,
            Year: values.Year,
            UserID: values.User.UserID,
            IsExcelDownload: values.IsExcelDownload
        };

        setTransactions([]);

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_GST(requestObj);
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
        if (data.IsExcelDownload) {
            formRef.current.values.IsExcelDownload = false;
            downloadFile(data.ReportPath, 'GST Report');
        }
        setTransactions(data.list);
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

        Month: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'month')
            .required(PLEASE_SELECT_VALID + 'month'),
        Year: Yup.string()
            .nullable(PLEASE_SELECT_VALID + 'year')
            .required(PLEASE_SELECT_VALID + 'year')
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
            FirmName: text
        };
        selectUsers(jsonObj);
    };

    const selectUsers = async (values) => {
        if (RETAILER_CODE === parseInt(currentUserTypeID)) {
            let objWithSelectAll = [userDefault];
            setDownLine(objWithSelectAll);
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

        setDownLine(objWithSelectAll);
    };

    useEffect(() => {
        CheckUserRole();
        GetYears();
        createColumns();
    }, []);

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'ID',
            srno: true
        });

        if (ADMIN === parseInt(currentUserTypeID)) {
            columns.push({
                label: 'Firm Name',
                name: 'FirmName'
            });
            columns.push({
                label: 'User',
                name: 'UserName'
            });
            columns.push({
                label: 'UserType',
                name: 'UserType'
            });
            columns.push({
                label: 'Mobile Number',
                name: 'MobileNumber'
            });
            columns.push({
                label: 'GST Number',
                name: 'GSTNumber'
            });

            columns.push({
                label: 'State',
                name: 'StateName'
            });
        }
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
            label: 'Commission',
            name: 'Commission',
            numeric: true
        });
        columns.push({
            label: 'GST',
            name: 'GST',
            numeric: true
        });

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
                render={({ values, setFieldValue, handleSubmit }) => {
                    return (
                        <Form style={{ marginBottom: '15px' }}>
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'GST Report'}
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
                                                <MaterialSelect
                                                    label="Month"
                                                    name="Month"
                                                    title="Month"
                                                >
                                                    {MonthItems}
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
                                                    label="Year"
                                                    name="Year"
                                                    title="Year"
                                                >
                                                    {YearItems}
                                                </MaterialSelect>
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
                                                md={2}
                                                lg={2}
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
                count={totalCount}
                rowsPerPage={100}
            />
        </CustomContainer>
    );
};

export default GSTReport;
