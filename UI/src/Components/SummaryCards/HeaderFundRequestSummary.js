import React, { useState, useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderFundRequestSummary = ({ headerFundRequest }) => {
    const classes = commonStyle();
    const [display, setDisplay] = useState('none');

    const isDisplayHeader = () => {
        if (headerFundRequest.TotalSuccess >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerFundRequest.TotalFailure >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerFundRequest.TotalProcess >= 1) {
            setDisplay('inline');
            return;
        }
        setDisplay('none');
    };
    useEffect(() => {
        isDisplayHeader();
    }, [headerFundRequest]);

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
                    <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                        <h2 className={classes.recharge_h2}>
                            Total Success :
                            <span className={classes.Success_h3}>
                                {headerFundRequest.TotalSuccess}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                        <h2 className={classes.recharge_h2}>
                            Total Failure :
                            <span className={classes.Fail_h3}>
                                {headerFundRequest.TotalFailure}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                        <h2 className={classes.recharge_h2}>
                            Total Process :
                            <span className={classes.Pending_h3}>
                                {headerFundRequest.TotalProcess}
                            </span>
                        </h2>
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};
