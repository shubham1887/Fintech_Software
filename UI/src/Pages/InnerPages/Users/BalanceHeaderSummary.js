import React from 'react';
import { Grid, Paper } from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { indianFormatNumber } from 'Components/utils';

export const HeaderSummary = ({ headerSummary }) => {
    const classes = commonStyle();
    const AmountAndCount = ({ name, amount }) => {
        return (
            <h2 className={classes.recharge_h2}>
                {name} :
                <span className={classes.Success_h3}>
                    {indianFormatNumber(Number(amount || 0.0))}
                </span>
            </h2>
        );
    };

    return (
        <Paper style={{ paddingTop: 12, marginBottom: 16, paddingLeft: 10 }}>
            <Grid container spacing={1}>
                {headerSummary &&
                    headerSummary.map(({ name, amount }, index) => (
                        <Grid item xs={12} sm={12} md={6} xl={2} key={name}>
                            <AmountAndCount name={name} amount={amount} />
                        </Grid>
                    ))}
            </Grid>
        </Paper>
    );
};
