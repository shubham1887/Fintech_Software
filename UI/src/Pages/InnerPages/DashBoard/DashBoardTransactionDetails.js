import React, { memo, useState } from 'react';
import Purchase from 'Assets/Images/purchase.svg';
import Sale from 'Assets/Images/sale.svg';
import Surcharge from 'Assets/Images/surcharge.svg';
import Target from 'Assets/Images/target.svg';

const DashBoardTransactionDetails = ({ transactionDetails }) => {
    const [target, setTarget] = useState(false);
    if (transactionDetails.TotalTarget > 0) setTarget(true);

    return (
        <div className="alltransaction">
            <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Sale} alt="Sale" className="triconsize" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Total Earning </h5>
                    <h6>
                        <span>{transactionDetails.TotalSuccessAmount}</span>
                    </h6>
                </div>
            </div>
            {/* <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Purchase} className="triconsize" alt="Purchase" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Purchase</h5>
                    <h6>
                        <span>{transactionDetails.PurchaseAmount}</span> |
                        <span>{transactionDetails.PurchaseCount}</span>
                    </h6>
                </div>
            </div> */}

            <div className="transactionbox d-flex flex-row">
                <div className="transactionicon text-center w-25">
                    <img
                        src={Surcharge}
                        alt="Surcharge"
                        className="triconsize"
                    />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Commission</h5>
                    <h6>
                        <span>{transactionDetails.TotalCommission}</span>{' '}
                        {/* |
                        <span>{transactionDetails.Surcharge}</span> */}
                    </h6>
                </div>
            </div>
        </div>
    );
};

export default memo(DashBoardTransactionDetails);
