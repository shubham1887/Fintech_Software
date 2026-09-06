import React, { useEffect } from 'react';

import { Grid, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderLog = ({ headerLog }) => {
    const classes = commonStyle();

    useEffect(() => {}, [headerLog]);

    return (
        <Box
            style={{
                paddingTop: 6,
                marginBottom: 16,
                paddingLeft: 10
            }}
        >
            <Grid container spacing={1}>
                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <h2 className={classes.recharge_h2}>
                        TransactionID :
                        <span className={classes.Success_h3}>
                            {headerLog.TransactionMasterID}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <h2 className={classes.recharge_h2}>
                        ConsumerNumber :
                        <span className={classes.Success_h3}>
                            {headerLog.ConsumerNumber}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={3} lg={3} xl={3}>
                    <h2 className={classes.recharge_h2}>
                        Amount :
                        <span className={classes.Success_h3}>
                            {headerLog.Amount}
                        </span>
                    </h2>
                </Grid>
            </Grid>
        </Box>
    );
};
