import React, { useState, useEffect } from 'react';
import { Container } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { SelectResendDMTTransaction as SELECT_RESEND_DMT_TRANSACTION } from 'Components/Api/TransactionReportAPI';
import { DMTTransactionResend as TRANSACTION_RESEND } from 'Components/Api/TransactionAPI';
import { DMTTransactionRefund as TRANSACTION_RFEUND } from 'Components/Api/TransactionAPI';
import ReactTable from 'Components/ReactTable/ReactTable';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const DMTResendWallet = () => {
    const classes = commonStyle();

    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [itemList, setItemList] = useState([]);

    const [mainColumns, setMainColumns] = useState([]);

    const selectDMTResendTransaction = async () => {
        setLoadingOpen(true);
        const response = await SELECT_RESEND_DMT_TRANSACTION();
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }
        var data = JSON.parse(response.data);
        setItemList(data);
    };

    const ResendTransaction = async (request) => {
        var inputObj = {
            ID: request
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);

        var response = await TRANSACTION_RESEND(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);

        if (!data.Status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
    };

    const RefundTransaction = async (request) => {
        var inputObj = {
            ID: request
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoadingOpen(true);

        var response = await TRANSACTION_RFEUND(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);
        setSeverity('success');
        setResponseMessage(response.data);

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(response.data);
        setResponseMessage(data.Message);

        if (!data.Status) {
            setIsError(true);
            setSeverity('error');
            return;
        }
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'Date',
            name: 'DOC'
        });

        columns.push({
            label: 'Name',
            name: 'FirmName'
        });
        columns.push({
            label: 'Number',
            name: 'ConsumerNumber'
        });

        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'ProviderRequestID',
            name: 'ProviderRequestID'
        });
        columns.push({
            label: 'SenderNo',
            name: 'SenderNo'
        });
        columns.push({
            label: 'Bank',
            name: 'Bank'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });
        columns.push({
            label: 'Amount',
            name: 'Amount'
        });
        columns.push({
            label: 'Resend',
            name: 'TransactionMasterID',
            button: true,
            buttonTitle: 'Resend',
            callback: (value) => {
                ResendTransaction(value);
            }
        });
        columns.push({
            label: 'Refund',
            name: 'TransactionMasterID',
            buttonTitle: 'Refund',
            button: true,
            callback: (value) => {
                RefundTransaction(value);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        createColumns();
        selectDMTResendTransaction();
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
            <h2 className={classes.PaperHead_h2}>Resend DMT Transaction</h2>
            <ReactTable data={itemList} columns={mainColumns} />
        </CustomContainer>
    );
};
