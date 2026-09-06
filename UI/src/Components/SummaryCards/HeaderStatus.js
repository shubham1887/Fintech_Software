import React from 'react';

import { Grid, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderStatus = ({ headerStatus }) => {
    const classes = commonStyle();

    return (
        <Box
            style={{
                marginBottom: 16,
                paddingLeft: 5
            }}
        >
            <Grid container spacing={1}>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        Firm :
                        <span className={classes.Success_h3}>
                            {headerStatus.FirmName}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={3} lg={3}>
                    <h2 className={classes.recharge_h2}>
                        Number :
                        <span className={classes.Success_h3}>
                            {headerStatus.ConsumerNumber}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <h2 className={classes.recharge_h2}>
                        Amount :
                        <span className={classes.Success_h3}>
                            {headerStatus.Amount}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={3} lg={3}>
                    <h2 className={classes.recharge_h2}>
                        Date :
                        <span className={classes.Success_h3}>
                            {headerStatus.DateStr}
                        </span>
                    </h2>
                </Grid>
            </Grid>
        </Box>
    );
};
