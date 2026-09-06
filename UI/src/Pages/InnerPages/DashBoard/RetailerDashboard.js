import { Grid, Container, Box, Button } from '@material-ui/core';
import { SelectForRetailer as SELECT_RETAILER_DASHBOARD_DATA } from 'Components/Api/DashboardApi';
import { SelectForRetailer as SELECT_RETAILER_DASHBOARD_TODAYSUMMARY } from 'Components/Api/TodaySummaryApi';
import { SelectSale as SELECT_SERVICE_WISE_SALE } from 'Components/Api/TransactionReportAPI';
import { SelectWalletSummary as SELECT_WALLET_Summary } from 'Components/Api/WalletApi';
import { SelectVirtualAccount as SELECT_VIRTUAL_ACCOUNT } from 'Components/Api/VirtualAccountAPI';
import { SelectLowBalance as SELECT_LOW_BALANCE } from 'Components/Api/UserwiseLowBalanceAPI';
import CircularProgress from '@material-ui/core/CircularProgress';
import { Formik, Form } from 'formik';
import HOCLoader from 'Components/Loader/HOCLoader';
import MonthlySummaryGraph from 'Pages/InnerPages/DashBoard/MonthlySummaryGraph';
import { LOGOUT } from 'ConstComponents';
import VirtualAccount from 'Pages/InnerPages/DashBoard/VirtualAccount';
import ServiceSummary from 'Pages/InnerPages/DashBoard/DashServiceSummary/ServiceSummary';
import TodaysSummary from './TodaysSummary';
import React, { useEffect, useState } from 'react';
import { PRIMARY_COLOR } from 'Config';
import { useHistory } from 'react-router';
import DashboardShortCutMenu from './DashboardShortCutMenu';
import DashBoardTransactionDetails from './DashBoardTransactionDetails';
import DashBoardWalletDetails from './DashBoardWalletDetails';
import { ADMIN_USERID } from 'Components/Helper/Messages';
import {
    SelectParentBanks as SELECT_BY_PARENT_BANK,
    SelectCompanyBanks as SELECT_ADMIN_BANK
} from 'Components/Api/UserBankDetailApi';
import TodaysTransactions from './TodaysTransactions';
import { UserLowBalance } from 'Pages/InnerPages/UserwiseLowBalance/LowBalance';
import { makeStyles } from '@material-ui/core/styles';
import { TODAYSUMMARYONTOP } from 'Config';
import { MaterialDatePickerField } from 'Components/Inputes/MaterialDatePickerField';
import {
    PrimaryButton,
    SecondaryButton
} from 'Components/Inputes/CustomButton';
import { UpdateBalance } from 'Components/Api/CreditInstitutionSettingAPI';

import ReactTable from 'Components/ReactTable/ReactTable';
import { CLIENT_CODE } from 'Components/Helper/UsertypeCode';
import { QRImageView } from 'Pages/InnerPages/UserBankDetail/QRImageView';
import { CustomModal } from 'Components/Modal/CustomModal';
import { SelectUserWiseSubscriptionCharge as SELECT_SUBSCRIPTION_CHARGE } from 'Components/Api/ServiceSubscriptionChargeAPI';
import { SelectByUserID as SELECT } from 'Components/Api/SubscriptionPlanAPI';

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

export const RetailerDashboard = () => {
    const props = {
        bgColor: PRIMARY_COLOR()
    };
    const [mainColumns, setMainColumns] = useState([]);
    const [adminColumns, setAdminColumns] = useState([]);
    const classes = useStyles(props);
    const history = useHistory();
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [LowBalanceMessage, setLowBalanceMessage] = useState('');
    const [responseMessage, setResponseMessage] = useState('');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [lowBalance, setLowBalance] = useState(false);
    const [VirtualAccountlist, setVirtualAccountlist] = useState([]);
    const [TodaySummarylist, setTodaySummarylist] = useState([]);
    const [itemList, setItemList] = useState([]);
    const [bankList, setBankList] = useState([]);
    const [WalletSummaryList, setWalletSummaryList] = useState([]);
    const [isImageModelOpen, setIsImageModelOpen] = useState(false);
    const [ImageURL, setImageURL] = useState('');
    const [BankName, setBankName] = useState('');

    const [ServicewiseSale, setServicewiseSale] = useState([]);
    const [ParentID] = useState(localStorage.getItem('ParentID'));

    const [isClientRole, setIsClientRole] = useState(false);
    const [isTodaySummaryOnTop, setTodaySummaryOnTop] = useState(false);
    const [isTodaySummaryOnBottom, setTodaySummaryOnBottom] = useState(true);
    const [isMonthlyGraphOnTop, setMonthlyGraphOnTop] = useState(true);
    const [isMonthlyGraphOnBottom, setMonthlyGraphOnBottom] = useState(false);
    const [isServiceSubscription, setIsServiceSubscription] = useState(false);

    var IsCreditInstitution =
        localStorage.getItem('IsCreditInstitution') === 'yes';

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
    const [ModelObj] = useState({
        Date: new Date()
    });
    var currentUsertypeid = parseInt(localStorage.getItem('UserTypeID'));
    const CheckUserRole = () => {
        if (CLIENT_CODE === parseInt(currentUsertypeid)) {
            setIsClientRole(true);
        }
    };
    const fetchTodaySummaryOnTop = async () => {
        let isTop = await TODAYSUMMARYONTOP();
        if (isTop === 'YES') {
            setTodaySummaryOnTop(true);
            setTodaySummaryOnBottom(false);
            setMonthlyGraphOnTop(false);
            setMonthlyGraphOnBottom(true);
            return;
        }
    };
    const selectParentBank = async () => {
        createColumns();
        setItemList([]);
        const response = await SELECT_BY_PARENT_BANK();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        if (data !== null && data !== undefined) setItemList(data);
    };

    const selectAdminBank = async () => {
        setBankList([]);
        const response = await SELECT_ADMIN_BANK();
        setLoadingOpen(false);
        createAdminColumns();
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        if (data !== null && data !== undefined) setBankList(data);
    };
    const selectTodaySummary = async () => {
        let inputObj = {
            Date: new Date(),
            ToDate: new Date(),
            UserID: 0
        };
        setTodaySummarylist([]);

        let requestObj = JSON.stringify(inputObj);
        const {
            isAuthorized,
            status,
            data
        } = await SELECT_RETAILER_DASHBOARD_TODAYSUMMARY(requestObj);
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            return;
        }

        let d = JSON.parse(data);
        if (d !== null && d !== undefined) setTodaySummarylist(d);
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

        const { isAuthorized, status, data } = response;
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            return;
        }

        let d = JSON.parse(data);

        if (d.list !== undefined && d.list !== null) {
            setServicewiseSale(d.list);
            setLoadingOpen(false);
        }
    };
    const selectWalletSummary = async () => {
        let inputObj = {
            Date: new Date(),
            ToDate: new Date(),
            UserID: 0
        };
        setWalletSummaryList([]);

        let requestObj = JSON.stringify(inputObj);
        const response = await SELECT_WALLET_Summary(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }

        let data = JSON.parse(response.data);

        if (data !== null && data !== undefined) setWalletSummaryList(data);
    };
    const selectVirtualAccount = async () => {
        setVirtualAccountlist([]);
        const response = await SELECT_VIRTUAL_ACCOUNT();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        let data = JSON.parse(response.data);

        if (data !== null && data !== undefined) setVirtualAccountlist(data);
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
        const response = await SELECT_RETAILER_DASHBOARD_DATA(requestObj);
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

    const selectData = async () => {
        setLoadingOpen(true);
        const response = await UpdateBalance();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();
    };

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        var inputObj = {
            Date: values.Date,
            ToDate: values.Date,
            UserID: 0
        };
        var requestObj = JSON.stringify(inputObj);
        setTodaySummarylist([]);
        setServicewiseSale([]);
        setWalletSummaryList([]);
        setTodaySummarylist([]);

        setLoadingOpen(true);

        const responseCard = await SELECT_RETAILER_DASHBOARD_DATA(requestObj);
        var dataCard = JSON.parse(responseCard.data);
        setFormObject({
            TotalSuccessAmount: dataCard.TotalSuccessAmount,
            TotalFailedAmount: dataCard.TotalFailedAmount,
            PurchaseAmount: dataCard.PurchaseAmount,
            PurchaseCount: dataCard.PurchaseCount,
            TotalTarget: dataCard.TotalTarget,
            RemainsTarget: dataCard.RemainsTarget,
            TotalCommission: dataCard.TotalCommission,
            Surcharge: dataCard.Surcharge
        });

        const response = await SELECT_WALLET_Summary(requestObj);
        setLoadingOpen(false);

        var data = JSON.parse(response.data);

        const responseUser = await SELECT_RETAILER_DASHBOARD_TODAYSUMMARY(
            requestObj
        );
        var dataUser = JSON.parse(responseUser.data);
        if (dataUser !== null && dataUser !== undefined)
            setTodaySummarylist(dataUser);

        if (data !== null && data !== undefined) setWalletSummaryList(data);

        var serviceObj = {
            FromDate: values.Date,
            ToDate: values.Date,
            UserID: 0
        };
        var servicerequestObj = JSON.stringify(serviceObj);
        const responseService = await SELECT_SERVICE_WISE_SALE(
            servicerequestObj
        );

        var dataService = JSON.parse(responseService.data);
        if (dataService.list !== null && dataService.list !== undefined)
            setServicewiseSale(dataService.list);

        setLoadingOpen(false);
        setSubmitting(false);
    };

    useEffect(() => {
        setLoadingOpen(true);
        fetchTodaySummaryOnTop();
        selectTodaySummary();
        selectServicewiseSale();
        selectWalletSummary();
        selectVirtualAccount();
        selectLowBalance();
        select();
        selectParentBank();
        selectAdminBank();
        setLoadingOpen(false);
        CheckUserRole();
        selectByUserID();
    }, [ParentID]);

    const selectByUserID = async () => {
        setLoadingOpen(true);
        const response = await SELECT();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) return;
        var data = JSON.parse(response.data);

        if (data == 'Record not found') {
            history.push('/Subscription');
        }
    };

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
            label: 'Bank',
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
        columns.push({
            label: 'QR Code',
            name: 'QRCodeImageURL',
            buttonTitle: 'QR',
            testbutton: true,
            callback: (value, row) => {
                viewImg(value, row[3]);
            }
        });
        setMainColumns(columns);
    };
    const viewImg = async (request, BankName) => {
        setIsImageModelOpen(true);
        setImageURL(request);
        setBankName(BankName);
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
            label: 'Bank',
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
        columns.push({
            label: 'QR Code',
            name: 'QRCodeImageURL',
            buttonTitle: 'QR',
            testbutton: true,
            callback: (value, row) => {
                viewImg(value, row[3]);
            }
        });
        setAdminColumns(columns);
    };
    const handleCustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    const MonthlyGraphHideShow = () => {
        return (
            <Grid container className="mb-5">
                <Grid item xs={12}>
                    <MonthlySummaryGraph />
                </Grid>
            </Grid>
        );
    };

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={handleCustomContainerCloseCallback}
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
                                    <Grid item xs={12} sm={12} md={2} lg={2}>
                                        <PrimaryButton
                                            type="submit"
                                            title="Search"
                                            className={
                                                classes.CustomPrimaryButtonStyle
                                            }
                                            style={{
                                                marginTop: '11px'
                                            }}
                                            fullWidth
                                            disabled={loadingOpen}
                                        >
                                            Search
                                        </PrimaryButton>

                                        {loadingOpen && (
                                            <CircularProgress
                                                size={24}
                                                className={
                                                    classes.buttonProgress
                                                }
                                            />
                                        )}
                                    </Grid>
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={6}
                                        lg={6}
                                    ></Grid>
                                    {IsCreditInstitution && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                        >
                                            <Box
                                                className={classes.flexReverse}
                                            >
                                                <Button
                                                    variant="contained"
                                                    color="primary"
                                                    className={
                                                        classes.CustomPrimaryButtonStyle
                                                    }
                                                    onClick={selectData}
                                                >
                                                    Refresh Bal
                                                </Button>
                                            </Box>
                                        </Grid>
                                    )}

                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={6}
                                        lg={6}
                                    ></Grid>

                                    {isServiceSubscription && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={3}
                                            lg={3}
                                        >
                                            <PrimaryButton
                                                type="Service Subscription Payment"
                                                title="Service Subscription Payment"
                                                style={{
                                                    fontSize: '13px',
                                                    fontWeight: '500',
                                                    //font-size: 13px;
                                                    margin: '10px'
                                                }}
                                                onClick={() => {
                                                    history.push(
                                                        '/SubscriptionPendingPayment'
                                                    );
                                                }}
                                                disabled={loadingOpen}
                                            >
                                                Service Subscription Payment
                                            </PrimaryButton>
                                        </Grid>
                                    )}
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={6}
                                        lg={6}
                                    ></Grid>
                                </Grid>
                            </Grid>
                        </Form>
                    );
                }}
            />
            <Grid container className="mb-5">
                <Grid item xs={12} sm={12} md={9} lg={9} xl={9}>
                    <div className="dashrechargepanel">
                        <DashboardShortCutMenu />
                    </div>
                    {isMonthlyGraphOnTop ? MonthlyGraphHideShow() : null}
                    <Grid container className="mb-5">
                        <Grid item xs={12} sm={12} md={12} lg={4} xl={4}>
                            <TodaysTransactions data={ServicewiseSale} />
                        </Grid>
                        <Grid item xs={12} sm={12} md={12} lg={8} xl={8}>
                            <ServiceSummary data={ServicewiseSale} />
                        </Grid>
                    </Grid>
                    {isMonthlyGraphOnBottom ? MonthlyGraphHideShow() : null}
                </Grid>

                <Grid item xs={12} sm={12} md={12} lg={3} xl={3}>
                    <div className={classes.rightpanel}>
                        <DashBoardTransactionDetails
                            transactionDetails={formObject}
                        />

                        <DashBoardWalletDetails wallets={WalletSummaryList} />
                        {/* <DashBoardTargetSalesDetails wallets={TargetSaleList} /> */}
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

            {isImageModelOpen && (
                <CustomModal
                    isOpen={isImageModelOpen}
                    title={BankName}
                    width="50%"
                    onDismissCallback={(value) => {
                        setIsImageModelOpen(value);
                    }}
                >
                    <QRImageView
                        ImageURL={ImageURL}
                        setLoading={(value) => setLoadingOpen(value)}
                    />
                </CustomModal>
            )}
        </CustomContainer>
    );
};
