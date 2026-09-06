import React from 'react';
import ServiceSummaryResult from 'Pages/InnerPages/DashBoard/DashServiceSummary/ServiceSummaryResult';
const ServiceSummary = ({ data }) => {
    const colorList = [
        '#09b4b6',
        '#39b54a',
        '#42057f',
        '#eb008b',
        '#7e00e0',
        '#ff0000'
    ];
    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Service Summary
            </h3>
            <section className="maincardbg mb-5">
                <div className="container-fluid mt-5">
                    <div className="row">
                        {data.map(
                            (
                                { ServiceName, SuccessAmt, ClosingBalance },
                                index
                            ) => {
                                return (
                                    <div
                                        className="col-sm-4 col-md-4 col-lg-6 col-xl-6 mb-3"
                                        key={`serviceDiv-${index}`}
                                    >
                                        <ServiceSummaryResult
                                            resultCount={SuccessAmt}
                                            resultLabelupper={ServiceName}
                                            bgColor={colorList[index % 6]}
                                        />
                                    </div>
                                );
                            }
                        )}
                    </div>
                </div>
            </section>
        </div>
    );
};

export default ServiceSummary;
