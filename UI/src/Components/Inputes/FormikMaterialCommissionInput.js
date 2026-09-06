import React, { Fragment, useState, useEffect } from 'react';
import { useField } from 'formik';
import { FormControl, Grid, Box } from '@material-ui/core';

import { ComissionComponent } from 'Components/CommissionComponent';

export const MaterialCommissionField = ({
    Commission,
    displayName,
    label,
    RealCommission,

    ...props
}) => {
    const {
        form: { setFieldValue }
    } = props;
    const [field, meta, helpers] = useField(props);
    const { General, Real } = field.value[label];
    const [realDisplay, setRealDisplay] = useState('block');
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

    const RealCallback = (CommissionType, Commission, CommissionCategory) => {
        if (label === undefined) {
            return;
        }
        setFieldValue(label + '.Real.CommissionType', CommissionType);
        setFieldValue(label + '.Real.Commission', Commission);
        setFieldValue(label + '.Real.CommissionCategory', CommissionCategory);
    };

    useEffect(() => {
        if (RealCommission === undefined) {
            setRealDisplay('block');
            return;
        }

        if (RealCommission === false) {
            setRealDisplay('none');
            return;
        }
    }, [RealCommission]);

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
                <Grid item xs={12} md={12} lg={12} sm={12}>
                    <Box
                        display={realDisplay}
                        border={1}
                        borderColor="grey.500"
                        boxShadow={1}
                        bgcolor="background.paper"
                        m={0.5}
                        px={1}
                        py={0.5}
                    >
                        <ComissionComponent
                            value={Real}
                            callback={RealCallback}
                            displayName="Real"
                        />
                    </Box>
                </Grid>
            </Fragment>
        </FormControl>
    );
};
