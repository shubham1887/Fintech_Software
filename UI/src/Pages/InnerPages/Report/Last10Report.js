import React, { useState, useEffect } from 'react';
import { SelectForLast10 as TRANSACTION_LAST10 } from 'Components/Api/TransactionReportAPI';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import LinearProgress from '@material-ui/core/LinearProgress';
import { DMTTransactionPrint } from 'Components/Print/DMTPrint';
import { GetDMTTransactionStatus as TRANSACTION_STATUS } from 'Components/Api/TransactionAPI';
import { GetTransactionRefund } from 'Components/Api/TransactionAPI';
import PrintConfirmation from 'Pages/InnerPages/Report/PrintConfirmation';
import { SelectDMTForReceiptForWeb as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { OTPModal } from 'Components/Modal/OTPModal';
var receivedID = 0;
export const Last10Report = ({
    ServiceID,
    setIsError,
    setSeverity,
    setLoading,
    setResponseMessage,
    formRef
}) => {
    const classes = commonStyle();
    const [transactions, setTransactions] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    receivedID = ServiceID;
    const [masterID, setMasterID] = useState(0);
    const [isPrint, setIsPrint] = useState(false);
    const [completed, setCompleted] = useState(0);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [isChargeOpen, setIsChargeOpen] = useState(false);
    const [printConfirmationRequest, setPrintConfirmationRequest] = useState(
        {}
    );
    const [TransactionMasterID, setTransactionMasterID] = useState(0);
    const [openOTP, setOpenOTP] = useState(false);
    const [printData, setPrintData] = useState({});
    const Select = async () => {
        if (receivedID <= 0) return;

        createColumns();

        setTransactions([]);

        const modelObj = {
            ServiceID: receivedID
        };

        progress();
        var requestObj = JSON.stringify(modelObj);
        setLoadingOpen(true);
        var response = await TRANSACTION_LAST10(requestObj);

        if (!response.isAuthorized) LOGOUT();
        setLoadingOpen(false);
        if (!response.status) {
            return;
        }

        var data = JSON.parse(response.data);

        setTransactions(data);
    };

    const progress = () => {
        setCompleted((oldCompleted) => {
            if (oldCompleted === 100) return 0;
            const diff = Math.random() * 10;
            return Math.min(oldCompleted + diff, 100);
        });
    };

    useEffect(() => {
        Select();
    }, [receivedID]);

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
            label: 'Beneficiary Name',
            name: 'BeneName',
            textwrapwidth: 100,
            textwrap: true
        });

        columns.push({
            label: 'Account Number.',
            name: 'ConsumerNumber'
        });

        columns.push({
            label: 'Op Bal',
            name: 'OpeningBalance',
            textwrapwidth: 70,
            numeric: true
        });

        columns.push({
            label: 'Amount',
            name: 'Amount',
            textwrapwidth: 60,
            numeric: true
        });

        columns.push({
            label: 'Debit',
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
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'Txn ID',
            name: 'ReferenceNo'
        });

        columns.push({
            label: 'Payment Mode',
            name: 'PaymentType'
        });

        columns.push({
            label: 'Sender',
            name: 'SenderNo'
        });

        var options = [
            {
                Title: 'Print',
                disabledOn: ['FAILURE'],
                statusIndex: 8,
                Callback: (values) => {
                    displayCharge({ ID: values[0], Amount: values[5] });
                }
            }
        ];

        options.push({
            Title: 'Check',
            statusIndex: 8,
            disabledOn: ['FAILURE', 'SUCCESS'],
            Callback: (values) => {
                StatusCheck(values[0]);
            }
        });

        columns.push({
            label: 'Action',
            name: 'TransactionMasterID',
            MoreMenu: true,
            options: options
        });

        setMainColumns(columns);
    };

    const StatusCheck = async (request) => {
        var inputObj = {
            ID: request
        };

        var requestObj = JSON.stringify(inputObj);

        setIsError(false);
        setLoading(true);

        var response = await TRANSACTION_STATUS(requestObj);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        receivedID = 0;
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

        if (data.Description === 'OTP sent to customer mobile') {
            setOpenOTP(true);
            setTransactionMasterID(request);
        }

        if (formRef !== undefined && formRef.current) {
            formRef.current.handleSubmit();
        }

        if (data.Status !== 'Ok') {
            setIsError(true);
            setSeverity('error');
            return;
        }
        Select();
    };
    const CustomOtpModal = openOTP ? (
        <OTPModal
            isOpen={openOTP}
            title="OTP"
            error={setResponseMessage}
            submitCallback={(value) => {
                GetTransactionRefundByOTP(value);
            }}
            resendCallback={(value) => {}}
            onDismissCallback={(value) => {}}
        />
    ) : null;

    const GetTransactionRefundByOTP = async (otpObj) => {
        setLoading(true);
        setOpenOTP(false);

        const { OTP } = otpObj;
        let RequestObj = {
            OTP: OTP,
            TransactionMasterID: TransactionMasterID,
            ID: TransactionMasterID
        };

        var response = await GetTransactionRefund(JSON.stringify(RequestObj));
        setLoading(false);

        setResponseMessage(response.data);
        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            setOpenOTP(true);
            return;
        }
        let data = JSON.parse(response.data);

        setIsError(true);
        setSeverity('success');
        setResponseMessage(data.Message);
    };
    const SelectPrintData = async (TransactionMasterID, Charge) => {
        var req = {
            ID: TransactionMasterID
        };
        var request = JSON.stringify(req);

        const resp = await TRANSACTION_RECEIPT(request);

        if (!resp.isAuthorized) LOGOUT();

        if (!resp.status) return;

        var data = JSON.parse(resp.data);

        data.Charge =
            Charge === undefined || Charge === null ? data.Charge : Charge;

        setPrintData(data);

        setIsPrint(true);
    };

    const displayCharge = (values) => {
        setPrintConfirmationRequest(values);
        updateCustomCharge(true);
    };

    const Print = ({ ID, Charge }) => {
        SelectPrintData(ID, Charge);
    };
    const updateCustomCharge = (flag) => {
        setIsChargeOpen(flag);
    };
    return (
        <>
            <h2 className={classes.PaperHead_h2}>Last 10 DMT Transaction</h2>
            {loadingOpen ? (
                <LinearProgress variant="determinate" value={completed} />
            ) : null}
            <ReactTable data={transactions} columns={mainColumns} />

            {CustomOtpModal}

            {isPrint ? (
                <DMTTransactionPrint
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
            ) : null}

            {isChargeOpen ? (
                <PrintConfirmation
                    isOpen={isChargeOpen}
                    title="Do you want print ?"
                    message="Are you sure to get this print?"
                    MasterID={masterID}
                    request={printConfirmationRequest}
                    positiveButtonClick={(request) => {
                        setIsChargeOpen(false);

                        Print(request);
                    }}
                    negativeButtonClick={(value) => {
                        setIsChargeOpen(false);
                    }}
                />
            ) : null}
        </>
    );
};
