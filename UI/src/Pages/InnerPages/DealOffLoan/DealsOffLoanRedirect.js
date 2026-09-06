import React from 'react';
import { Container, Grid, Paper } from '@material-ui/core';
import { useLocation } from 'react-router-dom';
import HOCLoader from 'Components/Loader/HOCLoader';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
const CustomContainer = HOCLoader(Container);

export const DealsOffLoanRedirect = () => {
    const classes = commonStyle();
    const location = useLocation();
    const RedirectURL = location.state.RedirectURL;
    return (
        <CustomContainer>
            <Paper className={classes.PaperStyle}>
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        <iframe
                            title="Loan"
                            src={RedirectURL}
                            width="100%"
                            style={{ height: '90vh' }}
                        ></iframe>
                    </Grid>
                </Grid>
            </Paper>
        </CustomContainer>
    );
};
