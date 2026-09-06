import React, { useContext, useState, useEffect } from 'react';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import { ViewBeneficiary as BENE_SELECT } from 'Components/Api/YesBankDMTAPI';
import { DeleteBeneficiary as BENE_DELETE } from 'Components/Api/YesBankDMTAPI';
import PrintConfirmation from 'Pages/InnerPages/Report/PrintConfirmation';
import { LOGOUT } from 'ConstComponents';
import { DMTTransactionConfirmation } from 'Components/Alert/DMTTransactionConfirmation';
import { SelectDMTForReceiptForWeb as TRANSACTION_RECEIPT } from 'Components/Api/TransactionReportAPI';
import { DMTTransactionPrint } from 'Components/Print/DMTPrint';
import { MoneyTransfer as DMT_TRANSFER } from 'Components/Api/YesBankDMTAPI';
import { BalanceContext } from 'Components/BalanceContext';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import * as Yup from 'yup';
import {
    MONEY_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
    MONEY_TRANSFER_OPERATOR_VERIFY_CODE,
    PLEASE_PROVIDE_VALID
} from 'Components/Helper/Messages';
import { TransactionResponse } from 'Components/Alert/TransactionResponse';
import AddbeneficiaryIcon from 'Components/SvgButtons/AddbeneficiaryIcon';
import { Box, Button } from '@material-ui/core';
import { useHistory } from 'react-router';
import { PayoutReceipt } from 'Components/Api/PayoutBeneficiaryAPI';
var mobile = '';

export default function YesBankBeneficiaryList({
    senderNo,
    ActiveDMTName,
    senderName,
    setIsError,
    setSeverity,
    setLoading,
    setResponseMessage,
    formRef
}) {
    const { DashboardBalanceUpdate } = useContext(BalanceContext);
    const classes = commonStyle();
    const history = useHistory();
    const [mainColumns, setMainColumns] = useState([]);
    const [list, setList] = useState([]);
    const [isDeleteAlertOpen, setIsDeleteAlertOpen] = useState(false);
    const [
        confirmDefaultAlertRequest,
        setConfirmDefaultAlertRequest
    ] = useState({});
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [serviceCode, setserviceCode] = useState(MONEY_TRANSFER_CODE);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    mobile = senderNo;
    const [isPin] = useState(localStorage.getItem('IsPin'));
    const [isResponseAlertOpen, setIsResponseAlertOpen] = useState(false);
    const [ResponseAlert, setResponseAlert] = useState({});
    const [printConfirmationRequest, setPrintConfirmationRequest] = useState(
        {}
    );
    const [isChargeOpen, setIsChargeOpen] = useState(false);
    const [printData, setPrintData] = useState(null);
    const [isPrint, setIsPrint] = useState(false);

    useEffect(() => {
        select();
        createColumns();
    }, [mobile]);

    const select = async () => {
        setList([]);
        setLoading(true);
        setIsError(false);

        if (senderNo === undefined) return;

        if (senderNo === '') return;

        var beneReqObj = JSON.stringify({
            Mobile: mobile
        });
        var response = await BENE_SELECT(beneReqObj);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setIsError(true);
            setResponseMessage(response.data);
            return;
        }

        var data = JSON.parse(response.data);
        var filteredData = data.filter((bene) => bene.Status !== '0');
        setList(filteredData);
    };

    const Transfer = async (request) => {
        let jsonObj = {
            Mobile: mobile,
            Name: request.BeniName,
            SenderName: senderName,
            BankName: request.BankName,
            AccountNo: request.AccountNumber,
            IFSC: request.IFSC,
            BeneficiaryID: request.BeneficiaryID,
            SenderID: request.SenderID,
            Amount: request.Amount,
            PaymentType: request.PaymentType,
            RechargePin: request.RechargePin,
            Remark: request.Remark,
            DMTName: ActiveDMTName
        };
        setIsError(false);
        setLoading(true);

        var apirequest = JSON.stringify(jsonObj);

        var response = await DMT_TRANSFER(apirequest);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            var dataError = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(dataError.Message);

            setIsError(true);
            select();
            return;
        }

        var data = JSON.parse(response.data);

        setResponseAlert(data);
        setIsResponseAlertOpen(true);
        select();
    };

    const Verify = async (values) => {
        var jsonObj = {
            BeneficiaryID: values.BeneficiaryID,
            Name: values.BeniName,
            SenderName: senderName,
            BankName: values.BankName,
            AccountNo: values.AccountNumber,
            IFSC: values.IFSC,
            SenderID: values.SenderID,
            Mobile: mobile,
            PaymentType: 'IMPS',
            Through: 'WEB',
            IsVerify: true,
            Amount: 1,
            RechargePin: values.RechargePin,
            BankID: values.BankID,
            DMTName: ActiveDMTName
        };

        setLoading(true);
        setIsError(false);

        var request = JSON.stringify(jsonObj);

        var response = await DMT_TRANSFER(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();

        setIsError(true);

        if (!response.status) {
            var dataError = JSON.parse(response.data);
            setSeverity('error');
            setResponseMessage(dataError.Message);

            setIsError(true);
            select();
            return;
        }

        setSeverity('success');
        var data = JSON.parse(response.data);

        setResponseMessage(data.Message);
        setIsError(true);
        if (formRef.current) formRef.current.handleSubmit();

        if (data.Status === 'FAILURE') {
            setSeverity('error');
            select();
            return;
        }
    };

    const onDeleteClick = (item) => {
        setIsDeleteAlertOpen(true);
        setConfirmDefaultAlertRequest(item);
    };

    const Delete = async (values) => {
        var jsonObj = {
            BeneficiaryID: values[0],
            AccountNumber: values[3],
            IFSC: values[4],
            MobileNumber: mobile
        };
        setLoading(true);
        setIsError(false);

        var request = JSON.stringify(jsonObj);

        var response = await BENE_DELETE(request);

        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setIsError(true);
        setResponseMessage(response.data);
        if (!response.status) {
            setSeverity('error');
            select();
            return;
        }

        setSeverity('success');
        select();
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    };
    const Print = ({ ID, Charge }) => {
        SelectPrintData(ID, Charge);
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

    const MTTransferValidation = Yup.object({
        BeniName: Yup.string().required(PLEASE_PROVIDE_VALID + 'name'),
        BeneficiaryID: Yup.string().required(PLEASE_PROVIDE_VALID + 'bene'),
        SenderID: Yup.string().required(PLEASE_PROVIDE_VALID + 'sender'),
        // BankName: Yup.string().required(PLEASE_PROVIDE_VALID + 'bank'),

        AccountNumber: Yup.string()
            .required(PLEASE_PROVIDE_VALID + 'account no')
            .min(7, PLEASE_PROVIDE_VALID + 'account no')
            .max(20, PLEASE_PROVIDE_VALID + 'account no'),
        Amount: Yup.number()
            .required(PLEASE_PROVIDE_VALID + 'amount')
            .min(0, PLEASE_PROVIDE_VALID + 'amount'),
        RechargePin: Yup.string().when(['IsPin'], {
            is: (isPin) => isPin === 'true',

            then: Yup.string().required('Please enter pin')
        })
    });
    const displayCharge = (values) => {
        setPrintConfirmationRequest(values);
        updateCustomCharge(true);
    };

    const updateCustomCharge = (flag) => {
        setIsChargeOpen(flag);
    };

    const MTTransferConfirmation = async (values) => {
        setConfirmAlertRequest({});
        const inputObj = {
            Amount: values[5],
            OperatorID: MONEY_TRANSFER_OPERATOR_TRANSFER_CODE
        };

        var reqObj = JSON.stringify(inputObj);
        var response = await PayoutReceipt(reqObj);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        var result = JSON.parse(response.data);
        let SenderID = values[10];
        let RechargePin = values[6];
        if (isPin !== 'true') {
            SenderID = values[9];
            RechargePin = '';
        }
        if (RechargePin === undefined) {
            RechargePin = '';
        }
        let jsonObj = {
            BeneficiaryID: values[0],
            BeniName: values[1],
            BankName: values[2],
            AccountNumber: values[3],
            IFSC: values[4],
            Amount: values[5],
            RechargePin: RechargePin,
            SenderID: SenderID,
            OperatorID: MONEY_TRANSFER_OPERATOR_TRANSFER_CODE,
            ServiceID: MONEY_TRANSFER_CODE,
            Charge: result.Charges,
            Gst: result.Gst,
            TotalAmount: result.TotalAmount,
            TDS: result.TDS,
            Commission: result.Commission
        };
        setIsError(false);

        var errorValidation = false;

        await MTTransferValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);

            setIsError(true);
            return;
        });

        if (errorValidation === true) return;

        setIsAlertOpen(true);
        setConfirmAlertRequest(jsonObj);
    };

    const VerifyConfirmation = async (values) => {
        setConfirmAlertRequest({});

        const inputObj = {
            Amount: 1,
            OperatorID: MONEY_TRANSFER_OPERATOR_VERIFY_CODE
        };

        var reqObj = JSON.stringify(inputObj);
        var response = await PayoutReceipt(reqObj);
        if (!response.isAuthorized) LOGOUT();
        var result = JSON.parse(response.data);

        let SenderID = values[10];
        let RechargePin = values[6];
        if (isPin !== 'true') {
            SenderID = values[9];
            RechargePin = '';
        }

        if (RechargePin === undefined) {
            RechargePin = '';
        }
        let jsonObj = {
            BeneficiaryID: values[0],
            BeniName: values[1],
            BankName: values[2],
            AccountNumber: values[3],
            IFSC: values[4],
            BankID: values[12],
            Amount: 1,
            RechargePin: RechargePin,
            SenderID: SenderID,
            OperatorID: MONEY_TRANSFER_OPERATOR_VERIFY_CODE,
            ServiceID: MONEY_TRANSFER_CODE,
            Charge: result.Charges,
            Gst: result.Gst,
            TotalAmount: result.TotalAmount
        };

        var errorValidation = false;

        await MTTransferValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            return;
        });

        if (errorValidation === true) return;

        setIsAlertOpen(true);
        setConfirmAlertRequest(jsonObj);
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'BeneficiaryID',
            srno: true
        });

        columns.push({
            label: 'Beneficiary Name',
            name: 'Name'
        });
        columns.push({
            label: 'Bank Name',
            name: 'BankName',
            hide: true
        });

        columns.push({
            label: 'Account Number',
            name: 'AccountNo'
        });
        columns.push({
            label: 'IFSC',
            name: 'IFSC'
        });

        columns.push({
            label: 'Amount (₹)',
            name: 'Amount',
            editableWithNumberOnly: true,
            length: 6
        });

        if (isPin === 'true') {
            columns.push({
                label: 'Pin',
                name: 'RechargePin',
                editableWithPinOnly: true,
                length: 4
            });
        }

        columns.push({
            label: 'Verify Status',
            name: 'IsVerified',
            buttonTitle: 'Verify',
            buttonOrDMTVerify: true,
            callback: (values) => {
                VerifyConfirmation(values);
            }
        });

        columns.push({
            label: 'Action',
            name: 'BeneficiaryID',
            transfer: true,
            buttonTitle: 'Transfer',
            callback: (value) => {
                MTTransferConfirmation(value);
            }
        });

        columns.push({
            label: 'Delete',
            name: 'BeneficiaryID',
            icon: 'DeleteIcon',
            iconButtonWithRow: true,
            callback: (value) => {
                onDeleteClick(value);
            }
        });

        columns.push({
            label: 'BeneficiaryID',
            name: 'BeneficiaryID',
            hide: true
        });

        columns.push({
            label: 'SenderID',
            name: 'SenderID',
            hide: true
        });

        columns.push({
            label: 'BankID',
            name: 'BankID',
            hide: true
        });
        setMainColumns(columns);
    };

    const sendCallback = (data) => {
        setIsResponseAlertOpen(false);

        setSeverity('success');
        if (data.Status === 'FAILURE') {
            setSeverity('error');
        }
        setResponseMessage(data.Message);
        setIsError(true);
    };

    if (senderNo === undefined || senderNo === null || senderNo.length <= 5) {
        return null;
    }

    return (
        <>
            <h2 className={classes.PaperHead_h2}>Beneficiaries</h2>
            <Box style={{ position: 'absolute', right: '24px' }}>
                <Button
                    style={{
                        padding: '3px 10px',
                        background: '#ff0000',
                        color: '#fff',
                        margin: '-71px 0 0 0'
                    }}
                    title="Add Beneficiary"
                    endIcon={<AddbeneficiaryIcon />}
                    onClick={(e) => {
                        let params = {
                            //Mobile: mobile,
                            DMTName: ActiveDMTName
                        };
                        history.push('/YBDMT_BeneficiaryAdd', params);
                    }}
                >
                    Add Beneficiary
                </Button>
            </Box>

            <ReactTable data={list} columns={mainColumns} />

            {isAlertOpen && (
                <DMTTransactionConfirmation
                    isOpen={isAlertOpen}
                    ActiveDMTName={ActiveDMTName}
                    title="Transfer"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        if (
                            parseInt(request.ServiceID) ===
                                MONEY_TRANSFER_CODE &&
                            parseInt(request.OperatorID) ===
                                MONEY_TRANSFER_OPERATOR_TRANSFER_CODE
                        ) {
                            Transfer(request);
                        }

                        if (
                            parseInt(request.ServiceID) ===
                                MONEY_TRANSFER_CODE &&
                            parseInt(request.OperatorID) ===
                                MONEY_TRANSFER_OPERATOR_VERIFY_CODE
                        ) {
                            Verify(request);
                        }
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                        select();
                    }}
                />
            )}

            {isResponseAlertOpen && (
                <TransactionResponse
                    isOpen={isResponseAlertOpen}
                    isPrint={true}
                    title={ResponseAlert.Status}
                    ServiceID={serviceCode}
                    request={ResponseAlert}
                    OkButtonClick={(request) => {
                        sendCallback(ResponseAlert);
                    }}
                    PrintButtonClick={() => {
                        setIsResponseAlertOpen(false);
                        Print({
                            ...ResponseAlert,
                            ID: ResponseAlert.VendorID
                        });
                    }}
                />
            )}

            {isPrint && (
                <DMTTransactionPrint
                    isOpen={isPrint}
                    title="Transaction Print"
                    request={printData}
                    positiveButtonClick={() => {
                        setIsPrint(false);
                        DashboardBalanceUpdate();
                    }}
                    negativeButtonClick={() => {
                        setIsPrint(false);
                    }}
                />
            )}

            {isDeleteAlertOpen && (
                <ConfirmAlert
                    isOpen={isDeleteAlertOpen}
                    title="Confirm"
                    message={`Do you want to Delete the "${confirmDefaultAlertRequest[1]}" from the list of beneficiaries`}
                    request={confirmDefaultAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsDeleteAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsDeleteAlertOpen(false);
                    }}
                />
            )}
        </>
    );
}
