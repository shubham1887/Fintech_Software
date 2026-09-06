import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Box from '@material-ui/core/Box';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectSale as SELECT_SERVICE_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { Container, Grid, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import ReactTable from 'Components/ReactTable/ReactTable';

import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { ADMIN, RETAILER } from 'Components/Helper/UsertypeCode';

import { PLEASE_SELECT_VALID, SUPPORT_CODE } from 'Components/Helper/Messages';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { ExportTypeItems } from 'Components/ReportHelper';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const ServicewiseSale = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);

    const [downline, setDownline] = useState([]);

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
        FirmName: 'All'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ExportType: 'Select Download',
        IsExcelDownload: false,
        IsPDFDownload: false,
        User: {
            UserID: 0,
            FirmName: 'All'
        }
    });

    const CheckUserRole = () => {
        if (ADMIN === parseInt(currentUserTypeID)) setIsAdminRole(true);
        if (SUPPORT_CODE === parseInt(currentUserTypeID)) setIsAdminRole(true);
        createColumns();
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            UserID: values.User.UserID,
            IsExcelDownload: values.IsExcelDownload,
            ExportType: values.ExportType,
            IsPDFDownload: values.IsPDFDownload
        };

        setTransactions([]);

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SELECT_SERVICE_WISE_SALE(requestObj);
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
            downloadFile(data.ReportPath, 'E-collection Report');
            return;
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
        if (RETAILER === parseInt(currentUserTypeID)) {
            let objWithSelectAll = [userDefault];
            setDownline(objWithSelectAll);
            return;
        }

        setLoading(true);
        var request = JSON.stringify(values);

        const userResponse = await USER_SELECT_FOR_DROPDOWN(request);
        setLoading(false);
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
    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        refreshForm();
    }, []);

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
            label: 'FailAmt',
            name: 'FailAmt',
            numeric: true
        });
        columns.push({
            label: 'FailCount',
            name: 'FailCount',
            numeric: true
        });
        columns.push({
            label: 'RCommission',
            name: 'RCommission',
            numeric: true
        });
        if (ADMIN === parseInt(currentUserTypeID)) {
            columns.push({
                label: 'DCommission',
                name: 'DCommission',
                numeric: true
            });
        }
        if (ADMIN === parseInt(currentUserTypeID)) {
            columns.push({
                label: 'SCommission',
                name: 'SCommission',
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
                        <Form
                            style={{
                                marginBottom: '15px'
                            }}
                        >
                            <Grid item xs={12}>
                                <Paper>
                                    <CustomAccordion
                                        title={'Service-wise Sale'}
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

                                            {isAdmin ? (
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
                                                        options={downline}
                                                        getOptionLabel={(
                                                            option
                                                        ) => option.FirmName}
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
                rowsPerPage={100}
            />
        </CustomContainer>
    );
};
