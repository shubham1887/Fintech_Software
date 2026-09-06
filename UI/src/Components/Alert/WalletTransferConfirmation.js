
import React, { useState } from 'react';
import { Grid, Typography } from '@material-ui/core';
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
    DialogTitle
} from '@material-ui/core';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { withStyles } from '@material-ui/core/styles';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import { border } from '@material-ui/system';
export function WalletTransferConfirmation({
    isOpen,
    title,
    message,
    request,
    positiveButtonTitle,
    positiveButtonClick,
    PrintButtonClick,
    negativeButtonClick
}) {
    const [open, setOpen] = useState(isOpen);
    const classes = commonStyle();
    const handlePrintButtonClick = () => {
        setOpen(false);
        PrintButtonClick();
    };
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
            open={open}
            onClose={() => {
                setOpen(false);
                negativeButtonClick(false);
            }}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">{title}</DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                    <RechargeConfirmation request={request} />
                    <p> {message}</p>
                </DialogContentText>
            </DialogContent>
            <DialogActions>

                <Button
                    onClick={handlePositiveButtonClick}
                    className={classes.CustomPrimaryButtonStyle}
                >
                    {positiveButtonTitle ? positiveButtonTitle : 'Yes'}
                </Button>


            </DialogActions>
        </Dialog>
    );
}

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

const RechargeConfirmation = (request) => {
    const {
        mainWalletBal,
        AepsWalletBal,
        ReferenceID,
        Amount,
        Remark,
        DateTime,
        OperatorName,
    } = request.request;

    const OperatorImage = (
        <img
            src={CompanyLogo}
            height={80}
            width={150}
            alt={OperatorName}
        />
    );
    const Viewrows = [
        ViewData('Updated e-balance', `₹` + mainWalletBal),
        ViewData('Updated My Settlement', `₹` + AepsWalletBal),

    ];
    function ViewData(Parameter, Values) {
        return { Parameter, Values };
    }
    const Updatedata = [
        UpdateData('Amount', `₹` + Amount),
        UpdateData('Date & Time', DateTime),
        UpdateData('Reference ID', ReferenceID),
        UpdateData('Remark', Remark),

    ];
    function UpdateData(Parameter, Values) {
        return { Parameter, Values };
    }
    return (
        <>
            <Grid item xs={12}>
                <div style={{ textAlign: 'center' }}>
                    <div style={{
                        fontWeight: 'bold',
                        fontSize: 10,
                        display: 'block',
                        padding: '4px 22px',
                    }}>
                        {OperatorImage}
                    </div>
                    <div
                        style={{
                            fontWeight: 'bold',
                            color: '#fff',
                            background: '#42b86f',
                            fontSize: '15px',
                            display: 'inline-block',
                            padding: '4px 22px',
                            borderRadius: '100px',
                        }}
                    >
                        e-Balance Transfer Successful
                    </div>
                </div>
                <TableContainer>
                    <Table aria-label="customized table">
                        <TableBody>
                            {Viewrows.map((row) => (
                                <TableRow key={row.Parameter}>
                                    <StyledTableCell align="left">
                                        {row.Parameter}
                                    </StyledTableCell>
                                    <ValueTableCell align="left">
                                        {row.Values}
                                    </ValueTableCell>
                                </TableRow>
                            ))}
                            <StyledTableCell >
                                Transfer Details :-
                            </StyledTableCell>
                            {Updatedata.map((row) => (

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
