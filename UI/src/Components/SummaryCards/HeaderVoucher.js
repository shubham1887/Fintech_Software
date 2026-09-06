import React, { useState, useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { indianFormatNumber } from 'Components/utils';

export const HeaderVoucher = ({ headerSummary }) => {
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
        if (headerSummary.ContraAmount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.PaymentAmount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.ReceiptAmount >= 1) {
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
                            name={'Contra'}
                            amount={headerSummary.ContraAmount}

                        />
                    </Grid>

                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <AmountAndCount
                            summayClass={classes.Pending_h3}
                            name={'Payment'}
                            amount={headerSummary.PaymentAmount}

                        />
                    </Grid>

                    <Grid item xs={12} sm={6} md={4} lg={2} xl={2}>
                        <AmountAndCount
                            summayClass={classes.blue_h3}
                            name={'Receipt'}
                            amount={headerSummary.ReceiptAmount}

                        />
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};
