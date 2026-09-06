import React from 'react';
import { Icon } from '@material-ui/core';
import { NavLink } from 'react-router-dom';

const HomeLink = (
    <NavLink
        key="home"
        to={
            localStorage.getItem('HomePage')
                ? localStorage.getItem('HomePage')
                : '/#'
        }
        color="inherit"
    >
        <Icon className="align-middle ml-2 mb-1" color="primary">
            home
        </Icon>
    </NavLink>
);

export default HomeLink;
