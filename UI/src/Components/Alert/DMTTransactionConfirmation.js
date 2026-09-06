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
    MenuItem,
    TextField
} from '@material-ui/core';

import {
    MONEY_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_VERIFY_CODE
} from 'Components/Helper/Messages';
import { SelectWithoutFormik } from 'Components/Inputes/SelectWithoutFormik';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { SelectTransferModeByIFSC as SELECT_TRANSFERMODE_BYIFSC } from 'Components/Api/TransactionAPI';
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

export function DMTTransactionConfirmation({
    isOpen,
    ActiveDMTName,
    title,
    message,
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
                    {parseInt(request.ServiceID) === MONEY_TRANSFER_CODE &&
                    parseInt(request.OperatorID) ===
                        MONEY_TRANSFER_OPERATOR_TRANSFER_CODE ? (
                        <DMTTransferConfirmation
                            request={request}
                            ActiveDMTName={ActiveDMTName}
                            onTransferModeChange={(value) => {
                                request.PaymentType = value;
                            }}
                            onRemarkChange={(value) => {
                                request.Remark = value;
                            }}
                        />
                    ) : null}

                    {parseInt(request.ServiceID) === MONEY_TRANSFER_CODE &&
                    parseInt(request.OperatorID) ===
                        MONEY_TRANSFER_OPERATOR_VERIFY_CODE ? (
                        <DMTVerifyConfirmation request={request} />
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
                        >
                            {negativeButtonTitle
                                ? negativeButtonTitle
                                : ' Cancel '}
                        </Button>
                    </Grid>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={handlePositiveButtonClick}
                            className={classes.CustomPrimaryButtonStylePopup}
                        >
                            {positiveButtonTitle
                                ? positiveButtonTitle
                                : ' Proceed '}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}

const DMTTransferConfirmation = ({
    request,
    ActiveDMTName,
    onTransferModeChange
}) => {
    let req = request;
    const [rows, setRows] = useState([]);
    const [isLoaded, setIsLoaded] = useState(false);

    const [transferModeList, setTransferModeList] = useState([]);
    const [defaultValue, setDefaultValue] = useState('');

    const selectTransferMode = async () => {
        var reqObj = JSON.stringify({
            ActiveDMT: ActiveDMTName,
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
    }, [req.IFSC, ActiveDMTName]);

    useEffect(() => {
        createRow();
    }, [isLoaded]);

    const OnPaymodeChange = (value) => {
        onTransferModeChange(value);
    };

    const handleTransferMode = (transferMode) => {
        if (transferMode === undefined || transferMode.length <= 0) {
            setDefaultValue('IMPS');
            onTransferModeChange('IMPS');
            return 'IMPS';
        }

        if (transferMode.length === 1) {
            setDefaultValue(transferMode[0].TransferMode);
            onTransferModeChange(transferMode[0].TransferMode);
            return transferMode[0].TransferMode;
        }
        setDefaultValue(transferMode[0].TransferMode);
        onTransferModeChange(transferMode[0].TransferMode);

        return (
            <SelectWithoutFormik
                //label="Select PaymentMode"
                defaultValue={defaultValue}
                name="TransferMode"
                onChange={(e) => {
                    let value = e.target.value;
                    OnPaymodeChange(value);
                }}
            >
                {transferMode.map((item, index) => (
                    <MenuItem key={index} value={item.TransferMode}>
                        {item.TransferMode}
                    </MenuItem>
                ))}
            </SelectWithoutFormik>
        );
    };

    const createRow = () => {
        let dummyRows = [];
        let data = request;
        dummyRows.push(DMTData('Beneficiary Name', data.BeniName));
        dummyRows.push(DMTData('Recipient IFSC', data.IFSC));
        dummyRows.push(DMTData('Recipient A/C Number', data.AccountNumber));
        dummyRows.push(
            DMTData('Payment Mode', handleTransferMode(transferModeList))
        );

        if (data.Total > 0) {
            dummyRows.push(
                DMTData('CCF / Comm ', data.Charge + ' / ' + data.Commission)
            );
            dummyRows.push(DMTData('TotalAmount(₹)', data.Total));
        }

        dummyRows.push(DMTData('Amount(₹)', data.Amount));
        if (data.Charge !== 0) {
            dummyRows.push(DMTData('Charge(₹)', data.Charge));
        }
        dummyRows.push(DMTData('GST(₹)', data.Gst));
        dummyRows.push(DMTData('Commission(₹)', data.Commission));
        dummyRows.push(DMTData('TDS(₹)', data.TDS));
        dummyRows.push(DMTData('TotalAmount(₹)', data.TotalAmount));

        setRows(dummyRows);
    };
    function DMTData(Parameter, Values) {
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

const DMTVerifyConfirmation = (request) => {
    let data = request.request;
    data.Amount = 1;

    const rows = [
        VerifyData('Name', data.BeniName),
        VerifyData('IFSC', data.IFSC),
        VerifyData('Account', data.AccountNumber),
        VerifyData('Amount (₹)', data.Amount),
        VerifyData('Word', NumberToWord(data.Amount))
    ];
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
