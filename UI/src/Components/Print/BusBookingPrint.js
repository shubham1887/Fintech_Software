import React, { useState, forwardRef } from 'react';
import {
    Grid,
    Typography,
    Paper,
    Box,
    Button,
    Dialog,
    DialogContent,
    DialogActions,
    DialogContentText,
    DialogTitle,
    Slide
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { styled } from '@mui/system';
import CommonPrint from 'Components/Print/CommonPrint';

const StyledPaper = styled(Paper)(({ theme }) => ({
    padding: theme.spacing(4),
    marginBottom: theme.spacing(3),
    backgroundColor: '#fff',
    boxShadow: '0px 3px 10px rgba(0, 0, 0, 0.1)',
    borderRadius: '8px'
}));

const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export function BusBookingPrint({
    isOpen,
    title,
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
    const handlePositiveA4ButtonClick = () => {
        let pageSize = 'A4';
        CommonPrint(pageSize);
        positiveButtonClick(request);
    };
    const handlePositiveA5ButtonClick = () => {
        let pageSize = 'A5';
        CommonPrint(pageSize);
        positiveButtonClick(request);
    };

    return (
        <>
            <Dialog
                classes={{ paperFullScreen: 'prePrint' }}
                fullScreen
                open={open}
                onClose={() => {
                    setOpen(false);
                    negativeButtonClick(false);
                }}
                TransitionComponent={Transition}
            >
                <DialogTitle id="alert-dialog-title" className={classes.title}>
                    {title}
                </DialogTitle>

                <DialogContent>
                    <DialogContentText id="DialogPrint">
                        <PrintDetails request={request} />
                    </DialogContentText>
                </DialogContent>
                <DialogActions displayPrint="none">
                    <Grid container spacing={1}>
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <Button
                                onClick={handleNegativeButtonClick}
                                className={
                                    classes.CustomSecondaryButtonStylePopup
                                }
                            >
                                {negativeButtonTitle
                                    ? negativeButtonTitle
                                    : 'No, Cancel Print'}
                            </Button>
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <Button
                                onClick={handlePositiveA4ButtonClick}
                                className={
                                    classes.CustomPrimaryButtonStylePopup
                                }
                            >
                                {positiveButtonTitle
                                    ? positiveButtonTitle
                                    : 'A4'}
                            </Button>
                        </Grid>
                        <Grid item xs={12} sm={12} md={3} lg={3}>
                            <Button
                                onClick={handlePositiveA5ButtonClick}
                                className={
                                    classes.CustomPrimaryButtonStylePopup
                                }
                            >
                                {positiveButtonTitle
                                    ? positiveButtonTitle
                                    : 'A5'}
                            </Button>
                        </Grid>
                    </Grid>
                </DialogActions>
            </Dialog>
        </>
    );
}

const PrintDetails = (request) => {
    let data = request.request;
    return (
        <Box sx={{ m: '16px' }}>
            <Grid container sx={{ justifyContent: 'center' }}>
                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                    <Paper
                        sx={{
                            p: '20px',
                            maxWidth: '100%',
                            borderRadius: '12px',
                            boxShadow:
                                '0px 0px 2px rgba(145, 158, 171, 0.2), 0px 12px 24px -4px rgba(145, 158, 171, 0.12)',
                            bgcolor: '#ffffff'
                        }}
                    >
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={6} lg={6} xl={6}>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Bus Type : {data.BusType}
                                </Typography>

                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Pickup Location : {data.pickupLocation}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Drop Location : {data.dropLocation}
                                </Typography>
                            </Grid>
                            <Grid item xs={12} sm={12} md={6} lg={6} xl={6}>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Seat Name : {data.seatName}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    TXN ID : {data.refid}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Pickup Time : {data.pickupTime}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Drop Time : {data.dropTime}
                                </Typography>
                            </Grid>
                            <Grid item xs={12}>
                                <Typography
                                    sx={{
                                        fontSize: '20px',
                                        fontWeight: '500',
                                        mt: '5px'
                                    }}
                                >
                                    <h5>Passenger Details</h5>
                                </Typography>
                            </Grid>
                            <Grid item xs={12} sm={12} md={6} lg={6} xl={6}>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Name : {data.name}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Mobile : {data.customer_mobile}
                                </Typography>
                            </Grid>
                            <Grid item xs={12} sm={12} md={6} lg={6} xl={6}>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Gender : {data.gender}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Age : {data.age}
                                </Typography>
                            </Grid>
                            <Grid item xs={12}>
                                <Typography
                                    sx={{
                                        fontSize: '20px',
                                        fontWeight: '500',
                                        mt: '5px'
                                    }}
                                >
                                    <h5>Payment Details</h5>
                                </Typography>
                            </Grid>
                            <Grid item xs={12} sm={12} md={6} lg={6} xl={6}>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Amount : {data.amount}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    TDS : {data.tds}
                                </Typography>
                            </Grid>
                            <Grid item xs={12} sm={12} md={6} lg={6} xl={6}>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Commission : {data.comm}
                                </Typography>
                                <Typography
                                    sx={{ fontSize: '16px', mb: '5px' }}
                                >
                                    Total Deduction : {data.total_deduction}
                                </Typography>
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    );
};
