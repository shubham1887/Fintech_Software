import React, { useState, useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { indianFormatNumber } from 'Components/utils';

export const HeaderPendingSummary = ({ headerSummary }) => {
    const classes = commonStyle();

    const [display, setDisplay] = useState('none');

    const AmountAndCount = ({ name, amount, count, summayClass }) => {
        return (
            <h2 className={classes.recharge_h2}>
                {name} :
                <span className={summayClass}>
                    {indianFormatNumber(Number(amount || 0.0))}
                    {' ['}
                    {count || 0} {']'}
                </span>
            </h2>
        );
    };

    const isDisplayHeader = () => {
        if (headerSummary.PendingCount >= 1) {
            setDisplay('inline');
            return;
        }

        setDisplay('none');
    };

    useEffect(() => {
        isDisplayHeader();
    }, [headerSummary]);

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
                    <Grid item xs={12} sm={12} md={3} xl={3}>
                        <AmountAndCount
                            summayClass={classes.Pending_h3}
                            name={'Pending'}
                            amount={headerSummary.PendingAmount}
                            count={headerSummary.PendingCount}
                        />
                    </Grid>
                    <Grid item xs={0} sm={0} md={0} xl={3}></Grid>
                    <Grid item xs={0} sm={0} md={0} xl={3}></Grid>
                    <Grid item xs={12} sm={12} md={3} xl={3}>
                        <AmountAndCount
                            summayClass={classes.blue_h3}
                            name={'Total'}
                            amount={headerSummary.TotalAmount}
                            count={headerSummary.TotalCount}
                        />
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};
