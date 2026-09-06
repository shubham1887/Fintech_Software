import React, { useState, memo } from 'react';
import { useHistory } from 'react-router-dom';
import { MenuItem, Icon, Menu, Link } from '@material-ui/core';
import { FORCELOGOUT } from 'ConstComponents';

const ProfileSubMenu = () => {
    const [mobileMoreAnchorEl, setMobileMoreAnchorEl] = useState(null);
    const [anchorEl, setAnchorEl] = useState(null);
    const isMenuOpen = Boolean(anchorEl);
    const menuId = 'primary-search-account-menu';
    const handleMobileMenuClose = () => {
        setMobileMoreAnchorEl(null);
    };
    const handleMenuClose = () => {
        setAnchorEl(null);
        handleMobileMenuClose();
    };
    const handleLogoutButton = () => {
        FORCELOGOUT();
    };

    const history = useHistory();
    return (
        <Menu
            anchorEl={anchorEl}
            anchorOrigin={{
                vertical: 'top',
                horizontal: 'right'
            }}
            id={menuId}
            keepMounted
            transformOrigin={{
                vertical: 'top',
                horizontal: 'right'
            }}
            open={isMenuOpen}
            onClose={handleMenuClose}
        >
            <MenuItem
                onClick={() => {
                    handleMenuClose();
                    history.push('/UsersProfile');
                }}
            >
                <Icon> person </Icon>
                <span className="pl-4"> Profile </span>
            </MenuItem>

            <MenuItem
                onClick={() => {
                    handleMenuClose();
                    history.push('/CreatePlan');
                }}
            >
                Create Plan
            </MenuItem>

            <MenuItem
                onClick={() => {
                    handleMenuClose();
                    history.push('/YesbankeKYC');
                }}
            >
                Agent KYC Update
            </MenuItem>
            <MenuItem
                onClick={() => {
                    history.push('/Users/ChangePassword');
                    handleMenuClose();
                }}
            >
                Change Password
            </MenuItem>
            <MenuItem
                onClick={() => {
                    history.push('/Users/ChangeMpin');
                    handleMenuClose();
                }}
            >
                Change Pin
            </MenuItem>
            <MenuItem
                onClick={() => {
                    history.push('/UserwiseLowBalance');
                    handleMenuClose();
                }}
            >
                Low Balance
            </MenuItem>

            <MenuItem onClick={handleLogoutButton}>
                <Link>Logout</Link>
            </MenuItem>
        </Menu>
    );
};

export default memo(ProfileSubMenu);
