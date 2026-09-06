import React, { memo } from 'react';
import PropTypes from 'prop-types';

import { Card, Icon, Fab } from '@material-ui/core';
const DashboardCard = memo((props) => {
    const { Title, TitleIcon, Data } = props;

    return (
        <Card elevation={3} className="p-4">
            <div className="flex items-center">
                <Fab
                    size="medium"
                    className="bg-light-green circle-44 box-shadow-none"
                >
                    <Icon className="text-green">{TitleIcon}</Icon>
                </Fab>
                <h5 className="font-medium text-green m-0 ml-3">{Title}</h5>
            </div>
            <div className="pt-4 flex items-center">
                <h2 className="m-0 text-muted flex-grow">{Data}</h2>
                <div className="ml-3 small-circle bg-green text-white">
                    <Icon className="small-icon">expand_less</Icon>
                </div>
                <span className="text-13 text-green ml-1"> (+21%)</span>
            </div>
        </Card>
    );
});

DashboardCard.propTypes = {
    Title: PropTypes.string,
    TitleIcon: PropTypes.string,
    Data: PropTypes.string
};

export default DashboardCard;
