import React, { useState, useEffect, forwardRef } from 'react';
import ReceiptLogo from 'Components/Logo/ReceiptLogo';
import {
    Button,
    Dialog,
    DialogContent,
    DialogActions,
    DialogContentText,
    DialogTitle,
    Slide,
    Box,
    Paper,
    Typography,
    Divider,
    TableContainer,
    Table
} from '@material-ui/core';
import { CLIENT_NAME } from 'Config';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Grid } from '@material-ui/core';
import TableCell from '@material-ui/core/TableCell';
import Success from 'Assets/Images/success.png';
import Fail from 'Assets/Images/fail.png';
import CommonPrint from 'Components/Print/CommonPrint';
import { withStyles } from '@material-ui/core/styles';
import TableRow from '@material-ui/core/TableRow';
import TableHead from '@material-ui/core/TableHead';
import TableBody from '@material-ui/core/TableBody';

const StyledTableCell = withStyles((theme) => ({
    body: {
        fontSize: 16,
        fontWeight: 'bold'
    }
}))(TableCell);

const Transition = forwardRef(function Transition(props, ref) {
    return <Slide direction="up" ref={ref} {...props} />;
});

export function DMTTransactionPrintV2({
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
        <Dialog
            open={open}
            onClose={() => {
                setOpen(false);
                negativeButtonClick(false);
            }}
            TransitionComponent={Transition}
            PaperProps={{
                style: {
                    maxWidth: 1000,
                    width: '100%'
                }
            }}
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
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handleNegativeButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {negativeButtonTitle
                                ? negativeButtonTitle
                                : 'No, Cancel Print'}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={3} lg={3}>
                        <Button
                            onClick={handlePositiveA4ButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                        >
                            {positiveButtonTitle ? positiveButtonTitle : 'A4'}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={3} lg={3}>
                        <Button
                            onClick={handlePositiveA5ButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                        >
                            {positiveButtonTitle ? positiveButtonTitle : 'A5'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const PrintDetails = (request) => {
    let data = request.request;
    const [logo, setLogo] = useState([]);
    const [IsShowCharge, setIsShowCharge] = useState(true);
    const [cName, setCName] = useState('');
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    let transaction = data.List;

    const ShowServiceCharge = () => {
        if (data.Charge === '0') {
            setIsShowCharge(false);
            return;
        }
    };

    useEffect(() => {
        ShowServiceCharge();
        createRowForLogo();
        // createRow();
        fetchCName();
    }, []);

    const createRowForLogo = () => {
        let dummyRows = [];
        dummyRows.push(LogoData(<ReceiptLogo />, 'Date:', transaction[0].DOC));
        setLogo(dummyRows);
    };

    function LogoData(Parameter1, Parameter2, Values2) {
        return { Parameter1, Parameter2, Values2 };
    }

    return (
        <>
            <Grid container style={{ marginBottom: '14px' }}>
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={12}
                    lg={12}
                    xl={12}
                    style={{ textAlign: 'center' }}
                >
                    <ReceiptLogo />
                </Grid>

                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={12}
                    lg={12}
                    xl={12}
                    style={{ marginTop: '16px' }}
                >
                    <Typography style={{ fontWeight: '600', color: '#000' }}>
                        {' '}
                        {data.FirmName}
                    </Typography>
                </Grid>

                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                    <Typography
                        style={{
                            fontWeight: '400',
                            color: '#000',
                            fontSize: '18px',
                            marginTop: '12px'
                        }}
                    >
                        Sender : {data.SenderNo} ({data.PersonName})
                    </Typography>
                </Grid>
            </Grid>

            <Divider />

            <Grid container>
                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                    <TableContainer style={{ boxShadow: '0 0 0 0' }}>
                        <Table style={{ width: '100%' }}>
                            <TableHead>
                                <TableRow style={{ padding: '0px' }}>
                                    <TableCell
                                        style={{
                                            borderBottom: 'none',
                                            padding: '4px',
                                            textAlign: 'left'
                                        }}
                                    >
                                        Name: {data.BeneName}
                                    </TableCell>
                                    <TableCell
                                        style={{
                                            borderBottom: 'none',
                                            padding: '4px',
                                            textAlign: 'left'
                                        }}
                                    >
                                        Bank: {data.Bank}
                                    </TableCell>
                                </TableRow>
                                <TableRow>
                                    <TableCell
                                        style={{
                                            borderBottom: 'none',
                                            padding: '4px',
                                            textAlign: 'left'
                                        }}
                                    >
                                        A/c No.: {data.ConsumerNumber}
                                    </TableCell>
                                    <TableCell
                                        style={{
                                            borderBottom: 'none',
                                            padding: '4px',
                                            textAlign: 'left'
                                        }}
                                    >
                                        IFSC: {data.IFSC}
                                    </TableCell>
                                </TableRow>
                                <TableRow>
                                    <TableCell
                                        style={{
                                            borderBottom: 'none',
                                            padding: '4px',
                                            textAlign: 'left'
                                        }}
                                    >
                                        Total Amount: {data.TotalAmount}
                                    </TableCell>
                                    <TableCell
                                        style={{
                                            borderBottom: 'none',
                                            padding: '4px',
                                            textAlign: 'left'
                                        }}
                                    >
                                        Charge: {data.Charge}
                                    </TableCell>
                                </TableRow>
                            </TableHead>
                        </Table>
                    </TableContainer>
                </Grid>
            </Grid>

            <Divider />

            <Grid container>
                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                    <TableContainer style={{ boxShadow: '0 0 0 0' }}>
                        <Table style={{ width: '100%' }}>
                            <TableHead>
                                <TableRow>
                                    <TableCell style={{ textAlign: 'left' }}>
                                        Transaction Details on
                                    </TableCell>
                                    <TableCell style={{ textAlign: 'left' }}>
                                        {transaction[0].DOC}
                                    </TableCell>
                                </TableRow>
                            </TableHead>
                        </Table>
                    </TableContainer>
                </Grid>
            </Grid>

            <Grid container style={{ marginTop: '14px' }}>
                <Grid item xs={12} sm={12} md={12} lg={12} xl={12}>
                    <TableContainer component={Paper}>
                        <Table style={{ width: '100%', textAlign: 'left' }}>
                            <TableHead style={{ textAlign: 'left' }}>
                                <TableRow>
                                    <TableCell
                                        style={{
                                            textAlign: 'left'
                                        }}
                                    >
                                        Sr.
                                    </TableCell>
                                    <TableCell
                                        style={{
                                            textAlign: 'left'
                                        }}
                                    >
                                        UTR
                                    </TableCell>
                                    <TableCell
                                        style={{
                                            textAlign: 'left'
                                        }}
                                    >
                                        Amount
                                    </TableCell>
                                    <TableCell
                                        style={{
                                            textAlign: 'left'
                                        }}
                                    >
                                        Status
                                    </TableCell>
                                </TableRow>
                            </TableHead>

                            {transaction.map((trans, index) => (
                                <TableBody>
                                    <TableCell style={{ textAlign: 'left' }}>
                                        {trans.ID}
                                    </TableCell>
                                    <TableCell style={{ textAlign: 'left' }}>
                                        {trans.UTRNumber}
                                    </TableCell>
                                    <TableCell style={{ textAlign: 'left' }}>
                                        {trans.Amount}
                                    </TableCell>
                                    <TableCell style={{ textAlign: 'left' }}>
                                        {trans.Status}
                                    </TableCell>
                                </TableBody>
                            ))}
                        </Table>
                    </TableContainer>
                </Grid>
            </Grid>

            {IsShowCharge ? (
                <Grid
                    item
                    xs={12}
                    sm={12}
                    md={5}
                    lg={5}
                    xl={5}
                    style={{ marginTop: '12px' }}
                >
                    <Typography
                        style={{
                            fontWeight: '400',
                            color: '#000',
                            fontSize: '18px'
                        }}
                    >
                        <b>Service Charge :</b> Rs.{data.Charge}
                    </Typography>
                </Grid>
            ) : null}

            <Box>
                <p
                    style={{
                        padding: '3px 5px',
                        margin: '0px',
                        backgroundColor: '#ccc',
                        color: '#000',
                        fontSize: '10px',
                        textAlign: 'center',
                        bottom: '5px',
                        width: '100%'
                    }}
                >
                    This is system generated receipt. Hence no seal or signature
                    required
                    <br />
                    Thank you for using {cName}
                </p>
            </Box>
        </>
    );
};
