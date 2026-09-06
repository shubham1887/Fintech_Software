import React, { useState, useEffect, forwardRef } from 'react';
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
import TableCell from '@material-ui/core/TableCell';

import TableRow from '@material-ui/core/TableRow';
import CommonPrint from 'Components/Print/CommonPrint';
import { withStyles } from '@material-ui/core/styles';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableContainer from '@material-ui/core/TableContainer';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import Yes_LOGO from 'Assets/Images/BBPS/yesbank.png';
import Tick from 'Assets/Images/BBPS/right.png';
import jsPDF from 'jspdf';

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

const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export function DMTTransactionPrint({
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
    const handleDownloadClick = () => {
        const doc = new jsPDF('p', 'pt', 'a4');

        doc.html(document.getElementById('DialogPrint'), {
            callback: function (doc) {
                doc.save('Recharge_Transaction.pdf');
            },
            x: 10,
            y: 10,
            html2canvas: {
                scale: 0.75
            },
            width: 500
        });
    };

    return (
        <Dialog
            classes={{ paperFullScreen: 'prePrint' }}
            //fullScreen
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
                    <Grid item xs={12} sm={12} md={4} lg={4}>
                        <Button
                            className={classes.CustomPrimaryButtonStylePopup}
                            onClick={handleDownloadClick}
                        >
                            Download
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={4} lg={4}>
                        <Button
                            onClick={handlePositiveA4ButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                        >
                            {positiveButtonTitle
                                ? positiveButtonTitle
                                : 'Print'}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={4} lg={4}>
                        <Button
                            onClick={handleNegativeButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {negativeButtonTitle
                                ? negativeButtonTitle
                                : 'Close'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const PrintDetails = (request) => {
    let data = request.request;
    let transaction = data.List;

    const YesImage = () => {
        return <img src={Yes_LOGO} width={90} alt="AEPS" />;
    };

    const TickImage = () => {
        return <img src={Tick} width={50} alt="Success" />;
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

    const [cName, setCName] = useState('');

    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    const classes = commonStyle();
    useEffect(() => {
        fetchCName();
    }, []);

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
    if (!isBlank(data.Amount)) rows.push(Data('Amount (₹):', data.Amount));

    if (!isBlank(data.Amount)) rows.push(Data('Charge (₹):', data.TotalCharge));
    if (!isBlank(data.Amount)) rows.push(Data('GST (₹):', data.GST));
    if (!isBlank(data.Amount))
        rows.push(Data('Total Amount (₹):', data.TotalAmount));

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(
            data.Status
        );
        rows.push(
            Data(
                'Transaction Status ',
                <span
                    style={{
                        color: statusColor
                    }}
                >
                    {statusLabel}
                </span>
            )
        );
    }
    data.DMT_BeneficiaryName !== null ||
        data.DMT_BeneficiaryName !== undefined ||
        data.DMT_BeneficiaryName !== 'Paramnotmatched' ||
        (data.DMT_BeneficiaryName.length >= 2 &&
            Data('Customer Name', data.DMT_BeneficiaryName));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid container spacing={2}>
            <Grid item xs={12}>
                <Grid container spacing={2}>
                    <Grid item xs={12}>
                        <Box
                            style={{
                                justifyContent: 'space-around',
                                display: 'flex'
                            }}
                        >
                            <YesImage />
                            {data.Status === 'SUCCESS' && <TickImage />}
                            <Logo />
                        </Box>
                    </Grid>
                </Grid>
                <TableContainer>
                    <Table aria-label="customized table">
                        <TableRow>
                            <StyledTableCell align="left" colSpan={2}>
                                <span
                                    style={{
                                        display: 'flex',
                                        flexDirection: 'row',
                                        justifyContent: 'space-around',
                                        gap: '5px'
                                    }}
                                >
                                    <Grid
                                        style={{
                                            display: 'flex',
                                            justifyContent: 'space-around',
                                            flex: 'auto'
                                        }}
                                    ></Grid>
                                </span>
                            </StyledTableCell>
                        </TableRow>
                        <StyledTableCell align="left" colSpan={2}>
                            <span
                                style={{
                                    display: 'flex',
                                    flexDirection: 'row',
                                    justifyContent: 'space-between'
                                }}
                            >
                                <Grid
                                    style={{
                                        textAlign: 'center',
                                        flex: 'auto'
                                    }}
                                >
                                    <div
                                        style={{
                                            textAlign: 'center'
                                        }}
                                    >
                                        <div
                                            style={{
                                                fontWeight: 'bold',
                                                fontSize: 20
                                            }}
                                        >
                                            {data.IconName}
                                        </div>
                                        <div
                                            style={{
                                                fontWeight: 'bold',
                                                color: '#fff',
                                                background:
                                                    data.Status === 'SUCCESS'
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
                                                fontSize: '15px',
                                                display: 'inline-block',
                                                padding: '4px 22px',
                                                borderRadius: '100px'
                                            }}
                                        >
                                            TRANSACTION {data.Status}
                                        </div>
                                    </div>
                                </Grid>
                            </span>
                        </StyledTableCell>
                    </Table>
                    <Table>
                        <TableBody>
                            {transaction.map((trans, index) => (
                                <TableContainer key={index}>
                                    <TableBody>
                                        <TableRow>
                                            <StyledTableCell
                                                align="left"
                                                style={{
                                                    padding: '10px',
                                                    width: '100%',
                                                    margin: '0px',
                                                    border: '1px solid #ddd',
                                                    boxShadow:
                                                        '0px 1px 3px rgba(0, 0, 0, 0.1)'
                                                }}
                                            >
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    Sender Mobile Number :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {data.SenderNo}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    Recipient Name :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {data.BeneName}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    Recipient A/C Number :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {data.ConsumerNumber}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    Recipient IFSC :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {data.IFSC}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    RRN :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {trans.UTRNumber}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    Date & Time :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {trans.DOC}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    Payment Mode :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {data.PaymentType}
                                                    </span>
                                                </p>
                                                <p
                                                    style={{
                                                        fontSize: '14px',
                                                        fontWeight: 'bold',
                                                        marginBottom: '5px',
                                                        color: '#000'
                                                    }}
                                                >
                                                    DMT Txn ID :
                                                    <span
                                                        style={{
                                                            fontSize: '14px',
                                                            color: '#000',
                                                            fontWeight: 'normal'
                                                        }}
                                                    >
                                                        {data.ProviderReqID}
                                                    </span>
                                                </p>
                                            </StyledTableCell>
                                        </TableRow>
                                    </TableBody>
                                </TableContainer>
                            ))}
                        </TableBody>
                    </Table>
                    <Table>
                        <TableBody>
                            {rows.map((row) => (
                                <TableRow key={row.Parameter}>
                                    <StyledTableCell
                                        align="left"
                                        style={{
                                            fontWeight: 400,
                                            fontSize: '16px',
                                            paddingTop: '4px',
                                            paddingBottom: '4px'
                                        }}
                                    >
                                        {row.Parameter}
                                    </StyledTableCell>
                                    <ValueTableCell
                                        align="left"
                                        style={{
                                            fontWeight: 500,
                                            fontSize: '16px',
                                            color: '#000',
                                            paddingTop: '4px',
                                            paddingBottom: '4px'
                                        }}
                                    >
                                        {row.Values}
                                    </ValueTableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Grid>
        </Grid>
    );
};
