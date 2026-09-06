import React, { memo } from 'react';
import { useField } from 'formik';
import { TextField, FormControl } from '@material-ui/core';
import { VARIANT } from 'Config';
import { checkEqual } from 'Components/Helper/CustomLoadash';

import TypedNumberLengthLabel from 'Components/Helper/TypedNumberLengthLabel';

const MaterialNumberFieldSub = ({ label, ...props }) => {
    const [field, meta, helpers] = useField(props);

    const FontStyle = {
        fontSize: props.font_size ? Number(props.font_size) : 20
    };

    field.onChange = (e) => {
        onChangeCore(e);
    };

    const onChangeCore = (e) => {
        const { value } = e.target;

        //meta.value = value;
        //helpers.setValue(value);

        const reg = /^-?[0-9]*(\.[0-9]*)?$/;

        if (
            (!isNaN(value) && reg.test(value)) ||
            value === '' ||
            value === '-'
        ) {
            meta.value = value;
            helpers.setValue(value);
        }
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
                    style: FontStyle,
                    autoComplete: 'new-password'
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

export const MaterialNumberField = memo(MaterialNumberFieldSub, checkEqual);
