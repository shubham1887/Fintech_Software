import React from 'react';
import { Card, Icon, IconButton, Tooltip } from '@material-ui/core';
import { withStyles } from '@material-ui/styles';
import FontAwesome from 'react-fontawesome';
import { indianFormatNumber } from 'Components/utils';
import { useHistory } from 'react-router';

const styles = (theme) => ({
    icon: {
        fontSize: '44px',
        opacity: 0.6,
        color: theme.palette.primary
    }
});

const TransactionSummaryCard = (props) => {
    const history = useHistory();
    const { classes, backgroundClass } = props;
    const CardClasses = ['play-card p-sm-24 bg-paper ', backgroundClass];
    const handleLinkClick = () => {
        history.push(props.link);
    };
    const IconStyle = {
        textShadow: '0 1px 0 rgba(0, 0, 0, 0.1)'
    };
    return (
        <Card className={CardClasses} elevation={6}>
            <div className="flex items-center">
                <Icon className={classes.icon}>
                    <FontAwesome
                        className="super-crazy-colors"
                        name="inr"
                        style={IconStyle}
                    />
                </Icon>
                <div className="ml-3">
                    <small className="text-muted">{props.Title}</small>
                    <h6 className="m-0 mt-1  font-medium">
                        {indianFormatNumber(props.Data)} |
                        {indianFormatNumber(props.SubData)}
                    </h6>
                </div>
            </div>
            <Tooltip title="View Details" placement="top">
                <IconButton onClick={handleLinkClick}>
                    <Icon className="text-light">arrow_right_alt</Icon>
                </IconButton>
            </Tooltip>
        </Card>
    );
};

export default withStyles(styles, { withTheme: true })(TransactionSummaryCard);
