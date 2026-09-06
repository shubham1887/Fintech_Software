import React, { memo, Fragment } from 'react';
import TextField from '@material-ui/core/TextField';
import Checkbox from '@material-ui/core/Checkbox';
import { Grid, FormControlLabel, Box } from '@material-ui/core';
import CheckBoxOutlineBlankIcon from '@material-ui/icons/CheckBoxOutlineBlank';
import CheckBoxIcon from '@material-ui/icons/CheckBox';

export const TextCheckboxList = memo((props) => {
    const { value, keyName, callback } = props;

    const handleChange = (newValue) => {
        var newList = value.map((element) =>
            parseInt(element.ProviderID) === parseInt(newValue)
                ? { ...element, IsActive: !element.IsActive }
                : { ...element, IsActive: element.IsActive }
        );

        callback(newList);
    };
    const handleCountChange = (newCount, ID) => {
        const reg = /^-?[0-9]*(\.[0-9]*)?$/;

        if (
            (!isNaN(newCount) && reg.test(newCount)) ||
            value === '' ||
            value === '-'
        ) {
            var newList = value.map((element) =>
                parseInt(element.ProviderID) === parseInt(ID)
                    ? { ...element, Count: newCount }
                    : { ...element, Count: element.Count }
            );

            callback(newList);
        }
    };

    return (
        <Fragment>
            {value.map((Item) => (
                <Grid item xs={6} md={6} lg={3} sm={3} key={Item.ProviderID}>
                    <Box
                        boxShadow={1}
                        bgcolor="background.paper"
                        m={0.5}
                        px={1}
                        py={0.5}
                    >
                        <div>
                            <FormControlLabel
                                control={
                                    <Checkbox
                                        style={{ width: 30, height: 30 }}
                                        icon={
                                            <CheckBoxOutlineBlankIcon
                                                style={{ fontSize: 20 }}
                                            />
                                        }
                                        checkedIcon={
                                            <CheckBoxIcon
                                                style={{ fontSize: 20 }}
                                            />
                                        }
                                        color="primary"
                                        value={Item.ProviderID}
                                        checked={Item.IsActive}
                                        inputProps={{
                                            'aria-label': 'primary checkbox'
                                        }}
                                        onChange={(e) => {
                                            handleChange(e.target.value);
                                        }}
                                    />
                                }
                                label={
                                    <span style={{ fontSize: '12px' }}>
                                        {Item[keyName]}
                                    </span>
                                }
                            />
                        </div>

                        <TextField
                            fullWidth
                            size="small"
                            style={{ fontSize: '12px' }}
                            variant="outlined"
                            value={Item.Count}
                            inputProps={{
                                maxLength: 3,
                                inputMode: 'numeric',
                                pattern: '[0-9]*'
                            }}
                            name={Item.ProviderID.toString()}
                            onChange={(e) => {
                                handleCountChange(
                                    e.target.value,
                                    e.target.name
                                );
                            }}
                        />
                    </Box>
                </Grid>
            ))}
        </Fragment>
    );
});
