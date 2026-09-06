import { Grid, IconButton } from '@material-ui/core';
import AddmoneyIcon from 'Components/DashboardIcon/AddmoneyIcon';
import BroadbandIcon from 'Components/DashboardIcon/BroadbandIcon';
import DmtIcon from 'Components/DashboardIcon/DmtIcon';
import DthIcon from 'Components/DashboardIcon/DthIcon';
import ElectricityIcon from 'Components/DashboardIcon/ElectricityIcon';
import InsuranceIcon from 'Components/DashboardIcon/InsuranceIcon';
import PipedgasIcon from 'Components/DashboardIcon/PipedgasIcon';
import OfflineIcon from 'Components/DashboardIcon/OfflineIcon';
import CylinderGasIcon from 'Components/DashboardIcon/CylinderGasIcon';
import CreditCardIcon from 'Components/DashboardIcon/CreditCardIcon';
import PostpaidIcon from 'Components/DashboardIcon/PostpaidIcon';
import PrepaidIcon from 'Components/DashboardIcon/PrepaidIcon';
import CmsIcon from 'Components/DashboardIcon/CmsIcon';
import AEPSIcon from 'Components/DashboardIcon/AEPSIcon';
import UPITransferIcon from 'Components/DashboardIcon/UPITransferIcon';
import FasTagIcon from 'Components/DashboardIcon/FasTagIcon';
import LoanPaymentIcon from 'Components/DashboardIcon/LoanPaymentIcon';
import { API_USER, CLIENT_CODE } from 'Components/Helper/UsertypeCode';
import WaterBillIcon from 'Components/DashboardIcon/WaterbillIcon';
import WalletTopUpIcon from 'Components/DashboardIcon/WalletTopUpIcon';

import GameIcon from 'Components/DashboardIcon/GameIcon';
import FRCIcon from 'Components/DashboardIcon/FRCIcon';
import PanCardIcon from 'Components/DashboardIcon/PanCardIcon';
import AccountIcon from 'Components/DashboardIcon/Account';
import BUSIcon from 'Components/DashboardIcon/BusIcon';
import { CustomModal } from 'Components/Modal/CustomModal';
import OTTIcon from 'Components/DashboardIcon/OTTIcon';
import FingpayInsuranceIcon from 'Components/DashboardIcon/FingpayInsuranceIcon';
import { Button } from '@material-ui/core';
import ReactDOM from 'react-dom';
import {
    BROADBAND_CODE,
    CABLE_TV_CODE,
    DTH_RECHARGE_CODE,
    ELECTRICITY_CODE,
    FAST_TAG_CODE,
    GENERAL_INSURANCE_CODE,
    INSURANCE_CODE,
    LANDLINE_CODE,
    LOAN_PAYMENT_CODE,
    MONEY_TRANSFER_CODE,
    PIPED_GAS,
    Cylinder_GAS,
    POSTPAID_RECHARGE_CODE,
    PREPAID_RECHARGE_CODE,
    CMS_CODE,
    WATER_BILL_CODE,
    AEPS_CODE,
    PAYMENT_GATEWAY_CODE,
    FRC_RECHARGE_CODE,
    GAME_CODE,
    LAPU_LOADING_CODE,
    CREDITCARDPAYMENT_CODE,
    WALLETTOPUP_CODE,
    PAYOUT,
    UPITRANSFER_CODE,
    PANCARD_CODE,
    ACCOUNT_OPENING_CODE,
    DTH_BOOKING,
    VENDOR_PAY,
    OTT,
    LEAD_GENERATION,
    BUS_BOOKING,
    FEE_PAYMENT,
    MUNCIPAL_TAXES,
    TRAFFIC_CHLLAN,
    HOSPITAL,
    HOUSING_SOCIETY,
    HOSPITAL_PATHOLOGY,
    AFFILIATE_INSURANCE
} from 'Components/Helper/Messages';
import React, { useEffect, memo, useState } from 'react';
import { useHistory } from 'react-router';
import CableTVIcon from 'Components/DashboardIcon/CableTVIcon';
import GeneralInsuranceIcon from 'Components/DashboardIcon/GeneralInsuranceIcon';
import LandLineIcon from 'Components/DashboardIcon/LandLineIcon';
import {
    SelectAll as SERVICE_SELECT,
    SelectCMS as SELECT_ACTIVE
} from 'Components/Api/ServiceApi';
import { LOGOUT } from 'ConstComponents';
import ComingSoon from 'Assets/Images/ComingSoon.png';
import LeadGenerationIcon from 'Components/DashboardIcon/LeadGenerationIcon';
import VendorPayIcon from 'Components/DashboardIcon/VendorPayIcon';
import PayoutIcon from 'Components/DashboardIcon/PayoutIcon';
import FeePaymentIcon from 'Components/DashboardIcon/FeePayment';
import MunicipalTaxIcon from 'Components/DashboardIcon/MunicipalTaxIcon';
import TrafficChallanIcon from 'Components/DashboardIcon/TrafficChallan';
import HospitalIcon from 'Components/DashboardIcon/HospitalIcon';
import { SelectActiveService as SELECT_ACTIVESERVICE } from 'Components/Api/SubscriptionPlanAPI';
import { SubscriptionConfirmation } from 'Components/Alert/SubscriptionConfirmation';

function DashboardShortCutMenu(props) {
    const [IsImageModelOpen, setIsImageModelOpen] = useState(false);
    const history = useHistory();
    const [Services, setServices] = useState([]);
    const [cmsServices, setCmsServices] = useState([]);
    const [isClientRole, setIsClientRole] = useState(false);
    const [showMore, setShowMore] = useState(false);
    var currentUsertypeid = parseInt(localStorage.getItem('UserTypeID'));
    const [isSubscriptionOpen, setIsSubscriptionOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [serviceid, setServiceId] = useState(0);
    useEffect(() => {
        SelectServicesForDisplay();
        CheckUserRole();
    }, []);

    const CheckUserRole = () => {
        if (CLIENT_CODE === parseInt(currentUsertypeid)) {
            setIsClientRole(true);
        }
    };

    const SelectActiveService = async (serviceId) => {
        setServiceId(serviceId);
        var jsonReq = {
            ServiceID: serviceId
        };
        var requestObj = JSON.stringify(jsonReq);

        const response = await SELECT_ACTIVESERVICE(requestObj);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return false;

        return true;
    };

    const SelectServicesForDisplay = async () => {
        const response = await SERVICE_SELECT();

        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const handlePrepaidClick = async () => {
        const isActive = await SelectActiveService(PREPAID_RECHARGE_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: PREPAID_RECHARGE_CODE.toString()
        });
    };

    const handleCMSClick = () => {
        history.push('/CMSInitiatePS', {
            ServiceID: CMS_CODE.toString()
        });
    };

    const handleAEPSClick = async (iconName) => {
        const isActive = await SelectActiveService(AEPS_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/YBAEPS', {
            ServiceID: AEPS_CODE.toString(),
            IconName: iconName
        });
    };

    const handleDTHClick = async () => {
        const isActive = await SelectActiveService(DTH_RECHARGE_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: DTH_RECHARGE_CODE.toString()
        });
    };

    const handlePostPaidClick = () => {
        history.push('/Transaction', {
            ServiceID: POSTPAID_RECHARGE_CODE.toString()
        });
    };
    const handleDMTClick = async () => {
        const isActive = await SelectActiveService(MONEY_TRANSFER_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/DMTTransaction', {
            ServiceID: MONEY_TRANSFER_CODE.toString()
        });
    };

    const handleCreditCardBillClick = async () => {
        const isActive = await SelectActiveService(CREDITCARDPAYMENT_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/CreditCardPaymentIndex', {
            ServiceID: CREDITCARDPAYMENT_CODE.toString()
        });
    };
    const handleElectricityClick = async () => {
        const isActive = await SelectActiveService(ELECTRICITY_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: ELECTRICITY_CODE.toString()
        });
    };
    const handleBBPSClick = () => {
        history.push('/BBPSTransaction');
    };

    const handleWalletTopUpClick = () => {
        history.push('/WalletTopUpIndex', {
            ServiceID: WALLETTOPUP_CODE.toString()
        });
    };
    const handleInsuranceClick = async () => {
        const isActive = await SelectActiveService(INSURANCE_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: INSURANCE_CODE.toString()
        });
    };

    const handleFasTagClick = async () => {
        const isActive = await SelectActiveService(FAST_TAG_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: FAST_TAG_CODE.toString()
        });
    };

    const handleBroadbandClick = async () => {
        const isActive = await SelectActiveService(BROADBAND_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: BROADBAND_CODE.toString()
        });
    };
    const handlePipedGasClick = async () => {
        const isActive = await SelectActiveService(PIPED_GAS);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: PIPED_GAS.toString()
        });
    };
    const handleCylinderGasClick = async () => {
        const isActive = await SelectActiveService(Cylinder_GAS);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: Cylinder_GAS.toString()
        });
    };

    const handleWaterBillClick = async () => {
        const isActive = await SelectActiveService(WATER_BILL_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: WATER_BILL_CODE.toString()
        });
    };
    const handleLoanPaymentBillClick = async () => {
        const isActive = await SelectActiveService(LOAN_PAYMENT_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: LOAN_PAYMENT_CODE.toString()
        });
    };

    const handleCableTVBillClick = async () => {
        const isActive = await SelectActiveService(CABLE_TV_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: CABLE_TV_CODE.toString()
        });
    };

    const handleGeneralInsuranceBillClick = async () => {
        const isActive = await SelectActiveService(GENERAL_INSURANCE_CODE);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/Transaction', {
            ServiceID: GENERAL_INSURANCE_CODE.toString()
        });
    };

    const handleFingpayInsuranceBillClick = () => {
        history.push('/FPInitiate', {
            ServiceID: AFFILIATE_INSURANCE.toString()
        });
    };

    const handleLandLineBillClick = () => {
        history.push('/Transaction', {
            ServiceID: LANDLINE_CODE.toString()
        });
    };
    const handleAddMoenyClick = () => {
        history.push('/PaymentGatewayTransaction');
    };

    const handleFrcRechargeClick = () => {
        history.push('/Transaction', {
            ServiceID: FRC_RECHARGE_CODE.toString()
        });
    };

    const handleGameClick = () => {
        history.push('/Transaction', {
            ServiceID: GAME_CODE.toString()
        });
    };

    const handleFeePaymentClick = () => {
        history.push('/Transaction', {
            ServiceID: FEE_PAYMENT.toString()
        });
    };

    const handleMuncipleTaxesClick = () => {
        history.push('/Transaction', {
            ServiceID: MUNCIPAL_TAXES.toString()
        });
    };

    const handleTrafficChalanClick = () => {
        history.push('/Transaction', {
            ServiceID: TRAFFIC_CHLLAN.toString()
        });
    };

    const handleHospitalClick = () => {
        history.push('/Transaction', {
            ServiceID: HOSPITAL.toString()
        });
    };
    const handleHousingClick = () => {
        history.push('/Transaction', {
            ServiceID: HOUSING_SOCIETY.toString()
        });
    };
    const handleHospitalPathologyClick = () => {
        history.push('/Transaction', {
            ServiceID: HOSPITAL_PATHOLOGY.toString()
        });
    };

    const handleLeadGenerationClick = () => {
        history.push('/LeadGenerationIndex', {
            ServiceID: LEAD_GENERATION.toString()
        });
    };
    const handleFintechLeadGenerationClick = () => {
        history.push('/LeadGeneration', {
            ServiceID: LEAD_GENERATION.toString()
        });
    };

    const handlePayoutClick = async () => {
        const isActive = await SelectActiveService(PAYOUT);
        if (!isActive) {
            setIsSubscriptionOpen(true);
            return;
        }
        history.push('/PayoutV1', {
            ServiceID: PAYOUT.toString()
        });
    };
    const handleSmartPayClick = () => {
        history.push('/SmartPayTransaction', {
            ServiceID: VENDOR_PAY.toString()
        });
    };
    const handleTransfertoEbalanceClick = () => {
        history.push('/WalletTransfer', {
            ServiceID: VENDOR_PAY.toString()
        });
    };

    const handlePanCardClick = () => {
        history.push('/PanCardIndex', {
            ServiceID: PANCARD_CODE.toString()
        });
    };

    const handleUPITransferClick = () => {
        history.push('/UPITransfer', {
            ServiceID: UPITRANSFER_CODE.toString()
        });
    };

    const handleAccountOpeningClick = () => {
        history.push('/AccountOpening', {
            ServiceID: ACCOUNT_OPENING_CODE.toString()
        });
    };
    const handleDTHBookingClick = () => {
        history.push('/OfflineBook', {
            ServiceID: DTH_BOOKING.toString()
        });
    };

    const handleBusBookingClick = () => {
        history.push('/BusBookingIndex', {
            ServiceID: BUS_BOOKING.toString()
        });
    };

    const handleFingerPrintClick = () => {
        history.push('/BusBookingIndex', {
            ServiceID: BUS_BOOKING.toString()
        });
    };

    const handleOTTClick = () => {
        history.push('/OTTIndex', {
            ServiceID: OTT.toString()
        });
    };
    const handleOTTIFrameClick = () => {
        const iframeElement = (
            <Grid container spacing={1}>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <iframe
                        title="Loan"
                        src={'http://localhost:3000/OTTIndexApp'}
                        width="100%"
                        style={{
                            height: '50vh',
                            marginTop: '1px'
                        }}
                    ></iframe>
                </Grid>
            </Grid>
        );

        const container = document.getElementById('iframe-container');
        if (container) {
            ReactDOM.render(iframeElement, container);
        } else {
            console.error('Could not find container element for iframe');
        }
    };

    if (currentUsertypeid === API_USER) {
        return null;
    }
    const isDisabled = (serviceCode) => {
        let isOff = false;

        Services.map((element) => {
            if (parseInt(element.ServiceID) === parseInt(serviceCode)) {
                isOff = !element.IsDisplay;
            }
        });

        if (isOff === true) {
            return { display: 'none' };
        }
        return { display: '' };
    };

    const handleShowMore = () => {
        setShowMore(!showMore);
    };

    return (
        <>
            <Grid container>
                <Grid
                    item
                    xs={6}
                    sm={6}
                    md={4}
                    lg={2}
                    xl={2}
                    className="text-center"
                    style={isDisabled(PREPAID_RECHARGE_CODE)}
                >
                    <IconButton
                        className="dashmenu"
                        color="primary"
                        component="div"
                        onClick={handlePrepaidClick}
                    >
                        <PrepaidIcon />
                    </IconButton>
                </Grid>
                <Grid
                    item
                    xs={6}
                    sm={6}
                    md={4}
                    lg={2}
                    xl={2}
                    className="text-center"
                    style={isDisabled(DTH_RECHARGE_CODE)}
                >
                    <IconButton
                        color="primary"
                        className="dashmenu"
                        component="div"
                        onClick={handleDTHClick}
                    >
                        <DthIcon />
                    </IconButton>
                </Grid>

                {!isClientRole ? (
                    <Grid
                        item
                        xs={6}
                        sm={6}
                        md={4}
                        lg={2}
                        xl={2}
                        className="text-center"
                        style={isDisabled(MONEY_TRANSFER_CODE)}
                    >
                        <IconButton
                            color="primary"
                            className="dashmenu"
                            component="div"
                            onClick={handleDMTClick}
                        >
                            <DmtIcon name={'DMT'} />
                        </IconButton>
                    </Grid>
                ) : null}

                {!isClientRole ? (
                    <Grid
                        item
                        xs={6}
                        sm={6}
                        md={4}
                        lg={2}
                        xl={2}
                        className="text-center"
                        style={isDisabled(AEPS_CODE)}
                    >
                        <IconButton
                            className="dashmenu"
                            color="primary"
                            component="div"
                            onClick={() =>
                                handleAEPSClick('AEPS Cash Withdrawal')
                            }
                        >
                            <AEPSIcon name={'AEPS Cash Withdrawal'} />
                        </IconButton>
                    </Grid>
                ) : null}

                {!isClientRole ? (
                    <Grid
                        item
                        xs={6}
                        sm={6}
                        md={4}
                        lg={2}
                        xl={2}
                        className="text-center"
                        style={isDisabled(AEPS_CODE)}
                    >
                        <IconButton
                            className="dashmenu"
                            color="primary"
                            component="div"
                            onClick={() =>
                                handleAEPSClick('AEPS Balance Enquiry')
                            }
                        >
                            <AEPSIcon name={'AEPS Balance Enquiry'} />
                        </IconButton>
                    </Grid>
                ) : null}

                {!isClientRole ? (
                    <Grid
                        item
                        xs={6}
                        sm={6}
                        md={4}
                        lg={2}
                        xl={2}
                        className="text-center"
                        style={isDisabled(AEPS_CODE)}
                    >
                        <IconButton
                            className="dashmenu"
                            color="primary"
                            component="div"
                            onClick={() =>
                                handleAEPSClick('AEPS Mini Statement')
                            }
                        >
                            <AEPSIcon name={'AEPS Mini Statement'} />
                        </IconButton>
                    </Grid>
                ) : null}

                {!isClientRole && (
                    <Grid
                        item
                        xs={6}
                        sm={6}
                        md={4}
                        lg={2}
                        xl={2}
                        className="text-center"
                        style={isDisabled(PAYOUT)}
                    >
                        <IconButton
                            className="dashmenu"
                            color="primary"
                            component="div"
                            onClick={handlePayoutClick}
                        >
                            <PayoutIcon name={'Transfer to Bank A/c'} />
                        </IconButton>
                    </Grid>
                )}
                {!isClientRole && (
                    <Grid
                        item
                        xs={6}
                        sm={6}
                        md={4}
                        lg={2}
                        xl={2}
                        className="text-center"
                        style={isDisabled(PAYOUT)}
                    >
                        <IconButton
                            className="dashmenu"
                            color="primary"
                            component="div"
                            onClick={handleTransfertoEbalanceClick}
                        >
                            <PayoutIcon name={'Transfer to e-balance'} />
                        </IconButton>
                    </Grid>
                )}

                <Grid
                    item
                    xs={6}
                    sm={6}
                    md={4}
                    lg={2}
                    xl={2}
                    className="text-center"
                    style={isDisabled(ELECTRICITY_CODE)}
                >
                    <IconButton
                        color="primary"
                        className="dashmenu"
                        component="div"
                        onClick={handleElectricityClick}
                    >
                        <ElectricityIcon />
                    </IconButton>
                </Grid>
                <Grid
                    item
                    xs={6}
                    sm={6}
                    md={4}
                    lg={2}
                    xl={2}
                    className="text-center"
                    style={isDisabled(FAST_TAG_CODE)}
                >
                    <IconButton
                        color="primary"
                        className="dashmenu"
                        component="div"
                        onClick={handleFasTagClick}
                    >
                        <FasTagIcon />
                    </IconButton>
                </Grid>
                <Grid
                    item
                    xs={6}
                    sm={6}
                    md={4}
                    lg={2}
                    xl={2}
                    className="text-center"
                    style={isDisabled(POSTPAID_RECHARGE_CODE)}
                >
                    <IconButton
                        color="primary"
                        className="dashmenu"
                        component="div"
                        onClick={handlePostPaidClick}
                    >
                        <PostpaidIcon />
                    </IconButton>
                </Grid>
                <Grid
                    item
                    xs={6}
                    sm={6}
                    md={4}
                    lg={2}
                    xl={2}
                    className="text-center"
                    style={isDisabled(WATER_BILL_CODE)}
                >
                    <IconButton
                        color="primary"
                        className="dashmenu"
                        component="div"
                        onClick={handleWaterBillClick}
                    >
                        <WaterBillIcon />
                    </IconButton>
                </Grid>
                {showMore && (
                    <>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(BROADBAND_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleBroadbandClick}
                            >
                                <BroadbandIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(CABLE_TV_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleCableTVBillClick}
                            >
                                <CableTVIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(CREDITCARDPAYMENT_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleCreditCardBillClick}
                            >
                                <CreditCardIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(Cylinder_GAS)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleCylinderGasClick}
                            >
                                <CylinderGasIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(FEE_PAYMENT)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleFeePaymentClick}
                            >
                                <FeePaymentIcon name={'Fee Payment'} />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(PIPED_GAS)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePipedGasClick}
                            >
                                <PipedgasIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(HOSPITAL)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleHospitalClick}
                            >
                                <HospitalIcon name={'Hospital'} />
                            </IconButton>
                        </Grid>

                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(LANDLINE_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleLandLineBillClick}
                            >
                                <LandLineIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(MUNCIPAL_TAXES)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleMuncipleTaxesClick}
                            >
                                <MunicipalTaxIcon name={'Munciple Taxes'} />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(INSURANCE_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleInsuranceClick}
                            >
                                <InsuranceIcon />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(LOAN_PAYMENT_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleLoanPaymentBillClick}
                            >
                                <LoanPaymentIcon name="Loan Payment" />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(PAYMENT_GATEWAY_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleAddMoenyClick}
                            >
                                <AddmoneyIcon />
                            </IconButton>
                        </Grid>
                        <>
                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(CMS_CODE)}
                            >
                                <IconButton
                                    className="dashmenu"
                                    color="primary"
                                    component="div"
                                    onClick={handleCMSClick}
                                >
                                    <CmsIcon name={'CMS'} />
                                </IconButton>
                            </Grid>

                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(TRAFFIC_CHLLAN)}
                            >
                                <IconButton
                                    color="primary"
                                    className="dashmenu"
                                    component="div"
                                    onClick={handleTrafficChalanClick}
                                >
                                    <TrafficChallanIcon
                                        name={'Traffic Challan'}
                                    />
                                </IconButton>
                            </Grid>

                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(GENERAL_INSURANCE_CODE)}
                            >
                                <IconButton
                                    color="primary"
                                    className="dashmenu"
                                    component="div"
                                    onClick={handleGeneralInsuranceBillClick}
                                >
                                    <GeneralInsuranceIcon />
                                </IconButton>
                            </Grid>
                        </>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(GAME_CODE)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleGameClick}
                            >
                                <GameIcon />
                            </IconButton>
                        </Grid>
                        {!isClientRole ? (
                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(FRC_RECHARGE_CODE)}
                            >
                                <IconButton
                                    color="primary"
                                    className="dashmenu"
                                    component="div"
                                    onClick={handleFrcRechargeClick}
                                >
                                    <FRCIcon />
                                </IconButton>
                            </Grid>
                        ) : null}
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(LEAD_GENERATION)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleLeadGenerationClick}
                            >
                                <LeadGenerationIcon
                                    name={'Lead Generation'}
                                    width={64}
                                    height={64}
                                    fill="red"
                                />
                            </IconButton>
                            <div id="iframe-container"></div>
                        </Grid>
                        {!isClientRole && (
                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(DTH_BOOKING)}
                            >
                                <IconButton
                                    color="primary"
                                    className="dashmenu"
                                    component="div"
                                    onClick={handleDTHBookingClick}
                                >
                                    <OfflineIcon />
                                </IconButton>
                            </Grid>
                        )}
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleBusBookingClick}
                            >
                                <BUSIcon
                                    name={'Bus Booking'}
                                    width={64}
                                    height={64}
                                    fill="red"
                                />
                            </IconButton>
                        </Grid>
                        <Grid
                            item
                            xs={6}
                            sm={6}
                            md={4}
                            lg={2}
                            xl={2}
                            className="text-center"
                            style={isDisabled(OTT)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleOTTClick}
                            >
                                <OTTIcon
                                    name={'OTT'}
                                    width={64}
                                    height={64}
                                    fill="red"
                                />
                            </IconButton>

                            <div id="iframe-container"></div>
                        </Grid>
                        {!isClientRole ? (
                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(ACCOUNT_OPENING_CODE)}
                            >
                                <IconButton
                                    className="dashmenu"
                                    color="primary"
                                    component="div"
                                    onClick={handleAccountOpeningClick}
                                >
                                    <AccountIcon name={'Account Opening'} />
                                </IconButton>
                            </Grid>
                        ) : null}
                        {!isClientRole ? (
                            <Grid
                                item
                                xs={6}
                                sm={6}
                                md={4}
                                lg={2}
                                xl={2}
                                className="text-center"
                                style={isDisabled(PANCARD_CODE)}
                            >
                                <IconButton
                                    color="primary"
                                    className="dashmenu"
                                    component="div"
                                    onClick={handlePanCardClick}
                                >
                                    <PanCardIcon />
                                </IconButton>
                            </Grid>
                        ) : null}
                        {cmsServices.map(
                            (service) =>
                                service.Name === 'FingpayInsurance' && (
                                    <Grid
                                        item
                                        xs={6}
                                        sm={6}
                                        md={4}
                                        lg={2}
                                        xl={2}
                                        className="text-center"
                                        style={isDisabled(AFFILIATE_INSURANCE)}
                                        key={service.id} // Ensure a unique key for each child in the list
                                    >
                                        <IconButton
                                            color="primary"
                                            className="dashmenu"
                                            component="div"
                                            onClick={
                                                handleFingpayInsuranceBillClick
                                            }
                                        >
                                            <FingpayInsuranceIcon />
                                        </IconButton>
                                    </Grid>
                                )
                        )}
                    </>
                )}
                <Grid item xs={12} className="text-center">
                    <Button onClick={handleShowMore}>
                        {showMore ? 'Show Less' : 'Show More'}
                    </Button>
                </Grid>
            </Grid>

            {IsImageModelOpen && (
                <CustomModal
                    isOpen={IsImageModelOpen}
                    title="Comming Soon..........."
                    width="40%"
                    onDismissCallback={(value) => {
                        setIsImageModelOpen(value);
                    }}
                >
                    <Grid container spacing={2}>
                        <Grid xs={12} sm={12} md={12} lg={12}>
                            <Grid xs={12} sm={12} md={3} lg={3}></Grid>
                            <Grid xs={12} sm={12} md={6} lg={6}>
                                <div>
                                    <img src={ComingSoon} />
                                </div>
                            </Grid>
                        </Grid>
                    </Grid>
                </CustomModal>
            )}

            {isSubscriptionOpen ? (
                <SubscriptionConfirmation
                    isOpen={isSubscriptionOpen}
                    request={confirmAlertRequest}
                    message={'Upgrade your subscription plan'}
                    positiveButtonClick={() => {
                        setIsSubscriptionOpen(false);
                        let params = {
                            ServiceID: serviceid
                        };
                        history.push('/Subscription', params);
                    }}
                    negativeButtonClick={() => {
                        setIsSubscriptionOpen(false);
                    }}
                />
            ) : null}
        </>
    );
}

export default memo(DashboardShortCutMenu);
