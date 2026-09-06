import React, { useState, useEffect } from 'react';
import ListItem from '@material-ui/core/ListItem';
import List from '@material-ui/core/List';
import Popper from '@material-ui/core/Popper';
import ListItemText from '@material-ui/core/ListItemText';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import { makeStyles } from '@material-ui/core/styles';
import ExpandLess from '@material-ui/icons/ExpandLess';
import ExpandMore from '@material-ui/icons/ExpandMore';
import { CustomIcon } from 'Components/CustomIcons';

import { SingleElement } from './SingleElement';
import Paper from '@material-ui/core/Paper';

const useStyles = makeStyles((theme) => ({
    nested: {
        paddingLeft: theme.spacing(4)
    },
    minW: {
        minWidth: theme.spacing(5)
    }
}));

const DropDownMenu = ({
    drawerElement,
    mainIndex,
    name,
    children,
    icon,
    currentUsertypeid,
    customCallback
}) => {
    const classes = useStyles();
    const [selectedIndex, setSelectedIndex] = useState(-1);
    const [open, setOpen] = useState(false);

    useEffect(() => {
        if (drawerElement === undefined) return;
        setOpen(drawerElement.open);
    }, [drawerElement]);

    const flexContainer = {
        display: 'flex',
        flexDirection: 'column',
        padding: 0
    };
    const [anchorEl, setAnchorEl] = React.useState(null);

    const handleClick = (event) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };
    return (
        <React.Fragment key={mainIndex}>
            <div style={flexContainer}>
                <ListItem
                    className="item-icon align-middle"
                    title={name}
                    button
                    onClick={(event) => {
                        setOpen(!open);
                        setAnchorEl(anchorEl ? null : event.currentTarget);
                        customCallback(mainIndex);
                    }}
                >
                    <ListItemIcon>{<CustomIcon icon={icon} />}</ListItemIcon>
                    <ListItemText primary={name} />
                    {open ? <ExpandLess /> : <ExpandMore />}
                </ListItem>

                <Popper
                    open={open}
                    anchorEl={anchorEl}
                    placement={'bottom'}
                    transition
                >
                    <Paper>
                        <List component="div" disablePadding>
                            {children.map((subOption, index) => {
                                var isVisible = false;
                                if (subOption.usertypeid !== undefined) {
                                    var arr = subOption.usertypeid;

                                    arr.map((arr) => {
                                        if (arr === -99) isVisible = true;

                                        if (arr === currentUsertypeid)
                                            isVisible = true;
                                    });
                                } else isVisible = true;

                                return isVisible === true ? (
                                    <SingleElement
                                        key={index}
                                        mainIndex={mainIndex}
                                        selectedIndex={selectedIndex}
                                        subIndex={index}
                                        isSingle={false}
                                        name={subOption.name}
                                        url={subOption.url}
                                        classes={classes.nested}
                                        icon={subOption.icon}
                                        customCallback={(indexM) => {
                                            setSelectedIndex(indexM);
                                        }}
                                    />
                                ) : null;
                            })}
                        </List>
                    </Paper>
                </Popper>
            </div>
        </React.Fragment>
    );
};

export default DropDownMenu;
