import React from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderCommission = ({ headerCommission }) => {
    const classes = commonStyle();

    return (
        <Box>
            <Paper
                style={{
                    paddingTop: 6,
                    marginBottom: 8,
                    paddingLeft: 6
                }}
            >
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Number :
                            <span
                                style={{
                                    color: 'green',
                                    fontSize: 16,
                                    fontWeight: '800',
                                    marginLeft: '3px'
                                }}
                            >
                                {headerCommission.Number}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Amount :
                            <span className={classes.Success_h3}>
                                {headerCommission.Amount}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Operator :
                            <span className={classes.Success_h3}>
                                {headerCommission.Operator}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={6} xl={6}>
                        <h2 className={classes.user_h2}>
                            User :
                            <span className={classes.Success_h3}>
                                {headerCommission.UserName}
                            </span>
                        </h2>
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};
