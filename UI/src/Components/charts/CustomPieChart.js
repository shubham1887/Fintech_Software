import React, { memo } from 'react';
import {
    PieChart,
    Legend,
    Pie,
    Cell,
    Tooltip,
    ResponsiveContainer
} from 'recharts';
import { indianFormatNumber } from 'Components/utils';
import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

const COLORS = [
    { start: '#9e54ed', end: '#5c4cb6' },
    { start: '#34c3ff', end: '#2876bd' },

    { start: '#f46b45', end: '#eea849' },
    { start: '#56ab2f', end: '#a8e063' },
    { start: '#FFAFBD', end: '#ffc3a0' },
    { start: '#4AC29A', end: '#BDFFF3' },
    { start: '#667db6', end: '#667db6' },
    { start: '#fc4a1a', end: '#f7b733' },
    { start: '#DA4453', end: '#89216B' },
    { start: '#8360c3', end: '#2ebf91' },

    { start: '#606c88', end: '#3f4c6b' },
    { start: '#a73737', end: '#7a2828' },
    { start: '#614385', end: '#516395' },
    { start: '#02AAB0', end: '#00CDAC' },

    { start: '#da9d35', end: '#e96935' }
];

const RADIAN = Math.PI / 180;

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

const CustomPieChart = ({ data }) => {
    const propsInline = { hoverColor: PRIMARY_COLOR() };

    const classesInline = useStyles(propsInline);

    const getIndianFormat = (value) => {
        if (value === undefined || value === NaN || value === '0') {
            return 0.0;
        }

        return indianFormatNumber(parseFloat(value));
    };

    const renderCustomizedLabel = ({
        cx,
        cy,
        midAngle,
        innerRadius,
        outerRadius,
        percent,
        index
    }) => {
        const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
        const x = cx + radius * Math.cos(-midAngle * RADIAN);
        const y = cy + radius * Math.sin(-midAngle * RADIAN);

        return (
            <text
                x={x}
                y={y}
                fill="white"
                textAnchor={x > cx ? 'start' : 'end'}
                dominantBaseline="central"
            >
                {`${(percent * 100).toFixed(0)}%`}
            </text>
        );
    };

    const CustomLegend = ({ payload, label }) => {
        if (!payload || payload.length <= 0) {
            return null;
        }

        return (
            <div className="container-fluid">
                <div className="row ">
                    {payload.map((item, index) => (
                        <span
                            key={'index' + index}
                            className="col-sm-6 col-md-6 col-lg-6 col-xl-6 mb-3"
                            style={{
                                background: `linear-gradient(90deg, ${
                                    COLORS[index % COLORS.length].start
                                } 30%, ${
                                    COLORS[index % COLORS.length].end
                                } 90%)`,

                                color: 'white'
                            }}
                        >
                            {item.value === 'Money Transfer'
                                ? 'DMT'
                                : item.value}
                            : {getIndianFormat(item.payload.value)}
                        </span>
                    ))}
                </div>
            </div>
        );
    };

    // const renderColorfulLegendText = (value, entry) => {
    //     const { color, payload } = entry;

    //     return (
    //         <span style={{ color }}>
    //             <span>{value} : </span>
    //             <span className={classesInline.amountLabel}>
    //                 {getIndianFormat(payload.value)}
    //             </span>
    //         </span>
    //     );
    // };

    return (
        <ResponsiveContainer width="100%" height={400}>
            <PieChart>
                <defs>
                    {data.map((entry, index) => (
                        <linearGradient
                            id={`myGradient${index}`}
                            key={'index' + index}
                        >
                            <stop
                                offset="0%"
                                stopColor={COLORS[index % COLORS.length].start}
                            />
                            <stop
                                offset="100%"
                                stopColor={COLORS[index % COLORS.length].end}
                            />
                        </linearGradient>
                    ))}
                </defs>

                <Legend
                    height={36}
                    iconType="square"
                    verticalAlign="bottom"
                    align="right"
                    content={<CustomLegend />}
                    //formatter={renderColorfulLegendText}
                />

                <Pie
                    data={data}
                    dataKey="value"
                    nameKey="name"
                    isAnimationActive={true}
                    label={renderCustomizedLabel}
                    labelLine={false}
                >
                    {data.map((entry, index) => (
                        <Cell
                            key={`cell-${index}`}
                            fill={`url(#myGradient${index})`}
                        />
                    ))}
                </Pie>
                <Tooltip />
            </PieChart>
        </ResponsiveContainer>
    );
};

export default memo(CustomPieChart);
