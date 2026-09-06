var a = [
    '',
    'one ',
    'two ',
    'three ',
    'four ',
    'five ',
    'six ',
    'seven ',
    'eight ',
    'nine ',
    'ten ',
    'eleven ',
    'twelve ',
    'thirteen ',
    'fourteen ',
    'fifteen ',
    'sixteen ',
    'seventeen ',
    'eighteen ',
    'nineteen '
];
var b = [
    '',
    '',
    'twenty',
    'thirty',
    'forty',
    'fifty',
    'sixty',
    'seventy',
    'eighty',
    'ninety'
];

export const NumberToWord = (num) => {
    if (num === undefined || num === null || num <= 0) {
        return '';
    }

    var nums = num.toString().split('.');
    var whole = convertNumberToWords(nums[0]);

    if (nums.length >= 2) {
        var fraction = convertNumberToWords(nums[1]);

        return whole + 'and ' + fraction;
    }
    return whole + 'only ';
};
const convertNumberToWords = (num) => {
    if ((num = num.toString()).length > 9) return 'overflow';
    let n = ('000000000' + num)
        .substr(-9)
        .match(/^(\d{2})(\d{2})(\d{2})(\d{1})(\d{2})$/);
    if (!n) return;
    var str = '';
    str +=
        n[1] != 0
            ? (a[Number(n[1])] || b[n[1][0]] + ' ' + a[n[1][1]]) + 'crore '
            : '';
    str +=
        n[2] != 0
            ? (a[Number(n[2])] || b[n[2][0]] + ' ' + a[n[2][1]]) + 'lakh '
            : '';
    str +=
        n[3] != 0
            ? (a[Number(n[3])] || b[n[3][0]] + ' ' + a[n[3][1]]) + 'thousand '
            : '';
    str +=
        n[4] != 0
            ? (a[Number(n[4])] || b[n[4][0]] + ' ' + a[n[4][1]]) + 'hundred '
            : '';
    str +=
        n[5] != 0
            ? (str !== '' ? 'and ' : '') +
              (a[Number(n[5])] || b[n[5][0]] + ' ' + a[n[5][1]])
            : '';
    return titleCase(str);
};
const titleCase = (str) => {
    str = str.toLowerCase().split(' ');
    for (var i = 0; i < str.length; i++) {
        str[i] = str[i].charAt(0).toUpperCase() + str[i].slice(1);
    }
    return str.join(' ');
};
