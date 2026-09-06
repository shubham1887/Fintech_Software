import { makeStyles } from '@material-ui/core/styles';
import { green } from '@material-ui/core/colors';

import {
    PRIMARY_COLOR,
    PRIMARY_HOVER_COLOR,
    SECONDARY_COLOR,
    SECONDARY_HOVER_COLOR,
    RESET_COLOR,
    MENU_FORE_COLOR
} from 'Config';
import { fontSize, fontStyle, width } from '@material-ui/system';
import { Button } from '@material-ui/core';

export const commonStyle = makeStyles((theme) => ({
    imageIcon: {
        height: '100%'
    },

    NewsMessage: {
        color: PRIMARY_COLOR,
        font: '500 16px Roboto,RobotoDraft,Helvetica,Arial,sans-serif'
    },

    twofa: { textAlign: 'right' },

    lasttwofa: { marginRight: 10 },

    labellasttwofa: {
        fontWeight: 700
    },

    title: {
        paddingTop: 10,
        marginBottom: 10,
        paddingLeft: 10,
        backgroundColor: PRIMARY_COLOR,
        color: '#fff',
        textAlign: 'center'
    },

    iconRoot: {
        textAlign: 'center'
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
    LinkButton: {
        background: '#0277bd',
        color: '#fff',
        cursor: 'pointer'
    },

    TestResponse: {
        color: '#0000ff',
        display: 'inline-block'
    },

    PaperHead_h2: {
        alignItems: 'center',
        display: 'flex',
        minHeight: '36px',
        fontWeight: '400',
        font: '500 20px Roboto,RobotoDraft,Helvetica,Arial,sans-serif',
        marginRight: '16px',
        marginLeft: '0px',
        marginTop: '10px',
        marginBottom: '5px'
    },
    Require: {
        color: '#ff0000',
        padding: 8
    },

    document_h2: {
        alignItems: 'center',
        display: 'flex',
        minHeight: '36px',
        fontWeight: '400',
        font: '500 20px Roboto,RobotoDraft,Helvetica,Arial,sans-serif',
        marginRight: '16px',
        marginLeft: '0px',
        marginTop: '10px',
        marginBottom: '5px',
        color: PRIMARY_COLOR
    },
    action_url: {
        color: '#A60FC8'
    },
    callback_url: {
        color: '#0000FF'
    },
    normalTextColor: {
        color: '#000000'
    },
    document_h3: {
        alignItems: 'center',
        display: 'flex',
        minHeight: '20px',
        fontWeight: '400',
        font: '500 20px Roboto,RobotoDraft,Helvetica,Arial,sans-serif',
        color: PRIMARY_COLOR,

        fontSize: 16
    },
    imageContainer: {
        display: 'block',
        alignItems: 'center',
        justifyContent: 'center',
        marginLeft: 'auto',
        marginRight: 'auto',
        border: '1px solid',
        paddingTop: '2px',
        textAlign: 'center'
    },
    imageHeader: {
        textAlign: 'center', // <-- the magic
        fontWeight: 'bold',
        color: '#000',
        marginTop: '5px'
    },
    imageFooter: {
        paddingTop: '2px',
        textAlign: 'center', // <-- the magic
        fontWeight: 'bold',
        color: '#000'
    },
    document_h4: {
        backgroundColor: '#e0e0e0',
        fontWeight: 'normal',
        fontSize: 16
    },

    PaperHead_Blank: {
        alignItems: 'center',
        display: 'flex',
        marginRight: '16px',
        marginLeft: '0px',
        marginTop: '8px',
        marginBottom: '4px'
    },

    CustomImageButtonStyle: {
        marginLeft: '10px',
        marginRight: '10px',
        cursor: 'pointer',
        border: 'none',
        textAlign: 'center',
        minHeight: '36px',
        minWidth: '36px',
        display: 'inline-block',
        verticalAlign: 'middle',
        '&:hover': {
            background: '#169154'
        }
    },

    CustomDMTButtonStyle: {
        background: PRIMARY_COLOR,
        color: '#fff',
        cursor: 'pointer',
        border: 'none',
        fontSize: '16px',
        textAlign: 'center',
        width: '100px',
        minHeight: '50px',
        display: 'inline-block',
        verticalAlign: 'middle',
        '&:hover': {
            background: PRIMARY_HOVER_COLOR
        },
        '&:desabled': {
            background: '#000'
        }
    },

    CustomPrimaryButtonStyle: {
        background: PRIMARY_COLOR,
        color: '#fff',
        cursor: 'pointer',
        border: 'none',
        textAlign: 'center',
        minHeight: '36px',
        textTransform: 'capitalize',
        display: 'inline-block',
        verticalAlign: 'middle',
        '&:hover': {
            background: PRIMARY_HOVER_COLOR
        },
        '&:desabled': {
            background: '#000'
        }
    },

    CustomSecondaryButtonStyle: {
        background: SECONDARY_COLOR,
        color: RESET_COLOR,
        cursor: 'pointer',
        border: 'none',
        textAlign: 'center',
        minHeight: '36px',
        textTransform: 'capitalize',
        display: 'inline-block',
        verticalAlign: 'middle',
        '&:hover': {
            background: PRIMARY_HOVER_COLOR
        },
        '&:desabled': {
            background: '#000'
        }
    },
    CustomPrimaryButtonStylePopup: {
        background: PRIMARY_COLOR,
        color: '#fff',
        cursor: 'pointer',
        border: 'none',
        textAlign: 'center',
        textTransform: 'capitalize',
        minHeight: '36px',
        width: '100%',
        display: 'inline-block',
        verticalAlign: 'middle',
        '&:hover': {
            background: PRIMARY_HOVER_COLOR
        },
        '&:desabled': {
            background: '#000'
        }
    },
    CustomSecondaryButtonStylePopup: {
        background: SECONDARY_COLOR,
        color: RESET_COLOR,
        cursor: 'pointer',
        border: 'none',
        textAlign: 'center',
        minHeight: '36px',
        textTransform: 'capitalize',
        width: '100%',
        display: 'inline-block',
        padding: '0 8px',
        verticalAlign: 'middle',
        marginRight: '1em',
        '&:hover': {
            color: RESET_COLOR,
            background: SECONDARY_HOVER_COLOR
        }
    },

    CustomDMTDangerButtonStyle: {
        background: SECONDARY_COLOR,
        color: RESET_COLOR,
        cursor: 'pointer',
        border: 'none',
        fontSize: '16px',
        textAlign: 'center',
        width: '100px',
        minHeight: '50px',
        display: 'inline-block',
        padding: '0 8px',
        verticalAlign: 'middle',
        marginRight: '1em',
        '&:hover': {
            color: RESET_COLOR,
            background: SECONDARY_HOVER_COLOR
        }
    },

    CustomDangerButtonStyle: {
        background: SECONDARY_COLOR,
        color: RESET_COLOR,
        cursor: 'pointer',
        border: 'none',
        textAlign: 'center',
        minHeight: '36px',
        display: 'inline-block',
        padding: '0 8px',
        verticalAlign: 'middle',
        marginRight: '1em',
        '&:hover': {
            color: RESET_COLOR,
            background: SECONDARY_HOVER_COLOR
        }
    },

    FooterStyle: {
        position: 'fixed',
        minWidth: '100%',
        height: '64px',
        bottom: 0,
        borderColor: '#f1f1f1',
        background: '#fff',
        left: 0,
        zIndex: '1500',
        borderStyle: 'solid',
        borderTopWidth: '1px',
        padding: '10px'
    },

    PaperStyle: {
        //padding: '10px'
        padding: '25px',
        border: '1px solid #f1f1f1'
    },
    PaperForm: {
        padding: '5px'
    },
    p10: {
        padding: '10px'
    },
    PaperStyleWithoutBottom: {
        paddingBottom: '15px',
        // height: '45px',
        display: 'flex',
        flexDirection: 'row'
    },

    VerticalFormContainer: {
        display: 'flex',
        flexDirection: 'column',
        flex: 1,
        alignItems: 'center'
    },
    VerticalForm: {
        width: '55em'
    },

    PaperFooterContainer: {
        height: '100%',
        alignItems: 'center',
        verticalAlign: 'middle',
        justifyContent: 'center',
        marginLeft: '300px'
    },
    PaperFooter: {
        display: 'flex',
        marginTop: '12px',
        alignItems: 'center',
        maxWidth: '960px',
        flexDirection: 'row-reverse '
    },

    SubmitWrapper: {
        position: 'relative'
    },

    SubmitWrapperWithMarginRight: {
        position: 'relative',
        marginRight: '10px'
    },

    buttonProgress: {
        color: green[500],
        position: 'absolute',
        top: '50%',
        left: '50%',
        marginTop: -12,
        marginLeft: -12
    },

    backdrop: {
        zIndex: theme.zIndex.drawer + 1,
        color: '#fff'
    },
    numbericRightAlign: {
        textAlign: 'right',
        paddingRight: 24
    },
    numbericRightAlignWithoutPadding: {
        textAlign: 'right',
        paddingRight: 4
    },
    multiSelect: {
        width: 500,
        '& > * + *': {
            marginTop: theme.spacing(3)
        }
    },

    footerGrid: {
        marginTop: theme.spacing(3),
        marginBottom: theme.spacing(1)
    },

    RightAlign: {
        alignItems: 'left'
    },
    flexReverse: {
        display: 'flex',
        flexDirection: 'row-reverse'
    },
    login_container: {
        height: '100%',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        flexDirection: 'column'
    },

    login_form_wrapper: {
        padding: '1em',
        flexDirection: 'column'
    },

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

    login_Logo: {
        alignItems: 'center'
    },
    card_sequence: {
        background: '#42b86f',
        color: '#fff !important',
        borderRadius: '2px',
        marginRight: '10px',
        padding: '4px',
        fontSize: '.8571rem'
    },

    success: {
        background: '#42b86f',
        color: '#fff !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '12px'
    },
    fail: {
        background: '#ff0000',
        color: '#fff !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '12px'
    },
    process: {
        background: '#f58220',
        color: '#fff !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '12px'
    },
    Open: {
        background: '#ecb81b',
        color: '#fff !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '12px'
    },
    resend: {
        background: '#0000ff',
        color: '#fff !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '12px'
    },
    Provider_Format: {
        //background: '#e0ebeb',
        color: '#008000 !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '.8571rem',
        overflowWrap: 'anywhere'
    },
    Document_Format: {
        background: '#e0ebeb',
        color: '#008000 !important',
        borderRadius: '2px',
        padding: '4px',
        fontSize: '.8571rem',
        margin: '1%',
        overflowWrap: 'anywhere'
    },
    selectedColor: {
        background: '#7B9FE0',
        color: '#fff',
        marginRight: '.5rem',
        paddingRight: '1rem',
        '&:hover': {
            background: '#657D8C',
            color: '#000'
        }
    },
    recharge_h2: {
        alignItems: 'center',
        display: 'flex',
        flexWrap: 'wrap',
        wordBreak: 'break-word',
        fontWeight: '500',
        font: '500 14px Roboto,RobotoDraft,Helvetica,Arial,sans-serif',
        color: PRIMARY_COLOR
    },

    user_h2: {
        alignItems: 'center',
        //display: 'flex',
        fontWeight: '450',
        font: '500 14px Roboto,RobotoDraft,Helvetica,Arial,sans-serif',
        color: PRIMARY_COLOR
    },

    svg_stroke_selected: {
        stroke: PRIMARY_COLOR,
        strokeWidth: '2px',
        fill: 'none'
    },
    svg_stroke_non_selected: {
        stroke: '#3e3e3e',
        strokeWidth: '2px',
        fill: 'none'
    },
    svg_stroke_white: {
        stroke: MENU_FORE_COLOR(),
        strokeWidth: '2px',
        fill: 'none'
    },
    svg_stroke_selected: {
        stroke: PRIMARY_COLOR,
        strokeWidth: '2px',
        fill: 'none'
    },

    svg_selected: {
        fill: PRIMARY_COLOR
    },
    svg_non_selected: {
        fill: '#3e3e3e'
    },
    svg_white: {
        fill: MENU_FORE_COLOR()
    },
    Success_h3: {
        color: '#008000',
        fontWeight: '800',
        fontSize: '16px',
        marginLeft: '5px'
    },
    Pending_h3: {
        color: '#ffa500',
        fontWeight: '800',
        fontSize: '16px',
        marginLeft: '5px'
    },
    Bold_h3: {
        color: '#000',
        fontWeight: '500',
        fontSize: '15px',
        marginLeft: '5px'
    },
    Fail_h3: {
        color: '#ff0000',
        fontWeight: '800',
        fontSize: '16px',
        marginLeft: '5px'
    },

    blue_h3: {
        color: '#0000ff',
        fontWeight: '800',
        fontSize: '16px',
        marginLeft: '5px'
    },
    dashboard_right_panel: {
        background: PRIMARY_COLOR,
        borderRadius: '3px',
        padding: '3px 15px 15px',
        color: '#fff',
        margin: '10px',
        height: '100%'
    },

    successBox: {
        display: 'flex',
        alignItems: 'center',
        backgroundColor: '#4caf50',
        color: '#fff',
        padding: theme.spacing(2),
        borderRadius: theme.shape.borderRadius,
        boxShadow: theme.shadows[1],
        marginTop: theme.spacing(2)
    },
    successIcon: {
        marginRight: theme.spacing(1)
    }
}));

