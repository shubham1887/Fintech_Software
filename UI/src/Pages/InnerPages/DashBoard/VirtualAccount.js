import React from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { PRIMARY_COLOR } from 'Config';

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
        textAlign: 'left'
    },
    summarytdli: {
        listStyleType: 'Square',
        margin: 3,
        color: PRIMARY_COLOR(),
        fontWeight: 'bold'
    },
    summarytable: { width: '100% !important', textAlign: 'left' }
});
const VirtualAccount = ({ data }) => {
    const classes = useStyles();

    if (
        data === undefined ||
        data === null ||
        data.length <= 0 ||
        data.AccountNumber.length <= 0
    ) {
        return null;
    }

    return (
        <div className="dashrechargepanel equalheight">
            <h3 className="text-center border-bottom pb-2 dashheading">
                Virtual Account
            </h3>

            <table className={classes.summarytable}>
                <thead>
                    <tr>
                        <th className={classes.summarytd}>Account Number</th>
                        <th className={classes.summarytd}>BankName</th>
                        <th className={classes.summarytd}>IFSC</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td className={classes.summarytd}>
                            {data.AccountNumber}
                        </td>
                        <td className={classes.summarytd}>{data.BankName}</td>
                        <td className={classes.summarytd}>{data.IFSC}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    );
};

export default VirtualAccount;
