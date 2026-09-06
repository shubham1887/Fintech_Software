import React from 'react';
import Checkbox from '@material-ui/core/Checkbox';
import { withStyles } from '@material-ui/core/styles';
import { green } from '@material-ui/core/colors';

export default function GreenCheckbox(prop) {
    const ColorCheckBox = withStyles({
        root: {
            color: green[400],
            '&$checked': {
                color: green[600]
            }
        },
        checked: {}
    })((props) => <Checkbox color="default" {...props} />);

    return <ColorCheckBox checked={prop.checked} value={prop.value} />;
}
