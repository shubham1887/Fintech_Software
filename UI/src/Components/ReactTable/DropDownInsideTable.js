import React, { memo } from 'react';
import { getClassName } from 'Components/utils';

import { Select, MenuItem } from '@material-ui/core';

const DropDownInsideTable = ({ list, value, callback }) => {
    if (value === undefined || value === '') value = '0';
    let className = getClassName(value);
    const callbackfunc = (selectedOption) => {
        callback(selectedOption);
    };
    return (
        <Select
            variant="outlined"
            margin="dense"
            className={className}
            fullWidth
            defaultValue={value}
            onChange={(event) => {
                callbackfunc(event.target.value);
            }}
        >
            <MenuItem key={'0'} value="0">
                Select Option
            </MenuItem>
            {list !== undefined
                ? list.map(({ ID, Type }) => (
                      <MenuItem key={ID} value={ID}>
                          {Type}
                      </MenuItem>
                  ))
                : null}
        </Select>
    );
};

export default memo(DropDownInsideTable);
