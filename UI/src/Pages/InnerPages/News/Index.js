import React, { useState, useEffect, useRef } from 'react';
import { Container, Grid } from '@material-ui/core';

import { LOGOUT } from 'ConstComponents';
import { Select as NEWS_SELECT_ALL } from 'Components/Api/NewsAPI';
import ReactTable from 'Components/ReactTable/ReactTable';
import { CustomModal } from 'Components/Modal/CustomModal';
import { NewsView } from 'Pages/InnerPages/News/NewsView';
import { ConfirmAlert } from 'Components/Alert/ConfirmAlert';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import NewsAdd from 'Pages/InnerPages/News/Add';
import NewsEdit from 'Pages/InnerPages/News/Edit';
import { Delete as NEWS_DELETE } from 'Components/Api/NewsAPI';
import { DeleteAll as DELETE_ALL } from 'Components/Api/NewsAPI';
import HOCLoader from 'Components/Loader/HOCLoader';
import { PrimaryButton } from 'Components/Inputes/CustomButton';
const CustomContainer = HOCLoader(Container);

export const News = () => {
    const classes = commonStyle();
    const formRef = useRef();
    const [itemList, setItemList] = useState([]);
    const [loadingOpen, setLoadingOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const [isError, setIsError] = useState(false);
    const [severity, setSeverity] = useState('success');
    const [responseMessage, setResponseMessage] = useState('');
    const [shouldOpenAdd, setShouldOpenAdd] = useState(false);
    const [shouldOpenEdit, setShouldOpenEdit] = useState(false);
    const [NewsID, setNewsID] = useState(0);
    const [isAlertOpen, setIsAlertOpen] = useState(false);
    const [confirmAlertRequest, setConfirmAlertRequest] = useState({});
    const [NewsImagePath, setNewsImagePath] = useState('');
    const [Title, setTitle] = useState('');
    const [Description, setDescription] = useState('');
    const [isViewOpen, setIsViewOpen] = useState(false);
    const [SortDirection, setSortDirection] = useState('');
    const [TakeCount, setTakeCount] = useState(10);
    const [SkipCount, setSkipCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(0);

    const selectData = async (
        TakeCount,
        SkipCount,
        SearchText,
        SortColumnName,
        setSortDirection
    ) => {
        let inputObj = {
            TakeCount: TakeCount,
            SkipCount: SkipCount,
            SearchText: SearchText,
            SortColumnName: SortColumnName,
            SortDirection: SortDirection
        };
        var requestObj = JSON.stringify(inputObj);
        setIsError(false);
        setLoadingOpen(true);
        const response = await NEWS_SELECT_ALL(requestObj);
        setLoadingOpen(false);

        if (!response.isAuthorized) LOGOUT();

        if (!response.status) {
            setIsError(true);
            setSeverity('error');
            setResponseMessage(response.data);

            return;
        }

        var data = JSON.parse(response.data);

        if (SkipCount <= 0) {
            setTotalCount(data.Count);
        }
        setItemList(data.list);
    };

    const Add = async () => {
        setShouldOpenAdd(true);
    };

    const Delete = async (request) => {
        var jsonReq = JSON.stringify(request);
        setLoadingOpen(true);
        setIsError(false);
        var response = await NEWS_DELETE(jsonReq);
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setResponseMessage(response.data);

        setIsError(true);

        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            return;
        }
        selectData();
    };
    const DeleteAll = async () => {
        setLoadingOpen(true);
        setIsError(false);
        var response = await DELETE_ALL();
        setLoadingOpen(false);
        if (!response.isAuthorized) LOGOUT();
        setResponseMessage(response.data);

        setIsError(true);

        setSeverity('success');

        if (!response.status) {
            setSeverity('error');
            return;
        }
        selectData();
    };

    const createColumns = () => {
        var columns = [];

        columns.push({
            label: '#',
            name: 'NewsID',
            srno: true
        });
        columns.push({
            label: 'Date',
            name: 'Date'
        });
        columns.push({
            label: 'Type',
            name: 'UserType'
        });
        columns.push({
            label: 'User',
            name: 'FirmName'
        });
        columns.push({
            label: 'Title',
            name: 'Title',
            ReadOnlyTextArea: true
        });

        columns.push({
            label: 'Description',
            name: 'Description',
            ReadOnlyTextArea: true
        });
        columns.push({
            label: 'HyperLink',
            name: 'HyperLink'
        });

        setMainColumns(columns);
    };

    useEffect(() => {
        setLoadingOpen(true);
        createColumns();
        selectData(TakeCount, SkipCount);
    }, []);

    return (
        <CustomContainer
            innerRef={formRef}
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
                    <h2 className={classes.PaperHead_h2}>News</h2>
                </Grid>

                <Grid item xs={12} sm={12} md={1} lg={1}>
                    <PrimaryButton
                        variant="contained"
                        color="primary"
                        title="New News"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            Add(0);
                        }}
                    >
                        New News
                    </PrimaryButton>
                </Grid>
                <Grid item xs={12} sm={12} md={1} lg={1}>
                    <PrimaryButton
                        variant="contained"
                        color="primary"
                        title="Delete All"
                        className={classes.CustomPrimaryButtonStyle}
                        onClick={() => {
                            DeleteAll();
                        }}
                    >
                        Delete All
                    </PrimaryButton>
                </Grid>
            </Grid>

            {shouldOpenAdd ? (
                <CustomModal
                    isOpen={shouldOpenAdd}
                    title="News Add"
                    width="75%"
                    onDismissCallback={(value) => {
                        setShouldOpenAdd(value);
                        selectData();
                    }}
                >
                    <NewsAdd
                        NewsID={NewsID}
                        onDismissCallback={(value) => {
                            setShouldOpenAdd(value);
                            selectData();
                        }}
                    />
                </CustomModal>
            ) : null}

            {shouldOpenEdit ? (
                <CustomModal
                    isOpen={shouldOpenEdit}
                    title="News Edit"
                    width="75%"
                    onDismissCallback={(value) => {
                        setShouldOpenEdit(value);
                        selectData();
                    }}
                >
                    <NewsEdit
                        NewsID={NewsID}
                        onDismissCallback={(value) => {
                            setShouldOpenEdit(value);
                            selectData();
                        }}
                    />
                </CustomModal>
            ) : null}

            {isAlertOpen ? (
                <ConfirmAlert
                    isOpen={isAlertOpen}
                    title="Delete Confirmation"
                    message="Are you sure to delete this news ?"
                    request={confirmAlertRequest}
                    positiveButtonClick={(request) => {
                        setIsAlertOpen(false);
                        Delete(request);
                    }}
                    negativeButtonClick={() => {
                        setIsAlertOpen(false);
                    }}
                />
            ) : null}
            {isViewOpen ? (
                <CustomModal
                    isOpen={isViewOpen}
                    title="News"
                    width="75%"
                    onDismissCallback={(value) => {
                        setIsViewOpen(value);
                    }}
                >
                    <NewsView
                        NewsImagePath={NewsImagePath}
                        Title={Title}
                        Description={Description}
                        onDismissCallback={(value) => {
                            setIsViewOpen(value);
                        }}
                    />
                </CustomModal>
            ) : null}

            <ReactTable
                data={itemList}
                headerToolbar={false}
                columns={mainColumns}
                count={totalCount}
                rowsPerPage={TakeCount}
                serverSide={true}
                getCallback={({ currentPage, RowsPerPage }) => {
                    setCurrentPage(currentPage);
                    setTakeCount(RowsPerPage);
                    setSkipCount(RowsPerPage * currentPage);
                    selectData(
                        RowsPerPage,
                        RowsPerPage * currentPage,
                        '',
                        '',
                        ''
                    );
                }}
            />
        </CustomContainer>
    );
};
