import React, { useState } from 'react';
import { Grid, IconButton, TextField } from '@material-ui/core';

import CheckCircleIcon from '@material-ui/icons/CheckCircle';
import DeleteForeverIcon from '@material-ui/icons/DeleteForever';

export const CustomSuccessFail = ({ value, callback }) => {
    const SuccessFailCallback = (TxnID, Pin, BtnTitle) => {
        callback(value, TxnID, Pin, BtnTitle);
    };

    return (
        <Grid item xs={12} md={12} lg={12} sm={12}>
            <SuccessFailComponent
                value={value}
                callback={(TxnID, Pin, BtnTitle) => {
                    SuccessFailCallback(TxnID, Pin, BtnTitle);
                }}
            />
        </Grid>
    );
};
const SuccessFailComponent = ({ value, callback }) => {
    const [TransactionID, setTransactionID] = useState('');
    const [Pin, setPin] = useState('');

    var isPin = localStorage.getItem('IsPin');

    return (
        <>
            <Grid item xs={12} md={12} lg={12} sm={12}>
                <TextField
                    variant="outlined"
                    margin="normal"
                    placeholder="TransactionID"
                    color="primary"
                    style={{ fontSize: '12px' }}
                    InputProps={{
                        style: {
                            height: '25px'
                        }
                    }}
                    value={TransactionID}
                    onChange={(event) => {
                        let newValue = event.target.value;
                        setTransactionID(newValue);
                    }}
                />
            </Grid>

            <Grid container>
                {isPin === 'true' && (
                    <Grid item xs={12} sm={12} md={6} lg={6}>
                        <TextField
                            variant="outlined"
                            placeholder="Pin"
                            color="primary"
                            type="password"
                            autoComplete="off"
                            inputProps={{
                                autoComplete: 'new-password'
                            }}
                            style={{ fontSize: '12px' }}
                            InputProps={{
                                style: {
                                    height: '25px'
                                }
                            }}
                            value={Pin}
                            onChange={(event) => {
                                var newValue = event.target.value;
                                setPin(newValue);
                            }}
                        />
                    </Grid>
                )}
                <Grid item xs={12} sm={12} md={3} lg={3}>
                    <IconButton
                        aria-label="REFUND"
                        title="REFUND"
                        style={{
                            color: 'red',
                            marginTop: '-10px'
                        }}
                        onClick={() => {
                            callback(TransactionID, Pin, 'REFUND');
                        }}
                    >
                        <DeleteForeverIcon style={{ color: 'red' }} />
                    </IconButton>
                </Grid>
                <Grid item xs={12} sm={12} md={3} lg={3}>
                    <IconButton
                        aria-label="SUCCESS"
                        title="SUCCESS"
                        style={{
                            color: 'green',
                            marginTop: '-10px'
                        }}
                        onClick={() => {
                            callback(TransactionID, Pin, 'SUCCESS');
                        }}
                    >
                        <CheckCircleIcon style={{ color: 'green' }} />
                    </IconButton>
                </Grid>
            </Grid>
        </>
    );
};
