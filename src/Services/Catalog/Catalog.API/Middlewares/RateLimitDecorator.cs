﻿using System;

namespace Catalog.API.Middlewares
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RateLimitDecorator : Attribute
    {
        public StrategyTypeEnum StrategyType { get; set; }
    }

    public enum StrategyTypeEnum
    {
        IpAddress,
        PerUser,
        PerApiKey
    }
}
