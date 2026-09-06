import React, { useState } from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from '@material-ui/core';

import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export function HeavyRefreshConfirmation({
    isOpen,
    title,
    message,
    request,
    positiveButtonTitle,
    positiveButtonClick,
    negativeButtonTitle,
    negativeButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = commonStyle();

    const handleNegativeButtonClick = () => {
        setOpen(false);
        negativeButtonClick();
    };
    const handlePositiveButtonClick = () => {
        setOpen(false);
        positiveButtonClick(request);
    };
    return (
        <Dialog
            open={open}
            onClose={() => {
                setOpen(false);
                negativeButtonClick(false);
            }}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">{title}</DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    <RechargeConfirmation request={request} />
                    <p> {message}</p>
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button
                    onClick={handlePositiveButtonClick}
                    className={classes.CustomPrimaryButtonStyle}
                >
                    {positiveButtonTitle ? positiveButtonTitle : 'Yes'}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

const LabelStyle = {
    color: 'green'
};

const RechargeConfirmation = (request) => {
    const { CustomerName, Desc } = request.request;

    return (
        <>
            <b>
                Name:<small style={LabelStyle}> {CustomerName}</small>
            </b>
            <br />
            <b>
                Desc:<small style={LabelStyle}> {Desc}</small>
            </b>
            <br />
        </>
    );
};
