import { Container, Grid } from '@material-ui/core';
import { SelectForDistributor as SELECT_DISTRIBUTOR_DASHBOARD_DATA } from 'Components/Api/DashboardApi';
import { SelectDistributorsummaray as SELECT_DISTRIBUTOR_DASHBOARD_TODAYSUMMARY } from 'Components/Api/TodaySummaryApi';
import { SelectSale as SELECT_SERVICE_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import { SelectUserSummaryForDistributor as SELECT_DISTRIBUTOR_DASHBOARD_USERSUMMARY } from 'Components/Api/UsersApi';
import HOCLoader from 'Components/Loader/HOCLoader';
import { ADMIN_USERID } from 'Components/Helper/Messages';
import MonthlySummaryGraph from 'Pages/InnerPages/DashBoard/MonthlySummaryGraph';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import ServiceSummary from 'Pages/InnerPages/DashBoard/DashServiceSummary/ServiceSummary';
import TodaysSummary from './TodaysSummary';
import React, { useEffect, useState } from 'react';
import { PRIMARY_COLOR } from 'Config';
import {
    SelectParentBanks as SELECT_BY_PARENT_BANK,
    SelectCompanyBanks as SELECT_ADMIN_BANK
} from 'Components/Api/UserBankDetailApi';
import DashBoardTransactionDetails from './DashBoardTransactionDetails';
import DashBoardUserSummary from './DashBoardUserSummary';
import TodaysTransactions from './TodaysTransactions';
import VirtualAccount from 'Pages/InnerPages/DashBoard/VirtualAccount';
import { SelectVirtualAccount as SELECT_VIRTUAL_ACCOUNT } from 'Components/Api/VirtualAccountAPI';
import { makeStyles } from '@material-ui/core/styles';
import { UserLowBalance } from 'Pages/InnerPages/UserwiseLowBalance/LowBalance';
import { useHistory } from 'react-router';
import { SelectLowBalance as SELECT_LOW_BALANCE } from 'Components/Api/UserwiseLowBalanceAPI';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { SelectUserWiseSubscriptionCharge as SELECT_SUBSCRIPTION_CHARGE } from 'Components/Api/ServiceSubscriptionChargeAPI';

const CustomContainer = HOCLoader(Container);

const useStyles = makeStyles({
    rightpanel: (props) => ({
        background: `${props.bgColor} !important`,
        borderRadius: '3px',
        padding: '3px 15px 15px',
        color: '#fff',
        margin: '10px',
        height: '100%'
    })
});

export const DistributorDashboardV2 = () => {
    const props = { bgColor: PRIMARY_COLOR() };
    const classes = useStyles(props);
    const [mainColumns, setMainColumns] = useState([]);
    const [adminColumns, setAdminColumns] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [VirtualAccountlist, setVirtualAccountlist] = useState([]);
    const [TodaySummarylist, setTodaySummarylist] = useState([]);
    const [UserSummraylist, setUserSummraylist] = useState([]);
    const [ServicewiseSale, setServicewiseSale] = useState([]);
    const [bankList, setBankList] = useState([]);
    const history = useHistory();
    const [lowBalance, setLowBalance] = useState(false);
    const [LowBalanceMessage, setLowBalanceMessage] = useState('');
    const [ParentID] = useState(localStorage.getItem('ParentID'));
    const [isServiceSubscription, setIsServiceSubscription] = useState(false);
    const [IsCashCollection] = useState(
        localStorage.getItem('IsCashCollection')
    );
    const [formObject, setFormObject] = useState({
        TotalSuccessAmount: 0,
        TotalFailedAmount: 0,
        PurchaseAmount: 0,
        PurchaseCount: 0,
        TotalCommission: 0,
        Surcharge: 0,
        TotalTarget: 0,
        RemainsTarget: 0
    });

    const selectTodaySummary = async () => {
        let inputObj = {
            Date: new Date(),
            ToDate: new Date(),
            UserID: 0
        };

        let requestObj = JSON.stringify(inputObj);
        const {
            isAuthorized,
            status,
            data
        } = await SELECT_DISTRIBUTOR_DASHBOARD_TODAYSUMMARY(requestObj);
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        let d = JSON.parse(data);
        if (d !== null || d !== undefined) setTodaySummarylist(d);
    };
    const selectParentBank = async (ParentID) => {
        createColumns();
        const response = await SELECT_BY_PARENT_BANK();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };
    const selectAdminBank = async () => {
        const response = await SELECT_ADMIN_BANK();
        setLoadingOpen(false);
        createAdminColumns();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);

        setBankList(data);
    };
    const createColumns = () => {
        var columns = [];
        columns.push({
            label: '#',
            name: 'UserBankDetailsID',
            srno: true
        });
        columns.push({
            label: 'Role',
            name: 'Role'
        });
        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'Bank Name',
            name: 'BankName'
        });

        columns.push({
            label: 'Holder Name',
            name: 'HolderName'
        });

        columns.push({
            label: 'Acc.No',
            name: 'AccountNumber'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        columns.push({
            label: 'Branch',
            name: 'Branch'
        });
        setMainColumns(columns);
    };
    const createAdminColumns = () => {
        var columns = [];
        columns.push({
            label: '#',
            name: 'UserBankDetailsID',
            srno: true
        });
        columns.push({
            label: 'Company',
            name: 'Role'
        });
        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'Bank Name',
            name: 'BankName'
        });

        columns.push({
            label: 'Holder Name',
            name: 'HolderName'
        });

        columns.push({
            label: 'Acc.No',
            name: 'AccountNumber'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        columns.push({
            label: 'Branch',
            name: 'Branch'
        });
        setAdminColumns(columns);
    };
    const selectServicewiseSale = async () => {
        let inputObj = {
            FromDate: new Date(),
            ToDate: new Date(),
            UserID: 0
        };
        setServicewiseSale([]);

        let requestObj = JSON.stringify(inputObj);
        const response = await SELECT_SERVICE_WISE_SALE(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        if (data.list !== null || data.list !== undefined)
            setServicewiseSale(data.list);
        setLoadingOpen(false);
    };
    const selectUserSummary = async () => {
        let inputObj = {
            Date: new Date(),
            ToDate: new Date(),
            UserID: 0
        };
        setServicewiseSale([]);

        let requestObj = JSON.stringify(inputObj);
        const response = await SELECT_DISTRIBUTOR_DASHBOARD_USERSUMMARY(
            requestObj
        );
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }

        let data = JSON.parse(response.data);

        if (data !== null || data !== undefined) setUserSummraylist(data);
    };
    const selectLowBalance = async () => {
        setLowBalance(false);
        setLowBalanceMessage('');

        const response = await SELECT_LOW_BALANCE();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        let data = JSON.parse(response.data);

        if (data.Status === 'Ok') {
            setLowBalance(false);
            setLowBalanceMessage('');
            return;
        }
        setLowBalance(true);
        setLowBalanceMessage(data.Message);
    };
    const select = async () => {
        let inputObj = {
            Date: new Date(),
            ToDate: new Date(),
            UserID: 0
        };

        let requestObj = JSON.stringify(inputObj);
        const response = await SELECT_DISTRIBUTOR_DASHBOARD_DATA(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        let data = JSON.parse(response.data);

        setFormObject({
            TotalSuccessAmount: data.TotalSuccessAmount,
            TotalFailedAmount: data.TotalFailedAmount,
            PurchaseAmount: data.PurchaseAmount,
            PurchaseCount: data.PurchaseCount,
            TotalTarget: data.TotalTarget,
            RemainsTarget: data.RemainsTarget,
            TotalCommission: data.TotalCommission,
            Surcharge: data.Surcharge
        });
    };
    const selectVirtualAccount = async () => {
        const response = await SELECT_VIRTUAL_ACCOUNT();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        let data = JSON.parse(response.data);

        if (data !== null || data !== undefined) setVirtualAccountlist(data);
    };

    useEffect(() => {
        setLoadingOpen(true);
        selectTodaySummary();
        selectServicewiseSale();
        selectUserSummary();
        select();
        selectVirtualAccount();
        selectParentBank();
        selectAdminBank();
        setLoadingOpen(false);
        selectLowBalance();
    }, [ParentID]);

    const selectSubscriptionCharge = async () => {
        const response = await SELECT_SUBSCRIPTION_CHARGE();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        let data = JSON.parse(response.data);
        if (data == null) return;
        if (data.length >= 1) setIsServiceSubscription(true);
    };

    const handleCustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={handleCustomContainerCloseCallback}
        >
            <Grid container className="mb-5">
                <Grid item xs={12} sm={12} md={9} lg={9} xl={9}>
                    <Grid container className="mb-5">
                        <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                            <MonthlySummaryGraph />
                        </Grid>
                    </Grid>

                    <Grid container className="mb-5">
                        <Grid
                            item
                            xs={12}
                            sm={12}
                            md={4}
                            lg={4}
                            xl={4}
                            className="mb-5"
                        >
                            <TodaysTransactions data={ServicewiseSale} />
                        </Grid>
                        <Grid
                            item
                            xs={12}
                            sm={12}
                            md={8}
                            lg={8}
                            xl={8}
                            className="mb-5"
                        >
                            <ServiceSummary data={ServicewiseSale} />
                        </Grid>
                    </Grid>
                </Grid>

                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <div className={classes.rightpanel}>
                        <DashBoardTransactionDetails
                            transactionDetails={formObject}
                        />
                        {/* <DashBoardUserSummary UserSummary={UserSummraylist} /> */}
                    </div>
                </Grid>
            </Grid>
            {lowBalance ? (
                <UserLowBalance
                    isOpen={lowBalance}
                    title="Low Balance"
                    message={LowBalanceMessage}
                    closeButtonClick={() => {
                        setLowBalance(false);
                    }}
                    fundRequestButtonClick={() => {
                        setLowBalance(false);
                        history.push('/Fund/Request');
                        return;
                    }}
                />
            ) : null}
        </CustomContainer>
    );
};
