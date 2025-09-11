// OctarineCodex/Messaging/MessagingExtensions.cs

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OctarineCodex.Logging;

namespace OctarineCodex.Messaging;

/// <summary>
///     Extensions for configuring the messaging system with dependency injection.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    ///     Auto-register message handlers from the executing assembly.
    /// </summary>
    public static void InitializeMessageHandlers(IServiceProvider services)
    {
        var messageBus = services.GetRequiredService<IMessageBus>();
        var logger = services.GetRequiredService<ILoggingService>();

        var assembly = Assembly.GetExecutingAssembly();
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsInterface: false, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                                                   i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)))
            .ToList();

        logger.Debug($"Found {handlerTypes.Count} message handler types");

        var successfulRegistrations = 0;

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                var handler = ActivatorUtilities.CreateInstance(services, handlerType); // Use DI here too

                // Register for each message type this handler supports
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

                foreach (var interfaceType in interfaces)
                {
                    var messageType = interfaceType.GetGenericArguments()[0];
                    var registerMethod = typeof(IMessageBus).GetMethod(nameof(IMessageBus.RegisterHandler))!
                        .MakeGenericMethod(messageType);
                    registerMethod.Invoke(messageBus, [handler]);

                    logger.Debug($"Registered {handlerType.Name} for message type {messageType.Name}");
                    successfulRegistrations++;
                }
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Failed to create instance of message handler {handlerType.Name}");
            }
        }

        logger.Info(
            $"Successfully registered {successfulRegistrations} message handlers from {handlerTypes.Count} handler types");
    }
}