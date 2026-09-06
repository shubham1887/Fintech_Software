import React, { useState } from 'react';
import IconButton from '@material-ui/core/IconButton';
import Menu from '@material-ui/core/Menu';
import MenuItem from '@material-ui/core/MenuItem';
import MoreVertIcon from '@material-ui/icons/MoreVert';

const ITEM_HEIGHT = 68;

export default function MoreDetail({ options, value }) {
    const [anchorEl, setAnchorEl] = useState(null);
    const open = Boolean(anchorEl);

    const handleClick = (event) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const isDisable = (option) => {
        let disabled = false;

        if (option.statusIndex === undefined) return disabled;

        if (option.disabledOn !== undefined) {
            const val = value[option.statusIndex];

            option.disabledOn.map((item, index) => {
                if (item === val) {
                    disabled = true;
                }
            });

            return disabled;
        }

        if (option.enableOn !== undefined) {
            disabled = true;
            const val = value[option.statusIndex];

            option.enableOn.map((item, index) => {
                if (item.toString() === val.toString()) {
                    disabled = false;
                }
            });

            return disabled;
        }
        return disabled;
    };

    return (
        <div>
            <IconButton
                aria-label="more"
                aria-controls="long-menu"
                aria-haspopup="true"
                onClick={handleClick}
            >
                <MoreVertIcon />
            </IconButton>
            <Menu
                id="long-menu"
                anchorEl={anchorEl}
                keepMounted
                open={open}
                onClose={handleClose}
                PaperProps={{
                    style: {
                        maxHeight: ITEM_HEIGHT * 4.5,
                        width: '30ch'
                    }
                }}
            >
                {options.map((option) => {
                    let disabled = isDisable(option);

                    return (
                        <MenuItem
                            disabled={disabled}
                            key={option.Title}
                            onClick={(e) => {
                                option.Callback(value);
                                handleClose();
                            }}
                        >
                            {option.Title}
                        </MenuItem>
                    );
                })}
            </Menu>
        </div>
    );
}
