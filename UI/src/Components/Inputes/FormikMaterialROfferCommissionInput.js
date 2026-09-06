import React, { Fragment, useState } from 'react';
import { useField } from 'formik';
import { FormControl, Grid, Box } from '@material-ui/core';

import { ComissionComponent } from 'Components/CommissionComponent';

export const MaterialROfferCommissionField = ({
    Commission,
    displayName,
    label,
    ...props
}) => {
    const {
        form: { setFieldValue }
    } = props;
    const [field, meta, helpers] = useField(props);

    const { General, Real } = field.value[label];

    const GeneralCallback = (
        CommissionType,
        Commission,
        CommissionCategory
    ) => {
        if (label === undefined) {
            return;
        }
        setFieldValue(label + '.General.CommissionType', CommissionType);
        setFieldValue(label + '.General.Commission', Commission);
        setFieldValue(
            label + '.General.CommissionCategory',
            CommissionCategory
        );
    };

    return (
        <FormControl fullWidth>
            <Fragment>
                <Grid item xs={12} md={12} lg={12} sm={12}>
                    <Box
                        border={1}
                        borderColor="grey.500"
                        boxShadow={1}
                        bgcolor="background.paper"
                        m={0.5}
                        px={1}
                        py={0.5}
                    >
                        <ComissionComponent
                            value={General}
                            callback={GeneralCallback}
                            displayName="General"
                        />
                    </Box>
                </Grid>
            </Fragment>
        </FormControl>
    );
};
