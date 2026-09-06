import React from 'react';
import { FormControlLabel, FormControl, Switch } from '@material-ui/core';

export const NormalMaterialSwitch = ({
    onCustomCallback,
    label,
    value,
    ...props
}) => {
    const GetBooleanValue = (inputVal) => {
        if (inputVal === 'Yes' || inputVal === true) {
            return true;
        }
        return false;
    };

    const handleChange = (e) => {
        if (onCustomCallback === undefined) return;
        const { value } = e.target;

        var booleanValue = GetBooleanValue(value);
        onCustomCallback(!booleanValue);
    };
    return (
        <FormControl>
            <FormControlLabel
                control={
                    <Switch
                        color="primary"
                        checked={value}
                        value={value ? 'Yes' : 'No'}
                        onChange={handleChange}
                        {...props}
                    />
                }
                label={label}
            />
        </FormControl>
    );
};
