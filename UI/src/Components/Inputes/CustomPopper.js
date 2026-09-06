import Popper from '@material-ui/core/Popper';
import { makeStyles, createStyles } from '@material-ui/core/styles';

const useStyles = makeStyles((theme) =>
    createStyles({
        root: {
            overrides: {
                '& .MuiAutocomplete-listbox': {
                    backgroundColor: 'green'
                }
            }
        }
    })
);

const styles = (theme) => ({
    popper: {
        backgroundColor: 'green'
    }
});

const CustomPopper = function (props) {
    const classes = useStyles();
    return <Popper {...props} style={styles.popper} />;
};

export default CustomPopper;
