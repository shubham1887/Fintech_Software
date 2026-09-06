import React, { memo, Fragment } from 'react';
import { Grid, FormControlLabel, Box, Switch } from '@material-ui/core';

const CustomSwitch = memo((props) => {
    const { value, callback } = props;

    const handleChange = (menumaterid, newValue) => {
        var newList = value.map((element) =>
            parseInt(element.menumaterid) === parseInt(menumaterid)
                ? { ...element, IsActive: newValue }
                : { ...element, IsActive: element.IsActive }
        );

        callback({ id: menumaterid, value: newValue }, newList);
    };

    return (
        <Fragment>
            {value.map(({ menumaterid, name, IsActive }, index) => (
                <Grid item xs={12} md={12} lg={3} sm={3} xl={3} key={index}>
                    <Box boxShadow={1} m={0.5} px={1} py={0.5}>
                        <span>
                            <FormControlLabel
                                label={name}
                                control={
                                    <Switch
                                        size="small"
                                        color="primary"
                                        checked={IsActive}
                                        value={IsActive ? 'Yes' : 'No'}
                                    />
                                }
                                onChange={(event) => {
                                    var newValue =
                                        event.target.value === 'Yes'
                                            ? false
                                            : true;

                                    handleChange(menumaterid, newValue);
                                }}
                            />
                        </span>
                    </Box>
                </Grid>
            ))}
        </Fragment>
    );
});

export default CustomSwitch;
