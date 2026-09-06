import React, { useState, useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ADMIN_CODE } from 'Components/Helper/Messages';
export const HeaderUserwiseSaleSummary = ({ headerUserwiseSale }) => {
    const classes = commonStyle();
    const [display, setDisplay] = useState('none');
    var currentUserTypeId = localStorage.getItem('UserTypeID');
    const [isAdminRole, setIsAdminRole] = useState(false);

    const CheckUserRole = () => {
        if (ADMIN_CODE === parseInt(currentUserTypeId)) setIsAdminRole(true);
    };
    const isDisplayHeader = () => {
        if (headerUserwiseSale.TotalSale >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerUserwiseSale.TotalRCommission >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerUserwiseSale.TotalTransfer >= 1) {
            setDisplay('inline');
            return;
        }
        setDisplay('none');
    };
    useEffect(() => {
        CheckUserRole();
        isDisplayHeader();
    }, [headerUserwiseSale]);

    return (
        <Box display={display}>
            <Paper
                style={{
                    paddingTop: 12,
                    marginBottom: 16,
                    paddingLeft: 10
                }}
            >
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Sale :
                            <span className={classes.Success_h3}>
                                {headerUserwiseSale.TotalSale}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            R.Commi :
                            <span className={classes.Success_h3}>
                                {headerUserwiseSale.TotalRCommission}
                            </span>
                        </h2>
                    </Grid>
                    {/* <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            D.Commi :
                            <span className={classes.Success_h3}>
                                {headerUserwiseSale.TotalDCommission}
                            </span>
                        </h2>
                    </Grid> */}
                    {/* <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            S.Commi :
                            <span className={classes.Success_h3}>
                                {headerUserwiseSale.TotalSCommission}
                            </span>
                        </h2>
                    </Grid> */}
                    <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Purchase :
                            <span className={classes.Success_h3}>
                                {headerUserwiseSale.TotalPurchase}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Transfer :
                            <span className={classes.Success_h3}>
                                {headerUserwiseSale.TotalTransfer}
                            </span>
                        </h2>
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};
