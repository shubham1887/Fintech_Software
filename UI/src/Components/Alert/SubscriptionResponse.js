import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Box
} from '@material-ui/core';
import Tick from 'Assets/Images/BBPS/right.png';
import { CLIENT_NAME } from 'Config';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import CompanyLogo from 'Assets/Images/BBPS/Logo.png';
import CommonPrint from 'Components/Print/CommonPrint';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TableRow from '@material-ui/core/TableRow';
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

export function SubscriptionResponse({
           isOpen,
           title,
           message,
           request,
           positiveButtonClick,
           negativeButtonClick
       }) {
           const [open, setOpen] = useState(isOpen);
           const classes = commonStyle();

           useEffect(() => {
               setOpen(isOpen);
           }, [isOpen]);

           const handleClose = () => {
               setOpen(false);
               negativeButtonClick(false);
           };

           const handleNegativeButtonClick = () => {
               setOpen(false);
               negativeButtonClick();
           };

           const handlePositiveA4ButtonClick = () => {
               let pageSize = 'A4';
               CommonPrint(pageSize);
               positiveButtonClick(request);
           };

           return (
               <Dialog
                   open={open}
                   onClose={handleClose}
                   aria-labelledby="alert-dialog-title"
                   aria-describedby="alert-dialog-description"
                   className={classes.dialog}
               >
                   <DialogTitle
                       id="alert-dialog-title"
                       className={classes.title}
                   >
                       {title}
                   </DialogTitle>
                   <DialogContent>
                       <DialogContentText id="DialogPrint">
                           <SubscriptionConfirmation request={request} />
                       </DialogContentText>
                   </DialogContent>
                   <DialogActions className={classes.actions}>
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
                   </DialogActions>
               </Dialog>
           );
       }

const SubscriptionConfirmation = (request) => {
    let data = request.request;
    const [cName, setCName] = useState('');

    const TickImage = () => {
        return <img src={Tick} width={50} alt="Success" />;
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

    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }

    useEffect(() => {
        fetchCName();
    }, []);

    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '') return true;

        return false;
    };

    let rows = [];

    if (!isBlank(data.TotalAmount))
        rows.push(Data('Total Amount (₹):', data.TotalAmount));
    if (!isBlank(data.Amount)) rows.push(Data('Amount (₹):', data.Amount));
    if (!isBlank(data.GST)) rows.push(Data('GST (₹):', data.GST));
    if (!isBlank(data.PlanName)) rows.push(Data('PlanName :', data.PlanName));
    if (!isBlank(data.Message)) rows.push(Data('Message :', data.Message));

    function Data(Parameter, Values) {
        return { Parameter, Values };
    }

    return (
        <Grid item xs={12}>
            <TableContainer>
                <Table aria-label="customized table">
                    <TableRow>
                        <StyledTableCell align="left" colSpan={2}>
                            <span
                                style={{
                                    display: 'flex',
                                    flexDirection: 'row',
                                    justifyContent: 'space-around',
                                    gap: '5px',
                                    padding: '10px'
                                }}
                            >
                                <Grid
                                    style={{
                                        display: 'flex',
                                        justifyContent: 'space-around',
                                        flex: 'auto'
                                    }}
                                >
                                    {data.Status === 'Ok' && <TickImage />}
                                    <Logo />
                                </Grid>
                            </span>
                        </StyledTableCell>
                    </TableRow>
                    <StyledTableCell align="left" colSpan={2}>
                        <span
                            style={{
                                display: 'flex',
                                flexDirection: 'row',
                                justifyContent: 'space-between'
                            }}
                        >
                            <Grid style={{ textAlign: 'center', flex: 'auto' }}>
                                <div style={{ textAlign: 'center' }}>
                                    <div
                                        style={{
                                            fontWeight: 'bold',
                                            fontSize: 20
                                        }}
                                    >
                                        {data.IconName}
                                    </div>
                                    <div
                                        style={{
                                            fontWeight: 'bold',
                                            color: '#fff',
                                            background:
                                                data.Status === 'Ok'
                                                    ? '#42b86f'
                                                    : data.Status ===
                                                          'PROCESS' ||
                                                      data.Status === 'PENDING'
                                                    ? '#f58220'
                                                    : 'gray',
                                            fontSize: '15px',
                                            display: 'inline-block',
                                            padding: '4px 22px',
                                            borderRadius: '100px'
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
                                <StyledTableCell
                                    align="left"
                                    style={{
                                        fontWeight: 400,
                                        fontSize: '16px',
                                        paddingTop: '4px',
                                        paddingBottom: '4px'
                                    }}
                                >
                                    {row.Parameter}
                                </StyledTableCell>
                                <ValueTableCell
                                    align="left"
                                    style={{
                                        fontWeight: 500,
                                        fontSize: '16px',
                                        color: '#000',
                                        paddingTop: '4px',
                                        paddingBottom: '4px'
                                    }}
                                >
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
