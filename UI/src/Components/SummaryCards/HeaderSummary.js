import React, { useState, useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { indianFormatNumber } from 'Components/utils';

export const HeaderSummary = ({ headerSummary }) => {
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
        if (headerSummary.SuccessCount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.PendingCount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.FailedCount >= 1) {
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
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <AmountAndCount
                            summayClass={classes.Success_h3}
                            name={'Success'}
                            amount={headerSummary.SuccessAmount}
                            count={headerSummary.SuccessCount}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <AmountAndCount
                            summayClass={classes.Pending_h3}
                            name={'Pending'}
                            amount={headerSummary.PendingAmount}
                            count={headerSummary.PendingCount}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <AmountAndCount
                            summayClass={classes.Fail_h3}
                            name={'Failed'}
                            amount={headerSummary.FailedAmount}
                            count={headerSummary.FailedCount}
                        />
                    </Grid>

                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <AmountAndCount
                            summayClass={classes.Fail_h3}
                            name={'Manual Fail'}
                            amount={headerSummary.ManualFailAmount}
                            count={headerSummary.ManualFailCont}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <h2 className={classes.recharge_h2}>
                            Comm :
                            <span className={classes.blue_h3}>
                                {indianFormatNumber(
                                    Number(headerSummary.Commission || 0.0)
                                )}
                            </span>
                        </h2>
                    </Grid>
                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
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
