import React, { memo } from 'react';
import Copyright from 'Components/Copyright/CopyRight';
import Version from 'Components/Versions/Version';

import { Grid } from '@material-ui/core';

const OuterFooter = () => {
    return (
        <>
            <div className="mb-3"></div>

            <Grid container>
                <Grid item xs={12} sm={12} md={6} lg={6}>
                    <Copyright />
                </Grid>
                <Grid item xs={12} sm={12} md={6} lg={6}>
                    <Version />
                </Grid>
                <Grid
                    container
                    justify="center"
                    item
                    xs={12}
                    sm={12}
                    md={12}
                    lg={12}
                    style={{ marginTop: 20 }}
                ></Grid>
            </Grid>
        </>
    );
};

export default memo(OuterFooter);
