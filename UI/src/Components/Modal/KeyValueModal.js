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
    DialogTitle
} from '@material-ui/core';
import { withStyles } from '@material-ui/core/styles';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

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
        color: 'black'
    }
}))(TableCell);

export function KeyValueModal({
    isOpen,
    title,
    request,
    status,
    OKButtonTitle,
    OKButtonClick
}) {
    const [open, setOpen] = useState(isOpen);

    const classes = commonStyle();
    const handleOKButtonClick = () => {
        setOpen(false);
        OKButtonClick();
    };

    const [bgColor, setBGColor] = useState('');
    useEffect(() => {
                        if (status === 'SUCCESS') {
                            setBGColor('#42b86f');
                            return;
                        }

                        // if (status === 'FAILURE' || status === 'REFUND') {
                        //     setBGColor('#ff0000');
                        //     return;
                        // }

                        // if (status === 'PROCESS' || status === 'PENDING') {
                        //     setBGColor('#f58220');
                        //     return;
                        // }
                    }, []);

    if (isOpen === undefined || isOpen === null || isOpen === false) {
        return null;
    }

    return (
        <Dialog
            fullWidth
            maxWidth="sm"
            open={open}
            onClose={handleOKButtonClick}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle
                id="alert-dialog-title"
                className={classes.title}
                style={{ background: bgColor }}
            >
                {title} {':'} {status}
            </DialogTitle>
            <DialogContent>
                <DialogContentText>
                    <Grid item xs={12}>
                        <TableContainer>
                            <Table aria-label="customized table">
                                <TableBody>
                                    {request.map((row) => (
                                        <TableRow key={row.Key}>
                                            <StyledTableCell align="left">
                                                {row.Key}
                                            </StyledTableCell>
                                            <ValueTableCell align="left">
                                                {row.Value}
                                            </ValueTableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </Grid>
                </DialogContentText>
            </DialogContent>
            <DialogActions displayPrint="none">
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={6} lg={6}></Grid>
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <Button
                            onClick={() => handleOKButtonClick()}
                            className={classes.CustomSecondaryButtonStylePopup}
                        >
                            {OKButtonTitle ? OKButtonTitle : 'Ok'}
                        </Button>
                    </Grid>
                </Grid>
            </DialogActions>
        </Dialog>
    );
}
