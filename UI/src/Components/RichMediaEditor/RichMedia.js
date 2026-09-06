import React from 'react';
import ReactQuill from 'react-quill';
import 'react-quill/dist/quill.snow.css';

const CustomRichEditor = ({ value, onChange }) => {
    const handleEditorChange = (content) => {
        if (typeof onChange === 'function') {
            onChange(content);
        }
    };

    const formats = [
        'header',
        'bold',
        'italic',
        'underline',
        'strike',
        'blockquote',
        'list',
        'bullet',
        'link',
        'image',
        'clean',
        'align',
        'color',
        'background',
        'font',
        'code-block'
    ];

    const modules = {
        toolbar: [
            [{ header: [1, 2, false] }],
            ['bold', 'italic', 'underline', 'strike', 'blockquote'],
            [{ list: 'ordered' }, { list: 'bullet' }],
            ['link', 'image'],
            ['clean'],
            [{ align: [] }],
            [{ color: [] }, { background: [] }],
            [{ font: [] }],
            ['code-block']
        ]
    };

    return (
        <ReactQuill
            value={value}
            onChange={handleEditorChange}
            placeholder="Type something..."
            modules={modules}
            formats={formats}
        />
    );
};

export default CustomRichEditor;
