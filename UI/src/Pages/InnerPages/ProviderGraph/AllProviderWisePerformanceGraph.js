import React, { useEffect, useState } from 'react';
import {
    BarChart,
    CartesianGrid,
    XAxis,
    YAxis,
    Tooltip,
    Legend,
    Bar,
    ResponsiveContainer
} from 'recharts';
import { AllProviderWisePerformance } from 'Components/Api/TransactionAPI';
import { LOGOUT } from 'ConstComponents';

function AllProviderWisePerformanceGraph(props) {
    const { date } = props;
    const [
        AllProviderWisePerformanceList,
        setAllProviderWisePerformanceList
    ] = useState([]);

    useEffect(() => {
        SelectAllProviderWisePerformance();
    }, [date]);

    const SelectAllProviderWisePerformance = async () => {
        let jsonObj = {
            Date: date
        };
        var request = JSON.stringify(jsonObj);
        var response = await AllProviderWisePerformance(request);
        if (!response.isAuthorized) LOGOUT();
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setAllProviderWisePerformanceList(data);
    };

    const formatDataForChart = () => {
        return AllProviderWisePerformanceList.map((item, index) => ({
            ProviderName: item.ProviderName,
            Average: item.AvgProcTime
        }));
    };

    const chartData = formatDataForChart();

    // Render a message or placeholder if data is not available
    if (AllProviderWisePerformanceList.length === 0) {
        return (
            <div className="dashrechargepanel">
                <h3 className="text-center border-bottom pb-2 dashheading">
                    Provider Performance Graph
                </h3>
                <h6 style={{ textAlign: 'center' }}>No data available </h6>
            </div>
        );
    }

    return (
        <div className="dashrechargepanel">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Provider Performance Graph
            </h3>
            <ResponsiveContainer width="100%" height={400}>
                <BarChart width={730} height={250} data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="ProviderName" />
                    <YAxis tickFormatter={(tick) => `${tick}s`} />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="Average" fill="#8884d8" />
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}

export default AllProviderWisePerformanceGraph;
