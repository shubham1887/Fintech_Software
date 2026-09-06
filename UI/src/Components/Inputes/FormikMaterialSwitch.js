import React from 'react';
import { useField } from 'formik';
import { FormControlLabel, FormControl, Switch } from '@material-ui/core';

export const FormikMaterialSwitch = ({ onCustomCallback, label, ...props }) => {
    const [field, meta, helpers] = useField(props);

    const GetBooleanValue = (inputVal) => {
        return JSON.parse(inputVal);
    };

    field.onChange = (e) => {
        if (onCustomCallback === undefined) return;
        const { name, value } = e.target;
        var booleanValue = GetBooleanValue(value);
        helpers.setValue(!booleanValue);
        onCustomCallback(name, !booleanValue);
    };
    return (
        <FormControl>
            <FormControlLabel
                control={
                    <Switch
                        color="primary"
                        checked={field.value}
                        value={field.value ? 'Yes' : 'No'}
                        {...field}
                        {...props}
                    />
                }
                label={label}
            />
        </FormControl>
    );
};
