//import 'date-fns';
import React from 'react';
import { useField } from 'formik';
import { FormControl } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import DateFnsUtils from '@date-io/date-fns';
import {
    MuiPickersUtilsProvider,
    KeyboardDatePicker
} from '@material-ui/pickers';
import { VARIANT } from 'Config';

const useStyles = makeStyles((theme) => ({
    MaterialDatePickerStyle: {
        marginTop: '0px',
        height: '40px'
    }
}));

export const MaterialDatePicker = ({ label, ...props }) => {
    const classes = useStyles();
    const [field, meta] = useField(props);
    const [selectedDate, setSelectedDate] = React.useState(new Date());
    const handleDateChange = (date) => {
        setSelectedDate(date);
    };

    return (
        <FormControl fullWidth>
            <MuiPickersUtilsProvider utils={DateFnsUtils}>
                <KeyboardDatePicker
                    className={classes.MaterialDatePickerStyle}
                    disableToolbar
                    inputVariant={VARIANT}
                    variant="inline"
                    format="MM/dd/yyyy"
                    margin="normal"
                    id="date-picker-inline"
                    label={label}
                    value={selectedDate}
                    onChange={handleDateChange}
                    KeyboardButtonProps={{
                        'aria-label': 'change date'
                    }}
                />
            </MuiPickersUtilsProvider>
        </FormControl>
    );
};
