import React from 'react';
import { useField } from 'formik';
import { TextField, FormControl, InputBase } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import { VARIANT } from 'Config';
import TypedNumberLengthLabel from 'Components/Helper/TypedNumberLengthLabel';

const useStyles = makeStyles((theme) => ({
    TextFieldStyle: {
        marginTop: '0px'
    }
}));

export const MaterialTextField = ({ label, ...props }) => {
    const [field, meta, helpers] = useField(props);

    return (
        <FormControl fullWidth>
            <TextField
                variant={VARIANT}
                fullWidth
                // className={classes.TextFieldStyle}
                label={label}
                type={props.type}
                error={meta.touched && meta.error ? true : false}
                helperText={meta.touched && meta.error ? meta.error : null}
                inputProps={{
                    maxLength: props.length ? props.length : null,
                    readOnly: props.readOnly ? props.readOnly : false,
                    autoComplete: props.autoComplete ? props.autoComplete : null
                }}
                {...field}
                {...props}
            />
            {props.typed_length ? (
                <TypedNumberLengthLabel Mobile={meta.value} />
            ) : null}
        </FormControl>
    );
};
export const MaterialTextAreaField = ({ label, ...props }) => {
    const classes = useStyles();
    const [field, meta, helpers] = useField(props);

    return (
        <FormControl fullWidth>
            <TextField
                variant={VARIANT}
                className={classes.TextFieldStyle}
                label={label}
                type={props.type}
                multiline={true}
                rowsMax={4}
                error={meta.touched && meta.error ? true : false}
                helperText={meta.touched && meta.error ? meta.error : null}
                inputProps={{
                    maxLength: props.length ? props.length : null,
                    readOnly: props.readOnly ? props.readOnly : false
                }}
                {...field}
                {...props}
            />
            {props.typed_length ? (
                <TypedNumberLengthLabel Mobile={meta.value} />
            ) : null}
        </FormControl>
    );
};
export const AmountInWordLabel = ({ label, ...props }) => {
    const classes = useStyles();
    const [field, meta, helpers] = useField(props);

    return (
        <FormControl fullWidth>
            <InputBase
                variant={VARIANT}
                className={classes.TextFieldStyle}
                style={{ color: 'green' }}
                label={label}
                type={props.type}
                error={meta.touched && meta.error ? true : false}
                inputProps={{
                    maxLength: props.length ? props.length : null,
                    readOnly: props.readOnly ? props.readOnly : false
                }}
                {...field}
                {...props}
            />
        </FormControl>
    );
};
