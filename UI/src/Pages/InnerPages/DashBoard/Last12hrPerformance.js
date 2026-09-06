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
import { GetLast12hrPerformance } from 'Components/Api/TransactionAPI';
import { LOGOUT } from 'ConstComponents';

const HourlyLineGraph = () => {
    const [Last12PerformanceList, setLast12PerformanceList] = useState([]);

    useEffect(() => {
        SelectLast12HrPerformance();
    }, []);

    const SelectLast12HrPerformance = async () => {
        var response = await GetLast12hrPerformance();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);

        setLast12PerformanceList(data);
    };

    const formatDataForChart = () => {
        return Last12PerformanceList.map((item, index) => ({
            time: `${item.IntervalStart} - ${item.IntervalEnd}`,
            Average: item.AvgProcTime
        }));
    };

    const chartData = formatDataForChart();

    return (
        Last12PerformanceList.length !== 0 && (
            <div className="dashrechargepanel">
                <h3 className="text-center border-bottom pb-2 dashheading">
                    Hourly Performance
                </h3>
                <ResponsiveContainer width="100%" height={400}>
                    <BarChart width={730} height={250} data={chartData}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="time" />
                        <YAxis tickFormatter={(tick) => `${tick}s`} />
                        <Tooltip />
                        <Legend />
                        <Bar dataKey="Average" fill="#8884d8" />
                    </BarChart>
                </ResponsiveContainer>
            </div>
        )
    );
};

export default HourlyLineGraph;
