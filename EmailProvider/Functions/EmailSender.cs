using Azure.Messaging.ServiceBus;
using EmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EmailProvider.Functions;

public class EmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly EmailService _emailService;
	public EmailSender(ILogger<EmailSender> logger, EmailService emailService)
	{
		_logger = logger;
		_emailService = emailService;
	}

	[Function(nameof(EmailSender))]
    public async Task Run([ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
    {
        try
        {
            var emailRequest = _emailService.UnpackEmailRequest(message);
            if(emailRequest != null && !string.IsNullOrEmpty(emailRequest.To))
            {
                if(_emailService.SendEmail(emailRequest))
                {
                    await messageActions.CompleteMessageAsync(message);
                }
            }
        }
        catch (Exception ex) 
        {
            _logger.LogError($"ERROR : EmailSender.Run() :: {ex.Message}");
        }
    }
}
