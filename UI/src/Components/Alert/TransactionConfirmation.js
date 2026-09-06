import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import {
    AEPS_CASHWITHDRAW,
    AEPS_AADHARPAY
} from 'Components/Helper/OperatorCode';
import TableRow from '@material-ui/core/TableRow';

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
    MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_VERIFY_CODE,
    PAYOUT,
    DTH_BOOKING,
    PHONE_BOOKING,
    AEPS_CODE,
    PREPAID_ACTIVATION_CODE,
    FUND_TRANSFER,
    CREDITCARDPAYMENT_CODE,
    CASHDEPOSIT_CODE,
    WALLETTOPUP_CODE,
    PAYMENT_GATEWAY_CODE,
    UPITRANSFER_CODE,
    ACCOUNT_OPENING_CODE,
    AEPS_Onboarding,
    DEMAT,
    MATM_Onboarding,
    DTH_RECHARGE_CODE,
    VENDOR_PAY,
    OTT,
    PANCARD_CODE
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

export function TransactionConfirmation({
    isOpen,
    title,
    message,
    request,
    positiveButtonTitle,
    positiveButtonClick,
    negativeButtonTitle,
    negativeButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    var IsMobikwikBBPS = localStorage.getItem('IsMobikwikBBPS') === 'true';

    const classes = commonStyle();

    const handleNegativeButtonClick = () => {
        setOpen(false);
        negativeButtonClick();
    };
    const handlePositiveButtonClick = () => {
        setOpen(false);
        positiveButtonClick(request);
    };
    const dialogStyle = IsMobikwikBBPS
        ? {
            justifyContent: 'right',
            display: 'flex',
            alignItems: 'flex-end',
            textAlign: 'right',
            marginTop: '7%'
        }
        : {};

    return (
        <Dialog
            fullWidth
            style={dialogStyle}
            maxWidth="xs"
            open={open}
            onClose={() => {
                setOpen(false);
                negativeButtonClick(false);
            }}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title" className={classes.title}>
                {title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    {parseInt(request.ServiceID) !== FUND_TRANSFER &&
                        parseInt(request.ServiceID) !== DTH_BOOKING &&
                        parseInt(request.ServiceID) !== PAYOUT &&
                        parseInt(request.ServiceID) !== VENDOR_PAY &&
                        parseInt(request.ServiceID) !== MONEY_TRANSFER_CODE &&
                        parseInt(request.ServiceID) !== AEPS_CODE &&
                        parseInt(request.ServiceID) !== PREPAID_ACTIVATION_CODE &&
                        parseInt(request.ServiceID) !== DEMAT &&
                        parseInt(request.ServiceID) !== CREDITCARDPAYMENT_CODE &&
                        parseInt(request.ServiceID) !== CASHDEPOSIT_CODE &&
                        parseInt(request.ServiceID) !== WALLETTOPUP_CODE &&
                        parseInt(request.ServiceID) !== PAYMENT_GATEWAY_CODE &&
                        parseInt(request.ServiceID) !== UPITRANSFER_CODE &&
                        parseInt(request.ServiceID) !== PHONE_BOOKING &&
                        parseInt(request.ServiceID) !== ACCOUNT_OPENING_CODE &&
                        parseInt(request.ServiceID) !== AEPS_Onboarding &&
                        parseInt(request.ServiceID) !== PANCARD_CODE &&
                        parseInt(request.ServiceID) !== MATM_Onboarding &&
                        parseInt(request.ServiceID) !== DTH_RECHARGE_CODE &&
                        parseInt(request.ServiceID) !== OTT &&
                        parseInt(request.ServiceID) !== PHONE_BOOKING ? (
                        <RechargeConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === MONEY_TRANSFER_CODE &&
                        parseInt(request.OperatorID) ===
                        MONEY_TRANSFER_OPERATOR_TRANSFER_CODE ? (
                        <DMTTransferConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === MONEY_TRANSFER_CODE &&
                        parseInt(request.OperatorID) ===
                        MONEY_TRANSFER_OPERATOR_VERIFY_CODE ? (
                        <DMTVerifyConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === PAYOUT ||
                        parseInt(request.ServiceID) === VENDOR_PAY ? (
                        <PayoutConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === DTH_BOOKING ||
                        parseInt(request.ServiceID) === PHONE_BOOKING ? (
                        <OfflineConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === AEPS_CODE ? (
                        <AEPSConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === CREDITCARDPAYMENT_CODE ? (
                        <CreditCardPaymentConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === CASHDEPOSIT_CODE ? (
                        <CashDepositConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === WALLETTOPUP_CODE ? (
                        <WalletTopUpConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === PREPAID_ACTIVATION_CODE ? (
                        <ActivationConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === FUND_TRANSFER ? (
                        <FundTrasferConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === PAYMENT_GATEWAY_CODE ? (
                        <PGConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === UPITRANSFER_CODE ? (
                        <UPITransferConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === ACCOUNT_OPENING_CODE ? (
                        <AccountOpeningConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === AEPS_Onboarding ? (
                        <OnbordingConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === PANCARD_CODE ? (
                        <PanCreationConfirmation request={request} />
                    ) : null}

                    {parseInt(request.ServiceID) === MATM_Onboarding ? (
                        <OnbordingConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === DEMAT ? (
                        <DematConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === OTT ? (
                        <OTTConfirmation request={request} />
                    ) : null}
                    {parseInt(request.ServiceID) === DTH_RECHARGE_CODE ? (
                        <DTHConfirmation request={request} />
                    ) : null}
                    <p> {message}</p>
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handleNegativeButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                            style={{ textTransform: 'none' }}
                        >
                            {negativeButtonTitle
                                ? negativeButtonTitle
                                : 'Cancel'}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handlePositiveButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                            style={{ textTransform: 'none' }}
                        >
                            {positiveButtonTitle
                                ? positiveButtonTitle
                                : 'Pay'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const WalletTopUpConfirmation = (request) => {
    let data = request.request;
    const rows = [
        WalletTopUpData('Operator Name', data.OperatorName),
        WalletTopUpData('Mobile Number', data.MobileNumber),
        WalletTopUpData('Amount', data.Amount),
        WalletTopUpData('Words', NumberToWord(data.Amount))
    ];
    function WalletTopUpData(Parameter, Values) {
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

const CashDepositConfirmation = (request) => {
    let data = request.request;
    const rows = [
        CashDepositData('Account Number', data.AccountNumber),
        CashDepositData('Mobile Number', data.MobileNumber),
        CashDepositData('Amount', data.Amount),
        CashDepositData('Words', NumberToWord(data.Amount))
    ];
    function CashDepositData(Parameter, Values) {
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

const CreditCardPaymentConfirmation = (request) => {
    let data = request.request;
    const rows = [
        CreditCardPaymentData('Card Network', data.OperatorName),
        CreditCardPaymentData('Card Number', data.CardNumber),
        CreditCardPaymentData('Card Name', data.Name),
        CreditCardPaymentData('Amount', data.Amount),
        CreditCardPaymentData('Words', NumberToWord(data.Amount))
    ];
    function CreditCardPaymentData(Parameter, Values) {
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

const PayoutConfirmation = (request) => {
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

    if (!isBlank(data.PaymentType.toString()))
        rows.push(Data('PaymentType', data.PaymentType.toString()));

    if (!isBlank(data.AccountNo)) rows.push(Data('Number', data.AccountNo));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount', data.Amount.toString()));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Words', NumberToWord(data.Amount)));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <>
            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
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

const DTHConfirmation = (request) => {
    let data = request.request;
    const OperatorImage = (
        <img
            src={data.OperatorImagePath}
            height={100}
            width={100}
            alt={data.OperatorName}
        />
    );
    const rows = [
        RechargeData(OperatorImage, data.OperatorName),
        RechargeData('Subscriber ID', data.ConsumerNumber),
        RechargeData('Amount', data.Amount === '' ? '0' : data.Amount),
    ];
    function RechargeData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableBody>
                        <TableRow>
                            <StyledTableCell align="left"
                                colSpan={2}
                                style={{ paddingLeft: '0px' }}
                            >
                                Please review the details below before proceeding
                            </StyledTableCell>
                        </TableRow>
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

const RechargeConfirmation = (request) => {
    let data = request.request;
    const OperatorImage = (
        <img
            src={data.OperatorImagePath}
            height={100}
            width={100}
            alt={data.OperatorName}
        />
    );
    const rows = [
        RechargeData(OperatorImage, data.OperatorName),
        RechargeData('Number', data.ConsumerNumber),
        RechargeData('Amount(₹)', data.Amount === '' ? '0' : data.Amount),

    ];
    function RechargeData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableBody>
                        <TableRow>
                            <StyledTableCell align="left"
                                colSpan={2}
                                style={{ paddingLeft: '0px' }}
                            >
                                Please review the details below before proceeding
                            </StyledTableCell>
                        </TableRow>
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

const DMTTransferConfirmation = (request) => {
    let data = request.request;
    const rows = [
        DMTData('Name', data.BeniName),
        DMTData('Bank', data.BankName),
        DMTData('IFSC', data.IFSC),
        DMTData('Account', data.AccountNumber),
        DMTData('Amount', data.Amount),
        DMTData('Word', NumberToWord(data.Amount))
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

const DMTVerifyConfirmation = (request) => {
    const [rows, setRows] = useState([]);
    const createRow = () => {
        let data = request.request;
        data.Amount = 1;

        let dummyRows = [];
        dummyRows.push(VerifyData('Name', data.BeniName));
        dummyRows.push(VerifyData('Bank', data.BankName));

        if (data.IFSC != 'ABCD0000000') {
            dummyRows.push(VerifyData('IFSC', data.IFSC));
        }
        dummyRows.push(VerifyData('Account', data.AccountNumber));
        dummyRows.push(VerifyData('Amount', data.Amount));
        dummyRows.push(VerifyData('Word', NumberToWord(data.Amount)));
        setRows(dummyRows);
    };
    useEffect(() => {
        createRow();
    }, []);
    function VerifyData(Parameter, Values) {
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

const OfflineConfirmation = (request) => {
    let data = request.request;
    const rows = [
        OfflineData('Operator', data.OperatorName),
        OfflineData('Package', data.Name),
        OfflineData('Total Amount', data.BookingAmount)
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

const AEPSConfirmation = (request) => {
    let data = request.request;
    if (
        data.OperatorID === AEPS_AADHARPAY ||
        data.OperatorID === AEPS_CASHWITHDRAW
    ) {
        const rows = [
            AEPSData('Type', data.OperatorName),
            AEPSData('Aadhaar', data.AadhaarNumber),
            AEPSData('Mobile', data.ConsumerNumber),
            AEPSData('Bank', data.BankName),
            AEPSData('Amount', data.Amount),
            AEPSData('Service Charge', data.Charge)
        ];
        function AEPSData(Parameter, Values) {
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
    } else {
        const rows = [
            AEPSData('Type', data.OperatorName),
            AEPSData('Aadhaar', data.AadhaarNumber),
            AEPSData('Mobile', data.ConsumerNumber),
            AEPSData('Bank', data.BankName)
        ];
        function AEPSData(Parameter, Values) {
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
    }
};
const ActivationConfirmation = (request) => {
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
const DematConfirmation = (request) => {
    let data = request.request;
    const rows = [
        OfflineData('Name', data.PersonName),
        OfflineData('MobileNumber', data.MobileNumber),
        OfflineData('EmailID', data.EmailID),
        OfflineData('PanNumber', data.PanNumber)
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
const FundTrasferConfirmation = (request) => {
    let data = request.request;
    const rows = [
        OfflineData('FirmName', data.User.FirmName),
        OfflineData('Wallet', data.Name),
        OfflineData('Amount', data.TotalAmount)
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

const PGConfirmation = (request) => {
    let data = request.request;
    if (data.IsCommissionShow) {
        const rows = [
            PGData('Wallet', data.Name),
            PGData('Amount', data.Amount)
            // PGData('Commi Type', data.CommissionCategory),
            // PGData('Commi', data.Commission)
        ];
        function PGData(Parameter, Values) {
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
    } else {
        const rows = [
            PGData('Wallet', data.Name),
            PGData('Amount', data.Amount)
        ];
        function PGData(Parameter, Values) {
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
    }
};

const UPITransferConfirmation = (request) => {
    const [rows, setRows] = useState([]);
    const createRow = () => {
        let data = request.request;
        let IsVerify = data.IsVerify;

        let dummyRows = [];
        dummyRows.push(UPITransferData('Payment Type', data.PaymentType));
        dummyRows.push(UPITransferData('UPI ID', data.UpiID));

        if (IsVerify === false) {
            dummyRows.push(UPITransferData('Amount', data.Amount));
            dummyRows.push(UPITransferData('Words', NumberToWord(data.Amount)));
        }
        setRows(dummyRows);
    };
    useEffect(() => {
        createRow();
    }, []);
    function UPITransferData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
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

const OnbordingConfirmation = (request) => {
    let data = request.request;
    const rowsupi = [
        UPITransferData('Service Charge', data.Charge),
        UPITransferData('Words', NumberToWord(data.Charge))
    ];
    function UPITransferData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                <TableContainer>
                    <Table aria-label="customized table">
                        <TableBody>
                            {rowsupi.map((row) => (
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
const PanCreationConfirmation = (request) => {
    let data = request.request;
    const rowsupi = [
        UPITransferData('Operator', data.OperatorName),
        UPITransferData('Charge', data.Charge),
        UPITransferData('Words', NumberToWord(data.Charge))
    ];
    function UPITransferData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                <TableContainer>
                    <Table aria-label="customized table">
                        <TableBody>
                            {rowsupi.map((row) => (
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

const AccountOpeningConfirmation = (request) => {
    let data = request.request;
    const rows = [Data('Account Type', data.OperatorName)];
    function Data(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
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

const OTTConfirmation = (request) => {
    let data = request.request;

    const rows = [
        RechargeData('Operator', data.OperatorName),
        RechargeData('Amount', data.Amount === '' ? '0' : data.Amount),
        RechargeData('Words', data.InWords)
    ];
    function RechargeData(Parameter, Values) {
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
