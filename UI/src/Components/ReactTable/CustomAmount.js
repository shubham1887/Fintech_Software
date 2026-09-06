import React from 'react';

import { getClassName } from 'Components/utils';

let customStyle = { textAlign: 'right' };
const CustomAmount = (props) => {
    const { value } = props;
    const { Amount, Status } = value;
    let className = getClassName(Status);
    return (
        <span className={className} style={customStyle}>
            <b>{Amount}</b>
        </span>
    );
};
export default CustomAmount;
