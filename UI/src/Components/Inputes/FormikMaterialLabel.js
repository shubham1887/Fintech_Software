import React from 'react';
import { useField } from 'formik';
import { FormControl, InputLabel } from '@material-ui/core';

import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles((theme) => ({
    FieldStyle: {
        marginTop: '0px',
        height: '40px'
    }
}));

export const MaterialLabel = ({ label, ...props }) => {
    const classes = useStyles();
    const [field, meta] = useField(props);

    return (
        <FormControl fullWidth>
            <InputLabel
                className={classes.FieldStyle}
                label={label}
                {...field}
                {...props}
            />
        </FormControl>
    );
};
