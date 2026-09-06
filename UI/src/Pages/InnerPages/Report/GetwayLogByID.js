import React, { useEffect, useState } from 'react';
import { Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import { SelectByOrderID as SELECT_BY_ID_LOG } from 'Components/Api/PaymentGatewayLogAPI';
import { HeaderGetwayLog } from 'Components/SummaryCards/HeaderGetwayLog';
import ReactTable from 'Components/ReactTable/ReactTable';

export const SelectByIDLog = ({ OrderID, UserName, Amount, setLoading }) => {
    const [ProviderLog, setProviderLog] = useState([]);
    const [LogColumns, setLogColumns] = useState([]);
    const [headerlog, setheaderlog] = useState({
        OrderID: '',
        UserName: '',
        Amount
    });
    const SearchByID = async () => {
        setheaderlog({
            OrderID: OrderID,
            UserName: UserName,
            Amount: Amount
        });

        let JSONObj = {
            OrderID: OrderID
        };
        createLogColumns();
        var request = JSON.stringify(JSONObj);

        const response = await SELECT_BY_ID_LOG(request);
        setLoading(false);
        if (!response.isAuthorized) {
            LOGOUT();
        }
        if (!response.status) {
            return;
        }
        var data = JSON.parse(response.data);
        setProviderLog(data);
    };

    useEffect(() => {
        setLoading(true);
        SearchByID();
    }, [OrderID]);

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
            label: 'Gateway Name',
            name: 'GatewayName',
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
            label: 'Response',
            name: 'Response',
            ReadOnlyTextArea: true,
            setCellHeaderProps: () => ({
                style: { width: '45%' }
            })
        });
        setLogColumns(columns);
    };

    return (
        <Grid item xs={12}>
            <HeaderGetwayLog headerLog={headerlog} />
            <ReactTable
                data={ProviderLog}
                columns={LogColumns}
                headerToolbar={false}
                footerToolbar={false}
                rowsPerPage={4}
            />
        </Grid>
    );
};
