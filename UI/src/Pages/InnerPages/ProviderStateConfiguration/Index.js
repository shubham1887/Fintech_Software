import { Container, Grid } from '@material-ui/core';
import {
    SelectByProviderID as PROVIDERCONFIG_SELECT,
    Update as PROVIDER_OPERATOR_UPDATE,
    UpdateAll as PROVIDER_OPERATOR_UPDATE_ALL
} from 'Components/Api/ProviderStateConfigurationApi';

import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import HOCLoader from 'Components/Loader/HOCLoader';
import Box from '@material-ui/core/Box';
import ReactTable from 'Components/ReactTable/ReactTable';
import { LOGOUT } from 'ConstComponents';

import React, { useEffect, useState } from 'react';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
import { useLocation } from 'react-router-dom';

const CustomContainer = HOCLoader(Container);
export const ProviderStateConfigSetup = () => {
    const classes = commonStyle();
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);
    const location = useLocation();
    const ProviderID = location.state.ProviderID;

    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [ProviderName, setProviderName] = useState(0);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');

    const [TakeCount, setTakeCount] = useState(100);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [sortColumnName] = useState('');
    const [sortDirection] = useState('');

    const selectData = async () => {
        selectDataByID(ProviderID, TakeCount, SkipCount, '');
    };

    const selectDataByID = async (ProviderID, TakeCount, SkipCount, Search) => {
        let JSONObj = {
            ProviderID: ProviderID,
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: Search,
            SortColumnName: sortColumnName,
            SortDirection: sortDirection
        };

        setLoadingOpen(true);
        setItemList([]);
        let request = JSON.stringify(JSONObj);

        const response = await PROVIDERCONFIG_SELECT(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setResponseMessage(response.data);
            setSeverity('error');
            return;
        }
        let data = JSON.parse(response.data);
        setItemList(data);
        setProviderName(data[0].ProviderName);
    };

    const UpdateClick = async (row) => {
        let ProviderStateConfigurationID = row[0];
        let StateCode = row[2];

        let jsonObj = {
            ProviderStateConfigurationID: ProviderStateConfigurationID,
            StateCode: StateCode
        };

        let request = JSON.stringify(jsonObj);

        setIsError(false);
        setLoadingOpen(true);
        const response = await PROVIDER_OPERATOR_UPDATE(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);
    };

    const createColumns = () => {
        let columns = [];

        columns.push({
            label: '#',
            name: 'ProviderStateConfigurationID',
            srno: true
        });

        columns.push({
            label: 'State Name',
            name: 'StateName'
        });
        columns.push({
            label: 'State Code',
            name: 'StateCode',
            editable: true
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
        selectData();
        createColumns();
    }, []);

    const CustomContainerCloseCallback = (alertState) => {
        setIsError(alertState);
    };

    const processUpdatedData = async (UpdatedList) => {
        setShouldGetUpdatedDataFromTable(false);
        let request = JSON.stringify(UpdatedList);

        setIsError(false);
        setLoadingOpen(true);
        const response = await PROVIDER_OPERATOR_UPDATE_ALL(request);

        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            selectData();
        }, MESSAGE_CLOSE_AFTER);

        // UpdatedList.forEach((row, index) => {
        //     row.forEach((item, index) => {});
        // });
    };

    const handlePageChange = ({ currentPage, RowsPerPage }) => {
        setTakeCount(RowsPerPage);
        setSkipCount(RowsPerPage * currentPage);
        selectDataByID(ProviderID, RowsPerPage, RowsPerPage * currentPage, '');
    };
    const handleSearchCallback = (SearchText) => {
        selectDataByID(ProviderID, TakeCount, SkipCount, SearchText.SearchText);
    };
    const handleSortCallback = ({ column, direction }) => {};
    return (
        <CustomContainer
            isLoading={loadingOpen}
            isError={isError}
            alertMessage={responseMessage}
            severity={severity}
            onCloseCallback={CustomContainerCloseCallback}
        >
            <Grid container spacing={2} style={{ marginBottom: '15px' }}>
                <Grid item xs={12} sm={12} md={8} lg={8}>
                    <h3 className={classes.PaperHead_h2}>
                        {ProviderName} Provider State Setup
                    </h3>
                </Grid>
                <Grid item xs={12} sm={12} md={2} lg={2}></Grid>

                <Grid item xs={12} sm={12} md={2} lg={2}>
                    <Box className={classes.flexReverse}>
                        <PrimaryButton
                            type="button"
                            variant="outlined"
                            color="primary"
                            size="small"
                            title="Update All"
                            onClick={(e) => {
                                setShouldGetUpdatedDataFromTable(true);
                            }}
                        >
                            Update All
                        </PrimaryButton>
                    </Box>
                </Grid>
            </Grid>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                rowsPerPage={50}
                count={totalCount}
                getCallback={handlePageChange}
                getSearchCallback={handleSearchCallback}
                getSortCallback={handleSortCallback}
                getUpdatedData={processUpdatedData}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'none'}
                bulkUpdate={true}
            />
        </CustomContainer>
    );
};
