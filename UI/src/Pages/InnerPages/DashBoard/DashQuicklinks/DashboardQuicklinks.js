import React, { memo } from 'react';

import DashboardQuickLinksCard from 'Pages/InnerPages/DashBoard/DashQuicklinks/DashboardQuicklinksCard';
import UserWiseBalanceReport from 'Pages/InnerPages/DashBoard/UserWiseBalanceReport';

const DashboardQuickLinks = (props) => {

    var IsUserListOnDashboard = localStorage.getItem('IsUserListOnDashboard');

    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Quick Links
            </h3>
            <section className="maincardbg mb-5">
                <div className="container-fluid mt-5">
                    <div className="row">
                        <div className="col-md-4col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3">
                            <DashboardQuickLinksCard
                                link="/Recharge"
                                upperText="Transaction"
                                lowerText="History"
                                bgColor="#09b4b6"
                                hoverColor="#008688"
                                imagePath={require('Assets/Images/TransactionHistory.svg')}
                            />
                        </div>
                        <div className="col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3">
                            <DashboardQuickLinksCard
                                link="/UserLedger"
                                upperText="Account"
                                lowerText="Ledger"
                                bgColor="#39b54a"
                                hoverColor="#127920"
                                imagePath={require('Assets/Images/AccountLedger.svg')}
                            />
                        </div>

                        <div className="col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3">
                            <DashboardQuickLinksCard
                                link="/PurchaseReport"
                                upperText="Purchase"
                                lowerText="History"
                                bgColor="#42057f"
                                hoverColor="#1d003b"
                                imagePath={require('Assets/Images/PurchaseHistory.svg')}
                            />
                        </div>

                        <div className="col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3">
                            <DashboardQuickLinksCard
                                link="/SaleReport"
                                upperText="Sales"
                                lowerText="Summary"
                                bgColor="#eb008b"
                                hoverColor="#9c005c"
                                imagePath={require('Assets/Images/SalesSummary.svg')}
                            />
                        </div>

                        <div className="col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3">
                            <DashboardQuickLinksCard
                                link="/Daybook/Report"
                                upperText="Day"
                                lowerText="Book"
                                bgColor="#7e00e0"
                                hoverColor="#4c0087"
                                imagePath={require('Assets/Images/DayBook.svg')}
                            />
                        </div>

                        <div className="col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3">
                            <DashboardQuickLinksCard
                                link="/Fund"
                                upperText="Fund"
                                lowerText="Request"
                                bgColor="#ff0000"
                                hoverColor="#950000"
                                imagePath={require('Assets/Images/FundRequest.svg')}
                            />
                        </div>

                        {IsUserListOnDashboard === 'yes' ? (
                            <UserWiseBalanceReport />
                        ) : null}
                    </div>
                </div>
            </section>
        </div>
    );
};

export default memo(DashboardQuickLinks);
