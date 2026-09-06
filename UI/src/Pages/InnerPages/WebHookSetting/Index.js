import React, { useState, useEffect } from 'react';
import { Container, Typography, Grid } from '@material-ui/core';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { LOGOUT } from 'ConstComponents';
import * as Yup from 'yup';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import {
    Select as SETTING_SELECT,
    Isactivate as IS_ACTIVATE,
    Update as SETTING_UPDATE
} from 'Components/Api/WebHookSettingAPI';
import ReactTable from 'Components/ReactTable/ReactTable';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);

export const WebHookSetting = () => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const selectData = async () => {
        const response = await SETTING_SELECT();
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
            WebHookSettingID: row[0],
            URL: row[2],
            MethodType: row[3],
            HeaderJson: row[4]
        };

        var request = JSON.stringify(jsonObj);

        var errorValidation = false;

        await WebhookValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            return;
        });

        if (errorValidation === true) return;

        setIsError(false);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await SETTING_UPDATE(request);

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
    const WebhookValidation = Yup.object({
        URL: Yup.string().required(PLEASE_PROVIDE_VALID + 'URL'),
        MethodType: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Method(Get/Post)'
        ),
        HeaderJson: Yup.string().required(PLEASE_PROVIDE_VALID + 'HeaderJson')
    });

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'WebHookSettingID',
            srno: true
        });
        columns.push({
            label: 'Type',
            name: 'Type'
        });

        columns.push({
            label: 'URL',
            name: 'URL',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });
        columns.push({
            label: 'MethodType',
            name: 'MethodType',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });

        columns.push({
            label: 'HeaderJson',
            name: 'HeaderJson',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });

        columns.push({
            label: 'IsActive',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                Isactivate(id, value);
            }
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

    const Isactivate = async (item, value) => {
        var jsonObj = {
            WebHookSettingID: item,
            IsActive: value
        };
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await IS_ACTIVATE(request);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        selectData();
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
            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
            />

            <Grid container spacing={2}>
                <Grid item xs={12} sm={12} md={12} lg={12}>
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong>Param</strong> :
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong>FundTransfer</strong> :
                            <span className={classes.normalTextColor}>
                                [SenderMobileNumber], [ReceiverMobileNumber],
                                [Wallet], [Amount], [Remark], [UTRNumber]
                            </span>
                        </h3>
                    </label>
                    <br />
                    <label>
                        <h3 className={classes.Provider_Format}>
                            <strong>Signup</strong> :
                            <span className={classes.normalTextColor}>
                                [FirmName], [PersonName], [MobileNumber],
                                [EmailId], [State], [Address], [Pincode],
                                [GSTNo], [PANNo], [AadharNo],
                                [WhatsAppNumber],[ParentNo]
                            </span>
                        </h3>
                    </label>
                </Grid>
            </Grid>
        </CustomContainer>
    );
};
