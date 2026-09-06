import React, { memo } from 'react';
import Walleticon from 'Assets/Images/wallet.svg';
import { indianFormatNumber } from 'Components/utils';
const UserSummaryHeader = () => {
    return (
        <h4>
            <img src={Walleticon} alt="User Stock" /> User Stock
        </h4>
    );
};

const WalletItem = ({ WalletBalance, WalletName }) => {
    const getIndianFormat = (value) => {
        if (
            value === undefined ||
            value === null ||
            value === '0' ||
            value === NaN
        ) {
            return 0.0;
        }

        return indianFormatNumber(parseFloat(value));
    };

    return (
        <>
            {WalletName}: <span>{getIndianFormat(WalletBalance)} </span>
        </>
    );
};

const UserSummaryItem = ({ UserType, WalletList, Count }) => {
    return (
        <div className="balance d-flex flex-row mt-4" key={UserType}>
            <div className="transactionicon text-center w-25">
                <p style={{ fontSize: 'xx-large' }}>{Count}</p>
            </div>
            <div className="transactiondetails w-75 border-left pl-2">
                <h3>{UserType}</h3>
                <p>
                    {WalletList.map(({ WalletBalance, WalletName }, index) => {
                        return (
                            <WalletItem
                                WalletName={WalletName}
                                WalletBalance={WalletBalance}
                                key={`UserSummaryItem-${index}`}
                                style={{ marginRight: '10px' }}
                            />
                        );
                    })}
                </p>
            </div>
        </div>
    );
};

const DashBoardUserSummary = ({ UserSummary }) => {
    return (
        <div className="walletbalance mt-5 alltransaction">
            <UserSummaryHeader />

            {UserSummary.map(({ UserType, WalletList, Count }, index) => {
                return (
                    <UserSummaryItem
                        UserType={UserType}
                        WalletList={WalletList}
                        Count={Count}
                        key={`DashBoardUserSummary-${index}`}
                    />
                );
            })}
        </div>
    );
};

export default memo(DashBoardUserSummary);
