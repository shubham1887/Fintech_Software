import React, { Suspense } from 'react';
import Backdrop from '@material-ui/core/Backdrop';
import LottieControl from 'Components/Loader/LoaderAnimation';

import { makeStyles } from '@material-ui/core/styles';

export const commonStyle = makeStyles((theme) => ({
    backdrop: {
        zIndex: theme.zIndex.drawer + 1,
        color: '#fff'
    }
}));

const CustomSuspense = (props) => {
    const classes = commonStyle();
    return (
        <Suspense
            fallback={
                <Backdrop className={classes.backdrop} open={true}>
                    <LottieControl />
                </Backdrop>
            }
        >
            {props.children}
        </Suspense>
    );
};

export default CustomSuspense;
