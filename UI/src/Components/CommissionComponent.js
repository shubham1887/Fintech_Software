import React, { Fragment, useState } from 'react';
import { TextField } from '@material-ui/core';
import ToggleButton from '@material-ui/lab/ToggleButton';
import ToggleButtonGroup from '@material-ui/lab/ToggleButtonGroup';
import { makeStyles } from '@material-ui/core/styles';
const commonStyleCustom = makeStyles(() => ({
    textField: {
        height: '20px',
        width: '80px',
        marginBottom: '2px',
        marginTop: '-15px',
        marginLeft: '10px'
    },
    font_size: {
        fontSize: '12px'
    },
    box_general: {
        margin: '4px',
        padding: '4px 8px',
        backgroundColor: 'rgb(255, 255, 255)',
        boxShadow:
            'rgba(0, 0, 0, 0.2) 0px 2px 1px -1px, rgba(0, 0, 0, 0.14) 0px 1px 1px 0px, rgba(0, 0, 0, 0.12) 0px 1px 3px 0px'
    },
    box_real: {
        margin: '4px',
        padding: '4px 8px',
        backgroundColor: 'rgb(255, 255, 255)',
        border: '1px solid rgb(158, 158, 158)',
        boxShadow:
            'rgba(0, 0, 0, 0.2) 0px 2px 1px -1px, rgba(0, 0, 0, 0.14) 0px 1px 1px 0px, rgba(0, 0, 0, 0.12) 0px 1px 3px 0px'
    },
    toggle_left: {
        fontSize: '12px',
        height: '18px'
    },
    toggle_right: {
        fontSize: '12px',
        height: '18px',
        marginLeft: '8px'
    }
}));

export const ComissionComponent = ({ callback, displayName, value }) => {
    const classes = commonStyleCustom();

    const [CommissionType, setCommissionType] = useState(
        value !== undefined ? value.CommissionType : 'Fix'
    );

    const [Commission, setCommission] = useState(
        value !== undefined ? value.Commission : 0.0
    );
    const [CommissionCategory, setCommissionCategory] = useState(
        value !== undefined ? value.CommissionCategory : 'Commission'
    );

    const handleFixClick = (event, newValue) => {
        setCommissionType(newValue);
        callback(newValue, Commission, CommissionCategory);
    };
    const handleSurchargeClick = (event, newValue) => {
        setCommissionCategory(newValue);
        callback(CommissionType, Commission, newValue);
    };
    return (
        <Fragment>
            <label>
                <b>{displayName}</b>
            </label>
            <TextField
                variant="outlined"
                margin="normal"
                color="primary"
                className={classes.font_size}
                InputProps={{
                    className: classes.textField
                }}
                value={Commission}
                inputProps={{
                    maxLength: 5,
                    inputMode: 'numeric'
                }}
                onChange={(event) => {
                    var newValue = event.target.value;
                    setCommission(newValue);
                    callback(CommissionType, newValue, CommissionCategory);
                }}
            />
            <br />
            <ToggleButtonGroup
                exclusive
                size="small"
                value={CommissionCategory}
                onChange={handleSurchargeClick}
                style={{ fontSize: '12px', height: '18px' }}
            >
                <ToggleButton value={'Commission'} style={{ fontSize: '12px' }}>
                    COM
                </ToggleButton>
                <ToggleButton value={'Surcharge'} style={{ fontSize: '12px' }}>
                    SUR
                </ToggleButton>
            </ToggleButtonGroup>
            <ToggleButtonGroup
                exclusive
                size="small"
                value={CommissionType}
                onChange={handleFixClick}
                style={{ fontSize: '12px', height: '18px', marginLeft: '8px' }}
            >
                <ToggleButton value={'Flexi'} style={{ fontSize: '12px' }}>
                    PER
                </ToggleButton>
                <ToggleButton value={'Fix'} style={{ fontSize: '12px' }}>
                    FIX
                </ToggleButton>
            </ToggleButtonGroup>
            <br />
        </Fragment>
    );
};
