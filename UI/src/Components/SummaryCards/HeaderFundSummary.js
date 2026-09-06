import React, { useState, useEffect } from 'react';

import { Grid, Paper, Box } from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { indianFormatNumber } from 'Components/utils';
import {
    ADMIN,
    RETAILER,
    API_USER,
    DISTRIBUTOR,
    SUPER_DISTRIBUTOR,
    ACCOUNTANT,
    SUPPORT_USER
} from 'Components/Helper/UsertypeCode';
export const HeaderFundSummary = ({ headerSummary }) => {
    const classes = commonStyle();
    const [isAdminRole, setIsAdminRole] = useState(false);
    const [isRetailerRole, setIsRetailerRole] = useState(false);
    const [isDistRole, setIsDistRole] = useState(false);
    const [display, setDisplay] = useState('none');
    var currentUsertypeid = localStorage.getItem('UserTypeID');

    const CheckUserRole = () => {
        if (ADMIN === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (SUPPORT_USER === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (ACCOUNTANT === parseInt(currentUsertypeid)) setIsAdminRole(true);
        if (RETAILER === parseInt(currentUsertypeid)) setIsRetailerRole(true);
        if (API_USER === parseInt(currentUsertypeid)) setIsRetailerRole(true);
        if (DISTRIBUTOR === parseInt(currentUsertypeid)) setIsDistRole(true);

        if (SUPER_DISTRIBUTOR === parseInt(currentUsertypeid))
            setIsDistRole(true);
    };

    const AmountAndCount = ({ name, amount, count, summayClass }) => {
        return (
            <h2 className={classes.recharge_h2}>
                {name} :
                <span className={summayClass}>
                    {indianFormatNumber(Number(amount || 0.0))}
                    {'[ '}
                    {count || 0} {' ]'}
                </span>
            </h2>
        );
    };

    const isDisplayHeader = () => {
        if (headerSummary.TransferCount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.ReceiveCount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.DownlineReverseCount >= 1) {
            setDisplay('inline');
            return;
        }
        if (headerSummary.ReverseCount >= 1) {
            setDisplay('inline');
            return;
        }

        setDisplay('none');
    };

    useEffect(() => {
        CheckUserRole();
        isDisplayHeader();
    }, [headerSummary, isAdminRole, isRetailerRole, isDistRole]);

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
                    {!isRetailerRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Fail_h3}
                                name={'Transfer'}
                                amount={headerSummary.TransferAmount}
                                count={headerSummary.TransferCount}
                            />
                        </Grid>
                    ) : null}
                    {isAdminRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Fail_h3}
                                name={'PGTransfer'}
                                amount={headerSummary.PGTransferAmount}
                                count={headerSummary.PGTransferCount}
                            />
                        </Grid>
                    ) : null}
                    {isAdminRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Fail_h3}
                                name={'ECollection'}
                                amount={headerSummary.ECTransferAmount}
                                count={headerSummary.ECTransferCount}
                            />
                        </Grid>
                    ) : null}
                    {!isAdminRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Fail_h3}
                                name={'Revers Debit'}
                                amount={headerSummary.ReverseAmount}
                                count={headerSummary.ReverseCount}
                            />
                        </Grid>
                    ) : null}

                    {!isAdminRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Success_h3}
                                name={'Receive'}
                                amount={headerSummary.ReceiveAmount}
                                count={headerSummary.ReceiveCount}
                            />
                        </Grid>
                    ) : null}
                    {!isAdminRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Success_h3}
                                name={'PGReceive'}
                                amount={headerSummary.PGReceiveAmount}
                                count={headerSummary.PGReceiveCount}
                            />
                        </Grid>
                    ) : null}
                    {!isAdminRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Success_h3}
                                name={'ECReceive'}
                                amount={headerSummary.ECReceiveAmount}
                                count={headerSummary.ECReceiveCount}
                            />
                        </Grid>
                    ) : null}

                    {!isRetailerRole ? (
                        <Grid item xs={12} sm={12} md={2} lg={2} xl={2}>
                            <AmountAndCount
                                summayClass={classes.Success_h3}
                                name={'Reverse Credit'}
                                amount={headerSummary.DownlineReverseAmount}
                                count={headerSummary.DownlineReverseCount}
                            />
                        </Grid>
                    ) : null}
                </Grid>
            </Paper>
        </Box>
    );
};
