import React, { useState } from 'react';
import { Grid } from '@material-ui/core';

import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Typography
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export function FetchDetailsForPincode({
    isOpen,
    title,
    request,
    negativeButtonTitle,
    negativeButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = commonStyle();
    let data = request;
    const handleNegativeButtonClick = () => {
        setOpen(false);
        negativeButtonClick();
    };

    return (
        <Dialog
            fullWidth
            maxWidth="sm"
            open={open}
            onClose={handleNegativeButtonClick}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title" className={classes.title}>
                {title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="DialogPrint">
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>City: {data.CityName}</b>
                        </span>
                    </Typography>

                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>Circle: {data.Circle}</b>
                        </span>
                    </Typography>
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>Division: {data.Division}</b>
                        </span>
                    </Typography>
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>District: {data.District}</b>
                        </span>
                    </Typography>
                    <Typography>
                        <span style={{ color: 'green' }}>
                            <b>State: {data.StateName}</b>
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
