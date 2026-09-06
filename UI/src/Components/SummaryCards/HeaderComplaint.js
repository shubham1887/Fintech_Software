import React from 'react';

import { Grid, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderComplaint = ({ headerComplaint }) => {
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
                        ProviderName:
                        <span className={classes.Success_h3}>
                            {headerComplaint.ProviderName}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        ProviderRequestID:
                        <span className={classes.Success_h3}>
                            {headerComplaint.ProviderRequestID}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        SupportNumber:
                        <span className={classes.Success_h3}>
                            {headerComplaint.SupportNumber}
                        </span>
                    </h2>
                </Grid>
            </Grid>
        </Box>
    );
};
