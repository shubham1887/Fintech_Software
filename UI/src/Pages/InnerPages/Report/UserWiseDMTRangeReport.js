import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { SelectUserForDropDown as USER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import { SelectUserWiseDMTRangeReport  } from 'Components/Api/TransactionReportAPI';
import { Container, Grid, Paper,MenuItem } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ExportTypeItems } from 'Components/ReportHelper';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import {
    PLEASE_SELECT_VALID,
    ADMIN_CODE
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { SelectServiceMoneyTransferAndUPITransfer as SERVICE_SELECT } from 'Components/Api/ServiceApi';

import { MONEY_TRANSFER_CODE } from 'Components/Helper/Messages';

const CustomContainer = HOCLoader(Container);

export const UserWiseDMTRangeReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [userList, setUserList] = useState([]);
    const [ServiceList, setServiceList] = useState([]);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: MONEY_TRANSFER_CODE,
        TakeCount: 100,
        User: userDefault,
        Range: 'ALL',
        ExportType: 'Select Download',
        ProviderID: 0
    });
    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUsertypeid)) setIsAdminRole(true);
    };

   
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setTransactions([]);
        var inputObj = {
            FromDate: values.FromDate,
            ToDate: values.ToDate,
            UserID: values.User.UserID,
            Range: values.Range,
            ServiceID : values.ServiceID,
            ExportType: values.ExportType
        };

        setTransactions([]);

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await SelectUserWiseDMTRangeReport(requestObj);
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
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'Userwise DMTrange wise Report');
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
    const selectService = async () => {
        const response = await SERVICE_SELECT();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setResponseMessage(response.data);
            setIsError(true);
        }

        var data = JSON.parse(response.data);
        setServiceList(data);
    };

    useEffect(() => {
        CheckUserRole();
        createColumns();
        selectService();
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
            label: 'UserName',
            name: 'UserName'
        });
        columns.push({
            label: '1 - 2',
            name: 'Range1_2',
           
        });
        columns.push({
            label: '3 - 1k',
            name: 'Range3_1000',
           
        });
        columns.push({
            label: '1k - 2k',
            name: 'Range1001_2000',
           
        });
        columns.push({
            label: '2k - 3k',
            name: 'Range2001_3000',
           
        });

        columns.push({
            label: '3k - 4k',
            name: 'Range3001_4000',
           
        });
        columns.push({
            label: '4k - 5k',
            name: 'Range4001_5000',
           
        });
        columns.push({
            label: '5k - 10k',
            name: 'Range5001_10000',
           
        });
        columns.push({
            label: '10k - 15k',
            name: 'Range10001_15000',
           
        });
        columns.push({
            label: '15k - 20k',
            name: 'Range15001_20000',
           
        });
        columns.push({
            label: '20k - 25k',
            name: 'Range20001_25000',
           
        });
        columns.push({
            label: '25k - 50k',
            name: 'Range25001_50000',
           
        });
        columns.push({
            label: '50k - 1L',
            name: 'Range50001_1L',
           
        });
        columns.push({
            label: 'Total Count',
            name: 'TotalTransactions',
           
        });
        
        

        setMainColumns(columns);
    };
    const ServiceItems = ServiceList.map(({ ServiceID, ServiceName }) => (
        <MenuItem key={ServiceID} value={ServiceID}>
            {ServiceName}
        </MenuItem>
    ));

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
                                        <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialSelect
                                    name="ServiceID"
                                    label="Service"
                                    title="Service"
                                >
                                    <MenuItem key="0" value="0">
                                     Service
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
