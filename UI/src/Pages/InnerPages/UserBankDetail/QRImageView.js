import React, { useEffect, useState } from 'react';
import { Grid } from '@material-ui/core';

export const QRImageView = ({ ImageURL, setLoading }) => {
    useEffect(() => {
        setLoading(false);
    }, [ImageURL]);

    return (
        <Grid container justify="center" alignItems="center">
            <Grid item xs={12} sm={12} md={12} lg={12}>
                <span
                    style={{
                        horizontalAlign: 'center'
                    }}
                >
                    <br />
                </span>
                <img
                    className="qrImage"
                    name="ImageURL"
                    src={ImageURL}
                    height={500}
                    width={500}
                    style={{ display: 'block', margin: '0 auto' }}
                    alt={ImageURL}
                />
            </Grid>
        </Grid>
    );
};
