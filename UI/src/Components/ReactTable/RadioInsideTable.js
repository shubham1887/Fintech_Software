import React from 'react';
import { FormControlLabel, Checkbox, Grid, Box } from '@material-ui/core';
import TextField from '@material-ui/core/TextField';
export const RadioInsideTable = ({ list, value, callback }) => {
    const handleChange = (newValue) => {
        let newList = list.map((element) =>
            parseInt(element.CircleID) === parseInt(newValue)
                ? { ...element, IsActive: !element.IsActive }
                : { ...element, IsActive: element.IsActive }
        );

        callback(newList);
    };

    const OpCodeHandleChange = (id, newValue) => {
        list.forEach((element) => {
            if (parseInt(element.CircleID) === parseInt(id)) {
                element.Opcode = newValue;
                return false;
            }
        });

        callback(list);
    };

    return (
        <React.Fragment>
            {list !== undefined && list !== null
                ? list.map((Item, index) => (
                      <Grid xs={12} sm={12} md={3} lg={3}>
                          <Box
                              boxShadow={1}
                              bgcolor="background.paper"
                              m={0.5}
                              px={1}
                              py={0.5}
                          >
                              <div>
                                  <FormControlLabel
                                      key={index}
                                      value={Item.CircleID}
                                      control={
                                          <Checkbox
                                              name={Item.CircleID}
                                              checked={Item.IsActive}
                                              onChange={(e) => {
                                                  handleChange(e.target.value);
                                              }}
                                          />
                                      }
                                      label={
                                          <span style={{ fontSize: '12px' }}>
                                              {Item.CircleName}
                                          </span>
                                      }
                                  />
                              </div>

                              <TextField
                                  fullWidth
                                  name={Item.CircleID}
                                  label="Opcode"
                                  variant="outlined"
                                  size="small"
                                  value={Item.Opcode}
                                  style={{ fontSize: '12px' }}
                                  onChange={(e) => {
                                      OpCodeHandleChange(
                                          e.target.name,
                                          e.target.value
                                      );
                                  }}
                              />
                          </Box>
                      </Grid>
                  ))
                : null}
        </React.Fragment>
    );
};
