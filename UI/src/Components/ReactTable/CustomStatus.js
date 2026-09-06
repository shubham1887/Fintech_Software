import React from 'react';

import { getClassName } from 'Components/utils';

export const CustomStatus = (props) => {
    const { value } = props;

    let className = getClassName(value);

    return <span className={className}>{value}</span>;
};
