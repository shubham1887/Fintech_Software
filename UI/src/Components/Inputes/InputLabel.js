import React, { memo } from 'react';

const InputLabel = ({ color, value }) => {
    let labelStyle = {
        color: color,
        fontWeight: 'bold'
    };
    return <label style={labelStyle}>{value}</label>;
};

export default memo(InputLabel);
