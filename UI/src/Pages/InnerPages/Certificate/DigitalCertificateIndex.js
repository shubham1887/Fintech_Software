import React, { useState, useEffect } from 'react';

import DigitalCertificateNewPrint from './DigitalCertificateNewPrint';
import { Grid } from '@material-ui/core';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
export const DigitalCertificateIndex = () => {
    const [isAlertOpen, setIsAlertOpen] = useState(false);

    const Print = () => {
        setIsAlertOpen(true);
    };
    useEffect(() => {
        setIsAlertOpen(true);
    }, []);
    return (
        <div>
            <Grid
                container
                item
                xs={12}
                sm={12}
                md={12}
                lg={12}
                style={{ marginTop: '15px' }}
            >
                <Grid xs={12} sm={12} md={2} lg={2}>
                    {
                        <PrimaryButton
                            variant="contained"
                            color="primary"
                            title="Print"
                            onClick={() => Print()}
                        >
                            Print
                        </PrimaryButton>
                    }
                    {isAlertOpen ? (
                        <DigitalCertificateNewPrint
                            isOpen={isAlertOpen}
                            title="Digital Certificate Print"
                            positiveButtonClick={(printData) => {
                                setIsAlertOpen(false);
                            }}
                            negativeButtonClick={() => {
                                setIsAlertOpen(false);
                            }}
                        />
                    ) : null}
                </Grid>
            </Grid>
        </div>
    );
};
