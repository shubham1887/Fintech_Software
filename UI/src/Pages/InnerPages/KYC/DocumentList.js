import React, { useEffect, useState } from 'react';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';
import ReactTable from 'Components/ReactTable/ReactTable';

import { Modal, Button, Grid } from '@material-ui/core';
import { CustomModal } from 'Components/Modal/CustomModal';
import { KYCAction } from 'Pages/InnerPages/KYC/Action';

import { makeStyles } from '@material-ui/core/styles';

import { KYCView } from 'Pages/InnerPages/KYC/View';

const useModalStyles = makeStyles((theme) => ({
    login_form: {
        display: 'flex',
        flexDirection: 'column'
    },
    login_title: {
        fontFamily: 'Raleway-Medium',
        fontSize: '20px',
        lineHeight: '1.2',
        textAlign: 'center',
        textTransform: 'uppercase'
    },
    customButton: {
        marginTop: '2px',
        alignItems: 'right'
    },

    modalContent: {
        position: 'absolute',
        background: '#fff',
        padding: theme.spacing(2)
    }
}));

function getModalStyle() {
    const top = 50;
    const left = 50;

    return {
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${top}%, -${left}%)`
    };
}

export const DocumentList = ({
    currentName,
    documents,
    UserID,
    modalOpen,
    onDismissCallback
}) => {
    const modalClasee = useModalStyles();

    const [modalStyle] = useState(getModalStyle);
    const [open, setOpen] = useState(modalOpen);
    const [ActionmodalOpen, setModalOpen] = useState(false);
    const [UserDocumentID, setUserDocumentID] = useState(0);

    const [isViewOpen, setIsViewOpen] = useState(false);
    const [mainColumns, setMainColumns] = useState([]);
    const classes = commonStyle();
    useEffect(() => {
        createColumns();
    }, []);

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };

    const ViewKYC = (UserDocumentID) => {
        setUserDocumentID(UserDocumentID);
        setIsViewOpen(true);
    };
    const createColumns = () => {
        var columns = [];

        var enableIndex = 3;

        var options = [
            {
                Title: 'Action',
                name: 'UserDocumentID',
                enableOn: ['PROCESS'],
                statusIndex: enableIndex,
                Callback: (values) => {
                    selectDocumentsByUserID(values[0], values[3]);
                }
            }
        ];

        options.push({
            Title: 'View',
            name: 'UserDocumentID',
            enableOn: ['PROCESS', 'APPROVE', 'REJECT'],
            statusIndex: enableIndex,
            Callback: (values) => {
                ViewKYC(values[0]);
            }
        });

        columns.splice(0, 0, {
            label: ' ',
            name: 'UserDocumentID',
            MoreMenu: true,
            options: options
        });

        columns.push({
            label: '#',
            name: 'UserDocumentID',
            srno: true
        });

        columns.push({
            label: 'Docs',
            name: 'CompactResponse'
        });

        columns.push({
            label: 'Status',
            name: 'Status',
            customStatus: true
        });
        columns.push({
            label: 'Download',
            name: 'Document',
            download: true
        });

        setMainColumns(columns);
    };
    const selectDocumentsByUserID = async (UserDocumentID) => {
        setUserDocumentID(UserDocumentID);
        setModalOpen(true);
    };
    const handleDismissCallback = () => {
        onDismissCallback(false);
        setModalOpen(false);
    };

    return (
        <>
            <Modal open={open} onClose={handleModalClose}>
                <div style={modalStyle} className={modalClasee.modalContent}>
                    <Grid
                        container
                        style={{ display: 'flex', direction: 'rtl' }}
                    >
                        <Button
                            variant="contained"
                            color="secondary"
                            size="small"
                            onClick={handleModalClose}
                        >
                            X
                        </Button>
                    </Grid>
                    <Grid item xs={12}>
                        <h2 className={classes.recharge_h2}>
                            User :
                            <span className={classes.Success_h3}>
                                {currentName}
                            </span>
                        </h2>
                    </Grid>
                    <ReactTable
                        headerToolbar={false}
                        footerToolbar={false}
                        data={documents}
                        columns={mainColumns}
                    />
                    {ActionmodalOpen ? (
                        <KYCAction
                            DocumentID={UserDocumentID}
                            modalOpen={modalOpen}
                            onDismissCallback={handleDismissCallback}
                        />
                    ) : null}

                    {isViewOpen ? (
                        <CustomModal
                            isOpen={isViewOpen}
                            title="KYC"
                            width="75%"
                            onDismissCallback={(value) => {
                                setIsViewOpen(value);
                            }}
                        >
                            <KYCView
                                UserDocumentID={UserDocumentID}
                                onDismissCallback={(value) => {
                                    setIsViewOpen(value);
                                }}
                            />
                        </CustomModal>
                    ) : null}
                </div>
            </Modal>
        </>
    );
};
