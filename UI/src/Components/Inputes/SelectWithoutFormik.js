import React from 'react';

import { Select, FormControl } from '@material-ui/core';
import InputLabel from '@material-ui/core/InputLabel';
import { VARIANT } from 'Config';

export const SelectWithoutFormik = ({
    onCustomCallback,
    label,
    name,
    ...props
}) => {
    return (
        <FormControl variant={VARIANT} fullWidth>
            <InputLabel id="demo-simple-select-outlined-label">
                {label}
            </InputLabel>

            <Select
                labelId="demo-simple-select-outlined-label"
                label={label}
                id={name}
                {...props}
            />
        </FormControl>
    );
};
