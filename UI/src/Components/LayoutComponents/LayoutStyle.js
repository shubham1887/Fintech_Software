import { APP_BAR_COLOR, MENU_FORE_COLOR, PRIMARY_COLOR } from 'Config';
import { makeStyles } from '@material-ui/core/styles';
const drawerWidth = 240;
const LayoutStyles = makeStyles((theme) => ({
    content: {
        flexGrow: 1,
        display: 'flex',
        flexDirection: 'column',
        paddingTop: theme.spacing(3),
        overflowX: 'inherit',
        //overflowX: 'hidden',
        width: '100% !important'
        // paddingLeft: '4px !important',
        // paddingRight: '4px !important',
    },
    fullWidth: {
        [theme.breakpoints.down('lg')]: {
            maxWidth: '100% !important'
        },
        [theme.breakpoints.up('lg')]: {
            maxWidth: '100% !important'
        }
    },
    maxWidthLg: {
        [theme.breakpoints.down('lg')]: {
            maxWidth: '100% !important'
        },
        [theme.breakpoints.up('lg')]: {
            maxWidth: '100% !important'
        }
    },
    grow: {
        flexGrow: 1
    },
    root: {
        display: 'flex'
    },

    rightAlignText: {
        textAlign: 'right',
        float: 'left'
    },

    table: {
        display: 'inline-flex'
    },
    tableRow: {
        textAlign: 'right',
        marginRight: '10px'
    },
    tableCell: {
        float: 'left',
        margin: '0 5px 0 0'
    },
    innerlogo: {
        background: '#fff',
        padding: '9px 10px'
    },
    navbarTogglerColor: {
        color: MENU_FORE_COLOR() + ' !important',
        background: APP_BAR_COLOR
    },
    appBar: {
        zIndex: theme.zIndex.drawer + 1,
        background: APP_BAR_COLOR,
        color: '#fff',
        transition: theme.transitions.create(['width', 'margin'], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen
        })
    },

    appBarShift: {
        marginLeft: drawerWidth,
        width: `calc(100% - ${drawerWidth}px)`,
        transition: theme.transitions.create(['width', 'margin'], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen
        })
    },
    menuButton: {
        marginRight: 36
    },
    hide: {
        display: 'none'
    },
    drawer: {
        width: drawerWidth,
        flexShrink: 0,
        whiteSpace: 'nowrap'
    },
    drawerPaper: {
        width: drawerWidth,
        [theme.breakpoints.up('md')]: {
            position: 'relative'
        }
    },
    drawerOpen: {
        width: drawerWidth,
        transition: theme.transitions.create('width', {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen
        })
    },
    drawerClose: {
        transition: theme.transitions.create('width', {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen
        }),
        overflowX: 'hidden',
        width: theme.spacing(7) + 1,
        [theme.breakpoints.up('sm')]: {
            width: theme.spacing(7) + 1
        }
    },
    toolbar: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'flex-end',
        backgroundColor: '#ffffff',

        ...theme.mixins.toolbar
    },

    sectionDesktop: {
        display: 'none',
        [theme.breakpoints.up('md')]: {
            display: 'flex'
        }
    },
    sectionContent: {
        margin: '5px',
        textAlign: 'center'
    },
    sectionContentColumn: {
        margin: '5px',
        textAlign: 'right',
        flexDirection: 'row',
        display: 'block ruby',
        right: '25px',
        position: 'absolute',
        fontSize: '17px',
        [theme.breakpoints.down('sm')]: {
            position: 'inherit'
        }
    },
    sectionMobile: {
        display: 'flex',
        position: 'fixed',
        top: '8px',
        right: '0',
        [theme.breakpoints.up('md')]: {
            display: 'none'
        }
    },
    backColor: {
        backgroundColor: '#e0e0e0'
    },
    bottomButton: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'flex-end',
        backgroundColor: '#d1d3d5',
        color: '#fff',
        padding: theme.spacing(0, 1),
        bottom: '0',
        position: 'sticky'
    },
    customBadge: {
        backgroundColor: PRIMARY_COLOR(),
        color: '#fff',
        marginTop: '4px'
    },
    NewsBadge: {
        backgroundColor: APP_BAR_COLOR,
        color: MENU_FORE_COLOR()
    },
    allValues: { display: 'inline-flex' }
}));

export default LayoutStyles;
