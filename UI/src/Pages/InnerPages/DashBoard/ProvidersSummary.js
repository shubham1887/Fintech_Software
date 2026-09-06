import React from 'react';
import { makeStyles } from '@material-ui/core/styles';
import Walleticon from 'Assets/Images/wallet.svg';
import { indianFormatNumber } from 'Components/utils';
const ProviderSummaryHeader = () => {
    return (
        <h4>
            <img src={Walleticon} alt="Providers Summary" /> Provider's Stock
        </h4>
    );
};

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
        color: '#fff',
        fontWeight: 'bold'
    },
    summarytable: { width: '100% !important', textAlign: 'left' }
});
const ProvidersSummary = ({ data }) => {
    const classes = useStyles();
    const getIndianFormat = (value) => {
        if (value === undefined || value === '0' || value === NaN) {
            return 0.0;
        }

        return indianFormatNumber(parseFloat(value));
    };
    return (
        <div className="alltransaction">
            <div className="walletbalance mt-5">
                <ProviderSummaryHeader />

                {data.map(({ ProviderName, Balances }, index) => {
                    return (
                        <table
                            className={classes.summarytable}
                            key={`providertable-${index}`}
                        >
                            <tbody>
                                <tr>
                                    <td>
                                        <li className={classes.summarytdli}>
                                            {ProviderName}
                                        </li>
                                    </td>
                                    <td className={classes.summarytd}>
                                        {Balances}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    );
                })}
            </div>
        </div>
    );
};

export default ProvidersSummary;
