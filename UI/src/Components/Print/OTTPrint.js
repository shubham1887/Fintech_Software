import React, { useState, useEffect, forwardRef } from 'react';
import Logo from 'Components/Logo/Logo';
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

export function OTTPrint({
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
        <Grid container spacing={1}>
            <Grid item xs={12} sm={12} md={6} lg={6}>
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
                    <DialogTitle
                        id="alert-dialog-title"
                        className={classes.title}
                    >
                        {title}
                    </DialogTitle>
                    <DialogContent>
                        <DialogContentText id="DialogPrint">
                            <Confirmation request={request} />
                        </DialogContentText>
                    </DialogContent>
                    <DialogActions displayPrint="none">
                        <Grid container spacing={1}>
                            <Grid item xs={12} sm={12} md={6} lg={6}>
                                <Button
                                    onClick={handleNegativeButtonClick}
                                    className={
                                        classes.CustomSecondaryButtonStylePopup
                                    }
                                >
                                    {negativeButtonTitle
                                        ? negativeButtonTitle
                                        : 'No, Cancel Print'}
                                </Button>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <Button
                                    onClick={handlePositiveA4ButtonClick}
                                    className={
                                        classes.CustomPrimaryButtonStylePopup
                                    }
                                >
                                    {positiveButtonTitle
                                        ? positiveButtonTitle
                                        : 'A4'}
                                </Button>
                            </Grid>
                            <Grid item xs={12} sm={12} md={3} lg={3}>
                                <Button
                                    onClick={handlePositiveA5ButtonClick}
                                    className={
                                        classes.CustomPrimaryButtonStylePopup
                                    }
                                >
                                    {positiveButtonTitle
                                        ? positiveButtonTitle
                                        : 'A5'}
                                </Button>
                            </Grid>
                        </Grid>
                    </DialogActions>
                </Dialog>
            </Grid>
        </Grid>
    );
}

const Confirmation = (request) => {
    let data = request.request;
    const [cName, setCName] = useState('');
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    const classes = commonStyle();

    useEffect(() => {
        fetchCName();
    }, []);
    const rows = [
        UPIData(<Logo />, data.FirmName),
        UPIData('Type', data.Operator),
        UPIData('Date', data.DOC),

        UPIData('Number', data.ConsumerNumber),
        UPIData('Amount', data.Amount === '' ? '0' : data.Amount),
        UPIData('Status', data.Status),
        UPIData('Txn ID', data.ReferenceNo)
    ];
    function UPIData(Parameter, Values) {
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
            <h3 className={classes.document_h3}>Thank you for using {cName}</h3>
        </Grid>
    );
};
