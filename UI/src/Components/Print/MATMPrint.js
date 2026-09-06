import React, { useState, useEffect, forwardRef } from 'react';
import ReceiptLogo from 'Components/Logo/ReceiptLogo';
import { Grid } from '@material-ui/core';
import TableCell from '@material-ui/core/TableCell';
import TableRow from '@material-ui/core/TableRow';

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

export function MATMPrint({
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
                    <MATMConfirmation request={request} />
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

const MATMConfirmation = (request) => {
    let data = request.request;

    const [datarows, setRows] = useState([]);
    const [cName, setCName] = useState('');
    async function fetchCName() {
        setCName(await CLIENT_NAME());
    }
    let transactions = data.ministatements;
    let AvailableBalance = data.AEPSResponseJSON;

    if (data.Operator === 'Mini Statement') {
        var Ministatementdata = JSON.parse(data.AEPSResponseJSON);
        AvailableBalance = Ministatementdata.Balance;
        transactions = Ministatementdata.ministatements;
        transactions = transactions.slice(0, 5);
    }

    useEffect(() => {
        createRow();
        fetchCName();
    }, []);
    const classes = commonStyle();
    const OperatorImage = (
        <img
            src={data.OperatorImagePath}
            height={50}
            width={50}
            alt={data.Operator}
        />
    );

    const isBlank = (param) => {
        if (param === undefined) return true;
        if (param === null) return true;
        if (param === '0') return true;
        if (param === '') return true;
        return false;
    };

    const createRow = () => {
        let dummyRows = [];
        dummyRows.push(AEPSData('Service: ', 'M-ATM'));
        dummyRows.push(AEPSData('Date: ', data.DOC));
        dummyRows.push(AEPSData('Type: ', data.Operator));
        dummyRows.push(AEPSData('Number: ', data.ConsumerNumber));
        dummyRows.push(AEPSData('CustomerNumber: ', data.CustomerNumber));
        dummyRows.push(AEPSData('Status: ', data.Status));
        dummyRows.push(AEPSData('Aval. Balance: ', AvailableBalance));
        dummyRows.push(AEPSData('Txn ID: ', data.ReferenceNo));

        if (!isBlank(data.Amount)) {
            dummyRows.push(AEPSData('Amount: ', data.Amount));
        }
        setRows(dummyRows);
    };

    function AEPSData(Parameter, Values) {
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
                        width: '340px',
                        height: '500px',
                        position: 'relative'
                    }}
                >
                    <p style={{ textAlign: 'center', marginBottom: '10px' }}>
                        <ReceiptLogo />
                    </p>

                    <Box
                        style={{
                            marginTop: '8px',
                            color: '#000',
                            borderBottom: '1px solid #000',
                            fontSize: '14px'
                        }}
                    >
                        <span style={{ fontWeight: 'bold' }}>
                            {data.FirmName}
                        </span>
                    </Box>
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
                    transactions.length <= 0
                        ? null
                        : transactions.map((trans, index) => (
                              <Box>
                                  <TableRow>
                                      <StyledTableCell
                                          align="left"
                                          style={{
                                              paddingTop: '5px',
                                              paddingRight: '0px',
                                              paddingBottom: '0px',
                                              paddingLeft: '0px',
                                              width: '60%'
                                          }}
                                      >
                                          <h6
                                              style={{
                                                  fontSize: '12px',
                                                  fontWeight: 'normal',
                                                  marginBottom: '0px',
                                                  color: '#000'
                                              }}
                                          >
                                              Narration :
                                              <span
                                                  style={{
                                                      fontSize: '14px',
                                                      color: '#000'
                                                  }}
                                              >
                                                  {trans.Narration}
                                              </span>
                                          </h6>
                                          <p
                                              style={{
                                                  fontSize: '12px',
                                                  marginBottom: '0px',
                                                  color: '#000'
                                              }}
                                          >
                                              Rs. :
                                              <span
                                                  style={{
                                                      fontSize: '14px',
                                                      fontWeight: 'bold',
                                                      color: '#000'
                                                  }}
                                              >
                                                  {trans.Amount}
                                              </span>
                                          </p>
                                      </StyledTableCell>
                                      <StyledTableCell
                                          align="left"
                                          style={{
                                              paddingTop: '5px',
                                              paddingRight: '0px',
                                              paddingBottom: '0px',
                                              paddingLeft: '0px',
                                              width: '50%'
                                          }}
                                      >
                                          <h6
                                              style={{
                                                  fontSize: '12px',
                                                  fontWeight: 'normal',
                                                  marginBottom: '0px',
                                                  color: '#000'
                                              }}
                                          >
                                              Date :
                                              <span
                                                  style={{
                                                      fontSize: '14px',
                                                      color: '#000'
                                                  }}
                                              >
                                                  {trans.Date}
                                              </span>
                                          </h6>
                                          <p
                                              style={{
                                                  fontSize: '12px',
                                                  marginBottom: '0px',
                                                  color: '#000'
                                              }}
                                          >
                                              TxnType :
                                              <span
                                                  style={{
                                                      fontSize: '14px',
                                                      fontWeight: 'bold',
                                                      color: '#000'
                                                  }}
                                              >
                                                  {trans.TxnType}
                                              </span>
                                          </p>
                                      </StyledTableCell>
                                  </TableRow>
                              </Box>
                          ))}

                    <Box>
                        <p
                            style={{
                                padding: '3px 0px',
                                margin: '0px',
                                backgroundColor: '#ccc',
                                color: '#000',
                                fontSize: '8px',
                                textAlign: 'center',
                                position: 'absolute',
                                bottom: '5px',
                                width: '90%'
                            }}
                        >
                            This is system generated receipt. Hence no seal or
                            signature required
                            <br />
                            Thank you for using {cName}
                        </p>
                    </Box>
                </Box>
            </Grid>
        </Grid>
    );
};
