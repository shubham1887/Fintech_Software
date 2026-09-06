import React, { useState, useEffect, memo } from 'react';
import { Container } from '@material-ui/core';
import { Grid } from '@material-ui/core';
import { LOGOUT } from 'ConstComponents';
import {
    Select as SELECT_SETTING,
    Update as SETTING_UPDATE,
    UpdateAll as SETTING_UPDATE_ALL
} from 'Components/Api/UISettingsAPI';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';

const CustomContainer = HOCLoader(Container);

const UISetting = memo(() => {
    const classes = commonStyle();
    const [itemList, setItemList] = useState([]);

    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);

    const [loadingOpen, setLoadingOpen] = useState(false);

    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const Select = async () => {
        setLoadingOpen(true);

        const { isAuthorized, status, data } = await SELECT_SETTING();
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        setItemList(JSON.parse(data));
    };

    const UpdateClick = async (row) => {
        let jsonObj = {
            UISettingMasterID: row[0],
            Key: row[1],
            Value: row[2],
            Description: row[3]
        };

        var request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const { isAuthorized, status, data } = await SETTING_UPDATE(request);

        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setIsError(true);
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }
        setSeverity('success');
        setResponseMessage(data);

        Select();
    };

    const createColumns = () => {
        let columns = [];

        columns.push({
            label: '#',
            name: 'UISettingMasterID',
            srno: true
        });

        columns.push({
            label: 'Key',
            name: 'Key'
        });

        columns.push({
            label: 'Value',
            name: 'Value',
            textwrapwidth: 100,
            editable: true,
            editableForUpdate: true
        });
        columns.push({
            label: 'Description',
            name: 'Description',
            textwrapwidth: 100
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
        Select();
    }, []);

    const processUpdatedData = (UpdatedList) => {
        setShouldGetUpdatedDataFromTable(false);
        if (!UpdatedList) {
            setSeverity('error');
            setResponseMessage('Please select row');
            setIsError(true);

            return;
        }

        // UpdatedList.forEach((obj) => {
        //
        // });

        // UpdatedList.map((item) => {
        //     var UISettingMasterID = 0;
        //     var Value = '---';

        //     item.map((subItem) => {
        //         if (subItem.column === 'UISettingMasterID') {
        //             UISettingMasterID = subItem.value;
        //         }
        //         if (subItem.column === 'Value') {
        //             Value = subItem.value;
        //         }
        //     });

        //     if (UISettingMasterID !== 0) {
        //         const singleObj = {
        //             UISettingMasterID: UISettingMasterID,
        //             Value: Value
        //         };
        //         list.push(singleObj);
        //     }
        // });

        UpdateAllClick(UpdatedList);
    };
    const UpdateAllClick = async (list) => {
        let jsonObj = {
            list: list
        };
        let request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);

        const { isAuthorized, status, data } = await SETTING_UPDATE_ALL(
            request
        );
        setLoadingOpen(false);

        if (!isAuthorized) LOGOUT();

        setIsError(true);
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);
            return;
        }
        setSeverity('success');
        setResponseMessage(data);
        Select();
        setShouldGetUpdatedDataFromTable(false);
    };

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
            <Grid container spacing={2} style={{ marginBottom: '15px' }}>
                <Grid item xs={12} sm={12} md={10} lg={10}>
                    <h2 className={classes.PaperHead_h2}>UI Settings</h2>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <PrimaryButton
                        type="button"
                        variant="outlined"
                        color="primary"
                        size="small"
                        title="Update All"
                        style={{ float: 'right' }}
                        onClick={(e) => {
                            setShouldGetUpdatedDataFromTable(true);
                        }}
                    >
                        Update All
                    </PrimaryButton>
                </Grid>
            </Grid>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                getUpdatedData={processUpdatedData}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'none'}
            />
        </CustomContainer>
    );
});

export default UISetting;
