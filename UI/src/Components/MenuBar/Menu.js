import React, { useState, useEffect } from 'react';
import { makeStyles } from '@material-ui/core/styles';

import menuItems from './menuItems';
import { SingleElement } from './SingleElement';
import DropDownMenu from './DropDownMenu';

const useStyles = makeStyles((theme) => ({
    root: {
        width: '100%',
        maxWidth: 400,
        backgroundColor: theme.palette.background.paper
    },
    nested: {
        paddingLeft: theme.spacing(2)
    }
}));

export const CustomMenu = () => {
    const [selectedIndex, setSelectedIndex] = useState(-1);
    const [drawerList, setDrawerList] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    var currentUsertypeid = parseInt(localStorage.getItem('UserTypeID'));
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

    const handler = (children) => {
        return children.map((subOption, index) => {
            var arr = subOption.usertypeid;

            var isVisible = false;

            arr.map((arr) => {
                if (arr === -99) isVisible = true;

                if (arr === currentUsertypeid) isVisible = true;
            });

            if (isVisible) {
                return !subOption.children ? (
                    <SingleElement
                        key={index}
                        mainIndex={index}
                        selectedIndex={selectedIndex}
                        subIndex={index}
                        isSingle={true}
                        name={subOption.name}
                        url={subOption.url}
                        icon={subOption.icon}
                        customCallback={(index) => {
                            updateSelectedIndex(index);
                        }}
                    />
                ) : (
                    <DropDownMenu
                        drawerElement={drawerList.find(
                            (element) => element.index === index
                        )}
                        key={index}
                        mainIndex={index}
                        name={subOption.name}
                        children={subOption.children}
                        icon={subOption.icon}
                        currentUsertypeid={currentUsertypeid}
                        customCallback={(index) => {
                            updateSelectedIndex(index);
                        }}
                    />
                );
            }
        });
    };

    const flexContainer = {
        display: 'flex',
        flexDirection: 'row',
        padding: 0
    };

    return (
        <div className={`navbar`}>
            <div className="container">{handler(menuItems.data)}</div>
        </div>

        // <List
        //   className="menu"
        //   style={flexContainer}
        //   key={selectedIndex + isOpen}
        //   component="nav"
        //   aria-labelledby="nested-list-subheader"
        // >

        // </List>
    );
};
