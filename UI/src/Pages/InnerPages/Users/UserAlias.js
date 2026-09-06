import React, { useState, useEffect } from 'react';
import { Container, Grid } from '@material-ui/core';
import {
    UpdateAliasName as UPDATE_ALIASNAME,
    Select as SELECT_USERTYPE
} from 'Components/Api/UserTypeApi';
import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import { LOGOUT } from 'ConstComponents';
import ReactTable from 'Components/ReactTable/ReactTable';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import * as Yup from 'yup';

const CustomContainer = HOCLoader(Container);

export const UserAlias = () => {
    const classes = commonStyle();

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);

    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    useEffect(() => {
        selectUserType();
        createColumns();
    }, []);

    const CustomValidation = Yup.object({
        AliasName: Yup.string().required(PLEASE_PROVIDE_VALID + 'AliasName'),

        MinimumAmount: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'MinimumAmount'
        )
    });

    const onUpdateClick = async (row) => {
        let requestObj = {
            UserTypeID: row[0],
            MinimumAmount: row[2],
            AliasName: row[3]
        };
        setIsError(false);
        setResponseMessage('');

        var errorValidation = false;

        await CustomValidation.validate(requestObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);

            setIsError(true);
            return;
        });

        if (errorValidation === true) return;

        var jsonReq = JSON.stringify(requestObj);
        setLoadingOpen(true);
        const response = await UPDATE_ALIASNAME(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setSeverity('error');
            setResponseMessage(response.data);
            setIsError(true);
            return;
        }
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        selectUserType();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'UserTypeID',
            srno: true
        });

        columns.push({
            label: 'UserType',
            name: 'UserType'
        });

        columns.push({
            label: 'Fund Txn Min Amount',
            name: 'MinimumAmount',
            editable: true,
            setCellHeaderProps: () => ({
                style: { width: '20%' }
            })
        });

        columns.push({
            label: 'AliasName',
            name: 'AliasName',
            editable: true,
            setCellHeaderProps: () => ({
                style: { width: '20%' }
            })
        });

        columns.push({
            label: 'Update',
            name: 'UserTypeID',
            buttonWithTitle: true,
            buttonTitle: 'Update',
            callback: (row) => {
                onUpdateClick(row);
            }
        });

        setMainColumns(columns);
    };

    const selectUserType = async () => {
        setLoadingOpen(true);
        const response = await SELECT_USERTYPE();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();

        if (!response.status) return;

        var data = JSON.parse(response.data);
        setItemList(data);
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
            <Grid item xs={12} sm={12} md={6} lg={6}>
                <h2 className={classes.PaperHead_h2}>User Alias</h2>
            </Grid>
            <ReactTable
                data={itemList}
                rowsPerPage={100}
                columns={mainColumns}
            />
        </CustomContainer>
    );
};
