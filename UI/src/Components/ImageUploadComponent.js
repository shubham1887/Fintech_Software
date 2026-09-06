import React, { useState, useEffect } from 'react';
import { Button, Grid, IconButton, Typography, Dialog, DialogContent, DialogTitle } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import RemoveCircleIcon from '@mui/icons-material/RemoveCircle';
import CloseIcon from '@mui/icons-material/Close';
import PdfLogo from 'Assets/Images/PdfImage.png';
import ImageNotFound from 'Assets/Images/ImageNotFound.png';

const ImageUploadComponent = ({ documentType, setFieldValue, DocumentImage, ID }) => {
    const [documentImage, setDocumentImage] = useState(DocumentImage ? DocumentImage : false);
    const [dialogOpen, setDialogOpen] = useState(false);

    const [isPDF, setPDF] = useState(DocumentImage ? DocumentImage.endsWith('.pdf') : false);


    useEffect(() => {
        if (ID != null) {
            setFieldValue(`${documentType}.ID`, ID);
            setFieldValue(`${documentType}.Data`, DocumentImage);
        }
    }, [ID, setFieldValue, documentType]);

    const handleFileChange = (e) => {
        const file = e.target.files[0];

        if (file) {
            setDocumentImage(URL.createObjectURL(file));

            const reader = new FileReader();


            setFieldValue(`${documentType}.Name`, file.name);
            setFieldValue(`${documentType}.Size`, file.size);
            setFieldValue(`${documentType}.Type`, file.type);
            setPDF(file.name.endsWith('.pdf'));

            reader.onload = (item) => {
                setFieldValue(
                    `${documentType}.Data`,
                    item.target.result.replace('data:', '').replace(/^.+,/, '')
                );
            };

            reader.readAsDataURL(file);
        }
    };

    const handleRemoveImage = () => {
        // Clear image preview
        setDocumentImage(null);

        // Clear document fields
        setFieldValue(`${documentType}.Name`, '');
        setFieldValue(`${documentType}.Size`, '');
        setFieldValue(`${documentType}.Type`, '');
        setFieldValue(`${documentType}.Data`, '');
    };

    const handleImageClick = () => {
        setDialogOpen(true);
    };

    const handleDialogClose = () => {
        setDialogOpen(false);
    };

    return (
        <Grid container direction="column" alignItems="center">
            {documentImage ? (
                <>
                    {isPDF ? (
                        <Button
                            onClick={() => window.open(documentImage, '_blank')}
                            style={{ width: 100, height: 100, marginTop: 10, cursor: 'pointer' }}
                        >
                        <img
                                src={PdfLogo}
                            alt={`${documentType} preview`}
                                style={{ width: '100%', height: '100%' }}
                        />
                        </Button>
                    ) : (
                        <img
                            src={documentImage !== "" ? documentImage : ImageNotFound}
                            alt={`${documentType} preview`}
                            style={{ width: 100, height: 100, marginTop: 10, cursor: 'pointer' }}
                            onClick={handleImageClick}
                        />

                    )}
                    <IconButton color="secondary" onClick={handleRemoveImage} style={{ marginTop: 10 }}>
                        <RemoveCircleIcon fontSize="large" />
                    </IconButton>
                </>
            ) : (
                <IconButton color="primary" component="label">
                    <UploadFileIcon fontSize="large" />
                    <input
                        type="file"
                        hidden
                        onChange={handleFileChange}
                    />
                </IconButton>
            )}
            <Typography variant="body2" sx={{ mt: 2 }}>
                {documentType} 
            </Typography>


            <Dialog open={dialogOpen} onClose={handleDialogClose} maxWidth="md" fullWidth>
                <DialogTitle>
                    <IconButton
                        edge="end"
                        color="inherit"
                        onClick={handleDialogClose}
                        aria-label="close"
                        style={{ position: 'absolute', right: 8, top: 8 }}
                    >
                        <CloseIcon />
                    </IconButton>
                </DialogTitle>
                <DialogContent>
                    <img
                        src={documentImage !== "" ? documentImage : ImageNotFound}
                        alt={`${documentType} full size`}
                        style={{ width: '100%', height: 'auto' }}
                    />
                </DialogContent>
            </Dialog>
        </Grid>
    );
};

export default ImageUploadComponent;
