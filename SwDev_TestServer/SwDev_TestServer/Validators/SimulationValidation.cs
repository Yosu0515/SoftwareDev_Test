using FluentValidation;
using SwDev_TestServer.Models;

namespace SwDev_TestServer.Validators
{
    public class SimulationValidation : AbstractValidator<Simulation>
    {
        public SimulationValidation()
        {
            RuleFor(p => p.A1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.A2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.A3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.A4)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.AB1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.AB2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.B1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.B2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.B3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.B4)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.B5)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.BB1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.C1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.C2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.C3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.D1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.D2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.D3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.E1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.E2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.EV1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.EV2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.EV3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.EV4)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.FF1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.FF2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.FV1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.FV2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.FV3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.FV4)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.GF1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.GF2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.GV1)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.GV2)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.GV3)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
            RuleFor(p => p.GV4)
                .NotEmpty()
                .Matches("^[0-999]+$")
                .MinimumLength(1)
                .MaximumLength(3);
        }
    }
}