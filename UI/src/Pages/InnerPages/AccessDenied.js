import React, { memo } from 'react';
import { Container, Typography } from '@material-ui/core';
const AccessDenied = memo(() => {
    return (
        <Container>
            <Typography variant="h3">404 - Access Denied</Typography>
            <Typography variant="p">
                You don't have access to this page
            </Typography>
        </Container>
    );
});

export default AccessDenied;
