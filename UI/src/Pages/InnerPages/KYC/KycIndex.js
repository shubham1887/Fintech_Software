import React, { useEffect, useState } from 'react';
import { LOGOUT } from 'ConstComponents';
import { useHistory } from 'react-router';
import { SelectActiveeKYC as SELECT_ACTIVE_EKYC } from 'Components/Api/eKYCSettingAPI';
import { Container } from '@material-ui/core';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const KycIndex = () => {
    const history = useHistory();

    const [severity, setSeverity] = useState('success');
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const selectActiveeKYC = async () => {

        history.push('/YesbankeKYC');
    };

    useEffect(() => {
        selectActiveeKYC();
    }, []);

    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        ></CustomContainer>
    );
};
