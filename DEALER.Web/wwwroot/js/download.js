// wwwroot/js/download.js
window.saveAsFile = (fileName, base64Data) => {
    const link = document.createElement('a');
    link.href = `data:application/zip;base64,${base64Data}`;
    link.download = fileName;
    link.click();
};