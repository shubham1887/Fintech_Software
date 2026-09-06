import React, { useState, useEffect, useRef } from 'react';

import { Container, Button } from '@material-ui/core';
import { useHistory } from 'react-router';
import { LOGOUT } from 'ConstComponents';
import { SelectURL as SELECT_URL } from 'Components/Api/ThirdPartyServiceProviderAPI';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const InstaProvider = (props) => {
    const classes = commonStyle();
    const [CustomContainerProps, setCustomContainerProps] = useState({
        loadingOpen: false,
        isError: false,
        severity: 'success',
        responseMessage: ''
    });
    const formRef = useRef();
    const history = useHistory();
    const initialValues = {
        URL: '',
        DomainURL: ''
    };

    const [FormObj, setFormObj] = useState(initialValues);

    const selectURL = async () => {
        setCustomContainerProps({
            isError: false,
            loading: true
        });

        const response = await SELECT_URL();

        if (!response.isAuthorized) LOGOUT();

        setCustomContainerProps({
            isError: false,
            loading: false
        });

        if (!response.status) {
            setCustomContainerProps({
                isError: true,
                severity: 'error',
                responseMessage: response.data
            });
            return;
        }

        var data = JSON.parse(response.data);

        setFormObj({ ...FormObj, URL: data.URL, DomainURL: data.DomainURL });
        formRef.current.submit();
    };

    useEffect(() => {
        // selectURL();
    }, []);

    return (
        <CustomContainer
            isLoading={CustomContainerProps.loading}
            isError={CustomContainerProps.isError}
            alertMessage={CustomContainerProps.responseMessage}
            severity={CustomContainerProps.severity}
            onCloseCallback={(alertState) => {
                setCustomContainerProps({
                    isError: alertState
                });
            }}
        >
            <form
                ref={formRef}
                method="post"
                action={FormObj.DomainURL}
                target="_blank"
            >
                <input
                    type="hidden"
                    id="parameters"
                    name="params"
                    value={FormObj.URL}
                />
                <input
                    type="submit"
                    style={{ display: 'none' }}
                    value="Redirect"
                />

                <Button
                    variant="contained"
                    color="primary"
                    onClick={() => {
                        selectURL();
                    }}
                >
                    Go to AEPS Txn
                </Button>
            </form>
            <h3
                style={{
                    marginTop: '10px',
                    textAlign: 'center',
                    display: 'none'
                }}
            >
                Please wait, page is redirecting to Third Party Service...
            </h3>
            <br />
            <br />
            <br />
            <label>
                <h3 className={classes.Provider_Format}>
                    Change the mobile number of the Outlet for InstantPay
                </h3>
            </label>
            <br />
            <Button
                variant="contained"
                color="primary"
                onClick={() => {
                    history.push('/IPChangeMobile');
                }}
            >
                Change Mobile Number
            </Button>
        </CustomContainer>
    );
};
