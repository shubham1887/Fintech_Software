import React, { useState } from 'react';
import { Grid, Box, Dialog, DialogTitle, DialogContent, DialogActions, Button } from '@material-ui/core';
import { makeStyles } from '@material-ui/styles';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

const useStyles = makeStyles((theme) => ({
    image: {
        width: '55px',
        height: '55px',
        transition: 'transform 0.3s ease',
        cursor: 'pointer',
    },
    imageExpanded: {
        position: 'relative',
        top: 0,
        left: 0,
        width: '1000px',
        height: '1000px',
        objectFit: 'contain',
        zIndex: -99,
    },
    overlay: {
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
        height: '100%',
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        zIndex: 999,
    },
    dialogImage: {
        width: '100%',
        height: 'auto',
    },
}));

export const HeaderBeneficiary = ({ headerobject }) => {
    const classes = commonStyle();
    const customClasses = useStyles();
    const [openDialog, setOpenDialog] = useState(false);

    const handleImageClick = () => {
        setOpenDialog(true);
    };

    const handleClose = () => {
        setOpenDialog(false);
    };

    return (
        <Box
            style={{
                marginBottom: 16,
                paddingLeft: 5,
            }}
        >

            <Grid container spacing={2} sx={{ flexGrow: 1 }}>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        Entity Name:
                        <span className={classes.Success_h3}>
                            {headerobject.FirmName}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        Entity ID:
                        <span className={classes.Success_h3}>

                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        Bank Name:
                        <span className={classes.Success_h3}>
                            {headerobject.BankName}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        AccountNumber:
                        <span className={classes.Success_h3}>
                            {headerobject.AccountNumber}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        IFSC:
                        <span className={classes.Success_h3}>
                            {headerobject.IFSC}
                        </span>
                    </h2>
                </Grid>
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        AccountHolderName:
                        <span className={classes.Success_h3}>
                            {headerobject.AccountHolder}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        IsNameVerified:
                        <span className={classes.Success_h3}>
                            {headerobject.IsNameVerified}
                        </span>
                    </h2>
                </Grid>

                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <h2 className={classes.recharge_h2}>
                        Uploaded Document:
                        <span className={classes.Success_h3}>
                            <img
                                src={headerobject.Document}
                                className={customClasses.image}
                                onMouseMove={handleImageClick}
                                alt="Document"
                            />
                        </span>
                    </h2>

                </Grid>
            </Grid>

            <Dialog open={openDialog} onClose={handleClose} maxWidth="lg" fullWidth>
                <DialogTitle>Image Preview</DialogTitle>
                <DialogContent onMouseLeave={handleClose} >
                    <img
                        src={headerobject.Document}
                        className={customClasses.dialogImage}
                        alt="Expanded Document"
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose} color="primary">
                        Close
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};
