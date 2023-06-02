using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassWhatsAppSite.Models;
using RestSharp;
using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;

public class WhatsAppController : Controller
{
    private readonly ILogger<WhatsAppController> _logger;
    private readonly IConfiguration _configuration;
    private Boolean visivel;

    public WhatsAppController(ILogger<WhatsAppController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration; 
    }

    private static string apiKeyHidden;

    [HttpGet]
    public IActionResult Index()
    {
        var message = new WhatsAppMessage
        {
            Message = "Digite sua mensagem!"
        };

        ViewBag.APIKeyHidden = TempData["APIKey"] as string; 



    
        return View(message);
        
    }

    [HttpPost]
    //public IActionResult SendMessage(WhatsAppMessage msg)
    public async Task<IActionResult> SendMessage(WhatsAppMessage msg)
    {
        _logger.LogInformation("SendMessage chamado com a mensagem: {message}", msg);

        if (msg == null || string.IsNullOrEmpty(msg.PhoneNumbers) || string.IsNullOrEmpty(msg.Message))
        {
            TempData["Error"] = "Por favor, forneça um número de telefone e uma mensagem.";
            return RedirectToAction("Index");
        }

        var phoneNumbers = msg.PhoneNumbers.Split('\n').Select(n => n.Trim()).ToList();

        var apiUrl = _configuration.GetValue<string>("UrlApiWhatsapp");
        var url = $"{apiUrl}/message/text?key=" + apiKeyHidden;

        var client = new RestClient(url);
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

        var count = 0;

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
                    _logger.LogInformation("Mensagem enviada com sucesso para o número: {phoneNumber} - Data e Hora: {dateTime}", phoneNumber, DateTime.Now);

                }
                else
                {
                    _logger.LogError("Falha ao enviar mensagem para o número: {phoneNumber}, StatusCode: {statusCode}, ErrorMessage: {errorMessage}", phoneNumber, response.StatusCode, response.ErrorMessage);
                }

                await Task.Delay(15000);
                count++;

                if (count == 5)
                {
                    await Task.Delay(40000);
                    count = 0;
                }
        
                
  

        }

        TempData["Success"] = "Mensagens enviadas com sucesso para todos os números.";

        return RedirectToAction("Index");
    }

    [HttpGet]
        public IActionResult EnterAPIKEY(string apiKey)
        {
            apiKeyHidden = apiKey;

            TempData["APIKey"] = apiKey;
            
            return RedirectToAction("Index");


        }



    [HttpGet]
    public IActionResult RegisterPhoneNumber(string phoneNumber, string apiKey)
    {
        apiKeyHidden = apiKey;
        TempData["apiKeyHidden"] = apiKey;

        var token = "RANDOM_STRING_HERE_RANDOM123"; // Substitua pela sua lógica para gerar um token aleatório
        var apiUrl = _configuration.GetValue<string>("UrlApiWhatsapp");
        var url = $"{apiUrl}/instance/init?token={token}&key={apiKey}";

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
        var apiUrl = _configuration.GetValue<string>("UrlApiWhatsapp");
        var url = $"{apiUrl}/instance/qrbase64?key={apiKeyHidden}";

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

            string qrCodeImage_esq = qrCodeImage.Substring(index + 8);

            int index2 = qrCodeImage_esq.IndexOf("}");

            string qrCodeBase64 = qrCodeImage_esq.Substring(0, index2 - 1);

            //qrCodeBase64 = qrCodeBase64_SP;

            // Armazenar a imagem em base64 no TempData para exibir no frontend
            TempData["QRCodeBase64"] = qrCodeBase64;
            TempData["ShowDiv"] = true;
        }
        else
        {
            // A requisição falhou, definir a mensagem de erro
            TempData["Error"] = "Falha ao gerar QRCode. Por favor, tente novamente.";
        }

        return RedirectToAction("Index");
        
    }
}
