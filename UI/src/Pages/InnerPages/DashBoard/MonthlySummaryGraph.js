import React, { useEffect, useState, memo } from 'react';
import { indianFormatNumber } from 'Components/utils';
import {
    AreaChart,
    Area,
    XAxis,
    YAxis,
    Legend,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip
} from 'recharts';

import { MonthlySummarySelect } from 'Components/Api/TransactionReportAPI';

import { LOGOUT } from 'ConstComponents';
import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const useStyles = makeStyles({
    customToolTipStyle: (props) => ({
        padding: '5px',
        borderRadius: '5px',
        width: '100%',
        background: `#fff !important`
    }),
    amountLabel: (props) => ({
        float: 'right'
    })
});

const MonthlySummaryGraph = () => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);
    const [MonthlySummaryModeList, setMonthlySummaryModeList] = useState([]);

    const [yAxisMax, setYAxisMax] = useState(100);

    useEffect(() => {
        SelectMonthlySummary();
    }, []);

    const SelectMonthlySummary = async () => {
        var response = await MonthlySummarySelect();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        let currentMonthMax = Math.max(...data.map((o) => o.CurrentMonth));
        let lastMonthMax = Math.max(...data.map((o) => o.LastMonth));
        let max = Math.max(currentMonthMax, lastMonthMax);

        setYAxisMax(max * 1.1);

        setMonthlySummaryModeList(data);
    };

    const getIndianFormat = (value) => {
        if (
            value === undefined ||
            value === null ||
            value === 'NaN' ||
            value === '0'
        ) {
            return 0.0;
        }

        return indianFormatNumber(parseFloat(value));
    };

    const CustomTooltip = ({ active, payload, label }) => {
        if (!active || !payload || payload.length <= 0) {
            return null;
        }

        return (
            <div className={classesInline.customToolTipStyle}>
                <span style={{ color: '#000' }}>{`Day : ${label}`}</span>
                <br />

                <div
                    style={{
                        color: '#e3a01e'
                    }}
                >
                    <span> Last Month :</span>
                    <span className={classesInline.amountLabel}>
                        {getIndianFormat(payload[0].value)}
                    </span>
                </div>

                <div
                    style={{
                        color: '#42b86e'
                    }}
                >
                    <span>Current Month :</span>
                    <span className={classesInline.amountLabel}>
                        {getIndianFormat(payload[1].value)}
                    </span>
                </div>
            </div>
        );
    };

    const DataFormatter = (number) => {
        if (number > 10000000) {
            return parseInt(number / 10000000).toString() + 'Cr';
        } else if (number > 100000) {
            return parseInt(number / 100000).toString() + 'L';
        } else if (number > 1000) {
            return parseInt(number / 1000).toString() + 'K';
        } else {
            return number.toString();
        }
    };

    if (
        MonthlySummaryModeList === undefined ||
        MonthlySummaryModeList === null ||
        MonthlySummaryModeList.length <= 0
    ) {
        return null;
    }

    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Monthly Performance
            </h3>

            <ResponsiveContainer width="100%" height={400}>
                <AreaChart
                    data={MonthlySummaryModeList}
                    margin={{
                        top: 10,
                        right: 0,
                        left: 0,
                        bottom: 0
                    }}
                >
                    <defs>
                        <linearGradient
                            id="colorUv"
                            x1="0"
                            y1="0"
                            x2="0"
                            y2="1"
                        >
                            <stop
                                offset="25%"
                                stopColor="#ffc658"
                                stopOpacity={0.8}
                            />
                            <stop
                                offset="95%"
                                stopColor="#ffc658"
                                stopOpacity={0}
                            />
                        </linearGradient>
                        <linearGradient
                            id="colorPv"
                            x1="0"
                            y1="0"
                            x2="0"
                            y2="1"
                        >
                            <stop
                                offset="15%"
                                stopColor="#82ca9d"
                                stopOpacity={0.8}
                            />
                            <stop
                                offset="95%"
                                stopColor="#82ca9d"
                                stopOpacity={0}
                            />
                        </linearGradient>
                    </defs>

                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="Day" />
                    <YAxis
                        type="number"
                        domain={[0, yAxisMax]}
                        tickFormatter={DataFormatter}
                    />

                    <Tooltip content={<CustomTooltip />} />
                    <Legend
                        height={36}
                        iconType="circle"
                        align="right"
                        verticalAlign="top"
                    />
                    <Area
                        type="monotone"
                        dataKey="LastMonth"
                        stroke="#ffc658"
                        fillOpacity={1}
                        fill="url(#colorUv)"
                    />
                    <Area
                        dataKey="CurrentMonth"
                        stroke="#82ca9d"
                        fillOpacity={1}
                        fill="url(#colorPv)"
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    );
};

export default memo(MonthlySummaryGraph);
