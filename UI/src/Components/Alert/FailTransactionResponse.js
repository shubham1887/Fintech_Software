import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from '@material-ui/core';
import { AEPS_CODE } from 'Components/Helper/Messages';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
export function FailTransactionResponse({
    isOpen,
    title,
    Message,
    OkButtonTitle,
    OkButtonClick
}) {
    const [open, setOpen] = useState(isOpen);

    const classes = commonStyle();
    const [bgColor, setBGColor] = useState('');
    const [width, setWidth] = useState('md');

    useEffect(() => {
        if (title === 'FAILURE' || title === 'REFUND') {
            setBGColor('#ff0000');
            return;
        }

        if (title === 'PROCESS' || title === 'PENDING') {
            setBGColor('#f58220');
            return;
        }
    }, []);

    const handleOkButtonClick = () => {
        setOpen(false);
        OkButtonClick();
    };

    if (isOpen === undefined || isOpen === null || isOpen === false) {
        return null;
    }

    return (
        <Dialog
            fullWidth
            maxWidth={width}
            open={open}
            onClose={() => {
                setOpen(false);
                OkButtonClick(false);
            }}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle
                id="alert-dialog-title"
                className={classes.title}
                style={{ background: bgColor }}
            >
                {title}
            </DialogTitle>

            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    <Grid
                        item
                        xs={12}
                        sm={12}
                        md={12}
                        lg={12}
                        style={{
                            marginTop: '5%',
                            marginBottom: '5%'
                        }}
                    >
                        <span
                            style={{
                                fontStyle: 'bold',
                                marginTop: '10%',
                                marginBottom: '10%',
                                color: '#000000fc',
                                fontSize: '30px',
                                fontWeight: '500'
                            }}
                        >
                            {Message}
                        </span>
                    </Grid>
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Grid container spacing={2}>
                    <Grid item xs={12} sm={12} md={8} lg={8}></Grid>
                    <Grid item xs={12} sm={12} md={4} lg={4}>
                        <Button
                            onClick={() => handleOkButtonClick()}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {OkButtonTitle ? OkButtonTitle : 'OK'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}
