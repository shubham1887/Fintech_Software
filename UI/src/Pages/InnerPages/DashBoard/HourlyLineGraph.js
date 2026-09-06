import React, { useEffect, useState } from 'react';
import {
  LineChart,
  CartesianGrid,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  Line,
  BarChart,
  Bar,
  ResponsiveContainer
} from 'recharts';
import { Grid, Typography } from '@material-ui/core';

import { GetHourlyPerformance, GetLast12hrPerformance } from 'Components/Api/TransactionAPI';
import { LOGOUT } from 'ConstComponents';

const HourlyLineGraph = () => {
  const [Last2PerformanceList, setLast2PerformanceList] = useState([]);
  const [Last12PerformanceList, setLast12PerformanceList] = useState([]);

  useEffect(() => {
    SelectHourlyPerformance();
    SelectLast12HrPerformance();
  }, []);

  const SelectHourlyPerformance = async () => {
    var response = await GetHourlyPerformance();

    if (!response.isAuthorized) LOGOUT();

    if (!response.status) {
      return;
    }

    var data = JSON.parse(response.data);
    setLast2PerformanceList(data);
  };

  const formatDataForChart = () => {
    return Last2PerformanceList.map((item, index) => ({
      Seconds: item.ProcTime,
      count: item.Count,
    }));
  };

  const chartData = formatDataForChart();
  const SelectLast12HrPerformance = async () => {
    var response = await GetLast12hrPerformance();

    if (!response.isAuthorized) LOGOUT();

    if (!response.status) {
      return;
    }
    var data = JSON.parse(response.data);

    setLast12PerformanceList(data);
  };

  const formatDataForChartHourly = () => {
    return Last12PerformanceList.map((item, index) => ({
      time: `${item.IntervalStart} - ${item.IntervalEnd}`,
      Average: item.AvgProcTime,
    }));
  };

  const HorulychartData = formatDataForChartHourly();

  return (
    <div className="dashrechargepanel">
      <div className="graph-container">
        <Typography variant="h5" align="center" gutterBottom>
          Hourly Performance
        </Typography>

        <Grid container spacing={3}>
          <Grid item md={6} xs={12}>
          <ResponsiveContainer width="100%" height={500}>
              <LineChart
               width={500}  
               height={250}
                data={chartData}
                margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <YAxis
                  dataKey="Seconds"
                  ticks={[0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 30, 40, 50, 60]}
                  tickFormatter={(tick) => `${tick}s`}
                />
                <XAxis dataKey="count" />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="Seconds" stroke="#0B5345" />
              </LineChart>
              </ResponsiveContainer>
          </Grid>

          <Grid item md={6} xs={12}>
          <ResponsiveContainer width="100%" height={500}>
              <BarChart  width={500}  
  height={250} data={HorulychartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis tickFormatter={(tick) => `${tick}s`} />
                <Tooltip />
                <Legend />
                <Bar dataKey="Average" fill="#8884d8" />
              </BarChart>
              </ResponsiveContainer>
          </Grid>
        </Grid>
      </div>
    </div>
  );
};

export default HourlyLineGraph;
