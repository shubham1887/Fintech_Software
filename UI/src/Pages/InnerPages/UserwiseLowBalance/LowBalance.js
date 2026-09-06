import React, { useState } from 'react';

import {
    Grid,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ADMIN } from 'Components/Helper/UsertypeCode';
export function UserLowBalance({
    isOpen,
    title,
    message,
    closeButtonTitle,
    closeButtonClick,
    loadMoneyButtonTitle,
    loadMoneyButtonClick,
    fundRequestButtonTitle,
    fundRequestButtonClick,
    VirtualBalanceButtonTitle,
    VirtualBalanceButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = commonStyle();
    const [currentUserTypeid] = useState(
        parseInt(localStorage.getItem('UserTypeID'))
    );

    const handleCloseButtonClick = () => {
        setOpen(false);
        closeButtonClick();
    };
    const handleLoadMoneyButtonClick = () => {
        setOpen(false);
        loadMoneyButtonClick();
    };
    const handleFundRequestButtonClick = () => {
        setOpen(false);
        fundRequestButtonClick();
    };
    const handleVirtualBalanceButtonClick = () => {
        setOpen(false);
        VirtualBalanceButtonClick();
    };

    return (
        <Dialog
            fullWidth
            maxWidth="xs"
            open={open}
            onClose={() => {
                setOpen(false);
                fundRequestButtonClick(false);
            }}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title" className={classes.title}>
                {title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    <p> {message}</p>
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={4} lg={4}>
                        <Button
                            onClick={handleCloseButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {closeButtonTitle ? closeButtonTitle : 'No'}
                        </Button>
                    </Grid>
                    {currentUserTypeid !== ADMIN ? (
                        <>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <Button
                                    onClick={handleFundRequestButtonClick}
                                    className={
                                        classes.CustomPrimaryButtonStylePopup
                                    }
                                >
                                    {fundRequestButtonTitle
                                        ? fundRequestButtonTitle
                                        : 'Fund Request'}
                                </Button>
                            </Grid>
                            <Grid item xs={12} sm={12} md={4} lg={4}>
                                <Button
                                    onClick={handleLoadMoneyButtonClick}
                                    className={
                                        classes.CustomPrimaryButtonStylePopup
                                    }
                                >
                                    {loadMoneyButtonTitle
                                        ? loadMoneyButtonTitle
                                        : 'Load Money'}
                                </Button>
                            </Grid>
                        </>
                    ) : null}
                    {currentUserTypeid === ADMIN ? (
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <Button
                                onClick={handleVirtualBalanceButtonClick}
                                className={
                                    classes.CustomPrimaryButtonStylePopup
                                }
                            >
                                {VirtualBalanceButtonTitle
                                    ? VirtualBalanceButtonTitle
                                    : 'Add Virtual Bal'}
                            </Button>
                        </Grid>
                    ) : null}
                </Grid>
            </DialogActions>
        </Dialog>
    );
}
