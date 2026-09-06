import React, { useState, useEffect } from 'react';
import { Grid } from '@material-ui/core';
import { IS_SIGNUP_WITH_EKYC } from 'Config';

import SignupWithEkyc from './SignupWithEkyc';
import Signup from './Signup';

const IndexSignUp = () => {
    const [isEkycWithSignUp, setIsEkycWithSignUp] = useState('');

    const fetchActiveSignup = async () => {
        let IsActiveSignup = await IS_SIGNUP_WITH_EKYC();

        if (
            IsActiveSignup === undefined ||
            IsActiveSignup === null ||
            IsActiveSignup === ''
        ) {
            setIsEkycWithSignUp('OFF');
            return;
        }
        setIsEkycWithSignUp(IsActiveSignup);
    };

    useEffect(() => {
        fetchActiveSignup();
    }, []);

    if (isEkycWithSignUp === 'ON') {
        return (
            <Grid>
                <SignupWithEkyc />
            </Grid>
        );
    }
    return (
        <Grid>
            <Signup />
        </Grid>
    );
};
export default IndexSignUp;
