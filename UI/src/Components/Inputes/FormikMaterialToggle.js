import React from 'react';
import { useField } from 'formik';
import { FormControl } from '@material-ui/core';
import ToggleButton from '@material-ui/lab/ToggleButton';
import ToggleButtonGroup from '@material-ui/lab/ToggleButtonGroup';

export const FormikMaterialToggle = ({
    onCustomCallback,
    options,
    label,
    ...props
}) => {
    const [field, meta, helpers] = useField(props);

    const [selectedValue, setSelectedValue] = React.useState('');

    field.onChange = (event, newFormats) => {
        setSelectedValue(newFormats);
        if (onCustomCallback === undefined) return;
        onCustomCallback(field.name, newFormats);
    };

    return (
        <FormControl>
            <ToggleButtonGroup
                size="small"
                value={selectedValue}
                {...field}
                {...props}
            >
                <ToggleButton value={'Allowed'}>Allowed</ToggleButton>
                <ToggleButton value={'STOP'}>STOP</ToggleButton>
                {/* {options &&
                    options.map(({ Key }) => (
                        <ToggleButton key={Key} value={Key}>
                            {Key}
                        </ToggleButton>
                    ))} */}
            </ToggleButtonGroup>
        </FormControl>
    );
};
