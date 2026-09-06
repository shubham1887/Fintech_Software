import React, { useState, useEffect, useRef } from 'react';
import { MESSAGE_CLOSE_AFTER } from 'Components/Helper/Messages';
import * as Yup from 'yup';
import { LOGOUT } from 'ConstComponents';
import {
    SelectAllBank as SELECT_ALl_BANK,
    UpdateBank as UPDATE,
    ActiveDeactive as DMTBank_ACTIVATE,
    IMPSActive as DMTBank_IMPS_ACTIVATE,
    UpdateAll as UPDATE_ALL
} from 'Components/Api/DMTSettingsAPI';
import { Container, Button } from '@material-ui/core';
import ReactTable from 'Components/ReactTable/ReactTable';

import { PLEASE_PROVIDE_VALID } from 'Components/Helper/Messages';
import HOCLoader from 'Components/Loader/HOCLoader';
const CustomContainer = HOCLoader(Container);
export const DMTBankSetting = () => {
    const formRef = useRef();
    const [responseMessage, setResponseMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const [loading, setLoading] = React.useState(false);
    const [severity, setSeverity] = useState('success');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [SearchText, setSearchText] = useState('');
    const [currentPage, setCurrentPage] = useState(0);
    const [itemList, setItemList] = useState([]);
    const [mainColumns, setMainColumns] = useState([]);
    const [
        shouldGetUpdatedDataFromTable,
        setShouldGetUpdatedDataFromTable
    ] = useState(false);

    const submithandle = async (values, { setSubmitting, resetForm }) => {
        setSubmitting(true);
        callSelectData(TakeCount, SkipCount, '', '', '');

        setSubmitting(false);
    };

    const selectAllBank = async () => {
        setLoading(true);
        const { isAuthorized, status, data } = await SELECT_ALl_BANK();
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }
        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        var d = JSON.parse(data);

        setItemList(d);
    };
    const callSelectData = async (TakeCount, SkipCount, SearchText) => {
        let requestObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            Search: SearchText,
            IsExcelDownload: false
        };

        var requestJSON = JSON.stringify(requestObj);
        setIsError(false);
        setLoading(true);
        const { isAuthorized, status, data } = await SELECT_ALl_BANK(
            requestJSON
        );
        setLoading(false);
        if (!isAuthorized) LOGOUT();
        if (!status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(data);

            return;
        }

        var d = JSON.parse(data);
        if (SkipCount <= 0) {
            setTotalCount(d.Count);
        }

        if (d.IsExcelDownload) {
            downloadFile(d.ReportPath, 'DMT Bank Report');
            return;
        }
        setItemList(d.list);
    };

    function downloadFile(URL, Title) {
        let a = document.createElement('a');
        a.href = URL;
        a.setAttribute('download', Title);
        a.setAttribute('target', '_blank');
        a.click();
        document.removeChild(a);
    }

    const activate = async (item, value) => {
        var jsonObj = {
            DMTBankID: item,
            IsActive: value
        };

        var request = JSON.stringify(jsonObj);

        setLoading(true);
        setIsError(false);
        const response = await DMTBank_ACTIVATE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };
    const activateIMPS = async (item, value) => {
        var jsonObj = {
            DMTBankID: item,
            IsIMPS: value
        };

        var request = JSON.stringify(jsonObj);

        setLoading(true);
        setIsError(false);
        const response = await DMTBank_IMPS_ACTIVATE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        if (!response.status) setSeverity('error');
        setIsError(true);
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'DMTBankID',
            srno: true
        });

        columns.push({
            label: 'Name',
            name: 'Name',
            deactived: true
        });

        columns.push({
            label: 'IFSC',
            name: 'DefaultIFSC',
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
        columns.push({
            label: 'IMPS Active',
            name: 'IsIMPS',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activateIMPS(id, value);
            }
        });

        columns.push({
            label: 'Active',
            name: 'IsActive',
            activate: true,
            primaryKeyIndex: 0,
            callback: (id, value) => {
                activate(id, value);
            }
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        createColumns();
        callSelectData(TakeCount, SkipCount, '');
        //     selectAllBank();
    }, []);

    const UpdateValidation = Yup.object({
        DMTBankID: Yup.number().required(PLEASE_PROVIDE_VALID + 'ID'),
        DefaultIFSC: Yup.string().required(
            PLEASE_PROVIDE_VALID + 'Default IFSC'
        )
    });

    const UpdateClick = async (row) => {
        setIsError(false);
        let jsonObj = {
            DMTBankID: row[0],
            DefaultIFSC: row[2]
        };
        var errorValidation = false;

        await UpdateValidation.validate(jsonObj).catch(function (err) {
            errorValidation = true;
            setSeverity('error');
            setResponseMessage(err.message);
            setIsError(true);
            return;
        });
        if (errorValidation === true) {
            return;
        }
        var request = JSON.stringify(jsonObj);
        setIsError(false);
        setLoading(true);
        const response = await UPDATE(request);
        setLoading(false);

        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');
        callSelectData(TakeCount, SkipCount, '', '', '');
    };

    const processUpdatedData = async (UpdatedList) => {
        setShouldGetUpdatedDataFromTable(false);
        let request = JSON.stringify(UpdatedList);

        setIsError(false);
        setLoading(true);
        const response = await UPDATE_ALL(request);
        setLoading(false);
        if (!response.isAuthorized) LOGOUT();
        setSeverity('success');
        setResponseMessage(response.data);
        setIsError(true);
        if (!response.status) setSeverity('error');

        setTimeout(() => {
            // selectData();
        }, MESSAGE_CLOSE_AFTER);
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
            <Button
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
            </Button>

            <ReactTable
                data={itemList}
                columns={mainColumns}
                count={totalCount}
                onSubmit={submithandle}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    callSelectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
                getSearchCallback={({ SearchText }) => {
                    callSelectData(TakeCount, 0, SearchText, '', '');
                }}
                getSortCallback={({ column, direction }) => {
                    callSelectData(
                        TakeCount,
                        currentPage,
                        SearchText,
                        column,
                        direction
                    );
                }}
                getUpdatedData={processUpdatedData}
                shouldGetUpdatedData={shouldGetUpdatedDataFromTable}
                isSelectableRows={'none'}
                bulkUpdate={true}
            />
        </CustomContainer>
    );
};
