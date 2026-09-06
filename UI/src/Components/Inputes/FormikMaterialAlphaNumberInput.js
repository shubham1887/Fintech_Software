import React, { memo } from 'react';
import { useField } from 'formik';
import { TextField, FormControl } from '@material-ui/core';
import { VARIANT } from 'Config';
import { isEqual } from 'lodash';
import TypedNumberLengthLabel from 'Components/Helper/TypedNumberLengthLabel';

const MaterialAlphaNumberFieldSub = ({ label, ...props }) => {
    const [field, meta, helpers] = useField(props);

    const style = {
        fontSize: props.font_size ? Number(props.font_size) : 20,
        textTransform: props.letter_case ? props.letter_case : 'none',
        numberTransform: props.number ? props.number : 'none'
    };

    const InputProps = {
        height: '60px',
        fontSize: '14px',
    };

    const numberOnlyPattern = /^[+-]?\d*\.?\d*$/; // Regex pattern to allow numbers (integer or decimal)
    field.onChange = (e) => {
        const { value } = e.target;

        if (props.number === 'number' && !numberOnlyPattern.test(value)) {
            return;
        }
        meta.value = value.replace(/[^\w\s-_/]/gi, '');
        helpers.setValue(value.replace(/[^\w\s-_/]/gi, ''));

        // const reg = /^-?[0-9]*(\.[0-9]*)?$/;

        // if (
        //     (!isNaN(value) && reg.test(value)) ||
        //     value === '' ||
        //     value === '-'
        // ) {
        //     meta.value = value;
        //     helpers.setValue(value);
        // }
    };

    return (
        <FormControl fullWidth>
            <TextField
                variant={VARIANT}
                fullWidth
                label={label}
                type={props.type}
                autoComplete="off"
                error={meta.touched && meta.error ? true : false}
                helperText={meta.touched && meta.error ? meta.error : null}
                inputProps={{
                    maxLength: props.length ? props.length : null,
                    readOnly: props.readOnly ? props.readOnly : false,
                    style: style,
                    autoComplete: 'new-password'
                }}
                // InputProps={{
                //     style: InputProps,
                // }}
                {...field}
                {...props}
            />
            {props.typed_length ? (
                <TypedNumberLengthLabel Mobile={meta.value} />
            ) : null}
        </FormControl>
    );
};

export const MaterialAlphaNumberField = memo(
    MaterialAlphaNumberFieldSub,
    isEqual
);
