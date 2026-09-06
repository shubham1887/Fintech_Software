import { createMuiTheme } from '@material-ui/core/styles';
import { PRIMARY_COLOR, SECONDARY_COLOR } from 'Config';
const CustomTheme = createMuiTheme({
    palette: {
        primary: {
            main: PRIMARY_COLOR()
        },
        secondary: {
            main: SECONDARY_COLOR()
        },
        info: {
            main: '#1976d2'
        }
    },
    '@global': {
        overflowY: 'auto',
        margin: 0,
        padding: 0,
        listStyle: 'none',
        height: '100%',
        '&::-webkit-scrollbar': {
            width: '0.4em'
        },
        '&::-webkit-scrollbar-track': {
            boxShadow: 'inset 0 0 6px rgba(0,0,0,0.00)',
            webkitBoxShadow: 'inset 0 0 6px rgba(0,0,0,0.00)'
        },
        '&::-webkit-scrollbar-thumb': {
            backgroundColor: 'rgba(0,0,0,.1)',
            outline: '1px solid slategrey'
        }
    },

    overrides: {
        MuiToggleButton: {
            root: {
                background: 'transparent',
                '&$selected': {
                    // <-- mixing the two classes
                    color: '#fff',
                    backgroundColor: PRIMARY_COLOR()
                }
            }
        },
        MuiButton: {
            containedSizeLarge: {
                padding: '15px 25px'
            }
        }
    }
});

export default CustomTheme;
