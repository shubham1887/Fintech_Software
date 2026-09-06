import React, { useState, useEffect, Fragment } from 'react';
import MUIDataTable, { debounceSearchRender } from 'mui-datatables';
import RefreshIcon from '@material-ui/icons/Refresh';
import AddShoppingCartIcon from '@material-ui/icons/AddShoppingCart';
import {
    TextField,
    Box,
    Tooltip,
    Button,
    Switch,
    FormControlLabel,
    TableFooter,
    TableRow,
    TableCell,
    TablePagination,
    Grid,
    Card,
    Icon
} from '@material-ui/core';

import { DISTRIBUTOR, SUPER_DISTRIBUTOR } from 'Components/Helper/UsertypeCode';
import DoneIcon from '@material-ui/icons/Done';

import ToggleButton from '@material-ui/lab/ToggleButton';
import ToggleButtonGroup from '@material-ui/lab/ToggleButtonGroup';
import SaveIcon from '@material-ui/icons/Save';

import CancelIcon from '@material-ui/icons/Cancel';
import SettingsIcon from '@material-ui/icons/Settings';
import SettingsInputComponentIcon from '@material-ui/icons/SettingsInputComponent';

import { createMuiTheme, MuiThemeProvider } from '@material-ui/core/styles';
import CloudDownloadIcon from '@material-ui/icons/CloudDownload';
import DeleteIcon from '@material-ui/icons/Delete';
import EditIcon from '@material-ui/icons/Edit';
import VisibilityIcon from '@material-ui/icons/Visibility';
import FileCopyIcon from '@material-ui/icons/FileCopy';
import { CustomIcon } from 'Components/CustomIcons';
import MoreDetail from 'Components/MenuBar/MoreDetail';
import CustomSwitch from './CustomSwitch';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { CustomStatus } from './CustomStatus';
import { ListInsideTable } from './ListInsideTable';
import DropDownInsideTable from './DropDownInsideTable';
import LabelListInsideTable from './LabelListInsideTable';
import { RadioInsideTable } from './RadioInsideTable';
import { CustomCommission } from './CustomCommission';
import { CustomInvoice } from './CustomInvoice';
import { AdminCustomCommission } from './AdminCustomCommission';
import { CustomSuccessFail } from './CustomSuccessFail';
import { CustomShiftResend } from './CustomShiftResend';
import { PRIMARY_COLOR, REPORT_FONT_SIZE } from 'Config';
import { MultipleSelectInsideTable } from './MultipleSelectInsideTable';
import { TextCheckboxListInsideTable } from './TextCheckboxListInsideTable';
import { ImageCheckboxListInsideTable } from './ImageCheckboxListInsideTable';
import { indianFormatNumber } from 'Components/utils';
import CustomAmount from './CustomAmount';

import TransferIcon from 'Components/SvgButtons/TransferIcon';

const getMuiTheme = (showHeader) => {
    return createMuiTheme({
        typography: {
            fontFamily: ['Arial', 'Montserrat', 'Helvetica', 'sans-serif'].join(
                ','
            ),
            h6: {
                color: 'red'
            }
        },

        overrides: {
            MUIDataTableBodyCell: {
                root: {
                    // paddingLeft: '8px',
                    // paddingRight: '0px',
                    paddingTop: '0px',
                    paddingBottom: '0px',
                    lineHeight: 'min(10vh, 2.0em)',
                    fontSize: REPORT_FONT_SIZE(),
                    fontFamily: 'Arial',
                    overflow: 'hidden'
                },
                stackedCommon: {
                    '@media (max-width:959.95px)': {
                        height: '100vh'
                    }
                }
            },
            MUIDataTableHeadCell: {
                root: {
                    fontWeight: 'bold',
                    backgroundColor: '#F2F2F2'
                },
                fixedHeader: {
                    backgroundColor: '#F2F2F2',
                    borderBottom: 'none',
                    fontWeight: 'bold',
                    fontSize: '12px'
                }
            },
            MUIDataTableSelectCell: {
                headerCell: {
                    backgroundColor: '#e0e0e0',
                    fontWeight: 'bold',
                    borderBottom: 'none'
                }
            },
            MUIDataTableToolbarSelect: {
                title: { color: '#000' }
            },
            MUIDataTableToolbar: {
                root: {
                    display: showHeader === false ? 'none' : 'flex'
                }
            },

            MUIDataTableBodyRow: {
                root: {
                    '&:nth-child(even)': {
                        backgroundColor: '#dad9d9'
                    }
                }
            },
            MuiTableCell: {
                root: {
                    fontSize: '12px',
                    border: '1px solid #c5c4c4'
                }
            },

            MuiSvgIcon: {
                root: {
                    verticalAlign: 'middle'
                }
            },
            MuiToggleButton: {
                root: {
                    background: 'transparent',
                    '&$selected': {
                        // <-- mixing the two classes
                        color: '#fff !important',
                        backgroundColor: PRIMARY_COLOR()
                    }
                }
            }
        }
    });
};
const ReactTable = ({
    serverSide,
    title,
    data,
    columns,
    count,
    rowsPerPage,
    getCallback,
    getSearchCallback,
    getSortCallback,
    sumkeys,
    headerToolbar,
    footerToolbar,
    getUpdatedData,
    shouldGetUpdatedData,
    isSelectableRows,
    bulkUpdate,
    tableRef
}) => {
    const classes = commonStyle();
    const [mainColumns, setMainColumns] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);
    const [isBulkUpdate, setIsBulkUpdate] = useState(false);
    let showHeader = headerToolbar === undefined ? true : headerToolbar;
    let showFooter = footerToolbar === undefined ? true : footerToolbar;

    const [tableData, settableData] = useState(data);
    const [updatedData, setUpdatedData] = useState([]);

    useEffect(() => {
        settableData(data);
        checkBulkUpdate();
    }, [data]);

    const checkBulkUpdate = () => {
        if (bulkUpdate === undefined || bulkUpdate === false) {
            setIsBulkUpdate(false);
        } else {
            setIsBulkUpdate(true);
        }
    };

    useEffect(() => {
        createColumns(columns);
        checkBulkUpdate();
    }, [columns.length]);

    const handleTableUpdate = (
        currentTableData,
        tableData,
        rowIndex,
        columnIndex,
        name,
        value
    ) => {
        if (!isBulkUpdate) return;

        let row = currentTableData[rowIndex].data;
        row[columnIndex] = value;

        let rowTable = tableData[rowIndex];
        rowTable[name] = value;

        settableData(tableData);
    };

    const createColumns = (tableColumns) => {
        let columns = [];

        tableColumns.map((column, index) => {
            columns.push({
                ...column,
                ...(column.numeric ? numeric(column) : null),
                ...(column.srno ? srNo() : null),
                ...(column.customStatus ? status() : null),
                ...(column.array ? array(column) : null),
                ...(column.bold ? bold(column) : null),
                ...(column.deactived ? deactived(column) : null),
                ...(column.actived ? actived(column) : null),
                ...(column.displayAsPassword
                    ? displayAsPassword(column)
                    : null),

                ...(column.activedboolean ? activedboolean(column) : null),

                ...(column.copy ? copy(column) : null),
                ...(column.edit ? edit(column) : null),
                ...(column.Show ? Show(column) : null),

                ...(column.setUp ? setUp(column) : null),
                ...(column.Downline ? Downline(column) : null),
                ...(column.editableTextBoxOrPassword
                    ? editableTextBoxOrPasswordFunc(column)
                    : null),
                ...(column.editable ? editable(column) : null),
                ...(column.editableWithNumberOnly
                    ? editableWithNumberOnly(column)
                    : null),
                ...(column.editableWithPinOnly
                    ? editableWithPinOnly(column)
                    : null),
                ...(column.ReadOnlyTextArea ? ReadOnlyTextArea(column) : null),
                ...(column.iconButton ? iconButton(column) : null),
                ...(column.iconButtonWithRow
                    ? iconButtonWithRow(column)
                    : null),
                ...(column.delete ? deleteRow(column) : null),
                ...(column.button ? button(column) : null),

                ...(column.testbutton ? testbutton(column) : null),
                ...(column.buttonOrLabel ? buttonOrLabel(column) : null),
                ...(column.buttonOrDMTVerify
                    ? buttonOrDMTVerify(column)
                    : null),
                ...(column.labelWithToolTip
                    ? labelWithToolTipFunc(column)
                    : null),
                ...(column.activate ? activate(column) : null),
                ...(column.activateRow ? activateRow(column) : null),
                ...(column.arrayTextBox ? arrayTextBox(column) : null),
                ...(column.arraycheckboxImage
                    ? arraycheckboxImage(column)
                    : null),
                ...(column.textwrap ? textwrap(column) : null),
                ...(column.update ? update(column) : null),
                ...(column.refresh ? refresh(column) : null),

                ...(column.buyButton ? buyButton(column) : null),
                ...(column.transfer ? transfer(column) : null),
                ...(column.tableButton ? tableButton(column) : null),

                ...(column.save ? save(column) : null),
                ...(column.buttonWithTitle ? ButtonWithTitle(column) : null),
                ...(column.deleteButtonWithTitle
                    ? DeleteButtonWithTitle(column)
                    : null),

                ...(column.hiddenbutton ? hiddenbutton(column) : null),
                ...(column.Image ? Image(column) : null),
                ...(column.ImageHoverComponent && ImageHoverComponent(column)),
                ...(column.BBPSImage ? BBPSImage(column) : null),

                ...(column.dropdown ? dropdown(column) : null),
                ...(column.autoComplete ? autoComplete(column) : null),
                ...(column.hide ? hide(column) : null),
                ...(column.download ? download(column) : null),

                ...(column.approval ? approval(column) : null),
                ...(column.textArea ? textArea(column) : null),
                ...(column.MoreMenu ? MoreMenu(column) : null),
                ...(column.Labels ? Labels(column) : null),
                ...(column.MultiSelectCheckbox
                    ? MultiSelectCheckbox(column)
                    : null),
                ...(column.toggle ? toggle(column) : null),
                ...(column.CustomCommission
                    ? CustomCommissionFunc(column)
                    : null),
                ...(column.AdminCustomCommission
                    ? AdminCustomCommissionFunc(column)
                    : null),
                ...(column.CustomSuccessFail
                    ? CustomSuccessFailFunc(column)
                    : null),
                ...(column.CustomShiftResend
                    ? CustomShiftResendFunc(column)
                    : null),
                ...(column.CustomSwitch ? CustomSwitchFunction(column) : null),
                ...(column.CustomInvoice ? CustomInvoiceFunc(column) : null),
                ...(column.Wallets ? Wallets(column) : null),
                ...(column.customAmount ? customAmount(column) : null)
            });
        });

        setMainColumns(columns);
    };

    const deactived = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let { rowData } = tableMeta;
                    // put always isActive column at last and hidden : example in User/Index
                    let idx = rowData.length - 1;

                    return rowData[idx] ? (
                        <span style={{ color: 'black' }}>{value}</span>
                    ) : (
                        <span style={{ color: 'red' }}>{value}</span>
                    );
                }
            }
        };
    };

    const displayAsPassword = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let len = value.length;
                    var str = new Array(len + 1).join('*');

                    let { rowData } = tableMeta;
                    // put always isActive column at  last : example in /DMTSettings
                    let idx = rowData.length - 1;

                    return rowData[idx] === 'Yes' || rowData[idx] === true ? (
                        <span style={{ color: 'green', fontWeight: 'bold' }}>
                            {str}
                        </span>
                    ) : (
                        <span style={{ color: 'black' }}>{str}</span>
                    );
                }
            }
        };
    };

    const actived = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let { rowData } = tableMeta;
                    // put always isActive column at second last : example in /DMTSettings
                    let idx = rowData.length - 1;

                    let idx1 = rowData.length - 2;

                    return rowData[idx] === 'Yes' ||
                        rowData[idx] === true ||
                        rowData[idx1] === true ||
                        rowData[idx] === 'GSTPaid' ? (
                        <span
                            style={{
                                color: 'green',
                                fontWeight: 'bold',
                                fontSize: '14px'
                            }}
                        >
                            {value}
                        </span>
                    ) : (
                        <span style={{ color: 'black' }}>{value}</span>
                    );
                }
            }
        };
    };

    const activedboolean = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let { rowData } = tableMeta;
                    // put always isActive column at  second last  : example in /SMSAdd
                    let idx = rowData.length - 1;

                    return rowData[idx] ? (
                        <span style={{ color: 'green' }}>{value}</span>
                    ) : (
                        <span style={{ color: 'black' }}>{value}</span>
                    );
                }
            }
        };
    };

    const customAmount = () => {
        return {
            options: {
                customBodyRender: (value, tableMeta, updateValue) => {
                    return <CustomAmount value={value} />;
                }
            }
        };
    };

    const BalanceComponent = ({ value }) => {
        let jsonValue = JSON.parse(value);

        const t = jsonValue.map(({ ShortName, Balance }, index) => (
            <React.Fragment key={index + ShortName}>
                <label> {ShortName}</label>:<b> {Balance}</b>
                <br />
            </React.Fragment>
        ));

        return <>{t}</>;
    };

    const Wallets = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return <BalanceComponent value={value} />;
                }
            }
        };
    };
    const CustomInvoiceFunc = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <CustomInvoice
                            value={value}
                            callback={(values) => {
                                updateValue(values);
                                column.callback(values);
                            }}
                        />
                    );
                }
            }
        };
    };
    const CustomCommissionFunc = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <CustomCommission
                            value={value}
                            disabled={column.disabled}
                            RealCommission={column.RealCommission}
                            callback={(values) => {
                                updateValue(values);
                            }}
                        />
                    );
                }
            }
        };
    };

    const AdminCustomCommissionFunc = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <AdminCustomCommission
                            value={value}
                            callback={(values) => {
                                updateValue(values);

                                let {
                                    rowIndex,
                                    columnIndex,
                                    columnData,
                                    rowData,
                                    tableData,
                                    tableState,
                                    currentTableData
                                } = tableMeta;

                                if (
                                    currentTableData !== null &&
                                    rowIndex !== null &&
                                    columnIndex !== null
                                ) {
                                    handleTableUpdate(
                                        currentTableData,
                                        tableData,
                                        rowIndex,
                                        columnIndex,
                                        column.name,
                                        values
                                    );
                                }
                            }}
                        />
                    );
                }
            }
        };
    };
    const CustomSuccessFailFunc = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <CustomSuccessFail
                            value={value}
                            callback={(
                                TransactionMasterID,
                                TxnID,
                                Pin,
                                BtnTitle
                            ) => {
                                column.callback(
                                    TransactionMasterID,
                                    TxnID,
                                    Pin,
                                    BtnTitle
                                );
                            }}
                        />
                    );
                }
            }
        };
    };

    const CustomShiftResendFunc = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <CustomShiftResend
                            value={value}
                            callback={(
                                TransactionMasterID,
                                TxnID,
                                BtnTitle
                            ) => {
                                column.callback(
                                    TransactionMasterID,
                                    TxnID,
                                    BtnTitle
                                );
                            }}
                        />
                    );
                }
            }
        };
    };

    const Labels = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <LabelListInsideTable
                            value={value}
                            index={tableMeta.columnIndex}
                            keyName={column.keyName}
                            keyID={column.keyID}
                            callback={(values) => {
                                let jSONObj = {
                                    PrimaryID: tableMeta.rowData[0],
                                    value: values
                                };
                                column.callback(jSONObj);
                            }}
                        />
                    );
                }
            }
        };
    };

    const arrayTextBox = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Grid container>
                            <TextCheckboxListInsideTable
                                value={value}
                                index={tableMeta.columnIndex}
                                keyName={column.keyName}
                                callback={(values) => {
                                    updateValue(values);
                                }}
                            />
                        </Grid>
                    );
                }
            }
        };
    };
    const arraycheckboxImage = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Grid container>
                            <ImageCheckboxListInsideTable
                                value={value}
                                index={tableMeta.columnIndex}
                                keyName={column.keyName}
                                callback={(values) => {
                                    updateValue(values);
                                }}
                            />
                        </Grid>
                    );
                }
            }
        };
    };

    const MultiSelectCheckbox = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let newList = value;
                    let handleRadioInsideCallback = (values) => {
                        updateValue(values);
                        let { rowIndex, columnIndex, tableData } = tableMeta;

                        // handleTableUpdate(
                        //     tableData,
                        //     rowIndex,
                        //     columnIndex,
                        //     column.name,
                        //     event.target.value
                        // );
                    };
                    return (
                        <Grid container>
                            <RadioInsideTable
                                list={newList}
                                value={value}
                                callback={handleRadioInsideCallback}
                            />
                        </Grid>
                    );
                }
            }
        };
    };
    const CustomSwitchFunction = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let newList = value;

                    return (
                        <Grid container>
                            <CustomSwitch
                                list={newList}
                                value={value}
                                callback={(obj, arr) => {
                                    column.callback(obj);

                                    updateValue(arr);
                                }}
                            />
                        </Grid>
                    );
                }
            }
        };
    };

    const Image = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <img
                            src={value}
                            className="gridImg"
                            alt="img"
                            height={30}
                            width={30}
                        />
                    );
                }
            }
        };
    };

    const ImageHoverComponent = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <img
                            src={value}
                            className="gridImg"
                            alt="img"
                            height={30}
                            width={30}
                            style={{
                                cursor: 'pointer',
                                transition: 'transform 0.2s',
                                position: 'absolute',
                                zIndex: '999999'
                            }}
                            onMouseOver={(e) =>
                                (e.target.style.transform = 'scale(6)')
                            }
                            onMouseOut={(e) =>
                                (e.target.style.transform = 'scale(1)')
                            }
                        />
                    );
                }
            }
        };
    };

    const BBPSImage = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    if (value.length < 4) return <></>;
                    else
                        return (
                            <img
                                src={value}
                                className="gridImg"
                                alt="img"
                                height={30}
                                width={30}
                            />
                        );
                }
            }
        };
    };

    const srNo = () => {
        return {
            options: {
                filter: false,
                sort: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    const { page, rowsPerPage } = tableMeta.tableState;
                    const rowId = tableMeta.rowIndex;
                    return <label>{page * rowsPerPage + 1 + rowId}</label>;
                }
            }
        };
    };
    const bold = () => {
        return {
            options: {
                filter: false,
                sort: false,

                customBodyRender: (value, tableMeta, updateValue) => {
                    return <b>{value}</b>;
                }
            }
        };
    };
    const array = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <ListInsideTable
                            value={value}
                            index={tableMeta.columnIndex}
                            keyName={column.keyName}
                        />
                    );
                }
            }
        };
    };

    const dropdown = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <DropDownInsideTable
                            list={column.list}
                            value={value}
                            callback={(values) => {
                                updateValue(values);
                            }}
                        />
                    );
                }
            }
        };
    };

    const autoComplete = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <MultipleSelectInsideTable
                            list={column.list}
                            value={value}
                            placeholder={column.placeholder}
                            label={column.label}
                            callback={(values) => {
                                updateValue(values);
                            }}
                        />
                    );
                }
            }
        };
    };

    const status = () => {
        return {
            options: {
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <CustomStatus
                            value={value}
                            index={tableMeta.columnIndex}
                        />
                    );
                }
            }
        };
    };

    const numeric = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    var num = '';
                    if (typeof value === 'number') {
                        num = indianFormatNumber(value); //.toFixed(3)
                    }

                    return (
                        <Grid container spacing={1}>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <div
                                    className={classes.numbericRightAlign}
                                    style={{
                                        width: column.textwrapwidth
                                            ? column.textwrapwidth
                                            : 100
                                    }}
                                >
                                    {value !== null && value !== undefined ? (
                                        column.bold === true ? (
                                            <b>num</b>
                                        ) : (
                                            num
                                        )
                                    ) : null}
                                </div>
                            </Grid>
                        </Grid>
                    );
                }
            }
        };
    };

    const edit = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let displayNone = '';

                    if (
                        column.disabledOn !== undefined &&
                        column.statusIndex !== undefined
                    ) {
                        const val = tableMeta.rowData[column.statusIndex];

                        column.disabledOn.map((item, index) => {
                            if (item === val) {
                                displayNone = 'none';
                            }
                        });
                    }
                    return (
                        <EditIcon
                            color="primary"
                            style={{ display: displayNone, margin: 8 }}
                            onClick={(event) => {
                                column.callback(value);
                            }}
                        />
                    );
                }
            }
        };
    };
    const Show = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <VisibilityIcon
                            color={column.color}
                            onClick={(event) => {
                                column.callback(value);
                            }}
                        />
                    );
                }
            }
        };
    };
    const copy = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <FileCopyIcon
                            color="primary"
                            onClick={(event) => {
                                column.callback(value);
                            }}
                        />
                    );
                }
            }
        };
    };

    const iconButton = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let displayNone = '';

                    if (
                        column.disabledOn !== undefined &&
                        column.statusIndex !== undefined
                    ) {
                        const val = tableMeta.rowData[column.statusIndex];

                        column.disabledOn.map((item, index) => {
                            if (item === val) {
                                displayNone = 'none';
                            }
                        });
                    }

                    return (
                        <span
                            onClick={(event) => {
                                column.callback(value);
                            }}
                            style={{ display: displayNone }}
                        >
                            <CustomIcon icon={column.icon} />
                        </span>
                    );
                }
            }
        };
    };

    const iconButtonWithRow = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <span
                            style={{ display: 'block', width: '80px' }}
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            <CustomIcon icon={column.icon} />
                        </span>
                    );
                }
            }
        };
    };

    const deleteRow = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <DeleteIcon
                            title="Delete"
                            color="secondary"
                            variant="contained"
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        />
                    );
                }
            }
        };
    };

    const transfer = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Icon
                            style={{ display: 'inline-table', width: '80px' }}
                            title={
                                column.buttonTitle
                                    ? column.buttonTitle
                                    : 'Transfer'
                            }
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            <TransferIcon />
                        </Icon>
                    );
                }
            }
        };
    };

    const tableButton = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Icon
                            style={{
                                display: 'inline',
                                height: 50,
                                width: 50
                            }}
                            title={
                                column.buttonTitle
                                    ? column.buttonTitle
                                    : 'Transfer'
                            }
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            <CustomIcon icon={column.icon} />
                        </Icon>
                    );
                }
            }
        };
    };

    const buyButton = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <AddShoppingCartIcon
                            title={
                                column.buttonTitle ? column.buttonTitle : 'Buy'
                            }
                            color="primary"
                            variant="contained"
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        />
                    );
                }
            }
        };
    };
    const update = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <EditIcon
                            title={
                                column.buttonTitle
                                    ? column.buttonTitle
                                    : 'Update'
                            }
                            color="primary"
                            variant="contained"
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        />
                    );
                }
            }
        };
    };
    const refresh = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <RefreshIcon
                            title={
                                column.buttonTitle
                                    ? column.buttonTitle
                                    : 'Update'
                            }
                            color="primary"
                            variant="contained"
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        />
                    );
                }
            }
        };
    };
    const save = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Fragment>
                            <SaveIcon
                                title={
                                    column.buttonTitle
                                        ? column.buttonTitle
                                        : 'Update'
                                }
                                color="primary"
                                variant="contained"
                                onClick={(event) => {
                                    column.callback(tableMeta.rowData);
                                }}
                            />
                        </Fragment>
                    );
                }
            }
        };
    };

    const ButtonWithTitle = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Button
                            variant="contained"
                            color={column.color ? column.color : 'primary'}
                            style={{ textTransform: 'capitalize', }}
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            {column.buttonTitle
                                ? column.buttonTitle
                                : tableMeta.columnData.label}
                        </Button>
                    );
                }
            }
        };
    };

    const DeleteButtonWithTitle = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Button
                            variant="contained"
                            color={column.color ? column.color : 'secondary'}
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            {column.buttonTitle
                                ? column.buttonTitle
                                : tableMeta.columnData.label}
                        </Button>
                    );
                }
            }
        };
    };

    const hiddenbutton = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Button
                            type="button"
                            variant="outlined"
                            style={{ padding: 0 }}
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            <div
                                className={
                                    classes.numbericRightAlignWithoutPadding
                                }
                            >
                                {value}
                            </div>
                        </Button>
                    );
                }
            }
        };
    };

    const button = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let displayNone = '';

                    if (
                        column.disabledOn !== undefined &&
                        column.statusIndex !== undefined
                    ) {
                        const val = tableMeta.rowData[column.statusIndex];

                        column.disabledOn.map((item, index) => {
                            if (item === val) {
                                displayNone = 'none';
                            }
                        });
                    }

                    return (
                        <Button
                            variant="contained"
                            color={column.color ? column.color : 'primary'}
                            onClick={(event) => {
                                column.callback(value);
                            }}
                            style={{ display: displayNone, margin: 8, textTransform: 'capitalize', }}
                        >
                            {column.buttonTitle
                                ? column.buttonTitle
                                : tableMeta.columnData.label}
                        </Button>
                    );
                }
            }
        };
    };
    const testbutton = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    let displayNone = '';

                    if (
                        column.disabledOn !== undefined &&
                        column.statusIndex !== undefined
                    ) {
                        const val = tableMeta.rowData[column.statusIndex];

                        column.disabledOn.map((item, index) => {
                            if (item === val) {
                                displayNone = 'none';
                            }
                        });
                    }

                    return (
                        <Button
                            variant="contained"
                            color={column.color ? column.color : 'primary'}
                            onClick={(event) => {
                                column.callback(value, tableMeta.rowData);
                            }}
                            style={{ display: displayNone, margin: 8 }}
                        >
                            {column.buttonTitle
                                ? column.buttonTitle
                                : tableMeta.columnData.label}
                        </Button>
                    );
                }
            }
        };
    };

    const buttonOrLabel = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <>
                            {value === false ? (
                                <Button
                                    variant="contained"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={(event) => {
                                        column.callback(tableMeta.rowData);
                                    }}
                                >
                                    {column.buttonTitle
                                        ? column.buttonTitle
                                        : 'Update'}
                                </Button>
                            ) : (
                                <span className={classes.card_sequence}>
                                    {column.label}
                                </span>
                            )}
                        </>
                    );
                }
            }
        };
    };

    const buttonOrDMTVerify = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <>
                            {value === false ? (
                                <Button
                                    variant="contained"
                                    className={classes.CustomPrimaryButtonStyle}
                                    onClick={(event) => {
                                        column.callback(tableMeta.rowData);
                                    }}
                                >
                                    {column.buttonTitle
                                        ? column.buttonTitle
                                        : 'Update'}
                                </Button>
                            ) : (
                                <span className={classes.card_sequence}>
                                    Verified
                                </span>
                            )}
                        </>
                    );
                }
            }
        };
    };
    
    const labelWithToolTipFunc = (column) => {
        return {
            options: {
                filter: true,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Tooltip title={value.Description}>
                            <span>{value.KeyText}</span>
                        </Tooltip>
                    );
                }
            }
        };
    };

    const activate = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    const id = tableMeta.rowData[column.primaryKeyIndex];

                    return (
                        <FormControlLabel
                            value={value ? 'Yes' : 'No'}
                            control={
                                <Switch
                                    size="small"
                                    color="primary"
                                    checked={value}
                                    value={value ? 'Yes' : 'No'}
                                />
                            }
                            onChange={(event) => {
                                var newValue =
                                    event.target.value === 'Yes' ? false : true;
                                updateValue(newValue);
                                column.callback(id, newValue);
                            }}
                        />
                    );
                }
            }
        };
    };
    const setUp = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    const id = tableMeta.rowData[column.primaryKeyIndex];
                    const SecondaryVal =
                        tableMeta.rowData[column.secondaryKeyIndex];

                    return value ? (
                        <SettingsIcon
                            color="primary"
                            onClick={(event) => {
                                column.callback(id, SecondaryVal);
                            }}
                        />
                    ) : (
                        '---'
                    );
                }
            }
        };
    };
    const Downline = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    const id = tableMeta.rowData[column.primaryKeyIndex];
                    return value === DISTRIBUTOR ||
                        value === SUPER_DISTRIBUTOR ? (
                        <SettingsInputComponentIcon
                            color="primary"
                            onClick={(event) => {
                                column.callback(id);
                            }}
                        />
                    ) : (
                        '--'
                    );
                }
            }
        };
    };
    const toggle = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    const id = tableMeta.rowData[column.primaryKeyIndex];
                    const TrueValue = column.trueValue;
                    const FalseValue = column.falseValue;

                    return (
                        <ToggleButtonGroup
                            aria-label="Types"
                            value={value}
                            exclusive
                            onChange={(event, newValue) => {
                                updateValue(newValue);
                                column.callback(id, newValue);
                            }}
                        >
                            <ToggleButton value={TrueValue}>
                                {TrueValue}
                            </ToggleButton>
                            <ToggleButton value={FalseValue}>
                                {FalseValue}
                            </ToggleButton>
                        </ToggleButtonGroup>
                    );
                }
            }
        };
    };

    const activateRow = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    const id = tableMeta.rowData[column.primaryKeyIndex];
                    return (
                        <FormControlLabel
                            value={value ? 'Yes' : 'No'}
                            control={
                                <Switch
                                    color="primary"
                                    checked={value}
                                    value={value ? 'Yes' : 'No'}
                                />
                            }
                            onChange={(event) => {
                                var newValue =
                                    event.target.value === 'Yes' ? false : true;
                                updateValue(newValue);
                                column.callback(
                                    id,
                                    newValue,
                                    tableMeta.rowData
                                );
                            }}
                        />
                    );
                }
            }
        };
    };
    const textwrap = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Tooltip title={value}>
                            <div
                                style={{
                                    width: column.textwrapwidth
                                        ? column.textwrapwidth
                                        : 100,
                                    whiteSpace: 'nowrap'
                                }}
                            >
                                <Box
                                    component="div"
                                    my={2}
                                    textOverflow="ellipsis"
                                    overflow="hidden"
                                >
                                    {value}
                                </Box>
                            </div>
                        </Tooltip>
                    );
                }
            }
        };
    };

    const hide = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                viewColumns: false,
                display: column.hide ? false : true
            }
        };
    };

    const approval = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return value ? (
                        <DoneIcon color="primary" />
                    ) : (
                        <CancelIcon color="error" />
                    );
                }
            }
        };
    };
    const download = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    if (value === undefined) return null;
                    if (value === null) return null;
                    if (value.length <= 2) return null;

                    return (
                        <a
                            href={value}
                            rel="noopener noreferrer"
                            target="_blank"
                            download
                        >
                            <CloudDownloadIcon />
                        </a>
                    );
                }
            }
        };
    };

    const textArea = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <TextField
                            multiline={true}
                            rows={column.rows ? column.rows : 4}
                            defaultValue={value}
                            inputProps={{ style: { fontSize: 12 } }}
                        />
                    );
                }
            }
        };
    };

    const editable = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <div>
                            <TextField
                                variant="outlined"
                                margin="dense"
                                multiline={true}
                                fullWidth
                                rowsMin={4}
                                rowsMax={6}
                                inputProps={{
                                    style: {
                                        fontSize: 14,
                                        width: '-webkit-fill-available'
                                    }
                                }}
                                color="primary"
                                value={value}
                                onChange={(event) => {
                                    let newValue = event.target.value;
                                    updateValue(newValue);
                                    let {
                                        rowIndex,
                                        columnIndex,
                                        columnData,
                                        rowData,
                                        tableData,
                                        tableState,
                                        currentTableData
                                    } = tableMeta;

                                    if (
                                        currentTableData !== null &&
                                        rowIndex !== null &&
                                        columnIndex !== null
                                    ) {
                                        handleTableUpdate(
                                            currentTableData,
                                            tableData,
                                            rowIndex,
                                            columnIndex,
                                            column.name,
                                            newValue
                                        );
                                    }
                                }}
                            />
                        </div>
                    );
                }
            }
        };
    };

    const editableWithPinOnly = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <TextField
                            type="password"
                            variant="outlined"
                            margin="dense"
                            autoComplete="false"
                            fullWidth
                            inputProps={{
                                style: { fontSize: 12 },
                                maxLength: column.length ? column.length : null,
                                autocomplete: 'new-password',
                                form: {
                                    autocomplete: 'off'
                                }
                            }}
                            color="primary"
                            value={value}
                            onChange={(event) => {
                                const { value: valueNumeric } = event.target;
                                const onlyNums = valueNumeric.replace(
                                    /[^0-9]/g,
                                    ''
                                );

                                const reg = /^-?[0-9]*(\.[0-9]*)?$/;

                                if (
                                    (!isNaN(onlyNums) && reg.test(onlyNums)) ||
                                    onlyNums === '' ||
                                    onlyNums === '-'
                                ) {
                                    updateValue(onlyNums);

                                    let {
                                        rowIndex,
                                        columnIndex,
                                        tableData
                                    } = tableMeta;

                                    handleTableUpdate(
                                        tableData,
                                        rowIndex,
                                        columnIndex,
                                        column.name,
                                        event.target.value
                                    );
                                } else return;
                            }}
                        />
                    );
                }
            }
        };
    };

    const editableTextBoxOrPasswordFunc = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <TextField
                            type={value.IsPassword ? 'password' : 'text'}
                            variant="outlined"
                            margin="dense"
                            autoComplete="false"
                            multiline={value.IsPassword ? false : true}
                            rowsMin={4}
                            rowsMax={6}
                            fullWidth
                            inputProps={{
                                autocomplete: 'new-password',
                                form: {
                                    autocomplete: 'off'
                                },
                                style: {
                                    fontSize: 14,
                                    width: '-webkit-fill-available'
                                }
                            }}
                            color="primary"
                            value={value.ValueText}
                            onChange={(event) => {
                                let newValue = event.target.value;
                                updateValue(newValue);
                                let {
                                    rowIndex,
                                    columnIndex,
                                    columnData,
                                    rowData,
                                    tableData,
                                    tableState,
                                    currentTableData
                                } = tableMeta;

                                if (
                                    currentTableData !== null &&
                                    rowIndex !== null &&
                                    columnIndex !== null
                                ) {
                                    handleTableUpdate(
                                        currentTableData,
                                        tableData,
                                        rowIndex,
                                        columnIndex,
                                        column.name,
                                        newValue
                                    );
                                }
                            }}
                        />
                    );
                }
            }
        };
    };
    const editableWithNumberOnly = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Grid container spacing={1}>
                            <Grid item xs={12} md={12} lg={12} sm={12}>
                                <TextField
                                    variant="outlined"
                                    margin="dense"
                                    multiline={true}
                                    rowsMax={4}
                                    fullWidth
                                    inputProps={{
                                        style: {
                                            fontSize: 20,
                                            textAlign: 'right',
                                            width: '-webkit-fill-available'
                                        },
                                        maxLength: column.length
                                            ? column.length
                                            : null
                                    }}
                                    color="primary"
                                    value={value}
                                    onChange={(event) => {
                                        const {
                                            value: valueNumeric
                                        } = event.target;

                                        const onlyNums = valueNumeric.replace(
                                            /[^0-9]/g,
                                            ''
                                        );

                                        const reg = /^-?[0-9]*(\.[0-9]*)?$/;

                                        if (
                                            (!isNaN(onlyNums) &&
                                                reg.test(onlyNums)) ||
                                            onlyNums === '' ||
                                            onlyNums === '-'
                                        ) {
                                            updateValue(onlyNums);

                                            let {
                                                rowIndex,
                                                columnIndex,
                                                tableData
                                            } = tableMeta;

                                            handleTableUpdate(
                                                tableData,
                                                rowIndex,
                                                columnIndex,
                                                column.name,
                                                event.target.value
                                            );
                                        } else {
                                            return;
                                        }
                                    }}
                                />
                            </Grid>
                        </Grid>
                    );
                }
            }
        };
    };

    const ReadOnlyTextArea = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Grid container spacing={1}>
                            <Grid item xs={12} sm={12} md={12} lg={12}>
                                <div
                                    style={{
                                        whiteSpace: 'normal',
                                        fontSize: '12px',
                                        wordBreak: 'break-word',
                                        width: 'auto'
                                    }}
                                >
                                    {value}
                                </div>
                            </Grid>
                        </Grid>
                    );
                }
            }
        };
    };

    const MoreMenu = (column) => {
        return {
            options: {
                setCellHeaderProps: column.setCellHeaderProps
                    ? column.setCellHeaderProps
                    : null,
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <MoreDetail
                            options={column.options}
                            value={tableMeta.rowData}
                        />
                    );
                }
            }
        };
    };

    const changePage = (page, sortOrder) => {
        setCurrentPage(page);
        if (getCallback !== undefined) {
            getCallback({
                currentPage: page,
                RowsPerPage: rowsPerPage
            });
        }
    };

    const searchAction = (SearchText) => {
        if (getSearchCallback !== undefined) {
            getSearchCallback({
                SearchText: SearchText
            });
        }
    };

    const sortAction = (page, sortOrder) => {
        const { name, direction } = sortOrder;

        if (getSortCallback !== undefined) {
            getSortCallback({
                column: name,
                direction: direction
            });
        }
    };
    const changeRowsPerPageAction = (page, rowsPerPage) => {
        setCurrentPage(page);
        if (getCallback !== undefined) {
            getCallback({
                currentPage: page,
                RowsPerPage: rowsPerPage
            });
        }
    };
    const sumColumns = (startIndex, endIndex, key) => {
        let response = data
            .slice(startIndex, endIndex)
            .map((a) => a[key])
            .reduce((total, salery) => (total += salery), 0);

        return response;
    };

    const rowSelection = (tableState, selectedRows) => {
        let data = tableState.data;
        let columns = tableState.columns;

        let tabData = [];
        for (let c = 0; c < selectedRows.length; c++) {
            let { dataIndex } = selectedRows[c];
            tabData.push(data.filter((x) => x.index === dataIndex));
        }

        let updatedTab = [];
        for (let i = 0; i < tabData.length; i++) {
            let rows = [];
            let [data] = tabData[i];
            let d = data.data;
            for (let j = 0; j < d.length; j++) {
                let { name } = columns[j];
                let key = name;
                let value = d[j];

                rows.push({ column: key, value: value });
            }
            updatedTab.push(rows);
        }
        setUpdatedData(updatedTab);
    };

    const returnUpdatedData = (tableState) => {
        if (isSelectableRows !== 'none' && shouldGetUpdatedData) {
            getUpdatedData(updatedData);
            return;
        }

        let tableData = tableState.data;
        if (tableData.length <= 0) return;

        let tab = [];
        if (!shouldGetUpdatedData) return;
        if (shouldGetUpdatedData === true) {
            let tableDataLength = tableState.data.length;
            let columnData = tableState.columns;
            for (let i = 0; i < tableDataLength; i++) {
                let newObj = {};
                let { data } = tableData[i];
                for (let j = 0; j < data.length; j++) {
                    let { name } = columnData[j];
                    let value = data[j];
                    newObj[name] = value;
                }

                tab.push(newObj);
            }

            getUpdatedData(tab);
        }
    };

    const globalOptions = {
        customSearchRender: debounceSearchRender(500),
        print: false,
        download: false,
        filter: showHeader,
        searchOpen: showHeader,
        customToolbar: showHeader,
        search: showHeader,
        viewColumns: showHeader,
        filterType: 'dropdown',
        responsive: 'standard', //'vertical scrollFullHeight',
        serverSide: serverSide !== undefined ? serverSide : false,

        count:
            count !== undefined
                ? count
                : data !== undefined
                ? data.length
                : null,
        rowsPerPage: rowsPerPage !== undefined ? rowsPerPage : 10,
        rowsPerPageOptions: [10, 50, 75, 100, 1000],
        selectableRows: isSelectableRows ? isSelectableRows : 'none',

        fixedSelectColumn: false,
        fixedHeader: true,

        onTableChange: (action, tableState) => {
            switch (action) {
                case 'changePage':
                    changePage(tableState.page, tableState.sortOrder);
                    break;
                case 'sort':
                    sortAction(tableState.page, tableState.sortOrder);
                    break;
                case 'search':
                    searchAction(tableState.searchText);
                    break;
                case 'changeRowsPerPage':
                    changeRowsPerPageAction(
                        tableState.page,
                        tableState.rowsPerPage
                    );
                    break;
                case 'rowSelectionChange':
                    rowSelection(tableState, tableState.selectedRows.data);
                    break;
                case 'propsUpdate':
                    returnUpdatedData(tableState);
                    break;

                default:
            }
        },
        customToolbar: (tableState) => {
            return <Fragment></Fragment>;
        },

        customToolbarSelect: (selectedRows) => <Fragment></Fragment>,
        customFooter: (
            count,
            page,
            rowsPerPage,
            changeRowsPerPage,
            changePage
        ) => {
            const startIndex = page * rowsPerPage;
            const endIndex = (page + 1) * rowsPerPage;
            const handleRowChange = (event) => {
                changeRowsPerPage(event.target.value);
            };

            return (
                <TableFooter className="px-4">
                    {showFooter ? (
                        <>
                            <TableRow>
                                {sumkeys === undefined || sumkeys === null
                                    ? null
                                    : sumkeys.map((key, index) => {
                                          return (
                                              <TableCell
                                                  colSpan="3"
                                                  key={index}
                                              >
                                                  <b
                                                      style={{
                                                          fontSize: '15px',
                                                          color: 'blue',
                                                          margin: 0,
                                                          padding: 0
                                                      }}
                                                  >
                                                      {key.label} :
                                                      {sumColumns(
                                                          startIndex,
                                                          endIndex,
                                                          key.key
                                                      )}
                                                  </b>
                                              </TableCell>
                                          );
                                      })}
                            </TableRow>
                            <TableRow>
                                <TablePagination
                                    page={page}
                                    count={count}
                                    rowsPerPage={rowsPerPage}
                                    onChangeRowsPerPage={handleRowChange}
                                    onChangePage={(_, page) => changePage(page)}
                                />
                            </TableRow>
                        </>
                    ) : null}
                </TableFooter>
            );
        }
    };

    return (
        <MuiThemeProvider theme={getMuiTheme(showHeader)}>
            <Grid container direction="row">
                <Card className="w-full maintable" elevation={6}>
                    <MUIDataTable
                        innerRef={tableRef}
                        title={title}
                        data={tableData}
                        columns={mainColumns}
                        options={globalOptions}
                        className="crud-table whitespace-pre min-w-3000"
                    />
                </Card>
            </Grid>
        </MuiThemeProvider>
    );
};

export default ReactTable;
