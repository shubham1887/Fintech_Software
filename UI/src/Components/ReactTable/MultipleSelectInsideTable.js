import React, { useState } from 'react';
import Autocomplete from '@material-ui/lab/Autocomplete';
import { Select, MenuItem, TextField } from '@material-ui/core';

export const MultipleSelectInsideTable = ({
    list,
    value,
    callback,
    placeholder,
    label
}) => {
    if (value === undefined || value === '') value = '0';

    const [selectedValues, setSelectedValues] = useState([]);

    const callbackfunc = (selectedOption) => {
        setSelectedValues(selectedOption);
        callback(selectedValues);
    };

    return (
        <Autocomplete
            variant="outlined"
            margin="dense"
            multiple
            options={list}
            getOptionLabel={(option) => option.Type}
            renderInput={(params) => (
                <TextField
                    {...params}
                    variant="outlined"
                    margin="dense"
                    label={label}
                    placeholder={placeholder}
                />
            )}
        />
    );
};
