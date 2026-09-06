import React, { useState, useEffect } from 'react';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import {
    Button,
    TextField,
    DialogTitle,
    Dialog,
    DialogContent,
    DialogContentText,
    DialogActions
} from '@material-ui/core';

function PrintConfirmation({
    isOpen,
    title,
    message,
    request,
    positiveButtonTitle,
    positiveButtonClick,
    negativeButtonTitle,
    negativeButtonClick
}) {
    const classes = commonStyle();
    const [open, setOpen] = useState(isOpen);

    const [errorMessage, setErrorMessage] = React.useState('');
    const [newCharge, setNewCharge] = useState();
    const { ID } = request;

    const handleNewChargeChange = (event) => {
        // let value = event.target.value;
        const limit = 4;
        setNewCharge(event.target.value.slice(0, limit));
        // const reg = /^-?[0-9]*(\.[0-9]*)?$/;
        // if ((!isNaN(value) && reg.test(value)) || value === '' || value === '-')
        //     setNewCharge(value);
    };
    const handleNegativeButtonClick = () => {
        setOpen(false);
        negativeButtonClick(false);
    };
    const handlePositiveButtonClick = () => {
        if (
            newCharge <= 0 ||
            newCharge === '' ||
            newCharge === undefined ||
            newCharge === null ||
            newCharge.length >= 5
        ) {
            setErrorMessage('Please provide valid charge');
            return;
        }

        setOpen(false);
        positiveButtonClick({
            ID: ID,
            Charge: newCharge
        });
    };
    useEffect(() => {
        setErrorMessage('');
    }, [newCharge]);

    const handleDialogCloseClick = () => {
        setOpen(false);
        negativeButtonClick(false);
    };
    return (
        <Dialog
            open={open}
            onClose={handleDialogCloseClick}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">{title}</DialogTitle>

            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    <div>
                        <TextField
                            variant="outlined"
                            type="number"
                            label="Service Charge"
                            name="Charge"
                            value={newCharge}
                            maxLength={4}
                            typed_length={1}
                            onChange={handleNewChargeChange}
                        />
                    </div>
                    <p>
                        <label style={{ color: 'red' }}>{errorMessage}</label>
                    </p>
                </DialogContentText>
            </DialogContent>

            <DialogActions>
                <Button
                    onClick={handleNegativeButtonClick}
                    color="secondary"
                    variant="contained"
                    autoFocus
                >
                    {negativeButtonTitle ? negativeButtonTitle : 'No'}
                </Button>

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

export default PrintConfirmation;
