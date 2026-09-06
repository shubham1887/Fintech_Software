import React, { memo } from 'react';
import Rupees from 'Assets/Images/rupees.svg';
import Walleticon from 'Assets/Images/wallet.svg';
const WalletDetailHeader = () => {
    return (
        <h4>
            <img src={Walleticon} alt="Wallets" /> Wallet Balance
        </h4>
    );
};

const WalletDetailItem = ({ WalletName, OpeningBalance, ClosingBalance }) => {
    return (
        <div
            className="balance d-flex flex-row mt-4"
            key={WalletName + '_' + OpeningBalance}
        >
            <div className="transactionicon text-center w-25">
                <img src={Rupees} alt="Wallet" className="triconsize" />
            </div>
            <div className="transactiondetails w-75 border-left pl-2">
                <h3>{WalletName}</h3>
                <p>
                    Op. Bal. : <span>{OpeningBalance}</span>
                </p>
                <p>
                    Cl. Bal. : <span>{ClosingBalance}</span>
                </p>
            </div>
        </div>
    );
};
function DashBoardWalletDetails({ wallets }) {
    return (
        <div className="walletbalance mt-5">
            <WalletDetailHeader />
            {wallets.map(({ WalletName, OpeningBalance, ClosingBalance }) => {
                return (
                    <WalletDetailItem
                        key={WalletName + '_' + OpeningBalance}
                        WalletName={WalletName}
                        OpeningBalance={OpeningBalance}
                        ClosingBalance={ClosingBalance}
                    />
                );
            })}
        </div>
    );
}

export default memo(DashBoardWalletDetails);
