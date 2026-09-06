import React, { useState, useEffect, forwardRef } from 'react';
import Logo from 'Components/Logo/Logo';
import { Grid } from '@material-ui/core';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import TableRow from '@material-ui/core/TableRow';
import Tick from 'Assets/Images/BBPS/right.png';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Slide
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CLIENT_NAME } from 'Config';
import { withStyles } from '@material-ui/core/styles';
import CommonPrint from 'Components/Print/CommonPrint';
const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

const StyledTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold'
    }
}))(TableCell);
const ValueTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold',
        color: 'green'
    }
}))(TableCell);

export function PayoutPrint({
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
        <Grid container spacing={1}>
            <Grid item xs={12} sm={12} md={6} lg={6}>
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
                    <DialogTitle
                        id="alert-dialog-title"
                        className={classes.title}
                    >
                        {title}
                    </DialogTitle>
                    <DialogContent>
                        <DialogContentText id="DialogPrint">
                            <RechargeConfirmation request={request} />
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
            </Grid>
        </Grid>
    );
}

const RechargeConfirmation = (request) => {
    let data = request.request;
    const [cName, setCName] = useState('');
    const [bgColor, setBGColor] = useState('');
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    const classes = commonStyle();

    useEffect(() => {
        fetchCName();
    }, []);


    const rows = [

        RechargeData('Payment Mode', data.Operator),
        RechargeData('Bank Name', data.BankName),

        RechargeData('A/c Holder Name', data.DMT_BeneficiaryName
        ),

        RechargeData('Account Number', data.ConsumerNumber),
        RechargeData('IFSC', data.IFSC),
        RechargeData('Amount   (₹)', data.Amount),

        RechargeData('Charge  (₹)', data.ChargeAmount),
        RechargeData('GST (₹)', data.RetailerGST),
        RechargeData('Total Amount  (₹)', data.DebitAmount)
    ];
    function RechargeData(Parameter, Values) {
        return { Parameter, Values };
    }
    React.useEffect(() => {

        if (data.Status === 'SUCCESS') {
            setBGColor('#42b86f');
            return;
        }

        if (data.Status === 'FAILURE' || data.Status === 'REFUND') {
            setBGColor('#ff0000');
            return;
        }

        if (data.Status === 'PROCESS' || data.Status === 'PENDING') {
            setBGColor('#f58220');
            return;
        }
    }, []);

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                height={50}
                width={100}
                style={{ float: 'right' }}
                alt="Payout"
            />
        );
    };
    const TickImage = () => {
        return (
            <img
                src={Tick}
                width={50}
                alt="Success"
            />
        );
    };
    return (
        <Grid item xs={12}>


            <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-around', gap: '5px', padding: '10px' }}>
                <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto', }}>
                    {data.Status === 'SUCCESS' && <TickImage />}
                    <Logo />

                </Grid>
            </span></StyledTableCell>
            <hr></hr>
            <span style={{ fontSize: "15px" }}>
                Your request to withdraw funds to your bank account
                has been received. You will receive a notification from
                your bank once the funds are credited.
            </span>

            <hr></hr>

            <TableContainer>
                <Table aria-label="customized table">
                    <TableBody>
                        {rows.map((row) => (
                            <TableRow key={row.Parameter}>
                                <StyledTableCell align="left">
                                    {row.Parameter}
                                </StyledTableCell>
                                <ValueTableCell align="left">
                                    {row.Values}
                                </ValueTableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <h3 className={classes.document_h3}>Thank you for using {cName}</h3>
        </Grid>
    );
};
