import React, { useState, useEffect, useRef } from 'react';
import { Formik, Form } from 'formik';
import { Container } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { useHistory } from 'react-router';
import { Initiate as INSURANCE_INIT } from 'Components/Api/InsuranceAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const FingpayInsuranceInitiate = () => {
    const formRef = useRef();
    const classes = commonStyle();
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [URL, setURL] = useState('');

    const history = useHistory();

    useEffect(() => {
        setLoadingOpen(true);
        Initiate();
    }, []);

    const Initiate = async () => {
        if (loadingOpen) {
            setLoadingOpen(false);
            return;
        }
        setIsError(false);
        var response = await INSURANCE_INIT();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        setSeverity('success');
        let data = JSON.parse(response.data);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(data.Message);

            setIsError(true);
            return;
        }
        setResponseMessage(data.Message);
        setURL(data.RedirectURL);

    };
    useEffect(() => {
        if (URL) {
            formRef.current.submit();
        }
    }, [URL]);

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <form ref={formRef} method="post" action={URL} target="_blank">
                <input
                    type="submit"
                    value="Redirect"
                    style={{ display: 'none' }}
                />
            </form>
        </CustomContainer>
    );
};
