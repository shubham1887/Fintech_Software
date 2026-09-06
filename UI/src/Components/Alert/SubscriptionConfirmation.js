import React from 'react';
import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export function SubscriptionConfirmation({
    isOpen,
    title,
    message,
    request,
    positiveButtonTitle = 'Upgrade',
    positiveButtonClick,
    negativeButtonTitle = 'Cancel',
    negativeButtonClick
}) {
    const classes = commonStyle();

    return (
        <Dialog
            open={isOpen}
            onClose={negativeButtonClick}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
            fullWidth
            maxWidth="sm"
            PaperProps={{
                style: {
                    minHeight: '170px',
                    maxHeight: '550px'
                }
            }}
        >
            <DialogTitle
                id="alert-dialog-title"
                style={{
                    textAlign: 'center',
                    backgroundColor: '#f5f5f5',
                    padding: '16px',
                    borderRadius: '4px 4px 0 0',
                    fontWeight: 'bold'
                }}
            >
                {title}
            </DialogTitle>
            <DialogContent style={{ overflowY: 'auto' }}>
                <DialogContentText
                    id="alert-dialog-description"
                    style={{
                        textAlign: 'center',
                        fontSize: '1rem',
                        fontWeight: 'bold'
                    }}
                >
                    <strong>{message}</strong>
                </DialogContentText>
            </DialogContent>
            <DialogActions
                style={{ justifyContent: 'center', paddingBottom: '16px' }}
            >
                <Button
                    onClick={negativeButtonClick}
                    variant="outlined"
                    style={{
                        padding: '10px 24px',
                        fontSize: '14px',
                        minWidth: '140px',
                        textTransform: 'capitalize',
                        color: '#606060',
                        borderColor: '#606060'
                    }}
                >
                    {negativeButtonTitle}
                </Button>
                <Button
                    onClick={() => positiveButtonClick(request)}
                    className={classes.CustomPrimaryButtonStyle}
                    style={{
                        padding: '10px 24px',
                        fontSize: '14px',
                        minWidth: '140px',
                        textTransform: 'capitalize',
                        backgroundColor: '#d32f2f',
                        color: '#fff',
                        marginLeft: '10px'
                    }}
                >
                    {positiveButtonTitle}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
