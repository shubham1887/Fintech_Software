import React, { memo } from 'react';
import { indianFormatNumber } from 'Components/utils';
import Purchase from 'Assets/Images/purchase.svg';
import Sale from 'Assets/Images/sale1.png';
import Surcharge from 'Assets/Images/surcharge.svg';
import Target from 'Assets/Images/target.svg';
import Commission from 'Assets/Images/commission.png';
import Profit from 'Assets/Images/profit.png';
import ROffer from 'Assets/Images/ROffer.png';

const DashBoardAdminTransactionDetails = ({ transactionDetails }) => {
    const getIndianFormat = (value) => {
        if (value === undefined || value === '0' || value === NaN) {
            return 0.0;
        }

        return indianFormatNumber(parseFloat(value));
    };

    return (
        <div className="alltransaction">
            <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Sale} alt="Sale" className="triconsize" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Sale</h5>
                    <h6>
                        <span>
                            {getIndianFormat(
                                transactionDetails.TotalSuccessAmount
                            )}
                        </span>
                        |
                        <span>
                            {getIndianFormat(
                                transactionDetails.TotalFailedAmount
                            )}
                        </span>
                    </h6>
                </div>
            </div>
            <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={ROffer} alt="Sale" className="triconsize" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>ROffer</h5>
                    <h6>
                        Comm :
                        <span>
                            {getIndianFormat(transactionDetails.RofferComm)}
                        </span>
                        <br />
                        Total :
                        <span>
                            {getIndianFormat(transactionDetails.RofferTotal)}
                        </span>
                        <br />
                        Percentage : <span>{transactionDetails.RofferPer}</span>
                    </h6>
                </div>
            </div>

            <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Commission} alt="Sale" className="triconsize" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Admin Comm</h5>
                    <h6>
                        Comm :
                        <span>
                            {getIndianFormat(
                                transactionDetails.TotalAdminCommission
                            )}
                        </span>
                        <br />
                        Sur :
                        <span>
                            {getIndianFormat(
                                transactionDetails.TotalAdminSurcharge
                            )}
                        </span>
                        {/* <br />
                        ROffer :
                        <span>{transactionDetails.ROfferCommition}</span> */}
                    </h6>
                </div>
            </div>
            <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Profit} alt="Sale" className="triconsize" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Admin Profit</h5>
                    <h6>
                        Profit thr. Comm :
                        <span>
                            {getIndianFormat(
                                transactionDetails.AdminCommissionProfit
                            )}
                        </span>
                        <br />
                        Profit thr. Sur :
                        <span>
                            {getIndianFormat(
                                transactionDetails.AdminSurchargeProfit
                            )}
                        </span>
                    </h6>
                </div>
            </div>
            <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Purchase} className="triconsize" alt="Purchase" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Transfer</h5>
                    <h6>
                        <span>
                            {getIndianFormat(transactionDetails.TransferAmount)}
                        </span>
                        | <span>{transactionDetails.TransferCount}</span>
                    </h6>
                </div>
            </div>

            {/* <div className="transactionbox d-flex flex-row border-bottom">
                <div className="transactionicon text-center w-25">
                    <img src={Target} alt="Target" className="triconsize" />
                </div>
                <div className="transactiondetails w-75 border-left pl-2">
                    <h5>Target</h5>
                    <h6>
                        <span>
                            {getIndianFormat(transactionDetails.TotalTarget)}
                        </span>
                        |
                        <span>
                            {getIndianFormat(transactionDetails.RemainsTarget)}
                        </span>
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
                    <h5>Dispute</h5>
                    <h6>
                        <span>{transactionDetails.ComplaintAmount}</span> |
                        <span>{transactionDetails.ComplaintCount}</span>
                    </h6>
                </div>
            </div>
        </div>
    );
};

export default memo(DashBoardAdminTransactionDetails);
