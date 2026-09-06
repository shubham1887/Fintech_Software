import React, { memo } from 'react';
import PropTypes from 'prop-types';
import { indianFormatNumber } from 'Components/utils.js';
import LayoutStyles from 'Components/LayoutComponents/LayoutStyle';

const BalanceAndValue = (props) => {
    const classes = LayoutStyles();
    const { name, balance, idx } = props;
    var color = '#005baa';

    if (idx % 2) {
        color = '#eb008b';
    }
    return (
        <div className={classes.tableRow} title={name}>
            <div className={classes.tableCell}>{name} :</div>
            <div className={[classes.rightAlignText]} style={{ color: color }}>
                {indianFormatNumber(Number(balance))}
            </div>
        </div>
    );
};

BalanceAndValue.propTypes = {
    name: PropTypes.string,
    balance: PropTypes.number
};

export default memo(BalanceAndValue);
