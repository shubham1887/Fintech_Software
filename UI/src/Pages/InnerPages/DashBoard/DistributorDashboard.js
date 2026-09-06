import React, { useEffect, useState } from 'react';

import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { DIST_CODE, SUPERDIST_CODE } from 'Components/Helper/Messages';

import { SelectForDistributor as SELECT_DISTRIBUTOR_DASHBOARD_DATA } from 'Components/Api/DashboardApi';
import { SelectSale as SELECT_SERVICE_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import { SelectDistributorsummaray as SELECT_DISTRIBUTOR_DASHBOARD_TODAYSUMMARY } from 'Components/Api/TodaySummaryApi';
import { SelectUserSummaryForDistributor as SELECT_DISTRIBUTOR_DASHBOARD_USERSUMMARY } from 'Components/Api/UsersApi';

import { Grid, Container, Paper } from '@material-ui/core';
import TransactionSummaryCard from 'Components/SummaryCards/TransactionSummary';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const DistributorDashboard = () => {
    var currentUsertypeid = localStorage.getItem('UserTypeID');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [UserColumns, setUserColumns] = useState([]);
    const [ServicewiseColumns, setServicewiseColumns] = useState([]);
    const [TodayColumns, setTodayColumns] = useState([]);

    const [UserSummraylist, setUserSummraylist] = useState([]);
    const [TodaySummraylist, setTodaySummraylist] = useState([]);
    const [ServicewiseSale, setServicewiseSale] = useState([]);
    const [isDistributorRole, setIsDistributorRole] = useState(false);
    const [isSuperDistributorRole, setIsSuperDistributorRole] = useState(false);
    const [formObject, setFormObject] = useState({
        TotalSuccessAmount: 0,
        TotalFailedAmount: 0,
        TransferAmount: 0,
        TransferCount: 0,
        PurchaseAmount: 0,
        PurchaseCount: 0,
        TotalCommission: 0,
        Surcharge: 0
    });
    const CheckUserRole = () => {
        if (DIST_CODE === parseInt(currentUsertypeid))
            setIsDistributorRole(true);
        if (SUPERDIST_CODE === parseInt(currentUsertypeid))
            setIsSuperDistributorRole(true);
    };
    const selectTodaySummary = async () => {
        const response = await SELECT_DISTRIBUTOR_DASHBOARD_TODAYSUMMARY();
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
        if (data !== null || data !== undefined) setServicewiseSale(data.list);
        setLoadingOpen(false);
    };

    const select = async () => {
        const response = await SELECT_DISTRIBUTOR_DASHBOARD_DATA();
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
            PurchaseAmount: data.PurchaseAmount,
            PurchaseCount: data.PurchaseCount,
            TotalCommission: data.TotalCommission,
            Surcharge: data.Surcharge
        });
    };
    const selectUserSummary = async () => {
        setUserSummraylist([]);
        const response = await SELECT_DISTRIBUTOR_DASHBOARD_USERSUMMARY();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);
        if (data !== null || data !== undefined) setUserSummraylist(data);
        setLoadingOpen(false);
    };

    useEffect(() => {
        setLoadingOpen(true);
        CheckUserRole();
        select();
        selectTodaySummary();
        selectServicewiseSale();
        selectUserSummary();

        createUserColumns();
        createTodayColumns();
        createServicewiseColumns();
        setLoadingOpen(false);
    }, []);

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

    const createServicewiseColumns = () => {
        var columns = [];

        columns.push({
            label: 'Service',
            name: 'ServiceName'
        });
        columns.push({
            label: 'Success',
            name: 'SuccessAmt',
            numeric: true
        });
        columns.push({
            label: 'SuccessCount',
            name: 'SuccessCount',
            numeric: true
        });
        columns.push({
            label: 'Fail',
            name: 'FailAmt',
            numeric: true
        });
        columns.push({
            label: 'FailCount',
            name: 'FailCount',
            numeric: true
        });
        if (isDistributorRole === true) {
            columns.push({
                label: 'Commission',
                name: 'DCommission',
                numeric: true
            });
        }
        if (isSuperDistributorRole === true) {
            columns.push({
                label: 'Commission',
                name: 'SCommission',
                numeric: true
            });
        }
        setServicewiseColumns(columns);
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
            <Grid container spacing={3} className="mb-3">
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            Title="Sale"
                            Data={formObject.TotalSuccessAmount}
                            SubData={formObject.TotalFailedAmount}
                        />
                    </Paper>
                </Grid>
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            Title="Transfer"
                            Data={formObject.TransferAmount}
                            SubData={formObject.TransferCount}
                        />
                    </Paper>
                </Grid>
                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            Title="Purchase"
                            Data={formObject.PurchaseAmount}
                            SubData={formObject.PurchaseCount}
                        />
                    </Paper>
                </Grid>

                <Grid item xs={12} md={3}>
                    <Paper>
                        <TransactionSummaryCard
                            Title="Comm | Surcharge "
                            Data={formObject.TotalCommission}
                            SubData={formObject.Surcharge}
                        />
                    </Paper>
                </Grid>
            </Grid>

            <Grid container spacing={2}>
                <Grid item xs={12} sm={12} md={8} lg={8}>
                    <h4 className="mb-5">User Stock</h4>
                    <ReactTable
                        data={UserSummraylist}
                        columns={UserColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                    />

                    <h4 className="mb-5">Service Summary</h4>
                    <ReactTable
                        data={ServicewiseSale}
                        columns={ServicewiseColumns}
                        headerToolbar={false}
                        footerToolbar={false}
                    />
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
