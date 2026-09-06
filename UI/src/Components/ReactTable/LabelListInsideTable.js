import React, { memo } from 'react';
import { Button, Grid } from '@material-ui/core';

import { makeStyles } from '@material-ui/core/styles';
const useStyles = makeStyles((theme) => ({
    margin: {
        margin: theme.spacing(1)
    },
    extendedIcon: {
        marginRight: theme.spacing(1)
    }
}));
const LabelListInsideTable = memo(({ value, keyName, keyID, callback }) => {
    const classes = useStyles();

    return (
        <Grid container>
            {value.map((Item, index) => (
                <Grid item xs style={{ flexGrow: 0 }} key={index}>
                    <Button
                        size="small"
                        variant="outlined"
                        color="primary"
                        className={classes.margin}
                        onClick={() => {
                            callback(Item[keyID]);
                        }}
                    >
                        {Item[keyName]}
                    </Button>
                </Grid>
            ))}
        </Grid>
    );
});

export default LabelListInsideTable;
