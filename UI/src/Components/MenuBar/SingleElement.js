import React from 'react';
import { ListItem, ListItemText, ListItemIcon } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { CustomIcon } from 'Components/CustomIcons';
import { Link } from 'react-router-dom';

const useStyles = makeStyles((theme) => ({
    minW: {
        minWidth: theme.spacing(5)
    },
    paddingL: {
        paddingLeft: theme.spacing(3)
    }
}));

export const SingleElement = ({
    key,
    mainIndex,
    selectedIndex,
    subIndex,
    isSingle,
    name,
    url,
    classes,
    icon,
    customCallback
}) => {
    const classesStyle = useStyles();
    const mainClasses = commonStyle();

    var clss = [];
    clss.push(classes);

    if (selectedIndex === subIndex) {
        clss.push(mainClasses.selectedColor);
    }

    return (
        <div key={mainIndex + key} title={name}>
            <Link to={url}>
                <ListItem
                    button
                    key={mainIndex + key}
                    className={clss.join(' ')}
                    onClick={() => {
                        if (isSingle) customCallback(mainIndex);
                        else customCallback(subIndex);
                    }}
                >
                    {isSingle ? (
                        <>
                            <ListItemIcon className={classesStyle.minW}>
                                <CustomIcon icon={icon} />
                            </ListItemIcon>
                            <ListItemText primary={name} />
                        </>
                    ) : (
                        <ListItemText
                            primary={name}
                            className={classesStyle.paddingL}
                        />
                    )}
                </ListItem>
            </Link>
        </div>
    );
};
