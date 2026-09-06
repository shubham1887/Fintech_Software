import React from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Button } from '@material-ui/core';

const useStyles = makeStyles({
    summarybutton: (props) => ({
        background: `${props.bgColor} !important`,
        width: '100%',
        color: '#fff',
        borderRadius: '3px',
        display: 'table !important',
        padding: '10px !important'
    }),
    summarycount: {
        float: 'left',
        fontFamily: 'Montserrat',
        fontWeight: '400',
        fontSize: '20px',
        marginRight: '10px',
        borderRight: '1px solid #fff',
        paddingRight: '8px'
    },
    summarylabel: {
        fontFamily: 'Montserrat',
        fontWeight: '500',
        fontSize: '14px',
        lineHeight: '20px'
    },
    summarylabeltext: {
        lineHeight: '20px',
        width: '100%',
        display: 'block'
    },
    servicesummarybtn: { width: '100% !important', textAlign: 'left' }
});

const ServiceSummaryResult = ({ bgColor, resultCount, resultLabelupper }) => {
    const props = { bgColor: bgColor };

    const classes = useStyles(props);

    return (
        <>
            <Button className={classes.servicesummarybtn}>
                <div className={classes.summarybutton}>
                    <div className={classes.summarycount}>{resultCount}</div>

                    <span className="downtext">{resultLabelupper}</span>
                </div>
            </Button>
        </>
    );
};

export default ServiceSummaryResult;
