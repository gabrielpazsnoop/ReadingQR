# ReadingQR
Prueba de concepto de lectura de QR

###Librerías/Nuget utilizados:

* [ZXing.Net](https://www.nuget.org/packages/ZXing.Net/#supportedframeworks-body-tab): de licencia tipo [Apache](https://licenses.nuget.org/Apache-2.0)
  Utilizado en el método 2 donde se crea la Luminance a mano y luego se decodifica el QR.
* [ZXing.Net.Bindings.Windows.Compatibility](https://www.nuget.org/packages/ZXing.Net.Bindings.Windows.Compatibility#supportedframeworks-body-tab): de licencia tipo [Apache](https://licenses.nuget.org/Apache-2.0)
  Utilizado en el método 1 donde se delega la creación de la Luminance a la librería a partir del stream de Bitmap, pero con la contraparte de que se pierde compatibilidad si la plataforma no es Windows.
* [PdfLibCore](https://www.nuget.org/packages/PdfLibCore): de licencia tipo [MIT](https://www.nuget.org/packages/PdfLibCore/2.4.0/License)
  Utilizado para poder acceder al stram de bitmap a partir del pdf.

###Requerimientos:

* Tener el archivo en la ruta en c:\temp\AFIPFactura.pdf
