import React, { useState, useEffect } from 'react';
import { Container, Grid, Tab, Paper } from '@material-ui/core';
import TabContext from '@material-ui/lab/TabContext';
import TabList from '@material-ui/lab/TabList';
import TabPanel from '@material-ui/lab/TabPanel';
import { PREPAID_RECHARGE_CODE } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PaysprintAadhar } from 'Pages/InnerPages/eKYC/Paysprint_Aadhar';
import { PaysprintPan } from 'Pages/InnerPages/eKYC/Paysprint_Pan';
import { PaysprintGST } from 'Pages/InnerPages/eKYC/Paysprint_GST';

import { useLocation } from 'react-router';
import { LOGOUT } from 'ConstComponents';
const CustomContainer = HOCLoader(Container);
const margin = {
    padding: '0px',
    paddingTop: '12px',
    marginLeft: '-24px'
};
export const IndexPaysprint = () => {
    const location = useLocation();
    const [value, setValue] = useState(PREPAID_RECHARGE_CODE.toString());
    const [loadingOpen] = useState(false);
    const [responseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity] = useState('success');
    const [tabIndex, setTabIndex] = useState('1');
    useEffect(() => {
        // SelectServicesForDisplay();
    }, []);
    const handleChange = (event, newValue) => {
        setTabIndex(newValue);
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
            <Grid item xs={12} style={{ marginTop: '10px' }}>
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        <TabContext value={tabIndex}>
                            <Paper square className="tabpanel">
                                <TabList
                                    variant="scrollable"
                                    scrollButtons="on"
                                    onChange={handleChange}
                                    aria-label="simple tabs example"
                                    indicatorColor="primary"
                                    textColor="primary"
                                    className="servicename"
                                >
                                    <Tab label="Aadhar / Pan" value="1" />
                                    {/* <Tab label="Pan Number" value="2" /> */}
                                    <Tab label="GST" value="3" />
                                </TabList>
                            </Paper>
                            <TabPanel value="1" style={margin}>
                                <PaysprintAadhar />
                            </TabPanel>
                            <TabPanel value="2" style={margin}>
                                <PaysprintPan />
                            </TabPanel>
                            <TabPanel value="3" style={margin}>
                                <PaysprintGST />
                            </TabPanel>
                        </TabContext>
                    </Grid>
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
