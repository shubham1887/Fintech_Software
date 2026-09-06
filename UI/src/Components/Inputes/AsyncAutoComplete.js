import React from 'react';
import { Autocomplete } from '@material-ui/lab';
import { TextField } from '@material-ui/core';
import { useField } from 'formik';
import { VARIANT } from 'Config';

const WAIT_INTERVAL = 500;
let timerID;

const AsyncAutoComplete = ({
    FieldName,
    SubFieldName,

    textFieldProps,
    onCustomCallback,
    onSearchCallback,
    ...props
}) => {
    const {
        form: { setTouched, setFieldValue },
        defaultValue
    } = props;

    const [field, meta] = useField(props);

    var errorMessage = '';
    const error = meta.error[FieldName];
    const touched = meta.touched;
    const isError = Boolean(touched && error);

    if (error !== undefined && SubFieldName !== undefined)
        errorMessage = error[SubFieldName];

    return (
        <Autocomplete
            {...props}
            name={props.name}
            value={defaultValue}
            onChange={(e, value) => {
                setFieldValue([FieldName], value);
                if (onCustomCallback === undefined) return;

                if (value === null) return;

                onCustomCallback(FieldName, value);
            }}
            onBlur={() => setTouched({ [FieldName]: true })}
            onInputChange={(e, value) => {
                if (onSearchCallback === undefined) return;

                clearTimeout(timerID);

                timerID = setTimeout(() => {
                    onSearchCallback(value);
                }, WAIT_INTERVAL);
            }}
            renderInput={(params) => (
                <TextField
                    variant={VARIANT}
                    {...textFieldProps}
                    {...params}
                    helperText={isError ? errorMessage : null}
                    error={isError ? true : false}
                />
            )}
        />
    );
};

export default AsyncAutoComplete;
