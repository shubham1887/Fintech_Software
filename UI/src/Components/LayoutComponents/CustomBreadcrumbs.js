import React from 'react';
import withBreadcrumbs from 'react-router-breadcrumbs-hoc';
import { Breadcrumbs, Link } from '@material-ui/core';
import HomeLink from 'Components/LayoutComponents/HomeLink';

const CustomBreadcrumbs = withBreadcrumbs()(({ breadcrumbs }) => (
    <div className="flex flex-wrap items-center">
        <Breadcrumbs separator="›" maxItems={4}>
            {breadcrumbs.map(({ breadcrumb }, index) => {
                return breadcrumb.key === '/' ? (
                    HomeLink
                ) : (
                    <Link color="inherit" href={breadcrumb.key} key={index}>
                        {breadcrumb}
                    </Link>
                );
            })}
        </Breadcrumbs>
    </div>
));
export default CustomBreadcrumbs;
