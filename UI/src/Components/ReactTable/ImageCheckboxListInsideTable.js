import React, { memo, Fragment } from 'react';
import TextField from '@material-ui/core/TextField';
import Checkbox from '@material-ui/core/Checkbox';
import { Grid, FormControlLabel, Box } from '@material-ui/core';
import CheckBoxOutlineBlankIcon from '@material-ui/icons/CheckBoxOutlineBlank';
import CheckBoxIcon from '@material-ui/icons/CheckBox';

export const ImageCheckboxListInsideTable = memo((props) => {
    const { value, keyName, callback } = props;

    const handleChange = (newValue) => {
        var newList = value.map((element) =>
            parseInt(element.CategoryChannelMappingID) === parseInt(newValue)
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
                parseInt(element.CategoryChannelMappingID) === parseInt(ID)
                    ? { ...element, Count: newCount }
                    : { ...element, Count: element.Count }
            );

            callback(newList);
        }
    };

    return (
        <Fragment>
            {value.map((Item, index) => (
                <Grid
                    item
                    item
                    xs={12}
                    sm={12}
                    md={3}
                    lg={3}
                    key={
                        Item.CategoryChannelMappingID ??
                        0 + '_' + Item.CategoryChannelMappingID ??
                        0 + '_' + index
                    }
                >
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
                                    Item.IsCheckbox ? (
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
                                            value={
                                                Item.CategoryChannelMappingID ??
                                                0
                                            }
                                            checked={Item.IsActive}
                                            inputProps={{
                                                'aria-label': 'primary checkbox'
                                            }}
                                            onChange={(e) => {
                                                handleChange(e.target.value);
                                            }}
                                        />
                                    ) : (
                                        <></>
                                    )
                                }
                                label={
                                    <span style={{ fontSize: '12px' }}>
                                        <img
                                            src={Item.ChannelImagePath}
                                            width="50px"
                                            height="auto"
                                            style={{
                                                marginRight: '10px',
                                                marginLeft: '10px'
                                            }}
                                        />
                                        {Item[keyName]}
                                    </span>
                                }
                            />
                        </div>
                    </Box>
                </Grid>
            ))}
        </Fragment>
    );
});
