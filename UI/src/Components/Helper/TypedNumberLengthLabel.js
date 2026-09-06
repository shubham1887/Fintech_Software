import React, { memo } from 'react';

import { checkEqual } from 'Components/Helper/CustomLoadash';
import { PRIMARY_COLOR } from 'Config';
const TypedNumberLengthLabel = ({ Mobile }) => {
    if (Mobile === undefined || Mobile === null) {
        return null;
    }

    return (
        <div
            style={{
                display: 'flex',
                flexDirection: 'row-reverse',
                marginRight: '5px',
                color: PRIMARY_COLOR()
            }}
        >
            <label>{'Count : ' + Mobile.length}</label>
        </div>
    );
};

export default memo(TypedNumberLengthLabel, checkEqual);
