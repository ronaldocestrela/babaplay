using BabaPlayShared.Library.Models.Requests.Cors;
using FluentValidation;
using System;

namespace Application.Features.Cors.Validations;

public class CreateCorsOriginRequestValidator : AbstractValidator<CreateCorsOriginRequest>
{
    public CreateCorsOriginRequestValidator()
    {
        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var u) &&
                          (u.Scheme == "http" || u.Scheme == "https"))
            .WithMessage("Invalid origin URL. Must be a valid http or https URI.");
    }
}