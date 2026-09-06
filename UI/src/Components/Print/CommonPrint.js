const CommonPrint = (pageSize) => {
    var panel = document.getElementById('DialogPrint');
    var printWindow = window.open('', '', 'height=800,width=800');
    printWindow?.document.write(
        '<html><head><title></title><link rel="stylesheet" type="text/css"/>'
    );
    if (pageSize === 'A5') {
        printWindow?.document.write(
            '<style>@page {size: A5; margin: 0mm}</style>'
        );
    }
    if (pageSize === 'A4') {
        printWindow?.document.write(
            '<style>@page {size: A4; margin: 0mm}</style>'
        );
    }
    printWindow?.document.write('</head><body>');
    printWindow?.document.write(panel.innerHTML);

    printWindow?.document.write('</body></html>');
    printWindow?.document.close();
    printWindow?.print();
};

export default CommonPrint;
