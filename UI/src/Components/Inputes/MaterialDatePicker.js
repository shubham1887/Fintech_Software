import React, { useEffect, useState } from 'react';
import { useField } from 'formik';
import {
    KeyboardDatePicker,
    MuiPickersUtilsProvider
} from '@material-ui/pickers';
import DateFnsUtils from '@date-io/date-fns';
import 'react-datepicker/dist/react-datepicker.css';

import { VARIANT } from 'Config';

// Utility function to format date to ISO 8601 string
function jsDateToLocalISO8601DateString(date) {
    return [
        String(date.getFullYear()),
        String(101 + date.getMonth()).substring(1),
        String(100 + date.getDate()).substring(1)
    ].join('-');
}

// Utility function to calculate min and max dates based on age constraints
function calculateDateRange() {
    const today = new Date();
    const maxDate = new Date(today.setFullYear(today.getFullYear() - 18));
    const minDate = new Date(today.setFullYear(today.getFullYear() - 80));
    return { minDate, maxDate };
}

export const MaterialDatePicker = ({ label, ...props }) => {
    const [field, meta, helpers] = useField(props);
    const [selectedDate, setSelectedDate] = useState(
        meta.value ? new Date(meta.value) : null
    );

    // Calculate date range
    const { minDate, maxDate } = calculateDateRange();

    useEffect(() => {
        setSelectedDate(meta.value ? new Date(meta.value) : null);
    }, [meta.value]);

    const handleDateChange = (date) => {
        const value = date ? jsDateToLocalISO8601DateString(date) : null;
        helpers.setValue(value);
        setSelectedDate(value ? new Date(value) : null);
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
                format="dd/MM/yyyy"
                autoOk
                value={selectedDate}
                onChange={handleDateChange}
                minDate={minDate}
                maxDate={maxDate}
                helperText={meta.touched && meta.error ? meta.error : ''}
                error={meta.touched && Boolean(meta.error)}
            />
        </MuiPickersUtilsProvider>
    );
};
