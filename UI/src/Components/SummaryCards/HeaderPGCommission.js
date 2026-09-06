import React, { useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderPGCommission = ({ headerCommission }) => {
    const classes = commonStyle();

    useEffect(() => {}, [headerCommission]);

    return (
        <Box>
            <Paper
                style={{
                    paddingTop: 12,
                    marginBottom: 16,
                    paddingLeft: 10
                }}
            >
                <Grid container spacing={1}>
                    <Grid item xs={4}>
                        <h2 className={classes.recharge_h2}>
                            User :
                            <span className={classes.Success_h3}>
                                {headerCommission.UserName}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={4}>
                        <h2 className={classes.recharge_h2}>
                            PayMode :
                            <span className={classes.Success_h3}>
                                {headerCommission.PayMode}
                            </span>
                        </h2>
                    </Grid>

                    <Grid item xs={4}>
                        <h2 className={classes.recharge_h2}>
                            Amount :
                            <span className={classes.Success_h3}>
                                {headerCommission.Amount}
                            </span>
                        </h2>
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};
