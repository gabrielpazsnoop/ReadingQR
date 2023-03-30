using System.Drawing;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using PdfLibCore;
using PdfLibCore.Enums;
using ZXing;
using ZXing.Windows.Compatibility;

const string pdfFilePath = "C:\\temp\\AFIPFactura.pdf";
// const string pdfFilePath = "C:\\temp\\NotaEscolar.pdf";
const double dpiX = 300D;
const double dpiY = 300D;

await using var stream = new FileStream(pdfFilePath, FileMode.Open, FileAccess.Read);
using var pdfDocument = new PdfDocument(stream);
foreach (var page in pdfDocument.Pages)
{
    using var pdfPage = page;
    var pageWidth = (int)(dpiX * pdfPage.Size.Width / 72);
    var pageHeight = (int)(dpiY * pdfPage.Size.Height / 72);

    using var bitmap = new PdfiumBitmap(pageWidth, pageHeight, true);
    pdfPage.Render(bitmap, PageOrientations.Normal, RenderingFlags.LcdText);
    await using var bmpStream = bitmap.AsBmpStream(dpiX, dpiY);

    var resultadoMetodo1 = Metodo1(bmpStream);
    Console.WriteLine("METODO 1");
    EscribirTodo(ProcessQRResults(resultadoMetodo1));

    var resultadoMetodo2 = Metodo2(bmpStream);
    Console.WriteLine("\n\nMETODO 2");
    EscribirTodo(ProcessQRResults(resultadoMetodo2));
}

void EscribirTodo(QrAFIPDtoResponse qrResponse)
{
    Console.WriteLine($"Version: {qrResponse.Version}");
    Console.WriteLine($"Fecha: {qrResponse.Fecha}");
    Console.WriteLine($"Cuit: {qrResponse.Cuit}");
    Console.WriteLine($"PuntoDeVenta: {qrResponse.PuntoDeVenta}");
    Console.WriteLine($"TipoComprobante: {qrResponse.TipoComprobante}");
    Console.WriteLine($"NumeroComprobante: {qrResponse.NumeroComprobante}");
    Console.WriteLine($"Importe: {qrResponse.Importe}");
    Console.WriteLine($"Moneda: {qrResponse.Moneda}");
    Console.WriteLine($"Cotizacion: {qrResponse.Cotizacion}");
    Console.WriteLine($"TipoDocumento: {qrResponse.TipoDocumento}");
    Console.WriteLine($"NumeroDocumento: {qrResponse.NumeroDocumento}");
    Console.WriteLine($"TipoCodigoAutorizacion: {qrResponse.TipoCodigoAutorizacion}");
    Console.WriteLine($"CodigoAutorizacion: {qrResponse.CodigoAutorizacion}");
}

Result? Metodo1(Stream bitmapStream)
{
    //Utilizando ZXing.Net.Bindings.Windows.Compatibility para crear la Luminance
    
    var image = new Bitmap(bitmapStream);
    // Obtener el formato de imagen
    var reader2 = new BarcodeReaderGeneric();

    using (image)
    {
        LuminanceSource source = new BitmapLuminanceSource(image);
        var resultado = reader2.Decode(source);

        return resultado is { BarcodeFormat: BarcodeFormat.QR_CODE } ? resultado : null;
    }
}

Result? Metodo2(Stream bitmapStream)
{
    //Creando la Luminance a mano
    
    var luminance = obtenerLuminance(bitmapStream);

    // create a barcode reader instance
    var reader = new BarcodeReaderGeneric();
    var result = reader.Decode(luminance);

    return result;
}

RGBLuminanceSource obtenerLuminance(Stream bitmapStream)
{
    var bitmap = new Bitmap(bitmapStream);

    var width = bitmap.Width;
    var height = bitmap.Height;

    // Crea una matriz de bytes que representan la imagen en formato RGB
    var rgbBytes = new byte[width * height * 3];

    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
    {
        var pixel = bitmap.GetPixel(x, y);
        var index = (y * width + x) * 3;
        rgbBytes[index] = pixel.R;
        rgbBytes[index + 1] = pixel.G;
        rgbBytes[index + 2] = pixel.B;
    }

    // Crea un objeto RGBLuminanceSource a partir de la matriz de bytes
    var source = new RGBLuminanceSource(rgbBytes, width, height);

    return source;
}

QrAFIPDtoResponse ProcessQRResults(Result? pageRes)
{
    if (pageRes is null) Console.WriteLine("QR no detectado o formato incorrecto");

    var queryString = new Uri(pageRes!.Text).Query;
    var queryDictionary = HttpUtility.ParseQueryString(queryString);
    var encodedValue = queryDictionary["p"];

    if (encodedValue is null) Console.WriteLine("QR no detectado o formato incorrecto");

    var base64EncodedBytes = Convert.FromBase64String(encodedValue!);
    var text = Encoding.UTF8.GetString(base64EncodedBytes);
    var qrAFIPDTO = JsonConvert.DeserializeObject<QrAFIPDTO>(text);

    return new QrAFIPDtoResponse
    {
        Version = qrAFIPDTO!.Ver,
        Fecha = qrAFIPDTO.Fecha,
        Cuit = qrAFIPDTO.Cuit,
        PuntoDeVenta = qrAFIPDTO.PtoVta,
        TipoComprobante = qrAFIPDTO.TipoCmp,
        NumeroComprobante = qrAFIPDTO.NroCmp,
        Importe = qrAFIPDTO.Importe,
        Moneda = qrAFIPDTO.Moneda,
        Cotizacion = qrAFIPDTO.Ctz,
        TipoDocumento = qrAFIPDTO.TipoDocRec,
        NumeroDocumento = qrAFIPDTO.NroDocRec,
        TipoCodigoAutorizacion = qrAFIPDTO.TipoCodAut,
        CodigoAutorizacion = qrAFIPDTO.CodAut
    };
}

public class QrAFIPDtoResponse
{
    public int Version { get; set; }
    public DateTime Fecha { get; set; }
    public long Cuit { get; set; }
    public int PuntoDeVenta { get; set; }
    public int TipoComprobante { get; set; }
    public long NumeroComprobante { get; set; }
    public decimal Importe { get; set; }
    public string Moneda { get; set; } = null!;
    public decimal Cotizacion { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public string TipoCodigoAutorizacion { get; set; } = null!;
    public long CodigoAutorizacion { get; set; }
}

public class QrAFIPDTO
{
    public int Ver { get; set; }
    public DateTime Fecha { get; set; }
    public long Cuit { get; set; }
    public int PtoVta { get; set; }
    public int TipoCmp { get; set; }
    public int NroCmp { get; set; }
    public decimal Importe { get; set; }
    public string Moneda { get; set; } = null!;
    public decimal Ctz { get; set; }
    public int TipoDocRec { get; set; }
    public long NroDocRec { get; set; }
    public string TipoCodAut { get; set; } = null!;
    public long CodAut { get; set; }
}