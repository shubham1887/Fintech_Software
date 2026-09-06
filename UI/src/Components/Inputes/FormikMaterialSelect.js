import React from 'react';
import { useField } from 'formik';
import { Select, FormControl, FormHelperText } from '@material-ui/core';
import InputLabel from '@material-ui/core/InputLabel';
import { VARIANT } from 'Config';

export const MaterialSelect = ({ onCustomCallback, label, ...props }) => {
    const [field, meta, helpers] = useField(props);

    field.onChange = (e) => {
        meta.value = e.target.value;
        helpers.setValue(e.target.value);

        if (onCustomCallback === undefined) {
            return;
        }

        onCustomCallback(e.target.name, e.target.value);
    };

    return (
        <FormControl
            variant={VARIANT}
            fullWidth
            error={meta.touched && meta.error}
        >
            <InputLabel id="demo-simple-select-outlined-label">
                {label}
            </InputLabel>

            <Select
                labelId="demo-simple-select-outlined-label"
                label={label}
                {...props}
                id={field.name}
                //                className="mb-2 w-full "
                error={meta.touched && meta.error ? true : false}
                helpertext={meta.touched && meta.error ? meta.error : null}
                {...field}
            />
            <FormHelperText>
                {meta.touched && meta.error ? meta.error : null}
            </FormHelperText>
        </FormControl>
    );
};
