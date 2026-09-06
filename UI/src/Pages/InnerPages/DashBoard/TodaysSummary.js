import React from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';
import { indianFormatNumber } from 'Components/utils';

const useStyles = makeStyles({
    summary: (props) => ({
        background: `${props.bgColor} !important`,
        width: '100%',
        color: '#fff',
        borderRadius: '3px',
        display: 'table !important',
        padding: '10px !important'
    }),

    summarythli: {
        marginLeft: '16px',
        width: '100%',
        display: 'block'
    },

    summaryth: {
        width: '100%',
        display: 'block',
        textAlign: 'right'
    },
    summarytd: {
        textAlign: 'right'
    },
    summarytdli: {
        listStyleType: 'Square',
        margin: 3,
        color: PRIMARY_COLOR(),
        fontWeight: 'bold'
    },
    summarytable: { width: '100% !important', textAlign: 'left' }
});
const TodaysSummary = ({ data }) => {
    const classes = useStyles();

    const getIndianFormat = (value) => {
        if (value === undefined || value === '0') {
            return 0;
        }

        return indianFormatNumber(parseFloat(value));
    };

    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Today's Summary
            </h3>
            {data.map(({ Type, TotalAmount }, index) => {
                return (
                    <table
                        className={classes.summarytable}
                        key={`TodaySummarytable-${index}`}
                    >
                        <tbody>
                            <tr>
                                <td>
                                    <li className={classes.summarytdli}>
                                        {Type}
                                    </li>
                                </td>
                                <td className={classes.summarytd}>
                                    {getIndianFormat(TotalAmount)}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                );
            })}
        </div>
    );
};

export default TodaysSummary;
