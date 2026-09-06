import React, { useState, useEffect } from 'react';
import { Container, Grid, Paper, AppBar, Tab } from '@material-ui/core';
import { Voucher } from './Vourcher/Vorchers';
import AddPaymentVoucher from './Vourcher/AddPaymentVoucher';
import AddContraVoucher from './Vourcher/AddContraVoucher';
import TabList from '@material-ui/lab/TabList';
import TabContext from '@material-ui/lab/TabContext';
import TabPanel from '@material-ui/lab/TabPanel';
import { makeStyles } from '@material-ui/core/styles';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

const useStyles = makeStyles((theme) => ({
    marginTab: {
        padding: '0px',
        paddingTop: '24px',

        //marginRight: '-18px'
        overflowWrap: 'break-word'
    },
    appBarHeight: {
        height: 50,
        marginTop: '10px'
    },
    p10: {
        padding: 10
    },
    document_h4: {
        fontWeight: 'normal',
        fontSize: 16
    },
    blue_h4: {
        color: '#481BAB',
        fontWeight: 'bold',
        fontSize: 16
    },

    method: {
        fontSize: 16,
        color: '#FFA500',
        fontWeight: 'bold'
    },
    response: {
        fontSize: 16,
        color: '#00807A',
        fontWeight: 'bold'
    },
    Tab: {
        paddingLeft: '16.5%',
        paddingRight: '16.5%'
    }
}));
export const AddVoucher = () => {
    const classes = commonStyle();
    const classesInner = useStyles();
    const [tabIndex, setTabIndex] = useState('1');

    const handleChange = (event, newValue) => {
        setTabIndex(newValue);
    };

    return (
        <TabContext value={tabIndex}>
            <AppBar
                position="static"
                color="default"
                className={classesInner.appBarHeight}
            >
                <TabList onChange={handleChange} aria-label="recharge tabs">
                    <Tab
                        label="Receipt"
                        value="1"
                        className={classesInner.Tab}
                    />
                    <Tab
                        label="Payment"
                        value="2"
                        className={classesInner.Tab}
                    />
                    <Tab
                        label="Contra"
                        value="3"
                        className={classesInner.Tab}
                    />
                </TabList>
            </AppBar>
            <TabPanel value="1" className={classesInner.marginTab}>
                <Voucher />
            </TabPanel>
            <TabPanel value="2" className={classesInner.marginTab}>
                <AddPaymentVoucher />
            </TabPanel>
            <TabPanel value="3" className={classesInner.marginTab}>
                <AddContraVoucher />
            </TabPanel>
        </TabContext>
    );
};
