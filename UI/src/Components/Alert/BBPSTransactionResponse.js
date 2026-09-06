import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import ReactTable from 'Components/ReactTable/ReactTable';
import TableRow from '@material-ui/core/TableRow';
import BBPSAssured_LOGO from 'Assets/Images/BBPS/BBPS-Assured.png';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import Yes_LOGO from 'Assets/Images/BBPS/yesbank.png';
import Tick from 'Assets/Images/BBPS/right.png';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,

} from '@material-ui/core';

import {
    MONEY_TRANSFER_CODE,
    PAYOUT,
    DTH_BOOKING,
    PHONE_BOOKING,
    AEPS_CODE,
    PREPAID_ACTIVATION_CODE,
    MICRO_ATM_CODE,
    UPITRANSFER_CODE,
    E_KYC,
    OTT,
    CREDITCARDPAYMENT_CODE,
    VENDOR_PAY,
    DTH_RECHARGE_CODE,
    PREPAID_RECHARGE_CODE, ELECTRICITY_CODE
} from 'Components/Helper/Messages';

import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { withStyles } from '@material-ui/core/styles';

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

export function BBPSTransactionResponse({

    isOpen,
    isPrint,
    title,
    ServiceID,
    request,
    OkButtonTitle,
    OkButtonClick,
    PrintButtonTitle,
    PrintButtonClick,
    CheckStatusClick
}) {
    const [open, setOpen] = useState(isOpen);
    const [print, setPrint] = useState(isPrint);
    const classes = commonStyle();
    const [bgColor, setBGColor] = useState('');
    const [width, setWidth] = useState('xs');


    useEffect(() => {
        if (parseInt(ServiceID) === AEPS_CODE) {
            setWidth('md');
        }

        if (title === 'SUCCESS') {
            setBGColor('#42b86f');
            return;
        }

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

    const handlePrintButtonClick = () => {
        setOpen(false);
        PrintButtonClick();
    };

    const handleCheckStatusClick = () => {
        setOpen(false);
        CheckStatusClick();
    };

    const isPrintEnable = () => {
        if (
            parseInt(ServiceID) !== MONEY_TRANSFER_CODE &&
            parseInt(ServiceID) !== AEPS_CODE &&
            parseInt(ServiceID) !== ELECTRICITY_CODE &&
            parseInt(ServiceID) !== PAYOUT &&
            parseInt(ServiceID) !== CREDITCARDPAYMENT_CODE &&
            parseInt(ServiceID) !== VENDOR_PAY
        )
            return false;

        if (title === 'FAILURE') return false;

        return true;
    };

    if (isOpen === undefined || isOpen === null || isOpen === false) {
        return null;
    }

    return (
        <Dialog
            fullWidth

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

            <DialogContent style={{ overflowY: 'initial' }}>
                <DialogContentText id="alert-dialog-description">
                    {parseInt(ServiceID) !== DTH_BOOKING &&
                        parseInt(ServiceID) !== PAYOUT &&
                        parseInt(ServiceID) !== VENDOR_PAY &&
                        parseInt(ServiceID) !== MONEY_TRANSFER_CODE &&
                        parseInt(ServiceID) !== MICRO_ATM_CODE &&
                        parseInt(ServiceID) !== AEPS_CODE &&
                        parseInt(ServiceID) !== PREPAID_ACTIVATION_CODE &&
                        parseInt(ServiceID) !== PHONE_BOOKING &&
                        parseInt(ServiceID) !== E_KYC &&
                        parseInt(ServiceID) !== OTT &&
                        parseInt(ServiceID) !== CREDITCARDPAYMENT_CODE &&
                        parseInt(ServiceID) !== PREPAID_RECHARGE_CODE &&
                        parseInt(ServiceID) !== DTH_RECHARGE_CODE &&
                        parseInt(ServiceID) !== UPITRANSFER_CODE ? (
                        <BBPSResponse request={request} />
                    ) : null}

                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Grid container spacing={1}>

                    {/* {request.Status === 'PROCESS' && (
                        <Grid item xs={12} sm={12} md={4} lg={4}>
                            <Button
                                onClick={handleCheckStatusClick}
                                className={classes.CustomSecondaryButtonStylePopup}
                                variant="contained"
                            >
                                Check Status
                            </Button>
                        </Grid>
                    )} */}
                    {isPrintEnable() ? (
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <Button
                                onClick={() => handlePrintButtonClick()}
                                className={
                                    classes.CustomSecondaryButtonStylePopup
                                }
                            >
                                {PrintButtonTitle ? PrintButtonTitle : 'Print'}
                            </Button>
                        </Grid>
                    ) : (
                        <Grid item xs={12} sm={12} md={6} lg={6}></Grid>
                    )}
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={() => handleOkButtonClick()}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {OkButtonTitle ? OkButtonTitle : 'Ok'}
                        </Button>
                    </Grid>


                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const BBPSResponse = (request) => {
    let data = request.request;
    const YesImage = () => {
        return (
            <img
                src={Yes_LOGO}
                width={100}
                alt="YesBankLogo"
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

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                width={100}
                style={{ float: 'right' }}
                alt="CompanyLogo"
            />
        );
    };

    const BBPSAssuredImage = () => {
        return (
            <div
                align="center"
                style={{
                    position: 'absolute',

                    left: '50%',
                    transform: 'translate(-50%, -50%)'
                }}
            >
                <img
                    src={BBPSAssured_LOGO}
                    height={60}
                    width={60}
                    style={{ float: 'center' }}
                    alt="BBPS Assured"
                />
            </div>
        );
    };

    const getStatusLabelAndColor = (status) => {
        let statusColor;
        let statusLabel;
        switch (status) {
            case 'SUCCESS':
                statusColor = 'green';
                statusLabel = 'Success';
                break;
            case 'FAILURE':
                statusColor = 'red';
                statusLabel = 'Failure';
                break;
            case 'PROCESS':
            case 'PENDING':
                statusColor = 'orange';
                statusLabel = 'Pending';
                break;
            default:
                statusColor = 'inherit';
                statusLabel = status;
        }
        return { statusColor, statusLabel };
    };

    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '') return true;
        if (param.toUpperCase() === 'data not found'.toUpperCase()) return true;
        if (param.toUpperCase() === 'Paramnotmatched'.toUpperCase())
            return true;

        return false;
    };

    let rows = [];

    if (!isBlank(data.TransactionID))
        rows.push(Data('BBPS Transaction Ref ID', data.TransactionID));

    rows.push(Data('Product', "BBPS"));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Customer Mobile Number', data.MobileNo));

    if (!isBlank(data.ServiceName
        .toString()))
        rows.push(Data('Service Name', data.ServiceName
            .toString()));

    if (!isBlank(data.ConsumerName.toString()))
        rows.push(Data('Customer Name', data.ConsumerName.toString()));

    if (!isBlank(data.DOC.toString()))
        rows.push(Data('Transaction Date', data.DOC.toString()));

    if (!isBlank(data.BillDate
        .toString()))
        rows.push(Data('Bill Date', data.BillDate
            .toString()));


    if (!isBlank(data.OperatorName.toString()))
        rows.push(Data('Biller Name', data.OperatorName.toString()));

    if (!isBlank(data.BillerID.toString()))
        rows.push(Data('Biller ID', data.BillerID.toString()));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Consumer Number', data.ConsumerNumber));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount(₹)', data.Amount.toString()));

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status ',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
    }

    if (data.Status !== 'SUCCESS' && !isBlank(data.Message)) {
        rows.push(Data('Message', data.Message));
    }
    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableRow>
                        <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-around', gap: '5px', padding: '10px' }}>
                            <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto', }}>
                                <YesImage />
                                {data.Status === 'SUCCESS' && <TickImage />}
                                <Logo />

                            </Grid>
                        </span></StyledTableCell>
                    </TableRow>
                    <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', gap: '10px', paddingTop: '20px' }}>
                        <Grid style={{ textAlign: 'center', flex: 'auto', }}>
                            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                                <div style={{ marginBottom: '40px' }}>
                                    <BBPSAssuredImage />
                                </div>
                                <div
                                    style={{
                                        fontWeight: 'bold',
                                        color: '#fff',
                                        background:
                                            data.Status === 'SUCCESS' ? '#42b86f' :
                                                data.Status === 'FAILURE' || data.Status === 'REFUND' ? '#ff0000' :
                                                    data.Status === 'PROCESS' || data.Status === 'PENDING' ? '#f58220' :
                                                        'gray',
                                        fontSize: '15px',
                                        display: 'inline-block',
                                        padding: '4px 22px',
                                        borderRadius: '100px',
                                        textAlign: 'center',
                                    }}
                                >
                                    TRANSACTION {data.Status}
                                </div>
                            </div>
                        </Grid>
                    </span>
                    </StyledTableCell>

                    <TableBody>
                        {rows.map((row) => (
                            <TableRow key={row.Parameter}>
                                <StyledTableCell align="left" style={{ fontWeight: 400, fontSize: '16px', paddingTop: '4px', paddingBottom: '4px' }}>
                                    {row.Parameter}
                                </StyledTableCell>
                                <ValueTableCell align="left" style={{ fontWeight: 500, fontSize: '16px', color: '#000', paddingTop: '4px', paddingBottom: '4px' }}>
                                    {row.Values}
                                </ValueTableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </Grid>
    );
};





