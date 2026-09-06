import React, { useEffect, useState } from 'react';
import { SelectRedirectUrlByName as DEMAT_LOGIN } from 'Components/Api/RedirectUrlAPI';

import { Container } from '@material-ui/core';
import { useHistory } from 'react-router';

import HOCLoader from 'Components/Loader/HOCLoader';
import ReactTable from 'Components/ReactTable/ReactTable';

const CustomContainer = HOCLoader(Container);

export const Loan = () => {
    const history = useHistory();
    const [itemList, setItemList] = useState([]);

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    const [mainColumns, setMainColumns] = useState([]);

    const SelectURL = async () => {
        var requestObj = { Name: 'DealsOffLoan' };
        setLoadingOpen(true);
        var request = JSON.stringify(requestObj);

        setIsError(false);

        var response = await DEMAT_LOGIN(request);
        setLoadingOpen(false);

        setIsError(true);

        setSeverity('success');
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        let data = JSON.parse(response.data);

        let param = {
            RedirectURL: data.Url
        };

        history.push('/LoanRedirect', param);
        return;
    };

    useEffect(() => {
        SelectURL();
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
        >
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
