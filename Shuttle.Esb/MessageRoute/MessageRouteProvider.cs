﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb;

public sealed class MessageRouteProvider : IMessageRouteProvider
{
    private readonly IMessageRouteCollection _messageRoutes = new MessageRouteCollection();

    public MessageRouteProvider(IOptions<ServiceBusOptions> serviceBusOptions)
    {
        var specificationFactory = new MessageRouteSpecificationFactory();

        foreach (var messageRouteOptions in Guard.AgainstNull(Guard.AgainstNull(serviceBusOptions).Value).MessageRoutes)
        {
            var messageRoute = _messageRoutes.Find(messageRouteOptions.Uri);

            if (messageRoute == null)
            {
                messageRoute = new MessageRoute(new(messageRouteOptions.Uri));

                _messageRoutes.Add(messageRoute);
            }

            foreach (var specification in messageRouteOptions.Specifications)
            {
                messageRoute.AddSpecification(specificationFactory.Create(specification.Name, specification.Value));
            }
        }
    }

    public async Task<IEnumerable<string>> GetRouteUrisAsync(string messageType)
    {
        return await Task.FromResult(GetRouteUris(messageType));
    }

    public IEnumerable<string> GetRouteUris(string messageType)
    {
        var uri = _messageRoutes.FindAll(Guard.AgainstNullOrEmptyString(messageType)).Select(messageRoute => messageRoute.Uri.ToString()).FirstOrDefault();

        return string.IsNullOrEmpty(uri) ? Array.Empty<string>() : new[] { uri };
    }

    public void Add(IMessageRoute messageRoute)
    {
        var existing = _messageRoutes.Find(Guard.AgainstNull(messageRoute).Uri);

        if (existing == null)
        {
            _messageRoutes.Add(messageRoute);
        }
        else
        {
            foreach (var specification in messageRoute.Specifications)
            {
                existing.AddSpecification(specification);
            }
        }
    }

    public IEnumerable<IMessageRoute> MessageRoutes => new List<IMessageRoute>(_messageRoutes).AsReadOnly();
}