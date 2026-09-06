import React from 'react';
import Backdrop from '@material-ui/core/Backdrop';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import LottieControl from 'Components/Loader/LoaderAnimation';
import CustomeAlert from 'Components/Alert/Alert';

const HOCLoader = (Comp) => ({
    isLoading,
    isError,
    alertMessage,
    severity,
    onCloseCallback,
    children,
    ...props
}) => {
    const classes = commonStyle();

    if (isError === false) {
        alertMessage = '';
    }
    if (alertMessage === undefined || alertMessage === null) {
        isError = false;
        alertMessage = '';
    }

    if (alertMessage.length <= 0) {
        isError = false;
    }

    return (
        <>
            <Backdrop
                className={classes.backdrop}
                open={isLoading === undefined ? false : isLoading}
            >
                <LottieControl />
            </Backdrop>

            <Comp {...props} className={classes.fullWidth}>
                {isError ? (
                    <div>
                        <CustomeAlert
                            alertMessage={alertMessage}
                            isOpen={isError}
                            severity={severity}
                            onCloseCallback={(AlertState) => {
                                onCloseCallback(AlertState);
                            }}
                        />
                    </div>
                ) : null}
                <br />
                {children}
            </Comp>
        </>
    );
};

export default HOCLoader;
