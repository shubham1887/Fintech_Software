import React, { useState, useEffect } from 'react';
import { SelectForLast10 as TRANSACTION_LAST10 } from 'Components/Api/TransactionReportAPI';
import { AEPSPrint } from 'Components/Print/AEPSPrint';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import LinearProgress from '@material-ui/core/LinearProgress';
import { TransactionPrint } from 'Components/Print/RechargePrint';
import { SelectForReceipt as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { AEPS_CODE } from 'Components/Helper/Messages';
import PrintConfirmation from 'Pages/InnerPages/Report/PrintConfirmation';
import { BBPSPrint } from 'Components/Print/BBPSPrint';

var receivedID = 0;
export const Last10BBPSReport = ({ ServiceID, refreshRandom }) => {
    receivedID = parseInt(ServiceID);

    const [isPrint, setIsPrint] = useState(false);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [printData, setPrintData] = useState({});
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [isChargeOpen, setIsChargeOpen] = useState(false);
    const [printConfirmationRequest, setPrintConfirmationRequest] = useState(
        {}
    );


    const Select = async () => {
        if (receivedID <= 0) return;
        createColumns();

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

    const updateCustomCharge = (flag) => {
        setIsChargeOpen(flag);
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


    const Print = ({ ID }) => {
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

        var d = JSON.parse(data);

        setPrintData(d);
        setIsPrint(true);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'TransactionMasterID',
            srno: true
        });

        columns.push({
            label: 'Date & Time',
            name: 'DOC'
        });

        columns.push({
            label: 'Transaction Type',
            name: 'OperatorName'
        });
        columns.push({
            label: 'Image',
            name: 'Logo',
            Image: true
        });

        columns.push({
            label: 'Mobile Number',
            name: 'ConsumerNumber'
        });
        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'Op Bal',
            name: 'OpeningBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Amount (₹)',
            label: 'Amount(₹)',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Total Amount (₹)',
            name: 'DebitAmount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Cl Bal',
            name: 'ClosingBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Reference ID',
            label: 'Operator Ref.ID',
            name: 'ReferenceNo'
        });



        setMainColumns(columns);
    };

    return (
        <>
            {loadingOpen ? (
                <LinearProgress variant="determinate" value={completed} />
            ) : null}
            <ReactTable
                data={transactions}
                columns={mainColumns}
                headerToolbar={false}
                footerToolbar={false}
            />



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

                        <BBPSPrint
                            isOpen={isPrint}
                            title="Transaction Print"
                            request={printData}
                            positiveButtonClick={(printData) => {
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
