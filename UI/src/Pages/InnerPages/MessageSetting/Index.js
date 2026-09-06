import React, { useState, useEffect, useRef } from 'react';
import { Container, Typography, Grid, Box } from '@material-ui/core';
import { Formik } from 'formik';

import { LOGOUT } from 'ConstComponents';

import {
    Select as MESSAGESETTING_SELECT,
    IsSMSactivate as ISSMS_ACTIVATE,
    IsOTPactivate as ISOTP_ACTIVATE,
    IsMailactivate as ISMAIL_ACTIVATE,
    Update as MESSAGE_SETTING_UPDATE,
    IsWhatsUpActiveDeactive as ISWHATSUP_ACTIVATE,
    SelectForFilter as SELECTALL
} from 'Components/Api/MessageSettingApi';

import ReactTable from 'Components/ReactTable/ReactTable';
import Icon from '@material-ui/core/Icon';
import excel from 'Assets/Images/excel.svg';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const MessageSetting = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(false);
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [ModelObj] = useState({
        ToDate: new Date(),
        FromDate: new Date(),
        Type: '',
        SmsMessage: '',
        TemplateID: '',
        WhatsUpMessage: '',
        Search: '',
        IsExcelDownload: false,
        Criteria: 0,
        CriteriaText: '',
        IsMessageSettingReport: false
    });

    const selectData = async () => {
        const response = await MESSAGESETTING_SELECT();
        setLoadingOpen(false);

        if (!response.isAuthorized) {
            LOGOUT();
        }

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }

        var data = JSON.parse(response.data);

        setItemList(data);
    };
    const UpdateClick = async (row) => {
        let jsonObj = {
            MessageSettingID: row[0],
            SmsMessage: row[3],
            TemplateID: row[4],
            EmailMessage: row[6],
            WhatsUpMessage: row[8]
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await MESSAGE_SETTING_UPDATE(
            request
        );

        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(data);
        setIsError(true);
        if (!status) {
            setSeverity('error');
            return;
        }

        selectData();
    };

    const IsOTPactivate = async (item, value) => {
        var jsonObj = {
            MessageSettingID: item,
            ISOTP: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ISOTP_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const IsSMSactivate = async (item, value) => {
        var jsonObj = {
            MessageSettingID: item,
            IsSMS: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ISSMS_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };
    const IsMailactivate = async (item, value) => {
        var jsonObj = {
            MessageSettingID: item,
            IsMail: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ISMAIL_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };
    const IsWhatsUpactivate = async (item, value) => {
        var jsonObj = {
            MessageSettingID: item,
            IsWhatsUp: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await ISWHATSUP_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
    };

    const submitHandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        setItemList([]);
        createColumns();
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    const callSelectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        SortDirection
    ) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            Type: formRef.current.values.Type,
            SmsMessage: formRef.current.values.SmsMessage,
            TemplateID: formRef.current.values.TemplateID,
            WhatsUpMessage: formRef.current.values.WhatsUpMessage,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection,
            FromDate: formRef.current.values.FromDate,
            ToDate: formRef.current.values.ToDate,
            Criteria: formRef.current.values.Criteria,
            CriteriaText: formRef.current.values.CriteriaText,
            IsMessageSettingReport: formRef.current.values.IsExcelDownload,
            IsExcelDownload: formRef.current.values.IsExcelDownload
        };
        setResponseMessage('');
        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);
        setLoading(true);

        var response = await SELECTALL(requestJSON);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);
            return;
        }
        var data = JSON.parse(response.data);

        if (data.IsExcelDownload)
            downloadFile(data.ReportPath, 'Message Setting');

        if (SkipCount <= 0) {
            //setHeaderSummary(data.HeaderSummary);
            setTotalCount(data.Count);
        }
        setItemList(data.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'MessageSettingID',
            srno: true
        });
        columns.push({
            label: 'Type',
            name: 'Type'
        });
        columns.push({
            label: 'ISOTP',
            name: 'ISOTP',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                IsOTPactivate(id, value);
            }
        });
        columns.push({
            label: 'IsSMS',
            name: 'IsSMS',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                IsSMSactivate(id, value);
            }
        });
        columns.push({
            label: 'SmsMessage',
            name: 'SmsMessage',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });
        columns.push({
            label: 'TemplateID',
            name: 'TemplateID',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });
        columns.push({
            label: 'IsMail',
            name: 'IsMail',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                IsMailactivate(id, value);
            }
        });
        columns.push({
            label: 'EmailMessage',
            name: 'EmailMessage',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });
        columns.push({
            label: 'IsWhatsUp',
            name: 'IsWhatsUp',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                IsWhatsUpactivate(id, value);
            }
        });

        columns.push({
            label: 'WhatsUpMessage',
            name: 'WhatsUpMessage',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });

        columns.push({
            label: 'Update',
            name: 'Update',
            save: true,
            buttonTitle: 'Update',
            callback: (row) => {
                UpdateClick(row);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectData();
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
            <Formik
                enableReinitialize={true}
                innerRef={formRef}
                initialValues={ModelObj}
                onSubmit={submitHandle}
                resetForm={ModelObj}
            >
                {({ setFieldValue, handleSubmit }) => {
                    return (
                        <Grid container spacing={2}>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <Typography>
                                    <span style={{ color: 'green' }}>
                                        <b>
                                            Param : <br />
                                            [ConsumerNumber], [AccountNo],
                                            [Amount], [Status], [TxnID],
                                            [UTRNumber], [Remark],
                                            [BeneficiaryName], [PaymentType]
                                            <br />
                                            [MobileNumber], [Name], [Pin],
                                            [OTP], [Password], [FirmName],
                                            <br />
                                            [Sender], [SenderMobileNumber],
                                            [Receiver], [ReceiverMobileNumber],
                                            <br />
                                            [OldBalance], [CurrentBalance],
                                            [DMTBalance], [Outstanding],
                                            [OldOutstanding],
                                            <br />
                                            [DateTime], [Wallet], [ToWallet],
                                            [ServiceName], [Provider],
                                            [SystemName], [Operator],[BankName]
                                        </b>
                                    </span>
                                </Typography>

                                <Grid item xs={12} sm={12} md={12} lg={12}>
                                    <Box style={{ textAlign: 'right' }}>
                                        <Icon
                                            className={
                                                classes.CustomImageButtonStyle
                                            }
                                            disabled={loading}
                                            classes={{ root: classes.iconRoot }}
                                            onClick={(e) => {
                                                setFieldValue(
                                                    'IsExcelDownload',
                                                    true
                                                );
                                                handleSubmit(e);
                                            }}
                                        >
                                            <img
                                                alt=""
                                                className={classes.imageIcon}
                                                src={excel}
                                            />
                                        </Icon>
                                    </Box>
                                </Grid>
                            </Grid>
                        </Grid>
                    );
                }}
            </Formik>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
            />
        </CustomContainer>
    );
};
