import React from 'react';
import { useHistory } from 'react-router';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { Grid, Button, Box } from '@material-ui/core';

const FormHeader = ({ title, buttonName, link, shouldDisplayButton }) => {
    const classes = commonStyle();
    const history = useHistory();
    const handleLinkOpen = () => {
        history.push(link);
    };
    const LinkButton = (
        <Button
            variant="contained"
            color="primary"
            className={classes.CustomPrimaryButtonStyle}
            onClick={handleLinkOpen}
        >
            {buttonName}
        </Button>
    );

    if (shouldDisplayButton === true) {
        return (
            <Grid container spacing={3}>
                <Grid item xs={6}>
                    <h2 className={classes.PaperHead_h2}>{title}</h2>
                </Grid>
                <Grid item xs={6}>
                    <Box className={classes.flexReverse}>{LinkButton}</Box>
                </Grid>
            </Grid>
        );
    }

    return (
        <Grid container spacing={3}>
            <Grid item xs={12}>
                <h2 className={classes.PaperHead_h2}>{title}</h2>
            </Grid>
        </Grid>
    );
};

export default FormHeader;
