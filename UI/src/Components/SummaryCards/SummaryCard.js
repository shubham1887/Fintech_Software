import React from 'react';
import { Card, Icon, IconButton, Tooltip } from '@material-ui/core';
import { withStyles } from '@material-ui/styles';

const styles = (theme) => ({
    icon: {
        fontSize: '44px',
        opacity: 0.6,
        color: theme.palette.primary.main
    }
});

const SummaryCard = (props) => {
    const { classes } = props;
    return (
        <Card className="play-card p-sm-24 bg-paper" elevation={6}>
            <div className="flex items-center">
                <Icon className={classes.icon}>group</Icon>
                <div className="ml-3">
                    <small className="text-muted">{props.Title}</small>
                    <h6 className="m-0 mt-1 text-primary font-medium">
                        {props.Data}
                    </h6>
                </div>
            </div>
            <Tooltip title="View Details" placement="top">
                <IconButton href={props.Link}>
                    <Icon>arrow_right_alt</Icon>
                </IconButton>
            </Tooltip>
        </Card>
    );
};

export default withStyles(styles, { withTheme: true })(SummaryCard);
