using System.Globalization;
using Ahorita.Domain.Routines;
using Ahorita.Infrastructure.Templates;

namespace Ahorita.Infrastructure.Tests.Templates;

public sealed class EmbeddedRoutineTemplateProviderTests
{
    private readonly EmbeddedRoutineTemplateProvider _provider = new();

    public static TheoryData<string> Languages => new(EmbeddedRoutineTemplateProvider.SupportedLanguages);

    [Theory]
    [MemberData(nameof(Languages))]
    public void Every_template_produces_a_valid_startable_routine(string language)
    {
        var templates = _provider.GetAll(CultureInfo.GetCultureInfo(language));

        templates.ShouldNotBeEmpty();
        foreach (var template in templates)
        {
            var schedule = RoutineSchedule.Create(template.Days, template.StartTime);
            schedule.IsSuccess.ShouldBeTrue($"Horario inválido en '{template.Key}' ({language}).");

            var routine = Routine.Create(template.Name, template.Emoji, template.Accent, schedule.Value, DateTimeOffset.UnixEpoch).Value;
            foreach (var step in template.Steps)
                routine.AddStep(step.Title, step.Emoji, step.Duration, step.IsOptional).IsSuccess
                    .ShouldBeTrue($"Paso inválido '{step.Title}' en '{template.Key}' ({language}).");

            routine.CanStart.ShouldBeTrue();
        }
    }

    [Fact]
    public void All_languages_offer_the_same_template_keys()
    {
        var keysByLanguage = EmbeddedRoutineTemplateProvider.SupportedLanguages
            .Select(l => _provider.GetAll(CultureInfo.GetCultureInfo(l)).Select(t => t.Key).ToArray())
            .ToArray();

        foreach (var keys in keysByLanguage)
            keys.ShouldBe(keysByLanguage[0]);
    }

    [Fact]
    public void Parses_days_and_start_time()
    {
        var morning = _provider.GetAll(CultureInfo.GetCultureInfo("es")).Single(t => t.Key == "morning");

        morning.Name.ShouldBe("Rutina de mañana");
        morning.Days.ShouldBe(WeekDays.Weekdays);
        morning.StartTime.ShouldBe(new TimeOnly(7, 0));
        morning.Steps.ShouldContain(s => s.IsOptional);
    }

    [Fact]
    public void Unsupported_languages_fall_back_to_spanish()
    {
        var german = _provider.GetAll(CultureInfo.GetCultureInfo("de"));
        var spanish = _provider.GetAll(CultureInfo.GetCultureInfo("es"));

        german.ShouldBe(spanish);
    }
}
