import React, { useState, useEffect } from 'react';
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
    TextField
} from '@material-ui/core';

import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { withStyles } from '@material-ui/core/styles';
import { SelectTransferModeByIFSC as SELECT_TRANSFERMODE_BYIFSC } from 'Components/Api/TransactionAPI';

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

export function PayoutTransactionConfirmation({
    isOpen,
    title,
    request,
    positiveButtonTitle,
    positiveButtonClick,
    negativeButtonTitle,
    negativeButtonClick,
    onCustomCallback
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = commonStyle();

    const handleNegativeButtonClick = () => {
        setOpen(false);
        negativeButtonClick();
    };
    const handlePositiveButtonClick = () => {
        setOpen(false);
        positiveButtonClick(request);
    };

    return (
        <Dialog
            fullWidth
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
                    <PayoutTransferConfirmation
                        request={request}
                        onTransferModeChange={(value) => {
                            request.PaymentType = value;
                        }}
                        onRemarkChange={(value) => {
                            request.Remark = value;
                        }}
                    />
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handleNegativeButtonClick}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {negativeButtonTitle
                                ? negativeButtonTitle
                                : 'Back'}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handlePositiveButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                        >
                            {positiveButtonTitle
                                ? positiveButtonTitle
                                : 'Submit'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const PayoutTransferConfirmation = ({
    request,
    onTransferModeChange,
    onRemarkChange
}) => {
    let req = request;

    const [rows, setRows] = useState([]);
    const [isLoaded, setIsLoaded] = useState(false);
    const [transferModeList, setTransferModeList] = useState([]);

    const selectTransferMode = async () => {
        var reqObj = JSON.stringify({
            IFSC: req.IFSC,
            BankName: req.BankName
        });

        onTransferModeChange('IMPS');
        var response = await SELECT_TRANSFERMODE_BYIFSC(reqObj);

        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setTransferModeList(data);
        setIsLoaded(true);
    };

    useEffect(() => {
        selectTransferMode();
    }, [req.IFSC]);

    useEffect(() => {
        createRow();
    }, [isLoaded]);

    const OnRemarkTextChange = (value) => {
        onRemarkChange(value);
    };

    const handleRemark = () => {
        return (
            <TextField
                id="Remark"
                label="Remark"
                variant="outlined"
                onChange={(e) => {
                    let value = e.target.value;

                    OnRemarkTextChange(value);
                }}
            />
        );
    };
    const createRow = () => {
        let dummyRows = [];
        let data = request;
        dummyRows.push(Data('Payment Mode', data.PaymentType));
        dummyRows.push(Data('Bank Name', data.BankName));
        if (data.BeneficiaryName !== null)
            dummyRows.push(Data('A/c Holder Name', data.BeneficiaryName));

        dummyRows.push(Data('Account Number', data.AccountNo));
        dummyRows.push(Data('IFSC', data.IFSC));
        dummyRows.push(Data('Amount   (₹)', data.Amount));

        dummyRows.push(Data('Charge  (₹)', data.Charge));
        dummyRows.push(Data('GST (₹)', data.Gst));
        dummyRows.push(Data('Total Amount  (₹)', data.TotalAmount));

        dummyRows.push(Data('Remark', handleRemark()));

        setRows(dummyRows);
    };
    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    if (request === undefined || request === null) {
        return null;
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
