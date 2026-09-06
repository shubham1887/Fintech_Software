import React, { memo } from 'react';
import { Route } from 'react-router-dom';
import OuterLayout from 'Pages/Layouts/OuterLayout';

const OuterLayoutRoute = memo(({ component: Component, ...rest }) => {
    return (
        <Route
            {...rest}
            render={(matchProps) => (
                <OuterLayout>
                    <Component {...matchProps} />
                </OuterLayout>
            )}
        />
    );
});

export default OuterLayoutRoute;
