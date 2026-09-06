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
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CLIENT_NAME } from 'Config';
import { withStyles } from '@material-ui/core/styles';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import Yes_LOGO from 'Assets/Images/BBPS/yesbank.png';
import Tick from 'Assets/Images/BBPS/right.png';
import jsPDF from 'jspdf';

import {
    DTH_RECHARGE_CODE,
    PREPAID_RECHARGE_CODE,
} from 'Components/Helper/Messages';
const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

var IsMobikwikBBPS = localStorage.getItem('IsMobikwikBBPS') === 'true';

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

export function TransactionPrint({
    isOpen,
    title,
    request,
    ServiceID,
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
                    <DialogContent style={{ overflowY: 'initial' }}>
                        <DialogContentText id="alert-dialog-description">
                            {parseInt(ServiceID) === PREPAID_RECHARGE_CODE ? (
                                <RechargeConfirmation request={request} />
                            ) : null}

                            {parseInt(ServiceID) === DTH_RECHARGE_CODE ? (
                                <DTHConfirmation request={request} />
                            ) : null}

                        </DialogContentText>
                    </DialogContent>
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

    const [cName, setCName] = useState('');

    const BBPSAssuredImage = () => {
        return (
            <img
                src={BBPSAssured_LOGO}
                height={50}
                width={50}
                style={{ float: 'right' }}
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

    if (!isBlank(data.ConsumerNumber))
        rows.push(Data(' Mobile Number ', data.ConsumerNumber));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Transaction Amount (₹) ', data.Amount.toString()));

    if (!isBlank(data.DOC)) rows.push(Data('Transaction Date & Time ', data.DOC));

    if (!isBlank(data.ReferenceNo))
        rows.push(Data('Operator Transaction ID ', data.ReferenceNo));

    if (!isBlank(data.ProviderRequestID))
        rows.push(Data('Transaction ID ', data.ProviderRequestID));

    if (!isBlank(data.ServiceName.toString()))
        rows.push(Data('Service Name ', data.ServiceName.toString()));

    if (!isBlank(data.Operator.toString()))
        rows.push(Data('Operator Name ', data.Operator.toString()));

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status ',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
    }

    if (IsMobikwikBBPS) {
        if (!isBlank(data.ProviderRefNo.toString()))
            rows.push(Data('BBPS ref id ', data.ProviderRefNo));

        rows.push(Data('CCF ', 1));
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
                    <TableRow>
                        <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-around', gap: '5px', padding: '10px' }}>
                            <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto', }}>
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
        </Grid>
    );
};

const DTHConfirmation = (request) => {
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

    if (!isBlank(data.MobileNumber))
        rows.push(Data(' Mobile Number ', data.MobileNumber));

    if (!isBlank(data.Amount.toString()))
        rows.push(Data('Transaction Amount (₹) ', `₹${data.Amount.toString()}`));

    if (!isBlank(data.DOC)) rows.push(Data('Transaction Date & Time ', data.DOC));

    if (!isBlank(data.ReferenceNo))
        rows.push(Data('Operator Transaction ID ', data.ReferenceNo));

    if (!isBlank(data.ProviderRequestID))
        rows.push(Data('Transaction ID ', data.ProviderRequestID));

    if (!isBlank(data.ServiceName.toString()))
        rows.push(Data('Service Name ', data.ServiceName.toString()));

    if (!isBlank(data.Operator.toString()))
        rows.push(Data('Operator Name ', data.Operator.toString()));

    if (!isBlank(data.ConsumerNumber.toString()))
        rows.push(Data('Reference ID ', data.ConsumerNumber.toString()));

    if (!isBlank(data.Status)) {
        const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
        rows.push(Data(
            'Transaction Status ',
            <span style={{ color: statusColor }}>
                {statusLabel}
            </span>
        ));
    }


    if (IsMobikwikBBPS) {
        if (!isBlank(data.ProviderRefNo.toString()))
            rows.push(Data('BBPS ref id :', data.ProviderRefNo));

        rows.push(Data('CCF', 1));
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
                    <TableRow>
                        <StyledTableCell align="left" colSpan={2}><span style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-around', gap: '5px', padding: '10px' }}>
                            <Grid style={{ display: 'flex', justifyContent: 'space-around', flex: 'auto', }}>
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
        </Grid>
    );
};
