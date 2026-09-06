import React, { useState, memo, useEffect } from 'react';
import { Grid, Paper } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import Logo from 'Components/Logo/Logo';
import { CLIENT_NAME } from 'Config';
const useStyles = makeStyles((theme) => ({
    marginTab: {
        padding: '0px',
        paddingTop: '24px',
        marginRight: '-18px'
    },
    borderPaper: {
        border: '1px solid'
    },
    appBarHeight: {
        height: 50
    },
    p10: {
        padding: 10
    },
    document_h3: {
        fontWeight: 'normal',
        fontSize: 30
    },
    document_h2: {
        fontWeight: 'bold',
        fontSize: 35
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
    center: {
        textAlign: 'center'
    }
}));

const DigitalCertificatePrint = () => {
    const classesInner = useStyles();
    const [FirmName, setFirmName] = useState(localStorage.getItem('FirmName'));
    const [cName, setCName] = useState('');

    const fetchCName = async () => {
        setCName(await CLIENT_NAME());
    };

    useEffect(() => {
        fetchCName();
    }, []);

    return (
        <Paper className={classesInner.borderPaper}>
            <Grid container spacing={2} className={classesInner.p10}>
                <Grid item xs={4} sm={4} md={4} lg={4}>
                    <Logo />
                </Grid>

                <Grid item xs={8} sm={8} md={8} lg={8}>
                    <h3 className={classesInner.document_h3}>CERTIFICATE OF</h3>
                    <h2 className={classesInner.document_h2}>
                        DIGITAL PRADHAN
                    </h2>
                </Grid>
            </Grid>
            <Grid className={classesInner.p10} container spacing={2}>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <p className={classesInner.center}>
                        THIS IS TO CERTIFY THAT
                    </p>
                    <p className={classesInner.center}>{FirmName}</p>
                    <p className={classesInner.center}>DIGITAL PRADHAN</p>
                    <p className={classesInner.center}>
                        I am proud to be appointed a DIGITAL PRADHAN for
                        services provided through {cName}
                    </p>
                </Grid>
            </Grid>
            <Grid className={classesInner.p10} container spacing={2}>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <Grid className={classesInner.borderPaper}>
                        <p className={classesInner.center}>
                            Oath of DIGITAL PRADHAN
                        </p>
                        <p className={classesInner.center}>
                            As a Digital Pradhan, I hereby solemnly affirm under
                            oath and undertake that at all times, I:
                        </p>
                        <p className={classesInner.center}>
                            Will diligently, ethically, and honestly perform
                            services as Digital Pradhan in my area in accordance
                            with Nearby policies and the applicable law;
                        </p>
                        <p className={classesInner.center}>
                            Will show utmost commitment, integrity, and honesty
                            in making Nearby services available in my area;
                        </p>
                        <p className={classesInner.center}>
                            Will not charge any extra fees from the customers
                            and will do my best to ensure that the customers are
                            always satisfied with my services;
                        </p>
                        <p className={classesInner.center}>
                            Will always deal with customers with utmost respect,
                            dignity, courtesy, and fairness and will try to
                            resolve any potential disputes in an amicable
                            manner;
                        </p>
                        <p className={classesInner.center}>
                            Shall not take any action that impairs nearby
                            abilities to render services and shall always remain
                            liable for any consequence in relation to the same.
                        </p>
                    </Grid>
                </Grid>
                <Grid className={classesInner.p10} container spacing={2}>
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        <p className={classesInner.center}>
                            THIS IS A DIGITAL CERTIFICATE AND DOES NOT REQUIRE
                            ANY SIGNATURE
                        </p>
                    </Grid>
                </Grid>
            </Grid>
        </Paper>
    );
};

export default memo(DigitalCertificatePrint);
