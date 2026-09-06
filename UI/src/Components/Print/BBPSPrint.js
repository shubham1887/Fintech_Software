import React, { useState, useEffect, forwardRef } from 'react';
import { Grid } from '@material-ui/core';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TableRow from '@material-ui/core/TableRow';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Slide
} from '@material-ui/core';
import CommonPrint from 'Components/Print/CommonPrint';
import BBPSAssured_LOGO from 'Assets/Images/BBPS/BBPS-Assured.png';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import Yes_LOGO from 'Assets/Images/BBPS/yesbank.png';
import Tick from 'Assets/Images/BBPS/right.png';
import jsPDF from 'jspdf';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CLIENT_NAME } from 'Config';
import { withStyles } from '@material-ui/core/styles';
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

export function BBPSPrint({
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
                    <RechargeConfirmation request={request} />
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

const RechargeConfirmation = (request) => {
    let data = request.request;
    const [cName, setCName] = useState('');

    const BBPSAssuredImage = () => {
        return (
            <img
                src={BBPSAssured_LOGO}
                height={50}
                width={50}
                style={{ float: 'centre' }}
                alt="BBPS"
            />
        );
    };

    const Logo = () => {
        return (
            <img
                src={CompanyLogo}
                width={90}
                style={{ float: 'right' }}
                alt="BBPS"
            />
        );
    };

    const YesImage = () => {
        return (
            <img
                src={Yes_LOGO}
                width={90}
                alt="BBPS"
            />
        );
    };

    const TickImage = () => {
        return (
            <img
                src={Tick}
                width={40}
                alt="BBPS"
            />
        );
    };
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    const classes = commonStyle();
    useEffect(() => {
        fetchCName();
    }, []);
    const OperatorImage = (
        <img
            src={data.OperatorImagePath}
            height={50}
            width={50}
            alt={data.Operator}
        />
    );

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

    rows.push(Data(OperatorImage, data.Operator));


    if (!isBlank(data.TransactionID))
        rows.push(Data('BBPS Transaction Ref ID', data.TransactionID));

    rows.push(Data('Product', "BBPS"));

    if (!isBlank(data.MobileNumber))
        rows.push(Data('Customer Mobile Number', data.MobileNumber));

    if (!isBlank(data.ServiceName
        .toString()))
        rows.push(Data('Service Name', data.ServiceName
            .toString()));

    if (!isBlank(data.BBPS_CustomerName.toString()))
        rows.push(Data('Customer Name', data.BBPS_CustomerName.toString()));

    if (!isBlank(data.DOC.toString()))
        rows.push(Data('Transaction Date', data.DOC.toString()));

    if (!isBlank(data.BillDate
        .toString()))
        rows.push(Data('Bill Date', data.BillDate
            .toString()));

    if (!isBlank(data.Operator
        .toString()))
        rows.push(Data('Biller Name', data.Operator
            .toString()));

    if (!isBlank(data.BillerID.toString()))
        rows.push(Data('Biller ID', data.BillerID.toString()));

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data('Consumer Number', data.ConsumerNumber));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Amount(₹) ', data.Amount.toString()));

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status ',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
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
            {/* <h3 className={classes.document_h3}>Thank you for using {cName}</h3> */}
        </Grid>
    );
};
