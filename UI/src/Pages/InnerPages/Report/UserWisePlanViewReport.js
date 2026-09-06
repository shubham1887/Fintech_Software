import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form, Field } from 'formik';
import * as Yup from 'yup';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { Container, Grid, MenuItem, Accordion, Box } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { MaterialSelect } from 'Components/Inputes/FormikMaterialSelect';
import ReactTable from 'Components/ReactTable/ReactTable';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectOnlyRetailerAndAPI as USER_TYPE_SELECT, SelectReport as SELECT } from 'Components/Api/UserWisePlanLimitSettingAPI';
import { SelectRetailerAndAPIUserForDropDown as RETAILER_AND_APIUSER_SELECT_FOR_DROPDOWN } from 'Components/Api/UsersApi';
import AsyncAutoComplete from 'Components/Inputes/AsyncAutoComplete';
import { Typography } from '@material-ui/core';
import {
    PLEASE_SELECT_VALID,
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ExportTypeItems } from 'Components/ReportHelper';
import Paper from '@material-ui/core/Paper';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';
const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const UserWisePlanViewReport = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [openFilter, setopenFilter] = useState(true);
    const [currentPage, setCurrentPage] = useState(0);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');

    const [mobileServiceCount, setMobileServiceCount] = useState(0);
    const [dthServiceCount, setDthServiceCount] = useState(0);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [userTypeList, setUserTypeList] = useState([]);
    const [downLine, setDownLine] = useState([]);

    const [userDefault, setUserDefault] = useState({
        UserID: 0,
        FirmName: 'All User'
    });

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        ServiceID: 0,
        OperatorID: 0,
        Status: 'ALL',
        ProviderID: 0,
        ExportType: 'Select Download',
        IsRechargeReport: false,
        UserTypeID: 0,
        User: userDefault
    });

    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
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
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            UserID: formRef.current.values.User.UserID,
            ExportType: formRef.current.values.ExportType,
            IsPDFDownload: formRef.current.values.IsPDFDownload,
        };
        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);
        setTransactions([]);

        var response = await SELECT(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data.IsDownload) {
            downloadFile(data.ReportPath, 'UserPlan View Report');
            return;
        }
        setTransactions(data.list);
        setTotalCount(data.Count);

        if (data.list.length > 0) {
            setMobileServiceCount(data.list[0].MobileServiceCount);
            setDthServiceCount(data.list[0].DTHServiceCount);
        }
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


    useEffect(() => {
        createColumns();
        refreshForm();
        selectUserTypes();
    }, []);

    const createColumns = () => {
        var columns = [];
        var statusIndex = 1;

        columns.push({
            label: '#',
            name: 'UserWisePlanViewID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Service Name',
            name: 'ServiceName',
        });

        columns.push({
            label: 'Operator Name',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Consumer Number',
            name: 'ConsumerNumber'
        });

        setMainColumns(columns);
    };

    const selectUsers = async (values) => {
        var jsonReq = {
            UserTypeID: values.UserTypeID,
            FirmName: values.FirmName
        };
        var request = JSON.stringify(jsonReq);
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

    const selectUserTypes = async () => {
        const response = await USER_TYPE_SELECT();

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


    const refreshForm = () => {
        if (!formRef.current) return;
        formRef.current.handleSubmit();
    };
    const loadingProgress = loading && (
        <CircularProgress size={24} className={classes.buttonProgress} />
    );

    const PlanViewForm = ({ values, setFieldValue, handleSubmit }) => {
        return (
            <Form style={{ marginBottom: '15px' }}>
                <Grid item xs={12}>
                    <CustomAccordion
                        title={'Plan View Report'}
                        open={openFilter}
                        setOpen={(value) => setopenFilter(value)}
                    >
                        <Grid container spacing={2}>

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

                            <Grid
                                item
                                xs={10}
                                sm={10}
                                md={2}
                                lg={2}
                            >
                                <Field
                                    component={AsyncAutoComplete}
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
                                    onSearchCallback={(text) => {
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
                                            UserID: value.UserID,
                                            FirmName: value.FirmName
                                        });
                                    }}
                                />
                            </Grid>

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
                                    disabled={loading}
                                >
                                    Search
                                </PrimaryButton>
                                {loadingProgress}
                            </Grid>
                        </Grid>
                    </CustomAccordion>
                </Grid>

                <Box mt={2} width="100%">
                    <Grid item xs={12}>
                        <Paper elevation={3}>
                            <Box padding={2}>
                                <Grid container spacing={1} alignItems="center">
                                    <Grid item xs={2}>
                                        <Typography variant="h6" component="span" color="primary">
                                            Mobile Service Count:
                                        </Typography>
                                        <Typography variant="h6" component="span">
                                            {mobileServiceCount}
                                        </Typography>
                                    </Grid>
                                    <Grid item xs={4}>
                                        <Typography variant="h6" component="span" color="primary">
                                            DTH Service Count:
                                        </Typography>
                                        <Typography variant="h6" component="span">
                                            {dthServiceCount}
                                        </Typography>
                                    </Grid>
                                </Grid>
                            </Box>
                        </Paper>
                    </Grid>
                </Box>
            </Form>

        );
    };
    const customContainerCallback = (alertState) => {
        setIsError(alertState);
    };
    return (

        <CustomContainer
            isLoading={loading}
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
                {PlanViewForm}
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
            />

        </CustomContainer>
    );
};
