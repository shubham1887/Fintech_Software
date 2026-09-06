import { makeStyles } from '@material-ui/core/styles';

export const commonStyle = makeStyles((theme) => ({
    PrimaryButton: {
        marginTop: '10px',
        background: '#0277bd',
        '&:hover': {
            background: '#0277be'
        },
        color: '#fff',
        boxShaddow:
            '0 2px 2px 0 rgba(0,0,0,0.14), 0 3px 1px -2px rgba(0,0,0,0.12), 0 1px 5px 0 rgba(0,0,0,0.2);'
    },

    DangerButton: {
        marginTop: '10px',
        backgroundColor: '#f00',
        color: '#fff',
        boxShaddow:
            '0 2px 2px 0 rgba(0,0,0,0.14), 0 3px 1px -2px rgba(0,0,0,0.12), 0 1px 5px 0 rgba(0,0,0,0.2);',
        '&:hover': {
            background: '#f00'
        }
    }
}));
