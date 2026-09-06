import React, { useEffect, useState } from 'react';
import { Formik, Form } from 'formik';
import { Container, Grid, Divider } from '@material-ui/core';

import HOCLoader from 'Components/Loader/HOCLoader';
import Accordion from '@material-ui/core/Accordion';
import HOCAccordion from 'Components/CustomAccordion/HOCAccordion';

const CustomAccordion = HOCAccordion(Accordion);
const CustomContainer = HOCLoader(Container);

export const VersionRowView = ({ index, DOC, Version, Title, Description }) => {
    const [openFilter, setopenFilter] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);

    useEffect(() => {
        setLoadingOpen(false);
        if (index === 0) setopenFilter(true);
    }, [index]);

    const UploadForm = () => {
        return (
            <Form>
                <CustomAccordion
                    title={Version + ' - ' + Title + ' @ ' + DOC}
                    open={openFilter}
                    setOpen={(value) => setopenFilter(value)}
                >
                    <Grid container spacing={2} xs={12}>
                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <Divider />
                        </Grid>

                        <Grid item xs={12} sm={12} md={10} lg={10}>
                            <div
                                style={{
                                    whiteSpace: 'pre-wrap',
                                    lineHeight: 1.8
                                }}
                            >
                                {Description}
                            </div>
                        </Grid>
                    </Grid>
                </CustomAccordion>
            </Form>
        );
    };

    return (
        <CustomContainer isLoading={loadingOpen}>
            <Formik enableReinitialize={true}>{UploadForm}</Formik>
        </CustomContainer>
    );
};
