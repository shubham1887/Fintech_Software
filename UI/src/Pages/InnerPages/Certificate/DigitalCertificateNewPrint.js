import React, { useState, useEffect, forwardRef } from 'react';
import Logo from 'Components/Logo/Logo';
import {
    Button,
    Dialog,
    DialogContent,
    DialogActions,
    DialogContentText,
    DialogTitle,
    Slide
} from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Grid, Paper } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import { CLIENT_NAME } from 'Config';
import CommonPrint from 'Components/Print/CommonPrint';
import { SelectByKey as SELECT_BY_KEY } from 'Components/Api/RichTextMediaAPI';

const useStyles = makeStyles((theme) => ({
    marginTab: {
        padding: '0px',
        paddingTop: '24px',
        marginRight: '-18px'
    },
    borderPaper: {
        border: '1px solid'
    },
    appBarHeight: {
        height: 50
    },
    p10: {
        padding: 10
    },
    document_h3: {
        fontWeight: 'normal',
        fontSize: 30
    },
    document_h2: {
        fontWeight: 'bold',
        fontSize: 35
    },
    blue_h4: {
        color: '#481BAB',
        fontWeight: 'bold',
        fontSize: 16
    },

    method: {
        fontSize: 16,
        color: '#FFA500',
        fontWeight: 'bold'
    },
    response: {
        fontSize: 16,
        color: '#00807A',
        fontWeight: 'bold'
    },
    center: {
        textAlign: 'center',
        margin: '4px'
    },

    center1: {
        textAlign: 'center',
        margin: '4px',
        fontWeight: 'bold'
    }
}));

const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export default function DigitalCertificateNewPrint({
    isOpen,
    title,
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
        /// window.print();
        let pageSize = 'A4';
        CommonPrint(pageSize);
    };
    return (
        <Dialog
            //fullWidth
            maxWidth="lg"
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
                    <CertificatePrint />
                </DialogContentText>
            </DialogContent>
            <DialogActions displayPrint="none">
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handleNegativeButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {negativeButtonTitle
                                ? negativeButtonTitle
                                : 'No, Cancel Print'}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handlePositiveButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                        >
                            {positiveButtonTitle
                                ? positiveButtonTitle
                                : 'Yes, Do Print'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const CertificatePrint = () => {
    const [isContentDisplay, setIsContentDisplay] = useState(false);
    const [content, setContent] = useState('');
    const [UserID, setUserID] = useState(localStorage.getItem('UserID'));
    const [Address, setAddress] = useState('');
    const [Name, setName] = useState('');
    const [State, setState] = useState('');
    const [PinCode, setPinCode] = useState('');
    const [MobileNumber, setMobileNumber] = useState('');
    const [cName, setCName] = useState('');

    const fetchCName = async () => {
        setCName(await CLIENT_NAME());
    };

    const selectByKey = async () => {
        setIsContentDisplay(false);
        var reqObj = JSON.stringify({
            Key: 'DigitalCertificate'
        });

        var response = await SELECT_BY_KEY(reqObj);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);

        if (data.Content == null || data.Content.length <= 3) {
            setAddress(data.Address);
            setName(data.FirmName);
            setState(data.StateName);
            setPinCode(data.Pincode);
            setMobileNumber(data.MobileNumber);
            return;
        }
        setContent(data.Content);
        setIsContentDisplay(true);
    };

    useEffect(() => {
        selectByKey();
        fetchCName();
    }, []);

    if (isContentDisplay)
        return (
            <div
                class="certificate"
                dangerouslySetInnerHTML={{ __html: content }}
            />
        );

    return (
        <Paper
            style={{
                border: '1px solid',
                padding: 10,
                margin: '1%',
                marginRight: '15%'
            }}
        >
            <Grid
                container
                spacing={2}
                style={{ padding: 10, lineHeight: 1.4 }}
            >
                <Grid item xs={12} sm={12} md={4} lg={4}>
                    <Logo />
                </Grid>
            </Grid>
        </Paper>
    );
};
