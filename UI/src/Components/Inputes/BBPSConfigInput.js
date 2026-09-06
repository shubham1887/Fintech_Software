import React, { useState, useEffect } from 'react';
import { MaterialNumberField } from 'Components/Inputes/FormikMaterialNumberInput';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';
import { AutoCompleteTextView } from 'Components/Inputes/AutoCompleteTextView';
import { Field } from 'formik';
const BBPSConfigInput = (props) => {
    const [list, setList] = useState([]);

    useEffect(() => {
        if (
            props.list === '' ||
            props.list === '[]' ||
            props.list === undefined ||
            props.list === null ||
            props.list.length <= 0 ||
            props.list === []
        )
            return;

        try {
            setList(JSON.parse(props.list));
        } catch (error) {}
    }, [props.list]);

    if (props.type === 'List') {
        return (
            <Field
                component={AutoCompleteTextView}
                FieldName={props.name}
                name={props.name}
                options={list}
                getOptionLabel={({ key, value }) =>
                    key.trim() === value.trim()
                        ? key.trim()
                        : key.trim() + ' [' + value.trim() + ']'
                }
                textFieldProps={{
                    label: props.label
                }}
            />
        );
    }
    if (props.type === 'Number') {
        return (
            <MaterialNumberField
                // type="text"
                label={props.label}
                name={props.name}
                icon={props.icon}
                placeholder={props.placeholder}
                title={props.title}
                length={10}
                typed_length={1}
            />
        );
    }
    const isDisabled = (IsDisplay) => {
        if (IsDisplay === false) {
            return { display: 'none' };
        }
        return { display: '' };
    };

    return (
        <MaterialTextField
            type={'text'}
            label={props.label}
            name={props.name}
            icon={props.icon}
            placeholder={props.placeholder}
            title={props.title}
            style={isDisabled(props.IsDisplay)}
        />
    );
};

export default BBPSConfigInput;
