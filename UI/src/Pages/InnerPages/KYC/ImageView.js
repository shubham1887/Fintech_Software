import React, { useEffect, useState, useRef } from 'react';
import { Form } from 'formik';
import {
    Modal,
    Icon,
    IconButton,
    Tooltip,
    FormControl,
    Button
} from '@material-ui/core';

import { Container, Grid } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import HOCLoader from 'Components/Loader/HOCLoader';

const CustomContainer = HOCLoader(Container);
function getModalStyle() {
    const top = 50;
    const left = 50;

    return {
        top: `${top}%`,
        left: `${left}%`,
        transform: `translate(-${top}%, -${left}%)`,
        width: '65%',
        height: '75%',
        display: 'inline-block',
        overflow: 'hidden',
        position: 'relative'
    };
}

const useStyles = makeStyles((theme) => ({
    Image_form: {
        display: 'flex',
        flexDirection: 'column',
        margin: '2%',
        alignItems: 'center'
    },
    Image_title: {
        fontFamily: 'Raleway-Medium',
        fontSize: '20px',
        lineHeight: '1.2',
        textAlign: 'center',
        textTransform: 'uppercase'
    },
    customButton: {
        marginTop: '2px',
        alignItems: 'right'
    },

    modalContent: {
        position: 'absolute',
        background: '#fff',
        padding: theme.spacing(1)
    }
}));

export const ImageView = ({
    isOpen,
    DocumentTextLabel,
    ImageURL,
    resendCallback,
    onDismissCallback
}) => {
    const [documentImage, setDocumentImage] = useState('');
    const [DocumentDimension, setDocumentDimension] = useState({
        width: 0,
        height: 0
    });
    const [loadingOpen, setLoadingOpen] = useState(false);
    const classes = useStyles();
    const [modalStyle] = useState(getModalStyle);
    const [open, setOpen] = useState(isOpen);

    const [zoomLevel, setZoomLevel] = useState(1);
    const [rotation, setRotation] = useState(0);
    const [dragging, setDragging] = useState(false);
    const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
    const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });

    const handleScroll = (event) => {
        const deltaY = event.deltaY;

        if (deltaY !== 0) {
            event.preventDefault();

            if (deltaY > 0) {
                setZoomLevel((prevZoomLevel) =>
                    Math.max(0.1, prevZoomLevel - 0.1)
                );
            } else {
                setZoomLevel((prevZoomLevel) => prevZoomLevel + 0.1);
            }
        }
    };

    const canvasRef = useRef(null);

    useEffect(() => {
        setDocumentDimension(getImageDimension(documentImage));
        setDocumentDimension(getImageDimension(ImageURL));
    }, [documentImage, ImageURL]);

    useEffect(() => {
        setLoadingOpen(false);
    }, []);

    useEffect(() => {
        setDocumentImage(ImageURL);
    }, [ImageURL]);

    const getImageDimension = (imageUrl) => {
        if (!imageUrl) {
            return {
                width: 0,
                height: 0
            };
        }
        const img = new Image();
        img.src = imageUrl;
        return {
            width: img.width,
            height: img.height
        };
    };

    const getMax = (imageSize) => {
        if (imageSize > 400) return 400;
        return imageSize;
    };

    const handleModalClose = () => {
        setOpen(false);
        onDismissCallback(false);
    };

    const rotateImage = (degree) => {
        let newRotation = rotation + degree;
        if (newRotation >= 360) {
            newRotation = newRotation - 360;
        } else if (newRotation < 0) {
            newRotation = 360 + newRotation;
        }
        setRotation(newRotation);
    };

    const handleMouseDown = (event) => {
        setDragging(true);
        setDragStart({ x: event.clientX, y: event.clientY });
    };

    const handleMouseMove = (event) => {
        if (dragging) {
            const offsetX = event.clientX - dragStart.x;
            const offsetY = event.clientY - dragStart.y;

            const angleInRadians = (rotation * Math.PI) / 180;
            const cosAngle = Math.cos(angleInRadians);
            const sinAngle = Math.sin(angleInRadians);
            const rotatedOffsetX = offsetX * cosAngle + offsetY * sinAngle;
            const rotatedOffsetY = -offsetX * sinAngle + offsetY * cosAngle;

            setDragOffset({ x: rotatedOffsetX, y: rotatedOffsetY });
        }
    };

    const handleMouseUp = () => {
        setDragging(false);
        setDragStart({ x: 0, y: 0 });
        setDragOffset({ x: 0, y: 0 });
    };

    const drawRotatedImage = () => {
        const canvas = canvasRef.current;
        if (canvas) {
            const context = canvas.getContext('2d');
            const img = new Image();
            img.src = ImageURL;
            img.onload = () => {
                const { width, height } = DocumentDimension;
                canvas.width = width;
                canvas.height = height;
                context.clearRect(0, 0, width, height);
                context.save();
                context.translate(width / 2, height / 2);
                context.rotate((rotation * Math.PI) / 180);
                context.drawImage(img, -width / 2, -height / 2, width, height);
                context.restore();
            };
        }
    };

    useEffect(() => {
        drawRotatedImage();
    }, [rotation]);

    return (
        <CustomContainer isLoading={loadingOpen}>
            <Modal open={open} onClose={handleModalClose}>
                <div style={modalStyle} className={classes.modalContent}>
                    <Form className={classes.Image_form}>
                        <Grid container justify="flex-end">
                            <Button
                                variant="contained"
                                color="secondary"
                                size="small"
                                onClick={handleModalClose}
                            >
                                X
                            </Button>
                        </Grid>
                        <FormControl className={classes.Image_title}>
                            <b>{DocumentTextLabel}</b>
                        </FormControl>

                        <Grid container justify="center" alignItems="center">
                            <Tooltip title="Rotate Image Right">
                                <IconButton
                                    className="kycicon"
                                    onClick={() => rotateImage(90)}
                                >
                                    <Icon>rotate_right</Icon>
                                </IconButton>
                            </Tooltip>
                            <Tooltip title="Rotate Image Left">
                                <IconButton
                                    className="kycicon"
                                    onClick={() => rotateImage(-90)}
                                >
                                    <Icon>rotate_left</Icon>
                                </IconButton>
                            </Tooltip>
                            <span>Rotate</span>
                        </Grid>
                        <Grid container justify="center" alignItems="center">
                            <Grid item xs={12} sm={12} md={9} lg={6} xl={4}>
                                <div
                                    style={{
                                        position: 'relative',
                                        width: '100%',
                                        height: '50vh',
                                        overflow: 'auto',
                                        display: 'flex',
                                        justifyContent: 'center',
                                        alignItems: 'center'
                                    }}
                                    onMouseDown={handleMouseDown}
                                    onMouseMove={handleMouseMove}
                                    onMouseUp={handleMouseUp}
                                    onMouseLeave={handleMouseUp}
                                    onWheel={handleScroll}
                                >
                                    <div
                                        style={{
                                            transformOrigin: 'center center',
                                            transform: `scale(${zoomLevel}) rotate(${rotation}deg) translate(${dragOffset.x}px, ${dragOffset.y}px)`,
                                            transition:
                                                'transform 0.3s ease-in-out',
                                            maxWidth: '100%',
                                            maxHeight: '100%'
                                        }}
                                    >
                                        <img
                                            className="kycImage"
                                            name="ImageURL"
                                            src={ImageURL}
                                            alt={ImageURL}
                                            style={{
                                                display: 'block',
                                                margin: 'auto'
                                            }}
                                        />
                                    </div>
                                </div>
                            </Grid>
                        </Grid>
                    </Form>
                </div>
            </Modal>
        </CustomContainer>
    );
};
