import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import { Copy as SLAB_COPY } from 'Components/Api/SlabApi';
import { MaterialTextField } from 'Components/Inputes/FormikMaterialInput';

import {
    Container,
    Grid,
    Paper,
    Button,
    CircularProgress,
    Box
} from '@material-ui/core';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const CopyCommission = ({ CopySlabID }) => {
    const [loadingOpen, setLoadingOpen] = React.useState(false);
    const classes = commonStyle();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [SlabObj, setSlabObj] = useState({
        CopySlabID: CopySlabID,
        SlabName: ''
    });

    const SlabValidationSchema = Yup.object({
        SlabName: Yup.string().required(PLEASE_PROVIDE_VALID + 'Slab Name')
    });
    const handleSubmit = async (values, { setSubmitting, resetForm }) => {
        if (loadingOpen) return;

        var requestObj = JSON.stringify(values);
        setLoadingOpen(true);
        setIsError(false);

        var response = await SLAB_COPY(requestObj);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setResponseMessage(response.data);
        setIsError(true);
        setSubmitting(false);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            return;
        }
        resetForm(SlabObj);

        reset();
    };
    const reset = () => {
        setSlabObj({
            SlabID: 0,
            SlabName: ''
        });
    };

    const slabForm = (formProps) => {
        return (
            <Form>
                <Grid item xs={12}>
                    <Paper className={classes.PaperStyle}>
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={6} lg={6}>
                                <MaterialTextField
                                    label="Slab Name"
                                    name="SlabName"
                                    type="text"
                                    placeholder="Slab Name"
                                />
                            </Grid>
                        </Grid>
                    </Paper>
                </Grid>

                <Grid className={classes.footerGrid}>
                    <Box className={classes.flexReverse}>
                        <div className={classes.SubmitWrapper}>
                            <Button
                                type="submit"
                                title="Copy"
                                className={classes.CustomPrimaryButtonStyle}
                                disabled={loadingOpen}
                            >
                                Copy
                            </Button>

                            {loadingOpen && (
                                <CircularProgress
                                    size={24}
                                    className={classes.buttonProgress}
                                />
                            )}
                        </div>
                        <Button
                            type="reset"
                            title="Reset"
                            className={classes.CustomDangerButtonStyle}
                            onClick={reset}
                        >
                            Reset
                        </Button>
                    </Box>
                </Grid>
            </Form>
        );
    };
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Formik
                enableReinitialize={true}
                initialValues={SlabObj}
                validationSchema={SlabValidationSchema}
                onSubmit={handleSubmit}
                resetForm={SlabObj}
            >
                {slabForm}
            </Formik>
        </CustomContainer>
    );
};
