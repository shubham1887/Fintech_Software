import React from 'react';
import Backdrop from '@material-ui/core/Backdrop';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import Loader from 'Components/Loader/Loader';
import { CustomeAlert } from 'Components/Alert/Alert';

const HOCModal = (Comp) => ({
    isLoading,
    isError,
    alertMessage,
    severity,
    onCloseCallback,
    children,
    ...props
}) => {
    const classes = commonStyle();

    return (
        <>
            <Backdrop className={classes.backdrop} open={isLoading}>
                <Loader />
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

                {children}
            </Comp>
        </>
    );
};

export default HOCModal;
