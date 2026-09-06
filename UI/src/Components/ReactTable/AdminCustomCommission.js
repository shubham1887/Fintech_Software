import React, { Fragment, useState } from 'react';
import { Grid, TextField } from '@material-ui/core';
import ToggleButton from '@material-ui/lab/ToggleButton';
import ToggleButtonGroup from '@material-ui/lab/ToggleButtonGroup';
import { makeStyles } from '@material-ui/core/styles';

const commonStyle = makeStyles((theme) => ({
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
    }
}));

const CommissionCategoryStyle = { fontSize: '12px', height: '18px' };
const CommissionTypeStyle = {
    fontSize: '12px',
    height: '18px',
    marginLeft: '8px'
};
const ToggleButtonStyle = { fontSize: '12px' };
export const AdminCustomCommission = ({ value, callback }) => {
    const classes = commonStyle();
    const { General } = value;

    const GeneralCallback = (callbackValue) => {
        callback({
            ...value,
            General: callbackValue
        });
    };

    return (
        <Grid item xs={12} md={12} lg={12} sm={12}>
            <div className={classes.box_general}>
                <CommissionComponent
                    value={General}
                    callback={(values) => {
                        GeneralCallback(values);
                    }}
                    displayName="General"
                />
            </div>
        </Grid>
    );
};
const CommissionComponent = ({ value, callback, displayName }) => {
    const classes = commonStyle();

    const { Commission, IsFix, CommissionCategory } = value;

    const [Fix, setFix] = useState(IsFix === true ? 'Fix' : 'Flexi');

    const [Surcharge, setSurcharge] = useState(
        CommissionCategory === 'Surcharge' ? 'SUR' : 'COM'
    );
    const handleFixClick = (event, newValue) => {
        setFix(newValue);
        let booleanVal = newValue === 'Fix' ? true : false;
        callback({
            ...value,
            IsFix: booleanVal,
            CommissionType: newValue
        });
    };
    const handleSurchargeClick = (event, newValue) => {
        setSurcharge(newValue);
        let booleanVal = newValue === 'SUR' ? true : false;
        callback({
            ...value,
            IsSurcharge: booleanVal,
            CommissionCategory: booleanVal ? 'Surcharge' : 'Commission'
        });
    };
    const handleCommissionChange = (event) => {
        let newValue = event.target.value;

        callback({
            ...value,
            Commission: newValue
        });
    };
    let commissionInputProps = {
        maxLength: 5,
        inputMode: 'numeric',
        pattern: '[0-9]*'
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
                inputProps={commissionInputProps}
                onChange={handleCommissionChange}
            />
            <br />
            <ToggleButtonGroup
                exclusive
                size="small"
                value={Surcharge}
                onChange={handleSurchargeClick}
                style={CommissionCategoryStyle}
            >
                <ToggleButton value={'COM'} style={ToggleButtonStyle}>
                    COM
                </ToggleButton>
                <ToggleButton value={'SUR'} style={ToggleButtonStyle}>
                    SUR
                </ToggleButton>
            </ToggleButtonGroup>
            <ToggleButtonGroup
                exclusive
                size="small"
                value={Fix}
                onChange={handleFixClick}
                style={CommissionTypeStyle}
            >
                <ToggleButton value={'Flexi'} style={ToggleButtonStyle}>
                    PER
                </ToggleButton>
                <ToggleButton value={'Fix'} style={ToggleButtonStyle}>
                    FIX
                </ToggleButton>
            </ToggleButtonGroup>
            <br />
        </Fragment>
    );
};
