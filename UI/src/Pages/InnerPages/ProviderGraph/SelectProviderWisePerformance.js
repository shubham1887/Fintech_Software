import React, { useEffect, useState } from 'react';
import { Grid } from '@mui/material';
import {
    BarChart,
    LineChart,
    CartesianGrid,
    XAxis,
    YAxis,
    Tooltip,
    Legend,
    Bar,
    Line,
    ResponsiveContainer
} from 'recharts';
import {
    GetProviderWisePerformance,
    GetHourlyPerformance
} from 'Components/Api/TransactionAPI';
import { LOGOUT } from 'ConstComponents';

const CombinedLineGraph = (props) => {
    const [last12PerformanceList, setLast12PerformanceList] = useState([]);
    const [last2PerformanceList, setLast2PerformanceList] = useState([]);
    const { date, providerID } = props;
    useEffect(() => {
        selectLast12HrPerformance();
        selectHourlyPerformance();
    }, []);

    const selectLast12HrPerformance = async () => {
        try {
            let jsonObj = {
                Date: date,
                ProviderID: providerID
            };
            var request = JSON.stringify(jsonObj);
            const response = await GetProviderWisePerformance(request);
            if (!response.isAuthorized) LOGOUT();
            if (response.status) {
                const data = JSON.parse(response.data);
                setLast12PerformanceList(data);
            }
        } catch (error) {
            console.error('Error fetching last 12 hours performance:', error);
        }
    };

    const selectHourlyPerformance = async () => {
        try {
            const response = await GetHourlyPerformance();
            if (!response.isAuthorized) LOGOUT();
            if (response.status) {
                const data = JSON.parse(response.data);
                setLast2PerformanceList(data);
            }
        } catch (error) {
            console.error('Error fetching last 2 hours performance:', error);
        }
    };

    const formatDataForBarChart = () => {
        return last12PerformanceList.map((item) => ({
            time: `${item.IntervalStart} - ${item.IntervalEnd}`,
            Average: item.AvgProcTime
        }));
    };

    const formatDataForLineChart = () => {
        return last2PerformanceList.map((item) => ({
            count: item.Count,
            Seconds: item.ProcTime
        }));
    };

    const barChartData = formatDataForBarChart();
    const lineChartData = formatDataForLineChart();

    return (
        <div className="dashrechargepanel">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Performance
            </h3>
            <Grid container spacing={2}>
                {!lineChartData.length ? (
                    <Grid item xs={12}>
                        <ResponsiveContainer width="100%" height={400}>
                            <BarChart
                                width={730}
                                height={250}
                                data={barChartData}
                            >
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="time" />
                                <YAxis tickFormatter={(tick) => `${tick}s`} />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="Average" fill="#8884d8" />
                            </BarChart>
                        </ResponsiveContainer>
                    </Grid>
                ) : (
                    <>
                        <Grid item xs={12} sm={6}>
                            <ResponsiveContainer width="100%" height={400}>
                                <LineChart
                                    width={730}
                                    height={250}
                                    data={lineChartData}
                                    margin={{
                                        top: 5,
                                        right: 30,
                                        left: 20,
                                        bottom: 5
                                    }}
                                >
                                    <CartesianGrid strokeDasharray="3 3" />
                                    <XAxis dataKey="count" />
                                    <YAxis
                                        dataKey="Seconds"
                                        ticks={[0, 10, 20, 30, 40, 50, 60]}
                                        tickFormatter={(tick) => `${tick}s`}
                                    />
                                    <Tooltip />
                                    <Legend />
                                    <Line
                                        type="monotone"
                                        dataKey="Seconds"
                                        stroke="#0B5345"
                                    />
                                </LineChart>
                            </ResponsiveContainer>
                        </Grid>
                        <Grid item xs={12} sm={6}>
                            <ResponsiveContainer width="100%" height={400}>
                                <BarChart
                                    width={730}
                                    height={250}
                                    data={barChartData}
                                >
                                    <CartesianGrid strokeDasharray="3 3" />
                                    <XAxis dataKey="time" />
                                    <YAxis
                                        tickFormatter={(tick) => `${tick}s`}
                                    />
                                    <Tooltip />
                                    <Legend />
                                    <Bar dataKey="Average" fill="#8884d8" />
                                </BarChart>
                            </ResponsiveContainer>
                        </Grid>
                    </>
                )}
            </Grid>
        </div>
    );
};

export default CombinedLineGraph;
