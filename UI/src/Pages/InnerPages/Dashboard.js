import React, { useEffect, useState } from 'react';
import { LOGOUT } from 'ConstComponents';
import Box from '@material-ui/core/Box';
import { Formik, Form } from 'formik';
import ReactTable from 'Components/ReactTable/ReactTable';
import { Select as SELECT_DASHBOARD_DATA } from 'Components/Api/DashboardApi';
import { selectUserSummary as SELECT_DASHBOARD_USERSUMMARY } from 'Components/Api/UsersApi';
import { SelectSale as SELECT_SERVICE_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import { Select as SELECT_DASHBOARD_TODAYSUMMARY } from 'Components/Api/TodaySummaryApi';
import { SelectProviderSummary as SELECT_PROVIDER_SUMMARY } from 'Components/Api/ProviderPurchaseAPI';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import { Grid, Container, Paper, Button } from '@material-ui/core';
import TransactionSummaryCard from 'Components/SummaryCards/TransactionSummary';
import CircularProgress from '@material-ui/core/CircularProgress';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const AdminDashboard = () => {
    const classes = commonStyle();
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [UserColumns, setUserColumns] = useState([]);
    const [ServicewiseColumns, setServicewiseColumns] = useState([]);
    const [TodayColumns, setTodayColumns] = useState([]);
    const [ProviderColumns, setProviderColumns] = useState([]);

    const [loading, setLoading] = useState(false);
    const [UserSummraylist, setUserSummraylist] = useState([]);
    const [ProviderSummraylist, setProviderSummraylist] = useState([]);
    const [TodaySummraylist, setTodaySummraylist] = useState([]);
    const [ServicewiseSale, setServicewiseSale] = useState([]);

    const [ModelObj] = useState({
        Date: new Date()
    });
    const [formObject, setFormObject] = useState({
        TotalSuccessAmount: 0,
        TotalFailedAmount: 0,
        TransferAmount: 0,
        TransferCount: 0,
        ComplaintAmount: 0,
        ComplaintCount: 0,
        KycCount: 0,
        TotalTarget: 0,
        RemainsTarget: 0,

        PrePaidSuccess: 0,
        PrePaidFail: 0,
        DthSuccess: 0,
        DthFail: 0,
        PostPaidSuccess: 0,
        PostPaidFail: 0,
        DMTSuccess: 0,
        DMTFail: 0,
        AEPSSuccess: 0,
        AEPSFail: 0
    });
    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            Date: values.Date,
            FromDate: values.Date,
            ToDate: values.Date,
            UserID: 0
        };
        var requestObj = JSON.stringify(inputObj);
        setTodaySummraylist([]);
        setServicewiseSale([]);
        setUserSummraylist([]);
        setProviderSummraylist([]);

        const [
            response,
            responseService,
            responseUser,
            responseProvider,
            responseCard
        ] = await Promise.all([
            SELECT_DASHBOARD_TODAYSUMMARY(requestObj),
            SELECT_SERVICE_WISE_SALE(requestObj),
            SELECT_DASHBOARD_USERSUMMARY(requestObj),
            SELECT_PROVIDER_SUMMARY(requestObj),
            SELECT_DASHBOARD_DATA(requestObj)
        ]);

        setLoadingOpen(true);
        //const response = await SELECT_DASHBOARD_TODAYSUMMARY(requestObj);
        var data = JSON.parse(response.data);
        if (data !== null || data !== undefined) setTodaySummraylist(data);

        //const responseService = await SELECT_SERVICE_WISE_SALE(requestObj);
        var dataService = JSON.parse(responseService.data);
        if (dataService !== null || dataService !== undefined)
            setServicewiseSale(dataService);

        //const responseUser = await SELECT_DASHBOARD_USERSUMMARY(requestObj);
        var dataUser = JSON.parse(responseUser.data);
        if (dataUser !== null || dataUser !== undefined)
            setUserSummraylist(dataUser);

        //const responseProvider = await SELECT_PROVIDER_SUMMARY(requestObj);
        var dataProvider = JSON.parse(responseProvider.data);
        if (dataProvider !== null || dataProvider !== undefined)
            setProviderSummraylist(dataProvider);

        //const responseCard = await SELECT_DASHBOARD_DATA(requestObj);
        var dataCard = JSON.parse(responseCard.data);
        setFormObject({
            TotalSuccessAmount: dataCard.TotalSuccessAmount,
            TotalFailedAmount: dataCard.TotalFailedAmount,
            TransferAmount: dataCard.TransferAmount,
            TransferCount: dataCard.TransferCount,
            ComplaintAmount: dataCard.ComplaintAmount,
            ComplaintCount: dataCard.ComplaintCount,
            KycCount: dataCard.KycCount,
            TotalTarget: dataCard.TotalTarget,
            RemainsTarget: dataCard.RemainsTarget
        });
        setLoadingOpen(false);
        setSubmitting(false);
    };

    const selectProviderSummary = async () => {
        var inputObj = {
            Date: new Date()
        };
        var requestObj = JSON.stringify(inputObj);
        setProviderSummraylist([]);
        const response = await SELECT_PROVIDER_SUMMARY(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);
        if (data !== null || data !== undefined) setProviderSummraylist(data);
    };
    const selectTodaySummary = async () => {
        var inputObj = {
            Date: new Date()
        };
        var requestObj = JSON.stringify(inputObj);
        const response = await SELECT_DASHBOARD_TODAYSUMMARY(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data !== null || data !== undefined) setTodaySummraylist(data);
        setLoadingOpen(false);
    };

    const selectUserSummary = async () => {
        var inputObj = {
            Date: new Date()
        };
        var requestObj = JSON.stringify(inputObj);
        setUserSummraylist([]);
        const response = await SELECT_DASHBOARD_USERSUMMARY(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);
        if (data !== null || data !== undefined) setUserSummraylist(data);
    };
    const selectServicewiseSale = async () => {
        var inputObj = {
            FromDate: new Date(),
            ToDate: new Date(),
            UserID: 0
        };
        setServicewiseSale([]);

        var requestObj = JSON.stringify(inputObj);
        const response = await SELECT_SERVICE_WISE_SALE(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);
        if (data !== null || data !== undefined) setServicewiseSale(data);
        setLoadingOpen(false);
    };

    const select = async () => {
        var inputObj = {
            Date: new Date()
        };
        var requestObj = JSON.stringify(inputObj);
        const response = await SELECT_DASHBOARD_DATA(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        setFormObject({
            TotalSuccessAmount: data.TotalSuccessAmount,
            TotalFailedAmount: data.TotalFailedAmount,
            TransferAmount: data.TransferAmount,
            TransferCount: data.TransferCount,
            ComplaintAmount: data.ComplaintAmount,
            ComplaintCount: data.ComplaintCount,
            KycCount: data.KycCount,
            TotalTarget: data.TotalTarget,
            RemainsTarget: data.RemainsTarget,

            PrePaidSuccess: data.PrePaidSuccess,
            PrePaidFail: data.PrePaidFail,
            DthSuccess: data.DthSuccess,
            DthFail: data.DthFail,
            PostPaidSuccess: data.PostPaidSuccess,
            PostPaidFail: data.PostPaidFail,
            DMTSuccess: data.DMTSuccess,
            DMTFail: data.DMTFail,
            AEPSSuccess: data.AEPSSuccess,
            AEPSFail: data.AEPSFail
        });
    };

    useEffect(() => {
        setLoadingOpen(true);
        select();
        selectUserSummary();
        selectTodaySummary();
        selectServicewiseSale();
        selectProviderSummary();

        createUserColumns();
        createTodayColumns();
        createServicewiseColumns();
        createProviderColumns();
        setLoadingOpen(false);
    }, []);

    const createServicewiseColumns = () => {
        var columns = [];

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Success.Amt',
            name: 'SuccessAmt',
            numeric: true
        });
        columns.push({
            label: 'S.Count',
            name: 'SuccessCount'
        });

        columns.push({
            label: 'Fail.Amt',
            name: 'FailAmt',
            numeric: true
        });
        columns.push({
            label: 'F.Count',
            name: 'FailCount'
        });
        columns.push({
            label: 'RComm',
            name: 'RCommission',
            numeric: true
        });
        columns.push({
            label: 'DComm',
            name: 'DCommission',
            numeric: true
        });
        columns.push({
            label: 'SComm',
            name: 'SCommission',
            numeric: true
        });
        setServicewiseColumns(columns);
    };
    const createUserColumns = () => {
        var columns = [];

        columns.push({
            label: 'Role',
            name: 'UserType'
        });
        columns.push({
            label: 'Count',
            name: 'Count'
        });
        columns.push({
            label: 'Recharge',
            name: 'Recharge',
            numeric: true
        });
        columns.push({
            label: 'DMT',
            name: 'DMT',
            numeric: true
        });
        columns.push({
            label: 'AEPS',
            name: 'AEPS',
            numeric: true
        });

        setUserColumns(columns);
    };

    const createTodayColumns = () => {
        var columns = [];

        columns.push({
            label: 'Type',
            name: 'Type'
        });

        columns.push({
            label: 'TotalAmount',
            name: 'TotalAmount',
            numeric: true
        });

        setTodayColumns(columns);
    };
    const createProviderColumns = () => {
        var columns = [];

        columns.push({
            label: 'Name',
            name: 'ProviderName'
        });

        columns.push({
            label: 'Purchase',
            name: 'Purchase',
            numeric: true
        });
        columns.push({
            label: 'Balance',
            name: 'RechargeBalance',
            numeric: true
        });
        columns.push({
            label: 'Commission',
            name: 'Commission',
            numeric: true
        });

        setProviderColumns(columns);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                initialValues={ModelObj}
                onSubmit={submithandle}
                resetForm={ModelObj}
                render={({
                    values,
                    setFieldValue,
                    handleChange,
                    handleSubmit
                }) => {
                    return (
                        <Form>
                            <Grid item xs={12}>
                                <Grid container spacing={2}>
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <MaterialDatePickerField
                                            name="Date"
                                            label="Date"
                                            maxDate={new Date()}
                                        />
                                    </Grid>
                                    <Grid item xs={12} sm={12} md={1} lg={1}>
                                        <Grid className={classes.footerGrid}>
                                            <Box
                                                className={classes.flexReverse}
                                            >
                                                <div
                                                    className={
                                                        classes.SubmitWrapper
                                                    }
                                                >
                                                    <Button
                                                        type="submit"
                                                        title="submit"
                                                        className={
                                                            classes.CustomPrimaryButtonStyle
                                                        }
                                                        disabled={loading}
                                                    >
                                                        Search
                                                    </Button>
                                                    {loading && (
                                                        <CircularProgress
                                                            size={24}
                                                            className={
                                                                classes.buttonProgress
                                                            }
                                                        />
                                                    )}
                                                </div>
                                            </Box>
                                        </Grid>
                                    </Grid>
                                </Grid>
                            </Grid>
                        </Form>
                    );
                }}
            />

            <Grid container spacing={3} className="mb-3">
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            backgroundClass="bg-primary text-light"
                            Title="Sale"
                            Data={formObject.TotalSuccessAmount}
                            SubData={formObject.TotalFailedAmount}
                            link={'/Recharge/Report'}
                        />
                    </Paper>
                </Grid>
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            backgroundClass="bg-warning text-light"
                            Title="Transfer"
                            Data={formObject.TransferAmount}
                            SubData={formObject.TransferCount}
                            link={'/Fund/Report'}
                        />
                    </Paper>
                </Grid>
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            backgroundClass="bg-success text-light"
                            Title="Target"
                            Data={formObject.TotalTarget}
                            SubData={formObject.RemainsTarget}
                            link={'/Users'}
                        />
                    </Paper>
                </Grid>
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            backgroundClass="bg-info text-light"
                            Title="Dispute"
                            Data={formObject.ComplaintCount}
                            SubData={formObject.ComplaintAmount}
                            link={'/Complaints'}
                        />
                    </Paper>
                </Grid>
            </Grid>

            <Grid container spacing={2}>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <ReactTable
                        title={'Todays Summary'}
                        data={TodaySummraylist}
                        columns={TodayColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                        rowsPerPage={50}
                    />
                </Grid>

                <Grid item xs={12} sm={12} md={8} lg={8}>
                    <ReactTable
                        title={'User Stock'}
                        data={UserSummraylist}
                        columns={UserColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                    />
                    <h4 className="mb-5"> </h4>

                    <ReactTable
                        title={'Service Summary'}
                        data={ServicewiseSale}
                        columns={ServicewiseColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                    />

                    <h4 className="mb-5"> </h4>

                    <ReactTable
                        title={'Provider Stock'}
                        data={ProviderSummraylist}
                        columns={ProviderColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                        rowsPerPage={50}
                    />
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
