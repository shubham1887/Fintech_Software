import React, { useState, useEffect, forwardRef } from 'react';
import ReceiptLogo from 'Components/Logo/ReceiptLogo';
import {
    Button,
    Dialog,
    DialogContent,
    DialogActions,
    DialogContentText,
    DialogTitle,
    Slide,
    Box
} from '@material-ui/core';
import { CLIENT_NAME } from 'Config';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Grid } from '@material-ui/core';
import { PurchaseReport as PURCHASE_SELECT_USERWISE } from 'Components/Api/TransactionReportAPI';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { logout } from 'Components/Helper/Helpers';

const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export function PaymentPurchase({
    isOpen,
    title,
    UserID,
    Month,
    Year,
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
        window.print();
    };

    return (
        <Dialog
            classes={{ paperFullScreen: 'prePrint' }}
            // fullScreen
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
                    <PrintDetails UserID={UserID} Month={Month} Year={Year} />
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

const PrintDetails = (Request, UserID, Month, Year) => {
    const [logo, setLogo] = useState([]);
    const [cName, setCName] = useState('');
    const [UserResponse, setUserResponse] = useState('');
    const [Daystring, setDaystring] = useState('');

    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }

    const SelectUserwiseReceipt = async () => {
        var inputObj = {
            Month: Request.Month,
            Year: Request.Year,
            UserID: Request.UserID
        };
        let requestJSON = JSON.stringify(inputObj);

        const { data, status, isAuthorized } = await PURCHASE_SELECT_USERWISE(
            requestJSON
        );

        if (!isAuthorized) logout();
        if (!status) {
            return;
        }

        var d = JSON.parse(data);

        var responselist = d.list[0];
        setUserResponse(responselist);
    };
    useEffect(() => {
        createRowForLogo();
        SelectUserwiseReceipt();
        fetchCName();
        setDaystring(
            new Date().getFullYear() +
                '/' +
                new Date().getMonth() +
                '/' +
                new Date().getDate()
        );
    }, []);

    const createRowForLogo = () => {
        let dummyRows = [];
        dummyRows.push(LogoData(<ReceiptLogo />));
        setLogo(dummyRows);
    };

    function LogoData(Parameter1) {
        return { Parameter1 };
    }

    return (
        <Grid item xs={12} sm={12} md={12} lg={12}>
            <Box
                style={{
                    border: '2px solid #000',
                    padding: '5px',
                    marginTop: '10px',
                    marginLeft: '10px',
                    backgroundColor: '#fff',
                    width: '450px',
                    height: '500px',
                    position: 'relative'
                }}
            >
                <p style={{ textAlign: 'center', marginBottom: '10px' }}>
                    <ReceiptLogo />
                </p>
                <Box
                    style={{
                        color: '#000',
                        margin: '5px 0',
                        borderBottom: '1px solid #ccc',
                        paddingBottom: '2px',
                        fontSize: '20px'
                    }}
                >
                    <p style={{ textAlign: 'center', marginBottom: '10px' }}>
                        <span style={{ fontWeight: 'bold' }}>{cName}</span>
                    </p>
                </Box>
                <Box
                    style={{
                        color: '#000',
                        margin: '5px 0',
                        borderBottom: '1px solid #ccc',
                        paddingBottom: '2px',
                        fontSize: '14px'
                    }}
                >
                    <p style={{ textAlign: 'center', marginBottom: '10px' }}>
                        {UserResponse.CompanyAddress}
                        {UserResponse.CompanyPincode}
                    </p>

                    <p style={{ textAlign: 'center', marginBottom: '10px' }}>
                        GSTIN: {UserResponse.CompanyPincode}
                    </p>
                </Box>
                <Box
                    style={{
                        color: '#000',
                        margin: '5px 0',
                        borderBottom: '1px solid #ccc',
                        paddingBottom: '2px',
                        fontSize: '14px'
                    }}
                >
                    <p style={{ textAlign: 'left', marginBottom: '10px' }}>
                        Voucher No. :
                        {'P' + '/' + UserResponse.UserID + '/' + Daystring}
                    </p>

                    <p style={{ textAlign: 'right', marginBottom: '10px' }}>
                        Date :{UserResponse.Month}
                    </p>
                </Box>
                <Box
                    style={{
                        color: '#000',
                        margin: '5px 0',
                        borderBottom: '1px solid #ccc',
                        paddingBottom: '2px',
                        fontSize: '14px'
                    }}
                >
                    <p style={{ textAlign: 'left', marginBottom: '10px' }}>
                        {UserResponse.FirmName}
                    </p>

                    <p style={{ textAlign: 'left', marginBottom: '10px' }}>
                        Credit : {UserResponse.Credit}
                    </p>

                    <p style={{ textAlign: 'right', marginBottom: '10px' }}>
                        Debit : {UserResponse.Debit}
                    </p>
                </Box>

                <Box
                    style={{
                        color: '#000',
                        margin: '5px 0',
                        borderBottom: '1px solid #ccc',
                        paddingBottom: '2px',
                        fontSize: '14px'
                    }}
                ></Box>

                <Box
                    style={{
                        borderTop: '1px solid #000'
                    }}
                >
                    <h1
                        style={{
                            fontSize: '14px',
                            color: '#000',
                            marginTop: '5px',
                            marginBottom: '0px'
                        }}
                    >
                        <span style={{ borderBottom: '2px solid #000' }}>
                            Rupees :{NumberToWord(UserResponse.Credit)}
                        </span>
                        <p style={{ textAlign: 'left', marginBottom: '10px' }}>
                            ( Cheque Subject to Realisation )
                        </p>
                        <span
                            style={{ fontSize: '11px', fontWeight: 'normal' }}
                        ></span>
                    </h1>
                </Box>

                <Box>
                    <p
                        style={{
                            padding: '3px 0px',
                            margin: '0px',
                            backgroundColor: '#ccc',
                            color: '#000',
                            fontSize: '8px',
                            textAlign: 'center',
                            position: 'absolute',
                            bottom: '5px',
                            width: '90%'
                        }}
                    >
                        This is system generated receipt. Hence no seal or
                        signature required
                        <br />
                        Thank you for using {cName}
                    </p>
                </Box>
            </Box>
        </Grid>
    );
};
