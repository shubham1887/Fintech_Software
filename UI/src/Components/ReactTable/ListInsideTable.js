import React from 'react';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

export const ListInsideTable = (props) => {
    const classes = commonStyle();
    const { value, keyName } = props;

    return (
        <span>
            {value.map((Item, index) => (
                <span key={index} className={classes.card_sequence}>
                    {index + 1}
                    {Item[keyName]}
                </span>
            ))}
        </span>
    );
};
