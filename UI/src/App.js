import React, { useState, lazy } from 'react';
import {
    BrowserRouter as Router,
    Redirect,
    Route,
    Switch
} from 'react-router-dom';
import { ThemeProvider } from '@material-ui/core/styles';
import CustomTheme from 'CustomTheme';
import { Refresh } from 'Components/Api/AppbarAPI';
import { RefreshSD } from 'Components/Api/AppbarAPI';
import { SelectByUserID as WALLET_SELECT } from 'Components/Api/WalletApi';
import { BalanceContext } from 'Components/BalanceContext.js';
import CustomSuspense from 'Components/CustomSuspense/CustomSuspense';
import { LOGOUT } from 'ConstComponents';

import { initializeReactGA } from 'ReactGa';

import './App.css';

const OuterLayoutNavigator = lazy(() =>
    import('Navigation/OuterLayoutNavigator')
);

const InnerLayoutNavigator = lazy(() =>
    import('Navigation/InnerLayoutNavigator')
);

function App(props) {
    initializeReactGA();

    const [Wallets, setWallets] = useState([]);
    const [PendingKYCCount, setPendingKYCCount] = useState(
        localStorage.getItem('PendingKYCCount') || 0
    );
    const [FundRequesCount, setFundRequesCount] = useState(
        localStorage.getItem('FundRequesCount') || 0
    );
    const [FundReverseRequestCount, setFundReverseRequestCount] = useState(
        localStorage.getItem('FundReverseRequestCount') || 0
    );
    const [ComplaintCount, setComplaintCount] = useState(
        localStorage.getItem('ComplaintCount') || 0
    );
    const [PendingTransactionCount, setPendingTransactionCount] = useState(
        localStorage.getItem('PendingTransactionCount') || 0
    );
    const [
        PendingDMTTransactionCount,
        setPendingDMTTransactionCount
    ] = useState(localStorage.getItem('PendingDMTTransactionCount') || 0);

    const [
        PendingUPITransactionCount,
        setPendingUPITransactionCount
    ] = useState(localStorage.getItem('PendingUPITransactionCount') || 0);

    const [NotificationCount, setNotificationCount] = useState(
        localStorage.getItem('NotificationCount') || 0
    );
    const [CallMeRequestCount, setCallMeRequestCount] = useState(
        localStorage.getItem('CallMeRequestCount') || 0
    );
    const [PendingTicketCount, setPendingTicketCount] = useState(
        localStorage.getItem('PendingTicketCount') || 0
    );
    const [PasswordExpiraryDays, setPasswordExpiraryDays] = useState(
        localStorage.getItem('PasswordExpiraryDays') || 0
    );
    const [SubscriptionDays, setSubscriptionDays] = useState(
        localStorage.getItem('SubscriptionDays') || 0
    );

    const [ProfileImagePath, setProfileImagePath] = useState(
        localStorage.getItem('ProfileImagePath') || ''
    );

    function setSystemIP(IP) {}

    const SelectBalance = async () => {
        var response = await WALLET_SELECT();
        const { isAuthorized, status, data } = response;
        if (!isAuthorized) LOGOUT();

        if (!status) return;

        localStorage.setItem('Wallets', data);
        setWallets(data);
    };

    function DashboardBalanceUpdate() {
        SelectBalance();
    }

    const SelectAppBarContent = async () => {
        var response = await Refresh();

        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();
        if (!status) return;

        let d = JSON.parse(data);

        localStorage.setItem('PendingKYCCount', d?.PendingKYCCount);
        localStorage.setItem('FundRequesCount', d?.FundRequesCount);
        localStorage.setItem(
            'FundReverseRequestCount',
            d?.FundReverseRequestCount
        );
        localStorage.setItem('ComplaintCount', d?.ComplaintCount);
        localStorage.setItem(
            'PendingTransactionCount',
            d?.PendingTransactionCount
        );
        localStorage.setItem(
            'PendingDMTTransactionCount',
            d?.PendingDMTTransactionCount
        );
        localStorage.setItem(
            'PendingUPITransactionCount',
            d?.PendingUPITransactionCount
        );
        localStorage.setItem('NotificationCount', d.NotificationCount);
        localStorage.setItem('CallMeRequestCount', d.CallMeRequestCount);

        localStorage.setItem('PendingTicketCount', d.PendingTicketCount);
        localStorage.setItem('PasswordExpiraryDays', d.PasswordExpiraryDays);
        localStorage.setItem('SubscriptionDays', d.SubscriptionDays);

        setPendingKYCCount(d.PendingKYCCount);
        setPendingDMTTransactionCount(d.PendingDMTTransactionCount);
        setPendingUPITransactionCount(d.PendingUPITransactionCount);
        setFundRequesCount(d.FundRequesCount);
        setFundReverseRequestCount(d.FundReverseRequestCount);
        setComplaintCount(d.ComplaintCount);
        setPendingTransactionCount(d.PendingTransactionCount);
        setNotificationCount(d.NotificationCount);
        setCallMeRequestCount(d.CallMeRequestCount);
        setPendingTicketCount(d.PendingTicketCount);
        setPasswordExpiraryDays(d.PasswordExpiraryDays);
        setSubscriptionDays(d.SubscriptionDays);
    };

    const SelectAppBarContentSD = async () => {
        var response = await RefreshSD();

        const { isAuthorized, status, data } = response;

        if (!isAuthorized) LOGOUT();
        if (!status) return;

        let d = JSON.parse(data);

        localStorage.setItem('FundRequesCount', d?.FundRequesCount);

        setFundRequesCount(d.FundRequesCount);
    };

    function AppbarRefresh() {
        SelectAppBarContent();
    }

    function AppbarRefreshSD() {
        SelectAppBarContentSD();
    }

    function ProfileImageRefresh() {
        setProfileImagePath(localStorage.getItem('ProfileImagePath') || '');
    }
    const BalanceContextProviderValues = {
        ProfileImagePath,
        PendingKYCCount,
        FundRequesCount,
        FundReverseRequestCount,
        ComplaintCount,
        PendingTransactionCount,
        PendingDMTTransactionCount,
        NotificationCount,
        setSystemIP,
        DashboardBalanceUpdate,
        AppbarRefresh,
        AppbarRefreshSD,
        CallMeRequestCount,
        Wallets,
        PendingTicketCount,
        PasswordExpiraryDays,
        SubscriptionDays,
        PendingUPITransactionCount
    };
    return (
        <Router>
            <ThemeProvider theme={CustomTheme}>
                <BalanceContext.Provider value={BalanceContextProviderValues}>
                    <CustomSuspense>
                        <Switch>
                            <Route exact path="/">
                                <Redirect to={'/Login'} />
                            </Route>
                        </Switch>
                        <OuterLayoutNavigator />
                        <InnerLayoutNavigator />
                    </CustomSuspense>
                </BalanceContext.Provider>
            </ThemeProvider>
        </Router>
    );
}

export default App;
