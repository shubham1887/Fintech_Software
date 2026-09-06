import React, { useState } from 'react';
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
    DialogTitle
} from '@material-ui/core';
import { NumberToWord } from 'Components/WordConverter/CustomConverter';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { withStyles } from '@material-ui/core/styles';
export function UtilityViewConfirmation({
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
    const classes = commonStyle();

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
            <DialogTitle id="alert-dialog-title" className={classes.title}>
                {title}
            </DialogTitle>
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
        CustomerName,
        DueDate,
        Amount,
        Message,
        OperatorName,
        OperatorImagePath,
        IsBBPS
    } = request.request;

    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '') return true;
        if (param.toUpperCase() === 'data not found'.toUpperCase()) return true;
        if (param.toUpperCase() === 'Paramnotmatched'.toUpperCase())
            return true;

        return false;
    };

    const OperatorImage = (
        <img
            src={OperatorImagePath}
            height={100}
            width={100}
            alt={OperatorName}
        />
    );

    let Viewrows = [];

    Viewrows.push(ViewData(OperatorImage, OperatorName));
    if (!isBlank(CustomerName)) Viewrows.push(ViewData('Name', CustomerName));
    if (!isBlank(DueDate)) Viewrows.push(ViewData('Due Date', DueDate));
    if (!isBlank(Amount)) Viewrows.push(ViewData('Amount (₹)', Amount));

    if (!isBlank(Message)) Viewrows.push(ViewData('Message', Message));

    function ViewData(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <>
            <Grid item xs={12}>
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
                        </TableBody>
                    </Table>
                </TableContainer>
            </Grid>
        </>
    );
};
