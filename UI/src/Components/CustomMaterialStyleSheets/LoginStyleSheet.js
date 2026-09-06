import { makeStyles } from '@material-ui/core/styles';

export const loginStyles = makeStyles((theme) => ({
    login_container: {
        width: '100%',
        height: '100%',
        justifyContent: 'center',
        alignItems: 'center',
        display: 'flex'
    },

    form_wrapper: {
        width: '25em',
        padding: '50px'
    },

    login_form: {},
    title: {
        fontFamily: 'Raleway-Medium',
        fontSize: '20px',
        lineHeight: '1.2',
        textAlign: 'center',
        textTransform: 'uppercase'
    },
    customButton: {
        marginTop: '2px',
        alignItems: 'right'
    }
}));
