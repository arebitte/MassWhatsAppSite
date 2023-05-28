using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MassWhatsAppSite.Models;
using RestSharp;
using System.Linq;

public class WhatsAppController : Controller
{
    private readonly ILogger<WhatsAppController> _logger;

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

        var client = new RestClient("http://localhost:3333/message/text?key=andre_pessoal_v2");
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

        foreach (var phoneNumber in phoneNumbers)
        {
            var newRequest = new RestRequest(); // Criar nova instância de RestRequest
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

            Thread.Sleep(8000); // Aguarda 5 segundos antes da próxima iteração
        }

        TempData["Success"] = "Mensagens enviadas com sucesso para todos os números.";

        return RedirectToAction("Index");
    }

}
