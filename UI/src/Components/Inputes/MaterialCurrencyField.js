import React, { useCallback, useRef } from 'react';
import { useField } from 'formik';
import { TextField, FormControl } from '@material-ui/core';
import { debounce } from 'lodash';
import NumberFormat from 'react-number-format';

import { makeStyles } from '@material-ui/core/styles';
import { VARIANT } from 'Config';

const useStyles = makeStyles((theme) => ({
    TextFieldStyle: {
        marginTop: '0px'
    }
}));

function NumberFormatCustom(props) {
    const { inputRef, onChange, onCustomCallback, ...other } = props;
    const valueRef = useRef('');

    const debouncedOnChange = useCallback(
        debounce(() => {
            onChange({
                target: {
                    name: props.name,
                    value: valueRef.current
                }
            });
            if (onCustomCallback === undefined) {
                return;
            }
            onCustomCallback(valueRef.current);
        }, 170), // Adjust the debounce delay as needed
        [onChange, onCustomCallback, props.name]
    );

    const handleValueChange = useCallback(
        (values) => {
            valueRef.current = values.value;
            debouncedOnChange();
        },
        [debouncedOnChange]
    );

    return (
        <NumberFormat
            {...other}
            getInputRef={inputRef}
            onValueChange={handleValueChange}
            thousandSeparator
            thousandsGroupStyle="lakh"
            isNumericString
            isAllowed={(values) => {
                const { formattedValue, floatValue } = values;
                return formattedValue === '' || floatValue <= props.max;
            }}
        />
    );
}

const MaterialCurrency = ({ onCustomCallback, ...props }) => {
    const classes = useStyles();
    const [field, meta] = useField(props);
    const FontStyle = {
        fontSize: props.font_size ? Number(props.font_size) : 30,
        textAlign: 'right',
        //fontWeight: 'bold'
    };

    return (
        <FormControl fullWidth>
            <TextField
                className={classes.TextFieldStyle}
                variant={VARIANT}
                InputProps={{
                    inputComponent: NumberFormatCustom
                }}
                error={meta.touched && meta.error ? true : false}
                helperText={meta.touched && meta.error ? meta.error : null}
                inputProps={{
                    style: FontStyle,
                    max: props.max ? props.max : 999999,
                    readOnly: props.readOnly ? props.readOnly : false,
                    onCustomCallback: onCustomCallback
                }}
                {...field}
                {...props}
            />
        </FormControl>
    );
};

//const MaterialCurrency = MaterialCurrencyCore;
export { MaterialCurrency };
