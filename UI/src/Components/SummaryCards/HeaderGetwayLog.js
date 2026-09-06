import React, { useEffect } from 'react';

import { Grid, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const HeaderGetwayLog = ({ headerLog }) => {
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
                <Grid item xs={4}>
                    <h2 className={classes.recharge_h2}>
                        OrderID :
                        <span className={classes.Success_h3}>
                            {headerLog.OrderID}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={4}>
                    <h2 className={classes.recharge_h2}>
                        UserName :
                        <span className={classes.Success_h3}>
                            {headerLog.UserName}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={4}>
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
