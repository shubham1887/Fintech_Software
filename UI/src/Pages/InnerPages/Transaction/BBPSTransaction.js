import {
    Container,
    Grid,
    Paper,
    Typography,
    IconButton
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import BroadbandIcon from 'Components/DashboardIcon/BroadbandIcon';
import ElectricityIcon from 'Components/DashboardIcon/ElectricityIcon';
import InsuranceIcon from 'Components/DashboardIcon/InsuranceIcon';
import PipedgasIcon from 'Components/DashboardIcon/PipedgasIcon';
import CylinderGasIcon from 'Components/DashboardIcon/CylinderGasIcon';
import PostpaidIcon from 'Components/DashboardIcon/PostpaidIcon';
import FasTagIcon from 'Components/DashboardIcon/FasTagIcon';
import LoanPaymentIcon from 'Components/DashboardIcon/LoanPaymentIcon';
import { API_USER } from 'Components/Helper/UsertypeCode';
import WaterBillIcon from 'Components/DashboardIcon/WaterbillIcon';
import WalletTopUpIcon from 'Components/DashboardIcon/WalletTopUpIcon';
import GameIcon from 'Components/DashboardIcon/GameIcon';
import BBPS_VERTICAL_LOGO from 'Assets/Images/BBPS/BBPS-Vertical-06.png';
import DmtIcon from 'Components/DashboardIcon/DmtIcon';
import LeadGenerationIcon from 'Components/DashboardIcon/LeadGenerationIcon';
import CreditCardIcon from 'Components/DashboardIcon/CreditCardIcon';
import DthIcon from 'Components/DashboardIcon/DthIcon';
import PrepaidIcon from 'Components/DashboardIcon/PrepaidIcon';

import BUSIcon from 'Components/DashboardIcon/BusIcon';

import {
    BROADBAND_CODE,
    CABLE_TV_CODE,
    ELECTRICITY_CODE,
    FAST_TAG_CODE,
    GENERAL_INSURANCE_CODE,
    PREPAID_RECHARGE_CODE,
    INSURANCE_CODE,
    LANDLINE_CODE,
    LOAN_PAYMENT_CODE,
    PIPED_GAS,
    Cylinder_GAS,
    POSTPAID_RECHARGE_CODE,
    WATER_BILL_CODE,
    GAME_CODE,
    HOUSING_SOCIETY,
    WALLETTOPUP_CODE,
    DTH_RECHARGE_CODE,
    FEE_PAYMENT,
    MUNCIPAL_TAXES,
    TRAFFIC_CHLLAN,
    HOSPITAL
} from 'Components/Helper/Messages';
import React, { useEffect, useState } from 'react';
import { useHistory } from 'react-router';
import CableTVIcon from 'Components/DashboardIcon/CableTVIcon';
import GeneralInsuranceIcon from 'Components/DashboardIcon/GeneralInsuranceIcon';
import HealthInsuranceIcon from 'Components/DashboardIcon/HealthInsurance';
import LandLineIcon from 'Components/DashboardIcon/LandLineIcon';
import { SelectBBPSEnabledServices as SERVICE_SELECT } from 'Components/Api/ServiceApi';
import { LOGOUT } from 'ConstComponents';

import FeePaymentIcon from 'Components/DashboardIcon/FeePayment';
import MunicipalTaxIcon from 'Components/DashboardIcon/MunicipalTaxIcon';
import TrafficChallanIcon from 'Components/DashboardIcon/TrafficChallan';
import HospitalIcon from 'Components/DashboardIcon/HospitalIcon';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const BBPSTransaction = () => {
    const history = useHistory();
    const [Services, setServices] = useState([]);
    const classes = commonStyle();

    var currentUsertypeid = parseInt(localStorage.getItem('UserTypeID'));

    useEffect(() => {
        SelectBBPSServices();
    }, []);

    const SelectBBPSServices = async () => {
        const response = await SERVICE_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setServices(data);
    };

    const handlePostPaidClick = () => {
        history.push('/Transaction', {
            ServiceID: POSTPAID_RECHARGE_CODE.toString()
        });
    };
    const handleElectricityClick = () => {
        history.push('/Transaction', {
            ServiceID: ELECTRICITY_CODE.toString()
        });
    };
    const handleWalletTopUpClick = () => {
        history.push('/WalletTopUpIndex', {
            ServiceID: WALLETTOPUP_CODE.toString()
        });
    };
    const handleInsuranceClick = () => {
        history.push('/Transaction', {
            ServiceID: INSURANCE_CODE.toString()
        });
    };
    const handleFasTagClick = () => {
        history.push('/Transaction', {
            ServiceID: FAST_TAG_CODE.toString()
        });
    };

    const handleBroadbandClick = () => {
        history.push('/Transaction', {
            ServiceID: BROADBAND_CODE.toString()
        });
    };
    const handlePipedGasClick = () => {
        history.push('/Transaction', {
            ServiceID: PIPED_GAS.toString()
        });
    };
    const handleCylinderGasClick = () => {
        history.push('/Transaction', {
            ServiceID: Cylinder_GAS.toString()
        });
    };

    const handleWaterBillClick = () => {
        history.push('/Transaction', {
            ServiceID: WATER_BILL_CODE.toString()
        });
    };
    const handleLoanPaymentBillClick = () => {
        history.push('/Transaction', {
            ServiceID: LOAN_PAYMENT_CODE.toString()
        });
    };
    const handlePrepaidClick = () => {
        history.push('/Transaction', {
            ServiceID: PREPAID_RECHARGE_CODE.toString()
        });
    };

    const handleCableTVBillClick = () => {
        history.push('/Transaction', {
            ServiceID: CABLE_TV_CODE.toString()
        });
    };

    const handleGeneralInsuranceBillClick = () => {
        history.push('/Transaction', {
            ServiceID: GENERAL_INSURANCE_CODE.toString()
        });
    };

    const handleDTHClick = () => {
        history.push('/Transaction', {
            ServiceID: DTH_RECHARGE_CODE.toString()
        });
    };

    const handleLandLineBillClick = () => {
        history.push('/Transaction', {
            ServiceID: LANDLINE_CODE.toString()
        });
    };

    const handleGameClick = () => {
        history.push('/Transaction', {
            ServiceID: GAME_CODE.toString()
        });
    };

    const handleHousingClick = () => {
        history.push('/Transaction', {
            ServiceID: HOUSING_SOCIETY.toString()
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

    const BBPSVerticalLogoImage = () => {
        return (
            <img
                src={BBPS_VERTICAL_LOGO}
                style={{ float: 'right', marginRight: '25px' }}
                alt="BBPS"
            />
        );
    };

    return (
        <>
            <Grid item xs={12}>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={10} lg={10} xl={10}>
                        <h2
                            className={classes.PaperHead_h2}
                            style={{ marginLeft: '40px', marginTop: '30px' }}
                        >
                            Bharat Bill Payment System (BBPS) Billers
                        </h2>
                    </Grid>

                    <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <BBPSVerticalLogoImage />
                    </Grid>
                </Grid>{' '}
            </Grid>
            <CustomContainer>
                <Paper elevation={0} className={classes.PaperStyle}>
                    <Grid container>
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
                            style={isDisabled(FEE_PAYMENT)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handleFeePaymentClick}
                            >
                                <FeePaymentIcon name={'Education Fees'} />
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <HospitalIcon
                                    name={'Housing Society'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <LoanPaymentIcon
                                    name={'Donation'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <HospitalIcon
                                    name={'Hospital And Pathology'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <MunicipalTaxIcon
                                    name={'Municipal Services'}
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
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <LeadGenerationIcon
                                    name={'Clubs and Associations'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <HospitalIcon
                                    name={'Rental'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <DmtIcon
                                    name={'Recurring Deposit'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <LeadGenerationIcon
                                    name={'Subscription'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <CreditCardIcon
                                    name={'Credit Card'}
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
                            //style={isDisabled(BUS_BOOKING)}
                        >
                            <IconButton
                                color="primary"
                                className="dashmenu"
                                component="div"
                                onClick={handlePostPaidClick}
                            >
                                <HealthInsuranceIcon
                                    name={'Health Insurance'}
                                    width={64}
                                    height={64}
                                    fill="red"
                                />
                            </IconButton>
                        </Grid>
                    </Grid>
                </Paper>
            </CustomContainer>
        </>
    );
};
