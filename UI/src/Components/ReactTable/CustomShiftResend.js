import React, { Fragment } from 'react';
import { Box, Grid, IconButton } from '@material-ui/core';

import RepeatOneIcon from '@material-ui/icons/RepeatOne';
import CallSplitIcon from '@material-ui/icons/CallSplit';

export const CustomShiftResend = ({ value, callback }) => {
    const ShiftResendCallback = (BtnTitle) => {
        callback(value, BtnTitle);
    };

    return (
        <React.Fragment>
            <Grid item xs={12} md={12} lg={12} sm={12}>
                <ShiftResendComponent
                    value={value}
                    callback={(BtnTitle) => {
                        ShiftResendCallback(BtnTitle);
                    }}
                />
            </Grid>
        </React.Fragment>
    );
};
const ShiftResendComponent = ({ value, callback }) => {
    return (
        <Fragment>
            <Grid container spacing={2}>
                <Grid item xs={12} md={12} lg={6} sm={6}>
                    <IconButton
                        aria-label="SHIFT"
                        title="SHIFT"
                        style={{ color: 'Blue' }}
                    >
                        <CallSplitIcon
                            onClick={() => {
                                callback('SHIFT');
                            }}
                        />
                    </IconButton>
                </Grid>
                <Grid item xs={12} md={12} lg={6} sm={6}>
                    <IconButton
                        aria-label="RESEND"
                        title="RESEND"
                        style={{ color: 'orange' }}
                    >
                        <RepeatOneIcon
                            onClick={() => {
                                callback('RESEND');
                            }}
                        />
                    </IconButton>
                </Grid>
            </Grid>
        </Fragment>
    );
};
