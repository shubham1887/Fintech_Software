import React from 'react';
import { Button } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import { useHistory } from 'react-router';
const useStyles = makeStyles({
    quickButton: (props) => ({
        background: `${props.bgColor} !important`,
        width: '100%',
        display: 'table !important',
        padding: '10px !important',
        '&:hover': {
            background: `${props.hoverColor} !important`
        }
    }),
    quickicon: (props) => ({
        background: `url(${props.imagePath}) no-repeat`,
        lineHeight: '30px',
        padding: '10px'
    }),
    quickiconBackground: {
        background: '#fff',
        borderRadius: '100px',
        height: '40px',
        width: '40px',
        textAlign: 'center',
        padding: '12px 9px 9px 9px',
        marginRight: '10px',
        float: 'left'
    }
});

const DashboardQuickLinksCard = ({
    link,
    upperText,
    lowerText,
    imagePath,
    bgColor,
    hoverColor
}) => {
    const history = useHistory();

    const props = { bgColor, hoverColor, imagePath };

    const classes = useStyles(props);
    const handleLickClick = () => {
        history.push(link);
    };
    return (
        <Button className={classes.quickButton} onClick={handleLickClick}>
            <div className={classes.quickiconBackground}>
                <span className={classes.quickicon}>&nbsp;</span>
            </div>
            <div className="buttontext text-left">
                <span className="uptext">{upperText}</span>
                <span className="downtext">{lowerText}</span>
            </div>
        </Button>
    );
};

export default DashboardQuickLinksCard;
