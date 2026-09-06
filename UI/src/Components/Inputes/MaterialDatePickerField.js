import React, { useEffect, useState } from 'react';
import { useField } from 'formik';

import { KeyboardDatePicker } from '@material-ui/pickers';
import { MuiPickersUtilsProvider } from '@material-ui/pickers';
import 'react-datepicker/dist/react-datepicker.css';

// pick a date util library
import DateFnsUtils from '@date-io/date-fns';
import { VARIANT } from 'Config';

function jsDateToLocalISO8601DateString(date) {
    return [
        String(date.getFullYear()),
        String(101 + date.getMonth()).substring(1),
        String(100 + date.getDate()).substring(1)
    ].join('-');
}

export const MaterialDatePickerField = ({ label, ...props }) => {
    const [field, meta, helpers] = useField(props);

    const [selectedDate, setSelectedDate] = useState(new Date());

    useEffect(() => {
        setSelectedDate(meta.value ? meta.value : null);
    });

    const handleDateChange = (date) => {
        meta.value = date ? jsDateToLocalISO8601DateString(date) : null;
        helpers.setValue(meta.value);
        setSelectedDate(meta.value);
    };

    return (
        <MuiPickersUtilsProvider utils={DateFnsUtils}>
            <KeyboardDatePicker
                inputVariant={VARIANT}
                className="mb-2 w-full"
                {...props}
                {...field}
                label={label}
                clearable
                format="dd/MMM/yyyy"
                autoOk
                value={selectedDate || ''}
                onChange={handleDateChange}
                minDate={props.minDate}
                maxDate={props.maxDate}
            />
        </MuiPickersUtilsProvider>
    );
};
