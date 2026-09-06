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
    DialogTitle
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

export function TransactionResponse({

    isOpen,
    isPrint,
    title,
    ServiceID,
    request,
    OkButtonTitle,
    OkButtonClick,
    PrintButtonTitle,
    PrintButtonClick
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
                    parseInt(ServiceID) !== UPITRANSFER_CODE &&
                    parseInt(ServiceID) !== DTH_RECHARGE_CODE ? (
                        <RechargeResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === DTH_RECHARGE_CODE ? (
                        <DTHResponse request={request} />
                    ) : null}

                    {parseInt(ServiceID) === MONEY_TRANSFER_CODE ? (
                        <DMTTransferResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === PAYOUT ? (
                        <PayoutResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === VENDOR_PAY ? (
                        <VendorPayResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === DTH_BOOKING ||
                    parseInt(ServiceID) === PHONE_BOOKING ? (
                        <OfflineResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === AEPS_CODE ? (
                        <AEPSResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === PREPAID_ACTIVATION_CODE ? (
                        <ActivationResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === E_KYC ? (
                        <EKYCResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === UPITRANSFER_CODE ? (
                        <UPITransferResponse request={request} />
                    ) : null}
                    {parseInt(ServiceID) === OTT ? (
                        <OTTTransferResponse request={request} />
                    ) : null}

                    {parseInt(ServiceID) === CREDITCARDPAYMENT_CODE ? (
                        <CreditCardResponse request={request} />
                    ) : null}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Grid container spacing={1}>
                    {isPrintEnable() ? (
                        <Grid item xs={12} sm={12} md={6} lg={6}>
                            <Button
                                onClick={() => handlePrintButtonClick()}
                                className={
                                    classes.CustomSecondaryButtonStylePopup
                                }
                            >
                                {PrintButtonTitle ? PrintButtonTitle : 'PRINT'}
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
                            {OkButtonTitle ? OkButtonTitle : 'OK'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const PayoutResponse = (request) => {
    let data = request.request;
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
    if (!isBlank(data.BeneficiaryName))
        rows.push(Data('Beneficiary', data.BeneficiaryName));

    if (!isBlank(data.Status)) rows.push(Data('Status', data.Status));

    if (!isBlank(data.TransactionID))
        rows.push(Data('TxnID', data.TransactionID));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('AccountNo', data.ConsumerNumber));

    if (!isBlank(data.OpeningBalance.toString()))
        rows.push(Data('Op.Bal', data.OpeningBalance.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Words', NumberToWord(data.Amount)));

    if (!isBlank(data.Commission.toString()))
        rows.push(Data('Commission', data.Commission.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Cl.Bal', data.ClosingBalance.toString()));

    if (!isBlank(data.Message)) rows.push(Data('Message', data.Message));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12}>
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
            </Grid>
        </>
    );
};
const VendorPayResponse = (request) => {
    let data = request.request;
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
    if (!isBlank(data.BeneficiaryName))
        rows.push(Data('Beneficiary', data.BeneficiaryName));

    if (!isBlank(data.Status)) rows.push(Data('Status', data.Status));

    if (!isBlank(data.TransactionID))
        rows.push(Data('TxnID', data.TransactionID));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('AccountNo', data.ConsumerNumber));

    if (!isBlank(data.OpeningBalance.toString()))
        rows.push(Data('Op.Bal', data.OpeningBalance.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Words', NumberToWord(data.Amount)));

    if (!isBlank(data.Commission.toString()))
        rows.push(Data('Commission', data.Commission.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Cl.Bal', data.ClosingBalance.toString()));

    if (!isBlank(data.Message)) rows.push(Data('Message', data.Message));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12}>
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
            </Grid>
        </>
    );
};

const RechargeResponse = (request) => {
    let data = request.request;

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

    const isBlank = (param) => {
        if (param === undefined || param === null) return true;
        if (typeof param === 'string') {
            if (param.trim() === '') return true;
            if (param.toUpperCase() === 'DATA NOT FOUND') return true;
            if (param.toUpperCase() === 'PARAMNOTMATCHED') return true;
        }
        return false;
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


    let rows = [];

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Mobile Number', data.ConsumerNumber));

    if (!isBlank(data.Amount))
        rows.push(Data('Transaction Amount (₹) ', data.Amount));

    if (!isBlank(data.DOC)) rows.push(Data('Transaction Date & Time ', data.DOC));

    if (!isBlank(data.OperatorName))
        rows.push(Data('Operator Name', data.OperatorName))

    if (!isBlank(data.ProviderRequestID));
    rows.push(Data('Transaction ID ', data.ProviderRequestID));

    if (!isBlank(data.TransactionID));
    rows.push(Data('Operator Transaction ID ', data.TransactionID));

    if (!isBlank(data.ServiceName))
        rows.push(Data('Service Name', data.ServiceName))

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
    }

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableRow>
                        <StyledTableCell align="left" colSpan={2}>
                            <span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-around', gap: '5px', padding: '10px' }}>
                                <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto' }}>
                                    {data.Status === 'SUCCESS' && <TickImage />}

                                    <Logo />
                                </Grid>
                            </span>
                        </StyledTableCell>
                    </TableRow>

                    <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', gap: '10px', paddingTop: '20px' }}>
                        <Grid style={{ textAlign: 'center', flex: 'auto', }}>
                            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
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

const DTHResponse = (request) => {
    let data = request.request;

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

    const isBlank = (param) => {
        if (param === undefined || param === null) return true;
        if (typeof param === 'string') {
            if (param.trim() === '') return true;
            if (param.toUpperCase() === 'DATA NOT FOUND') return true;
            if (param.toUpperCase() === 'PARAMNOTMATCHED') return true;
        }
        return false;
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


    let rows = [];

    if (!isBlank(data.MobileNo))
        rows.push(Data('Mobile Number', data.MobileNo));

    if (!isBlank(data.Amount))
        rows.push(Data('Transaction Amount (₹) ', data.Amount));

    if (!isBlank(data.DOC)) rows.push(Data('Transaction Date & Time ', data.DOC));

    if (!isBlank(data.OperatorName))
        rows.push(Data('Operator Name', data.OperatorName))

    if (!isBlank(data.ProviderRequestID));
    rows.push(Data('Transaction ID ', data.ProviderRequestID));

    if (!isBlank(data.TransactionID));
    rows.push(Data('Operator Transaction ID ', data.TransactionID));

    if (!isBlank(data.ServiceName))
        rows.push(Data('Service Name', data.ServiceName))

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Reference ID', data.ConsumerNumber))

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
    }

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableRow>
                        <StyledTableCell align="left" colSpan={2}>
                            <span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-around', gap: '5px', padding: '10px' }}>
                                <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto' }}>
                                    {data.Status === 'SUCCESS' && <TickImage />}
                                    <Logo />
                                </Grid>
                            </span>
                        </StyledTableCell>
                    </TableRow>
                    <StyledTableCell align="left" colSpan={2}>
                        <span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between' }}>
                            <Grid style={{ textAlign: 'center', flex: 'auto' }}>
                                <div style={{ textAlign: 'center' }}>
                                    <div style={{ fontWeight: 'bold', fontSize: 20 }}>{data.IconName}</div>
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

const DMTTransferResponse = (request) => {
                                             const YesImage = () => {
                                                 return (
                                                     <img
                                                         src={Yes_LOGO}
                                                         width={100}
                                                         alt="DMT"
                                                     />
                                                 );
                                             };

                                             const TickImage = () => {
                                                 return (
                                                     <img
                                                         src={Tick}
                                                         width={50}
                                                         alt="DMT"
                                                     />
                                                 );
                                             };

                                             const Logo = () => {
                                                 return (
                                                     <img
                                                         src={CompanyLogo}
                                                         width={100}
                                                         style={{
                                                             float: 'right'
                                                         }}
                                                         alt="DMT"
                                                     />
                                                 );
                                             };

                                             let data = request.request;

                                             const isBlank = (param) => {
                                                 if (param === undefined)
                                                     return true;
                                                 if (param === null)
                                                     return true;
                                                 if (param === '') return true;
                                                 return false;
                                             };

                                             let rows = [];

                                             if (!isBlank(data.Charge))
                                                 rows.push(
                                                     DMTData(
                                                         'Charge',
                                                         data.Charge
                                                     )
                                                 );
                                             if (!isBlank(data.GST))
                                                 rows.push(
                                                     DMTData('GST', data.GST)
                                                 );
                                             if (!isBlank(data.Total))
                                                 rows.push(
                                                     DMTData(
                                                         'Total Amount',
                                                         data.Total
                                                     )
                                                 );

                                             {
                                                 rows.push(
                                                     DMTData(
                                                         'Transaction Details',
                                                         '',
                                                         { fontWeight: 'bold' }
                                                     )
                                                 );
                                             }

                                             if (!isBlank(data.SenderNo))
                                                 rows.push(
                                                     DMTData(
                                                         'Sender Mobile Number',
                                                         data.SenderNo
                                                     )
                                                 );
                                             if (!isBlank(data.BeneficiaryName))
                                                 rows.push(
                                                     DMTData(
                                                         'Recipient Name',
                                                         data.BeneficiaryName
                                                     )
                                                 );

                                             if (!isBlank(data.BeneMobile))
                                                 rows.push(
                                                     DMTData(
                                                         'Recipient Mobile Number',
                                                         data.BeneMobile
                                                     )
                                                 );

                                             if (!isBlank(data.ConsumerNumber))
                                                 rows.push(
                                                     DMTData(
                                                         'Recipient A/C Number',
                                                         data.ConsumerNumber
                                                     )
                                                 );
                                             if (!isBlank(data.IFSC))
                                                 rows.push(
                                                     DMTData(
                                                         'Recipient IFSC',
                                                         data.IFSC
                                                     )
                                                 );

                                             if (!isBlank(data.Amount))
                                                 rows.push(
                                                     DMTData(
                                                         'Amount (₹)',
                                                         data.Amount === ''
                                                             ? '0'
                                                             : data.Amount
                                                     )
                                                 );
                                             if (!isBlank(data.TransactionID))
                                                 rows.push(
                                                     DMTData(
                                                         'RRN',
                                                         data.TransactionID
                                                     )
                                                 );
                                             if (!isBlank(data.ReferenceNo))
                                                 rows.push(
                                                     DMTData(
                                                         'DMT TxnID',
                                                         data.ReferenceNo
                                                     )
                                                 );
                                             if (!isBlank(data.TransactionID))
                                                 rows.push(
                                                     DMTData(
                                                         'TxnID',
                                                         data.TransactionID
                                                     )
                                                 );
                                             if (!isBlank(data.DOC))
                                                 rows.push(
                                                     DMTData('Date', data.DOC)
                                                 );
                                             if (!isBlank(data.PaymentType))
                                                 rows.push(
                                                     DMTData(
                                                         'Type',
                                                         data.PaymentType
                                                     )
                                                 );
                                             if (!isBlank(data.Charge))
                                                 rows.push(
                                                     DMTData(
                                                         'Charge',
                                                         data.Charge
                                                     )
                                                 );
                                             if (!isBlank(data.GST))
                                                 rows.push(
                                                     DMTData('GST', data.GST)
                                                 );

                                             if (!isBlank(data.Status))
                                                 rows.push(
                                                     DMTData(
                                                         'Status',
                                                         data.Status
                                                     )
                                                 );

                                             function DMTData(
                                                 Parameter,
                                                 Values
                                             ) {
                                                 return { Parameter, Values };
                                             }

                                             return (
                                                 <Grid item xs={12}>
                                                     <TableContainer>
                                                         <Table aria-label="customized table">
                                                             <TableRow>
                                                                 <StyledTableCell
                                                                     align="left"
                                                                     colSpan={2}
                                                                 >
                                                                     <span
                                                                         style={{
                                                                             display:
                                                                                 'flex',
                                                                             flexDirection:
                                                                                 'row',
                                                                             justifyContent:
                                                                                 'space-around',
                                                                             gap:
                                                                                 '5px',
                                                                             padding:
                                                                                 '10px'
                                                                         }}
                                                                     >
                                                                         <Grid
                                                                             style={{
                                                                                 display:
                                                                                     'flex',
                                                                                 justifyContent:
                                                                                     'space-around',
                                                                                 flex:
                                                                                     'auto'
                                                                             }}
                                                                         >
                                                                             <YesImage />
                                                                             {data.Status === 'SUCCESS' && <TickImage />}
                                                                             <Logo />
                                                                         </Grid>
                                                                     </span>
                                                                 </StyledTableCell>
                                                             </TableRow>
                                                             <StyledTableCell
                                                                 align="left"
                                                                 colSpan={2}
                                                             >
                                                                 <span
                                                                     style={{
                                                                         display:
                                                                             'flex',
                                                                         flexDirection:
                                                                             'row',
                                                                         justifyContent:
                                                                             'space-between'
                                                                     }}
                                                                 >
                                                                     <Grid
                                                                         style={{
                                                                             textAlign:
                                                                                 'center',
                                                                             flex:
                                                                                 'auto'
                                                                         }}
                                                                     >
                                                                         <div
                                                                             style={{
                                                                                 textAlign:
                                                                                     'center'
                                                                             }}
                                                                         >
                                                                             <div
                                                                                 style={{
                                                                                     fontWeight:
                                                                                         'bold',
                                                                                     fontSize: 20
                                                                                 }}
                                                                             >
                                                                                 {
                                                                                     data.IconName
                                                                                 }
                                                                             </div>
                                                                             <div
                                                                                 style={{
                                                                                     fontWeight:
                                                                                         'bold',
                                                                                     color:
                                                                                         '#fff',
                                                                                     background:
                                                                                         data.Status ===
                                                                                         'SUCCESS'
                                                                                             ? '#42b86f'
                                                                                             : data.Status ===
                                                                                                   'FAILURE' ||
                                                                                               data.Status ===
                                                                                                   'REFUND'
                                                                                             ? '#ff0000'
                                                                                             : data.Status ===
                                                                                                   'PROCESS' ||
                                                                                               data.Status ===
                                                                                                   'PENDING'
                                                                                             ? '#f58220'
                                                                                             : 'gray',
                                                                                     fontSize:
                                                                                         '15px',
                                                                                     display:
                                                                                         'inline-block',
                                                                                     padding:
                                                                                         '4px 22px',
                                                                                     borderRadius:
                                                                                         '100px'
                                                                                 }}
                                                                             >
                                                                                 TRANSACTION{' '}
                                                                                 {
                                                                                     data.Status
                                                                                 }
                                                                             </div>
                                                                         </div>
                                                                     </Grid>
                                                                 </span>
                                                             </StyledTableCell>
                                                             <TableBody>
                                                                 {rows.map(
                                                                     (row) => (
                                                                         <TableRow
                                                                             key={
                                                                                 row.Parameter
                                                                             }
                                                                         >
                                                                             <StyledTableCell
                                                                                 align="left"
                                                                                 style={{
                                                                                     fontWeight: 400,
                                                                                     fontSize:
                                                                                         '16px',
                                                                                     paddingTop:
                                                                                         '4px',
                                                                                     paddingBottom:
                                                                                         '4px'
                                                                                 }}
                                                                             >
                                                                                 {
                                                                                     row.Parameter
                                                                                 }
                                                                             </StyledTableCell>
                                                                             <ValueTableCell
                                                                                 align="left"
                                                                                 style={{
                                                                                     fontWeight: 500,
                                                                                     fontSize:
                                                                                         '16px',
                                                                                     color:
                                                                                         '#000',
                                                                                     paddingTop:
                                                                                         '4px',
                                                                                     paddingBottom:
                                                                                         '4px'
                                                                                 }}
                                                                             >
                                                                                 {
                                                                                     row.Values
                                                                                 }
                                                                             </ValueTableCell>
                                                                         </TableRow>
                                                                     )
                                                                 )}
                                                             </TableBody>
                                                         </Table>
                                                     </TableContainer>
                                                 </Grid>
                                             );
                                         };;

const OfflineResponse = (request) => {
    let data = request.request;
    const rows = [
        OfflineData('Operator', data.OperatorName),
        OfflineData('Package', data.Name),
        OfflineData('Total Amount', data.Cost * data.Quantity)
    ];
    function OfflineData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <Grid item xs={12}>
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
        </Grid>
    );
};
const AEPSResponse = (request) => {
    const [mainColumns, setMainColumns] = useState([]);

    const YesImage = () => {
        return (
            <img
                src={Yes_LOGO}
                width={100}
                alt="AEPS"
            />
        );
    };

    const TickImage = () => {
        return (
            <img
                src={Tick}
                width={50}
                alt="AEPS"
            />
        );
    };

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                width={100}
                style={{ float: 'right' }}
                alt="AEPS"
            />
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


    let data = request.request;
    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '') return true;
        return false;
    };

    let rows = [];
    if (!isBlank(data.DOC)) rows.push(Data('Date & Time', data.DOC));
    if (!isBlank(data.AgentID)) rows.push(Data('Agent ID', data.AgentID));
    if (!isBlank(data.AgentName)) rows.push(Data('Agent Name', data.AgentName));
    if (!isBlank(data.ConsumerNumber)) rows.push(Data('Customer Mobile Number', data.ConsumerNumber));

    if (!isBlank(data.AadhaarNumber)) rows.push(Data('Customer Aadhaar Number', data.AadhaarNumber));

    if (!isBlank(data.TransactionID)) rows.push(Data('Transaction ID', data.TransactionID));
    if (!isBlank(data.Stan)) rows.push(Data('STAN', data.Stan));
    if (!isBlank(data.BankRRN)) rows.push(Data('RRN', data.BankRRN));
    if (!isBlank(data.IconName)) rows.push(Data('Service Name', data.IconName));

    if (!isBlank(data.BankName)) rows.push(Data('Bank Name', data.BankName));

    if (!isBlank(data.Amount)) rows.push(Data('Amount(₹)', data.Amount));

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
    }

    if (data.Status !== 'SUCCESS' && !isBlank(data.Message)) {
        rows.push(Data('Message', data.Message));
    }

    if (!isBlank(data.AvailableBalance.toString()))
        rows.push(Data('Account Balance(₹)', data.AvailableBalance.toString()));
    if (!isBlank(data.ministatementStr))
        rows.push(Data('MiniStatements', data.ministatementStr));

    let transactions = data.ministatements;

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: 'Date',
            name: 'Date'
        });

        columns.push({
            label: 'Amount(₹)',
            name: 'Amount'
        });

        columns.push({
            label: 'TxnType',
            name: 'TxnType'
        });
        columns.push({
            label: 'Narration',
            name: 'Narration'
        });
        setMainColumns(columns);
    };

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    useEffect(() => {
        createColumns();
    }, []);

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
                    <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', }}>
                        <Grid style={{ textAlign: 'center', flex: 'auto', }}>
                            <div style={{ textAlign: 'center' }}>
                                <div style={{ fontWeight: 'bold', fontSize: 20 }}>{data.IconName}</div>
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

            {transactions === undefined ||
                transactions === null ||
                transactions.length <= 0 ? null : (
                <ReactTable
                    data={transactions}
                    columns={mainColumns}
                    headerToolbar={false}
                    footerToolbar={false}
                />
            )}
        </Grid>
    );
};
const ActivationResponse = (request) => {
    let data = request.request;
    const rows = [
        OfflineData('Operator', data.OperatorName),
        OfflineData('ActivationType', data.ActivationType),
        OfflineData('AlternateNumber', data.AlternateNumber)
    ];
    function OfflineData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <Grid item xs={12}>
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
        </Grid>
    );
};

const UPITransferResponse = (request) => {
    let data = request.request;
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
    // rows.push(Data(data));

    if (!isBlank(data.BeneficiaryName))
        rows.push(Data('Beneficiary', data.BeneficiaryName));

    if (!isBlank(data.Status)) rows.push(Data('Status', data.Status));

    if (!isBlank(data.TransactionID))
        rows.push(Data('TxnID', data.TransactionID));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('AccountNo', data.ConsumerNumber));

    if (!isBlank(data.OpeningBalance.toString()))
        rows.push(Data('Op.Bal', data.OpeningBalance.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Words', NumberToWord(data.Amount)));

    if (!isBlank(data.Commission.toString()))
        rows.push(Data('Commission', data.Commission.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Cl.Bal', data.ClosingBalance.toString()));

    if (!isBlank(data.Message)) rows.push(Data('Message', data.Message));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12}>
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
            </Grid>
        </>
    );
};
const EKYCResponse = (request) => {
    let data = request.request;

    const ProfileImage = (
        <img
            src={data.ProfileImage}
            height={100}
            width={100}
            alt={data.FullName}
        />
    );
    const rows = [
        EKYCData('Profile', ProfileImage),
        EKYCData('Name', data.FullName),
        EKYCData('Aadhaar', data.AadhaarNumber),
        EKYCData('Gender', data.Gender),
        EKYCData('Dob', data.Dob),
        EKYCData('Address', data.Address),
        EKYCData('Pin', data.Pin)
    ];
    function EKYCData(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid item xs={12}>
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
        </Grid>
    );
};

const OTTTransferResponse = (request) => {
    let data = request.request;

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

    if (!isBlank(data.Status)) rows.push(Data('Status', data.Status));

    if (!isBlank(data.TransactionID))
        rows.push(Data('TxnID', data.TransactionID));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Number', data.ConsumerNumber));

    if (!isBlank(data.OpeningBalance.toString()))
        rows.push(Data('Op.Bal', data.OpeningBalance.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Words', NumberToWord(data.Amount)));

    if (!isBlank(data.Commission.toString()))
        rows.push(Data('Commission', data.Commission.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Cl.Bal', data.ClosingBalance.toString()));

    if (!isBlank(data.Message)) rows.push(Data('Message', data.Message));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12}>
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
            </Grid>
        </>
    );
};

const CreditCardResponse = (request) => {
    let data = request.request;

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

    if (!isBlank(data.Status)) rows.push(Data('Status', data.Status));

    if (!isBlank(data.TransactionID))
        rows.push(Data('TxnID', data.TransactionID));

    if (!isBlank(data.RefrenceID))
        rows.push(Data('OperatorRefNo', data.RefrenceID));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Number', data.ConsumerNumber));

    if (!isBlank(data.OpeningBalance.toString()))
        rows.push(Data('Op.Bal', data.OpeningBalance.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Commission.toString()))
        rows.push(Data('Comm.', data.Commission.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Cl.Bal', data.ClosingBalance.toString()));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    const BBPSAssuredImage = () => {
        return (
            <img
                src={BBPSAssured_LOGO}
                height={200}
                width={200}
                style={{ float: 'right' }}
                alt="BBPS"
            />
        );
    };

    return (
        <Grid item xs={12}>
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
                    {data.IsBBPS && (
                        <div
                            align="center"
                            style={{
                                position: 'absolute',
                                top: '50%',
                                left: '50%',
                                transform: 'translate(-50%, -50%)'
                            }}
                        >
                            <BBPSAssuredImage />
                        </div>
                    )}
                </Table>
            </TableContainer>
        </Grid>
    );
};


