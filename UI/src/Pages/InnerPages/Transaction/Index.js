import React, { useState, useEffect } from 'react';
import { Container, Grid, Tab, Paper } from '@material-ui/core';
import TabContext from '@material-ui/lab/TabContext';
import TabList from '@material-ui/lab/TabList';
import TabPanel from '@material-ui/lab/TabPanel';

import {
    PREPAID_RECHARGE_CODE,
    CMS_CODE,
    POSTPAID_RECHARGE_CODE,
    DTH_RECHARGE_CODE,
    ELECTRICITY_CODE,
    INSURANCE_CODE,
    FAST_TAG_CODE,
    PIPED_GAS,
    Cylinder_GAS,
    WATER_BILL_CODE,
    LOAN_PAYMENT_CODE,
    CABLE_TV_CODE,
    LANDLINE_CODE,
    BROADBAND_CODE,
    GENERAL_INSURANCE_CODE,
    FRC_RECHARGE_CODE,
    GAME_CODE,
    FEE_PAYMENT,
    MUNCIPAL_TAXES,
    TRAFFIC_CHLLAN,
    HOSPITAL,
    HOSPITAL_PATHOLOGY,
    HOUSING_SOCIETY
} from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { Prepaid } from 'Pages/InnerPages/Transaction/PrePaid';
import { DTH } from 'Pages/InnerPages/Transaction/DTH';
import { FRC } from 'Pages/InnerPages/Transaction/FRC';
import { Game } from 'Pages/InnerPages/Transaction/Game';

import { Utility } from 'Pages/InnerPages/Transaction/Utility';

import { ElectricityIcon } from './ServiceIcon/ElectricityIcon';
import { PrepaidIcon } from './ServiceIcon/PrepaidIcon';
import { CmsIcon } from './ServiceIcon/CmsIcon';
import { DthIcon } from './ServiceIcon/DthIcon';
import { PostpaidIcon } from './ServiceIcon/PostpaidIcon';

import { InsuranceIcon } from './ServiceIcon/InsuranceIcon';
import { FasttagIcon } from './ServiceIcon/FasttagIcon';
import { PipedgasIcon } from './ServiceIcon/PipedgasIcon';
import { WaterbillIcon } from './ServiceIcon/WaterbillIcon';
import SvgIcon from '@material-ui/core/SvgIcon';
import { LoanpaymentIcon } from './ServiceIcon/LoanpaymentIcon';
import { CabletvIcon } from './ServiceIcon/CabletvIcon';
import { LandlineIcon } from './ServiceIcon/LandlineIcon';
import { GameIcon } from './ServiceIcon/GameIcon';

import { BroadbandIcon } from './ServiceIcon/BroadbandIcon';
import { GeneralInsuranceIcon } from './ServiceIcon/GeneralInsuranceIcon';
import { useLocation } from 'react-router';
import { SelectAll as SERVICE_SELECT } from 'Components/Api/ServiceApi';
import BBPS_HORIZONTAL_LOGO from 'Assets/Images/BBPS/BBPS-Horizontal-05.png';
import { LOGOUT } from 'ConstComponents';

const CustomContainer = HOCLoader(Container);
const margin = {
    padding: '0px',
    paddingTop: '12px',
    marginLeft: '-24px'
};
export const Transaction = ({ ServiceIDString }) => {
    const location = useLocation();
    const [value, setValue] = useState(PREPAID_RECHARGE_CODE.toString());
    const [loadingOpen] = useState(false);
    const [responseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const [severity] = useState('success');
    const [tabGridValue, setTabGridValue] = useState(12);
    const [refreshRandom, setRefreshRandom] = useState(1);
    const [Services, setServices] = useState([]);
    var IsMobikwikBBPS = localStorage.getItem('IsMobikwikBBPS') === 'true';
    useEffect(() => {
        if (ServiceIDString !== undefined && ServiceIDString !== null) {
            setValue(ServiceIDString.toString());
        }
    }, [ServiceIDString]);

    useEffect(() => {
        let { state } = location;

        if (state === null) {
            return;
        }

        if (!state) return;
        let { ServiceID } = state;
        setValue(ServiceID);
    }, [location]);

    useEffect(() => {
        if (IsMobikwikBBPS) setTabGridValue(10);
    }, [IsMobikwikBBPS]);

    useEffect(() => {
        SelectServicesForDisplay();
    }, []);

    const SelectServicesForDisplay = async () => {
        const response = await SERVICE_SELECT();
        if (!response.isAuthorized) {
            LOGOUT();
        }

        var data = JSON.parse(response.data);
        setServices(data);
    };
    const handleChange = (event, newValue) => {
        setValue(newValue);
    };

    const isVisible = (serviceCode, category) => {
        if (parseInt(value) === parseInt(serviceCode)) {
            return true;
        }
        const valueCategory = getCategory(value);
        if (category === valueCategory) {
            return true;
        }
        return false;
    };

    const getCategory = (serviceCode) => {
        return Services.find(
            (service) => parseInt(service.ServiceID) === parseInt(serviceCode)
        )?.ServiceCategory;
    };

    const isDisabled = (serviceCode) => {
        let isOff = false;

        const Category = getCategory(serviceCode);

        if (!isVisible(serviceCode, Category)) {
            isOff = true;
        }

        if (isOff === true) {
            return {
                display: 'none'
            };
        }
        return {
            display: ''
        };
    };

    const BBPSHorizontalLogoImage = () => {
        return (
            <img
                src={BBPS_HORIZONTAL_LOGO}
                style={{
                    float: 'right',
                    marginRight: '25px'
                }}
                alt="BBPS"
            />
        );
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
            <Grid
                item
                xs={12}
                style={{
                    marginTop: '10px'
                }}
            >
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        <TabContext value={value}>
                            <Paper square className="tabpanel">
                                <Grid container spacing={2}>
                                    <Grid
                                        item
                                        xs={12}
                                        sm={12}
                                        md={10}
                                        lg={tabGridValue}
                                        xl={tabGridValue}
                                    >
                                        <TabList
                                            variant="scrollable"
                                            scrollButtons="on"
                                            onChange={handleChange}
                                            aria-label="simple tabs example"
                                            indicatorColor="primary"
                                            textColor="primary"
                                            className="servicename"
                                        >
                                            <Tab
                                                label="Prepaid"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={PREPAID_RECHARGE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={PREPAID_RECHARGE_CODE.toString()}
                                                style={isDisabled(
                                                    PREPAID_RECHARGE_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Cms"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <CmsIcon
                                                            serviceID={value}
                                                            value={CMS_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={CMS_CODE.toString()}
                                                style={isDisabled(CMS_CODE)}
                                            />
                                            <Tab
                                                label="DTH"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <DthIcon
                                                            serviceID={value}
                                                            value={DTH_RECHARGE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={DTH_RECHARGE_CODE.toString()}
                                                style={isDisabled(
                                                    DTH_RECHARGE_CODE
                                                )}
                                            />
                                            <Tab
                                                label="PostPaid"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PostpaidIcon
                                                            serviceID={value}
                                                            value={POSTPAID_RECHARGE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={POSTPAID_RECHARGE_CODE.toString()}
                                                style={isDisabled(
                                                    POSTPAID_RECHARGE_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Electricity"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <ElectricityIcon
                                                            serviceID={value}
                                                            value={ELECTRICITY_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={ELECTRICITY_CODE.toString()}
                                                style={isDisabled(
                                                    ELECTRICITY_CODE
                                                )}
                                            />
                                            <Tab
                                                label="FASTag"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <FasttagIcon
                                                            serviceID={value}
                                                            value={FAST_TAG_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={FAST_TAG_CODE.toString()}
                                                style={isDisabled(
                                                    FAST_TAG_CODE
                                                )}
                                            />
                                            <Tab
                                                label="FRC"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={FRC_RECHARGE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={FRC_RECHARGE_CODE.toString()}
                                                style={isDisabled(
                                                    FRC_RECHARGE_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Game"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <GameIcon
                                                            serviceID={value}
                                                            value={GAME_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={GAME_CODE.toString()}
                                                style={isDisabled(GAME_CODE)}
                                            />
                                            <Tab
                                                label="Insurance"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <InsuranceIcon
                                                            serviceID={value}
                                                            value={INSURANCE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={INSURANCE_CODE.toString()}
                                                style={isDisabled(
                                                    INSURANCE_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Piped Gas"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PipedgasIcon
                                                            serviceID={value}
                                                            value={PIPED_GAS.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={PIPED_GAS.toString()}
                                                style={isDisabled(PIPED_GAS)}
                                            />
                                            <Tab
                                                label="Cylinder Gas"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PipedgasIcon
                                                            serviceID={value}
                                                            value={Cylinder_GAS.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={Cylinder_GAS.toString()}
                                                style={isDisabled(Cylinder_GAS)}
                                            />
                                            <Tab
                                                label="Water Bill"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <WaterbillIcon
                                                            serviceID={value}
                                                            value={WATER_BILL_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={WATER_BILL_CODE.toString()}
                                                style={isDisabled(
                                                    WATER_BILL_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Cable TV"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <CabletvIcon
                                                            serviceID={value}
                                                            value={CABLE_TV_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={CABLE_TV_CODE.toString()}
                                                style={isDisabled(
                                                    CABLE_TV_CODE
                                                )}
                                            />
                                            <Tab
                                                label="General Insurance"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <GeneralInsuranceIcon
                                                            serviceID={value}
                                                            value={GENERAL_INSURANCE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={GENERAL_INSURANCE_CODE.toString()}
                                                style={isDisabled(
                                                    GENERAL_INSURANCE_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Broadband"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <BroadbandIcon
                                                            serviceID={value}
                                                            value={BROADBAND_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={BROADBAND_CODE.toString()}
                                                style={isDisabled(
                                                    BROADBAND_CODE
                                                )}
                                            />
                                            <Tab
                                                label="Landline"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <LandlineIcon
                                                            serviceID={value}
                                                            value={LANDLINE_CODE.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={LANDLINE_CODE.toString()}
                                                style={isDisabled(
                                                    LANDLINE_CODE
                                                )}
                                            />

                                            <Tab
                                                label="Fee Payment"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={FEE_PAYMENT.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={FEE_PAYMENT.toString()}
                                                style={isDisabled(FEE_PAYMENT)}
                                            />
                                            <Tab
                                                label="Municipal Taxes"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={MUNCIPAL_TAXES.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={MUNCIPAL_TAXES.toString()}
                                                style={isDisabled(
                                                    MUNCIPAL_TAXES
                                                )}
                                            />
                                            <Tab
                                                label="Traffic Challan"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={TRAFFIC_CHLLAN.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={TRAFFIC_CHLLAN.toString()}
                                                style={isDisabled(
                                                    TRAFFIC_CHLLAN
                                                )}
                                            />
                                            <Tab
                                                label="Hospital"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={HOSPITAL.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={HOSPITAL.toString()}
                                                style={isDisabled(HOSPITAL)}
                                            />
                                            <Tab
                                                label="Hospital and Pathology"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={HOSPITAL_PATHOLOGY.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={HOSPITAL_PATHOLOGY.toString()}
                                                style={isDisabled(
                                                    HOSPITAL_PATHOLOGY
                                                )}
                                            />
                                            <Tab
                                                label="Housing Society"
                                                icon={
                                                    <SvgIcon className="serviceicon">
                                                        <PrepaidIcon
                                                            serviceID={value}
                                                            value={HOUSING_SOCIETY.toString()}
                                                        />
                                                    </SvgIcon>
                                                }
                                                value={HOUSING_SOCIETY.toString()}
                                                style={isDisabled(
                                                    HOUSING_SOCIETY
                                                )}
                                            />
                                        </TabList>
                                    </Grid>
                                    {IsMobikwikBBPS && (
                                        <Grid
                                            item
                                            xs={12}
                                            sm={12}
                                            md={2}
                                            lg={2}
                                            xl={2}
                                        >
                                            <BBPSHorizontalLogoImage />
                                        </Grid>
                                    )}
                                </Grid>
                            </Paper>
                            <TabPanel
                                value={PREPAID_RECHARGE_CODE.toString()}
                                style={margin}
                            >
                                <Prepaid
                                    serviceID={PREPAID_RECHARGE_CODE.toString()}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={CMS_CODE.toString()}
                                style={margin}
                            >
                                <Prepaid
                                    serviceID={CMS_CODE.toString()}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={DTH_RECHARGE_CODE.toString()}
                                style={margin}
                            >
                                <DTH
                                    serviceID={DTH_RECHARGE_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={POSTPAID_RECHARGE_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={POSTPAID_RECHARGE_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={ELECTRICITY_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={ELECTRICITY_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={FAST_TAG_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={FAST_TAG_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={FRC_RECHARGE_CODE.toString()}
                                style={margin}
                            >
                                <FRC
                                    serviceID={FRC_RECHARGE_CODE.toString()}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={GAME_CODE.toString()}
                                style={margin}
                            >
                                <Game
                                    serviceID={GAME_CODE.toString()}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={INSURANCE_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={INSURANCE_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={PIPED_GAS.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={PIPED_GAS}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={Cylinder_GAS.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={Cylinder_GAS}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={WATER_BILL_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={WATER_BILL_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={CABLE_TV_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={CABLE_TV_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={LANDLINE_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={LANDLINE_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={BROADBAND_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={BROADBAND_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={GENERAL_INSURANCE_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={GENERAL_INSURANCE_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={LOAN_PAYMENT_CODE.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={LOAN_PAYMENT_CODE}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={FEE_PAYMENT.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={FEE_PAYMENT}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>

                            <TabPanel
                                value={MUNCIPAL_TAXES.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={MUNCIPAL_TAXES}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={TRAFFIC_CHLLAN.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={TRAFFIC_CHLLAN}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                            <TabPanel
                                value={HOSPITAL.toString()}
                                style={margin}
                            >
                                <Utility
                                    serviceID={HOSPITAL}
                                    setRefreshRandom={(value) => {
                                        setRefreshRandom(value);
                                    }}
                                />
                            </TabPanel>
                           
                        </TabContext>
                    </Grid>
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
