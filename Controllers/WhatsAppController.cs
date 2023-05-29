using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassWhatsAppSite.Models;
using RestSharp;
using System;
using System.Linq;
using System.Threading;

public class WhatsAppController : Controller
{
    private readonly ILogger<WhatsAppController> _logger;
    private static string apiKeyHidden;

    public WhatsAppController(ILogger<WhatsAppController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var message = new WhatsAppMessage
        {
            Message = "Digite sua mensagem!"
        };

        return View(message);
    }

    [HttpPost]
    public IActionResult SendMessage(WhatsAppMessage msg)
    {
        _logger.LogInformation("SendMessage chamado com a mensagem: {message}", msg);

        if (msg == null || string.IsNullOrEmpty(msg.PhoneNumbers) || string.IsNullOrEmpty(msg.Message))
        {
            TempData["Error"] = "Por favor, forneça um número de telefone e uma mensagem.";
            return RedirectToAction("Index");
        }

        var phoneNumbers = msg.PhoneNumbers.Split('\n').Select(n => n.Trim()).ToList();

        var client = new RestClient("http://localhost:3333/message/text?key=" + apiKeyHidden);
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

        foreach (var phoneNumber in phoneNumbers)
        {
            var newRequest = new RestRequest();
            newRequest.Method = Method.Post;
            newRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            newRequest.AddParameter("id", phoneNumber);
            newRequest.AddParameter("message", msg.Message);

            RestResponse response = client.Execute(newRequest);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("Mensagem enviada com sucesso para o número: {phoneNumber}", phoneNumber);
            }
            else
            {
                _logger.LogError("Falha ao enviar mensagem para o número: {phoneNumber}, StatusCode: {statusCode}, ErrorMessage: {errorMessage}", phoneNumber, response.StatusCode, response.ErrorMessage);
            }

            Thread.Sleep(9000);
        }

        TempData["Success"] = "Mensagens enviadas com sucesso para todos os números.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult RegisterPhoneNumber(string phoneNumber, string apiKey)
    {
        apiKeyHidden = apiKey;

        var token = "RANDOM_STRING_HERE_RANDOM123"; // Substitua pela sua lógica para gerar um token aleatório
        var url = $"http://localhost:3333/instance/init?token={token}&key={apiKey}";

        // Fazer o post na API
        var register = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Get;
        // Configurar os parâmetros da requisição, se necessário
        // ...

        // Executar a requisição
        var response = register.Execute(request);

        if (response.IsSuccessful)
        {
            // A requisição foi bem-sucedida, definir a mensagem de sucesso
            TempData["Success"] = "Telefone registrado com sucesso!";
        }
        else
        {
            // A requisição falhou, definir a mensagem de erro
            TempData["Error"] = "Falha ao registrar o telefone. Por favor, tente novamente.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult ScanQR()
    {
        var url = $"http://localhost:3333/instance/qrbase64?key={apiKeyHidden}";

        // Fazer o post na API
        var scan = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Get;
        // Configurar os parâmetros da requisição, se necessário
        // ...

        // Executar a requisição
        var response = scan.Execute(request);

        if (response.IsSuccessful)
        {
            // A requisição foi bem-sucedida, definir a mensagem de sucesso
            TempData["Success"] = "Leia o QRCode com WhatsApp";

            // Obter a imagem do QR Code retornada pela API
            string qrCodeImage = response.Content;

            // Converter a imagem para base64
            //string qrCodeBase64 = "";//Convert.ToBase64String(qrCodeImage);
            int index = qrCodeImage.IndexOf(";base64,");

            string qrCodeImage_esq = qrCodeImage.Substring(index+8);

            int index2 = qrCodeImage_esq.IndexOf("}");

            string qrCodeBase64 = qrCodeImage_esq.Substring(0,index2-1);

            

            //qrCodeBase64 = qrCodeBase64_SP;

            // Armazenar a imagem em base64 no TempData para exibir no frontend
            TempData["QRCodeBase64"] = qrCodeBase64;
        }
        else
        {
            // A requisição falhou, definir a mensagem de erro
            TempData["Error"] = "Falha ao gerar QRCode. Por favor, tente novamente.";
        }

        return RedirectToAction("Index");
    }

}
