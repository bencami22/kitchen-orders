using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KitchenOrders.Extensions;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ValidateOnStartupTime<TOptions>(this OptionsBuilder<TOptions> builder)
        where TOptions : class
    {
        builder.Services.AddTransient<IStartupFilter, OptionsValidateFilter<TOptions>>();
        return builder;
    }

    public class OptionsValidateFilter<TOptions> : IStartupFilter where TOptions : class
    {
        private readonly IOptions<TOptions> _options;

        public OptionsValidateFilter(IOptions<TOptions> options)
        {
            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _ = _options.Value; // Trigger for validating options.
            return next;
        }
    }
}