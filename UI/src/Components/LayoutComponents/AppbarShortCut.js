import React, { memo } from 'react';
import PropTypes from 'prop-types';
import { useHistory } from 'react-router-dom';
import FontAwesome from 'react-fontawesome';
import { Badge, IconButton, Tooltip } from '@material-ui/core';
import LayoutStyles from 'Components/LayoutComponents/LayoutStyle';
const styleInner = {
    marginLeft: '2px',
    height: '40px'
};
const AppbarShortCut = ({ Title, Content, Link, Icon, ClassName }) => {
    const history = useHistory();
    const classes = LayoutStyles();

    return (
        <Tooltip title={Title} placement="top-start" className={ClassName}>
            <IconButton
                color="inherit"
                onClick={() => {
                    history.push(Link);
                }}
                style={styleInner}
            >
                <Badge
                    badgeContent={
                        Content === undefined ||
                        Content === null ||
                        Content === '' ||
                        Content === '0'
                            ? null
                            : Content.toString()
                    }
                    classes={{ badge: classes.customBadge }}
                >
                    <FontAwesome
                        className="super-crazy-colors"
                        name={Icon}
                        style={{
                            textShadow: '0 1px 0 rgba(0, 0, 0, 0.1)'
                        }}
                    />
                </Badge>
            </IconButton>
        </Tooltip>
    );
};

AppbarShortCut.propTypes = {
    Title: PropTypes.string,
    Content: PropTypes.string,
    Link: PropTypes.string,
    Icon: PropTypes.string
};

export default memo(AppbarShortCut);
