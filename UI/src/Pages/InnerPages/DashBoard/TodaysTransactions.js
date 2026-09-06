import React, { useState, useEffect } from 'react';

import CustomPieChart from 'Components/charts/CustomPieChart';

const TodaysTransactions = (props) => {
    let { data } = props;
    const [displayData, setDisplayData] = useState([]);
    useEffect(() => {
        if (data.length < 1) return;
        let d = [];

        data.forEach((element) => {
            if (element.ServiceName === 'Total' || element.SuccessCount <= 0)
                return;

            d.push({
                name: element.ServiceName,
                value: element.SuccessAmt
            });
        });
        setDisplayData(d);
    }, [data]);

    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Service Summary
            </h3>

            <span className="text-center">
                <CustomPieChart data={displayData} />
            </span>
        </div>
    );
};

export default TodaysTransactions;
