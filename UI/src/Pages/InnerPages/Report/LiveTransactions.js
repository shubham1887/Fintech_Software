import React, { useState, useEffect, memo } from 'react';
import { REFRESH_SECOND } from 'Config';

import { Grid, Container } from '@material-ui/core';
import { LiveTransactions as TRANSACTION_FILTER } from 'Components/Api/TransactionReportAPI';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { HeaderSummary } from 'Components/SummaryCards/HeaderSummary';
import { NormalMaterialSwitch } from 'Components/Inputes/NormalMaterialSwitch';
import { LOGOUT } from 'ConstComponents';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);
const LiveTransaction = () => {
    const classes = commonStyle();

    const [isPending, setIsPending] = useState(true);
    const [intervalTime, setIntervalTime] = useState(40);
    const [headerSummary, setHeaderSummary] = useState({});

    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [totalCount, setTotalCount] = useState(500);

    const [loading, setLoading] = useState(false);
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');

    useEffect(() => {
        createColumns();
        SelectTransactions(isPending);
        fetchTime();
    }, []);

    const fetchTime = async () => {
        setIntervalTime(await REFRESH_SECOND());
    };

    useEffect(() => {
        const interval = setInterval(() => {
            SelectTransactions(isPending);
        }, intervalTime * 1000);

        return () => clearInterval(interval);
    }, [isPending]);

    const SelectTransactions = async (isPendingFlag) => {
        setTransactions([]);

        let sts = 'ALL';
        if (isPendingFlag === true) {
            sts = 'PROCESS';
        }

        let inputObj = {
            TakeCount: 1000,
            SkipCount: 0,
            FromDate: new Date(),
            ToDate: new Date(),
            Status: sts,
            UserID: 0,
            ServiceID: 0,
            OperatorID: 0
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);
        const { status, data, isAuthorized } = await TRANSACTION_FILTER(
            requestObj
        );

        if (!isAuthorized) {
            LOGOUT();
        }

        setLoading(false);
        if (!status) return;

        var d = JSON.parse(data);

        setHeaderSummary(d.HeaderSummary);
        setTransactions(d.list);
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
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'ConsumerNo',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Operator',
            name: 'OperatorName'
        });

        columns.push({
            label: 'Amt',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });
        columns.push({
            label: 'ROffer',
            name: 'ROfferCommission',
            textwrapwidth: 60,
            numeric: true
        });
        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'ProcTime(In Sec.)',
            name: 'ProcTime',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Provider',
            name: 'ProviderName'
        });
        columns.push({
            label: 'OP ID',
            name: 'ReferenceNo'
        });

        columns.push({
            label: 'Circle',
            name: 'Circle'
        });
        columns.push({
            label: 'UpdateBy',
            name: 'ResponseType'
        });
        columns.push({
            label: 'AdminProfit',
            name: 'AdminProfit',
            textwrapwidth: 60,
            numeric: true
        });

        setMainColumns(columns);
    };
    return (
        <CustomContainer
            isLoading={loading}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={(alertState) => {
                setIsError(alertState);
            }}
        >
            <Grid container spacing={2}>
                <Grid item xs={7} sm={7} md={4} lg={4}>
                    <h2 className={classes.PaperHead_h2}>Live Transactions</h2>
                </Grid>
                <Grid
                    item
                    xs={5}
                    sm={5}
                    md={4}
                    lg={4}
                    style={{ marginTop: '10px' }}
                >
                    <NormalMaterialSwitch
                        label="Pending"
                        name="Pending"
                        value={isPending}
                        onCustomCallback={(value) => {
                            setIsPending(value);
                            SelectTransactions(value);
                        }}
                    />
                </Grid>
            </Grid>
            <HeaderSummary headerSummary={headerSummary} />
            <ReactTable
                rowsPerPage={100}
                data={transactions}
                columns={mainColumns}
                count={totalCount}
                headerToolbar={false}
                footerToolbar={false}
                getCallback={({ currentPage, RowsPerPage }) => {}}
                getSearchCallback={(SearchText) => {}}
                getSortCallback={({ column, direction }) => {}}
            />
        </CustomContainer>
    );
};

export default memo(LiveTransaction);
