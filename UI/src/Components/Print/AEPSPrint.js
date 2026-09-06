import React, { useState, useEffect, forwardRef } from 'react';
import { Grid } from '@material-ui/core';
import TableCell from '@material-ui/core/TableCell';
import ReactTable from 'Components/ReactTable/ReactTable';
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

export function AEPSPrint({
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
                doc.save('AEPS_Transaction.pdf');
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
                    <AEPSConfirmation request={request} />
                </DialogContentText>
            </DialogContent>




            <DialogActions displayPrint="none">
                <Grid container spacing={1} justifyContent="center">
                    <Grid item xs={12} sm={12} md={12} lg={12} style={{ textAlign: 'center' }}>
                        <Button
                            variant="contained"
                            className={classes.CustomPrimaryButtonStyle}
                            style={{
                                backgroundColor: 'primary',
                                color: '#fff',
                                marginRight: '10px'
                            }}
                            onClick={handleDownloadClick}
                        >
                            Download
                        </Button>

                        <Button
                            variant="contained"
                            className={classes.CustomPrimaryButtonStyle}
                            style={{
                                backgroundColor: 'primary',
                                color: '#fff',
                                marginRight: '10px'
                            }}
                            onClick={handlePositiveA4ButtonClick}
                        >
                            Print
                        </Button>
                        <Button
                            variant="contained"
                            className={classes.CustomSecondaryButtonStyle}
                            style={{
                                backgroundColor: 'primary',
                                color: '#fff'
                            }}
                            onClick={handleNegativeButtonClick}
                        >
                            Close
                        </Button>

                    </Grid>

                </Grid>
            </DialogActions>

        </Dialog>
    );
}

const AEPSConfirmation = (request) => {
    const [mainColumns, setMainColumns] = useState([]);

    let data = request.request;

    const [datarows, setRows] = useState([]);
    const [cName, setCName] = useState('');

    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    let transactions = data.ministatements;
    let AvailableBalance = data.AEPSResponseJSON.Balance;

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

    useEffect(() => {
        createRow();
        fetchCName();
        createColumns();

    }, []);

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

    const createRow = () => {
        let rows = [];
        if (!isBlank(data.DOC)) rows.push(Data('Date & Time :', data.DOC));
        if (!isBlank(data.AgentName)) rows.push(Data('Agent Name :', data.AgentName));
        if (!isBlank(data.AgentID)) rows.push(Data('Agent ID :', data.AgentID));
        if (!isBlank(data.MobileNumber)) rows.push(Data('Customer Mobile Number :', data.MobileNumber));
        if (!isBlank(data.ConsumerNumber)) rows.push(Data('Customer Aadhaar Number :', data.ConsumerNumber));
        if (!isBlank(data.Operator
        )) rows.push(Data('Service Name :', data.Operator
        ));
        if (!isBlank(data.ProviderRefNo)) rows.push(Data('Transaction ID :', data.ProviderRefNo));
        if (!isBlank(data.STANNo)) rows.push(Data('STAN :', data.STANNo));
        if (!isBlank(data.ReferenceNo)) rows.push(Data('RRN :', data.ReferenceNo));
        if (!isBlank(data.IconName)) rows.push(Data('Service Name :', data.IconName));

        if (!isBlank(data.BankName)) rows.push(Data('Bank Name :', data.BankName));

        if (!isBlank(data.Amount)) rows.push(Data('Amount(₹) :', data.Amount));

        if (!isBlank(data.Message)) rows.push(Data('Message :', data.Message));
        if (!isBlank(data.Status)) {
            const { statusColor, statusLabel } = getStatusLabelAndColor(data.Status);
            rows.push(Data(
                'Transaction Status :',
                <span style={{ color: statusColor }}>
                    {statusLabel}
                </span>
            ));
        }

        if (!isBlank(data.AccBalance))
            rows.push(Data('Account Balance (₹) :', data.AccBalance));

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
                        width: 'auto',
                        height: 'auto',
                        position: 'relative',
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
                        transactions.length <= 0 ? null : (
                        <ReactTable
                            data={transactions}
                            columns={mainColumns}
                            headerToolbar={false}
                            footerToolbar={false}
                        />
                    )}
                </Box>

            </Grid>
        </Grid>
    );
};
