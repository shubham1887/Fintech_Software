import React, { useState, useEffect, forwardRef } from 'react';
import { Grid } from '@material-ui/core';
import TableCell from '@material-ui/core/TableCell';
import TableRow from '@material-ui/core/TableRow';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import Yes_LOGO from 'Assets/Images/BBPS/yesbank.png';
import Tick from 'Assets/Images/BBPS/right.png';
import jsPDF from 'jspdf';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Slide,
    Box
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

export function BBPSPrintV2({
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
    const handleDownloadClick = () => {
        const doc = new jsPDF('p', 'pt', 'a4');

        doc.html(document.getElementById('DialogPrint'), {
            callback: function (doc) {
                doc.save('BBPS_Transaction.pdf');
            },
            x: 10,
            y: 10,
            html2canvas: {
                scale: 0.75,
            },
            width: 500,
        });
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
                    <AEPSConfirmation request={request} />
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
                            {positiveButtonTitle ? positiveButtonTitle : 'Print'}
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

const AEPSConfirmation = (request) => {
    let data = request.request;

    const [datarows, setRows] = useState([]);
    const [cName, setCName] = useState('');

    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    let transactions = data.ministatements;
    let AvailableBalance = data.AEPSResponseJSON;

    if (data.Operator === 'Mini Statement') {
        var Ministatementdata = JSON.parse(data.AEPSResponseJSON);
        AvailableBalance = Ministatementdata.Balance;
        transactions = Ministatementdata.ministatements;
        transactions = transactions.slice(0, 5);
    }

    useEffect(() => {
        createRow();
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

    const classes = commonStyle();

    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '0') return true;
        if (param === '') return true;
        return false;
    };

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                width={90}
                style={{ float: 'right' }}
                alt="AEPS"
            />
        );
    };

    const YesImage = () => {
        return (
            <img
                src={Yes_LOGO}
                width={90}
                alt="AEPS"
            />
        );
    };

    const TickImage = () => {
        return (
            <img
                src={Tick}
                width={40}
                alt="AEPS"
            />
        );
    };

    const createRow = () => {
        let rows = [];



        if (!isBlank(data.TransactionID))
            rows.push(Data('BBPS Transaction Ref ID :', data.TransactionID));

        rows.push(Data('Product :', "BBPS"));

        if (!isBlank(data.ConsumerNumber))
            rows.push(Data('Customer Mobile Number :', data.MobileNo));

        if (!isBlank(data.ServiceName
            .toString()))
            rows.push(Data('Service Name :', data.ServiceName
                .toString()));

        if (!isBlank(data.ConsumerName.toString()))
            rows.push(Data('Customer Name :', data.ConsumerName.toString()));

        if (!isBlank(data.DOC.toString()))
            rows.push(Data('Transaction Date :', data.DOC.toString()));

        if (!isBlank(data.BillDate
            .toString()))
            rows.push(Data('Bill Date :', data.BillDate
                .toString()));

        if (!isBlank(data.OperatorName.toString()))
            rows.push(Data('Biller Name :', data.OperatorName.toString()));

        if (!isBlank(data.BillerID.toString()))
            rows.push(Data('Biller ID :', data.BillerID.toString()));

        if (!isBlank(data.ConsumerNumber))
            rows.push(Data('Consumer Number :', data.ConsumerNumber));

        if (!isBlank(data.Amount.toString()))
            rows.push(Data('Amount(₹) :', data.Amount.toString()));

        if (!isBlank(data.Status)) {
            const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
            rows.push(Data(
                'Transaction Status :',
                <span style={{ color: statusColor }}>
                    {statusLabel}
                </span>
            ));
        }

        setRows(rows);
    };

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid item xs={12}>
            <Grid item xs={12}>
                <Box
                    style={{
                        border: '2px solid #000',
                        padding: '5px',
                        marginTop: '10px',
                        marginLeft: '10px',
                        backgroundColor: '#fff',
                        width: '340px',
                        height: '500px',
                        position: 'relative'
                    }}
                >
                    <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto' }}>
                        <YesImage />
                        {data.Status === 'SUCCESS' && <TickImage />}
                        <Logo />
                    </Grid>

                    <Grid style={{ textAlign: 'center', flex: 'auto' }}>
                        <div style={{ textAlign: 'center' }}>
                            <div style={{
                                fontWeight: 'bold',
                                fontSize: 10,
                                display: 'block',
                                padding: '4px 22px',
                            }}>
                                {data.IconName}
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
                                    fontSize: '10px',
                                    display: 'inline-block',
                                    padding: '4px 22px',
                                    borderRadius: '100px',
                                }}
                            >
                                TRANSACTION {data.Status}
                            </div>
                        </div>
                    </Grid>

                    <Box
                        style={{
                            marginTop: '5px',
                            color: '#000',
                            borderBottom: '1px solid #000',
                            fontSize: '14px'
                        }}
                    >
                        {datarows.map((row) => (
                            <Box
                                style={{
                                    paddingBottom: '5px'
                                }}
                            >
                                <p
                                    style={{
                                        margin: '0px',
                                        color: '#000',
                                        fontSize: '12px'
                                    }}
                                >
                                    {row.Parameter}
                                    <span style={{ fontWeight: 'bold' }}>
                                        {row.Values}
                                    </span>
                                </p>
                            </Box>
                        ))}
                    </Box>
                    {transactions === undefined ||
                        transactions === null ||
                        transactions.length <= 0
                        ? null
                        : transactions.map((trans, index) => (
                            <Box>
                                <TableRow>
                                    <StyledTableCell
                                        align="left"
                                        style={{
                                            paddingTop: '5px',
                                            paddingRight: '0px',
                                            paddingBottom: '0px',
                                            paddingLeft: '0px',
                                            width: '60%'
                                        }}
                                    >
                                        <h6
                                            style={{
                                                fontSize: '12px',
                                                fontWeight: 'normal',
                                                marginBottom: '0px',
                                                color: '#000'
                                            }}
                                        >
                                            Narration :
                                            <span
                                                style={{
                                                    fontSize: '14px',
                                                    color: '#000'
                                                }}
                                            >
                                                {trans.Narration}
                                            </span>
                                        </h6>
                                        <p
                                            style={{
                                                fontSize: '12px',
                                                marginBottom: '0px',
                                                color: '#000'
                                            }}
                                        >
                                            Rs. :
                                            <span
                                                style={{
                                                    fontSize: '14px',
                                                    fontWeight: 'bold',
                                                    color: '#000'
                                                }}
                                            >
                                                {trans.Amount}
                                            </span>
                                        </p>
                                    </StyledTableCell>
                                    <StyledTableCell
                                        align="left"
                                        style={{
                                            paddingTop: '5px',
                                            paddingRight: '0px',
                                            paddingBottom: '0px',
                                            paddingLeft: '0px',
                                            width: '50%'
                                        }}
                                    >
                                        <h6
                                            style={{
                                                fontSize: '12px',
                                                fontWeight: 'normal',
                                                marginBottom: '0px',
                                                color: '#000'
                                            }}
                                        >
                                            Date :
                                            <span
                                                style={{
                                                    fontSize: '14px',
                                                    color: '#000'
                                                }}
                                            >
                                                {trans.Date}
                                            </span>
                                        </h6>
                                        <p
                                            style={{
                                                fontSize: '12px',
                                                marginBottom: '0px',
                                                color: '#000'
                                            }}
                                        >
                                            TxnType :
                                            <span
                                                style={{
                                                    fontSize: '14px',
                                                    fontWeight: 'bold',
                                                    color: '#000'
                                                }}
                                            >
                                                {trans.TxnType}
                                            </span>
                                        </p>
                                    </StyledTableCell>
                                </TableRow>
                            </Box>
                        ))}

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
        </Grid>
    );
};
