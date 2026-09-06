import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import { IS_RECHARGE_REPORT_COMPACT } from 'Config';
import { RechargeReport } from 'Pages/InnerPages/Report/RechargeReport';
import { CompactRechargeReport } from 'Pages/InnerPages/Report/CompactRechargeReport';

export const ActiveReport = () => {
    const [isRechargeReportCompact, setRechargeReportCompact] = useState('');

    const fetchRechargeCompact = async () => {
        let IsCompact = await IS_RECHARGE_REPORT_COMPACT();

        if (IsCompact === undefined || IsCompact === null || IsCompact === '') {
            setRechargeReportCompact('OFF');
            return;
        }
        setRechargeReportCompact(IsCompact);
    };

    useEffect(() => {
        fetchRechargeCompact();
    }, []);

    if (isRechargeReportCompact === 'ON') {
        return (
            <Grid>
                <CompactRechargeReport />
            </Grid>
        );
    }
    return (
        <Grid>
            <RechargeReport />
        </Grid>
    );
};
