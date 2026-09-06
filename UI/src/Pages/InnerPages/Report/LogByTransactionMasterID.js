import React, { useEffect, useState } from 'react';
import { Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';

import { SearchByID as SELECT_BY_ID_LOG } from 'Components/Api/ProviderReqestResponceApi';
import { SearchByID as SELECT_CALLBACK_BY_ID } from 'Components/Api/ProviderCallBackLogApi';
import { HeaderLog } from 'Components/SummaryCards/HeaderLog';
import ReactTable from 'Components/ReactTable/ReactTable';

export const SelectByIDLog = ({
    TransactionMasterID,
    ConsumerNumber,
    Amount,
    setLoading
}) => {
    const [ProviderLog, setProviderLog] = useState([]);
    const [ProviderCallbackLog, setProviderCallbackLog] = useState([]);
    const [LogColumns, setLogColumns] = useState([]);
    const [CallBackColumns, setCallBackColumns] = useState([]);
    const [headerlog, setheaderlog] = useState({
        TransactionMasterID: '',
        ConsumerNumber: '',
        Amount
    });
    const SearchByID = async () => {
        setheaderlog({
            TransactionMasterID: TransactionMasterID,
            ConsumerNumber: ConsumerNumber,
            Amount: Amount
        });

        let JSONObj = {
            ID: TransactionMasterID
        };
        createCallBackColumns();
        createLogColumns();
        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID_LOG(request);
        const callbackresponse = await SELECT_CALLBACK_BY_ID(request);
        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setProviderLog(data);
        var callbackdata = JSON.parse(callbackresponse.data);
        setProviderCallbackLog(callbackdata);
    };

    useEffect(() => {
        setLoading(true);
        SearchByID();
    }, [TransactionMasterID]);

    const createCallBackColumns = () => {
        var columns = [];
        columns.push({
            label: 'Date',
            name: 'DOC'
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'CallBackResponse',
            name: 'ProviderReply',
            ReadOnlyTextArea: true
        });

        setCallBackColumns(columns);
    };

    const createLogColumns = () => {
        var columns = [];
        columns.push({
            label: 'Date',
            name: 'DOC',
            setCellHeaderProps: () => ({
                style: { width: '5%' }
            })
        });
        columns.push({
            label: 'Provider',
            name: 'ProviderName',
            setCellHeaderProps: () => ({
                style: { width: '5%' }
            })
        });
        columns.push({
            label: 'Request',
            name: 'Request',
            ReadOnlyTextArea: true,
            setCellHeaderProps: () => ({
                style: { width: '45%' }
            })
        });
        columns.push({
            label: 'InitialResponse',
            name: 'Response',
            ReadOnlyTextArea: true,
            setCellHeaderProps: () => ({
                style: { width: '45%' }
            })
        });
        setLogColumns(columns);
    };

    return (
        <Grid container>
            <Grid item xs={12} sm={12} md={12} lg={12}>
                <HeaderLog headerLog={headerlog} />
                <ReactTable
                    data={ProviderLog}
                    columns={LogColumns}
                    headerToolbar={false}
                    footerToolbar={false}
                    rowsPerPage={4}
                />
                <br />

                <ReactTable
                    data={ProviderCallbackLog}
                    columns={CallBackColumns}
                    headerToolbar={false}
                    footerToolbar={false}
                    rowsPerPage={4}
                />
            </Grid>
        </Grid>
    );
};
