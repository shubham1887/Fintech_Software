import React, { useState, useEffect } from 'react';
import MUIDataTable from 'mui-datatables';
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
    Grid,
    TablePagination
} from '@material-ui/core';
import { createMuiTheme, MuiThemeProvider } from '@material-ui/core/styles';
import CloudDownloadIcon from '@material-ui/icons/CloudDownload';
import DeleteIcon from '@material-ui/icons/Delete';
import EditIcon from '@material-ui/icons/Edit';
import { CustomIcon } from 'Components/CustomIcons';
import MoreDetail from 'Components/MenuBar/MoreDetail';
import { commonStyle } from 'Components/CustomMaterialStyleSheets/CommonStyleSheet';

import { CustomStatus } from './CustomStatus';
import { ListInsideTable } from './ListInsideTable';
import { DropDownInsideTable } from './DropDownInsideTable';

const getMuiTheme = () =>
    createMuiTheme({
        typography: {
            fontFamily: [
                '"Montserrat"',
                'Helvetica',
                'Arial',
                'sans-serif'
            ].join(','),
            h6: {
                color: 'red'
            }
        },

        overrides: {
            MUIDataTableBodyCell: {
                root: {
                    paddingLeft: '8px',
                    paddingRight: '0px',
                    paddingTop: '0px',
                    paddingBottom: '0px',
                    lineHeight: 'min(10vh, 2.0em)'
                },
                stackedCommon: {
                    '@media (max-width:959.95px)': {
                        height: '100%'
                    }
                }
            },
            MUIDataTableHeadCell: {
                fixedHeader: {
                    backgroundColor: '#e0e0e0',
                    fontWeight: 'bold',
                    borderBottom: 'none'
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
            MUIDataTableBodyRow: {
                root: {
                    '&:nth-child(even)': {
                        backgroundColor: '#e0e0e0'
                    }
                }
            },
            MuiTableCell: {
                root: {
                    lineHeight: 2.7
                }
            },
            MuiSvgIcon: {
                root: {
                    verticalAlign: 'middle'
                }
            }
        }
    });

export const NewTable = ({
    title,
    data,
    columns,
    count,
    rowsPerPage,
    getCallback,
    getSearchCallback,
    getSortCallback,
    sumkeys
}) => {
    const classes = commonStyle();
    const [mainColumns, setMainColumns] = useState([]);
    const [currentPage, setCurrentPage] = useState(0);

    //var sumkeys = ['DMTBalance', 'RechargeBalance'];

    useEffect(() => {
        createColumns(columns);
    }, [columns.length]);

    const createColumns = (tableColumns) => {
        var columns = [];

        tableColumns.map((column, index) => {
            columns.push({
                ...column,
                ...(column.numeric ? numeric(column) : null),
                ...(column.srno ? srNo() : null),
                ...(column.customStatus ? status() : null),
                ...(column.array ? array(column) : null),
                ...(column.edit ? edit(column) : null),
                ...(column.editable ? editable(column) : null),
                ...(column.ReadOnlyTextArea ? ReadOnlyTextArea(column) : null),

                ...(column.iconButton ? iconButton(column) : null),
                ...(column.iconButtonWithRow
                    ? iconButtonWithRow(column)
                    : null),
                ...(column.delete ? deleteRow(column) : null),
                ...(column.button ? button(column) : null),
                ...(column.buttonOrLabel ? buttonOrLabel(column) : null),
                ...(column.activate ? activate(column) : null),
                ...(column.textwrap ? textwrap(column) : null),
                ...(column.update ? update(column) : null),
                ...(column.dropdown ? dropdown(column) : null),
                ...(column.hide ? hide(column) : null),
                ...(column.download ? download(column) : null),
                ...(column.textArea ? textArea(column) : null),
                ...(column.MoreMenu ? MoreMenu(column) : null)
            });
        });

        setMainColumns(columns);
    };

    const srNo = () => {
        return {
            options: {
                filter: false,
                sort: false,
                customBodyRender: (value, tableMeta, updateValue) => {
                    const rowId = tableMeta.rowIndex;

                    return <label>{rowId + 1}</label>;
                }
            }
        };
    };

    const array = (column) => {
        return {
            options: {
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
                customBodyRender: (value, tableMeta, updateValue) => {
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
                                    {value !== null || value !== undefined
                                        ? value.toFixed(2)
                                        : null}
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
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <EditIcon
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
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <span
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
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <DeleteIcon
                            color="secondary"
                            variant="contained"
                            onClick={(event) => {
                                column.callback(value);
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
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Button
                            variant="contained"
                            color={
                                column.color !== undefined
                                    ? column.color
                                    : 'primary'
                            }
                            onClick={(event) => {
                                column.callback(tableMeta.rowData);
                            }}
                        >
                            {column.buttonTitle ? column.buttonTitle : 'Update'}
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
                            style={{ display: displayNone }}
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

    const activate = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
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
                                column.callback(id, newValue);
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
                                <Box component="div" my={2}>
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
                display: column.hide ? false : true
            }
        };
    };

    const download = (column) => {
        return {
            options: {
                filter: false,
                sort: false,
                download: false,
                customBodyRender: (value, tableMeta, updateValue) => {
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
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <TextField
                            multiline={true}
                            rows={column.rows ? column.rows : 4}
                            defaultValue={value}
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
                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <Grid item xs={12} sm={12} md={12} lg={12}>
                            <TextField
                                multiline={true}
                                fullWidth
                                fontSize={16}
                                color="primary"
                                value={value}
                                onChange={(event) => {
                                    updateValue(event.target.value);
                                }}
                            />
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

                customBodyRender: (value, tableMeta, updateValue) => {
                    return (
                        <TextField
                            multiline={true}
                            readOnly={true}
                            rowsMax={4}
                            fullWidth
                            fontSize={10}
                            color="primary"
                            value={value}
                        />
                    );
                }
            }
        };
    };

    const MoreMenu = (column) => {
        return {
            options: {
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
        getCallback({
            currentPage: page,
            RowsPerPage: rowsPerPage
        });
    };

    const searchAction = (SearchText) => {
        getSearchCallback({
            SearchText: SearchText
        });
    };

    const sortAction = (page, sortOrder) => {
        const { name, direction } = sortOrder;

        getSortCallback({
            column: name,
            direction: direction
        });
    };
    const options = {
        filter: true,
        filterType: 'dropdown',
        responsive: 'vertical',
        serverSide: true,
        count: count,
        rowsPerPage: rowsPerPage,
        rowsPerPageOptions: [10, 50, 75, 100],
        selectableRows: 'none',
        searchOpen: true,
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
                case 'changeRowsPerPage':
                    break;
                case 'search':
                    searchAction(tableState.searchText);

                    break;
                case 'rowDelete':
                    break;

                default:
            }
        },
        customFooter: (
            count,
            page,
            rowsPerPage,
            changeRowsPerPage,
            changePage
        ) => {
            const startIndex = page * rowsPerPage;
            const endIndex = (page + 1) * rowsPerPage;
            return (
                <TableFooter>
                    <TableRow>
                        {sumkeys === undefined || sumkeys === null
                            ? null
                            : sumkeys.map((key, index) => {
                                  return (
                                      <TableCell colSpan="3" key={index}>
                                          <b
                                              style={{
                                                  fontSize: '15px',
                                                  color: 'blue'
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
                            onChangePage={(_, page) => changePage(page)}
                        />
                    </TableRow>
                </TableFooter>
            );
        }
    };

    const sumColumns = (startIndex, endIndex, key) => {
        let response = data
            .slice(startIndex, endIndex)
            .map((a) => a[key])
            .reduce((total, salery) => (total += salery), 0);

        return response;
    };
    return (
        <MuiThemeProvider theme={getMuiTheme()}>
            <MUIDataTable
                title={title}
                data={data}
                columns={mainColumns}
                options={options}
                className="table nowrap"
                style={{
                    marginTop: '5px'
                }}
            />
        </MuiThemeProvider>
    );
};
