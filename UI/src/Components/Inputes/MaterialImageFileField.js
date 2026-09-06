import React from 'react';
import { useField } from 'formik';
import { TextField, FormControl } from '@material-ui/core';

import { makeStyles } from '@material-ui/core/styles';
import { VARIANT } from 'Config';

const useStyles = makeStyles((theme) => ({
    TextFieldStyle: {
        marginTop: '4px',
        height: '40px'
    }
}));

export const MaterialImageFileField = ({ label, ...props }) => {
    const classes = useStyles();

    const [field, meta, helpers] = useField(props);

    field.onChange = (event) => {
        var file = event.target.files[0];
        var reader = new FileReader();
        helpers.setValue(file.name);
        reader.onload = function (item) {
            helpers.setValue(item.target.result);
        };

        reader.readAsDataURL(file);
    };

    return (
        <FormControl fullWidth>
            <TextField
                className={classes.TextFieldStyle}
                label={label}
                type="file"
                variant={VARIANT}
                error={meta.touched && meta.error ? true : false}
                helperText={meta.touched && meta.error ? meta.error : null}
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
