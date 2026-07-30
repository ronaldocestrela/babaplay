using System;
using System.Collections.Generic;

namespace BabaPlay.Web.Models;

public record PlayerTacticalPositionDto(
    Guid PlayerId,
    string Name,
    string PositionName,
    int? JerseyNumber,
    double XPercent, // 0 to 100
    double YPercent, // 0 to 100
    string ColorHex,
    bool IsSelected = false);

public record TacticalFormationDto(
    string FormationName,
    string Mode, // "Futsal", "Society", "Campo"
    List<PlayerTacticalPositionDto> Positions);

public record PitchFormationPresetDto(
    string Id,
    string Name,
    string Mode,
    List<(double X, double Y, string PositionName)> DefaultCoordinates);
