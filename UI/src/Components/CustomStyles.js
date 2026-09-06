import { makeStyles } from '@material-ui/core/styles';

const PrimaryColor = '#ec008c';

const CustomStyles = makeStyles((theme) => ({
    paper: {
        marginTop: theme.spacing(8),
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center'
    },
    waves: {
        backgroundImage: `url(${Image})`,
        backgroundRepeat: 'no-repeat',
        backgroundPosition: 'top center',
        backgroundSize: 'cover',
        height: '250px',
        width: '100%'
    },
    avatar: {
        margin: theme.spacing(1),
        backgroundColor: theme.palette.secondary.main
    },
    form: {
        width: '100%', // Fix IE 11 issue.
        marginTop: theme.spacing(1)
    },
    submit: {
        margin: theme.spacing(3, 0, 2),
        backgroundColor: '#ec008c',
        fontFamily: 'poppinssemibold',
        fontSize: '1.2rem',
        boxShadow: 'none'
    },
    forgotpass: {
        borderBottom: '1px dashed #d7d7d7',
        color: '#6a6a6a',
        '&:hover': {
            color: '#ec008c',
            borderBottom: '1px dashed #ec008c',
            textDecoration: 'none'
        }
    },
    signup: { marginTop: '25px', marginBottom: '10px' },
    policy: {
        marginBottom: '20px',
        textAlign: 'center',
        color: '#fff',
        paddingTop: '30px'
    },
    policylink: {
        color: '#fff !important'
    },
    loginform: { paddingRight: '30px', paddingLeft: '30px' },
    version: { color: '#fff', fontFamily: 'poppinsmedium', marginTop: '20px' },
    drawerclose: { textAlign: 'right' },

    menuButton: {
        marginRight: theme.spacing(2)
    },
    title: {
        flexGrow: 1
    },

    toolbar: {
        flexWrap: 'wrap'
    },
    toolbarTitle: {
        flexGrow: 1
    },
    link: {
        margin: theme.spacing(1, 1.5),
        fontFamily: 'poppinsmedium',
        textTransform: 'capitalize',
        color: '#77838f',
        fontSize: '15px',
        padding: '5px 10px;',
        '&:hover': {
            color: '#fff',
            backgroundColor: PrimaryColor,
            textDecoration: 'none'
        }
    },
    loginbtn: {
        backgroundColor: PrimaryColor,
        color: '#fff',
        '&:hover': {
            color: PrimaryColor
        }
    },

    mainhead: {
        fontFamily: 'poppinsbold',
        fontSize: '50px',
        color: '#000',
        marginTop: '150px',
        marginBottom: '20px'
    },
    mainheadspan: { color: PrimaryColor },

    mainheadtypo: {
        fontFamily: 'poppinsregular',
        fontSize: '16px',
        color: '#606060'
    },

    tab: { marginTop: '100px', textAlign: 'center' }
}));

export default CustomStyles;
