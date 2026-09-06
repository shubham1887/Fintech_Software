import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectRangewiseDMT as RANGEWISE_DMT } from 'Components/Api/TransactionReportAPI';
import { Container, Grid, MenuItem, Paper } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { RangeItems, ExportTypeItems } from 'Components/ReportHelper';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import {
    PLEASE_SELECT_VALID,
    RETAILER_CODE,
    API_USER_CODE,
    ADMIN_CODE
} from 'Components/Helper/Messages';
import { SelectForDropDown as PROVIDER_DROP_DOWN } from 'Components/Api/ProviderApi';
import HOCLoader from 'Components/Loader/HOCLoader';

import { MONEY_TRANSFER_CODE } from 'Components/Helper/Messages';

const CustomContainer = HOCLoader(Container);

export const RangewiseDMT = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [userList, setUserList] = useState([]);

    const [providerList, setProviderList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: MONEY_TRANSFER_CODE,
        OperatorID: 0,
        Status: 'ALL',
        TakeCount: 10,
        User: userDefault,
        Range: 'ALL',
        ExportType: 'Select Download',
        ProviderID: 0
    });
    const CheckUserRole = () => {
        if (RETAILER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (API_USER_CODE === parseInt(currentUsertypeid)) {
            setIsRetailerRole(true);
        }
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

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

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setTransactions([]);
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            Status: values.Status,
            UserID: values.User.UserID,
            Range: values.Range,
            ProviderID: values.ProviderID,
            ExportType: values.ExportType
        };

        setTransactions([]);

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await RANGEWISE_DMT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        setSubmitting(false);
        var data = JSON.parse(response.data);

        if (values.IsDownload) {
            downloadFile(data.ReportPath, 'Rangewise DMT Report');
            return;
        }
        setTransactions(data.list);
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
        var currentUsertypeid = localStorage.getItem('UserTypeID');

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

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
    }

    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        selectProvider();
        refreshForm();
    }, []);

    const createColumns = () => {
        var columns = [];
        columns.push({
            label: '#',
            name: '1',
            srno: true
        });

        columns.push({
            label: 'Range',
            name: 'Range'
        });
        columns.push({
            label: 'Total Count',
            name: 'Count',
            numeric: true
        });
        columns.push({
            label: 'Total Sum',
            name: 'Sum',
            numeric: true
        });
        columns.push({
            label: 'Success Count',
            name: 'SuccessCount',
            numeric: true
        });
        columns.push({
            label: 'Success Sum',
            name: 'SuccessSum',
            numeric: true
        });

        columns.push({
            label: 'Fail Count',
            name: 'FailCount',
            numeric: true
        });
        columns.push({
            label: 'Fail Sum',
            name: 'FailSum',
            numeric: true
        });
        columns.push({
            label: 'Process Count',
            name: 'ProcessCount',
            numeric: true
        });
        columns.push({
            label: 'Process Sum',
            name: 'ProcessSum',
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
                                                label="Range"
                                                name="Range"
                                                title="Range"
                                            >
                                                {RangeItems}
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
            </Formik>

            <ReactTable data={transactions} columns={mainColumns} />
        </CustomContainer>
    );
};
