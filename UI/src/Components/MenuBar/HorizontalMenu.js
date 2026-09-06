import React, { useState, useEffect } from 'react';
import menuItems from './menuItems';

import { CustomIcon } from 'Components/CustomIcons';
import { NavLink } from 'react-router-dom';
import { withStyles } from '@material-ui/core/styles';
import { NAV_BAR_COLOR, CLIENT_NAME } from 'Config';
import { SUPPORT_USER } from 'Components/Helper/UsertypeCode';

const styles = (theme) => ({
    root: {
        '&, & .horizontal-nav ul ul': {
            background: NAV_BAR_COLOR,
            color: '#494949'
        },
        '& .horizontal-nav ul li a': {
            color: '#494949'
        },
        '& .horizontal-nav ul li': {
            padding: '0px'
        },
        '& .horizontal-nav ul li a:hover': {
            color: '#fff'
        },
        '& .horizontal-nav a, & .horizontal-nav label': {
            color: '#fff'
        },
        '& .horizontal-nav ul li ul li:hover, & .horizontal-nav ul li ul li.open': {
            background: '#eb008b',
            color: '#fff'
        },
        '& .horizontal-nav ul li ul': {
            background: '#000',
            color: '#fff'
        },
        '& .horizontal-nav ul li ul li a': {
            color: '#fff'
        },
        '& .horizontal-nav li > a::after': {
            color: '#fff'
        },
        '& .horizontal-nav ul li:hover': {
            background: '#000',
            color: '#fff'
        },
        '& .horizontal-nav': {
            padding: '0 15px'
        }
    }
});

const HorizontalMenu = (props) => {
    let { classes } = props;
    const [selectedIndex, setSelectedIndex] = useState(-1);
    const [drawerList, setDrawerList] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    var IsCashCollection = localStorage.getItem('IsCashCollection') === 'true';
    var currentUsertypeid = parseInt(localStorage.getItem('UserTypeID'));
    var IsChannel = localStorage.getItem('IsChannel') === 'true';

    const [MenuJson, setMenuJson] = useState([
        JSON.parse(localStorage.getItem('MenuResponse'))
    ]);

    useEffect(() => {
        var option = [];

        menuItems.data.map((item, index) => {
            option.push({ index: index, open: false });
        });

        setDrawerList(option);
    }, []);

    const updateSelectedIndex = (index) => {
        var item = drawerList.find((element) => element.index === index);

        setIsOpen(!item.open);

        setSelectedIndex(index);

        setDrawerList(
            drawerList.map((item) =>
                item.index === index
                    ? { ...item, open: !item.open }
                    : { ...item, open: false }
            )
        );
    };

    const isSubMenuVisible = (subOption) => {
        if (subOption.name === 'CashCollection' && IsCashCollection === false) {
            return false;
        }

        if (subOption.name === 'CashCollect' && IsCashCollection === false) {
            return false;
        }
        if (subOption.name === 'CashReverse' && IsCashCollection === false) {
            return false;
        }

        if (subOption.name === 'ChannelAdd' && IsChannel === false) {
            return false;
        }
        if (subOption.name === 'ChannelAssign' && IsChannel === false) {
            return false;
        }

        return true;
    };

    const isMenuVisible = (subOption) => {
        var arr = subOption.usertypeid;

        var isVisible = false;

        if (arr === undefined) {
            isVisible = isSubMenuVisible(subOption);

            return isVisible;
        }

        arr.map((arr) => {
            if (arr === -99 || arr === currentUsertypeid) {
                isVisible = isSubMenuVisible(subOption);
            }
        });

        return isVisible;
    };

    const renderLevels = (children) => {
        return children.map((subOption, index) => {
            var isVisible = isMenuVisible(subOption);

            if (!isVisible) {
                return null;
            }

            return !subOption.children || subOption.children.length <= 0 ? (
                <li key={index}>
                    <NavLink to={subOption.url ? subOption.url : '/#'}>
                        {subOption.icon && (
                            <CustomIcon
                                className="item-icon align-middle"
                                icon={subOption.icon}
                            ></CustomIcon>
                        )}
                        {subOption.name}
                    </NavLink>
                </li>
            ) : (
                <li key={index}>
                    <a>
                        {subOption.icon && (
                            <CustomIcon
                                className="item-icon align-middle"
                                icon={subOption.icon}
                            ></CustomIcon>
                        )}
                        {subOption.name}
                    </a>
                    <ul>{renderLevels(subOption.children)}</ul>
                </li>
            );
        });
    };

    const flexContainer = {
        display: 'flex',
        flexDirection: 'row',
        padding: 0
    };

    return (
        <div className={`navbar ${classes.root}`}>
            <div className={`horizontal-nav`}>
                <ul className="menu" style={flexContainer}>
                    {currentUsertypeid === SUPPORT_USER
                        ? renderLevels(MenuJson[0].data)
                        : renderLevels(menuItems.data)}
                </ul>
            </div>
        </div>
    );
};

export default withStyles(styles, { withTheme: true })(HorizontalMenu);
