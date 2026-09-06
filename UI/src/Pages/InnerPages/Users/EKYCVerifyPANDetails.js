import React, { useState } from 'react';

import {
    Grid,
    Typography,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export function EKYCVerifyPANDetails({
    isOpen,
    title,
    name,
    status,
    negativeButtonTitle,
    negativeButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = commonStyle();

    const handleNegativeButtonClick = () => {
        setOpen(false);
        negativeButtonClick();
    };

    return (
        <Dialog
            fullWidth
            maxWidth="xs"
            open={open}
            onClose={() => {
                setOpen(false);
                negativeButtonClick(false);
            }}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title" className={classes.title}>
                {title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>Name: {name}</b>
                        </span>
                    </Typography>
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>Status: {status}</b>
                        </span>
                    </Typography>
                </DialogContentText>
            </DialogContent>
            <DialogActions displayPrint="none">
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={6} lg={6}></Grid>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handleNegativeButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {negativeButtonTitle ? negativeButtonTitle : 'Ok'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}
