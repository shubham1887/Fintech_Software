import React, { useState } from 'react';

import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export function RefundConfirmAlert({
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
            <DialogTitle
                id="alert-dialog-title"
                // style={{ textAlign: 'center' }}
                style={{
                    textAlign: 'center',
                    backgroundColor: '#d3d3d3',
                    padding: '16px',
                    borderRadius: '4px 4px 0 0'
                }}
            >
                {title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    {message}
                </DialogContentText>
            </DialogContent>
            <DialogActions style={{ justifyContent: 'center' }}>
                <Button
                    type="reset"
                    onClick={() => {
                        setOpen(false);
                        negativeButtonClick();
                    }}
                    variant="contained"
                    autoFocus
                    style={{
                        padding: '12px 24px',
                        fontSize: '12px',
                        minWidth: '120px'
                    }}
                >
                    {negativeButtonTitle ? negativeButtonTitle : 'No'}
                </Button>
                <Button
                    onClick={() => {
                        setOpen(false);
                        positiveButtonClick(request);
                    }}
                    className={classes.CustomPrimaryButtonStyle}
                    style={{
                        padding: '12px 24px',
                        fontSize: '12px',
                        minWidth: '120px'
                    }}
                >
                    {positiveButtonTitle ? positiveButtonTitle : 'OK'}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
