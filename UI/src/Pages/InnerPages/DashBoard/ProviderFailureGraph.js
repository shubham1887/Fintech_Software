import React, { useEffect, useState, memo } from 'react';
import { indianFormatNumber } from 'Components/utils';
import {
    AreaChart,
    Area,
    Brush,
    XAxis,
    YAxis,
    Legend,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip
} from 'recharts';
import moment from 'moment';
import { SelectProviderFailureGraph as SELECT_PROVIDER_FAILURE } from 'Components/Api/TransactionReportAPI';

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

const ProviderFailGraph = () => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);
    const [ProviderFailureList, setProviderFailureList] = useState([]);

    useEffect(() => {
        SelectProviderFailure();
    }, []);

    const SelectProviderFailure = async () => {
        var response = await SELECT_PROVIDER_FAILURE();

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        setProviderFailureList(data);
    };

    const getIndianFormat = (value) => {
        if (value === undefined || value === '0' || value === NaN) {
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
                        color: '#82ca9d'
                    }}
                >
                    <span> Amount :</span>
                    <span className={classesInline.amountLabel}>
                        {getIndianFormat(payload[0].value)}
                    </span>
                </div>

                <div
                    style={{
                        color: '#de3636'
                    }}
                >
                    <span>FailuerMessage:</span>
                    <span className={classesInline.label}>
                        {payload[1].value}
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
        ProviderFailureList === undefined ||
        ProviderFailureList === null ||
        ProviderFailureList.length <= 0 ||
        ProviderFailureList.value === NaN
    ) {
        return null;
    }

    const CustomizedAxisTick = ({ x, y, payload }) => {
        const dateTip = moment(payload.value).format('ll').slice(0, 6);
        return (
            <g transform={`translate(${x},${y})`}>
                <text
                    x={23}
                    y={0}
                    dy={14}
                    fontSize="0.90em"
                    fontFamily="bold"
                    textAnchor="end"
                    fill="#363636"
                >
                    {dateTip}
                </text>
            </g>
        );
    };

    const xAxisTickFormatter = (timestamp_measured) => {
        return moment(timestamp_measured).format('ll').slice(0, 6);
    };

    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Provider Failure
            </h3>
            <ResponsiveContainer width="100%" height={400}>
                <AreaChart
                    width={500}
                    height={400}
                    data={ProviderFailureList}
                    margin={{
                        top: 10,
                        right: 30,
                        left: 0,
                        bottom: 0
                    }}
                >
                    <CartesianGrid strokeDasharray="3 3" />

                    <XAxis
                        height={60}
                        dataKey="DOCStr"
                        tick={CustomizedAxisTick}
                    />
                    <YAxis
                        type="number"
                        domain={[0, (dataMax) => parseInt(dataMax) + 500]}
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
                        dataKey="Amount"
                        stroke="#82ca9d"
                        fillOpacity={1}
                        fill="url(#colorUv)"
                    />
                    <Area
                        dataKey="FailureMessage"
                        name="Reason"
                        stroke="#de3636"
                        fillOpacity={1}
                        fill="url(#colorPv)"
                    />

                    <Brush
                        tickFormatter={xAxisTickFormatter}
                        dataKey="DOCStr"
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    );
};

export default memo(ProviderFailGraph);
