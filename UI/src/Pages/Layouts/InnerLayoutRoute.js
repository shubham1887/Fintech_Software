import React, { memo } from 'react';

import { Route, Redirect } from 'react-router-dom';

const InnerLayoutRoute = ({ component: Component, ...rest }) => {
    return (
        <Route
            {...rest}
            render={(matchProps) =>
                localStorage.getItem('Authorize') ? (
                    <Component {...matchProps} />
                ) : (
                    <Redirect to="/Login" />
                )
            }
        />
    );
};

export default memo(InnerLayoutRoute);
