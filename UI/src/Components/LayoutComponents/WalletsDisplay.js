import { BalanceContext } from 'Components/BalanceContext';
import BalanceAndValue from 'Components/LayoutComponents/BalanceAndValue';
import LayoutStyles from 'Components/LayoutComponents/LayoutStyle';
import React, { useContext, useEffect, useState, memo } from 'react';

const WalletsDisplay = ({ start, end }) => {
    const classes = LayoutStyles();
    const { Wallets } = useContext(BalanceContext);

    const [UserWallets, setUserWallets] = useState([]);

    useEffect(() => {
        setUserWallets(JSON.parse(localStorage.getItem('Wallets') || Wallets));
    }, []);

    useEffect(() => {
        if (Wallets.length >= 1) {
            setUserWallets(JSON.parse(Wallets));
        }
    }, [Wallets]);

    const UserWalletList =
        UserWallets &&
        UserWallets.map(({ ShortName, Balance }, index) => {
            return (
                <BalanceAndValue
                    name={ShortName}
                    balance={Balance}
                    key={index}
                    idx={index}
                />
            );
        });
    return (
        <div className={classes.sectionContentColumn}>
            <div className={classes.allValues}>
                <div className={classes.table}>
                    {UserWallets.length >= 1 ? UserWalletList : null}
                </div>
            </div>
        </div>
    );
};

export default memo(WalletsDisplay);
