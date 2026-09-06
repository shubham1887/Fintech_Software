import React from 'react';
import { useField } from 'formik';
import { FormControl, TextareaAutosize } from '@material-ui/core';

import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles((theme) => ({
    TextFieldStyle: {
        marginTop: '4px',
        height: '40px'
    }
}));

export const MaterialTextArea = ({ label, ...props }) => {
    const classes = useStyles();

    const [field, meta, helpers] = useField(props);

    return (
        <FormControl fullWidth>
            <label>{label}</label>
            <TextareaAutosize
                rowsMin={props.rows}
                className={classes.TextFieldStyle}
                label={label}
                type={props.type}
                error={meta.touched && meta.error ? true : false}
                // helperText={meta.touched && meta.error ? meta.error : null}
                inputProps={{
                    maxLength: props.length ? props.length : null,
                    readOnly: props.readOnly ? props.readOnly : false
                }}
                {...field}
                {...props}
            />

            <label style={{ color: 'red' }}>
                {meta.touched && meta.error ? meta.error : null}
            </label>
        </FormControl>
    );
};
