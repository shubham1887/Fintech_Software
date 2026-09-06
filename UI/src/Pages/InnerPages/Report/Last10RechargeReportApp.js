import React, { useState, useEffect } from 'react';
import { SelectForLast10 as TRANSACTION_LAST10 } from 'Components/Api/TransactionReportAPI';
import { AEPSPrint } from 'Components/Print/AEPSPrint';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import LinearProgress from '@material-ui/core/LinearProgress';
import { TransactionPrint } from 'Components/Print/RechargePrint';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { AEPS_CODE } from 'Components/Helper/Messages';
import { Container, Grid, Icon, Paper, Divider } from '@material-ui/core';

import Success from 'Assets/Images/success.png';
import Fail from 'Assets/Images/fail.png';

var receivedID = 0;
export const Last10RechargeReportAPP = ({ ServiceID, refreshRandom }) => {
    receivedID = parseInt(ServiceID);

    const [isPrint, setIsPrint] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [printData, setPrintData] = useState({});
    const [transactions, setTransactions] = useState([]);
    const Select = async () => {
        if (receivedID <= 0) return;

        setTransactions([]);
        const modelObj = {
            ServiceID: receivedID
        };

        var requestObj = JSON.stringify(modelObj);
        setLoadingOpen(true);
        progress();
        var response = await TRANSACTION_LAST10(requestObj);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setTransactions(data);
    };

    const [completed, setCompleted] = useState(0);
    const progress = () => {
        setCompleted((oldCompleted) => {
            if (oldCompleted === 100) return 0;

            const diff = Math.random() * 10;
            return Math.min(oldCompleted + diff, 100);
        });
    };

    useEffect(() => {
        Select();
    }, [receivedID, refreshRandom]);

    const Print = (ID) => {
        SelectPrintData(ID);
    };

    const SelectPrintData = async (TransactionMasterID) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await TRANSACTION_RECEIPT(
            request
        );
        setLoadingOpen(false);
        if (!isAuthorized) LOGOUT();

        // if (!status) return;

        var d = JSON.parse(data);
        setPrintData(d);

        setIsPrint(true);
    };

    const GetImageByStatus = (status) => {
        if (status === 'SUCCESS') {
            return <img src={Success} height={20} width={20} alt="success" />;
        } else if (status === 'FAILURE') {
            return <img src={Fail} height={20} width={20} alt="fail" />;
        }
    };

    const GetOperatorImage = (path, OperatorName) => {
        return <img src={path} height={20} width={20} alt={OperatorName} />;
    };

    return (
        <>
            {loadingOpen ? (
                <LinearProgress variant="determinate" value={completed} />
            ) : null}

            <Grid
                item
                xs={12}
                sm={12}
                md={12}
                lg={12}
                style={{ marginTop: '10px' }}
            >
                <Grid container spacing={1}>
                    <Grid item xs={12} sm={12} md={12} lg={12}>
                        {transactions.map((txn, index) => (
                            <Paper>
                                <Grid
                                    xs={12}
                                    sm={12}
                                    md={12}
                                    lg={12}
                                    style={{ margin: '5%' }}
                                >
                                    <Grid container spacing={2}>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            <div
                                                style={{
                                                    whiteSpace: 'pre-wrap',
                                                    lineHeight: 1.8,
                                                    fontSize: 8
                                                }}
                                            >
                                                TxnID: {txn.ReferenceNo}
                                            </div>
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            <div
                                                style={{
                                                    whiteSpace: 'pre-wrap',
                                                    lineHeight: 1.8,
                                                    fontSize: 8
                                                }}
                                            >
                                                {txn.DOC}
                                            </div>
                                        </Grid>
                                    </Grid>
                                    <Divider />
                                    <Grid container spacing={2}>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            {GetOperatorImage(
                                                txn.Logo,
                                                txn.OperatorName
                                            )}
                                            {txn.OperatorName}
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            {txn.ConsumerNumber}
                                        </Grid>
                                    </Grid>
                                    <Grid container spacing={2}>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            Op.Bal: {txn.OpeningBalance}
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            Amount: {txn.Amount}
                                        </Grid>
                                    </Grid>
                                    <Grid container spacing={2}>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            Comm: {txn.RetailerCommission}
                                        </Grid>
                                        <Grid item xs={6} sm={6} md={6} lg={6}>
                                            Cl.Bal: {txn.ClosingBalance}
                                        </Grid>
                                    </Grid>
                                    {GetImageByStatus(txn.Status)} {txn.Status}
                                </Grid>
                            </Paper>
                        ))}
                    </Grid>
                </Grid>
            </Grid>

            {isPrint ? (
                <>
                    {receivedID === AEPS_CODE ? (
                        <AEPSPrint
                            isOpen={isPrint}
                            title="AEPS Receipt"
                            request={printData}
                            positiveButtonClick={() => {
                                setIsPrint(false);
                            }}
                            negativeButtonClick={() => {
                                setIsPrint(false);
                            }}
                        />
                    ) : (
                        <TransactionPrint
                            isOpen={isPrint}
                            title="Transaction Print"
                            request={printData}
                            positiveButtonClick={() => {
                                setIsPrint(false);
                            }}
                            negativeButtonClick={() => {
                                setIsPrint(false);
                            }}
                        />
                    )}
                </>
            ) : null}
        </>
    );
};
