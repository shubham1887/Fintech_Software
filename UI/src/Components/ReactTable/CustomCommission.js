import React, { Fragment, useState, memo, useEffect } from 'react';
import { Grid, TextField, Box } from '@material-ui/core';
import { ToggleButton, ToggleButtonGroup } from '@material-ui/lab';
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

export const CustomCommission = memo(
    ({ value, callback, disabled, RealCommission }) => {
        const classes = commonStyleCustom();
        const { General, Real } = value;
        const [realDisplay, setRealDisplay] = useState('block');

        useEffect(() => {
            if (RealCommission === undefined) {
                setRealDisplay('block');
                return;
            }

            if (RealCommission === false) {
                setRealDisplay('none');
                return;
            }
        }, [RealCommission]);

        const GeneralCallback = (callbackValue) => {
            callback({
                ...value,
                General: callbackValue
            });
        };

        const RealCallback = (callbackValue) => {
            callback({
                ...value,
                Real: callbackValue
            });
        };

        return (
            <React.Fragment>
                <Grid item xs={12} md={12} lg={12} sm={12}>
                    <div className={classes.box_general}>
                        <CommissionComponent
                            value={General}
                            disabled={disabled}
                            callback={GeneralCallback}
                            displayName="General"
                        />
                    </div>
                </Grid>
                <Box display={realDisplay}>
                    <Grid item xs={12} md={12} lg={12} sm={12}>
                        <div className={classes.box_real}>
                            <CommissionComponent
                                value={Real}
                                disabled={disabled}
                                callback={RealCallback}
                                displayName="Real"
                            />
                        </div>
                    </Grid>
                </Box>
            </React.Fragment>
        );
    }
);

const CommissionComponent = ({ value, callback, displayName, disabled }) => {
    const classes = commonStyleCustom();

    const { Commission, IsFix, CommissionCategory } =
        value !== undefined
            ? value
            : {
                  Commission: 0,
                  IsFix: true,
                  CommissionCategory: 'Commission'
              };

    const [Fix, setFix] = useState(IsFix === true ? 'Fix' : 'Flexi');

    const [Surcharge, setSurcharge] = useState(
        CommissionCategory === 'Surcharge' ? 'SUR' : 'COM'
    );

    const handleFixClick = (event, newValue) => {
        setFix(newValue);
        var booleanVal = newValue === 'Fix' ? true : false;

        callback({
            ...value,
            IsFix: booleanVal,
            CommissionType: newValue
        });
    };
    const handleSurchargeClick = (event, newValue) => {
        setSurcharge(newValue);
        var booleanVal = newValue === 'SUR' ? true : false;

        callback({
            ...value,
            IsSurcharge: booleanVal,
            CommissionCategory: booleanVal ? 'Surcharge' : 'Commission'
        });
    };

    const handleValueChange = (event) => {
        var newValue = event.target.value;

        callback({
            ...value,
            Commission: newValue
        });
    };

    const get_Commission = (Commission) => {
        if (Commission === undefined) return 0;

        if (Commission === null) return 0;

        if (Commission.length <= 0) return 0;

        if (typeof Commission === 'number') {
            if (Commission.countDecimals() <= 2) {
                return Commission.toFixed(2);
            }
        }

        return Commission;
    };

    return (
        <Fragment>
            <label>
                <b>{displayName}</b>
            </label>
            <TextField
                disabled={disabled}
                variant="outlined"
                margin="normal"
                color="primary"
                className={classes.font_size}
                InputProps={{
                    className: classes.textField
                }}
                value={get_Commission(Commission)}
                inputProps={{
                    maxLength: 5,
                    inputMode: 'numeric',
                    pattern: '[0-9]*'
                }}
                onChange={handleValueChange}
            />
            <br />
            <ToggleButtonGroup
                exclusive
                size="small"
                value={Surcharge}
                onChange={handleSurchargeClick}
                className={classes.toggle_left}
            >
                <ToggleButton
                    value={'COM'}
                    disabled={disabled}
                    className={classes.font_size}
                >
                    COM
                </ToggleButton>
                <ToggleButton
                    value={'SUR'}
                    disabled={disabled}
                    className={classes.font_size}
                >
                    SUR
                </ToggleButton>
            </ToggleButtonGroup>
            <ToggleButtonGroup
                exclusive
                size="small"
                value={Fix}
                onChange={handleFixClick}
                className={classes.toggle_right}
            >
                <ToggleButton
                    value={'Flexi'}
                    disabled={disabled}
                    className={classes.font_size}
                >
                    PER
                </ToggleButton>
                <ToggleButton
                    value={'Fix'}
                    disabled={disabled}
                    className={classes.font_size}
                >
                    FIX
                </ToggleButton>
            </ToggleButtonGroup>
            <br />
        </Fragment>
    );
};
