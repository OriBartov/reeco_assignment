using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrderOps.Api.Json;

namespace OrderOps.Api.ModelBinding;

public sealed class SnakeCaseQueryValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        var query = context.ActionContext.HttpContext.Request.Query;
        context.ValueProviders.Insert(0, new SnakeCaseQueryValueProvider(query));
        return Task.CompletedTask;
    }
}

public sealed class SnakeCaseQueryValueProvider : QueryStringValueProvider
{
    public SnakeCaseQueryValueProvider(IQueryCollection values)
        : base(BindingSource.Query, values, CultureInfo.InvariantCulture)
    {
    }

    public override ValueProviderResult GetValue(string key)
    {
        var snakeKey = SnakeCaseNamingPolicy.Instance.ConvertName(key);
        if (snakeKey != key)
        {
            var snakeResult = base.GetValue(snakeKey);
            if (snakeResult != ValueProviderResult.None)
            {
                return snakeResult;
            }
        }

        return base.GetValue(key);
    }
}
