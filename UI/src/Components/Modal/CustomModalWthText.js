import React from 'react';
import { makeStyles } from '@material-ui/core/styles';

import { Box, Modal, Grid, Button } from '@material-ui/core';

function getModalStyle() {
    const top = 45;
    const left = 50;

    return {
        position: 'absolute',
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${left}%, -${top}%)`,
        maxHeight: '80vh',
        maxWidth: '50vw'
    };
}

const useStyles = makeStyles((theme) => ({
    modal: {
        position: 'absolute',
        backgroundColor: theme.palette.background.paper,
        border: '1px solid #000',
        boxShadow: theme.shadows[5],
        padding: theme.spacing(1, 1, 1),
        overflow: 'auto',
        maxHeight: '80vh',
        width: '60vw !important',
        display: 'block'
    }
}));

export function CustomModalWthText(props) {
    const classes = useStyles();
    // getModalStyle is not a pure function, we roll the style only on the first render

    const [modalStyle] = React.useState(getModalStyle);
    const [open, setOpen] = React.useState(props.isOpen);

    const handleClose = () => {
        setOpen(false);
        props.onDismissCallback(false);
    };

    return (
        <Modal
            open={open}
            onClose={handleClose}
            aria-labelledby="simple-modal-title"
            aria-describedby="simple-modal-description"
            disablePortal={true}
            closeAfterTransition={true}
        >
            <Box
                component="span"
                m={1}
                width={props.width}
                style={modalStyle}
                className={classes.modal}
            >
                <Grid
                    container
                    spacing={2}
                    style={{
                        borderBottom: '1px solid #c6d2d9'
                    }}
                >
                    <Grid item xs={3} sm={3} md={3} lg={3}>
                        <b>{props.title}</b>
                    </Grid>
                    <Grid
                        item
                        xs={3}
                        sm={3}
                        md={3}
                        lg={3}
                        style={{ display: 'flex', direction: 'rtl', marginLeft: "auto", marginRight: "24px", }}
                    >
                        <Button
                            variant="contained"
                            color="secondary"
                            size="small"
                            onClick={handleClose}
                        >
                            X
                        </Button>
                    </Grid>
                </Grid>

                <Grid container style={{ marginTop: '20px' }}>
                    {props.children}
                </Grid>
            </Box>
        </Modal>
    );
}
