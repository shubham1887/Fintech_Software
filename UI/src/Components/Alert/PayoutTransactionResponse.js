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
    PREPAID_RECHARGE_CODE
} from 'Components/Helper/Messages';

import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { withStyles } from '@material-ui/core/styles';
import { CreditCard } from '@material-ui/icons';
import { VendorPay } from 'Components/Api/TransactionAPI';
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

export function PayoutTransactionResponse({

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
    const Logo = (
        <img
            src={CompanyLogo}
            height={80}
            width={150}
        />
    );
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


            >
                <div style={{ textAlign: 'center' }}>
                    <div style={{
                        fontWeight: 'bold',
                        fontSize: 10,
                        display: 'block',
                        padding: '4px 22px',
                    }}>
                        {Logo}
                    </div>
                    <div
                        style={{
                            fontWeight: 'bold',
                            color: '#fff',
                            background: bgColor,
                            fontSize: '15px',
                            display: 'inline-block',
                            padding: '4px 22px',
                            borderRadius: '100px',
                        }}
                    >
                        {title}
                    </div>
                    <hr></hr>
                    <span style={{ fontSize: "15px" }}>
                        Your request to withdraw funds to your bank account
                        has been received. You will receive a notification from
                        your bank once the funds are credited.
                    </span>
                </div>
                <hr></hr>
            </DialogTitle>

            <DialogContent>
                <DialogContentText id="alert-dialog-description">

                    <PayoutResponse request={request} />
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

        return false;
    };

    let rows = [];
    if (!isBlank(data.DateAndTime))
        rows.push(Data('Date & Time', data.DateAndTime));
    if (!isBlank(data.VendorID))
        rows.push(Data('Reference ID', data.VendorID));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Account Number', data.ConsumerNumber));
    if (!isBlank(data.BankName))
        rows.push(Data('Bank Name', data.BankName));

    if (!isBlank(data.Charges.toString()))
        rows.push(Data('Charges  (₹)', data.Charges.toString()));

    if (!isBlank(data.GST.toString()))
        rows.push(Data('GST  (₹)', data.GST.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Total Amount  (₹)', data.Amount.toString()));
    if (!isBlank(data.OpeningBalance.toString()))
        rows.push(Data('Opening My Settlement  (₹)', data.OpeningBalance.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Closing  My Settlement  (₹)', data.ClosingBalance.toString()));

    if (!isBlank(data.VendorID))
        rows.push(Data('Transaction ID', data.VendorID));
    if (!isBlank(data.TransactionID))
        rows.push(Data('UTR no/ Bank txn ref ', data.TransactionID));
    if (!isBlank(data.Status))
        rows.push(Data('Status', data.Status));
    if (!isBlank(data.remark)) rows.push(Data('remark', data.remark));

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

    if (data.IsBBPS) rows.push(Data(<BBPSAssuredImage />));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Commission.toString()))
        rows.push(Data('Comm.', data.Commission.toString()));

    if (!isBlank(data.ClosingBalance.toString()))
        rows.push(Data('Cl.Bal', data.ClosingBalance.toString()));

    function Data(Parameter, Values) {
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

const BBPSResponse = (request) => {
    let data = request.request;
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

    const YesImage = () => {
        return (
            <img
                src={Yes_LOGO}
                height={50}
                width={50}
                alt="BBPS"
            />
        );
    };

    const TickImage = () => {
        return (
            <img
                src={Tick}
                height={50}
                width={50}
                alt="BBPS"
            />
        );
    };

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                height={50}
                width={50}
                style={{ float: 'right' }}
                alt="BBPS"
            />
        );
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

    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableBody>
                        <TableRow>
                            <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', gap: '10px', padding: '20px' }}>
                                <Grid style={{ display: 'flex', justifyContent: 'space-between', flex: 'auto', }}>
                                    <YesImage />
                                    {data.Status === 'SUCCESS' && <TickImage />}
                                    <Logo />
                                </Grid>
                            </span></StyledTableCell>
                        </TableRow>
                        <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', gap: '10px', padding: '20px' }}>
                            <Grid style={{ textAlign: 'center', flex: 'auto', }}>
                                <p> <BBPSAssuredImage /></p>
                                <div style={{ marginTop: '10px', textAlign: 'center' }}>
                                    <div style={{ fontWeight: 'bold' }}>BBPS Bill Payment</div>
                                    <div style={{ color: 'green' }}>TRANSACTION {data.Status}</div>
                                </div>
                            </Grid>
                        </span>
                        </StyledTableCell>
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

const DMTTransferResponse = (request) => {

    let data = request.request;
    const rows = [
        DMTData('Status', data.Status),
        DMTData('RRN', data.TransactionID),
        DMTData('AccountNo', data.ConsumerNumber),
        DMTData('Beneficiary', data.BeneficiaryName),
        DMTData('Bank Name', data.BankName),
        DMTData('IFSC', data.IFSC),
        DMTData('Amount', data.Amount === '' ? '0' : data.Amount),
        DMTData('Message', data.Description)
    ];
    function DMTData(Parameter, Values) {
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
                height={50}
                width={50}
                alt="AEPS"
            />
        );
    };

    const TickImage = () => {
        return (
            <img
                src={Tick}
                height={50}
                width={50}
                alt="AEPS"
            />
        );
    };

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                height={50}
                width={50}
                style={{ float: 'right' }}
                alt="AEPS"
            />
        );
    };

    let data = request.request;
    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '') return true;
        return false;
    };

    let rows = [];
    if (!isBlank(data.Status)) rows.push(Data('Status', data.Status));
    if (!isBlank(data.BankName)) rows.push(Data('BankName', data.BankName));
    if (!isBlank(data.BankRRN)) rows.push(Data('BankRRN', data.BankRRN));
    if (!isBlank(data.Amount)) rows.push(Data('Amount', data.Amount));
    if (!isBlank(data.AvailableBalance.toString()))
        rows.push(Data('AvailableBalance', data.AvailableBalance.toString()));
    if (!isBlank(data.Message)) rows.push(Data('Message', data.Message));
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
            label: 'Amount',
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
                        <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', gap: '10px', padding: '20px' }}>
                            <Grid style={{ display: 'flex', justifyContent: 'space-between', flex: 'auto', }}>
                                <YesImage />
                                {data.Status === 'SUCCESS' && <TickImage />}
                                <Logo />
                            </Grid>
                        </span></StyledTableCell>
                    </TableRow>
                    <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', gap: '10px', padding: '20px' }}>
                        <Grid style={{ textAlign: 'center', flex: 'auto', }}>
                            <div style={{ marginTop: '10px', textAlign: 'center' }}>
                                <div style={{ fontWeight: 'bold' }}>AEPS</div>
                                <div style={{ color: 'green' }}>TRANSACTION {data.Status}</div>
                            </div>
                        </Grid>
                    </span>
                    </StyledTableCell>
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
