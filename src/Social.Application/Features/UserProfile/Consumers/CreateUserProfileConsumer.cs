using Cendrite.Message.Contracts.Messages.Profile;
using Cendrite.Message.Contracts.Responses.Profile;
using Social.Domain.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Social.Application.Contracts.Repositories;

namespace Social.Application.Features.UserProfile.Consumers;

public class CreateUserProfileConsumer(
    IUserProfileRepository userProfileRepository,
    IUnitOfWork unitOfWork,
    IUserProfileLifecycleService userProfileLifecycleService,
    ILogger<CreateUserProfileConsumer> logger)
    : IConsumer<CreateUserProfileMessage>
{
    public async Task Consume(ConsumeContext<CreateUserProfileMessage> context)
    {
        var message = context.Message;
        try
        {
            var created = userProfileLifecycleService.CreateUserProfile(message.UserId, message.Username, message.UserTag);

            if (!created.Success)
            {
                await context.RespondAsync(new CreateUserProfileResponse
                {
                    CorrelationId = Guid.NewGuid(),
                    CausationId = message.CausationId,
                    IsSuccess = false,
                    Error = created.Error!.Description,
                });
                return;
            }

            await userProfileRepository.CreateAsync(created.RequiredValue, context.CancellationToken);

            await unitOfWork.SaveChangesAsync();

            await context.RespondAsync(new CreateUserProfileResponse
            {
                CorrelationId = Guid.NewGuid(),
                CausationId = message.CausationId,
                IsSuccess = true,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Exception occured while attempting to create a user profile");
            await context.RespondAsync(new CreateUserProfileResponse()
            {
                CorrelationId = Guid.NewGuid(),
                CausationId = message.CausationId,
                IsSuccess = false,
                Error = "An exception occured while creating attempting to create the user profile"
            });
            throw;
        }
    }
}